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
using System.IO;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class set_period : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        StringBuilder HtmlPeriodTable = new StringBuilder();
        StringBuilder Pagination = new StringBuilder();//untuk Pagination
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
                var paging = Request.QueryString["page"];//untuk pagination
                int page = 0;
                decimal no_header = 0;//inisialisasi
                decimal data_per_page = 5, max_select_data = 0, max_page = 0;//untuk pagination
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='set_period' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    SqlCommand sql_check_access_page = new SqlCommand(string_check_access_page, conn);
                    SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);
                    conn.Open();

                    using (SqlDataReader PageReader = sql_check_access_page.ExecuteReader())
                    {
                        if (!PageReader.HasRows)
                        {
                            Response.Redirect("" + baseUrl + "index.aspx");
                        }
                        PageReader.Close();
                        PageReader.Dispose();
                    }

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

                    if (paging == null)//untuk pertama kali Load Page
                    {
                        page = 1;
                        no_header = (1 * data_per_page) - (data_per_page - 1);//untuk no. header kolom 1 Table jika data yang ditampilkan per halaman = 5
                    }
                    else
                    {
                        page = int.Parse(paging.ToString());
                        no_header = (page * data_per_page) - (data_per_page - 1);
                    }

                    //Active Warning Message
                    string search_active_period = "SELECT Period_Status FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
                    SqlCommand sql_search_active = new SqlCommand(search_active_period, conn);
                    using (SqlDataReader ActiveReader = sql_search_active.ExecuteReader())
                    {
                        if (!ActiveReader.HasRows)
                        {
                            WarningActiveMessage.Attributes.Add("style", "margin-bottom:-15px; margin-top:20px; visibility:visible; color:red; font-size:16px");
                        }
                        else
                        {
                            WarningActiveMessage.Attributes.Add("style", "margin-bottom:0px; margin-top:-20px; visibility:hidden");
                        }
                    }

                    //link untuk add new BSC Period
                    AddNewBSCPeriod.Attributes.Add("href","add_new_period.aspx?page="+page+"");

                    string get_max_data = "SELECT COUNT(Period_ID) FROM BSC_Period WHERE data_status='exist'";
                    string select_all_period = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY Start_Period ASC) "
                                             + "AS rowNum, * FROM BSC_Period WHERE data_status='exist')sub WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND "
                                             + "rowNum<=" + data_per_page + "*" + page + "";
                    SqlCommand sql_select_all_period = new SqlCommand(select_all_period, conn);
                    SqlCommand sql_get_max_data = new SqlCommand(get_max_data, conn);
                    max_select_data = (int)sql_get_max_data.ExecuteScalar();//untuk mengetahui banyaknya page pada pagination
                    max_page = Math.Ceiling(max_select_data / data_per_page);//mendapatkan nilai banyaknya jumlah page
                    using (SqlDataReader PeriodReader = sql_select_all_period.ExecuteReader())
                    {
                        if (PeriodReader.HasRows)
                        {
                            HtmlPeriodTable.Append("<tr>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Period No.</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Start Date</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>End Date</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Description</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Status</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Edit</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Delete</th>");
                            HtmlPeriodTable.Append("</tr>");
                            while (PeriodReader.Read())
                            {
                                string start_date_formatted, end_date_formatted;//agar Time-nya tidak ditampilkan
                                DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                start_date_formatted = start_date.ToString("MMM yyyy");
                                end_date_formatted = end_date.ToString("MMM yyyy");

                                HtmlPeriodTable.Append("<tr align='center'>");
                                HtmlPeriodTable.Append("<td class='td-align'>" + no_header + "</td>");
                                HtmlPeriodTable.Append("<td class='td-align'>" + start_date_formatted + "</td>");
                                HtmlPeriodTable.Append("<td class='td-align'>" + end_date_formatted + "</td>");

                                if (PeriodReader["Description"].ToString() == "")//jika tidak ada deskripsi yang diisi
                                {
                                    HtmlPeriodTable.Append("<td class='td-align'>None</td>");
                                }
                                else
                                {
                                    HtmlPeriodTable.Append("<td class='td-align'>" + PeriodReader["Description"] + "</td>");
                                }

                                if (PeriodReader["Period_Status"].ToString() == "Active")//jika status=Active, background-color menjadi hijau, jika status=Inactive, background-color menjadi merah
                                {
                                    HtmlPeriodTable.Append("<td class='td-align active-period'>" + PeriodReader["Period_Status"] + "</td>");
                                }
                                else
                                {
                                    HtmlPeriodTable.Append("<td class='td-align inactive-period'>" + PeriodReader["Period_Status"] + "</td>");
                                }

                                HtmlPeriodTable.Append("<td class='td-align'><a href='edit_period.aspx?page="+page+"&period_id=" + PeriodReader["Period_ID"] + "' class='btn btn-default'>Edit</a></td>");
                                HtmlPeriodTable.Append("<td class='td-align'><a href='delete_period.aspx?page=" + page + "&period_id=" + PeriodReader["Period_ID"] + "' class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete this period?');\">Delete</a></td>");
                                HtmlPeriodTable.Append("</tr>");
                                no_header++;
                            }//end of While
                        }//end of if(HasRows)
                        else//jika tidak ada data period sama sekali
                        {
                            HtmlPeriodTable.Append("<tr align='center'>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>There is no period to display</th>");
                            HtmlPeriodTable.Append("</tr>");
                        }
                    }//end of SqlDataReader
                    conn.Close();
                }//end of SqlConnection
                PlaceHolderPeriod.Controls.Add(new Literal {Text = HtmlPeriodTable.ToString() });//menampilkan table period

                //Code untuk Pagination
                Pagination.Append("<ul id='my-pagination' class='pagination'></ul>");

                //Pagination JQuery
                Pagination.Append("<script>");
                Pagination.Append("$('#my-pagination').twbsPagination({");
                Pagination.Append("totalPages: " + max_page + ",");
                Pagination.Append("visiblePages: 7,");
                Pagination.Append("href: '?page={{number}}'");
                Pagination.Append("});");
                Pagination.Append("</script>");
                PlaceHolderPaging.Controls.Add(new Literal { Text = Pagination.ToString() });//untuk Pagination

            }//end of if(!IsPostBack)
        }//end of Page_Load

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