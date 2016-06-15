using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class set_review_month : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            StringBuilder HtmlPeriodTable = new StringBuilder();
            StringBuilder Pagination = new StringBuilder();//untuk Pagination
            if (!IsPostBack)
            {
                var paging = Request.QueryString["page"];//untuk pagination
                int page = 0, loop_quarterly = 0, loop_semesterly = 0;
                decimal no_header = 0;//inisialisasi
                decimal data_per_page = 10, max_select_data = 0, max_page = 0;//untuk pagination
                string user_nik = "", user_name;

                if ((string)Session["user_nik"] == null)
                {
                    Response.Redirect("" + baseUrl + "index.aspx");
                }
                else
                {
                    user_nik = (string)Session["user_nik"];
                }

                string string_select_user_name = "SELECT empName FROM ScorecardUser WHERE EmpId='" + user_nik + "'";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='set_review_month' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";
                SqlConnection connect = new SqlConnection(str_connect);
                SqlCommand sql_select_user_name = new SqlCommand(string_select_user_name, connect);
                connect.Open();
                user_name = sql_select_user_name.ExecuteScalar().ToString();
                connect.Close();
                Session["user_name"] = user_name;

                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page

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

                    string get_max_data = "SELECT COUNT(Review_ID) FROM ScorecardReview";
                    string select_all_review = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY Review_Name, month_num ASC) AS rowNum, "
                                             + "* FROM ScorecardReview)sub WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 "
                                             + "AND rowNum<=" + data_per_page + "*" + page + "";
                    SqlCommand sql_select_all_review = new SqlCommand(select_all_review, conn);
                    SqlCommand sql_get_max_data = new SqlCommand(get_max_data, conn);
                    max_select_data = (int)sql_get_max_data.ExecuteScalar();//untuk mengetahui banyaknya page pada pagination
                    max_page = Math.Ceiling(max_select_data / data_per_page);//mendapatkan nilai banyaknya jumlah page
                    using (SqlDataReader ReviewReader = sql_select_all_review.ExecuteReader())
                    {
                        if (ReviewReader.HasRows)
                        {
                            HtmlPeriodTable.Append("<tr>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>No.</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Review Type</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Review Month</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Review Status</th>");
                            HtmlPeriodTable.Append("<th class='centering-th-period'>Action</th>");
                            HtmlPeriodTable.Append("</tr>");
                            while (ReviewReader.Read())
                            {
                                HtmlPeriodTable.Append("<tr align='center'>");
                                HtmlPeriodTable.Append("<td class='td-align'>" + no_header + "</td>");
                                if (ReviewReader["Review_Name"].ToString() == "Quarterly" && loop_quarterly==0)
                                {
                                    HtmlPeriodTable.Append("<td class='td-align' rowspan='4'>" + ReviewReader["Review_Name"] + "</td>");
                                    loop_quarterly=1;
                                }
                                else if (ReviewReader["Review_Name"].ToString() == "Semesterly" && loop_semesterly == 0)
                                {
                                    HtmlPeriodTable.Append("<td class='td-align' rowspan='2'>" + ReviewReader["Review_Name"] + "</td>");
                                    loop_semesterly = 1;
                                }
                                else if (ReviewReader["Review_Name"].ToString() == "Yearly")
                                {
                                    HtmlPeriodTable.Append("<td class='td-align'>" + ReviewReader["Review_Name"] + "</td>");
                                }

                                HtmlPeriodTable.Append("<td class='td-align'>" + ReviewReader["Review_Month"] + "</td>");

                                if (ReviewReader["Review_Status"].ToString() == "Active")
                                {
                                    HtmlPeriodTable.Append("<td class='td-align active-period'>" + ReviewReader["Review_Status"] + "</td>");
                                }
                                else
                                {
                                    HtmlPeriodTable.Append("<td class='td-align inactive-period'>" + ReviewReader["Review_Status"] + "</td>");
                                }

                                HtmlPeriodTable.Append("<td class='td-align'><a href='edit_review.aspx?page=" + page + "&review_id=" + ReviewReader["Review_ID"] + "' class='btn btn-default'>Edit</a></td>");
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
                PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlPeriodTable.ToString() });//menampilkan table period

                //Code untuk Pagination

                if (max_page > 1)
                {
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
                }

            }//end of if(!IsPostBack)
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