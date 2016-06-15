﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class import_user : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        string str_connect2 = ConfigurationManager.ConnectionStrings["HumanCapitalConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (Session["user_name"] == null)
            {
                Response.Redirect("" + baseUrl + "index.aspx");
            }
            ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var filter = Request.QueryString["filter"];

            if (filter == null) scorecard_user_link.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "");
            else scorecard_user_link.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&filter=" + filter + "");

            if (filter == null) cancel_import_user.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "");
            else cancel_import_user.Attributes.Add("href", "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&filter=" + filter + "");

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID="+period_id+"";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan UserGroup
                                           + "WHERE Access_Rights_Code NOT IN "                       
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
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
                    AccessReader.Dispose();
                    AccessReader.Close();
                }
                using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                {
                    while(PeriodReader.Read())
                    {
                        string start_date_formatted, end_date_formatted;
                        DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                        DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                        start_date_formatted = start_date.ToString("MMM");
                        end_date_formatted = end_date.ToString("MMM yyyy");
                        LabelPeriod.Text = start_date_formatted + " - " + end_date_formatted;
                    }
                    PeriodReader.Dispose();
                    PeriodReader.Close();
                }
                conn.Close();
            }

        }

        protected void OnClickImport(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var filter = Request.QueryString["filter"];

            

            StringBuilder SameNIK = new StringBuilder();
            string strFileType = System.IO.Path.GetExtension(FileUploadExcel.FileName).ToString().ToLower();//.pdf, .xls
            if (FileUploadExcel.HasFile)
            {
                if (strFileType == ".xls" || strFileType == ".xlsx")
                {
                    string CurrentFilePath = Server.MapPath(FileUploadExcel.FileName);//agar dapat full directory
                    FileUploadExcel.SaveAs(CurrentFilePath);//harus SaveAs agar Nama File-nya tersimpan pada Server
                    OleDbConnection cnn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + CurrentFilePath + ";Extended Properties='Excel 8.0;HDR=[1]'");
                    cnn.Open();

                    string sheet_name = cnn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["TABLE_NAME"].ToString();//mengambil Sheet Name di EXCEL USER
                    OleDbCommand com = new OleDbCommand("SELECT * FROM [" + sheet_name + "]", cnn);//untuk membaca Sheet pada Excel
                    OleDbDataReader ExcelReader = com.ExecuteReader();

                    //inisialisasi
                    string NIK = "";
                    //string employee_name = "";
                    //string employee_org = "";
                    //string employee_job_title = "";
                    //string employee_adtcode = "";
                    string employee_pass = "";
                    string employee_status = "";
                    string employee_superior_id = "";
                    string user_group_id = "";
                    //string employee_email = "";
                    string MD5_Password = "";

                    int error_flag = 0;

                    while (ExcelReader.Read())
                    {
                        NIK = ExcelReader[1].ToString();//KOLOM 2 pada Excel <-- yang menjadi kunci utama
                        //employee_name = ExcelReader[2].ToString();// KOLOM 3 pada Excel
                        //employee_org = ExcelReader[3].ToString();// KOLOM 4 pada Excel
                        //employee_job_title = ExcelReader[4].ToString();
                        //employee_joblvl = ExcelReader[5].ToString();
                        //employee_additional = ExcelReader[6].ToString();
                        //employee_email = ExcelReader[7].ToString();
                        employee_pass = ExcelReader[8].ToString();
                        employee_status = ExcelReader[9].ToString();
                        employee_superior_id = ExcelReader[10].ToString();
                        user_group_id = ExcelReader[11].ToString();

                        using (SqlConnection conn = new SqlConnection(str_connect))
                        {
                            conn.Open();

                            string string_select_NIK = "SELECT EmpId FROM ScorecardUser WHERE EmpId='" + NIK + "'";//cek apakah sudah terdaftar di ScorecardUser
                            SqlCommand sql_select_NIK = new SqlCommand(string_select_NIK, conn);
                            using (SqlDataReader NIKReader = sql_select_NIK.ExecuteReader())
                            {
                                if (!NIKReader.HasRows)//jika belum terdaftar di ScorecardUser
                                {
                                    using (SqlConnection hc_conn = new SqlConnection(str_connect2))//cek NIK pada Database HRIS
                                    {
                                        hc_conn.Open();
                                        string string_select_hris_db = "select empname,	OrgCode, JobTtlCode, empemail, "
                                                                     + "OrgAdtGroupCode, JobLvlCode from [Human_Capital_demo].dbo.Employee (nolock) "
                                                                     + "JOIN [Human_Capital_demo].dbo.Organization (nolock) ON employee.emporg = Organization.orgcode "
                                                                     + "JOIN [Human_Capital_demo].dbo.JobTitle (nolock) ON employee.EmpJobTtl = jobtitle.JobTtlCode "
                                                                     + "JOIN [Human_Capital_demo].dbo.JobLevel (nolock) ON employee.EmpJobLvl = JobLevel.JobLvlCode "
                                                                     + "JOIN [Balanced Scorecard].dbo.ScorecardGroupLink (nolock) ON Organization.OrgAdtGroup = ScorecardGroupLink.OrgAdtGroupCode "
                                                                     + "JOIN [Human_Capital_demo].dbo.OrgAdtGroup (nolock) ON OrgAdtGroup.OrgAdtCode = ScorecardGroupLink.OrgAdtGroupCode "
                                                                     + "JOIN [Balanced Scorecard].dbo.BSC_Period ON ScorecardGroupLink.Period_ID = BSC_Period.Period_ID AND BSC_Period.Period_ID=" + period_id + " "
                                                                     + "WHERE employee.empdateend is null and employee.EmpId = '" + NIK + "'";
                                        SqlCommand sql_select_hris_db = new SqlCommand(string_select_hris_db, hc_conn);
                                        using (SqlDataReader HRIS_Reader = sql_select_hris_db.ExecuteReader())
                                        {
                                            if (HRIS_Reader.HasRows)
                                            {
                                                while (HRIS_Reader.Read())
                                                {
                                                    string user_create, date_create, user_update, date_update;
                                                    string string_insert_to_user = "INSERT INTO ScorecardUser VALUES(@EmpId, @empName, @empOrgCode, @empJobTitleCode, "
                                                                                +  "@empPassword, @empStatus, @user_create, @date_create, @user_update, @date_update, "
                                                                                +  "@empOrgAdtGroupCode, @empEmail, @usergroup_id, @empgrade, @Superior_ID)";
                                                    SqlCommand sql_insert_to_user = new SqlCommand(string_insert_to_user, conn);

                                                    user_create = Session["user_name"].ToString();
                                                    user_update = Session["user_name"].ToString();
                                                    date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                                    date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                                                    if (employee_superior_id == "")//Jika Superior_ID = null, langsung insert
                                                    {
                                                        sql_insert_to_user.Parameters.AddWithValue("@EmpId", NIK);
                                                        sql_insert_to_user.Parameters.AddWithValue("@empName", HRIS_Reader["empname"].ToString());
                                                        sql_insert_to_user.Parameters.AddWithValue("@empOrgCode", HRIS_Reader["OrgCode"].ToString());
                                                        sql_insert_to_user.Parameters.AddWithValue("@empJobTitleCode", HRIS_Reader["JobTtlCode"].ToString());
                                                        sql_insert_to_user.Parameters.AddWithValue("@empStatus", employee_status);
                                                        sql_insert_to_user.Parameters.AddWithValue("@user_create", user_create);
                                                        sql_insert_to_user.Parameters.AddWithValue("@date_create", date_create);
                                                        sql_insert_to_user.Parameters.AddWithValue("@user_update", user_update);
                                                        sql_insert_to_user.Parameters.AddWithValue("@date_update", date_update);
                                                        sql_insert_to_user.Parameters.AddWithValue("@empOrgAdtGroupCode", HRIS_Reader["OrgAdtGroupCode"].ToString());
                                                        if (HRIS_Reader["empemail"].ToString() == "")
                                                        {
                                                            sql_insert_to_user.Parameters.AddWithValue("@empEmail", "-");
                                                        }
                                                        else
                                                        {
                                                            sql_insert_to_user.Parameters.AddWithValue("@empEmail", HRIS_Reader["empemail"].ToString());
                                                        }
                                                        sql_insert_to_user.Parameters.AddWithValue("@usergroup_id", user_group_id);
                                                        sql_insert_to_user.Parameters.AddWithValue("@empgrade", HRIS_Reader["JobLvlCode"].ToString());
                                                        sql_insert_to_user.Parameters.AddWithValue("@Superior_ID", employee_superior_id);

                                                        //hash employee_pass
                                                        using (MD5 md5Hash = MD5.Create())
                                                        {
                                                            string hash = GetMd5Hash(md5Hash, employee_pass);
                                                            MD5_Password = hash;
                                                            sql_insert_to_user.Parameters.AddWithValue("@empPassword", MD5_Password);
                                                        }

                                                        if (!string.IsNullOrEmpty(HRIS_Reader["empemail"].ToString()))
                                                        {
                                                            sendMail(HRIS_Reader["empname"].ToString(), NIK, employee_pass, HRIS_Reader["empemail"].ToString());
                                                        }
                                                        sql_insert_to_user.ExecuteNonQuery();
                                                    }
                                                    else
                                                    {
                                                        string check_superior_id = "SELECT EmpId FROM ScorecardUser WHERE EmpId='" + employee_superior_id + "'";
                                                        SqlCommand sql_check_superior_id = new SqlCommand(check_superior_id, conn);
                                                        using (SqlDataReader SuperiorReader = sql_check_superior_id.ExecuteReader())
                                                        {
                                                            if (SuperiorReader.HasRows)//Jika NIK Superior sudah terdaftar di DB User, langsung INSERT
                                                            {
                                                                if (NIK != employee_superior_id)
                                                                {
                                                                    sql_insert_to_user.Parameters.AddWithValue("@EmpId", NIK);
                                                                    sql_insert_to_user.Parameters.AddWithValue("@empName", HRIS_Reader["empname"].ToString());
                                                                    sql_insert_to_user.Parameters.AddWithValue("@empOrgCode", HRIS_Reader["OrgCode"].ToString());
                                                                    sql_insert_to_user.Parameters.AddWithValue("@empJobTitleCode", HRIS_Reader["JobTtlCode"].ToString());
                                                                    sql_insert_to_user.Parameters.AddWithValue("@empStatus", employee_status);
                                                                    sql_insert_to_user.Parameters.AddWithValue("@user_create", user_create);
                                                                    sql_insert_to_user.Parameters.AddWithValue("@date_create", date_create);
                                                                    sql_insert_to_user.Parameters.AddWithValue("@user_update", user_update);
                                                                    sql_insert_to_user.Parameters.AddWithValue("@date_update", date_update);
                                                                    sql_insert_to_user.Parameters.AddWithValue("@empOrgAdtGroupCode", HRIS_Reader["OrgAdtGroupCode"].ToString());
                                                                    if (HRIS_Reader["empemail"].ToString() == "")
                                                                    {
                                                                        sql_insert_to_user.Parameters.AddWithValue("@empEmail", "-");
                                                                    }
                                                                    else
                                                                    {
                                                                        sql_insert_to_user.Parameters.AddWithValue("@empEmail", HRIS_Reader["empemail"].ToString());
                                                                    }
                                                                    sql_insert_to_user.Parameters.AddWithValue("@usergroup_id", user_group_id);
                                                                    sql_insert_to_user.Parameters.AddWithValue("@empgrade", HRIS_Reader["JobLvlCode"].ToString());
                                                                    sql_insert_to_user.Parameters.AddWithValue("@Superior_ID", employee_superior_id);

                                                                    //hash employee_pass
                                                                    using (MD5 md5Hash = MD5.Create())
                                                                    {
                                                                        string hash = GetMd5Hash(md5Hash, employee_pass);
                                                                        MD5_Password = hash;
                                                                        sql_insert_to_user.Parameters.AddWithValue("@empPassword", MD5_Password);
                                                                    }

                                                                    if (!string.IsNullOrEmpty(HRIS_Reader["empemail"].ToString()))
                                                                    {
                                                                        sendMail(HRIS_Reader["empname"].ToString(), NIK, employee_pass, HRIS_Reader["empemail"].ToString());
                                                                    }
                                                                    sql_insert_to_user.ExecuteNonQuery();
                                                                }
                                                            }
                                                            else//Jika NIK Superior tidak terdaftar di DB User, langsung BREAK, baca baris excel selanjutnya
                                                            {
                                                                error_flag++;
                                                                SameNIK.Append("\\n- " + NIK);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }//end of While HRIS_Reader.Read()
                                            }
                                            else
                                            {
                                                error_flag++;
                                                SameNIK.Append("\\n- " + NIK);
                                            }
                                        }//end of HRIS_Reader
                                        hc_conn.Close();
                                    }//end of HC_Connect
                                    
                                }
                                else
                                {
                                    error_flag++;
                                    SameNIK.Append("\\n- "+NIK);
                                }
                            }
                            conn.Close();
                        }
                    }//end of While ExcelReader
                    cnn.Close();
                    if (error_flag == 0)
                    {
                        if (filter == null) ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Import Success!'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "';", true);
                        else ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Import Success!'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&filter=" + filter + "';", true);
                    }
                    else
                    {
                        if (filter == null) ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Import Success With " + error_flag + " Errors. Caused By NIK: " + SameNIK + "'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "';", true);
                        else ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Import Success With " + error_flag + " Error(s). Caused By NIK: " + SameNIK + "'); window.location='" + baseUrl + "scorecard_user.aspx?page=" + page + "&period_id=" + period_id + "&filter=" + filter + "';", true);
                    }
                }
                else
                {
                    check_file_type.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                    check_file_type.InnerText = "File type: " + strFileType + " is not supported";
                }
            }
            else
            {
                check_file_type.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                check_file_type.InnerText = "Please pick an Excel (.xls or .xlsx) file";
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
            string string_get_marital_sex_user = "SELECT EmpSex, EmpMaritalSt FROM [Human_Capital_demo].dbo.Employee "
                                               + "WHERE EmpId = '" + NIK + "'";
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
                /******************** SEND Email TO Users **************************************/
                msg.Subject = "MPPA Balanced Scorecard";
                msg.Body = "Dear, " + user_title + " " + employee_name + "<br/>"
                            + "You have been registered in MPPA Balanced Scorecard Web Application.<br/><br/>"
                            + "Your NIK: " + NIK + "<br/>"
                            + "Your Password: " + employee_password + "<br/><br/>"
                            + "Try to Login to your account at: " + strApplicationURL + "<br/><br/>"
                            + "Thank You.<br/><br/><br/>"
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