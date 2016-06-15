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
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.parser;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class individual_scorecard : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        StringBuilder HtmlDropdown = new StringBuilder();//untuk DropDown
        StringBuilder HtmlTableData = new StringBuilder();//untuk Table Data-nya
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
                var periode_id = Request.QueryString["id"];
                var paging = Request.QueryString["page"];//untuk pagination
                int page = 0;
                decimal no_header = 0;//inisialisasi
                decimal data_per_page = 25, max_select_data = 0, max_page = 0;//untuk pagination
                string sql_string_active = "";//inisialisasi
                string user_id = (string)Session["user_id"];

                int current_month = DateTime.Now.Month, current_year = DateTime.Now.Year;
                int start_date_bsc = 0, end_date_bsc = 0, minus_one_bsc = 0, year_from_startdate = 0;
                int month_num = 0;
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='individual_scorecard' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";

                if (user_id == "")
                {
                    Response.Redirect("" + baseUrl + "index.aspx");
                }

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
                    conn.Close();
                }

                if (periode_id == null)
                {
                    using (SqlConnection conn = new SqlConnection(str_connect))
                    {
                        sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                        SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                        conn.Open();
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
                                        periode_id = PeriodIDReader["Period_ID"].ToString();//harus string untuk ke object
                                    }
                                }
                                else
                                {
                                    periode_id = "0";
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
                                    periode_id = PeriodIDReader["Period_ID"].ToString();//harus string untuk ke object
                                }
                            }
                        }
                        conn.Close();
                    }
                }
                else
                {
                    sql_string_active = "SELECT * FROM BSC_Period WHERE Period_ID=" + periode_id + "";
                }

                //FOR PLACEHOLDER DropDown Period//
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    if (paging == null)//untuk pertama kali Load Page (PAGINATION)
                    {
                        page = 1;
                        no_header = (1 * data_per_page) - (data_per_page - 1);//untuk no. header kolom 1 Table jika data yang ditampilkan per halaman = 5
                    }
                    else
                    {
                        page = int.Parse(paging.ToString());
                        no_header = (page * data_per_page) - (data_per_page - 1);
                    }

                    if (periode_id == null)//untuk Link ke Add Group
                    {
                        string select_period_for_add_group = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
                        SqlCommand cmd_for_add_group = new SqlCommand(select_period_for_add_group, conn);
                        conn.Open();
                        Object link_add_group = cmd_for_add_group.ExecuteScalar();//ambil 1 value saja dari DB !
                        int period_id_for_linking = (int)link_add_group;
                        btnNewKPI.Attributes.Add("a href", "add_new_kpi.aspx?page=" + page + "&period_id=" + period_id_for_linking + "");
                        //btnDeleteKPI.Attributes.Add("a href", "delete_kpi.aspx?page=" + page + "&period_id=" + period_id_for_linking + "");
                        conn.Close();
                    }
                    else
                    {
                        btnNewKPI.Attributes.Add("a href", "add_new_kpi.aspx?page=" + page + "&period_id=" + periode_id + "");
                        //btnDeleteKPI.Attributes.Add("a href", "delete_kpi.aspx?page=" + page + "&period_id=" + periode_id + "");
                    }
                    string sql_all_period = "SELECT * FROM BSC_Period WHERE data_status='exist' ORDER BY Start_Period ASC";
                    SqlCommand sql_command = new SqlCommand(sql_string_active, conn);
                    SqlCommand sql_command_all = new SqlCommand(sql_all_period, conn);
                    conn.Open();
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
                                HtmlDropdown.Append("<li role='presentation'><a role='menuitem' href='individual_scorecard.aspx?id=" + PeriodReader["Period_ID"] + "'>");
                                HtmlDropdown.Append(start_end_date + "</a></li>");
                            }
                        }
                        else
                        {
                            HtmlDropdown.Append("<li role='presentation'>No Period</li>");
                        }
                        HtmlDropdown.Append("</ul>");
                    }
                    conn.Close();
                }
                PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlDropdown.ToString() });//untuk DropDown

                //Placeholder Table
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    SqlCommand sql_command = new SqlCommand(sql_string_active, conn);//sql_string_active mengambil Period_ID dari yang dikirim sama halaman ini
                    //jika baru pertama kali Load, sql_string_active memanggil Period_ID yang 'Active'
                    //jika periode diganti, sql_string_active memanggil Period_ID yang dikirim oleh halaman ini
                    conn.Open();

                    string string_select_individual_stretch_review = "SELECT FinancialHeader_IndividualStretchRating, FinancialHeader_Review FROM ScorecardUser "
                                                                    + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + periode_id + " "
                                                                    + "join BSC_Period ON BSC_Period.Period_ID = ScorecardGroupLink.Period_ID "
                                                                    + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                                    + "WHERE ScorecardUser.user_id=" + user_id + " AND FinancialMeasures_Header.Period_ID=" + periode_id + " AND FinancialMeasures_Header.data_status='exist'";
                    SqlCommand sql_select_individual_stretch_review = new SqlCommand(string_select_individual_stretch_review, conn);
                    using (SqlDataReader IndividualStretchRating = sql_select_individual_stretch_review.ExecuteReader())
                    {
                        if (IndividualStretchRating.HasRows)
                        {
                            while (IndividualStretchRating.Read())
                            {
                                LabelStretchRating.Text = IndividualStretchRating["FinancialHeader_IndividualStretchRating"].ToString();
                                LabelReview.Text = IndividualStretchRating["FinancialHeader_Review"].ToString();
                            }
                        }
                        else
                        {
                            LabelStretchRating.Text = "0";
                            LabelReview.Text = " - ";
                            btnNewKPI.Attributes.Add("disabled", "true");
                            //btnDeleteKPI.Attributes.Add("disabled", "true");
                        }
                    }
                    //------------------------------------------------------------//

                    using (SqlDataReader PeriodReader = sql_command.ExecuteReader())
                    {
                        if (PeriodReader.HasRows)//untuk menghindari jika Period_ID ada yang mengubah
                        {
                            while (PeriodReader.Read())
                            {
                                float total_rating = 0;
                                int no_detail = 1;
                                int collapse_jscript = 1;
                                //ADMIN -- > string get_max_data = "SELECT COUNT(IndividualHeader_ID) FROM IndividualMeasures_Header WHERE Period_ID=" + periode_id + " AND data_status='exist'";
                                //ADMIN --> string select_individual_header = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY Period_ID ASC) AS rowNum, * FROM IndividualMeasures_Header WHERE Period_ID=" + periode_id + " AND data_status='exist')sub WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";//sama seperti LIMIT pada MySQL

                                //asumsi NIK = 100
                                string get_max_data = "SELECT COUNT(IndividualHeader_ID) FROM IndividualMeasures_Header WHERE Period_ID=" + periode_id + " AND data_status='exist' AND user_id=" + user_id + "";
                                string select_individual_header = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY Period_ID ASC) AS rowNum, "
                                                                    + "* FROM IndividualMeasures_Header WHERE Period_ID=" + periode_id + " AND data_status='exist' AND user_id=" + user_id + ")sub "
                                                                    + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";//sama seperti LIMIT pada MySQL
                                SqlCommand sql_select_individual_header = new SqlCommand(select_individual_header, conn);
                                SqlCommand sql_get_max_data = new SqlCommand(get_max_data, conn);
                                max_select_data = (int)sql_get_max_data.ExecuteScalar();//untuk mengetahui banyaknya page pada pagination
                                max_page = Math.Ceiling(max_select_data / data_per_page);//mendapatkan nilai banyaknya jumlah page

                                using (SqlDataReader IndividualHeaderReader = sql_select_individual_header.ExecuteReader())
                                {
                                    if (IndividualHeaderReader.HasRows)
                                    {
                                        HtmlTableData.Append("<tr><th class='centering-th2'>No.</th><th class='centering-th2'>KPI</th><th class='centering-th2'>Target</th><th class='centering-th2'>Result</th><th class='centering-th2'>Formula</th><th class='centering-th2'>Rating</th><th class='centering-th2'>Weight(%)</th><th class='centering-th2'>Score</th><th class='centering-th2'>Objective</th><th class='centering-th2'>Action</th></tr>");
                                        while (IndividualHeaderReader.Read())
                                        {
                                            HtmlTableData.Append("<tr align='center'>");
                                            HtmlTableData.Append("<td class='td-align'>" + no_header + "</td>");
                                            HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_KPI"] + "</td>");

                                            if (int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika Targetnya = 'Based on Schedule'
                                            {
                                                HtmlTableData.Append("<td class='td-align'>Based On Schedule</td>");
                                                HtmlTableData.Append("<td class='td-align'> - </td>");
                                            }
                                            else//jika Targetnya bukan -1
                                            {
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    string month_name_target = "", month_name_result = "";
                                                    int month_num_target = int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString());
                                                    int month_num_result = int.Parse(IndividualHeaderReader["IndividualHeader_Result"].ToString());
                                                    month_name_target = ShowMonthNameTarget(month_num_target);
                                                    month_name_result = ShowMonthNameResult(month_num_result);
                                                    HtmlTableData.Append("<td class='td-align'>" + month_name_target + "</td>");
                                                    HtmlTableData.Append("<td class='td-align'>" + month_name_result + "</td>");
                                                }
                                                else
                                                {
                                                    if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                                                    {
                                                        HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Target"] + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Result"] + "</td>");
                                                    }
                                                    else
                                                    {
                                                        HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Target"] + " " + IndividualHeaderReader["IndividualHeader_MeasureBy"] + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Result"] + " " + IndividualHeaderReader["IndividualHeader_MeasureBy"] + "</td>");
                                                    }
                                                }
                                            }

                                            HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Formula"] + "</td>");
                                            HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Rating"] + "%" + "</td>");
                                            HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Weight"] + "%" + "</td>");
                                            HtmlTableData.Append("<td class='td-align'>" + IndividualHeaderReader["IndividualHeader_Score"] + "%" + "</td>");
                                            HtmlTableData.Append("<td class='td-align'><a class='collapsed' data-toggle='collapse' href='#collapse" + collapse_jscript + "' aria-expanded='false' aria-controls='collapse+" + collapse_jscript + "'>See Objective</a></td>");

                                            //CODE untuk DISABLE yang INACTIVE
                                            string select_status_period = "";//inisialisasi
                                            bool status = true, review_status = true;
                                            if (periode_id == null)
                                            {
                                                select_status_period = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
                                                SqlCommand sql_active_status_period = new SqlCommand(select_status_period, conn);
                                                Object output_period_status2 = sql_active_status_period.ExecuteScalar();
                                                if (output_period_status2 == null)//untuk ERROR HANDLING jika semuanya INACTIVE
                                                {
                                                    select_status_period = "SELECT TOP(1) * FROM BSC_Period WHERE data_status='exist'";
                                                }
                                                else
                                                {
                                                    select_status_period = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                                                    //ACTIVE butuh petik satu KARENA dia STRING!
                                                }
                                            }
                                            else
                                            {
                                                select_status_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + periode_id +"";
                                            }
                                            SqlCommand sql_select_status = new SqlCommand(select_status_period, conn);
                                            using (SqlDataReader StatusReader = sql_select_status.ExecuteReader())
                                            {
                                                while (StatusReader.Read())
                                                {
                                                    if (StatusReader["Period_Status"].Equals("Active"))//Kalau Statusnya Active harus bisa edit
                                                    {
                                                        string active_review = "";
                                                        DateTime start_date = Convert.ToDateTime(StatusReader["Start_Period"]);
                                                        DateTime end_date = Convert.ToDateTime(StatusReader["End_Period"]);
                                                        start_date_bsc = int.Parse(start_date.ToString("MM"));//bulan mulai
                                                        end_date_bsc = int.Parse(end_date.ToString("MM"));//bulan berakhir
                                                        year_from_startdate = int.Parse(start_date.ToString("yyyy"));

                                                        if (start_date_bsc == 1)
                                                        {
                                                            minus_one_bsc = 12;
                                                        }
                                                        else
                                                        {
                                                            minus_one_bsc = start_date_bsc - 1;
                                                        }

                                                        string string_select_active_month = "SELECT month_num FROM ScorecardReview WHERE "
                                                                                               +"Review_Status='Active' AND Review_Name='" + LabelReview.Text + "'";
                                                        string string_select_active_review_month = "SELECT Review_Status FROM ScorecardReview WHERE month_num=" + current_month + " AND Review_Status='Active' "
                                                                                                  + "AND Review_Name='" + LabelReview.Text + "'";
                                                        SqlCommand sql_select_active_month = new SqlCommand(string_select_active_month, conn);
                                                        using (SqlDataReader MonthReader = sql_select_active_month.ExecuteReader())
                                                        {
                                                            if (MonthReader.HasRows)
                                                            {
                                                                while (MonthReader.Read())
                                                                {
                                                                    month_num = int.Parse(MonthReader["month_num"].ToString());
                                                                }
                                                            }
                                                            MonthReader.Dispose();
                                                            MonthReader.Close();
                                                        }

                                                        SqlCommand sql_select_active_review_month = new SqlCommand(string_select_active_review_month, conn);
                                                        active_review = (string)sql_select_active_review_month.ExecuteScalar();
                                                        if (active_review == "Active")//BULAN REVIEW yang AKTIF
                                                        {
                                                            if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-10px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Add Specific Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Add Specific Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Edit KPI</a></li>");
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            }
                                                            else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)// jika mulai januari, desember sudah bisa add
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-10px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Add Specific Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Add Specific Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Edit KPI</a></li>");
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            }
                                                            else if (current_month == minus_one_bsc && minus_one_bsc != 12)// jika mulai maret, februari udah bisa
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-10px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Add Specific Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Add Specific Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Edit KPI</a></li>");
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            }
                                                            /*else if (current_month == minus_one_bsc && current_year != year_from_startdate - 1)//jika mulai februari, januari sudah bisa add
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-10px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Add Specific Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Add Specific Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Edit KPI</a></li>");
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            }*/
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                            {//jika mulai januari, selesai oktober, review desember. ga bisa review di desember
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                //HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Change/Delete KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {*/
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");
                                                                //}

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num > end_date_bsc)
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num < start_date_bsc)//jika mulai maret, review januari. Januari ga bisa review
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                //HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Change/Delete KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {*/
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");
                                                                //}

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            }
                                                            else if (current_month == month_num && current_year != year_from_startdate)// jika review aktif, tapi sudah lewat tahun
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                //HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Change/Delete KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {*/
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");
                                                                //}

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            }
                                                        }
                                                        else//Jika BULAN REVIEW TIDAK AKTIF
                                                        {
                                                            if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-10px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Add Specific Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Add Specific Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Edit KPI</a></li>");
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                                review_status = false;
                                                            }
                                                            else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)//jika mulai januari, desember udah bisa 
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width disabled' role='menu' style='min-width:100px; margin-left:-10px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Add Specific Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Add Specific Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Edit KPI</a></li>");
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                                review_status = false;
                                                            }
                                                            else if (current_month == minus_one_bsc && minus_one_bsc != 12)// jika mulai maret, februari udah bisa
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-10px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Add Specific Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Add Specific Obj.</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Edit KPI</a></li>");
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                                review_status = false;
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                //HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Change/Delete KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {*/
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");
                                                                //}

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                                review_status = false;
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num > end_date_bsc)
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                review_status = false;
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num < start_date_bsc)//jika januari aktif, tapi bulan mulai maret, ga bisa review di januari
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                //HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Change/Delete KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {*/
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");
                                                                //}

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                                review_status = false;
                                                            }
                                                            else if (current_month == month_num && current_year != year_from_startdate)// jika review aktif, tapi sudah lewat tahun
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                                }

                                                                //HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {

                                                                    HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                                }*/

                                                                /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                                {
                                                                    HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Change/Delete KPI</a></li>");
                                                                }
                                                                else//jika tidak ada Specific Objective
                                                                {*/
                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");
                                                                //}

                                                                HtmlTableData.Append("</ul>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                                review_status = false;
                                                            }
                                                            else
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                                HtmlTableData.Append("<td class='td-align'>");
                                                                HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                                HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                                HtmlTableData.Append("</div>");
                                                                HtmlTableData.Append("</td>");
                                                                HtmlTableData.Append("</tr>");
                                                                btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                // btnDeleteKPI.Attributes.Add("class", "btn btn-default disabled");
                                                                review_status = false;
                                                            }
                                                        }
                                                    }
                                                    else//Kalau Status PERIODE-nya ga Active hanya bisa VIEW (bukan BULAN REVIEW)
                                                    {
                                                            if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                            }
                                                            else if (current_month == minus_one_bsc && minus_one_bsc != 12)
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                            }
                                                            else if (current_month == minus_one_bsc && current_year != year_from_startdate - 1)
                                                            {
                                                                LabelAddRequest.Text = "Add New KPI";
                                                            }
                                                            else if (current_month == month_num)
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                            }
                                                            else
                                                            {
                                                                LabelAddRequest.Text = "Request New KPI";
                                                            }
                                                            HtmlTableData.Append("<td class='td-align'>");
                                                            HtmlTableData.Append("<div class='btn-group' role='group'>");
                                                            HtmlTableData.Append("<button class='btn btn-default dropdown-toggle disabled' type='button' data-toggle='dropdown' aria-expanded='false'>Choose Action &nbsp;<span class='caret'></span></button>");
                                                            HtmlTableData.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu' style='min-width:100px; margin-left:-80px'>");

                                                            if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                            {
                                                                HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='add_specific_objective.aspx?page=" + page + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'>Request Add Spec. Obj.</a></li>");
                                                                HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Update KPI</a></li>");
                                                            }
                                                            else//jika tidak ada Specific Objective
                                                            {
                                                                HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Add Spec. Obj.</a></li>");
                                                                HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='edit_individual_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Update KPI</a></li>");
                                                            }

                                                            //HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_single_kpi.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "' onclick=\"return confirm('Are you sure you want to delete " + IndividualHeaderReader["IndividualHeader_KPI"] + "?');\">Delete KPI</a></li>");

                                                            /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                            {

                                                                HtmlTableData.Append("<li role='presentation'><a role='menuitem' href='delete_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Delete Multiple Obj.</a></li>");
                                                            }
                                                            else//jika tidak ada Specific Objective
                                                            {
                                                                HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Delete Multiple Obj.</a></li>");
                                                            }*/

                                                            /*if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada Specific Objective baru bisa Add Specific Objective
                                                            {
                                                                HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='#'>Request Change/Delete KPI</a></li>");
                                                            }
                                                            else//jika tidak ada Specific Objective
                                                            {*/
                                                            HtmlTableData.Append("<li role='presentation' class='disabled'><a role='menuitem' href='request_for_change_KPI.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&period_id=" + periode_id + "'>Request Change/Delete KPI</a></li>");
                                                            //}

                                                            HtmlTableData.Append("</ul>");
                                                            HtmlTableData.Append("</div>");
                                                            HtmlTableData.Append("</td>");
                                                            HtmlTableData.Append("</tr>");
                                                            btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                            // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                            status = false;
                                                    }
                                                }
                                            }//end of StatusReader
                                            //END OF CODE untuk DISABLE yang INACTIVE

                                            //CODE untuk menampilkan yang ada SPECIFIC OBJECTIVE. Jika ada, tampilkan. Jika tidak ada, maka informasi tidak ditampilkan
                                            string select_individual_detail = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualHeader_ID="+IndividualHeaderReader["IndividualHeader_ID"]+" AND data_status='exist'";
                                            SqlCommand sql_select_individual_detail = new SqlCommand(select_individual_detail, conn);
                                            Object specific_objective = sql_select_individual_detail.ExecuteScalar();
                                            if (specific_objective != null)//jika ada specific objective
                                            {
                                                using (SqlDataReader SpecificReader = sql_select_individual_detail.ExecuteReader())
                                                {
                                                    HtmlTableData.Append("<tr>");
                                                    HtmlTableData.Append("<td colspan='10' style='padding:0'>");
                                                    HtmlTableData.Append("<div id='collapse" + collapse_jscript + "' class='panel-collapse collapse' role='tabpanel'>");
                                                    HtmlTableData.Append("<table class='table table-bordered' style='margin-bottom:0px'>");
                                                    HtmlTableData.Append("<tr>");
                                                    HtmlTableData.Append("<th colspan='10' style='background-color:#29166f; color:white'>Objective: ");
                                                    HtmlTableData.Append("<span style='font-weight:100'>" + IndividualHeaderReader["IndividualHeader_Objective"] + "</span>");
                                                    HtmlTableData.Append("</th>");
                                                    HtmlTableData.Append("</tr>");
                                                    if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Edit</th><th class='centering-th'>Delete</th></tr>");
                                                    }
                                                    else if(current_month == minus_one_bsc && current_year == year_from_startdate - 1)//jika buka januari, desember bisa add
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Edit</th><th class='centering-th'>Delete</th></tr>");
                                                    }
                                                    else if (current_month == minus_one_bsc && minus_one_bsc != 12)//jika buka maret, februari bisa add
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Edit</th><th class='centering-th'>Delete</th></tr>");
                                                    }
                                                    else if (current_month == minus_one_bsc && current_year != year_from_startdate - 1)
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Update</th><th class='centering-th'>Request For Change/Delete</th></tr>");
                                                    }
                                                    else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Update</th><th class='centering-th'>Request For Change/Delete</th></tr>");
                                                    }
                                                    else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num > end_date_bsc)
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Update</th><th class='centering-th'>Request For Change/Delete</th></tr>");
                                                    }
                                                    else if (current_month == month_num && current_year == year_from_startdate && month_num < start_date_bsc)
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Edit</th><th class='centering-th'>Delete</th></tr>");
                                                    }
                                                    else
                                                    {
                                                        HtmlTableData.Append("<tr><th class='centering-th2 custom-detail-header'>No.</th><th class='centering-th2 custom-detail-header'>Specific Objective</th><th class='centering-th'>Target</th><th class='centering-th'>Result</th><th class='centering-th'>Formula</th><th class='centering-th'>Rating</th><th class='centering-th'>Update</th><th class='centering-th'>Request For Change/Delete</th></tr>");
                                                    }
                                                    while (SpecificReader.Read())
                                                    {
                                                        HtmlTableData.Append("<tr align='center'>");
                                                        HtmlTableData.Append("<td class='td-align'>" + no_header + "." + no_detail + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + SpecificReader["IndividualDetail_Title"] + "</td>");
                                                        if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                                        {
                                                            string month_name_target = "", month_name_result = "";
                                                            int month_num_target = int.Parse(SpecificReader["IndividualDetail_Target"].ToString());
                                                            int month_num_result = int.Parse(SpecificReader["IndividualDetail_Result"].ToString());
                                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                                            month_name_result = ShowMonthNameResult(month_num_result);
                                                            HtmlTableData.Append("<td class='td-align'>" + month_name_target + "</td>");
                                                            HtmlTableData.Append("<td class='td-align'>" + month_name_result + "</td>");
                                                        }
                                                        else
                                                        {
                                                            if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                                                            {
                                                                HtmlTableData.Append("<td class='td-align'>" + SpecificReader["IndividualDetail_Target"] + "</td>");
                                                                HtmlTableData.Append("<td class='td-align'>" + SpecificReader["IndividualDetail_Result"] + "</td>");
                                                            }
                                                            else
                                                            {
                                                                HtmlTableData.Append("<td class='td-align'>" + SpecificReader["IndividualDetail_Target"] + " " + SpecificReader["IndividualDetail_MeasureBy"] + "</td>");
                                                                HtmlTableData.Append("<td class='td-align'>" + SpecificReader["IndividualDetail_Result"] + " " + SpecificReader["IndividualDetail_MeasureBy"] + "</td>");
                                                            }
                                                        }
                                                        HtmlTableData.Append("<td class='td-align'>" + SpecificReader["IndividualDetail_Formula"] + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + SpecificReader["IndividualDetail_Rating"] + "%" + "</td>");

                                                        if (status == false)//jika status periode nya TIDAK AKTIF
                                                        {
                                                            if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Edit</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Delete</a></td>");
                                                            }
                                                            else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Edit</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Delete</a></td>");
                                                            }
                                                            else if (current_month == minus_one_bsc && current_year != year_from_startdate - 1)
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                            }
                                                            else if (current_month == month_num && current_year != year_from_startdate)
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num > end_date_bsc)
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                            }
                                                            else if (current_month == month_num && current_year == year_from_startdate && month_num < start_date_bsc)
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                            }
                                                            else
                                                            {
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                            }
                                                        }
                                                        else//jika status Period BSC adalah AKTIF
                                                        {
                                                            if (review_status == false)//jika Review Status-nya adalah INACTIVE
                                                            {
                                                                if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">Delete</a></td>");
                                                                }
                                                                else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)//jika januari buka, desember bisa add
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">Delete</a></td>");
                                                                }
                                                                else if (current_month == minus_one_bsc && minus_one_bsc != 12)//jika maret buka, februari bisa add
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">Delete</a></td>");
                                                                }
                                                                /*else if (current_month == minus_one_bsc && current_year != year_from_startdate)//jika bulan review, tapi tahun sudah lewat. ga bisa review
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default disabled'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default disabled' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">current_month == minus_one_bsc && current_year != year_from_startdate - 1</a></td>");
                                                                }*/
                                                                else if (current_month == month_num && current_year != year_from_startdate)//jika bulan review, tapi tahun sudah lewat. ga bisa review
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change</a></td>");
                                                                }
                                                                else if (current_month == month_num && current_year == year_from_startdate && month_num < start_date_bsc)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='request_for_change_specific_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Delete</a></td>");
                                                                }
                                                                else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='request_for_change_specific_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Request</a></td>");
                                                                }
                                                                else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num > end_date_bsc)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                                }
                                                                else
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                                }
                                                            }
                                                            else//jika status REVIEW adalah ACTIVE
                                                            {
                                                                if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">Delete</a></td>");
                                                                }
                                                                else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)//jika januari buka, desember bisa add
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">Delete</a></td>");
                                                                }
                                                                else if (current_month == minus_one_bsc && minus_one_bsc != 12)//jika maret buka, februari bisa add
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">Delete</a></td>");
                                                                }
                                                                /*else if (current_month == minus_one_bsc && current_year != year_from_startdate)//jika bulan review, tapi tahun sudah lewat. ga bisa review
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='delete_single_specific_objective.aspx?page=" + page + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "'  class='btn btn-default' onclick=\"return confirm('Are you sure you want to delete " + SpecificReader["IndividualDetail_Title"] + "?');\">current_year != year_from_startdate - 1</a></td>");
                                                                }*/
                                                                else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='edit_individual_specific.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='request_for_change_specific_objective.aspx?page=" + page + "&header_id=" + IndividualHeaderReader["IndividualHeader_ID"] + "&detail_id=" + SpecificReader["IndividualDetail_ID"] + "&period_id=" + periode_id + "' class='btn btn-default'>Change/Delete</a></td>");
                                                                }
                                                                else if (current_month == month_num && current_year == year_from_startdate && month_num > start_date_bsc && month_num > end_date_bsc)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                                }
                                                                else if (current_month == month_num && current_year != year_from_startdate)//jika bulan review, tapi tahun sudah lewat. ga bisa review
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                                }
                                                                else if (current_month == month_num && current_year == year_from_startdate && month_num < start_date_bsc)
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align'><a href='#' class='btn btn-default disabled'>Edit</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align'><a href='#' class='btn btn-default disabled'>Delete</a></td>");
                                                                }
                                                                else
                                                                {
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Update</a></td>");
                                                                    HtmlTableData.Append("<td class='td-align disabled'><a href='#' class='btn btn-default disabled'>Change/Delete</a></td>");
                                                                }
                                                            }
                                                        }
                                                        HtmlTableData.Append("</tr>");
                                                        total_rating = total_rating + float.Parse(SpecificReader["IndividualDetail_Rating"].ToString());
                                                        no_detail++;
                                                    }//end of While SpecificReader Read
                                                    HtmlTableData.Append("</tr>");
                                                    HtmlTableData.Append("<tr align='center'>");
                                                    HtmlTableData.Append("<td colspan=5><b>TOTAL RATING</b></td>");
                                                    HtmlTableData.Append("<td><b>" + Math.Round(total_rating,2) + "%</b></td>");
                                                    HtmlTableData.Append("</tr>");
                                                    total_rating = 0;//agar penjumlahannya tidak dilanjutkan lagi
                                                }//end of SpecificReader
                                            }
                                            else if(int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString())==-1)//jika ada SO tetapi belum di-insert
                                            {
                                                HtmlTableData.Append("<tr>");
                                                HtmlTableData.Append("<td colspan='10' style='padding:0'>");
                                                HtmlTableData.Append("<div id='collapse" + collapse_jscript + "' class='panel-collapse collapse' role='tabpanel'>");
                                                HtmlTableData.Append("<table class='table table-bordered' style='margin-bottom:0px'>");
                                                HtmlTableData.Append("<tr>");
                                                HtmlTableData.Append("<th colspan='10' style='background-color:#29166f; color:white'>Objective: ");
                                                HtmlTableData.Append("<span style='font-weight:100'>" + IndividualHeaderReader["IndividualHeader_Objective"] + "</span>");
                                                HtmlTableData.Append("</th>");
                                                HtmlTableData.Append("</tr>");
                                                HtmlTableData.Append("<tr><th class='centering-th2' colspan='10'>No Specific Objective Inserted</th></tr>");
                                            }
                                            else if (int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) >= 0)//jika tidak ada SO dan berdiri sendiri
                                            {
                                                HtmlTableData.Append("<tr>");
                                                HtmlTableData.Append("<td colspan='10' style='padding:0'>");
                                                HtmlTableData.Append("<div id='collapse" + collapse_jscript + "' class='panel-collapse collapse' role='tabpanel'>");
                                                HtmlTableData.Append("<table class='table table-bordered' style='margin-bottom:0px'>");
                                                HtmlTableData.Append("<tr>");
                                                HtmlTableData.Append("<th colspan='10' style='background-color:#29166f; color:white'>Objective: ");
                                                HtmlTableData.Append("<span style='font-weight:100'>" + IndividualHeaderReader["IndividualHeader_Objective"] + "</span>");
                                                HtmlTableData.Append("</th>");
                                            }
                                            no_detail = 1;
                                            HtmlTableData.Append("</table>");
                                            HtmlTableData.Append("</div>");
                                            HtmlTableData.Append("</td>");
                                            HtmlTableData.Append("</tr>");
                                            no_header++;
                                            collapse_jscript++;
                                        }
                                    }//end of if(individualheaderreader.hasrows)
                                    else//error handling untuk PERIODE yang Aktif, tetapi Datanya Tidak ada!
                                    {
                                        string check_if_period_active = "SELECT * FROM BSC_Period WHERE Period_ID=" + periode_id + "";
                                        SqlCommand sql_check_if_period_active = new SqlCommand(check_if_period_active, conn);
                                        HtmlTableData.Append("<tr><th class='centering-th2' colspan='10'>There is no KPI inserted</th></tr>");

                                        using (SqlDataReader ActivePeriodReader = sql_check_if_period_active.ExecuteReader())
                                        {
                                            while (ActivePeriodReader.Read())
                                            {
                                                if (ActivePeriodReader["Period_Status"].ToString() == "Active")
                                                {
                                                    string active_review = "";
                                                    DateTime start_date = Convert.ToDateTime(ActivePeriodReader["Start_Period"]);
                                                    start_date_bsc = int.Parse(start_date.ToString("MM"));
                                                    year_from_startdate = int.Parse(start_date.ToString("yyyy"));

                                                    if (start_date_bsc == 1)
                                                    {
                                                        minus_one_bsc = 12;
                                                    }
                                                    else
                                                    {
                                                        minus_one_bsc = start_date_bsc - 1;
                                                    }

                                                    string string_select_active_month = "SELECT month_num FROM ScorecardReview WHERE "
                                                                                      + "Review_Status='Active' AND Review_Name='" + LabelReview.Text + "'";
                                                    string string_select_active_review_month = "SELECT Review_Status FROM ScorecardReview WHERE month_num=" + current_month + " AND Review_Status='Active' "
                                                                                              + "AND Review_Name='" + LabelReview.Text + "'";
                                                    SqlCommand sql_select_active_month = new SqlCommand(string_select_active_month, conn);
                                                    using (SqlDataReader MonthReader = sql_select_active_month.ExecuteReader())
                                                    {
                                                        while (MonthReader.Read())
                                                        {
                                                            month_num = int.Parse(MonthReader["month_num"].ToString());
                                                        }
                                                        MonthReader.Dispose();
                                                        MonthReader.Close();
                                                    }

                                                    if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                    {
                                                        LabelAddRequest.Text = "Add New KPI";
                                                    }
                                                    else if (current_month == minus_one_bsc && minus_one_bsc != 12)
                                                    {
                                                        LabelAddRequest.Text = "Add New KPI";
                                                    }
                                                    else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                                                    {
                                                        LabelAddRequest.Text = "Add New KPI";
                                                    }
                                                    else if (current_month == month_num && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                    {
                                                        LabelAddRequest.Text = "Request New KPI";
                                                    }
                                                    else if (current_month == month_num && month_num > start_date_bsc && month_num > end_date_bsc)
                                                    {
                                                        LabelAddRequest.Text = "Request New KPI";
                                                    }
                                                    else
                                                    {
                                                        LabelAddRequest.Text = "Request New KPI";
                                                    }

                                                    SqlCommand sql_select_active_review_month = new SqlCommand(string_select_active_review_month, conn);
                                                    active_review = (string)sql_select_active_review_month.ExecuteScalar();
                                                    if (active_review == "Active")
                                                    {
                                                        btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                        // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                    }
                                                    else if (current_month == start_date_bsc && current_year == year_from_startdate)
                                                    {
                                                        btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                        // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                    }
                                                    else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                                                    {
                                                        btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                        // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                    }
                                                    else if (current_month == minus_one_bsc && current_year != year_from_startdate - 1)
                                                    {
                                                        btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                        // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                    }
                                                    else if (current_month == month_num && month_num > start_date_bsc && month_num <= end_date_bsc)
                                                    {
                                                        btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                        // btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                    }
                                                    else if (current_month == month_num && month_num > start_date_bsc && month_num > end_date_bsc)
                                                    {
                                                        btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                    }
                                                    else
                                                    {
                                                        btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                    }
                                                    //btnNewKPI.Attributes.Add("a href", "add_new_kpi.aspx?page=" + page + "&period_id=" + periode_id + "");
                                                    //btnNewKPI.Attributes.Add("class", "btn btn-default");
                                                    //btnDeleteKPI.Attributes.Add("a href", "delete_kpi.aspx?page=" + page + "&period_id=" + periode_id + "");
                                                    //btnDeleteKPI.Attributes.Add("class", "btn btn-default");
                                                }
                                                else
                                                {
                                                    btnNewKPI.Attributes.Add("a href", "#");
                                                    btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                                                    LabelAddRequest.Text = "Add New KPI";
                                                    //btnDeleteKPI.Attributes.Add("a href", "#");
                                                    //btnDeleteKPI.Attributes.Add("class", "btn btn-default disabled");
                                                }
                                            }
                                            ActivePeriodReader.Dispose();
                                            ActivePeriodReader.Close();
                                        }//end of using ActivePeriodReader
                                    }
                                }
                            }
                        }//end of if(PeriodReader.HasRows)
                        else//error handling untuk yang PERIODE TIDAK ADA, dan DATA JUGA TIDAK ADA
                        {
                            HtmlTableData.Append("<tr><th class='centering-th2'>There is no data to display</th></tr>");
                            btnNewKPI.Attributes.Add("a href", "#");
                            btnNewKPI.Attributes.Add("class", "btn btn-default disabled");
                            //btnDeleteKPI.Attributes.Add("a href", "#");
                            //btnDeleteKPI.Attributes.Add("class", "btn btn-default disabled");
                        }
                    }//end of PeriodReader
                    conn.Close();
                }//end of SqlConnection
                PlaceHolderTable.Controls.Add(new Literal { Text = HtmlTableData.ToString() }); //untuk Table

                //Code untuk Pagination
                Pagination.Append("<ul id='my-pagination' class='pagination'></ul>");

                //Pagination JQuery
                Pagination.Append("<script>");
                Pagination.Append("$('#my-pagination').twbsPagination({");
                Pagination.Append("totalPages: "+max_page+",");
                Pagination.Append("visiblePages: 7,");
                Pagination.Append("href: '?page={{number}}&id=" + periode_id + "'");
                Pagination.Append("});");
                Pagination.Append("</script>");
                PlaceHolderPaging.Controls.Add(new Literal { Text = Pagination.ToString() });//untuk Pagination

            }//end of IsPostBack
        }//end of Page_Load

        protected void OnClickExportPDF(object sender, EventArgs e)//Tampilkan FINANCIAL MEASURE dahulu, baru INDIVIDUAL SCORECARD dibawahnya
        {
            //Buat PDF
            Response.ContentType = "application/pdf";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWriter);
            StringReader read = new StringReader(stringWriter.ToString());
            string ImageURL = Server.MapPath(".") + "/Images/mppa.png";//untuk insert image ke PDF
            iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(ImageURL);
            png.ScaleToFit(650f, 200f);//untuk resize image 
            png.SpacingBefore = 10f;//kasih space sebelum image
            png.SpacingAfter = 10f;//kasih space sesudah image
            png.Alignment = Element.ALIGN_CENTER;

            Document pdfDoc = new Document();
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());//agar halaman PDF Landscape
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            writer.PageEvent = new System.Data.Common.ITextEvents();
            pdfDoc.Open();

            HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
            htmlContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(false);

            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(pdfDoc, writer)));

            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser xmlParse = new XMLParser(true, worker);

            StringBuilder DataTable = new StringBuilder();

            //data di PDF
            int no_header = 1;
            string check_reserved_char_measure, check_reserved_char_remarks;//digunakan unuk mengecek XML Reserved Characters
            var period_id = Request.QueryString["id"];
            DateTime start_date = new DateTime();
            DateTime end_date = new DateTime();
            object period_id_active;
            string header_individual, scorecard_user_name = "";
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                if (period_id == null)
                {
                    string select_active_period_id = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
                    SqlCommand sql_select_active_period_id = new SqlCommand(select_active_period_id, conn);
                    period_id_active = sql_select_active_period_id.ExecuteScalar();
                    if (period_id_active != null)//jika ADA Period_ID yang berstatus 'Active'
                    {
                        period_id = period_id_active.ToString();
                    }
                    else//jika TIDAK ADA Period_ID yang berstatus 'Active'
                    {
                        period_id = "1";
                    }
                }

                string select_period_date = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                SqlCommand sql_select_period_date = new SqlCommand(select_period_date, conn);

                //ASUMSI--> USER NIK = 100
                string user_id = (string)Session["user_id"];
                string select_user_info = "SELECT empId, empName, OrgName, JobTtlName, empGrade FROM ScorecardUser "
                                        + "join [Human_Capital_demo].dbo.Organization ON ScorecardUser.empOrgCode = Organization.OrgCode "
                                        + "join [Human_Capital_demo].dbo.JobTitle ON ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                        + "WHERE user_id=" + user_id + "";
                SqlCommand sql_select_user_info = new SqlCommand(select_user_info, conn);
                string select_group_name = "SELECT Group_Name FROM ScorecardUser "
                                         + "JOIN ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode = ScorecardGroupLink.OrgAdtGroupCode "
                                         + "WHERE ScorecardUser.user_id=" + user_id + " AND ScorecardGroupLink.Period_ID=" + period_id + "";
                SqlCommand sql_select_group_name = new SqlCommand(select_group_name, conn);
                header_individual = (string)sql_select_group_name.ExecuteScalar();

                //select informasi Group user
                string select_group_info = "SELECT * FROM FinancialMeasures_Header WHERE FinancialHeader_Group='" + header_individual + "' AND Period_ID=" + period_id + " AND data_status='exist'";
                SqlCommand sql_select_group_info = new SqlCommand(select_group_info, conn);

                DataTable.Append("<p style='text-align:center; font-size:50px'>PT. Matahari Putra Prima Tbk.</p>");

                using (SqlDataReader PeriodReader = sql_select_period_date.ExecuteReader())
                {
                    while (PeriodReader.Read())
                    {
                        string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya tidak keluar
                        start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                        end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                        startdate_to_date = start_date.ToString("MMM");
                        if (DateTime.Now.ToString("yyyy") != end_date.ToString("yyyy"))
                        {
                            enddate_to_date = end_date.ToString("MMM") + " " + end_date.ToString("yyyy");
                        }
                        else
                        {
                            enddate_to_date = "" + DateTime.Now.ToString("MMM") + " " + end_date.ToString("yyyy") + "";
                        }
                        start_end_date = startdate_to_date + " - " + enddate_to_date;
                        DataTable.Append("<p style='text-align:center; font-size:30px'>Individual Scorecard " + start_date.ToString("yyyy") + "</p>");
                        DataTable.Append("<p style='text-align:center; font-size:30px'>(" + start_end_date + ")</p>");
                        DataTable.Append("<br/>");
                        pdfDoc.Add(png);
                    }
                }

                using (SqlDataReader UserReader = sql_select_user_info.ExecuteReader())
                {
                    while(UserReader.Read())
                    {
                        scorecard_user_name = UserReader["empName"].ToString();
                        DataTable.Append("<p style='text-align:center; font-size:30px'><b>Name: </b>" + UserReader["empName"].ToString() + "</p>");
                        DataTable.Append("<p style='text-align:center; font-size:30px'><b>NIK: </b>" + UserReader["EmpId"].ToString() + "</p>");
                        DataTable.Append("<p style='text-align:center; font-size:30px'><b>Organization: </b>" + UserReader["OrgName"].ToString() + "</p>");
                        DataTable.Append("<p style='text-align:center; font-size:30px'><b>Position Title: </b>" + UserReader["JobTtlName"].ToString() + "</p>");
                        DataTable.Append("<p style='text-align:center; font-size:30px; page-break-after: always'><b>Grade: </b>" + UserReader["empGrade"].ToString() + "</p>");
                        Response.AddHeader("content-disposition", "attachment;filename=" + UserReader["empName"] + " (" + UserReader["EmpId"] + ") Individual Balanced Scorecard " + start_date.ToString("yyyy") + ".pdf");
                    }
                }

                using (SqlDataReader HeaderReader = sql_select_group_info.ExecuteReader())
                {
                    while (HeaderReader.Read())
                    {
                        float total_weight = 0, total_score = 0;
                        string select_detail_single = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Single'";
                        string select_detail_share_zero = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked=0";//untuk error handling jika ada share yang belum di Link
                        SqlCommand sql_select_detail_single = new SqlCommand(select_detail_single, conn);
                        SqlCommand sql_select_detail_share_zero = new SqlCommand(select_detail_share_zero, conn);

                        DataTable.Append("<table align='center' width='100%' style='border-collapse:collapse; border:2px solid black; font-size:8pt' >");
                        DataTable.Append("<tr>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black; background-color:yellow' colspan='8'><b>Financial Measures</b></td>");
                        DataTable.Append("</tr>");

                        DataTable.Append("<tr>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'><b>Group</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'>" + HeaderReader["FinancialHeader_Group"] + "</td>");
                        DataTable.Append("</tr>");
                        DataTable.Append("<tr>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'><b>Stretch Rating</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'>" + HeaderReader["FinancialHeader_StretchRating"] + "%</td>");
                        DataTable.Append("</tr>");

                        DataTable.Append("<tr>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>No.</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Financial Measure</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Target</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Result</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Rating</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Weight (%)</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Score</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Remarks</b></td>");
                        DataTable.Append("</tr>");

                        //Financial Type = 'Single'
                        using (SqlDataReader SingleReader = sql_select_detail_single.ExecuteReader())
                        {
                            if (SingleReader.HasRows)
                            {
                                while (SingleReader.Read())
                                {
                                    DataTable.Append("<tr>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + no_header + "</td>");

                                    //cek apakah ada XML Reserved Characters
                                    check_reserved_char_measure = SingleReader["FinancialMeasure"].ToString();
                                    if (check_reserved_char_measure.Contains("&")) check_reserved_char_measure = check_reserved_char_measure.Replace("&", "&amp;");
                                    check_reserved_char_remarks = SingleReader["FinancialRemarks"].ToString();
                                    if (check_reserved_char_remarks.Contains("&")) check_reserved_char_remarks = check_reserved_char_remarks.Replace("&", "&amp;");

                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_reserved_char_measure + "</td>");
                                    if (SingleReader["FinancialMeasureBy"].ToString() == "Month")
                                    {
                                        string month_name_target = "", month_name_result = "";
                                        int month_num_target = int.Parse(SingleReader["FinancialTarget"].ToString());
                                        int month_num_result = int.Parse(SingleReader["FinancialResult"].ToString());
                                        month_name_target = ShowMonthNameTarget(month_num_target);
                                        month_name_result = ShowMonthNameResult(month_num_result);
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                    }
                                    else
                                    {
                                        if (SingleReader["FinancialMeasureBy"].ToString() == "Numbers")
                                        {
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SingleReader["FinancialTarget"] + "</td>");
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SingleReader["FinancialResult"] + "</td>");
                                        }
                                        else
                                        {
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SingleReader["FinancialTarget"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SingleReader["FinancialResult"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                        }
                                    }
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SingleReader["FinancialRating"] + "%</td>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SingleReader["FinancialWeight"] + "%</td>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SingleReader["FinancialScore"] + "%</td>");
                                    if (SingleReader["FinancialRemarks"].Equals(""))
                                    {
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>No Remarks</td>");
                                    }
                                    else
                                    {
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_reserved_char_remarks + "</td>");
                                    }
                                    DataTable.Append("</tr>");
                                    no_header++;
                                    total_weight = total_weight + float.Parse(SingleReader["FinancialWeight"].ToString());
                                    total_score = total_score + float.Parse(SingleReader["FinancialScore"].ToString());
                                }
                            }
                        }

                        //Financial Type = 'Share' & Financial Linked = 0

                        using (SqlDataReader ShareZeroReader = sql_select_detail_share_zero.ExecuteReader())
                        {
                            if (ShareZeroReader.HasRows)
                            {
                                while (ShareZeroReader.Read())
                                {
                                    DataTable.Append("<tr>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + no_header + "</td>");

                                    //cek apakah ada XML Reserved Characters
                                    check_reserved_char_measure = ShareZeroReader["FinancialMeasure"].ToString();
                                    if (check_reserved_char_measure.Contains("&")) check_reserved_char_measure = check_reserved_char_measure.Replace("&", "&amp;");
                                    check_reserved_char_remarks = ShareZeroReader["FinancialRemarks"].ToString();
                                    if (check_reserved_char_remarks.Contains("&")) check_reserved_char_remarks = check_reserved_char_remarks.Replace("&", "&amp;");

                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_reserved_char_measure + "</td>");
                                    if (ShareZeroReader["FinancialMeasureBy"].ToString() == "Month")
                                    {
                                        string month_name_target = "", month_name_result = "";
                                        int month_num_target = int.Parse(ShareZeroReader["FinancialTarget"].ToString());
                                        int month_num_result = int.Parse(ShareZeroReader["FinancialResult"].ToString());
                                        month_name_target = ShowMonthNameTarget(month_num_target);
                                        month_name_result = ShowMonthNameResult(month_num_result);
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                    }
                                    else
                                    {
                                        if (ShareZeroReader["FinancialMeasureBy"].ToString() == "Numbers")
                                        {
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialTarget"] + "</td>");
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialResult"] + "</td>");
                                        }
                                        else
                                        {
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialTarget"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialResult"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                        }
                                    }
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialRating"] + "%</td>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialWeight"] + "%</td>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialScore"] + "%</td>");
                                    if (ShareZeroReader["FinancialRemarks"].Equals(""))
                                    {
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>No Remarks</td>");
                                    }
                                    else
                                    {
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_reserved_char_remarks + "</td>");
                                    }
                                    DataTable.Append("</tr>");
                                    no_header++;
                                    total_weight = total_weight + float.Parse(ShareZeroReader["FinancialWeight"].ToString());
                                    total_score = total_score + float.Parse(ShareZeroReader["FinancialScore"].ToString());
                                }
                            }
                        }

                        //Financial Type = 'Share' & Financial Linked != 0
                        string string_distinct_link_value = "SELECT DISTINCT FinancialLinked FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + "AND FinancialType='Share' AND FinancialLinked<>0";
                        SqlCommand sql_distinct_link_value = new SqlCommand(string_distinct_link_value, conn);
                        using (SqlDataReader DistinctReader = sql_distinct_link_value.ExecuteReader())//butuh agar item yang Linked menjadi teratur
                        {
                            if (DistinctReader.HasRows)
                            {
                                while (DistinctReader.Read())
                                {
                                    string select_detail_share_linked = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialLinked=" + DistinctReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked<>0";
                                    SqlCommand sql_select_detail_share_linked = new SqlCommand(select_detail_share_linked, conn);
                                    using (SqlDataReader ShareLinkedReader = sql_select_detail_share_linked.ExecuteReader())
                                    {
                                        if (ShareLinkedReader.HasRows)
                                        {
                                            int loop_number = 1;
                                            while (ShareLinkedReader.Read())
                                            {
                                                string link_count = "SELECT COUNT(FinancialLinked) FROM FinancialMeasures_Detail WHERE FinancialLinked=" + ShareLinkedReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + "";
                                                SqlCommand sql_count = new SqlCommand(link_count, conn);
                                                int rowspan = (int)sql_count.ExecuteScalar();
                                                DataTable.Append("<tr>");
                                                if (loop_number == 1)
                                                {
                                                    DataTable.Append("<td align='center' style='vertical-align:middle; padding:10px; border:1px solid black' rowspan='" + rowspan + "'>" + no_header + "</td>");
                                                }

                                                //cek apakah ada XML Reserved Characters
                                                check_reserved_char_measure = ShareLinkedReader["FinancialMeasure"].ToString();
                                                if (check_reserved_char_measure.Contains("&")) check_reserved_char_measure = check_reserved_char_measure.Replace("&", "&amp;");
                                                check_reserved_char_remarks = ShareLinkedReader["FinancialRemarks"].ToString();
                                                if (check_reserved_char_remarks.Contains("&")) check_reserved_char_remarks = check_reserved_char_remarks.Replace("&", "&amp;");

                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_reserved_char_measure + "</td>");
                                                if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Month")
                                                {
                                                    string month_name_target = "", month_name_result = "";
                                                    int month_num_target = int.Parse(ShareLinkedReader["FinancialTarget"].ToString());
                                                    int month_num_result = int.Parse(ShareLinkedReader["FinancialResult"].ToString());
                                                    month_name_target = ShowMonthNameTarget(month_num_target);
                                                    month_name_result = ShowMonthNameResult(month_num_result);
                                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                                }
                                                else
                                                {
                                                    if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Numbers")
                                                    {
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialTarget"] + "</td>");
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialResult"] + "</td>");
                                                    }
                                                    else
                                                    {
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialTarget"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialResult"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                    }
                                                }
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialRating"] + "%</td>");
                                                if (loop_number == 1)
                                                {
                                                    DataTable.Append("<td align='center' style='vertical-align:middle; padding:10px; border:1px solid black' rowspan='" + rowspan + "'>" + ShareLinkedReader["FinancialWeight"] + "%" + "</td>");
                                                    DataTable.Append("<td align='center' style='vertical-align:middle; padding:10px; border:1px solid black' rowspan='" + rowspan + "'>" + ShareLinkedReader["FinancialScore"] + "%" + "</td>");
                                                    total_weight = total_weight + float.Parse(ShareLinkedReader["FinancialWeight"].ToString());
                                                    total_score = total_score + float.Parse(ShareLinkedReader["FinancialScore"].ToString());
                                                }
                                                if (ShareLinkedReader["FinancialRemarks"].Equals(""))
                                                {
                                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>No Remarks</td>");
                                                }
                                                else
                                                {
                                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_reserved_char_remarks + "</td>");
                                                }
                                                DataTable.Append("</tr>");
                                                loop_number++;
                                            }//end of While ShareReaderLinked Read
                                        }//end of if ShareReaderLinked Has Rows
                                    }//end of SqlDataReader ShareReaderLinked
                                    no_header++;
                                }//end of While Distinct Reader Read
                            }//end of If Distinct Reader Has Rows
                        }//end of SqlDataReader DistinctReader

                        DataTable.Append("<tr>");
                        DataTable.Append("<td colspan='5' style='padding:10px; border:1px solid black'></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_weight + "%</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_score + "%</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black; page-break-after: always'></td>");
                        DataTable.Append("</tr>");

                        //Menampilkan INDIVIDUAL MEASURES (harusnya ada USERNAME)
                        int no_header_detail = 1, no_specific_obj = 1;
                        float total_individual_weight = 0, total_individual_score = 0;
                        string check_char_KPI, check_char_obj, check_char_title;

                        //ASUMSI --> NIK=100
                        string string_select_individual_header = "SELECT * FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + " AND data_status='exist' AND user_id=" + user_id + "";
                        SqlCommand sql_select_individual_header = new SqlCommand(string_select_individual_header, conn);
                        using (SqlDataReader IndividualHeaderReader = sql_select_individual_header.ExecuteReader())
                        {
                            //INDIVIDUAL MEASURES
                            DataTable.Append("<tr>");
                            DataTable.Append("<td align='center' colspan='15' style='background-color:yellow; padding:10px; border:1px solid black'><b>Individual Measures</b></td>");
                            DataTable.Append("</tr>");

                            DataTable.Append("<tr>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>No.</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>KPI</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Target</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Result</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Rating</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Weight (%)</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>Score</b></td>");
                            DataTable.Append("<td align='center' colspan='8' style='padding:10px; border:1px solid black'><b>Objective</b></td>");
                            DataTable.Append("</tr>");

                            if (IndividualHeaderReader.HasRows)
                            {
                                while (IndividualHeaderReader.Read())
                                {
                                    DataTable.Append("<tr>");
                                    DataTable.Append("<td align='left' style='padding:10px; border:1px solid black'>" + no_header_detail + "</td>");

                                    check_char_KPI = IndividualHeaderReader["IndividualHeader_KPI"].ToString();
                                    if (check_char_KPI.Contains("&")) check_char_KPI = check_char_KPI.Replace("&", "&amp;");
                                    check_char_obj = IndividualHeaderReader["IndividualHeader_Objective"].ToString();
                                    if (check_char_obj.Contains("&")) check_char_obj = check_char_obj.Replace("&", "&amp;");

                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_char_KPI + "</td>");

                                    if (int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika Targetnya = 'Based on Schedule'
                                    {
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>Based On Schedule</td>");
                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'> - </td>");
                                    }
                                    else//jika Targetnya bukan -1
                                    {
                                        if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString());
                                            int month_num_result = int.Parse(IndividualHeaderReader["IndividualHeader_Result"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                        }
                                        else
                                        {
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                                            {
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + IndividualHeaderReader["IndividualHeader_Target"] + "</td>");
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + IndividualHeaderReader["IndividualHeader_Result"] + "</td>");
                                            }
                                            else
                                            {
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + IndividualHeaderReader["IndividualHeader_Target"] + " " + IndividualHeaderReader["IndividualHeader_MeasureBy"] + "</td>");
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + IndividualHeaderReader["IndividualHeader_Result"] + " " + IndividualHeaderReader["IndividualHeader_MeasureBy"] + "</td>");
                                            }
                                        }
                                    }

                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + IndividualHeaderReader["IndividualHeader_Rating"] + "%" + "</td>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + IndividualHeaderReader["IndividualHeader_Weight"] + "%" + "</td>");
                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + IndividualHeaderReader["IndividualHeader_Score"] + "%" + "</td>");
                                    DataTable.Append("<td align='center' colspan='8' style='padding:10px; border:1px solid black'>" + check_char_obj + "</td>");
                                    DataTable.Append("</tr>");

                                    total_individual_weight = total_individual_weight + float.Parse(IndividualHeaderReader["IndividualHeader_Weight"].ToString());
                                    total_individual_score = total_individual_score + float.Parse(IndividualHeaderReader["IndividualHeader_Score"].ToString());

                                    //CODE untuk menampilkan yang ada SPECIFIC OBJECTIVE. Jika ada, tampilkan. Jika tidak ada, maka informasi tidak ditampilkan
                                    string select_individual_detail = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualHeader_ID=" + IndividualHeaderReader["IndividualHeader_ID"] + " AND data_status='exist'";
                                    SqlCommand sql_select_individual_detail = new SqlCommand(select_individual_detail, conn);
                                    Object specific_objective = sql_select_individual_detail.ExecuteScalar();

                                    if (specific_objective != null)//jika ada specific objective
                                    {
                                        using (SqlDataReader SpecificReader = sql_select_individual_detail.ExecuteReader())
                                        {
                                            while (SpecificReader.Read())
                                            {
                                                DataTable.Append("<tr align='center' style='padding:10px; border:1px solid black'>");
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + no_header_detail + "." + no_specific_obj + "</td>");

                                                check_char_title = SpecificReader["IndividualDetail_Title"].ToString();
                                                if (check_char_title.Contains("&")) check_char_title = check_char_title.Replace("&", "&amp;");

                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + check_char_title + "</td>");
                                                if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                                {
                                                    string month_name_target = "", month_name_result = "";
                                                    int month_num_target = int.Parse(SpecificReader["IndividualDetail_Target"].ToString());
                                                    int month_num_result = int.Parse(SpecificReader["IndividualDetail_Result"].ToString());
                                                    month_name_target = ShowMonthNameTarget(month_num_target);
                                                    month_name_result = ShowMonthNameResult(month_num_result);
                                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                                    DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                                }
                                                else
                                                {
                                                    if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                                                    {
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SpecificReader["IndividualDetail_Target"] + "</td>");
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SpecificReader["IndividualDetail_Result"] + "</td>");
                                                    }
                                                    else
                                                    {
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SpecificReader["IndividualDetail_Target"] + " " + SpecificReader["IndividualDetail_MeasureBy"] + "</td>");
                                                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SpecificReader["IndividualDetail_Result"] + " " + SpecificReader["IndividualDetail_MeasureBy"] + "</td>");
                                                    }
                                                }
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>" + SpecificReader["IndividualDetail_Rating"] + "%" + "</td>");
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>-</td>");
                                                DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'>-</td>");
                                                DataTable.Append("</tr>");
                                                no_specific_obj++;
                                            }//end of While SpecificReader Read
                                        }//end of SqlDataReader SpecificReader
                                    }
                                    else if (int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada SO tetapi belum di-insert
                                    {
                                        DataTable.Append("<tr><td align='center' colspan='7' style='padding:10px; border:1px solid black'>No Specific Objectives</td></tr>");
                                    }
                                    no_header_detail++;
                                    no_specific_obj = 1;
                                }//end of While IndividualHeaderReader
                            }
                            else
                            {
                                DataTable.Append("<tr>");
                                DataTable.Append("<td colspan='15' align='center' style='padding:10px; border:1px solid black'>No KPIs</td>");
                                DataTable.Append("</tr>");
                            }
                        }

                        DataTable.Append("<tr>");
                        DataTable.Append("<td colspan='5' align='center' style='background-color:#dbeef3; padding:10px; border:1px solid black'><b>SUB TOTAL</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_individual_weight + "%</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_individual_score + "%</b></td>");
                        DataTable.Append("</tr>");

                        total_weight = total_weight + total_individual_weight;
                        total_score = total_score + total_individual_score;

                        DataTable.Append("<tr>");
                        DataTable.Append("<td colspan='5' align='center' style='background-color:#00b0f0; padding:10px; border:1px solid black'><b>TOTAL SCORE</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_weight + "%</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_score + "%</b></td>");
                        DataTable.Append("</tr>");

                        DataTable.Append("</table>");
                    }//end of while HeaderReader
                }
                conn.Close();
            }

            DataTable.Append("<table>");
            DataTable.Append("<tr rowspan='10'>");
            DataTable.Append("<td style='color:white'>blank space</td>");
            DataTable.Append("</tr>");
            DataTable.Append("<tr rowspan='10'>");
            DataTable.Append("<td style='color:white'>blank space</td>");
            DataTable.Append("</tr>");
            DataTable.Append("<tr>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td><b>Scorecard Holder</b></td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td><b>Approved By</b></td>");
            DataTable.Append("</tr>");
            DataTable.Append("<tr rowspan='10'>");
            DataTable.Append("<td style='color:white'>blank space</td>");
            DataTable.Append("</tr>");
            DataTable.Append("<tr rowspan='10'>");
            DataTable.Append("<td style='color:white'>blank space</td>");
            DataTable.Append("</tr>");
            DataTable.Append("<tr>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td>"+scorecard_user_name+"</td>");
            DataTable.Append("</tr>");
            DataTable.Append("<tr>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td>___________________</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td>___________________</td>");
            DataTable.Append("</tr>");
            DataTable.Append("<tr>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td>Date:</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td colspan='10' style='color:white'>blank space</td>");
            DataTable.Append("<td>Date:</td>");
            DataTable.Append("</tr>");

            DataTable.Append("</table>");

            //============================================================================================//

            StringReader sr = new StringReader(DataTable.ToString());
            xmlParse.Parse(sr);
            xmlParse.Flush();
            pdfDoc.Close();
        }

        protected void OnClickExportExcel(object sender, EventArgs e)
        {
            int no_header = 1, no_header_detail = 1, no_specific_obj = 1;
            float total_header_weight = 0, total_header_score = 0, total_individual_weight = 0, total_individual_score = 0, total_score = 0, total_weight = 0;
            string header_individual;
            var period_id = Request.QueryString["id"];
            object period_id_active;
            DateTime start_date = new DateTime();
            DateTime end_date = new DateTime();
            string select_active_period_id = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";

            string user_id = (string)Session["user_id"];

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                if (period_id == null)
                {
                    SqlCommand sql_select_active_period_id = new SqlCommand(select_active_period_id, conn);
                    period_id_active = sql_select_active_period_id.ExecuteScalar();
                    if (period_id_active != null)//jika ADA Period_ID yang berstatus 'Active'
                    {
                        period_id = period_id_active.ToString();
                    }
                    else//jika TIDAK ADA Period_ID yang berstatus 'Active'
                    {
                        period_id = "1";
                    }
                }

                string select_period_date = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                SqlCommand sql_select_period_date = new SqlCommand(select_period_date, conn);

                //ASUMSI--> USER NIK = 100
                string select_user_info = "SELECT empId, empName, OrgName, JobTtlName, empGrade FROM ScorecardUser "
                                        + "join [Human_Capital_demo].dbo.Organization ON ScorecardUser.empOrgCode = Organization.OrgCode "
                                        + "join [Human_Capital_demo].dbo.JobTitle ON ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                        + "WHERE user_id=" + user_id + "";
                SqlCommand sql_select_user_info = new SqlCommand(select_user_info, conn);

                string select_group_name = "SELECT Group_Name FROM ScorecardUser "
                                         + "JOIN ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode = ScorecardGroupLink.OrgAdtGroupCode "
                                         + "WHERE ScorecardUser.user_id=" + user_id + " AND ScorecardGroupLink.Period_ID=" + period_id + "";
                SqlCommand sql_select_group_name = new SqlCommand(select_group_name, conn);
                header_individual = (string)sql_select_group_name.ExecuteScalar();

                //select informasi Group user
                string select_group_info = "SELECT * FROM FinancialMeasures_Header WHERE FinancialHeader_Group='"+header_individual+"' AND Period_ID="+period_id+" AND data_status='exist'";
                SqlCommand sql_select_group_info = new SqlCommand(select_group_info, conn);

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

                using (SqlDataReader PeriodReader = sql_select_period_date.ExecuteReader())
                {
                    while (PeriodReader.Read())
                    {
                        string startdate_to_date, enddate_to_date, start_end_date;
                        start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                        end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                        startdate_to_date = start_date.ToString("MMM");
                        if (DateTime.Now.ToString("yyyy") != end_date.ToString("yyyy"))
                        {
                            enddate_to_date = end_date.ToString("MMM") + " " + end_date.ToString("yyyy");
                        }
                        else
                        {
                            enddate_to_date = "" + DateTime.Now.ToString("MMM") + " " + end_date.ToString("yyyy") + "";
                        }
                        start_end_date = startdate_to_date + " - " + enddate_to_date;
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='left' style='font-size:18pt; background-color:#fde9d9; border:none'><b>Individual Balanced Scorecard " + start_date.ToString("yyyy") + "</b></td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='left' style='font-size:14pt; background-color:#fde9d9; border:none'><b>Planning Period: </b>" + start_end_date + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' style='font-size:14pt; background-color:#fde9d9; border:none'></td>");
                        HttpContext.Current.Response.Write("</tr>");
                    }
                }

                using (SqlDataReader UserReader = sql_select_user_info.ExecuteReader())
                {
                    while (UserReader.Read())
                    {
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='left' style='font-size:12pt; background-color:#fde9d9; border:none'><b>Name: </b>" + UserReader["empName"].ToString() + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='left' style='font-size:12pt; background-color:#fde9d9; border:none'><b>NIK: </b>" + UserReader["EmpId"].ToString() + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='left' style='font-size:12pt; background-color:#fde9d9; border:none'><b>Organization: </b>" + UserReader["OrgName"].ToString() + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='left' style='font-size:12pt; background-color:#fde9d9; border:none'><b>Position Title: </b>" + UserReader["JobTtlName"].ToString() + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='left' style='font-size:12pt; background-color:#fde9d9; border:none'><b>Grade: </b>" + UserReader["empGrade"].ToString() + "</td>");
                        HttpContext.Current.Response.Write("</tr>");

                        HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + UserReader["empName"].ToString() + "(" + UserReader["EmpId"].ToString() + ") Individual Balanced Scorecard " + start_date.ToString("yyyy") + ".xls");
                    }
                }

                HttpContext.Current.Response.Write("<tr>");
                HttpContext.Current.Response.Write("<td colspan='12' style='font-size:14pt; border:none'></td>");
                HttpContext.Current.Response.Write("</tr>");

                //Menampilkan FINANCIAL MEASURES
                using (SqlDataReader GroupReader = sql_select_group_info.ExecuteReader())
                {
                    if (GroupReader.HasRows)
                    {
                        while (GroupReader.Read())
                        {
                            string select_detail_single = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + GroupReader["FinancialHeader_ID"] + " AND FinancialType='Single'";
                            string select_detail_share_zero = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + GroupReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked=0";//untuk error handling jika ada share yang belum di Link
                            SqlCommand sql_select_detail_single = new SqlCommand(select_detail_single, conn);
                            SqlCommand sql_select_detail_share_zero = new SqlCommand(select_detail_share_zero, conn);

                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td colspan='15' align='center' style='font-size:11pt; background-color:yellow'><b>Financial Measures</b></td>");
                            HttpContext.Current.Response.Write("</tr>");
                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td><b>Group</b></td>");
                            HttpContext.Current.Response.Write("<td colspan='14' align='left'>" + GroupReader["FinancialHeader_Group"] + "</td>");
                            HttpContext.Current.Response.Write("</tr>");
                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td><b>Stretch Rating</b></td>");
                            HttpContext.Current.Response.Write("<td colspan='14' align='left'>" + GroupReader["FinancialHeader_StretchRating"] + "%</td>");
                            HttpContext.Current.Response.Write("</tr>");

                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td colspan='15' style='border-left:none; border-right:none'></td>");
                            HttpContext.Current.Response.Write("</tr>");

                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>No.</b></td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Financial Measure</b></td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Target</b></td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Result</b></td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Rating</b></td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Weight (%)</b></td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Score</b></td>");
                            HttpContext.Current.Response.Write("<td align='center' colspan='8' style='vertical-align:middle'><b>Remarks</b></td>");
                            HttpContext.Current.Response.Write("</tr>");

                            //Financial Type = 'Single'
                            using (SqlDataReader SingleReader = sql_select_detail_single.ExecuteReader())
                            {
                                if (SingleReader.HasRows)
                                {
                                    while (SingleReader.Read())
                                    {
                                        HttpContext.Current.Response.Write("<tr>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + no_header + "</td>");
                                        HttpContext.Current.Response.Write("<td align='left' style='vertical-align:middle'>" + SingleReader["FinancialMeasure"] + "</td>");
                                        if (SingleReader["FinancialMeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(SingleReader["FinancialTarget"].ToString());
                                            int month_num_result = int.Parse(SingleReader["FinancialResult"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_target + "</td>");
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_result + "</td>");
                                        }
                                        else
                                        {
                                            if (SingleReader["FinancialMeasureBy"].ToString() == "Numbers")
                                            {
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SingleReader["FinancialTarget"] + "</td>");
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SingleReader["FinancialResult"] + "</td>");
                                            }
                                            else
                                            {
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SingleReader["FinancialTarget"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SingleReader["FinancialResult"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                            }
                                        }
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SingleReader["FinancialRating"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SingleReader["FinancialWeight"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SingleReader["FinancialScore"] + "%</td>");
                                        if (SingleReader["FinancialRemarks"].Equals(""))
                                        {
                                            HttpContext.Current.Response.Write("<td colspan='8' style='vertical-align:middle'></td>");
                                        }
                                        else
                                        {
                                            HttpContext.Current.Response.Write("<td colspan='8' style='vertical-align:middle'>" + SingleReader["FinancialRemarks"] + "</td>");
                                        }
                                        HttpContext.Current.Response.Write("</tr>");
                                        no_header++;
                                        total_header_weight = total_header_weight + float.Parse(SingleReader["FinancialWeight"].ToString());
                                        total_header_score = total_header_score + float.Parse(SingleReader["FinancialScore"].ToString());
                                    }
                                }
                            }

                            //Financial Type = 'Share' & Financial Linked = 0

                            using (SqlDataReader ShareZeroReader = sql_select_detail_share_zero.ExecuteReader())
                            {
                                if (ShareZeroReader.HasRows)
                                {
                                    while (ShareZeroReader.Read())
                                    {
                                        HttpContext.Current.Response.Write("<tr>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + no_header + "</td>");
                                        HttpContext.Current.Response.Write("<td align='left' style='vertical-align:middle'>" + ShareZeroReader["FinancialMeasure"] + "</td>");
                                        if (ShareZeroReader["FinancialMeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(ShareZeroReader["FinancialTarget"].ToString());
                                            int month_num_result = int.Parse(ShareZeroReader["FinancialResult"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_target + "</td>");
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_result + "</td>");
                                        }
                                        else
                                        {
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + ShareZeroReader["FinancialTarget"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + ShareZeroReader["FinancialResult"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                        }
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + ShareZeroReader["FinancialRating"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + ShareZeroReader["FinancialWeight"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + ShareZeroReader["FinancialScore"] + "%</td>");
                                        if (ShareZeroReader["FinancialRemarks"].Equals(""))
                                        {
                                            HttpContext.Current.Response.Write("<td colspan='8'></td>");
                                        }
                                        else
                                        {
                                            HttpContext.Current.Response.Write("<td colspan='8' style='vertical-align:middle'>" + ShareZeroReader["FinancialRemarks"] + "</td>");
                                        }
                                        HttpContext.Current.Response.Write("</tr>");
                                        no_header++;
                                        total_header_weight = total_header_weight + float.Parse(ShareZeroReader["FinancialWeight"].ToString());
                                        total_header_score = total_header_score + float.Parse(ShareZeroReader["FinancialScore"].ToString());
                                    }
                                }
                            }

                            //Financial Type = 'Share' & Financial Linked != 0
                            string string_distinct_link_value = "SELECT DISTINCT FinancialLinked FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + GroupReader["FinancialHeader_ID"] + "AND FinancialType='Share' AND FinancialLinked<>0";
                            SqlCommand sql_distinct_link_value = new SqlCommand(string_distinct_link_value, conn);
                            using (SqlDataReader DistinctReader = sql_distinct_link_value.ExecuteReader())//butuh agar item yang Linked menjadi teratur
                            {
                                if (DistinctReader.HasRows)
                                {
                                    while (DistinctReader.Read())
                                    {
                                        string select_detail_share_linked = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialLinked=" + DistinctReader["FinancialLinked"] + " AND FinancialHeader_ID=" + GroupReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked<>0";
                                        SqlCommand sql_select_detail_share_linked = new SqlCommand(select_detail_share_linked, conn);
                                        using (SqlDataReader ShareLinkedReader = sql_select_detail_share_linked.ExecuteReader())
                                        {
                                            if (ShareLinkedReader.HasRows)
                                            {
                                                int loop_number = 1;
                                                while (ShareLinkedReader.Read())
                                                {
                                                    string link_count = "SELECT COUNT(FinancialLinked) FROM FinancialMeasures_Detail WHERE FinancialLinked=" + ShareLinkedReader["FinancialLinked"] + " AND FinancialHeader_ID=" + GroupReader["FinancialHeader_ID"] + "";
                                                    SqlCommand sql_count = new SqlCommand(link_count, conn);
                                                    int rowspan = (int)sql_count.ExecuteScalar();
                                                    if (loop_number == 1)
                                                    {
                                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle' rowspan=" + rowspan + ">" + no_header + "</td>");
                                                    }
                                                    HttpContext.Current.Response.Write("<td align='left' style='vertical-align:middle'>" + ShareLinkedReader["FinancialMeasure"] + "</td>");
                                                    if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Month")
                                                    {
                                                        string month_name_target = "", month_name_result = "";
                                                        int month_num_target = int.Parse(ShareLinkedReader["FinancialTarget"].ToString());
                                                        int month_num_result = int.Parse(ShareLinkedReader["FinancialResult"].ToString());
                                                        month_name_target = ShowMonthNameTarget(month_num_target);
                                                        month_name_result = ShowMonthNameResult(month_num_result);
                                                        HttpContext.Current.Response.Write("<td align='center'>" + month_name_target + "</td>");
                                                        HttpContext.Current.Response.Write("<td align='center'>" + month_name_result + "</td>");
                                                    }
                                                    else
                                                    {
                                                        if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Numbers")
                                                        {
                                                            HttpContext.Current.Response.Write("<td align='center'>" + ShareLinkedReader["FinancialTarget"] + "</td>");
                                                            HttpContext.Current.Response.Write("<td align='center'>" + ShareLinkedReader["FinancialResult"] + "</td>");
                                                        }
                                                        else
                                                        {
                                                            HttpContext.Current.Response.Write("<td align='center'>" + ShareLinkedReader["FinancialTarget"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                            HttpContext.Current.Response.Write("<td align='center'>" + ShareLinkedReader["FinancialResult"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                        }
                                                    }
                                                    HttpContext.Current.Response.Write("<td>" + ShareLinkedReader["FinancialRating"] + "%</td>");
                                                    if (loop_number == 1)
                                                    {
                                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle' rowspan=" + rowspan + ">" + ShareLinkedReader["FinancialWeight"] + "%" + "</td>");
                                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle' rowspan=" + rowspan + ">" + ShareLinkedReader["FinancialScore"] + "%" + "</td>");
                                                        total_header_weight = total_header_weight + float.Parse(ShareLinkedReader["FinancialWeight"].ToString());
                                                        total_header_score = total_header_score + float.Parse(ShareLinkedReader["FinancialScore"].ToString());
                                                    }
                                                    if (ShareLinkedReader["FinancialRemarks"].Equals(""))
                                                    {
                                                        HttpContext.Current.Response.Write("<td colspan='8'></td>");
                                                    }
                                                    else
                                                    {
                                                        HttpContext.Current.Response.Write("<td colspan='8' style='vertical-align:middle'>" + ShareLinkedReader["FinancialRemarks"] + "</td>");
                                                    }
                                                    HttpContext.Current.Response.Write("</tr>");
                                                    loop_number++;
                                                }//end of While ShareReaderLinked Read
                                            }//end of if ShareReaderLinked Has Rows
                                        }//end of SqlDataReader ShareReaderLinked
                                        no_header++;
                                    }//end of While Distinct Reader Read
                                }//end of If Distinct Reader Has Rows
                            }//end of SqlDataReader DistinctReader

                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td colspan='5'></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>" + total_header_weight + "%</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>" + total_header_score + "%</b></td>");
                            HttpContext.Current.Response.Write("<td colspan='8'></td>");
                            HttpContext.Current.Response.Write("</tr>");
                        }
                    }
                    else
                    {
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='center' style='font-size:13pt; background-color:yellow'><b>Financial Measures</b></td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>No.</b></td>");
                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Financial Measure</b></td>");
                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Target</b></td>");
                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Result</b></td>");
                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Rating</b></td>");
                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Weight (%)</b></td>");
                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>Score</b></td>");
                        HttpContext.Current.Response.Write("<td align='center' colspan='8' style='vertical-align:middle'><b>Remarks</b></td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='center' style='font-size:13pt'>Measures Not Found</td>");
                        HttpContext.Current.Response.Write("</tr>");
                    }
                }//end of SqlDataReader GroupReader

                //Menampilkan INDIVIDUAL MEASURES --> NIK=100
                string string_select_individual_header = "SELECT * FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + " AND data_status='exist' AND user_id=" + user_id + "";
                SqlCommand sql_select_individual_header = new SqlCommand(string_select_individual_header, conn);
                using (SqlDataReader IndividualHeaderReader = sql_select_individual_header.ExecuteReader())
                {
                    HttpContext.Current.Response.Write("<tr>");
                    HttpContext.Current.Response.Write("<td colspan='15' align='center' style='background-color:yellow'><b>Individual Measures</b></td>");
                    HttpContext.Current.Response.Write("</tr>");

                    HttpContext.Current.Response.Write("<tr>");
                    HttpContext.Current.Response.Write("<td align='center'><b>No.</b></td>");
                    HttpContext.Current.Response.Write("<td align='center'><b>KPI</b></td>");
                    HttpContext.Current.Response.Write("<td align='center'><b>Target</b></td>");
                    HttpContext.Current.Response.Write("<td align='center'><b>Result</b></td>");
                    HttpContext.Current.Response.Write("<td align='center'><b>Rating</b></td>");
                    HttpContext.Current.Response.Write("<td align='center'><b>Weight (%)</b></td>");
                    HttpContext.Current.Response.Write("<td align='center'><b>Score</b></td>");
                    HttpContext.Current.Response.Write("<td align='center' colspan='8'><b>Objective</b></td>");
                    HttpContext.Current.Response.Write("</tr>");
                    if (IndividualHeaderReader.HasRows)
                    {
                        while (IndividualHeaderReader.Read())
                        {
                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td align='left' style='vertical-align:middle'>" + no_header_detail + "</td>");
                            HttpContext.Current.Response.Write("<td align='left' style='vertical-align:middle'  >" + IndividualHeaderReader["IndividualHeader_KPI"] + "</td>");

                            if (int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika Targetnya = 'Based on Schedule'
                            {
                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>Based On Schedule</td>");
                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'> - </td>");
                            }
                            else//jika Targetnya bukan -1
                            {
                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                {
                                    string month_name_target = "", month_name_result = "";
                                    int month_num_target = int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString());
                                    int month_num_result = int.Parse(IndividualHeaderReader["IndividualHeader_Result"].ToString());
                                    month_name_target = ShowMonthNameTarget(month_num_target);
                                    month_name_result = ShowMonthNameResult(month_num_result);
                                    HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_target + "</td>");
                                    HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_result + "</td>");
                                }
                                else
                                {
                                    if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                                    {
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + IndividualHeaderReader["IndividualHeader_Target"] + "</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + IndividualHeaderReader["IndividualHeader_Result"] + "</td>");
                                    }
                                    else
                                    {
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + IndividualHeaderReader["IndividualHeader_Target"] + " " + IndividualHeaderReader["IndividualHeader_MeasureBy"] + "</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + IndividualHeaderReader["IndividualHeader_Result"] + " " + IndividualHeaderReader["IndividualHeader_MeasureBy"] + "</td>");
                                    }
                                }
                            }

                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + IndividualHeaderReader["IndividualHeader_Rating"] + "%" + "</td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + IndividualHeaderReader["IndividualHeader_Weight"] + "%" + "</td>");
                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + IndividualHeaderReader["IndividualHeader_Score"] + "%" + "</td>");
                            HttpContext.Current.Response.Write("<td align='center' colspan='8'>" + IndividualHeaderReader["IndividualHeader_Objective"] + "</td>");
                            total_individual_weight = total_individual_weight + float.Parse(IndividualHeaderReader["IndividualHeader_Weight"].ToString());
                            total_individual_score = total_individual_score + float.Parse(IndividualHeaderReader["IndividualHeader_Score"].ToString());

                            //CODE untuk menampilkan yang ada SPECIFIC OBJECTIVE. Jika ada, tampilkan. Jika tidak ada, maka informasi tidak ditampilkan
                            string select_individual_detail = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualHeader_ID=" + IndividualHeaderReader["IndividualHeader_ID"] + " AND data_status='exist'";
                            SqlCommand sql_select_individual_detail = new SqlCommand(select_individual_detail, conn);
                            Object specific_objective = sql_select_individual_detail.ExecuteScalar();
                            if (specific_objective != null)//jika ada specific objective
                            {
                                using (SqlDataReader SpecificReader = sql_select_individual_detail.ExecuteReader())
                                {
                                    while (SpecificReader.Read())
                                    {
                                        HttpContext.Current.Response.Write("<tr align='center'>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + no_header_detail + "." + no_specific_obj + "</td>");
                                        HttpContext.Current.Response.Write("<td align='left' style='vertical-align:middle'>" + SpecificReader["IndividualDetail_Title"] + "</td>");
                                        if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(SpecificReader["IndividualDetail_Target"].ToString());
                                            int month_num_result = int.Parse(SpecificReader["IndividualDetail_Result"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_target + "</td>");
                                            HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + month_name_result + "</td>");
                                        }
                                        else
                                        {
                                            if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                                            {
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SpecificReader["IndividualDetail_Target"] + "</td>");
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SpecificReader["IndividualDetail_Result"] + "</td>");
                                            }
                                            else
                                            {
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SpecificReader["IndividualDetail_Target"] + " " + SpecificReader["IndividualDetail_MeasureBy"] + "</td>");
                                                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SpecificReader["IndividualDetail_Result"] + " " + SpecificReader["IndividualDetail_MeasureBy"] + "</td>");
                                            }
                                        }
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>" + SpecificReader["IndividualDetail_Rating"] + "%" + "</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>-</td>");
                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'>-</td>");
                                        HttpContext.Current.Response.Write("</tr>");
                                        no_specific_obj++;
                                    }//end of While SpecificReader Read
                                }//end of SqlDataReader SpecificReader
                            }
                            else if (int.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)//jika ada SO tetapi belum di-insert
                            {
                                HttpContext.Current.Response.Write("<tr><td align='center' style='vertical-align:middle' colspan='7'>No Specific Objectives</td></tr>");
                            }
                            no_header_detail++;
                            no_specific_obj = 1;
                        }//end of While IndividualHeaderReader
                    }
                    else
                    {
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='15' align='center'; vertical-align:middle'>No KPIs</td>");
                        HttpContext.Current.Response.Write("</tr>");
                    }
                }

                HttpContext.Current.Response.Write("<tr>");
                HttpContext.Current.Response.Write("<td colspan='5' align='center' style='background-color:#dbeef3; vertical-align:middle'><b>SUB TOTAL</b></td>");
                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>" + total_individual_weight + "%</b></td>");
                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>" + total_individual_score + "%</b></td>");
                HttpContext.Current.Response.Write("</tr>");

                total_weight = total_header_weight + total_individual_weight;
                total_score = total_header_score + total_individual_score;

                HttpContext.Current.Response.Write("<tr>");
                HttpContext.Current.Response.Write("<td colspan='5' align='center' style='background-color:#00b0f0; vertical-align:middle'><b>TOTAL SCORE</b></td>");
                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>" + total_weight + "%</b></td>");
                HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle'><b>" + total_score + "%</b></td>");
                HttpContext.Current.Response.Write("</tr>");

                HttpContext.Current.Response.Write("</Table>");
                HttpContext.Current.Response.Write("</font>");
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
                conn.Close();
            }
        }//end of OnClickExportExcel

        public string ShowMonthNameTarget(int target_value)
        {
            string month_name = "";
            switch (target_value)
            {
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

        public string ShowMonthNameResult(int result_value)
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