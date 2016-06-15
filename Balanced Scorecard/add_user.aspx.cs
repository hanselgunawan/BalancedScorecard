using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class add_user : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        string str_connect2 = ConfigurationManager.ConnectionStrings["HumanCapitalConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (!IsPostBack)
            {
                if (Session["user_name"] == null)
                {
                    Response.Redirect("" + baseUrl + "index.aspx");
                }
                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();
                var page = Request.QueryString["page"];
                var period_id = Request.QueryString["period_id"];
                var filter = Request.QueryString["filter"];


                DateTime start_date, end_date;
                string start_end_date;
                string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                string string_get_bsc_user_group = "SELECT * FROM BSC_UserGroup WHERE UserGroup_ID>1";//hilangkan WHERE nya jika ADMIN mau tambah ADMIN lain
                SqlConnection conn = new SqlConnection(str_connect);
                SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                SqlCommand sql_get_bsc_user_group = new SqlCommand(string_get_bsc_user_group, conn);
                conn.Open();
                //source code untuk hak akses
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);

                using (SqlDataReader AccessReader = sql_access_rights.ExecuteReader())
                {
                    if (AccessReader.HasRows)
                    {
                        while (AccessReader.Read())
                        {
                            generateHtmlAnchor(AccessReader["Access_Rights_Code"].ToString());
                        }
                    }
                    AccessReader.Close();
                    AccessReader.Dispose();
                }

                using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                {
                    if (PeriodReader.HasRows)
                    {
                        while (PeriodReader.Read())
                        {
                            string start_date_formatted, end_date_formatted;
                            start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                            end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                            start_date_formatted = start_date.ToString("MMM");
                            end_date_formatted = end_date.ToString("MMM yyyy");
                            start_end_date = start_date_formatted + " - " + end_date_formatted;
                            ScorecardPeriod.InnerText = start_end_date;
                            SpanAddUser.Attributes.Remove("disabled");
                        }
                    }
                    else
                    {
                        ScorecardPeriod.InnerText = "Period Not Found";
                        SpanAddUser.Attributes.Add("disabled", "true");
                    }
                    PeriodReader.Dispose();
                    PeriodReader.Close();
                }

                using (SqlDataReader GroupReader = sql_get_bsc_user_group.ExecuteReader())
                {
                    while(GroupReader.Read())
                    {
                        DropDownListRole.Items.Add(GroupReader["Group_Name"].ToString());
                    }
                    GroupReader.Dispose();
                    GroupReader.Close();
                }

                conn.Close();

                DropDownListStatus.Items.Add("Yes");
                DropDownListStatus.Items.Add("No");

                //disable Name, Organization, Job Title, Additional Group, Scorecard Group
                TextBoxName.Attributes.Add("disabled", "true");
                TextBoxOrganization.Attributes.Add("disabled", "true");
                TextBoxJobTitle.Attributes.Add("disabled", "true");
                TextBoxAdditional.Attributes.Add("disabled", "true");
                TextBoxGroup.Attributes.Add("disabled", "true");
                TextBoxEmail.Attributes.Add("disabled", "true");
                TextBoxGrade.Attributes.Add("disabled", "true");
                TextBoxJobLevelName.Attributes.Add("disabled", "true");

                check_NIK.Attributes.Add("style", "visibility:hidden; margin-bottom:0px !important; margin-top:0px !important");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-40px !important; margin-top:5px !important");

                if (filter == null) cancel_add.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "");//kirim balik page nya!
                else cancel_add.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&filter=" + filter + "");

                if (filter == null) scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "");//kirim balik page nya!
                else scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&filter=" + filter + "");

                TextBoxColor.Attributes.Add("class", "form-group");
                TextBoxColorConfirmation.Attributes.Add("class", "form-group");
            }
        }

        protected void TextBoxConfirmation_TextChanged(object sender, EventArgs e)
        {
            if (TextBoxConfirmation.Text != TextBoxPassword.Text)//jika Confirmation Password BUKAN SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Add("disabled", "true");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:visible; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
                TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-error");
            }
            else//jika Confirmation Password SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Remove("disabled");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-40px !important; margin-top:5px !important");
                TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-success");
            }
        }

        protected void TextBoxNIK_TextChanged(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
            check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-40px !important; margin-top:5px !important");

            TextBoxColorConfirmation.Attributes.Add("class", "form-group");

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                string string_select_scorecard_user = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, OrgName, "
                                                    + "JobTtlName, empPassword, empStatus, ScorecardUser.empEmail, "
                                                    + "OrgAdtGroupName, Group_Name, empGrade, JobLvlName, Superior_ID, UserGroup_ID FROM ScorecardUser "
                                                    + "JOIN ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode=ScorecardGroupLink.OrgAdtGroupCode "
                                                    + "JOIN BSC_Period ON BSC_Period.Period_ID=ScorecardGroupLink.Period_ID "
                                                    + "join [Human_Capital_demo].dbo.Organization ON ScorecardUser.empOrgCode = Organization.OrgCode "
                                                    + "join [Human_Capital_demo].dbo.JobTitle ON ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                    + "join [Human_Capital_demo].dbo.Employee ON Employee.EmpId = ScorecardUser.EmpId "
                                                    + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                                    + "WHERE ScorecardUser.EmpId='" + TextBoxNIK.Text + "' AND ScorecardGroupLink.Period_ID=" + period_id + "";
                SqlCommand sql_select_scorecard_user = new SqlCommand(string_select_scorecard_user, conn);
                using (SqlDataReader ScorecardUserReader = sql_select_scorecard_user.ExecuteReader())
                {
                    if (ScorecardUserReader.HasRows)//jika NIK sudah ada di Database Scorecard, tidak bisa Add User lagi
                    {
                        while (ScorecardUserReader.Read())
                        {
                            SpanAddUser.Attributes.Add("disabled", "true");
                            TextBoxPassword.Attributes.Add("disabled", "true");
                            TextBoxConfirmation.Enabled = false;
                            TextBoxColor.Attributes.Add("class", "form-group has-error");
                            check_NIK.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            check_NIK.InnerText = "NIK already registered";
                            TextBoxName.Value = ScorecardUserReader["empName"].ToString();
                            TextBoxOrganization.Value = ScorecardUserReader["OrgName"].ToString();
                            TextBoxJobTitle.Value = ScorecardUserReader["JobTtlName"].ToString();
                            TextBoxAdditional.Value = ScorecardUserReader["OrgAdtGroupName"].ToString();
                            TextBoxGroup.Value = ScorecardUserReader["Group_Name"].ToString();
                            DropDownListStatus.SelectedValue = ScorecardUserReader["empStatus"].ToString();
                            TextBoxGrade.Value = ScorecardUserReader["empGrade"].ToString();
                            TextBoxJobLevelName.Value = ScorecardUserReader["JobLvlName"].ToString();
                            TextBoxSuperior.Text = ScorecardUserReader["Superior_ID"].ToString();
                            DropDownListRole.SelectedIndex = int.Parse(ScorecardUserReader["UserGroup_ID"].ToString()) - 2;

                            if (!DBNull.Value.Equals(ScorecardUserReader["empEmail"]))
                            {
                                if (ScorecardUserReader["empEmail"].ToString() == "")
                                {
                                    TextBoxEmail.Value = "-";
                                }
                                else
                                {
                                    TextBoxEmail.Value = ScorecardUserReader["empEmail"].ToString();
                                }
                            }
                            else
                            {
                                TextBoxEmail.Value = "-";
                            }

                            if (!DBNull.Value.Equals(ScorecardUserReader["Superior_ID"]))
                            {
                                if (ScorecardUserReader["Superior_ID"].ToString() == "")
                                {
                                    TextBoxSuperior.Text = "-";
                                }
                                else
                                {
                                    TextBoxSuperior.Text = ScorecardUserReader["Superior_ID"].ToString();
                                }
                            }
                            else
                            {
                                TextBoxSuperior.Text = "-";
                            }

                            TextBoxSuperior.Enabled = false;
                            DropDownListStatus.Enabled = false;
                            DropDownListRole.Enabled = false;
                        }
                    }
                    else//jika NIK tidak ada di Database Scorecard, cek di database HRIS. Jika ditemukkan, bisa Add. Jika tidak, Add di disabled
                    {
                        using (SqlConnection conn2 = new SqlConnection(str_connect2))
                        {
                            conn2.Open();
                            string string_select_from_HRIS = "select empname,	orgname, JobTtlName, empemail, OrgAdtName, OrgAdtGroup, Group_Name, EmpJobLvl, JobLvlName from [Human_Capital_demo].dbo.Employee (nolock) " +
                                                        "JOIN [Human_Capital_demo].dbo.Organization (nolock) ON employee.emporg = Organization.orgcode " +
                                                        "JOIN [Human_Capital_demo].dbo.JobTitle (nolock) ON employee.EmpJobTtl = jobtitle.JobTtlCode " +
                                                        "JOIN [Balanced Scorecard].dbo.ScorecardGroupLink (nolock) ON Organization.OrgAdtGroup = ScorecardGroupLink.OrgAdtGroupCode " +
                                                        "JOIN [Human_Capital_demo].dbo.OrgAdtGroup (nolock) ON OrgAdtGroup.OrgAdtCode = ScorecardGroupLink.OrgAdtGroupCode " +
                                                        "JOIN [Balanced Scorecard].dbo.BSC_Period ON ScorecardGroupLink.Period_ID = BSC_Period.Period_ID AND BSC_Period.Period_ID=" + period_id + " " +//mengambil Period yang Aktif, karena tiap periode bisa saja Group Name-nya berubah
                                                        "JOIN [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = Employee.EmpJobLvl " +
                                                        "WHERE employee.empdateend is null and employee.EmpId = '" + TextBoxNIK.Text + "'";
                            SqlCommand sql_select_from_HRIS = new SqlCommand(string_select_from_HRIS, conn2);
                            using (SqlDataReader HRISReader = sql_select_from_HRIS.ExecuteReader())
                            {
                                if (HRISReader.HasRows)
                                {
                                    while (HRISReader.Read())
                                    {
                                        SpanAddUser.Attributes.Remove("disabled");
                                        TextBoxPassword.Attributes.Remove("disabled");
                                        TextBoxConfirmation.Enabled = true;
                                        TextBoxColor.Attributes.Add("class", "form-group has-success");
                                        check_NIK.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px !important; margin-top:0px !important");
                                        TextBoxName.Value = HRISReader["empname"].ToString();
                                        TextBoxOrganization.Value = HRISReader["orgname"].ToString();
                                        TextBoxJobTitle.Value = HRISReader["JobTtlName"].ToString();
                                        TextBoxAdditional.Value = HRISReader["OrgAdtName"].ToString();
                                        TextBoxGroup.Value = HRISReader["Group_Name"].ToString();
                                        TextBoxGrade.Value = HRISReader["EmpJobLvl"].ToString();
                                        TextBoxJobLevelName.Value = HRISReader["JobLvlName"].ToString();
                                        if (!DBNull.Value.Equals(HRISReader["empemail"])) TextBoxEmail.Value = HRISReader["empemail"].ToString();
                                        else TextBoxEmail.Value = "-";
                                        TextBoxSuperior.Enabled = true;
                                        TextBoxSuperior.Text = "";
                                        DropDownListRole.Enabled = true;
                                        DropDownListStatus.Enabled = true;
                                    }
                                }
                                else
                                {
                                    SpanAddUser.Attributes.Add("disabled", "true");
                                    TextBoxPassword.Attributes.Add("disabled", "true");
                                    TextBoxConfirmation.Enabled = false;
                                    TextBoxColor.Attributes.Add("class", "form-group has-error");
                                    check_NIK.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                                    check_NIK.InnerText = "Your NIK Not Found";
                                    TextBoxName.Value = "Name Not Found";
                                    TextBoxOrganization.Value = "Organization Not Found";
                                    TextBoxJobTitle.Value = "Job Title Not Found";
                                    TextBoxAdditional.Value = "Additional Group Not Found";
                                    TextBoxGroup.Value = "Scorecard Group Not Found";
                                    TextBoxEmail.Value = "Email Not Found";
                                    TextBoxGrade.Value = "Grade Not Found";
                                    TextBoxJobLevelName.Value = "Job Level Name Not Found";
                                    TextBoxSuperior.Enabled = false;
                                    TextBoxSuperior.Text = "";
                                    DropDownListRole.Enabled = false;
                                    DropDownListStatus.Enabled = false;
                                }
                            }
                            conn2.Close();
                        }//end of Human Capital Connection
                    }
                }
                conn.Close();
            }//end of Balanced Scorecard Connection
        }

        protected void OnClickAddUser(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var filter = Request.QueryString["filter"];
            string user_create, user_update, date_create, date_update;
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                string MD5_Password;
                string user_password = TextBoxPassword.Text;
                string email_value;
                bool null_email = true;
                string string_insert_user = "";
                email_value = TextBoxEmail.Value;
                if (email_value == "-")
                {
                    string_insert_user = "INSERT INTO ScorecardUser(EmpId, empName, empOrgCode, empJobTitleCode, "
                                        +"empPassword, empStatus, user_create, date_create, user_update, date_update, "
                                        +"empOrgAdtGroupCode, UserGroup_ID, empGrade, Superior_ID) "
                                        +"VALUES(@EmpId, @empName, @empOrgCode, @empJobTitleCode, @empPassword, @empStatus, "
                                        +"@user_create, @date_create, @user_update, @date_update, @empOrgAdtGroupCode, "
                                        +"@usergroup_id, @empGrade, @Superior_ID)";
                }
                else
                {
                    string_insert_user = "INSERT INTO ScorecardUser VALUES(@EmpId, @empName, @empOrgCode, @empJobTitleCode, "
                                        + "@empPassword, @empStatus, @user_create, @date_create, @user_update, @date_update, "
                                        + "@empOrgAdtGroupCode, @empEmail, @usergroup_id, @empGrade, @Superior_ID)";
                    null_email = false;
                }
                
                SqlCommand sql_insert_user = new SqlCommand(string_insert_user, conn);

                user_create = Session["user_name"].ToString();
                user_update = Session["user_name"].ToString();
                date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (TextBoxConfirmation.Text != TextBoxPassword.Text)//jika Confirmation Password BUKAN SAMA DENGAN Password
                {
                    SpanAddUser.Attributes.Remove("disabled");
                    check_password_confirmation.Attributes.Add("style", "width:400px; visibility:visible; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
                    TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                    TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                    TextBoxColorConfirmation.Attributes.Add("class", "form-group has-error");
                }
                else
                {
                    string select_emp_info = "select empname,	OrgCode, JobTtlCode, empemail, "
                                           + "OrgAdtGroupCode from [Human_Capital_demo].dbo.Employee (nolock) "
                                           + "JOIN [Human_Capital_demo].dbo.Organization (nolock) ON employee.emporg = Organization.orgcode "
                                           + "JOIN [Human_Capital_demo].dbo.JobTitle (nolock) ON employee.EmpJobTtl = jobtitle.JobTtlCode "
                                           + "JOIN [Balanced Scorecard].dbo.ScorecardGroupLink (nolock) ON Organization.OrgAdtGroup = ScorecardGroupLink.OrgAdtGroupCode "
                                           + "JOIN [Human_Capital_demo].dbo.OrgAdtGroup (nolock) ON OrgAdtGroup.OrgAdtCode = ScorecardGroupLink.OrgAdtGroupCode "
                                           + "JOIN [Balanced Scorecard].dbo.BSC_Period ON ScorecardGroupLink.Period_ID = BSC_Period.Period_ID AND BSC_Period.Period_ID=" + period_id + " "
                                           + "WHERE employee.empdateend is null and employee.EmpId = '" + TextBoxNIK.Text + "'";
                    SqlConnection hc_connect = new SqlConnection(str_connect2);
                    SqlCommand sql_select_emp_info = new SqlCommand(select_emp_info, hc_connect);

                    hc_connect.Open();
                    check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-40px !important; margin-top:5px !important");
                    TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                    TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                    TextBoxColorConfirmation.Attributes.Add("class", "form-group has-success");

                    using (SqlDataReader UserReader = sql_select_emp_info.ExecuteReader())
                    {
                        while (UserReader.Read())
                        {
                            sql_insert_user.Parameters.AddWithValue("@EmpId", TextBoxNIK.Text);
                            sql_insert_user.Parameters.AddWithValue("@empName", TextBoxName.Value);
                            sql_insert_user.Parameters.AddWithValue("@empOrgCode", UserReader["OrgCode"].ToString());
                            sql_insert_user.Parameters.AddWithValue("@empJobTitleCode", UserReader["JobTtlCode"].ToString());
                            sql_insert_user.Parameters.AddWithValue("@empStatus", DropDownListStatus.SelectedValue);
                            sql_insert_user.Parameters.AddWithValue("@user_create", user_create);
                            sql_insert_user.Parameters.AddWithValue("@date_create", date_create);
                            sql_insert_user.Parameters.AddWithValue("@user_update", user_update);
                            sql_insert_user.Parameters.AddWithValue("@date_update", date_update);
                            sql_insert_user.Parameters.AddWithValue("@empOrgAdtGroupCode", UserReader["OrgAdtGroupCode"].ToString());//yang disimpan ke DB adalah OrgAdtCode, BUKAN OrgAdtName
                            sql_insert_user.Parameters.AddWithValue("@usergroup_id", DropDownListRole.SelectedIndex + 2);
                            sql_insert_user.Parameters.AddWithValue("@empGrade", TextBoxGrade.Value);
                            sql_insert_user.Parameters.AddWithValue("@Superior_ID", TextBoxSuperior.Text);
                        }
                        UserReader.Dispose();
                        UserReader.Close();
                    }
                    hc_connect.Close();

                    if (null_email == false)
                    {
                        sql_insert_user.Parameters.AddWithValue("@empEmail", TextBoxEmail.Value);
                    }

                    //ubah Password dengan MD5
                    using (MD5 md5Hash = MD5.Create())
                    {
                        string hash = GetMd5Hash(md5Hash, user_password);
                        MD5_Password = hash;
                        sql_insert_user.Parameters.AddWithValue("@empPassword", MD5_Password);
                    }

                    if (TextBoxEmail.Value != "-")//jika E-Mail user tersebut ditemukkan
                    {
                        sendMail(TextBoxName.Value, TextBoxNIK.Text, TextBoxPassword.Text, TextBoxEmail.Value);
                    }

                    sql_insert_user.ExecuteNonQuery();

                    
                    if (filter == null)
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Scorecard User Added'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "';", true);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Scorecard User Added'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&filter=" + filter + "';", true);
                    }
                }
                conn.Close();
            }
        }

        protected void OnSuperiorChanged(object sender, EventArgs e)
        {
            if (TextBoxSuperior.Text == TextBoxNIK.Text)
            {
                LabelSuperiorName.Visible = true;
                LabelSuperiorName.Text = "Cannot use same NIK";
                LabelSuperiorName.Attributes.Add("style", "color:red; font-weight:bold");
                SpanAddUser.Attributes.Add("disabled", "true");
            }
            else if (TextBoxSuperior.Text == "")
            {
                LabelSuperiorName.Visible = false;
                SpanAddUser.Attributes.Remove("disabled");
            }
            else
            {
                string string_check_superior_name = "SELECT empName FROM ScorecardUser WHERE EmpId='" + TextBoxSuperior.Text + "'";
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_check_superior_name = new SqlCommand(string_check_superior_name, conn);
                    using (SqlDataReader SuperiorReader = sql_check_superior_name.ExecuteReader())
                    {
                        if (SuperiorReader.HasRows)
                        {
                            while (SuperiorReader.Read())
                            {
                                LabelSuperiorName.Visible = true;
                                LabelSuperiorName.Text = SuperiorReader["empName"].ToString();
                                LabelSuperiorName.Attributes.Add("style", "color:black");
                                SpanAddUser.Attributes.Remove("disabled");
                            }
                        }
                        else
                        {
                            LabelSuperiorName.Visible = true;
                            LabelSuperiorName.Text = "NIK Not Found";
                            LabelSuperiorName.Attributes.Add("style", "color:red; font-weight:bold");
                            SpanAddUser.Attributes.Add("disabled", "true");
                        }
                        SuperiorReader.Dispose();
                        SuperiorReader.Close();
                    }
                    conn.Close();
                }
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert string menjadi byte array agar bisa di hash
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Buat StringBuilder buat tampung byte-nya
            // dan membuat string baru.
            StringBuilder sBuilder = new StringBuilder();

            // Loop setiap byte array
            // dan format semua ke nilai hexadecimal
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Mengembalikan nilai Hexadecimal
            return sBuilder.ToString();
        }

        public void sendMail(string employee_name, string NIK, string employee_password, string employee_email)
        {
            SqlConnection conn = new SqlConnection(str_connect2);
            string string_get_marital_and_sex = "SELECT EmpMaritalSt, EmpSex FROM [Human_Capital_demo].dbo.Employee WHERE EmpId = '" + NIK + "'";
            string strApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];
            string user_title = "";
            conn.Open();
            SqlCommand sql_get_marital_and_sex = new SqlCommand(string_get_marital_and_sex, conn);
            using (SqlDataReader MaritalReader = sql_get_marital_and_sex.ExecuteReader())
            {
                if (MaritalReader.HasRows)
                {
                    while (MaritalReader.Read())
                    {
                        if (MaritalReader["EmpSex"].ToString() == "F" && MaritalReader["EmpMaritalSt"].ToString() == "NIKAH")
                        {
                            user_title = "Ms.";
                        }
                        else if (MaritalReader["EmpSex"].ToString() == "F" && MaritalReader["EmpMaritalSt"].ToString() == "BELUM NIKAH")
                        {
                            user_title = "Mrs.";
                        }
                        else if (MaritalReader["EmpSex"].ToString() == "F" && DBNull.Value.Equals(MaritalReader["EmpMaritalSt"]))
                        {
                            user_title = "Ms.";
                        }
                        else if (MaritalReader["EmpSex"].ToString() == "M")
                        {
                            user_title = "Mr.";
                        }
                    }
                }
                MaritalReader.Close();
                MaritalReader.Dispose();
            }
            conn.Close();

            SmtpClient mailclient = new SmtpClient();  //Karena FILE_LOCATION terjadi perubahan setiap di-klik, maka
            using (MailMessage msg = new MailMessage())//harus pake USING untuk CLEAR semua Resource yang pernah dipake
            {
                /******************** SEND Email TO Users **************************************/
                msg.Subject = "MPPA Balanced Scorecard";
                msg.Body = "Dear, "+user_title+" " + employee_name + "<br/>"
                            + "You have been registered in MPPA Balanced Scorecard Web Application<br/><br/>"
                            + "Your NIK: " + NIK + "<br/>"
                            + "Your Password: " + employee_password + "<br/><br/>"
                            + "Try to Login to your account at: " + strApplicationURL + "<br/><br/>"
                            + "Thank You<br/><br/>"
                            + "Best Regards,<br/>"
                            + "Human Capital Department"
                            + "<br/><br/>This is an automatically generated email – please do not reply to it.";
                msg.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["emailSender"]);
                msg.To.Add(employee_email);
                msg.IsBodyHtml = true;
                mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                mailclient.Send(msg);
            }
        }

        public void generateHtmlAnchor(string anchor)
        {
            switch (anchor)
            {
                case "dashboard":
                    HtmlAnchor dashboard = (HtmlAnchor)(this.Master).FindControl("dashboard");
                    dashboard.Attributes.Add("href", "#");
                    dashboard.Attributes.Add("style", "color:black");
                    break;
                case "setting":
                    HtmlAnchor setting = (HtmlAnchor)(this.Master).FindControl("setting");
                    setting.Attributes.Add("data-target", "#");
                    setting.Attributes.Add("style", "color:black");
                    break;
                case "set_period":
                    HtmlAnchor set_period = (HtmlAnchor)(this.Master).FindControl("set_period");
                    set_period.Attributes.Add("href", "#");
                    set_period.Attributes.Add("style", "color:black !important");
                    break;
                case "set_review_month":
                    HtmlAnchor set_review_month = (HtmlAnchor)(this.Master).FindControl("set_review_month");
                    set_review_month.Attributes.Add("href", "#");
                    set_review_month.Attributes.Add("style", "color:black !important");
                    break;
                case "link_financial_group":
                    HtmlAnchor link_financial_group = (HtmlAnchor)(this.Master).FindControl("link_financial_group");
                    link_financial_group.Attributes.Add("href", "#");
                    link_financial_group.Attributes.Add("style", "color:black !important");
                    break;
                case "scorecard":
                    HtmlAnchor scorecard = (HtmlAnchor)(this.Master).FindControl("scorecard");
                    scorecard.Attributes.Add("data-target", "#");
                    scorecard.Attributes.Add("style", "color:black");
                    break;
                case "financial_scorecard":
                    HtmlAnchor financial_scorecard = (HtmlAnchor)(this.Master).FindControl("financial_scorecard");
                    financial_scorecard.Attributes.Add("href", "#");
                    financial_scorecard.Attributes.Add("style", "color:black !important");
                    break;
                case "individual_scorecard":
                    HtmlAnchor individual_scorecard = (HtmlAnchor)(this.Master).FindControl("individual_scorecard");
                    individual_scorecard.Attributes.Add("href", "#");
                    individual_scorecard.Attributes.Add("style", "color:black !important");
                    break;
                case "my_requests":
                    HtmlAnchor my_requests = (HtmlAnchor)(this.Master).FindControl("my_requests");
                    my_requests.Attributes.Add("data-target", "#");
                    my_requests.Attributes.Add("style", "color:black");
                    break;
                case "request_kpi":
                    HtmlAnchor request_kpi = (HtmlAnchor)(this.Master).FindControl("request_kpi");
                    request_kpi.Attributes.Add("href", "#");
                    request_kpi.Attributes.Add("style", "color:black !important");
                    break;
                case "request_so":
                    HtmlAnchor request_so = (HtmlAnchor)(this.Master).FindControl("request_so");
                    request_so.Attributes.Add("href", "#");
                    request_so.Attributes.Add("style", "color:black !important");
                    break;
                case "approve_requests":
                    HtmlAnchor approve_requests = (HtmlAnchor)(this.Master).FindControl("approve_requests");
                    approve_requests.Attributes.Add("data-target", "#");
                    approve_requests.Attributes.Add("style", "color:black");
                    break;
                case "approval":
                    HtmlAnchor approval = (HtmlAnchor)(this.Master).FindControl("approval");
                    approval.Attributes.Add("href", "#");
                    approval.Attributes.Add("style", "color:black !important");
                    break;
                case "approval_specific_objective":
                    HtmlAnchor approval_specific_objective = (HtmlAnchor)(this.Master).FindControl("approval_specific_objective");
                    approval_specific_objective.Attributes.Add("href", "#");
                    approval_specific_objective.Attributes.Add("style", "color:black !important");
                    break;
                case "request_history":
                    HtmlAnchor request_history = (HtmlAnchor)(this.Master).FindControl("request_history");
                    request_history.Attributes.Add("data-target", "#");
                    request_history.Attributes.Add("style", "color:black");
                    break;
                case "request_change_kpi_history":
                    HtmlAnchor request_change_kpi_history = (HtmlAnchor)(this.Master).FindControl("request_change_kpi_history");
                    request_change_kpi_history.Attributes.Add("href", "#");
                    request_change_kpi_history.Attributes.Add("style", "color:black !important");
                    break;
                case "request_change_specific_objective_history":
                    HtmlAnchor request_change_specific_objective_history = (HtmlAnchor)(this.Master).FindControl("request_change_specific_objective_history");
                    request_change_specific_objective_history.Attributes.Add("href", "#");
                    request_change_specific_objective_history.Attributes.Add("style", "color:black !important");
                    break;
                case "user_management":
                    HtmlAnchor user_management = (HtmlAnchor)(this.Master).FindControl("user_management");
                    user_management.Attributes.Add("data-target", "#");
                    user_management.Attributes.Add("style", "color:black");
                    break;
                case "scorecard_user":
                    HtmlAnchor scorecard_user = (HtmlAnchor)(this.Master).FindControl("scorecard_user");
                    scorecard_user.Attributes.Add("href", "#");
                    scorecard_user.Attributes.Add("style", "color:black !important");
                    break;
                case "user_access_rights":
                    HtmlAnchor user_access_rights = (HtmlAnchor)(this.Master).FindControl("user_access_rights");
                    user_access_rights.Attributes.Add("href", "#");
                    user_access_rights.Attributes.Add("style", "color:black !important");
                    break;
            }
        }

    }
}