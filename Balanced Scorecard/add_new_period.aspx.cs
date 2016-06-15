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
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class add_new_period : System.Web.UI.Page
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
                var page = Request.QueryString["page"];

                //source code untuk hak akses
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
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
                    conn.Close();
                }

                //link untuk kembali ke Set BSC Period
                SetBSCPeriod.Attributes.Add("href","set_period.aspx?page=" + page + "");

                //link untuk Cancel Add Period
                CancelAddPeriod.Attributes.Add("href", "set_period.aspx?page=" + page + "");

                //insert data ke Drop Down List Status
                DropDownListStatus.Items.Add("Active");
                DropDownListStatus.Items.Add("Inactive");

                //Pilihan Default untuk Status
                DropDownListStatus.SelectedValue = "Inactive";

                //error message jika sudah ada yang Active
                FailActive.Attributes.Add("style", "margin-bottom:-20px; color:red; font-weight:bold; visibility:hidden");

                //error message jika Tahun sudah ada
                start_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                end_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                less_than_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");

                //error message jika field Tahun tidak diisi
                required_start_date.Attributes.Add("style", "visibility:hidden");
                required_end_date.Attributes.Add("style", "visibility:hidden");

                //error message jika Format Date yang dimasukkan Invalid
                invalid_start_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                invalid_end_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");

                SpanAddPeriod.Attributes.Add("class", "btn btn-success btn-add-group btn-add-group-container edit-button");

            }
        }

        protected void OnTextChanged_StartPeriod(object sender, EventArgs e)
        {
            string string_start_date = input_start_date.Text, start_date_formatted;
            int start_year = 0;
            bool start_year_exist;
            DateTime start_date;
            if (!DateTime.TryParse(string_start_date, out start_date))
            {
                invalid_start_date.Attributes.Add("style", "margin-top:-25px; color:red; font-weight:bold; visibility:visible");
            }
            else
            {
                invalid_start_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                start_date = Convert.ToDateTime(string_start_date);
                start_date_formatted = start_date.ToString("yyyy");
                start_year = int.Parse(start_date_formatted);//value TAHUN yang diinput user
                SqlConnection conn = new SqlConnection(str_connect);
                conn.Open();
                start_year_exist = CheckStartYear(start_year, conn);//check Start Year
                conn.Close();
            }
        }

        protected void OnTextChanged_EndPeriod(object sender, EventArgs e)
        {
            string string_end_date = input_end_date.Text, end_date_formatted;
            DateTime end_date;// = Convert.ToDateTime(string_end_date);
            int end_year = 0;
            bool end_year_exist;

            if (!DateTime.TryParse(string_end_date, out end_date))
            {
                invalid_end_date.Attributes.Add("style", "margin-top:-25px; color:red; font-weight:bold; visibility:visible");
            }
            else 
            {
                invalid_end_date.Attributes.Add("style", "margin-top:-25px; visibility:hidden");
                end_date = Convert.ToDateTime(string_end_date);
                end_date_formatted = end_date.ToString("yyyy");
                end_year = int.Parse(end_date_formatted);//value TAHUN yang diinput user
                SqlConnection conn = new SqlConnection(str_connect);
                conn.Open();
                end_year_exist = CheckEndYear(end_year, conn);//check Start Year
                //end_lessthan_start = CheckIfEndGreaterThanStart(date_diff);//check apakah End Period lebih besar dari Start Period
                conn.Close();
            }
        }

        protected void OnClickAddPeriod(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            string desc = description.InnerText;
            string status = DropDownListStatus.SelectedValue;
            string user_create, user_update, date_create, date_update;
            bool start_year_exist, end_year_exist, active_status_exist, end_lessthan_start, more_than_a_year;

            if (input_start_date.Text == "" && input_end_date.Text == "")
            {
                required_start_date.Attributes.Add("style", "color:red !important; margin-top:5px; font-weight:bold !important; visibility:visible");
                required_end_date.Attributes.Add("style", "color:red !important; margin-top:5px; font-weight:bold !important; visibility:visible");
                invalid_start_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                invalid_end_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                start_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                end_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                less_than_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
            }
            else if (input_end_date.Text == "")
            {
                required_start_date.Attributes.Add("style", "visibility:hidden");
                required_end_date.Attributes.Add("style", "color:red !important; margin-top:5px; font-weight:bold !important; visibility:visible");
                invalid_end_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                end_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                less_than_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
            }
            else if (input_start_date.Text == "")
            {
                required_start_date.Attributes.Add("style", "color:red !important; margin-top:5px; font-weight:bold !important; visibility:visible");
                required_end_date.Attributes.Add("style", "visibility:hidden");
                invalid_start_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                start_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
            }
            else
            {
                //hilangkan tulisan "This field is required"
                required_start_date.Attributes.Add("style", "visibility:hidden");
                required_end_date.Attributes.Add("style", "visibility:hidden");

                //untuk melihat apakah Tahun yang dimasukkan sudah ada atau belum
                string start_date_formatted, end_date_formatted;
                int start_year, end_year, date_diff;

                string string_start_date = input_start_date.Text;
                string string_end_date = input_end_date.Text;
                DateTime start_date;
                DateTime end_date;

                //cek apakah Format Date yang dimasukkan benar atau tidak (Error Handling)
                if (DateTime.TryParse(string_start_date, out start_date) && DateTime.TryParse(string_end_date, out end_date))
                {
                    start_date = Convert.ToDateTime(string_start_date);
                    end_date = Convert.ToDateTime(string_end_date);

                    start_date_formatted = start_date.ToString("yyyy");//mengambil Tahun saja dari apa yang diinput user
                    end_date_formatted = end_date.ToString("yyyy");//mengambil Tahun saja dari apa yang diinput user

                    date_diff = DateTime.Compare(start_date, end_date);
                    start_year = int.Parse(start_date_formatted);//value TAHUN yang diinput user
                    end_year = int.Parse(end_date_formatted);//value TAHUN yang diinput user

                    using (SqlConnection conn = new SqlConnection(str_connect))
                    {
                        conn.Open();//membuka koneksi ke Database
                        active_status_exist = CheckActive(conn);//check status yang Active
                        start_year_exist = CheckStartYear(start_year, conn);//check Start Year
                        end_year_exist = CheckEndYear(end_year, conn);//check End Year
                        more_than_a_year = CheckMoreThanAYear(start_year, end_year);
                        end_lessthan_start = CheckIfEndGreaterThanStart(date_diff);//check apakah End Period lebih besar dari Start Period

                        user_create = Session["user_name"].ToString();
                        user_update = Session["user_name"].ToString();
                        date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        
                        if (active_status_exist == false && start_year_exist == false && end_year_exist == false && end_lessthan_start == false && more_than_a_year == false)
                        {
                            string add_period = "INSERT INTO BSC_Period VALUES('" + input_start_date.Text + "','" + input_end_date.Text + "'" 
                                              + ",@period_description,@period_status, @user_create, @user_update, @date_create, @date_update, @data_status)";
                            SqlCommand sql_add_period = new SqlCommand(add_period, conn);
                            sql_add_period.Parameters.AddWithValue("@period_description", desc);
                            sql_add_period.Parameters.AddWithValue("@period_status", status);
                            sql_add_period.Parameters.AddWithValue("@user_create", user_create);
                            sql_add_period.Parameters.AddWithValue("@user_update", user_update);
                            sql_add_period.Parameters.AddWithValue("@date_create", date_create);
                            sql_add_period.Parameters.AddWithValue("@date_update", date_update);
                            sql_add_period.Parameters.AddWithValue("@data_status", "exist");
                            sql_add_period.ExecuteNonQuery();

                            
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Period Added!'); window.location='" + baseUrl + "set_period.aspx?page=" + page + "';", true);
                        }
                        conn.Close();//menutup koneksi ke Database
                    }
                }
                else if (!DateTime.TryParse(string_start_date, out start_date) && !DateTime.TryParse(string_end_date, out end_date))
                {
                    invalid_start_date.Attributes.Add("style", "margin-top:-25px; color:red; font-weight:bold; visibility:visible");
                    invalid_end_date.Attributes.Add("style", "margin-top:-25px; color:red; font-weight:bold; visibility:visible");
                }
                else if (!DateTime.TryParse(string_start_date, out start_date))
                {
                    invalid_start_date.Attributes.Add("style","margin-top:-25px; color:red; font-weight:bold; visibility:visible");
                    invalid_end_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                }
                else if (!DateTime.TryParse(string_end_date, out end_date))
                {
                    invalid_start_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                    invalid_end_date.Attributes.Add("style", "margin-top:-25px; color:red; font-weight:bold; visibility:visible");
                }
            }
        }

        public bool CheckActive(SqlConnection conn)//mengecek apakah sudah ada status yang Active atau belum
        {
            string select_active_period = "SELECT Period_Status FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
            SqlCommand sql_select_active_period = new SqlCommand(select_active_period, conn);
            using (SqlDataReader ActiveReader = sql_select_active_period.ExecuteReader())
            {
                if (ActiveReader.HasRows && DropDownListStatus.SelectedValue == "Active")//jika ada status yang Active. Syaratnya Dropdown yang dipilih harus "Active" juga. Jika tidak, ketika user memilih Inactive, error message-nya tetap muncul
                {
                    FailActive.Attributes.Add("style", "margin-bottom:-20px; color:red; font-weight:bold; visibility:visible");
                    return true;
                }
                else
                {
                    FailActive.Attributes.Add("style", "margin-bottom:-20px; color:red; font-weight:bold; visibility:hidden");
                    return false;
                }
            }
        }

        public bool CheckMoreThanAYear(int start_year, int end_year)
        {
            bool more_than_a_year = true;
            if (end_year - start_year != 0)
            {
                LabelYear.Visible = true;
                SpanAddPeriod.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
                more_than_a_year = true;
            }
            else if (end_year - start_year == 0)
            {
                LabelYear.Visible = false;
                invalid_end_date.Attributes.Add("style", "visibility:hidden; margin-top:-25px");
                more_than_a_year = false;
            }
            return more_than_a_year;
        }

        public bool CheckStartYear(int start_year, SqlConnection conn) //cek apakah START YEAR Ada atau Tidak
        {
            string str_select_start_year = "SELECT Start_Period FROM BSC_Period WHERE DATEPART(yyyy,Start_Period)=" + start_year + " AND data_status='exist'";
            SqlCommand sql_select_start_year = new SqlCommand(str_select_start_year, conn);
            using (SqlDataReader StartDateReader = sql_select_start_year.ExecuteReader())
            {
                if (StartDateReader.HasRows)//untuk mengecek apakah TAHUN sudah ada atau belum
                {
                    start_year_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:-5px");
                    SpanAddPeriod.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
                    return true;
                }
                else
                {
                    start_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                    SpanAddPeriod.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button");
                    return false;
                }
            }
        }

        public bool CheckEndYear(int end_year, SqlConnection conn)//cek apakah END YEAR Ada atau Tidak
        {
            string str_select_end_year = "SELECT End_Period FROM BSC_Period WHERE DATEPART(yyyy,End_Period)=" + end_year + " AND data_status='exist'";
            SqlCommand sql_select_end_year = new SqlCommand(str_select_end_year, conn);
            using (SqlDataReader EndDateReader = sql_select_end_year.ExecuteReader())
            {
                if (EndDateReader.HasRows)//untuk mengecek apakah TAHUN sudah ada atau belum
                {
                    end_year_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:-5px");
                    SpanAddPeriod.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
                    return true;
                }
                else
                {
                    end_year_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                    SpanAddPeriod.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button");
                    return false;
                }
            }
        }

        public bool CheckIfEndGreaterThanStart(int date_diff)
        {
            if (date_diff > 0)//<--untuk cek apakah end date lebih kecil dari start date
            {
                less_than_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:-5px; margin-top:-5px");
                SpanAddPeriod.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
                return true;
            }
            else
            {
                less_than_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px");
                SpanAddPeriod.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button");
                return false;
            }
        }

        protected void OnStatusChanged(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(str_connect);
            string string_select_active_status = "SELECT Period_Status FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
            SqlCommand sql_select_active_status = new SqlCommand(string_select_active_status, conn);
            conn.Open();
            using (SqlDataReader ActiveReader = sql_select_active_status.ExecuteReader())
            {
                if (ActiveReader.HasRows && DropDownListStatus.SelectedValue=="Active")
                {
                    FailActive.Attributes.Add("style", "margin-bottom:-20px; color:red; font-weight:bold; visibility:visible");
                }
                else if (ActiveReader.HasRows && DropDownListStatus.SelectedValue == "Inactive")
                {
                    FailActive.Attributes.Add("style", "margin-bottom:-20px; color:red; font-weight:bold; visibility:hidden");
                }
                ActiveReader.Dispose();
                ActiveReader.Close();
            }
            conn.Close();
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