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
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class change_password : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (!IsPostBack)
            {
                string user_nik;
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                user_nik = (string)Session["user_nik"];

                if (Session["user_name"] == null)
                {
                    
                    Response.Redirect(baseUrl + "index.aspx");
                }

                TextBoxPassword.Enabled = false;

                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-20px !important; margin-top:5px !important");
                check_new_password.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
                TextBoxColorNewPassword.Attributes.Add("class", "form-group");
                TextBoxColorConfirmation.Attributes.Add("class", "form-group");

                cancel_change.Attributes.Add("href", "financial_scorecard.aspx");

                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page

                string string_select_user_info = "SELECT * FROM ScorecardUser WHERE EmpId='" + user_nik + "'";

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_select_user_info = new SqlCommand(string_select_user_info, conn);
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

                    using (SqlDataReader UserReader = sql_select_user_info.ExecuteReader())
                    {
                        while (UserReader.Read())
                        {
                            TextBoxPassword.Attributes.Add("value", UserReader["empPassword"].ToString());
                        }
                    }
                    conn.Close();
                }
            }
        }

        protected void TextBoxConfirmation_TextChanged(object sender, EventArgs e)
        {
            string new_pass = TextBoxNewPassword.Text;
            string confirmation_pass = TextBoxConfirmation.Text;
            if (TextBoxConfirmation.Text != TextBoxNewPassword.Text)//jika Confirmation Password BUKAN SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Add("disabled", "true");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:visible; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
                TextBoxNewPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-error");
                TextBoxNewPassword.Attributes.Add("value", new_pass);
                TextBoxConfirmation.Attributes.Add("value", confirmation_pass);
                UpdatePanelConfirmation.Update();
                UpdatePanelChangeButton.Update();
            }
            else//jika Confirmation Password SAMA DENGAN Password
            {
                SpanAddUser.Attributes.Remove("disabled");
                check_password_confirmation.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-40px !important; margin-top:5px !important");
                TextBoxNewPassword.Attributes.Add("value", TextBoxPassword.Text);
                TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                TextBoxColorConfirmation.Attributes.Add("class", "form-group has-success");
                TextBoxNewPassword.Attributes.Add("value", new_pass);
                TextBoxConfirmation.Attributes.Add("value", confirmation_pass);
                UpdatePanelConfirmation.Update();
                UpdatePanelChangeButton.Update();
            }
        }

        protected void TextBoxPassword_TextChanged(object sender, EventArgs e)
        {
            string new_pass = TextBoxNewPassword.Text;
            check_new_password.Attributes.Add("style", "width:400px; visibility:hidden; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
            TextBoxColorNewPassword.Attributes.Add("class", "form-group has-success");
            TextBoxNewPassword.Attributes.Add("value", new_pass);
            UpdatePanelNewPassword.Update();
            UpdatePanelChangeButton.Update();
        }

        protected void OnClickChangePassword(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            string new_pass = TextBoxNewPassword.Text;
            string confirmation_pass = TextBoxConfirmation.Text;
            string user_update, date_update, MD5_new_pass;
            string user_nik;

            user_nik = (string)Session["user_nik"];

            user_update = Session["user_name"].ToString();
            date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (TextBoxNewPassword.Text == "")
            {
                check_new_password.Attributes.Add("style", "width:400px; visibility:visible; color:red; font-weight:bold; margin-bottom:-5px !important; margin-top:5px !important");
                TextBoxColorNewPassword.Attributes.Add("class", "form-group has-error");
                UpdatePanelNewPassword.Update();
                UpdatePanelChangeButton.Update();
            }
            else
            {
                if (TextBoxConfirmation.Text != TextBoxNewPassword.Text)//jika Confirmation Password BUKAN SAMA DENGAN Password
                {
                    SpanAddUser.Attributes.Add("disabled", "true");
                    check_password_confirmation.Attributes.Add("style", "width:400px; visibility:visible; color:red; font-weight:bold; margin-bottom:-25px !important; margin-top:5px !important");
                    TextBoxNewPassword.Attributes.Add("value", TextBoxPassword.Text);
                    TextBoxConfirmation.Attributes.Add("value", TextBoxConfirmation.Text);
                    TextBoxColorConfirmation.Attributes.Add("class", "form-group has-error");
                    TextBoxNewPassword.Attributes.Add("value", new_pass);
                    TextBoxConfirmation.Attributes.Add("value", confirmation_pass);
                    UpdatePanelConfirmation.Update();
                }
                else
                {
                    //ubah Password dengan MD5
                    using (MD5 md5Hash = MD5.Create())
                    {
                        string hash = GetMd5Hash(md5Hash, new_pass);
                        MD5_new_pass = hash;
                    }
                    string string_update_password = "UPDATE ScorecardUser SET empPassword='" + MD5_new_pass + "', "
                                                  + "user_update=@user_update, date_update=@date_update "
                                                  + "WHERE EmpId='" + user_nik + "'";
                    SqlConnection DB_Connection = new SqlConnection(str_connect);
                    SqlCommand sql_update_password = new SqlCommand(string_update_password, DB_Connection);
                    sql_update_password.Parameters.AddWithValue("@user_update", user_update);
                    sql_update_password.Parameters.AddWithValue("@date_update", date_update);
                    DB_Connection.Open();
                    sql_update_password.ExecuteNonQuery();
                    DB_Connection.Close();

                    
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Password Changed'); window.location='" + baseUrl + "change_password.aspx';", true);
                }
            }
        }//end of OnClickChangePassword

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