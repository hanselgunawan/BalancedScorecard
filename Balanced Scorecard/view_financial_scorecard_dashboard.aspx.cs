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
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.xml;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.end;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class view_financial_scorecard_dashboard : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        StringBuilder HtmlTable = new StringBuilder();//untuk DropDown
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
                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page
                var periode_id = Request.QueryString["period_id"];
                var prev_page = Request.QueryString["prev_page"];//untuk pagination
                var group_name = Request.QueryString["group_name"];
                var submit_id = Request.QueryString["submit_id"];
                var nik = Request.QueryString["nik"];
                var org = Request.QueryString["organization"];
                var adt_org = Request.QueryString["adt_organization"];
                var bsc_group = Request.QueryString["bsc_group"];
                var name = Request.QueryString["name"];

                int no_header = 1;
                string select_header = "";

                if (nik == null && org == null && adt_org == null && bsc_group == null && name == null)
                {
                    if (submit_id == "1")
                    {
                        LabelSubmit.Text = "View Submit Users";
                        LabelSubmitBreadcrumb.Text = "Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "");
                    }
                    else
                    {
                        LabelSubmit.Text = "View Not Submit Users";
                        LabelSubmitBreadcrumb.Text = "Not Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "");
                    }
                }
                else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null)
                {
                    if (submit_id == "1")
                    {
                        LabelSubmit.Text = "View Submit Users";
                        LabelSubmitBreadcrumb.Text = "Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&nik="+nik+"");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&nik="+nik+"");
                    }
                    else
                    {
                        LabelSubmit.Text = "View Not Submit Users";
                        LabelSubmitBreadcrumb.Text = "Not Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&nik="+nik+"");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&nik=" + nik + "");
                    }
                }
                else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null)
                {
                    if (submit_id == "1")
                    {
                        LabelSubmit.Text = "View Submit Users";
                        LabelSubmitBreadcrumb.Text = "Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&organization="+org+"");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&organization=" + org + "");
                    }
                    else
                    {
                        LabelSubmit.Text = "View Not Submit Users";
                        LabelSubmitBreadcrumb.Text = "Not Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&organization=" + org + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&organization=" + org + "");
                    }
                }
                else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null)
                {
                    if (submit_id == "1")
                    {
                        LabelSubmit.Text = "View Submit Users";
                        LabelSubmitBreadcrumb.Text = "Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&adt_organization=" + adt_org + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&adt_organization=" + adt_org + "");
                    }
                    else
                    {
                        LabelSubmit.Text = "View Not Submit Users";
                        LabelSubmitBreadcrumb.Text = "Not Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&adt_organization=" + adt_org + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&adt_organization=" + adt_org + "");
                    }
                }
                else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null)
                {
                    if (submit_id == "1")
                    {
                        LabelSubmit.Text = "View Submit Users";
                        LabelSubmitBreadcrumb.Text = "Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&bsc_group=" + bsc_group + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&bsc_group=" + bsc_group + "");
                    }
                    else
                    {
                        LabelSubmit.Text = "View Not Submit Users";
                        LabelSubmitBreadcrumb.Text = "Not Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&bsc_group=" + bsc_group + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&bsc_group=" + bsc_group + "");
                    }
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null)
                {
                    if (submit_id == "1")
                    {
                        LabelSubmit.Text = "View Submit Users";
                        LabelSubmitBreadcrumb.Text = "Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&name=" + name + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&name=" + name + "");
                    }
                    else
                    {
                        LabelSubmit.Text = "View Not Submit Users";
                        LabelSubmitBreadcrumb.Text = "Not Submit Users";
                        link_breadcrumb_view_submit_users.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&name=" + name + "");
                        btnBackToSubmitUsers.Attributes.Add("href", "view_no_submit_users.aspx?page=" + prev_page + "&period_id=" + periode_id + "&name=" + name + "");
                    }
                }

                link_breadcrumb_dashboard.Attributes.Add("href", "dashboard.aspx?period_id=" + periode_id + "");

                //((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page

                //untuk PLACEHOLDER Table//
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    int no_detail = 1;
                    int collapse_jscript = 1;

                    string select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + periode_id + "";
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                    SqlCommand sql_select_period = new SqlCommand(select_period, conn);
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
                        if (PeriodReader.HasRows)
                        {
                            while (PeriodReader.Read())
                            {
                                string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya tidak keluar
                                DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                                enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                                start_end_date = startdate_to_date + " - " + enddate_to_date;
                                LabelPeriod.Text = start_end_date;
                            }
                        }
                        else
                        {
                            LabelPeriod.Text = "Period Not Found";
                        }
                    }

                    select_header = "SELECT * FROM FinancialMeasures_Header WHERE FinancialHeader_Group='" + group_name + "' AND Period_ID=" + periode_id + "";

                    SqlCommand sql_select_header = new SqlCommand(select_header, conn);

                    using (SqlDataReader HeaderReader = sql_select_header.ExecuteReader())
                    {
                        if (HeaderReader.HasRows)//kalau data Header sudah terisi
                        {
                            HtmlTableData.Append("<tr><th class='centering-th2'>No.</th><th class='centering-th2'>Group</th><th class='centering-th2'>Stretch Rating</th><th class='centering-th2'>Review</th><th class='centering-th2'>Detail</th></tr>");
                            while (HeaderReader.Read())
                            {
                                HtmlTableData.Append("<tr align='center' style='border-bottom:1px solid #ddd'>");
                                HtmlTableData.Append("<td class='td-align'>" + no_header + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + HeaderReader["FinancialHeader_Group"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + HeaderReader["FinancialHeader_StretchRating"] + "%</td>");
                                HtmlTableData.Append("<td class='td-align'>" + HeaderReader["FinancialHeader_Review"] + "</td>");

                                //CODE FOR DETAIL (Single Type)
                                string select_detail = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + "AND FinancialType='Single' AND data_status='exist'";
                                SqlCommand sql_select_detail = new SqlCommand(select_detail, conn);//buat check null tan
                                Object output = sql_select_detail.ExecuteScalar();
                                if (output != null)//jika tidak ada detail, berarti tidak ada Collapse
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='collapsed' data-toggle='collapse' href='#collapse" + collapse_jscript + "' aria-expanded='false' aria-controls='collapse+" + collapse_jscript + "'>See Details</a></td>");
                                }
                                else
                                {
                                    HtmlTableData.Append("<td class='td-align'>No Detail</td>");
                                }
                                //END OF CODE FOR DETAIL//

                                if (output != null)//kalau ada detail
                                {
                                    float total_weight = 0, total_score = 0;//keluarin yang SINGLE dahulu
                                    string select_single_type = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + "AND FinancialType='Single' AND data_status='exist'";
                                    SqlCommand sql_select_single_type = new SqlCommand(select_single_type, conn);
                                    using (SqlDataReader sdr_collapse = sql_select_single_type.ExecuteReader())
                                    {
                                        HtmlTableData.Append("<tr>");
                                        HtmlTableData.Append("<td colspan='9' style='padding:0'>");
                                        HtmlTableData.Append("<div id='collapse" + collapse_jscript + "' class='panel-collapse collapse' role='tabpanel'>");
                                        HtmlTableData.Append("<table class='table table-bordered' style='margin-bottom:0px'>");
                                        HtmlTableData.Append("<tr><th class='centering-th'>No.</th><th class='centering-th'>Financial Measures</th><th class='centering-th'>Financial Type</th><th class='centering-th'>Target</th><th class='centering-th'>Results</th><th class='centering-th'>Rating</th><th class='centering-th'>Weight (%)</th><th class='centering-th'>Score</th><th class='centering-th'>Formula</th><th class='centering-th'>Remarks</th></tr>");

                                        if (sdr_collapse.HasRows)
                                        {

                                            while (sdr_collapse.Read())
                                            {
                                                HtmlTableData.Append("<tr align='center'>");
                                                HtmlTableData.Append("<td class='td-align'>" + no_header + "." + no_detail + "</td>");
                                                HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialMeasure"] + "</td>");
                                                HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialType"] + "</td>");
                                                if (sdr_collapse["FinancialMeasureBy"].ToString() == "Month")
                                                {
                                                    string month_name_target = "", month_name_result = "";
                                                    int month_num_target = int.Parse(sdr_collapse["FinancialTarget"].ToString());
                                                    int month_num_result = int.Parse(sdr_collapse["FinancialResult"].ToString());
                                                    month_name_target = ShowMonthNameTarget(month_num_target);
                                                    month_name_result = ShowMonthNameResult(month_num_result);
                                                    HtmlTableData.Append("<td class='td-align'>" + month_name_target + "</td>");
                                                    HtmlTableData.Append("<td class='td-align'>" + month_name_result + "</td>");
                                                }
                                                else
                                                {
                                                    HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialTarget"] + " " + sdr_collapse["FinancialMeasureBy"] + "</td>");
                                                    HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialResult"] + " " + sdr_collapse["FinancialMeasureBy"] + "</td>");
                                                }
                                                HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialRating"] + "%" + "</td>");
                                                HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialWeight"] + "%" + "</td>");
                                                HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialScore"] + "%" + "</td>");
                                                HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialFormula"] + "</td>");

                                                if (sdr_collapse["FinancialRemarks"].ToString() == "")//cek apakah ada Remarks atau tidak. NULL-nya harus diganti dengan ""
                                                {
                                                    HtmlTableData.Append("<td class='td-align'>No Remarks</td>");
                                                }
                                                else
                                                {
                                                    HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialRemarks"] + "</td>");
                                                }

                                                HtmlTableData.Append("</tr>");
                                                no_detail++;
                                                total_weight = total_weight + float.Parse(sdr_collapse["FinancialWeight"].ToString());
                                                total_score = total_score + float.Parse(sdr_collapse["FinancialScore"].ToString());
                                            }
                                        }
                                    }
                                    string string_share_type = "SELECT DISTINCT FinancialLinked FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + "AND FinancialType='Share' AND data_status='exist'";
                                    SqlCommand sql_share_type = new SqlCommand(string_share_type, conn);
                                    using (SqlDataReader ShareReader = sql_share_type.ExecuteReader())
                                    {
                                        while (ShareReader.Read())
                                        {
                                            String select_same_link = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialLinked=" + ShareReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND data_status='exist'";
                                            SqlCommand sql_same_link = new SqlCommand(select_same_link, conn);
                                            using (SqlDataReader LinkReader = sql_same_link.ExecuteReader())
                                            {
                                                int loop = 1;//agar nomor muncul untuk data pertama saja!
                                                while (LinkReader.Read())
                                                {
                                                    int linked_id = int.Parse(LinkReader["FinancialLinked"].ToString());
                                                    if (linked_id == 0)//untuk munculin yang FinancialLinked-nya 0 terlebih dahulu!
                                                    {
                                                        HtmlTableData.Append("<tr align='center'>");
                                                        HtmlTableData.Append("<td class='td-align'>" + no_header + "." + no_detail + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialMeasure"] + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialType"] + "</td>");
                                                        if (LinkReader["FinancialMeasureBy"].ToString() == "Month")
                                                        {
                                                            string month_name_target = "", month_name_result = "";
                                                            int month_num_target = int.Parse(LinkReader["FinancialTarget"].ToString());
                                                            int month_num_result = int.Parse(LinkReader["FinancialResult"].ToString());
                                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                                            month_name_result = ShowMonthNameResult(month_num_result);
                                                            HtmlTableData.Append("<td class='td-align'>" + month_name_target + "</td>");
                                                            HtmlTableData.Append("<td class='td-align'>" + month_name_result + "</td>");
                                                        }
                                                        else
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialTarget"] + " " + LinkReader["FinancialMeasureBy"] + "</td>");
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialResult"] + " " + LinkReader["FinancialMeasureBy"] + "</td>");
                                                        }
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialRating"] + "%" + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialWeight"] + "%" + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialScore"] + "%" + "</td>");
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialFormula"] + "</td>");
                                                        if (LinkReader["FinancialRemarks"].ToString() == "")//cek apakah ada Remarks atau tidak. NULL-nya harus diganti dengan ""
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>No Remarks</td>");
                                                        }
                                                        else
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialRemarks"] + "</td>");
                                                        }

                                                        total_weight = total_weight + float.Parse(LinkReader["FinancialWeight"].ToString());
                                                        total_score = total_score + float.Parse(LinkReader["FinancialScore"].ToString());
                                                        no_detail++;
                                                    }
                                                    else//untuk munculin yang FinancialLinked != 0
                                                    {
                                                        string link_count = "SELECT COUNT(FinancialLinked) FROM FinancialMeasures_Detail WHERE FinancialLinked=" + ShareReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND data_status='exist'";
                                                        SqlCommand sql_count = new SqlCommand(link_count, conn);
                                                        int rowspan = (int)sql_count.ExecuteScalar();
                                                        HtmlTableData.Append("<tr align='center'>");
                                                        if (loop == 1)
                                                        {
                                                            HtmlTableData.Append("<td class='td-align' rowspan=" + rowspan + ">" + no_header + "." + no_detail + "</td>");
                                                            no_detail++;
                                                        }
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialMeasure"] + "</td>");
                                                        if (loop == 1)
                                                        {
                                                            HtmlTableData.Append("<td class='td-align' rowspan=" + rowspan + ">" + LinkReader["FinancialType"] + "</td>");
                                                        }

                                                        if (LinkReader["FinancialMeasureBy"].ToString() == "Month")
                                                        {
                                                            string month_name_target = "", month_name_result = "";
                                                            int month_num_target = int.Parse(LinkReader["FinancialTarget"].ToString());
                                                            int month_num_result = int.Parse(LinkReader["FinancialResult"].ToString());
                                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                                            month_name_result = ShowMonthNameResult(month_num_result);
                                                            HtmlTableData.Append("<td class='td-align'>" + month_name_target + "</td>");
                                                            HtmlTableData.Append("<td class='td-align'>" + month_name_result + "</td>");
                                                        }
                                                        else
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialTarget"] + " " + LinkReader["FinancialMeasureBy"] + "</td>");
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialResult"] + " " + LinkReader["FinancialMeasureBy"] + "</td>");
                                                        }

                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialRating"] + "%" + "</td>");
                                                        if (loop == 1)
                                                        {
                                                            HtmlTableData.Append("<td class='td-align' rowspan=" + rowspan + ">" + LinkReader["FinancialWeight"] + "%" + "</td>");
                                                            HtmlTableData.Append("<td class='td-align' rowspan=" + rowspan + ">" + LinkReader["FinancialScore"] + "%" + "</td>");
                                                            total_weight = total_weight + float.Parse(LinkReader["FinancialWeight"].ToString());
                                                            total_score = total_score + float.Parse(LinkReader["FinancialScore"].ToString());
                                                        }
                                                        HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialFormula"] + "</td>");
                                                        if (LinkReader["FinancialRemarks"].Equals(""))//cek apakah ada Remarks atau tidak. NULL-nya harus diganti dengan ""
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>No Remarks</td>");
                                                        }
                                                        else
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialRemarks"] + "</td>");
                                                        }
                                                        loop++;
                                                    }
                                                }
                                            }

                                        }
                                    }
                                    HtmlTableData.Append("</tr>");
                                    HtmlTableData.Append("<tr align='center'>");
                                    HtmlTableData.Append("<td colspan=6><b>TOTAL SCORE</b></td>");
                                    HtmlTableData.Append("<td><b>" + total_weight + "%</b></td>");
                                    HtmlTableData.Append("<td class='td-align'><b>" + total_score + "%</b></td>");
                                    HtmlTableData.Append("</tr>");
                                    no_detail = 1;

                                    HtmlTableData.Append("</table>");
                                    HtmlTableData.Append("</div>");
                                    HtmlTableData.Append("</td>");
                                    HtmlTableData.Append("</tr>");
                                }
                                no_header++;
                                collapse_jscript++;
                            }
                        }
                        else//jika Financial Measure TIDAK DITEMUKAN
                        {
                            HtmlTableData.Append("<tr><th class='centering-th2'>There is no data to display</th></tr>");
                        }
                    }
                    conn.Close();
                }
                PlaceHolder2.Controls.Add(new Literal { Text = HtmlTableData.ToString() }); //untuk Table
            }
        }//end of Page_Load

        

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