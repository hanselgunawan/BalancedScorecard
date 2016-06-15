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
    public partial class approval : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (!IsPostBack)
            {
                string user_nik = "", user_name, get_max_data = "", select_all_header_request = "", sql_string_active = "";
                var req_nik = Request.QueryString["req_nik"];
                var req_name = Request.QueryString["req_name"];
                var req_org = Request.QueryString["req_org"];
                var req_adt_org = Request.QueryString["req_adt_org"];

                if ((string)Session["user_nik"] == null)
                {
                    
                    Response.Redirect(baseUrl + "index.aspx");
                }
                else
                {
                    user_nik = (string)Session["user_nik"];
                }

                //untuk hak akses
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='approval' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";

                string string_select_user_name = "SELECT empName FROM ScorecardUser WHERE EmpId='" + user_nik + "'";
                SqlConnection connect = new SqlConnection(str_connect);
                SqlCommand sql_select_user_name = new SqlCommand(string_select_user_name, connect);
                connect.Open();
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

                DropDownListSearchBy.Items.Add("Search By NIK");
                DropDownListSearchBy.Items.Add("Search By Name");
                DropDownListSearchBy.Items.Add("Search By Organization");
                DropDownListSearchBy.Items.Add("Search By Adt. Group");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();

                    SqlCommand sql_check_access_page = new SqlCommand(string_check_access_page, conn);
                    SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);

                    using (SqlDataReader PageReader = sql_check_access_page.ExecuteReader())
                    {
                        if (!PageReader.HasRows)
                        {
                            Response.Redirect("" + baseUrl + "index.aspx");
                        }
                        PageReader.Dispose();
                        PageReader.Close();
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
                        AccessReader.Close();
                        AccessReader.Dispose();
                    }

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
                                    HtmlDropdown.Append("<li role='presentation'><a role='menuitem' href='approval.aspx?period_id=" + PeriodReader["Period_ID"] + "'>");
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

                    if (req_nik == null && req_name == null && req_org == null && req_adt_org == null)
                    {
                        get_max_data = "SELECT COUNT(IndividualHeaderRequest_ID) FROM IndividualHeader_RequestChange WHERE Period_ID=" + period_id + " AND Superior_ID='" + user_nik + "'";
                        select_all_header_request = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardUser.EmpId ASC, IndividualHeader_RequestChange.date_create DESC) "
                                                 + "AS rowNum, ScorecardUser.EmpId, OrgName, OrgAdtGroupName, "
                                                 + "empName, IndividualHeaderHistory.IndividualHeader_KPI, "
                                                 + "IndividualHeader_RequestChange.date_create, IndividualHeader_RequestChange.date_update, Approval_Status, IndividualHeader_RequestChange.IndividualHeaderRequest_ID, "
                                                 + "IndividualHeader_RequestChange.delete_flag FROM IndividualHeader_RequestChange "
                                                 + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID = IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                                 + "join ScorecardUser on IndividualHeader_RequestChange.user_id = ScorecardUser.user_id "
                                                 + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " "
                                                 + "AND IndividualHeader_RequestChange.Superior_ID='" + user_nik + "')sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                    }
                    else if (req_nik != null && req_name == null && req_org == null && req_adt_org == null)
                    {
                        get_max_data = "SELECT COUNT(IndividualHeaderRequest_ID) FROM IndividualHeader_RequestChange "
                                     + "join ScorecardUser ON IndividualHeader_RequestChange.user_id = ScorecardUser.user_id "
                                     + "WHERE Period_ID=" + period_id + " AND "
                                     + "IndividualHeader_RequestChange.Superior_ID='" + user_nik + "' AND ScorecardUser.EmpId LIKE '" + req_nik + "%'";
                        select_all_header_request = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardUser.EmpId ASC, IndividualHeader_RequestChange.date_create DESC) "
                                                 + "AS rowNum, ScorecardUser.EmpId, OrgName, OrgAdtGroupName, "
                                                 + "empName, IndividualHeaderHistory.IndividualHeader_KPI, "
                                                 + "IndividualHeader_RequestChange.date_create, IndividualHeader_RequestChange.date_update, Approval_Status, IndividualHeader_RequestChange.IndividualHeaderRequest_ID, "
                                                 + "IndividualHeader_RequestChange.delete_flag FROM IndividualHeader_RequestChange "
                                                 + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID = IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                                 + "join ScorecardUser on IndividualHeader_RequestChange.user_id = ScorecardUser.user_id "
                                                 + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " "
                                                 + "AND IndividualHeader_RequestChange.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.EmpId LIKE '" + req_nik + "%')sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        DropDownListSearchBy.SelectedIndex = 0;
                        TextBoxSearch.Value = req_nik;
                    }
                    else if (req_nik == null && req_name != null && req_org == null && req_adt_org == null)
                    {
                        get_max_data = "SELECT COUNT(IndividualHeaderRequest_ID) FROM IndividualHeader_RequestChange "
                                     + "join ScorecardUser ON ScorecardUser.user_id = IndividualHeader_RequestChange.user_id "
                                     + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " AND IndividualHeader_RequestChange.Superior_ID='" + Session["user_nik"].ToString() + "' "
                                     + "AND ScorecardUser.empName LIKE '" + req_name + "%'";
                        select_all_header_request = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardUser.EmpId ASC, IndividualHeader_RequestChange.date_create DESC) "
                                                 + "AS rowNum, ScorecardUser.EmpId, OrgName, OrgAdtGroupName, "
                                                 + "empName, IndividualHeaderHistory.IndividualHeader_KPI, "
                                                 + "IndividualHeader_RequestChange.date_create, IndividualHeader_RequestChange.date_update, Approval_Status, IndividualHeader_RequestChange.IndividualHeaderRequest_ID, "
                                                 + "IndividualHeader_RequestChange.delete_flag FROM IndividualHeader_RequestChange "
                                                 + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID = IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                                 + "join ScorecardUser on IndividualHeader_RequestChange.user_id = ScorecardUser.user_id "
                                                 + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " "
                                                 + "AND IndividualHeader_RequestChange.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.empName LIKE '" + req_name + "%')sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        DropDownListSearchBy.SelectedIndex = 1;
                        TextBoxSearch.Value = req_name;
                    }
                    else if (req_nik == null && req_name == null && req_org != null && req_adt_org == null)
                    {
                        get_max_data = "SELECT COUNT(IndividualHeaderRequest_ID) FROM IndividualHeader_RequestChange "
                                     + "join ScorecardUser ON ScorecardUser.user_id = IndividualHeader_RequestChange.user_id "
                                     + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                     + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " AND IndividualHeader_RequestChange.Superior_ID='" + Session["user_nik"].ToString() + "' "
                                     + "AND OrgName LIKE '" + req_org + "%'";
                        select_all_header_request = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardUser.EmpId ASC, IndividualHeader_RequestChange.date_create DESC) "
                                                 + "AS rowNum, ScorecardUser.EmpId, OrgName, OrgAdtGroupName, "
                                                 + "empName, IndividualHeaderHistory.IndividualHeader_KPI, "
                                                 + "IndividualHeader_RequestChange.date_create, IndividualHeader_RequestChange.date_update, Approval_Status, IndividualHeader_RequestChange.IndividualHeaderRequest_ID, "
                                                 + "IndividualHeader_RequestChange.delete_flag FROM IndividualHeader_RequestChange "
                                                 + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID = IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                                 + "join ScorecardUser on IndividualHeader_RequestChange.user_id = ScorecardUser.user_id "
                                                 + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " "
                                                 + "AND IndividualHeader_RequestChange.Superior_ID='" + Session["user_nik"].ToString() + "' AND OrgName LIKE '" + req_org + "%')sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        DropDownListSearchBy.SelectedIndex = 2;
                        TextBoxSearch.Value = req_org;
                    }
                    else if (req_nik == null && req_name == null && req_org == null && req_adt_org != null)
                    {
                        get_max_data = "SELECT COUNT(IndividualHeaderRequest_ID) FROM IndividualHeader_RequestChange "
                                     + "join ScorecardUser ON ScorecardUser.user_id = IndividualHeader_RequestChange.user_id "
                                     + "join ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode = ScorecardGroupLink.OrgAdtGroupCode "
                                     + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " AND IndividualHeader_RequestChange.Superior_ID='" + Session["user_nik"].ToString() + "' "
                                     + "AND OrgAdtGroupName LIKE '" + req_adt_org + "%'";
                        select_all_header_request = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardUser.EmpId ASC, IndividualHeader_RequestChange.date_create DESC) "
                                                 + "AS rowNum, ScorecardUser.EmpId, OrgName, OrgAdtGroupName, "
                                                 + "empName, IndividualHeaderHistory.IndividualHeader_KPI, "
                                                 + "IndividualHeader_RequestChange.date_create, IndividualHeader_RequestChange.date_update, Approval_Status, IndividualHeader_RequestChange.IndividualHeaderRequest_ID, "
                                                 + "IndividualHeader_RequestChange.delete_flag FROM IndividualHeader_RequestChange "
                                                 + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID = IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                                 + "join ScorecardUser on IndividualHeader_RequestChange.user_id = ScorecardUser.user_id "
                                                 + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE IndividualHeader_RequestChange.Period_ID=" + period_id + " "
                                                 + "AND IndividualHeader_RequestChange.Superior_ID='" + Session["user_nik"].ToString() + "' AND OrgAdtGroupName LIKE '" + req_adt_org + "%')sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        DropDownListSearchBy.SelectedIndex = 3;
                        TextBoxSearch.Value = req_adt_org;
                    }
                    SqlCommand sql_select_all_header_request = new SqlCommand(select_all_header_request, conn);
                    SqlCommand sql_get_max_data = new SqlCommand(get_max_data, conn);
                    max_select_data = (int)sql_get_max_data.ExecuteScalar();//untuk mengetahui banyaknya page pada pagination
                    max_page = Math.Ceiling(max_select_data / data_per_page);//mendapatkan nilai banyaknya jumlah page
                    using (SqlDataReader RequestReader = sql_select_all_header_request.ExecuteReader())
                    {
                        if (RequestReader.HasRows)
                        {
                            HtmlApprovalTable.Append("<tr>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>No.</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>NIK / <i>Barcode</i></th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Name</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Organization</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Adt. Group</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>KPI Requested</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Request For</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Details</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Request Date</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Response Date</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Status</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Approve</th>");
                            HtmlApprovalTable.Append("<th class='centering-th-period'>Reject</th>");
                            HtmlApprovalTable.Append("</tr>");
                            while (RequestReader.Read())
                            {
                                string request_date, approve_date;
                                DateTime req_date = Convert.ToDateTime(RequestReader["date_create"]);
                                request_date = req_date.ToString("MM-dd-yyyy");

                                if (RequestReader["date_update"] == System.DBNull.Value)
                                {
                                    approve_date = "-";
                                }
                                else
                                {
                                    DateTime app_date = Convert.ToDateTime(RequestReader["date_update"]);
                                    approve_date = app_date.ToString("MM-dd-yyyy");
                                }

                                HtmlApprovalTable.Append("<tr align='center'>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + no_header + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + RequestReader["EmpId"] + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + RequestReader["empName"] + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + RequestReader["OrgName"] + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + RequestReader["OrgAdtGroupName"] + "</td>");
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

                                if (req_nik == null && req_name == null && req_org == null && req_adt_org == null)
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'><a href='view_request_kpi.aspx?page=" + page + "&request_id=" + RequestReader["IndividualHeaderRequest_ID"] + "&period_id=" + period_id + "'>See Details</a></td>");
                                }
                                else if (req_nik != null && req_name == null && req_org == null && req_adt_org == null)
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'><a href='view_request_kpi.aspx?page=" + page + "&request_id=" + RequestReader["IndividualHeaderRequest_ID"] + "&period_id=" + period_id + "&req_nik=" + req_nik + "'>See Details</a></td>");
                                }
                                else if (req_nik == null && req_name != null && req_org == null && req_adt_org == null)
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'><a href='view_request_kpi.aspx?page=" + page + "&request_id=" + RequestReader["IndividualHeaderRequest_ID"] + "&period_id=" + period_id + "&req_name=" + req_name + "'>See Details</a></td>");
                                }
                                else if (req_nik == null && req_name == null && req_org != null && req_adt_org == null)
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'><a href='view_request_kpi.aspx?page=" + page + "&request_id=" + RequestReader["IndividualHeaderRequest_ID"] + "&period_id=" + period_id + "&req_org=" + req_org + "'>See Details</a></td>");
                                }
                                else if (req_nik == null && req_name == null && req_org == null && req_adt_org != null)
                                {
                                    HtmlApprovalTable.Append("<td class='td-align'><a href='view_request_kpi.aspx?page=" + page + "&request_id=" + RequestReader["IndividualHeaderRequest_ID"] + "&period_id=" + period_id + "&req_adt_org=" + req_adt_org + "'>See Details</a></td>");
                                }

                                HtmlApprovalTable.Append("<td class='td-align'>" + request_date + "</td>");
                                HtmlApprovalTable.Append("<td class='td-align'>" + approve_date + "</td>");
                                if (RequestReader["Approval_Status"].ToString() == "pending")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align' style='background-color:yellow'>" + RequestReader["Approval_Status"] + "</td>");
                                    HtmlApprovalTable.Append("<td class='td-align'><a class='btn btn-default' href='approve_kpi.aspx?page=" + page + "&request_id=" + RequestReader["IndividualHeaderRequest_ID"] + "&period_id=" + period_id + "'>Approve</a></td>");
                                    HtmlApprovalTable.Append("<td class='td-align'><a class='btn btn-default' href='reject_kpi.aspx?page=" + page + "&request_id=" + RequestReader["IndividualHeaderRequest_ID"] + "&period_id=" + period_id + "'>Reject</a></td>");
                                }
                                else if (RequestReader["Approval_Status"].ToString() == "rejected")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align' style='background-color:#ff5050'>" + RequestReader["Approval_Status"] + "</td>");
                                    HtmlApprovalTable.Append("<td class='td-align'><a class='btn btn-default disabled' href='#'>Approve</a></td>");
                                    HtmlApprovalTable.Append("<td class='td-align'><a class='btn btn-default disabled' href='#'>Reject</a></td>");
                                }
                                else if (RequestReader["Approval_Status"].ToString() == "approved")
                                {
                                    HtmlApprovalTable.Append("<td class='td-align' style='background-color:#66ff99'>" + RequestReader["Approval_Status"] + "</td>");
                                    HtmlApprovalTable.Append("<td class='td-align'><a class='btn btn-default disabled' href='#'>Approve</a></td>");
                                    HtmlApprovalTable.Append("<td class='td-align'><a class='btn btn-default disabled' href='#'>Reject</a></td>");
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
                if (req_nik == null && req_name == null && req_org == null && req_adt_org == null)
                {
                    Pagination.Append("href: '?page={{number}}&period_id=" + period_id + "'");
                }
                else if (req_nik != null && req_name == null && req_org == null && req_adt_org == null)
                {
                    Pagination.Append("href: '?page={{number}}&req_nik=" + req_nik + "&period_id=" + period_id + "'");
                }
                else if (req_nik == null && req_name != null && req_org == null && req_adt_org == null)
                {
                    Pagination.Append("href: '?page={{number}}&req_name=" + req_name + "&period_id=" + period_id + "'");
                }
                else if (req_nik == null && req_name == null && req_org != null && req_adt_org == null)
                {
                    Pagination.Append("href: '?page={{number}}&req_org=" + req_org + "&period_id=" + period_id + "'");
                }
                else if (req_nik == null && req_name == null && req_org == null && req_adt_org != null)
                {
                    Pagination.Append("href: '?page={{number}}&req_adt_org=" + req_adt_org + "&period_id=" + period_id + "'");
                }

                Pagination.Append("});");
                Pagination.Append("</script>");
                PlaceHolderPaging.Controls.Add(new Literal { Text = Pagination.ToString() });//untuk Pagination
            }
        }

        protected void OnClickSearch(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var period_id = Request.QueryString["period_id"];

            if (period_id == null)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    string sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                    SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                    Object output_period_status = sql_active.ExecuteScalar();
                    if (output_period_status == null)//jika tidak ada yang Active
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
                    conn.Close();
                }
            }

            

            if (TextBoxSearch.Value == "")
            {
                Response.Redirect(baseUrl + "approval.aspx?period_id=" + period_id + "");
            }
            else
            {
                if (DropDownListSearchBy.SelectedIndex == 0)
                {
                    Response.Redirect(baseUrl + "approval.aspx?req_nik=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 1)
                {
                    Response.Redirect(baseUrl + "approval.aspx?req_name=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 2)
                {
                    Response.Redirect(baseUrl + "approval.aspx?req_org=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 3)
                {
                    Response.Redirect(baseUrl + "approval.aspx?req_adt_org=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
            }
        }

        protected void OnClickExportExcel(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
            int no_header = 1;
            string start_date_formatted = "", end_date_formatted = "", current_month = "";

            current_month = DateTime.Now.ToString("MMM");

            if (period_id == null)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    string sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                    SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                    Object output_period_status = sql_active.ExecuteScalar();
                    if (output_period_status == null)//jika tidak ada yang Active
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
                    conn.Close();
                }
            }

            string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
            using (SqlConnection conn_period = new SqlConnection(str_connect))
            {
                conn_period.Open();
                SqlCommand sql_select_period = new SqlCommand(string_select_period, conn_period);
                using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                {
                    while (PeriodReader.Read())
                    {
                        DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                        DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                        start_date_formatted = start_date.ToString("MMM");
                        end_date_formatted = end_date.ToString("yyyy");
                    }
                    PeriodReader.Dispose();
                    PeriodReader.Close();
                }
                conn_period.Close();
            }

            if (start_date_formatted == "")
            {
                current_month = "";
            }
            else
            {
                if (DateTime.Now.ToString("yyyy") != end_date_formatted)
                {
                    current_month = "Dec";
                }
                else
                {
                    current_month = DateTime.Now.ToString("MMM");
                }
            }

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.ContentType = "application/xls";
            HttpContext.Current.Response.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");

            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            //sets font
            HttpContext.Current.Response.Write("<font style='font-size:12pt; font-family:Times New Roman;'>");
            HttpContext.Current.Response.Write("<BR><BR><BR>");
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            HttpContext.Current.Response.Write("<Table border='1' bgColor='#ffffff' " +
              "borderColor='#000000' cellSpacing='0' cellPadding='0' " +
              "style='font-size:12.0pt; font-family:'Times New Roman'; background:white;'>");

            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='18' align='center' style='font-size:20pt'><b>KPI CHANGE REQUESTS</b></td>");
            HttpContext.Current.Response.Write("</tr>");
            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='18' align='center' style='font-size:16pt'><b>Period: " + start_date_formatted + " - " + current_month + " " + end_date_formatted + "</b></td>");
            HttpContext.Current.Response.Write("</tr>");
            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='18'></td>");
            HttpContext.Current.Response.Write("</tr>");

            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + "KPI CHANGE REQUESTS (" + start_date_formatted + " - " + current_month + " " + end_date_formatted + ").xls\"");

            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td align='center'><b>No.</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>NIK</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Name</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Organization</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Additional Group</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Job Title</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Job Level</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Grade</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Group</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Previous KPI</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>New KPI</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Previous Target</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>New Target</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Responder NIK</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Responder Name</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Request Date</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Response Date</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Status</b></td>");
            HttpContext.Current.Response.Write("</tr>");

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string string_select_KPI_change_request = "SELECT t1.EmpId, t1.empName, t1.OrgName, "
                                                        + "t1.JobTtlName, t1.JobLvlName, t1.OrgAdtGroupName, t1.Group_Name, t1.Old_KPI, "
                                                        + "t1.New_KPI, t1.Old_Target, t1.New_Target, t1.Old_MeasureBy, t1.empGrade, "
                                                        + "t1.New_MeasureBy, t1.Approval_Status, t1.Request_Date, t1.Approved_Date, t2.Superior_NIK, t2.Superior_Name, t1.delete_flag "
                                                        + "FROM "
                                                        + "( "
                                                            + "SELECT ScorecardUser.EmpId, ScorecardUser.empName, OrgName, "
                                                            + "JobTtlName, JobLvlName, empGrade, OrgAdtGroupName, Group_Name, IndividualHeaderHistory.IndividualHeader_KPI Old_KPI, "
                                                            + "IndividualHeader_RequestChange.IndividualHeader_KPI New_KPI, IndividualHeaderHistory.IndividualHeader_Target Old_Target, "
                                                            + "IndividualHeader_RequestChange.IndividualHeader_Target New_Target, IndividualHeaderHistory.IndividualHeader_MeasureBy Old_MeasureBy, "
                                                            + "IndividualHeader_RequestChange.IndividualHeader_MeasureBy New_MeasureBy, IndividualHeader_RequestChange.date_create Request_Date, "
                                                            + "IndividualHeader_RequestChange.date_update Approved_Date, IndividualHeader_RequestChange.Approval_Status, IndividualHeader_RequestChange.delete_flag "
                                                            + "FROM ScorecardUser "
                                                            + "join ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode = ScorecardGroupLink.OrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                            + "join IndividualHeader_RequestChange ON IndividualHeader_RequestChange.user_id = ScorecardUser.user_id AND IndividualHeader_RequestChange.Period_ID=" + period_id + " "
                                                            + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID = IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                                            + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                            + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                            + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                                        + ") t1 "
                                                        + "LEFT JOIN "
                                                        + "( "
                                                            + "SELECT DISTINCT ScorecardUser.EmpId Superior_NIK, ScorecardUser.empName Superior_Name, "
                                                            + "IndividualHeaderHistory.IndividualHeader_KPI Old_KPI "
                                                            + "FROM ScorecardUser "
                                                            + "join IndividualHeader_RequestChange ON IndividualHeader_RequestChange.Superior_ID = ScorecardUser.EmpId AND IndividualHeader_RequestChange.Period_ID=" + period_id + " "
                                                            + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID = IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                                        + ") t2 "
                                                        + "ON t1.Old_KPI = t2.Old_KPI "
                                                        + "WHERE t2.Superior_NIK = '" + Session["user_nik"].ToString() + "' "
                                                        + "ORDER BY CAST(t1.EmpId AS int) ASC, t1.Request_Date DESC";
                conn.Open();
                SqlCommand sql_select_KPI_change_request = new SqlCommand(string_select_KPI_change_request, conn);
                using (SqlDataReader UserReader = sql_select_KPI_change_request.ExecuteReader())
                {
                    if (UserReader.HasRows)
                    {
                        while (UserReader.Read())
                        {
                            string request_date, approve_date;

                            DateTime request = Convert.ToDateTime(UserReader["Request_Date"]);
                            request_date = request.ToString("MM-dd-yyyy");

                            if (UserReader["Approved_Date"] == System.DBNull.Value)
                            {
                                approve_date = "-";
                            }
                            else
                            {
                                DateTime approve = Convert.ToDateTime(UserReader["Approved_Date"]);
                                approve_date = request.ToString("MM-dd-yyyy");
                            }

                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td align='center'>" + no_header + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["EmpId"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["empName"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["OrgName"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["OrgAdtGroupName"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["JobTtlName"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["JobLvlName"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["empGrade"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Group_Name"] + "</td>");
                            if (UserReader["delete_flag"].ToString() == "2")
                            {
                                HttpContext.Current.Response.Write("<td align='center'>-</td>");
                            }
                            else
                            {
                                HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Old_KPI"] + "</td>");
                            }

                            if (UserReader["delete_flag"].ToString() == "0" || UserReader["delete_flag"].ToString() == "2")
                            {
                                HttpContext.Current.Response.Write("<td align='center'>" + UserReader["New_KPI"] + "</td>");
                            }
                            else
                            {
                                HttpContext.Current.Response.Write("<td align='center'>-</td>");
                            }

                            if (UserReader["delete_flag"].ToString() == "2")
                            {
                                HttpContext.Current.Response.Write("<td align='center'>-</td>");
                            }
                            else
                            {
                                if (UserReader["Old_MeasureBy"].ToString() == "Month")
                                {
                                    string old_target_month_name;
                                    old_target_month_name = GetMonthName(int.Parse(UserReader["Old_Target"].ToString()));
                                    HttpContext.Current.Response.Write("<td align='center'>" + old_target_month_name + "</td>");
                                }
                                else if (UserReader["Old_MeasureBy"].ToString() == "Numbers")
                                {
                                    HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Old_Target"] + "</td>");
                                }
                                else if (UserReader["Old_MeasureBy"].ToString() == "-")
                                {
                                    HttpContext.Current.Response.Write("<td align='center'>Based On Schedule</td>");
                                }
                                else
                                {
                                    HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Old_Target"] + " " + UserReader["Old_MeasureBy"] + "</td>");
                                }
                            }

                            if (UserReader["delete_flag"].ToString() == "0" || UserReader["delete_flag"].ToString() == "2")
                            {
                                if (UserReader["New_MeasureBy"].ToString() == "Month")
                                {
                                    string new_target_month_name;
                                    new_target_month_name = GetMonthName(int.Parse(UserReader["New_Target"].ToString()));
                                    HttpContext.Current.Response.Write("<td align='center'>" + new_target_month_name + "</td>");
                                }
                                else if (UserReader["New_MeasureBy"].ToString() == "Numbers")
                                {
                                    HttpContext.Current.Response.Write("<td align='center'>" + UserReader["New_Target"] + "</td>");
                                }
                                else if (UserReader["New_MeasureBy"].ToString() == "-")
                                {
                                    HttpContext.Current.Response.Write("<td align='center'>Based On Schedule</td>");
                                }
                                else
                                {
                                    HttpContext.Current.Response.Write("<td align='center'>" + UserReader["New_Target"] + " " + UserReader["New_MeasureBy"] + "</td>");
                                }
                            }
                            else
                            {
                                HttpContext.Current.Response.Write("<td align='center'>-</td>");
                            }

                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Superior_NIK"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Superior_Name"] + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + request_date + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'>" + approve_date + "</td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>" + UserReader["Approval_Status"] + "</b></td>");
                            HttpContext.Current.Response.Write("</tr>");
                            no_header++;
                        }
                    }
                    else
                    {
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td align='center' colspan='18'>No KPI Change Requests</td>");
                        HttpContext.Current.Response.Write("</tr>");
                    }
                }
                conn.Close();
            }
            HttpContext.Current.Response.Write("</Table>");
            HttpContext.Current.Response.Write("</font>");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        public string GetMonthName(int result_value)
        {
            string month_name = "";
            switch (result_value)
            {
                case 0:
                    month_name = "-"; break;
                case 1:
                    month_name = "January"; break;
                case 2:
                    month_name = "February"; break;
                case 3:
                    month_name = "March"; break;
                case 4:
                    month_name = "April"; break;
                case 5:
                    month_name = "May"; break;
                case 6:
                    month_name = "June"; break;
                case 7:
                    month_name = "July"; break;
                case 8:
                    month_name = "August"; break;
                case 9:
                    month_name = "September"; break;
                case 10:
                    month_name = "October"; break;
                case 11:
                    month_name = "November"; break;
                case 12:
                    month_name = "December"; break;
            }
            return month_name;
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