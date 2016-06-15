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
using System.Net.Mail;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class edit_review : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
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
                var review_id = Request.QueryString["review_id"];
                var page = Request.QueryString["page"];
                string string_select_scorecard_review = "SELECT * FROM ScorecardReview WHERE Review_ID=" + review_id + "";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                                    + "WHERE Access_Rights_Code NOT IN "            //UserGroup
                                                    + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                    + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                set_review_breadcrumb.Attributes.Add("href", "set_review_month.aspx?page=" + page + "");
                cancel_edit_review_month.Attributes.Add("href", "set_review_month.aspx?page=" + page + "");

                //review_error_message.Attributes.Add("style", "visibility:hidden; color:red; font-weight:bold; padding-bottom:-20px; margin-top:-25px;");

                //memberikan value kepada Drop Down List Month
                DropDownListMonth.Items.Add("January");
                DropDownListMonth.Items.Add("February");
                DropDownListMonth.Items.Add("March");
                DropDownListMonth.Items.Add("April");
                DropDownListMonth.Items.Add("May");
                DropDownListMonth.Items.Add("June");
                DropDownListMonth.Items.Add("July");
                DropDownListMonth.Items.Add("August");
                DropDownListMonth.Items.Add("September");
                DropDownListMonth.Items.Add("October");
                DropDownListMonth.Items.Add("November");
                DropDownListMonth.Items.Add("December");

                //memberikan value kepada Drop Down List Status
                DropDownListStatus.Items.Add("Active");
                DropDownListStatus.Items.Add("Inactive");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);
                    SqlCommand sql_string_select_scorecard_review = new SqlCommand(string_select_scorecard_review, conn);

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

                    using (SqlDataReader ReviewReader = sql_string_select_scorecard_review.ExecuteReader())
                    {
                        if (ReviewReader.HasRows)
                        {
                            while (ReviewReader.Read())
                            {
                                if (ReviewReader["Review_Status"].ToString() == "Active")
                                {
                                    DropDownListMonth.Enabled = false;
                                }
                                LabelReviewName.InnerText = ReviewReader["Review_Name"].ToString();
                                DropDownListMonth.SelectedValue = ReviewReader["Review_Month"].ToString();
                                DropDownListStatus.SelectedValue = ReviewReader["Review_Status"].ToString();
                            }
                        }
                        else
                        {
                            LabelReviewName.InnerText = "Review Not Found";
                            DropDownListMonth.SelectedValue = "January";
                            DropDownListStatus.SelectedValue = "Inactive";
                            DropDownListMonth.Enabled = false;
                            DropDownListStatus.Enabled = false;
                            ButtonEdit.Enabled = false;
                            SpanEditGroup.Attributes.Add("disabled", "true");
                        }
                        ReviewReader.Dispose();
                        ReviewReader.Close();
                    }
                    conn.Close();
                }
            }
        }

        protected void OnMonthChanged(object sender, EventArgs e)
        {
            var review_id = Request.QueryString["review_id"];
            var page = Request.QueryString["page"];
            int selected_month = MonthCharToInt(DropDownListMonth.SelectedValue);
            int current_month = DateTime.Now.Month;

            if (selected_month < current_month && DropDownListStatus.SelectedValue == "Active")
            {
                check_review_month.Attributes.Add("style", "color:red; font-weight:bold; visibility:visible; padding-bottom:20px; margin-top:3px");
            }
            else
            {
                bool month_exist = false;
                string review_name = "";
                string get_review_name = "SELECT Review_Name FROM ScorecardReview WHERE Review_ID=" + review_id + "";
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_get_review_name = new SqlCommand(get_review_name, conn);
                    review_name = sql_get_review_name.ExecuteScalar().ToString();

                    string string_check_month = "SELECT Review_Month FROM ScorecardReview WHERE Review_Month='" + DropDownListMonth.SelectedValue + "' "
                                               + "AND Review_Name ='" + review_name + "' AND Review_ID<>" + review_id + "";
                    SqlCommand sql_string_check_month = new SqlCommand(string_check_month, conn);
                    using (SqlDataReader MonthReader = sql_string_check_month.ExecuteReader())
                    {
                        while (MonthReader.Read())
                        {
                            if (MonthReader["Review_Month"].ToString() == DropDownListMonth.SelectedValue)
                            {
                                month_exist = true;
                            }
                        }
                        MonthReader.Dispose();
                        MonthReader.Close();
                    }
                    conn.Close();
                }
                check_review_month.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                if (month_exist == true)
                {
                    check_review_month_exist.Attributes.Add("style", "color:red; font-weight:bold; visibility:visible; padding-bottom:20px; margin-top:3px");
                }
                else
                {
                    check_review_month_exist.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                }
            }
        }

        /*protected void OnReviewableChanged(object sender, EventArgs e)
        {
            var review_id = Request.QueryString["review_id"];
            int month_num, selected_month_num;
            string string_select_month_num = "SELECT month_num FROM ScorecardReview WHERE Review_ID=" + review_id + "";
            selected_month_num = int.Parse(DropDownListReviewable.SelectedValue);
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_select_month_num = new SqlCommand(string_select_month_num, conn);
                month_num = int.Parse(sql_select_month_num.ExecuteScalar().ToString());
                conn.Close();
            }

            if ((DropDownListMonth.SelectedIndex + 1) - selected_month_num <= 0)
            {
                review_error_message.Attributes.Add("style", "visibility:visible; width:200px; padding-bottom:20px; margin-top:2px; color:red; font-weight:bold");
                SpanEditGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
            }
            /*else if((DropDownListMonth.SelectedIndex + 1) - selected_month_num <= 0)
            {
                review_error_message.Attributes.Add("style", "visibility:visible; width:200px; padding-bottom:20px; margin-top:2px; color:red; font-weight:bold");
                SpanEditGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
            }
            else
            {
                review_error_message.Attributes.Add("style", "visibility:hidden; color:red; font-weight:bold; padding-bottom:-20px; margin-top:-25px");
                SpanEditGroup.Attributes.Add("class", "btn btn-success btn-add-group btn-add-group-container edit-button");
            }
        }*/

        protected void OnStatusChanged(object sender, EventArgs e)
        {
            var review_id = Request.QueryString["review_id"];
            int selected_month = MonthCharToInt(DropDownListMonth.SelectedValue);
            int current_month = DateTime.Now.Month;
            string string_select_active_review = "SELECT Review_Status FROM ScorecardReview "
                                                + "WHERE Review_Name='" + LabelReviewName.InnerText + "' AND Review_Status='Active' "
                                                + "AND Review_ID<>" + review_id + "";

            if (selected_month < current_month && DropDownListStatus.SelectedValue == "Active")
            {
                check_review_month.Attributes.Add("style", "color:red; font-weight:bold; visibility:visible; padding-bottom:20px; margin-top:3px");
            }
            else if (selected_month < current_month && DropDownListStatus.SelectedValue == "Inactive")
            {
                check_review_month.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
            }

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_string_select_active_review = new SqlCommand(string_select_active_review, conn);
                using (SqlDataReader StatusReader = sql_string_select_active_review.ExecuteReader())
                {
                    if (StatusReader.HasRows && DropDownListStatus.SelectedValue=="Active")
                    {
                        check_review_status.Attributes.Add("style", "color:red; font-weight:bold; visibility:visible; padding-bottom:20px; margin-top:3px");
                        SpanEditGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
                    }
                    else if (StatusReader.HasRows && DropDownListStatus.SelectedValue == "Inactive")
                    {
                        check_review_status.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                        SpanEditGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button");
                    }
                    StatusReader.Dispose();
                    StatusReader.Close();
                }
                conn.Close();
            }

        }

        protected void OnClickEdit(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var review_id = Request.QueryString["review_id"];
            var page = Request.QueryString["page"];
            int selected_month = MonthCharToInt(DropDownListMonth.SelectedValue);
            int current_month = DateTime.Now.Month;
            bool period_active = true;
            string string_update_review_month = "UPDATE ScorecardReview SET Review_Month=@new_month, Review_Status=@new_status,"
                                               +"user_update=@user_update, date_update=@date_update, month_num=@new_month_num "
                                               + "WHERE Review_ID=" + review_id + "";
            string string_select_review_status = "SELECT Review_Status FROM ScorecardReview WHERE Review_ID=" + review_id + "";
            string get_user_email = "SELECT empEmail FROM ScorecardUser WHERE EmpId=" + Session["user_nik"] + "";
            string date_update, user_update, review_status, user_email;
            SqlConnection conn_status = new SqlConnection(str_connect);
            conn_status.Open();
            SqlCommand sql_review_status = new SqlCommand(string_select_review_status, conn_status);
            review_status = sql_review_status.ExecuteScalar().ToString();
            conn_status.Close();

            user_update = Session["user_name"].ToString();
            date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (review_status == "Active")
            {
                string update_review_month = "UPDATE ScorecardReview SET Review_Month=@new_month, Review_Status=@new_status, "
                                           + "user_update=@user_update, date_update=@date_update WHERE Review_ID=" + review_id + "";
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_get_email = new SqlCommand(get_user_email, conn);
                    user_email = (string)sql_get_email.ExecuteScalar();
                    SqlCommand sql_update_review_month = new SqlCommand(update_review_month, conn);
                    sql_update_review_month.Parameters.AddWithValue("@new_month", DropDownListMonth.SelectedValue);
                    sql_update_review_month.Parameters.AddWithValue("@new_status", DropDownListStatus.SelectedValue);
                    sql_update_review_month.Parameters.AddWithValue("@user_update", user_update);
                    sql_update_review_month.Parameters.AddWithValue("@date_update", date_update);
                    sql_update_review_month.ExecuteNonQuery();
                    sendMailDeactiveReview(LabelReviewName.InnerText, DropDownListMonth.SelectedValue, user_email);
                    conn.Close();

                    
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Review Status Changed Successfully'); window.location='" + baseUrl + "set_review_month.aspx?page=" + page + "';", true);
                }
            }
            else
            {
                if (selected_month < current_month && DropDownListStatus.SelectedValue == "Active")
                {
                    check_review_month.Attributes.Add("style", "color:red; font-weight:bold; visibility:visible; padding-bottom:20px; margin-top:3px");
                    check_review_month_exist.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                }
                else
                {
                    bool month_exist = false;
                    string review_name = "";
                    string get_review_name = "SELECT Review_Name FROM ScorecardReview WHERE Review_ID=" + review_id + "";
                    using (SqlConnection conn = new SqlConnection(str_connect))
                    {
                        conn.Open();
                        SqlCommand sql_get_review_name = new SqlCommand(get_review_name, conn);
                        review_name = sql_get_review_name.ExecuteScalar().ToString();

                        string string_check_month = "SELECT Review_Month FROM ScorecardReview WHERE Review_Month='" + DropDownListMonth.SelectedValue + "' "
                                                   + "AND Review_Name ='" + review_name + "' AND Review_ID<>" + review_id + "";
                        SqlCommand sql_string_check_month = new SqlCommand(string_check_month, conn);
                        using (SqlDataReader MonthReader = sql_string_check_month.ExecuteReader())
                        {
                            while (MonthReader.Read())
                            {
                                if (MonthReader["Review_Month"].ToString() == DropDownListMonth.SelectedValue)
                                {
                                    month_exist = true;
                                }
                            }
                            MonthReader.Dispose();
                            MonthReader.Close();
                        }
                        conn.Close();
                    }
                    check_review_month.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                    if (month_exist == true)
                    {
                        check_review_month_exist.Attributes.Add("style", "color:red; font-weight:bold; visibility:visible; padding-bottom:20px; margin-top:3px");
                    }
                    else
                    {
                        check_review_month_exist.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                        if (DropDownListStatus.SelectedValue == "Active" && selected_month < current_month)
                        {
                            check_month_pass.Attributes.Add("style", "color:red; font-weight:bold; visibility:visible; padding-bottom:20px; margin-top:3px");
                        }
                        else
                        {
                            using (SqlConnection conn = new SqlConnection(str_connect))
                            {
                                conn.Open();
                                string string_select_active_period = "SELECT Review_Status FROM ScorecardReview WHERE Review_Status='Active' "
                                                                    + "AND Review_ID<>" + review_id + " AND Review_Name='" + review_name + "'";
                                SqlCommand sql_select_active_period = new SqlCommand(string_select_active_period, conn);
                                using (SqlDataReader ActiveReader = sql_select_active_period.ExecuteReader())
                                {
                                    if (!ActiveReader.HasRows)
                                    {
                                        period_active = false;
                                    }
                                    ActiveReader.Dispose();
                                    ActiveReader.Close();
                                }

                                if (period_active == false)//jika tidak ada yang aktif
                                {
                                    SqlCommand sql_string_update_review_month = new SqlCommand(string_update_review_month, conn);
                                    SqlCommand sql_get_email = new SqlCommand(get_user_email, conn);
                                    user_email = (string)sql_get_email.ExecuteScalar();
                                    sql_string_update_review_month.Parameters.AddWithValue("@new_month", DropDownListMonth.SelectedValue);
                                    sql_string_update_review_month.Parameters.AddWithValue("@new_status", DropDownListStatus.SelectedValue);
                                    sql_string_update_review_month.Parameters.AddWithValue("@user_update", user_update);
                                    sql_string_update_review_month.Parameters.AddWithValue("@date_update", date_update);
                                    sql_string_update_review_month.Parameters.AddWithValue("@new_month_num", selected_month);
                                    sql_string_update_review_month.ExecuteNonQuery();
                                    if (DropDownListStatus.SelectedValue == "Active")//jika memilih aktif, baru kirim email ke user
                                    {
                                        sendMailActiveReview(LabelReviewName.InnerText, DropDownListMonth.SelectedValue, user_email);
                                    }

                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Review Month Updated!'); window.location='" + baseUrl + "set_review_month.aspx?page=" + page + "';", true);
                                }
                                else//jika tidak ada yang aktif, tetap SAVE tapi jangan kirim e-mail
                                {
                                    SqlCommand sql_string_update_review_month = new SqlCommand(string_update_review_month, conn);
                                    SqlCommand sql_get_email = new SqlCommand(get_user_email, conn);
                                    user_email = (string)sql_get_email.ExecuteScalar();
                                    sql_string_update_review_month.Parameters.AddWithValue("@new_month", DropDownListMonth.SelectedValue);
                                    sql_string_update_review_month.Parameters.AddWithValue("@new_status", DropDownListStatus.SelectedValue);
                                    sql_string_update_review_month.Parameters.AddWithValue("@user_update", user_update);
                                    sql_string_update_review_month.Parameters.AddWithValue("@date_update", date_update);
                                    sql_string_update_review_month.Parameters.AddWithValue("@new_month_num", selected_month);
                                    sql_string_update_review_month.ExecuteNonQuery();

                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Review Month Updated!'); window.location='" + baseUrl + "set_review_month.aspx?page=" + page + "';", true);
                                }
                                conn.Close();
                            }
                        }
                    }
                }
            }
        }

        public void sendMailActiveReview(string review_name, string review_month, string sender_email)
        {
            string get_all_email_bsc_user = "SELECT DISTINCT empEmail FROM ScorecardUser WHERE empStatus='Yes' AND EmpId<>'" + Session["emp_nik"] + "'";
            /*string diff_month;
            int review_month_int, reviewable_int, diff_month_int;
            review_month_int = MonthCharToInt(review_month);
            reviewable_int = int.Parse(reviewable);
            diff_month_int = review_month_int - reviewable_int;
            diff_month = MonthIntToChar(diff_month_int);*/

            SmtpClient mailclient = new SmtpClient();  //Karena FILE_LOCATION terjadi perubahan setiap di-klik, maka
            using (MailMessage msg = new MailMessage())//harus pake USING untuk CLEAR semua Resource yang pernah dipake
            {
                /******************** SEND Email TO Users **************************************/
                msg.Subject = "" + review_name + " BSC review period has been activated";

                
                msg.Body = "Dear all,<br/>"
                        + "Review period for: " + review_name + "</b> Balanced Scorecard has been ACTIVATED</b>.<br/>"
                        + "You can review your Individual Scorecard starts from " + review_month + " 1st</b> until the end of " + review_month + "</b>.<br/>"
                        + "Thank you for your cooperation.<br/><br/>"
                        + "Best Regards,<br/>"
                        + "Human Capital Department"
                        + "<br/><br/>This is an automatically generated email – please do not reply to it.";
                
                msg.From = new MailAddress(sender_email.ToLower());

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_get_all_email_bsc_user = new SqlCommand(get_all_email_bsc_user, conn);
                    using (SqlDataReader EmailReader = sql_get_all_email_bsc_user.ExecuteReader())
                    {
                        while (EmailReader.Read())
                        {
                            string recipient_email;
                            if (DBNull.Value.Equals(EmailReader["empEmail"]))
                            {
                                recipient_email = "unknown@email.com";
                            }
                            else
                            {
                                recipient_email = EmailReader["empEmail"].ToString();
                            }
                            msg.To.Add(new MailAddress(recipient_email.ToLower()));
                        }
                        EmailReader.Dispose();
                        EmailReader.Close();
                    }
                    conn.Close();
                }
                msg.IsBodyHtml = true;
                mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                mailclient.Send(msg);
            }
        }

        public void sendMailDeactiveReview(string review_name, string review_month, string sender_email)
        {
            string get_all_email_bsc_user = "SELECT DISTINCT empEmail FROM ScorecardUser WHERE empStatus='Yes' AND EmpId<>'" + Session["emp_nik"] + "'";
            /*string diff_month;
            int review_month_int, reviewable_int, diff_month_int;
            review_month_int = MonthCharToInt(review_month);
            reviewable_int = int.Parse(reviewable);
            diff_month_int = review_month_int - reviewable_int;
            diff_month = MonthIntToChar(diff_month_int);*/

            SmtpClient mailclient = new SmtpClient();  //Karena FILE_LOCATION terjadi perubahan setiap di-klik, maka
            using (MailMessage msg = new MailMessage())//harus pake USING untuk CLEAR semua Resource yang pernah dipake
            {
                /******************** SEND Email TO Users **************************************/
                msg.Subject = "" + review_name + " BSC review period has been deactivated";

                msg.Body = "Dear all,<br/>"
                        + "Review period for: " + review_name + "</b> Balanced Scorecard has been DEACTIVATED</b>.<br/>"
                        + "Thank you for your cooperation.<br/><br/>"
                        + "Best Regards,<br/>"
                        + "Human Capital Department"
                        + "<br/><br/>This is an automatically generated email – please do not reply to it.";
                
                msg.From = new MailAddress(sender_email.ToLower());

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_get_all_email_bsc_user = new SqlCommand(get_all_email_bsc_user, conn);
                    using (SqlDataReader EmailReader = sql_get_all_email_bsc_user.ExecuteReader())
                    {
                        while (EmailReader.Read())
                        {
                            string recipient_email;
                            if (DBNull.Value.Equals(EmailReader["empEmail"]))
                            {
                                recipient_email = "unknown@email.com";
                            }
                            else
                            {
                                recipient_email = EmailReader["empEmail"].ToString();
                            }
                            msg.To.Add(new MailAddress(recipient_email.ToLower()));
                        }
                        EmailReader.Dispose();
                        EmailReader.Close();
                    }
                    conn.Close();
                }
                msg.IsBodyHtml = true;
                mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                mailclient.Send(msg);
            }
        }

        public int MonthCharToInt(string selected_month)
        {
            int int_month = 0;
            switch (selected_month)
            {
                case "January":
                    int_month = 1; break;
                case "February":
                    int_month = 2; break;
                case "March":
                    int_month = 3; break;
                case "April":
                    int_month = 4; break;
                case "May":
                    int_month = 5; break;
                case "June":
                    int_month = 6; break;
                case "July":
                    int_month = 7; break;
                case "August":
                    int_month = 8; break;
                case "September":
                    int_month = 9; break;
                case "October":
                    int_month = 10; break;
                case "November":
                    int_month = 11; break;
                case "December":
                    int_month = 12; break;
            }
            return int_month;
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