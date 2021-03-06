﻿using System;
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
    public partial class edit_user : System.Web.UI.Page
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
                var user_id = Request.QueryString["user_id"];

                var nik = Request.QueryString["nik"];
                var org = Request.QueryString["organization"];
                var adt_org = Request.QueryString["adt_organization"];
                var bsc_group = Request.QueryString["bsc_group"];
                var name = Request.QueryString["name"];
                var active = Request.QueryString["active"];
                var superior = Request.QueryString["superior"];
                var role = Request.QueryString["role"];

                string start_date_formatted = "", end_date_formatted = "", start_end_date = "";
                string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                string string_get_bsc_user_group = "SELECT * FROM BSC_UserGroup WHERE UserGroup_ID>1";//hilangkan WHERE jika ADMIN mau tambah ADMIN lain
                string string_select_user = "SELECT EmpId, empName, OrgName, JobTtlName, empEmail, empPassword, empStatus, OrgAdtGroupName, "
                                           + "Group_Name, empGrade, JobLvlName, UserGroup_ID, Superior_ID FROM ScorecardUser "
                                           + "JOIN ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode=ScorecardGroupLink.OrgAdtGroupCode "
                                           + "JOIN BSC_Period ON BSC_Period.Period_ID=ScorecardGroupLink.Period_ID "
                                           + "join [Human_Capital_demo].dbo.JobTitle ON ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                           + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                           + "join [Human_Capital_demo].dbo.Organization ON ScorecardUser.empOrgCode = Organization.OrgCode "
                                           + "WHERE ScorecardUser.user_id=" + user_id + " AND ScorecardGroupLink.Period_ID =" + period_id + " ";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan UserGroup
                                                    + "WHERE Access_Rights_Code NOT IN "
                                                    + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                    + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                DropDownListStatus.Items.Add("Yes");
                DropDownListStatus.Items.Add("No");

                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");

                if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "");
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id="+period_id+"");
                }
                else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&nik=" + nik + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&nik=" + nik + "");//kirim balik page nya!
                }
                else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&org=" + org + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&org=" + org + "");//kirim balik page nya!
                }
                else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&adt_org=" + adt_org + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&adt_org=" + adt_org + "");//kirim balik page nya!
                }
                else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null && active == null && superior == null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&bsc_group=" + bsc_group + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&bsc_group=" + bsc_group + "");//kirim balik page nya!
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null && active == null && superior == null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&name=" + name + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&name=" + name + "");//kirim balik page nya!
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active != null && superior == null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&active=" + active + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&active=" + active + "");//kirim balik page nya!
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior != null && role == null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&superior=" + superior + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&superior=" + superior + "");//kirim balik page nya!
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role != null)
                {
                    cancel_edit.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&role=" + role + "");//kirim balik page nya!
                    scorecard_user_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&role=" + role + "");//kirim balik page nya!
                }

                TextBoxColor.Attributes.Add("class", "form-group");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_select_user = new SqlCommand(string_select_user, conn);
                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                    SqlCommand sql_get_bsc_user_group = new SqlCommand(string_get_bsc_user_group, conn);
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
                                DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                start_date_formatted = start_date.ToString("MMM");
                                end_date_formatted = end_date.ToString("MMM yyyy");
                                start_end_date = start_date_formatted + " - " + end_date_formatted;
                                ScorecardPeriod.InnerText = start_end_date;
                                SpanAddUser.Attributes.Remove("disabled");
                            }
                        }
                        else
                        {
                            ScorecardPeriod.InnerText = "No Period Found";
                            SpanAddUser.Attributes.Add("disabled", "true");
                        }
                        PeriodReader.Dispose();
                        PeriodReader.Close();
                    }

                    using (SqlDataReader GroupReader = sql_get_bsc_user_group.ExecuteReader())
                    {
                        while (GroupReader.Read())
                        {
                            DropDownListRole.Items.Add(GroupReader["Group_Name"].ToString());
                        }
                        GroupReader.Dispose();
                        GroupReader.Close();
                    }

                    using (SqlDataReader UserReader = sql_select_user.ExecuteReader())
                    {
                        if (UserReader.HasRows)
                        {
                            while (UserReader.Read())
                            {
                                LabelUserNIK_Breadcrumb.Text = UserReader["EmpId"].ToString();
                                LabelUserName_Breadcrumb.Text = UserReader["empName"].ToString();
                                LabelUserNIK_Title.Text = UserReader["EmpId"].ToString();
                                LabelUserName_Title.Text = UserReader["empName"].ToString();

                                TextBoxNIK.Text = UserReader["EmpId"].ToString();
                                TextBoxName.Value = UserReader["empName"].ToString();
                                TextBoxOrganization.Value = UserReader["OrgName"].ToString();
                                TextBoxJobTitle.Value = UserReader["JobTtlName"].ToString();
                                TextBoxGrade.Value = UserReader["empGrade"].ToString();
                                TextBoxJobLevelName.Value = UserReader["JobLvlName"].ToString();
                                    if (!DBNull.Value.Equals(UserReader["empEmail"]))
                                    {
                                        if (UserReader["empEmail"].ToString() == "")
                                        {
                                            TextBoxEmail.Value = "-";
                                        }
                                        else
                                        {
                                            TextBoxEmail.Value = UserReader["empEmail"].ToString();
                                        }
                                    }
                                    else
                                    {
                                        TextBoxEmail.Value = "-";
                                    }
                                TextBoxGroup.Value = UserReader["Group_Name"].ToString();
                                TextBoxAdditional.Value = UserReader["OrgAdtGroupName"].ToString();
                                TextBoxSuperior.Text = UserReader["Superior_ID"].ToString();
                                TextBoxPassword.Attributes.Add("value", UserReader["empPassword"].ToString());
                                TextBoxConfirmation.Attributes.Add("value", UserReader["empPassword"].ToString());
                                DropDownListStatus.SelectedValue = UserReader["empStatus"].ToString();
                                DropDownListRole.SelectedIndex = int.Parse(UserReader["UserGroup_ID"].ToString()) - 2;

                                TextBoxPassword.Enabled = true;
                                TextBoxConfirmation.Enabled = true;
                                DropDownListStatus.Enabled = true;

                                string get_superior_name = "SELECT empName FROM ScorecardUser WHERE EmpId='" + TextBoxSuperior.Text + "'";
                                SqlCommand sql_get_superior_name = new SqlCommand(get_superior_name, conn);
                                using (SqlDataReader SuperiorReader = sql_get_superior_name.ExecuteReader())
                                {
                                    if (SuperiorReader.HasRows)
                                    {
                                        while (SuperiorReader.Read())
                                        {
                                            LabelSuperiorName.Text = SuperiorReader["empName"].ToString();
                                            LabelSuperiorName.Visible = true;
                                        }
                                    }
                                    else
                                    {
                                        LabelSuperiorName.Visible = false;
                                    }
                                }

                                SpanAddUser.Attributes.Remove("disabled");

                                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-success");//default color
                            }
                        }
                        else
                        {
                            LabelUserNIK_Breadcrumb.Text = "NIK not found";
                            LabelUserName_Breadcrumb.Text = "Name not found";
                            LabelUserNIK_Title.Text = "Nik not found";
                            LabelUserName_Title.Text = "Name not found";

                            TextBoxNIK.Text = "NIK Not Found";
                            TextBoxName.Value = "Name Not Found";
                            TextBoxOrganization.Value = "Organization Not Found";
                            TextBoxJobTitle.Value = "Job Title Not Found";
                            TextBoxEmail.Value = "E-Mail Not Found";
                            TextBoxGroup.Value = "Scorecard Group Not Found";
                            TextBoxAdditional.Value = "Additional Group Not Found";
                            TextBoxGrade.Value = "Grade Not Found";
                            TextBoxJobLevelName.Value = "Job Level Name Not Found";
                            TextBoxPassword.Enabled = false;
                            TextBoxConfirmation.Enabled = false;
                            DropDownListStatus.Enabled = false;

                            SpanAddUser.Attributes.Add("disabled", "true");

                            TextBoxColorConfirmation.Attributes.Add("class", "form-group");//default color
                        }
                    }
                    conn.Close();
                }
            }
        }

        protected void TextBoxConfirmation_TextChanged(object sender, EventArgs e)
        {
            if (TextBoxConfirmation.Text != TextBoxPassword.Text)//jika Confirmation Password BUKAN SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Add("disabled", "true");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:visible; color:red; font-weight:bold; margin-bottom:-5px !important; margin-top:5px !important");
                TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-error");
            }
            else//jika Confirmation Password SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Remove("disabled");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
                TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-success");
            }
        }

        protected void OnSuperiorChanged(object sender, EventArgs e)
        {
            if (TextBoxSuperior.Text == TextBoxNIK.Text)
            {
                LabelSuperiorName.Visible = true;
                LabelSuperiorName.Text = "Cannot use same NIK";
                LabelSuperiorName.Attributes.Add("style", "color:red; font-weight:bold");
                SpanAddUser.Attributes.Add("class", "btn btn-add-more-finance btn-add-more-container add-button disabled");
            }
            else if (TextBoxSuperior.Text == "")
            {
                LabelSuperiorName.Visible = false;
                //SpanAddUser.Attributes.Add("class", "btn btn-add-more-finance btn-add-more-container add-button disabled");
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
                                SpanAddUser.Attributes.Add("class", "btn btn-add-more-finance btn-add-more-container add-button");
                            }
                        }
                        else
                        {
                            LabelSuperiorName.Visible = true;
                            LabelSuperiorName.Text = "NIK Not Found";
                            LabelSuperiorName.Attributes.Add("style", "color:red; font-weight:bold");
                            SpanAddUser.Attributes.Add("class", "btn btn-add-more-finance btn-add-more-container add-button disabled");
                        }
                        SuperiorReader.Dispose();
                        SuperiorReader.Close();
                    }
                    conn.Close();
                }
            }
        }

        protected void OnClickEditUser(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var user_id = Request.QueryString["user_id"];
            var period_id = Request.QueryString["period_id"];

            var nik = Request.QueryString["nik"];
            var org = Request.QueryString["organization"];
            var adt_org = Request.QueryString["adt_organization"];
            var bsc_group = Request.QueryString["bsc_group"];
            var name = Request.QueryString["name"];
            var active = Request.QueryString["active"];
            var superior = Request.QueryString["superior"];

            

            if (TextBoxConfirmation.Text != TextBoxPassword.Text)//jika Confirmation Password BUKAN SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Add("disabled", "true");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:visible; color:red; font-weight:bold; margin-bottom:-5px !important; margin-top:5px !important");
                TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-error");
            }
            else//jika Confirmation Password SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Remove("disabled");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
                TextBoxPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-success");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    string orgadtcode;
                    string string_select_orgadtcode = "SELECT OrgAdtCode FROM [Human_Capital_demo].dbo.OrgAdtGroup WHERE OrgAdtName='" + TextBoxAdditional.Value + "'";
                    SqlConnection hc_connect = new SqlConnection(str_connect2);
                    SqlCommand sql_select_orgadtcode = new SqlCommand(string_select_orgadtcode, hc_connect);
                    hc_connect.Open();
                    orgadtcode = (string)sql_select_orgadtcode.ExecuteScalar();
                    hc_connect.Close();

                    if (orgadtcode != null)
                    {
                        string password_in_db = "", status_in_db = "", user_update, date_update, MD5_Password;
                        string string_update_user = "";

                        string_update_user = "UPDATE ScorecardUser SET empPassword=@empPassword, empStatus=@empStatus, "
                                            +"user_update=@user_update, date_update=@date_update, UserGroup_ID=@role, Superior_ID=@superior_id WHERE user_id=" + user_id + "";
                        SqlCommand sql_update_user = new SqlCommand(string_update_user, conn);

                        string string_check_password_and_status = "SELECT empPassword, empStatus FROM ScorecardUser WHERE user_id=" + user_id + "";
                        SqlCommand sql_check_password_and_status = new SqlCommand(string_check_password_and_status, conn);
                        using (SqlDataReader PasswordStatusReader = sql_check_password_and_status.ExecuteReader())
                        {
                            while (PasswordStatusReader.Read())
                            {
                                password_in_db = PasswordStatusReader["empPassword"].ToString();
                                status_in_db = PasswordStatusReader["empStatus"].ToString();
                            }
                        }

                        user_update = Session["user_name"].ToString();
                        date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                        sql_update_user.Parameters.AddWithValue("@empStatus", DropDownListStatus.SelectedValue);
                        sql_update_user.Parameters.AddWithValue("@user_update", user_update);
                        sql_update_user.Parameters.AddWithValue("@date_update", date_update);
                        sql_update_user.Parameters.AddWithValue("@role", DropDownListRole.SelectedIndex + 2);
                        sql_update_user.Parameters.AddWithValue("@superior_id", TextBoxSuperior.Text);

                        if (TextBoxPassword.Text == password_in_db)//untuk ERROR HANDLING jika ada USER memilih EDIT USER tanpa mengubah Password sama sekali
                        {
                            sql_update_user.Parameters.AddWithValue("@empPassword", TextBoxPassword.Text);
                        }
                        else//jika PASSWORD di-UPDATE
                        {
                            //ubah Password dengan MD5
                            using (MD5 md5Hash = MD5.Create())
                            {
                                string hash = GetMd5Hash(md5Hash, TextBoxPassword.Text);
                                MD5_Password = hash;
                                sql_update_user.Parameters.AddWithValue("@empPassword", MD5_Password);
                            }
                        }
                        sql_update_user.ExecuteNonQuery();
                        conn.Close();

                        //code untuk E-Mail
                        if(TextBoxEmail.Value!="-")
                        {
                            //code untuk E-Mail
                            if (TextBoxPassword.Text != password_in_db && DropDownListStatus.SelectedValue != status_in_db)
                            {
                                sendMailPassStatus(TextBoxName.Value, TextBoxNIK.Text, TextBoxPassword.Text, DropDownListStatus.SelectedValue, TextBoxEmail.Value);
                            }
                            else if (DropDownListStatus.SelectedValue != status_in_db)
                            {
                                sendMailStatus(TextBoxName.Value, TextBoxNIK.Text, DropDownListStatus.SelectedValue, TextBoxEmail.Value);
                            }
                            else if (TextBoxPassword.Text != password_in_db)
                            {
                                sendMailPassword(TextBoxName.Value, TextBoxNIK.Text, TextBoxPassword.Text, TextBoxEmail.Value);
                            }
                        }

                        if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "';", true);
                        }
                        else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&nik=" + nik + "';", true);
                        }
                        else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null && active == null && superior == null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&org=" + org + "';", true);
                        }
                        else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null && active == null && superior == null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&adt_org=" + adt_org + "';", true);
                        }
                        else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null && active == null && superior == null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&bsc_group=" + bsc_group + "';", true);
                        }
                        else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null && active == null && superior == null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&name=" + name + "';", true);
                        }
                        else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active != null && superior == null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&active=" + active + "';", true);
                        }
                        else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior != null)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('" + TextBoxName.Value + " (" + TextBoxNIK.Text + ") Has Been Updated'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&superior=" + superior + "';", true);
                        }
                    }
                    else//error handling jika Group_Name User belum di Grouping
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Update Error'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "';", true);
                    }
                }//end of SqlConnection Update
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

        public void sendMailPassword(string name, string nik, string password, string email)
        {
            SqlConnection conn = new SqlConnection(str_connect2);
            string string_get_marital_sex_user = "SELECT EmpSex, EmpMaritalSt FROM [Human_Capital_demo].dbo.Employee "
                                               + "WHERE EmpId = '" + nik + "'";
            string strApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];
            conn.Open();
            string user_title = "";
            SqlCommand sql_get_marital_and_sex = new SqlCommand(string_get_marital_sex_user, conn);
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
                msg.Subject = "Password Changed";
                msg.Body = "Dear, " + user_title + " " + name + " ( " + nik + " )<br/>"
                            + "Your password has been changed.<br/><br/>"
                            + "Your New Password: " + password + "<br/><br/>"
                            + "Try to Login with your new password at: " + strApplicationURL + "<br/><br/>"
                            + "Thank You.<br/><br/><br/>"
                            + "Best Regards,<br/>"
                            + "Human Capital Department"
                            + "<br/><br/>This is an automatically generated email – please do not reply to it.";
                msg.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["emailSender"]);
                msg.To.Add(email);
                msg.IsBodyHtml = true;
                mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                mailclient.Send(msg);
            }
        }

        public void sendMailStatus(string name, string nik, string status, string email)
        {
            SqlConnection conn = new SqlConnection(str_connect2);
            string string_get_marital_sex_user = "SELECT EmpSex, EmpMaritalSt FROM [Human_Capital_demo].dbo.Employee "
                                               + "WHERE EmpId = '" + nik + "'";
            conn.Open();
            string user_title = "";
            SqlCommand sql_get_marital_and_sex = new SqlCommand(string_get_marital_sex_user, conn);
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
                string active_inactive = "";

                if (status == "Yes") active_inactive = "Active";
                else active_inactive = "Inactive";

                msg.Subject = "Active Status Changed";
                msg.Body = "Dear, " + user_title + " " + name + " ( " + nik + " )<br/>"
                            + "Your Active Status has been changed.<br/><br/>"
                            + "Your Currect Status: " + active_inactive.ToUpper() + "<br/><br/>"
                            + "Thank You.<br/><br/><br/>"
                            + "Best Regards,<br/>"
                            + "Human Capital Department"
                            + "<br/><br/>This is an automatically generated email – please do not reply to it.";
                msg.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["emailSender"]);
                msg.To.Add(email);
                msg.IsBodyHtml = true;
                mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                mailclient.Send(msg);
            }
        }

        public void sendMailPassStatus(string name, string nik, string password, string status, string email)
        {
            SqlConnection conn = new SqlConnection(str_connect2);
            string string_get_marital_sex_user = "SELECT EmpSex, EmpMaritalSt FROM [Human_Capital_demo].dbo.Employee "
                                               + "WHERE EmpId = '" + nik + "'";
            conn.Open();
            string user_title = "";
            SqlCommand sql_get_marital_and_sex = new SqlCommand(string_get_marital_sex_user, conn);
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
                string active_inactive = "";

                if (status == "Yes") active_inactive = "Active";
                else active_inactive = "Inactive";

                msg.Subject = "Password and Active Status Changed";
                msg.Body = "Dear, " + user_title + " " + name + " ( " + nik + " )<br/>"
                            + "Your Password and Active Status has been changed.<br/><br/>"
                            + "Your New Password: " + password + "<br/>"
                            + "Your Currect Status:" + active_inactive.ToUpper() + "<br/><br/>"
                            + "Thank You.<br/><br/><br/>"
                            + "Best Regards,<br/>"
                            + "Human Capital Department"
                            + "<br/><br/>This is an automatically generated email – please do not reply to it.";
                msg.From = new MailAddress(System.Configuration.ConfigurationManager.AppSettings["emailSender"]);
                msg.To.Add(email);
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