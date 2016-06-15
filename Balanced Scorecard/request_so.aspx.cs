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
    public partial class request_so : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (!IsPostBack)
            {
                string user_nik = "", user_name;
                if ((string)Session["user_nik"] == null)
                {
                    Response.Redirect("" + baseUrl + "index.aspx");
                }
                else
                {
                    user_nik = (string)Session["user_nik"];
                }
                string string_select_user_name = "SELECT empName FROM ScorecardUser WHERE EmpId='" + user_nik + "'", sql_string_active = "";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='request_so' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";
                SqlConnection connect = new SqlConnection(str_connect);
                SqlCommand sql_select_user_name = new SqlCommand(string_select_user_name, connect);
                SqlCommand sql_check_access_page = new SqlCommand(string_check_access_page, connect);
                SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, connect);
                connect.Open();
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
                user_name = sql_select_user_name.ExecuteScalar().ToString();
                connect.Close();
                Session["user_name"] = user_name;

                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page

                StringBuilder HtmlApprovalTable = new StringBuilder();
                StringBuilder Pagination = new StringBuilder();//untuk Pagination
                StringBuilder HtmlDropdown = new StringBuilder();

                var paging = Request.QueryString["page"];//untuk pagination
                var period_id = Request.QueryString["period_id"];

                int page = 0;
                decimal no_header = 0;//inisialisasi
                decimal data_per_page = 10, max_select_data = 0, max_page = 0;//untuk pagination

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    if (period_id == null)
                    {
                        sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                        SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                        Object output_period_status = sql_active.ExecuteScalar();
                        if (output_period_status == null)
                        {
                            sql_string_active = "SELECT TOP(1) * FROM BSC_Period WHERE data_status='exist'";//langsung cari yang id-nya 1
                            SqlCommand sql_select_active_period_id = new SqlCommand(sql_string_active, conn);
                            using (SqlDataReader PeriodIDReader = sql_select_active_period_id.ExecuteReader())
                            {
                                if (PeriodIDReader.HasRows)
                                {
                                    while (PeriodIDReader.Read())
                                    {
                                        period_id = PeriodIDReader["Period_ID"].ToString();//harus string untuk ke object
                                    }
                                }
                                else
                                {
                                    period_id = "0";
                                }
                            }
                        }
                        else
                        {
                            string select_active_period_id = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
                            SqlCommand sql_select_active_period_id = new SqlCommand(select_active_period_id, conn);
                            using (SqlDataReader PeriodIDReader = sql_select_active_period_id.ExecuteReader())
                            {
                                while (PeriodIDReader.Read())
                                {
                                    period_id = PeriodIDReader["Period_ID"].ToString();//harus string untuk ke object
                                }
                            }
                        }
                    }
                    else
                    {
                        sql_string_active = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                    }
                    string sql_all_period = "SELECT * FROM BSC_Period WHERE data_status='exist' ORDER BY Start_Period ASC";
                    SqlCommand sql_command = new SqlCommand(sql_string_active, conn);
                    SqlCommand sql_command_all = new SqlCommand(sql_all_period, conn);
                    using (SqlDataReader ActivePeriodReader = sql_command.ExecuteReader())//UNTUK VIEW YANG STATUS = ACTIVE
                    {
                        if (ActivePeriodReader.HasRows)
                        {
                            while (ActivePeriodReader.Read())
                            {
                                string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya ga keluar!!
                                DateTime start_date = Convert.ToDateTime(ActivePeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(ActivePeriodReader["End_Period"]);
                                startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                                enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                                start_end_date = startdate_to_date + " - " + enddate_to_date;
                                HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                HtmlDropdown.Append(start_end_date + "&nbsp;<span class='caret'></span>");
                                HtmlDropdown.Append("</button>");
                            }
                        }
                        else
                        {
                            HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                            HtmlDropdown.Append("Period Not Set &nbsp;<span class='caret'></span>");
                            HtmlDropdown.Append("</button>");
                        }
                    }

                    using (SqlDataReader PeriodReader = sql_command_all.ExecuteReader())//UNTUK VIEW SEMUA PERIODE YANG ADA
                    {
                        HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                        if (PeriodReader.HasRows)
                        {
                            while (PeriodReader.Read())
                            {
                                string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya ga keluar!!
                                DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                                enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                                start_end_date = startdate_to_date + " - " + enddate_to_date;
                                HtmlDropdown.Append("<li role='presentation'><a role='menuitem' href='request_so.aspx?period_id=" + PeriodReader["Period_ID"] + "'>");
                                HtmlDropdown.Append(start_end_date + "</a></li>");
                            }
                        }
                        else
                        {
                            HtmlDropdown.Append("<li role='presentation'>No Period</li>");
                        }
                        HtmlDropdown.Append("</ul>");
                    }
                    PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlDropdown.ToString() });//untuk DropDown

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

                    string get_max_data = "SELECT COUNT(IndividualDetailRequest_ID) FROM IndividualDetail_RequestChange WHERE Period_ID=" + period_id + " AND user_id=" + Session["user_id"] + "";
                    string select_all_detail_request = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY IndividualDetail_RequestChange.IndividualDetailRequest_ID DESC) "
                                                     + "AS rowNum, IndividualDetailHistory.IndividualDetail_Title, IndividualDetail_RequestChange.date_create, "
                                                     + "IndividualDetail_RequestChange.IndividualDetailRequest_ID, IndividualDetailHistory.IndividualHeader_KPI, Approval_Status, IndividualDetail_RequestChange.user_update, "
                                                     + "IndividualDetail_RequestChange.date_update, IndividualDetail_RequestChange.delete_flag "
                                                     + "FROM IndividualDetail_RequestChange "
                                                     + "join IndividualDetailHistory ON IndividualDetailHistory.IndividualDetailRequest_ID = IndividualDetail_RequestChange.IndividualDetailRequest_ID "
                                                     + "WHERE IndividualDetail_RequestChange.Period_ID="+period_id+" "
                                                     + "AND IndividualDetail_RequestChange.user_id=" + Session["user_id"] + ")sub "
                                                     + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                    SqlCommand sql_select_all_detail_request = new SqlCommand(select_all_detail_request, conn);
                    SqlCommand sql_get_max_data = new SqlCommand(get_max_data, conn);
                    max_select_data = (int)sql_get_max_data.ExecuteScalar();//untuk mengetahui banyaknya page pada pagination
                    max_page = Math.Ceiling(max_select_data / data_per_page);//mendapatkan nilai banyaknya jumlah page
                    using (SqlDataReader RequestReader = sql_select_all_detail_request.ExecuteReader())
                    {
                        if (RequestReader.HasRows)
                        {
                            HtmlApprovalTable.Append("<tr>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>No.</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Spec. Obj. Requested</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Spec. Objective's KPI</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Request For</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Details</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Request Date</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Responder Name</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Response Date</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Status</th>");
                            HtmlApprovalTable.Append("</tr>");
                            while (RequestReader.Read())
                            {
                                string request_date, approve_date, app_name;
                                DateTime req_date = Convert.ToDateTime(RequestReader["date_create"]);
                                request_date = req_date.ToString("MM-dd-yyyy");

                                if (RequestReader["date_update"] == System.DBNull.Value)
                                {
                                    approve_date = " - ";
                                }
                                else
                                {
                                    DateTime app_date = Convert.ToDateTime(RequestReader["date_update"]);
                                    approve_date = app_date.ToString("MM-dd-yyyy");
                                }

                                if (RequestReader["user_update"] == System.DBNull.Value)
                                {
                                    app_name = "-";
                                }
                                else
                                {
                                    app_name = RequestReader["user_update"].ToString();
                                }

                                HtmlApprovalTable.Append("<tr align='center'>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + no_header + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + RequestReader["IndividualDetail_Title"] + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + RequestReader["IndividualHeader_KPI"] + "</td>");

                                if (RequestReader["delete_flag"].ToString() == "0")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'>Change</td>");
                                }
                                else if (RequestReader["delete_flag"].ToString() == "1")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'>Delete</td>");
                                }
                                else if (RequestReader["delete_flag"].ToString() == "2")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'>Add</td>");
                                }

                                HtmlApprovalTable.Append("<td class='td-align'><a href='view_my_specific_objective_request.aspx?page=" + page + "&request_id=" + RequestReader["IndividualDetailRequest_ID"] + "&period_id=" + period_id + "'>See Details</a></td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + request_date + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + app_name + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + approve_date + "</td>");
                                if (RequestReader["Approval_Status"].ToString() == "pending")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align' style='background-color:yellow'>" + RequestReader["Approval_Status"] + "</td>");
                                }
                                else if (RequestReader["Approval_Status"].ToString() == "rejected")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align' style='background-color:#ff5050'>" + RequestReader["Approval_Status"] + "</td>");
                                }
                                else if (RequestReader["Approval_Status"].ToString() == "approved")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align' style='background-color:#66ff99'>" + RequestReader["Approval_Status"] + "</td>");
                                }
                                HtmlApprovalTable.Append("</tr>");
                                no_header++;
                            }//end of While
                        }//end of if(HasRows)
                        else//jika tidak ada data period sama sekali
                        {
                            HtmlApprovalTable.Append("<tr align='center'>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>There is no request to display</th>");
                            HtmlApprovalTable.Append("</tr>");
                        }
                    }//end of SqlDataReader
                    conn.Close();
                }//end of SqlConnection
                PlaceHolderApproval.Controls.Add(new Literal { Text = HtmlApprovalTable.ToString() });//menampilkan table period

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