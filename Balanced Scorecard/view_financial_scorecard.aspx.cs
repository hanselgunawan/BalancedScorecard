﻿using System;
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
    public partial class view_financial_scorecard : System.Web.UI.Page
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
                var filter = Request.QueryString["filter"];

                int no_header = 1;
                string select_header = "";

                if (filter == null)
                {
                    link_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + prev_page + "&period_id=" + periode_id + "");
                    btnBackToScorecardUsers.Attributes.Add("href", "scorecard_user.aspx?page=" + prev_page + "&period_id=" + periode_id + "");
                }
                else
                {
                    link_breadcrumb.Attributes.Add("href", "scorecard_user.aspx?page=" + prev_page + "&period_id=" + periode_id + "&filter=" + filter + "");
                    btnBackToScorecardUsers.Attributes.Add("href", "scorecard_user.aspx?page=" + prev_page + "&period_id=" + periode_id + "&filter=" + filter + "");
                }

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
                                                else if (sdr_collapse["FinancialMeasureBy"].ToString() == "Numbers")
                                                {
                                                    HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialTarget"].ToString() + "</td>");
                                                    HtmlTableData.Append("<td class='td-align'>" + sdr_collapse["FinancialResult"].ToString() + "</td>");
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
                                                        else if (LinkReader["FinancialMeasureBy"].ToString() == "Numbers")
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialTarget"].ToString() + "</td>");
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialResult"].ToString() + "</td>");
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
                                                        else if (LinkReader["FinancialMeasureBy"].ToString() == "Numbers")
                                                        {
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialTarget"].ToString() + "</td>");
                                                            HtmlTableData.Append("<td class='td-align'>" + LinkReader["FinancialResult"].ToString() + "</td>");
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

        protected void OnClickExportExcel(object sender, EventArgs e)
        {
            int no_header = 1;
            var period_id = Request.QueryString["period_id"];
            var group_name = Request.QueryString["group_name"];
            object period_id_active;
            string select_active_period_id = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
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
                string select_header = "SELECT * FROM FinancialMeasures_Header WHERE Period_ID=" + period_id + " AND data_status='exist' AND FinancialHeader_Group='" + group_name + "'";
                SqlCommand sql_select_period_date = new SqlCommand(select_period_date, conn);
                SqlCommand sql_select_header = new SqlCommand(select_header, conn);

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
                        string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya tidak keluar
                        DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                        DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                        startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                        enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                        start_end_date = startdate_to_date + " - " + enddate_to_date;
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='8' align='left' style='font-size:18pt; background-color:#FFC'><b>FINANCIAL MEASURES " + start_date.ToString("yyyy") + "</b></td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td colspan='8' align='left' style='font-size:14pt; background-color:#FFC'><b>Planning Period: </b>" + start_end_date + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=Financial Measures " + start_date.ToString("yyyy") + ".xls");
                    }
                }

                HttpContext.Current.Response.Write("<tr></tr>");

                using (SqlDataReader HeaderReader = sql_select_header.ExecuteReader())
                {
                    if (HeaderReader.HasRows)
                    {
                        while (HeaderReader.Read())
                        {
                            float total_weight = 0, total_score = 0;
                            string select_detail_single = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Single' AND data_status='exist'";
                            string select_detail_share_zero = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked=0 AND data_status='exist'";//untuk error handling jika ada share yang belum di Link
                            SqlCommand sql_select_detail_single = new SqlCommand(select_detail_single, conn);
                            SqlCommand sql_select_detail_share_zero = new SqlCommand(select_detail_share_zero, conn);

                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td><b>Group</b></td>");
                            HttpContext.Current.Response.Write("<td align='left' colspan='7'>" + HeaderReader["FinancialHeader_Group"] + "</td>");
                            HttpContext.Current.Response.Write("</tr>");
                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td><b>Stretch Rating</b></td>");
                            HttpContext.Current.Response.Write("<td align='left' colspan='7'>" + HeaderReader["FinancialHeader_StretchRating"] + "%</td>");
                            HttpContext.Current.Response.Write("</tr>");

                            HttpContext.Current.Response.Write("<tr>");
                            HttpContext.Current.Response.Write("<td align='center'><b>No.</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>Financial Measure</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>Target</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>Result</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>Rating</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>Weight (%)</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>Score</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>Remarks</b></td>");
                            HttpContext.Current.Response.Write("</tr>");

                            //Financial Type = 'Single'
                            using (SqlDataReader SingleReader = sql_select_detail_single.ExecuteReader())
                            {
                                if (SingleReader.HasRows)
                                {
                                    while (SingleReader.Read())
                                    {
                                        HttpContext.Current.Response.Write("<tr>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + no_header + "</td>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + SingleReader["FinancialMeasure"] + "</td>");
                                        if (SingleReader["FinancialMeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(SingleReader["FinancialTarget"].ToString());
                                            int month_num_result = int.Parse(SingleReader["FinancialResult"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            HttpContext.Current.Response.Write("<td align='right'>" + month_name_target + "</td>");
                                            HttpContext.Current.Response.Write("<td align='right'>" + month_name_result + "</td>");
                                        }
                                        else if (SingleReader["FinancialMeasureBy"].ToString() == "Numbers")
                                        {
                                            HttpContext.Current.Response.Write("<td align='right'>" + SingleReader["FinancialTarget"].ToString() + "</td>");
                                            HttpContext.Current.Response.Write("<td align='right'>" + SingleReader["FinancialResult"].ToString() + "</td>");
                                        }
                                        else
                                        {
                                            HttpContext.Current.Response.Write("<td align='right'>" + SingleReader["FinancialTarget"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                            HttpContext.Current.Response.Write("<td align='right'>" + SingleReader["FinancialResult"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                        }
                                        HttpContext.Current.Response.Write("<td>" + SingleReader["FinancialRating"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + SingleReader["FinancialWeight"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + SingleReader["FinancialScore"] + "%</td>");
                                        if (SingleReader["FinancialRemarks"].Equals(""))
                                        {
                                            HttpContext.Current.Response.Write("<td></td>");
                                        }
                                        else
                                        {
                                            HttpContext.Current.Response.Write("<td>" + SingleReader["FinancialRemarks"] + "</td>");
                                        }
                                        HttpContext.Current.Response.Write("</tr>");
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
                                        HttpContext.Current.Response.Write("<tr>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + no_header + "</td>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + ShareZeroReader["FinancialMeasure"] + "</td>");
                                        if (ShareZeroReader["FinancialMeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(ShareZeroReader["FinancialTarget"].ToString());
                                            int month_num_result = int.Parse(ShareZeroReader["FinancialResult"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            HttpContext.Current.Response.Write("<td align='right'>" + month_name_target + "</td>");
                                            HttpContext.Current.Response.Write("<td align='right'>" + month_name_result + "</td>");
                                        }
                                        else if (ShareZeroReader["FinancialMeasureBy"].ToString() == "Numbers")
                                        {
                                            HttpContext.Current.Response.Write("<td align='right'>" + ShareZeroReader["FinancialTarget"].ToString() + "</td>");
                                            HttpContext.Current.Response.Write("<td align='right'>" + ShareZeroReader["FinancialResult"].ToString() + "</td>");
                                        }
                                        else
                                        {
                                            HttpContext.Current.Response.Write("<td align='right'>" + ShareZeroReader["FinancialTarget"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                            HttpContext.Current.Response.Write("<td align='right'>" + ShareZeroReader["FinancialResult"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                        }
                                        HttpContext.Current.Response.Write("<td>" + ShareZeroReader["FinancialRating"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + ShareZeroReader["FinancialWeight"] + "%</td>");
                                        HttpContext.Current.Response.Write("<td align='center'>" + ShareZeroReader["FinancialScore"] + "%</td>");
                                        if (ShareZeroReader["FinancialRemarks"].Equals(""))
                                        {
                                            HttpContext.Current.Response.Write("<td></td>");
                                        }
                                        else
                                        {
                                            HttpContext.Current.Response.Write("<td>" + ShareZeroReader["FinancialRemarks"] + "</td>");
                                        }
                                        HttpContext.Current.Response.Write("</tr>");
                                        no_header++;
                                        total_weight = total_weight + float.Parse(ShareZeroReader["FinancialWeight"].ToString());
                                        total_score = total_score + float.Parse(ShareZeroReader["FinancialScore"].ToString());
                                    }
                                }
                            }

                            //Financial Type = 'Share' & Financial Linked != 0
                            string string_distinct_link_value = "SELECT DISTINCT FinancialLinked FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + "AND FinancialType='Share' AND FinancialLinked<>0 AND data_status='exist'";
                            SqlCommand sql_distinct_link_value = new SqlCommand(string_distinct_link_value, conn);
                            using (SqlDataReader DistinctReader = sql_distinct_link_value.ExecuteReader())//butuh agar item yang Linked menjadi teratur
                            {
                                if (DistinctReader.HasRows)
                                {
                                    while (DistinctReader.Read())
                                    {
                                        string select_detail_share_linked = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialLinked=" + DistinctReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked<>0 AND data_status='exist'";
                                        SqlCommand sql_select_detail_share_linked = new SqlCommand(select_detail_share_linked, conn);
                                        using (SqlDataReader ShareLinkedReader = sql_select_detail_share_linked.ExecuteReader())
                                        {
                                            if (ShareLinkedReader.HasRows)
                                            {
                                                int loop_number = 1;
                                                while (ShareLinkedReader.Read())
                                                {
                                                    string link_count = "SELECT COUNT(FinancialLinked) FROM FinancialMeasures_Detail WHERE FinancialLinked=" + ShareLinkedReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND data_status='exist'";
                                                    SqlCommand sql_count = new SqlCommand(link_count, conn);
                                                    int rowspan = (int)sql_count.ExecuteScalar();
                                                    if (loop_number == 1)
                                                    {
                                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle' rowspan=" + rowspan + ">" + no_header + "</td>");
                                                    }
                                                    HttpContext.Current.Response.Write("<td align='center'>" + ShareLinkedReader["FinancialMeasure"] + "</td>");
                                                    if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Month")
                                                    {
                                                        string month_name_target = "", month_name_result = "";
                                                        int month_num_target = int.Parse(ShareLinkedReader["FinancialTarget"].ToString());
                                                        int month_num_result = int.Parse(ShareLinkedReader["FinancialResult"].ToString());
                                                        month_name_target = ShowMonthNameTarget(month_num_target);
                                                        month_name_result = ShowMonthNameResult(month_num_result);
                                                        HttpContext.Current.Response.Write("<td align='right'>" + month_name_target + "</td>");
                                                        HttpContext.Current.Response.Write("<td align='right'>" + month_name_result + "</td>");
                                                    }
                                                    else if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Numbers")
                                                    {
                                                        HttpContext.Current.Response.Write("<td align='right'>" + ShareLinkedReader["FinancialTarget"].ToString() + "</td>");
                                                        HttpContext.Current.Response.Write("<td align='right'>" + ShareLinkedReader["FinancialResult"].ToString() + "</td>");
                                                    }
                                                    else
                                                    {
                                                        HttpContext.Current.Response.Write("<td align='right'>" + ShareLinkedReader["FinancialTarget"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                        HttpContext.Current.Response.Write("<td align='right'>" + ShareLinkedReader["FinancialResult"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                    }
                                                    HttpContext.Current.Response.Write("<td>" + ShareLinkedReader["FinancialRating"] + "%</td>");
                                                    if (loop_number == 1)
                                                    {
                                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle' rowspan=" + rowspan + ">" + ShareLinkedReader["FinancialWeight"] + "%" + "</td>");
                                                        HttpContext.Current.Response.Write("<td align='center' style='vertical-align:middle' rowspan=" + rowspan + ">" + ShareLinkedReader["FinancialScore"] + "%" + "</td>");
                                                        total_weight = total_weight + float.Parse(ShareLinkedReader["FinancialWeight"].ToString());
                                                        total_score = total_score + float.Parse(ShareLinkedReader["FinancialScore"].ToString());
                                                    }
                                                    if (ShareLinkedReader["FinancialRemarks"].Equals(""))
                                                    {
                                                        HttpContext.Current.Response.Write("<td></td>");
                                                    }
                                                    else
                                                    {
                                                        HttpContext.Current.Response.Write("<td>" + ShareLinkedReader["FinancialRemarks"] + "</td>");
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
                            HttpContext.Current.Response.Write("<td colspan='5' align='center' style='background-color:yellow'><b>TOTAL</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>" + total_weight + "%</b></td>");
                            HttpContext.Current.Response.Write("<td align='center'><b>" + total_score + "%</b></td>");
                            HttpContext.Current.Response.Write("<td></td>");
                            HttpContext.Current.Response.Write("</tr>");
                            no_header = 1;//reset nomor header
                        }//end of while HeaderReader
                    }
                    else//error handling jika Group Name yang ada, belum di Setting oleh Admin
                    {
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td><b>Group</b></td>");
                        HttpContext.Current.Response.Write("<td align='left' colspan='7'>" + group_name + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td><b>Stretch Rating</b></td>");
                        HttpContext.Current.Response.Write("<td align='left' colspan='7'>Unknown</td>");
                        HttpContext.Current.Response.Write("</tr>");

                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td align='center'><b>No.</b></td>");
                        HttpContext.Current.Response.Write("<td align='center'><b>Financial Measure</b></td>");
                        HttpContext.Current.Response.Write("<td align='center'><b>Target</b></td>");
                        HttpContext.Current.Response.Write("<td align='center'><b>Result</b></td>");
                        HttpContext.Current.Response.Write("<td align='center'><b>Rating</b></td>");
                        HttpContext.Current.Response.Write("<td align='center'><b>Weight (%)</b></td>");
                        HttpContext.Current.Response.Write("<td align='center'><b>Score</b></td>");
                        HttpContext.Current.Response.Write("<td align='center'><b>Remarks</b></td>");
                        HttpContext.Current.Response.Write("</tr>");

                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td align='center' colspan='8'>No Financial Measures</td>");
                        HttpContext.Current.Response.Write("</tr>");
                    }
                }

                HttpContext.Current.Response.Write("</Table>");
                HttpContext.Current.Response.Write("</font>");
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
                conn.Close();
            }
        }//end of OnClickExportExcel

        protected void OnClickExportPDF(object sender, EventArgs e)//Export ke PDF
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
            string check_reserved_char;//digunakan unuk mengecek XML Reserved Characters
            var period_id = Request.QueryString["id"];
            var group_name = Request.QueryString["group_name"];
            object period_id_active;
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
                string select_header = "SELECT * FROM FinancialMeasures_Header WHERE Period_ID=" + period_id + " AND data_status='exist' AND FinancialHeader_Group='" + group_name + "'";
                SqlCommand sql_select_period_date = new SqlCommand(select_period_date, conn);
                SqlCommand sql_select_header = new SqlCommand(select_header, conn);

                DataTable.Append("<p style='text-align:center; font-size:50px'>PT. Matahari Putra Prima Tbk.</p>");

                using (SqlDataReader PeriodReader = sql_select_period_date.ExecuteReader())
                {
                    while (PeriodReader.Read())
                    {
                        string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya tidak keluar
                        DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                        DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                        startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                        enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                        start_end_date = startdate_to_date + " - " + enddate_to_date;
                        DataTable.Append("<p style='text-align:center; font-size:30px'>FINANCIAL MEASURES " + start_date.ToString("yyyy") + "</p>");
                        DataTable.Append("<p style='text-align:center; font-size:30px; page-break-after: always'>(" + start_end_date + ")</p>");
                        Response.AddHeader("content-disposition", "attachment;filename=Financial Measures " + start_date.ToString("yyyy") + ".pdf");
                        pdfDoc.Add(png);
                    }
                }

                using (SqlDataReader HeaderReader = sql_select_header.ExecuteReader())
                {
                    if (HeaderReader.HasRows)
                    {
                        while (HeaderReader.Read())
                        {
                            float total_weight = 0, total_score = 0;
                            string select_detail_single = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Single' AND data_status='exist'";
                            string select_detail_share_zero = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked=0 AND data_status='exist'";//untuk error handling jika ada share yang belum di Link
                            SqlCommand sql_select_detail_single = new SqlCommand(select_detail_single, conn);
                            SqlCommand sql_select_detail_share_zero = new SqlCommand(select_detail_share_zero, conn);

                            DataTable.Append("<table align='center' width='100%' style='border-collapse:collapse; text-align:center; font-size:10pt; page-break-after: always'>");
                            DataTable.Append("<tr>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow' colspan='2'><b>Group</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'>" + HeaderReader["FinancialHeader_Group"] + "</td>");
                            DataTable.Append("</tr>");
                            DataTable.Append("<tr>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow' colspan='2'><b>Stretch Rating</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'>" + HeaderReader["FinancialHeader_StretchRating"] + "%</td>");
                            DataTable.Append("</tr>");

                            DataTable.Append("<tr>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>No.</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Financial Measure</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Target</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Result</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Rating</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Weight (%)</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Score</b></td>");
                            DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Remarks</b></td>");
                            DataTable.Append("</tr>");

                            //Financial Type = 'Single'
                            using (SqlDataReader SingleReader = sql_select_detail_single.ExecuteReader())
                            {
                                if (SingleReader.HasRows)
                                {
                                    while (SingleReader.Read())
                                    {
                                        DataTable.Append("<tr>");
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + no_header + "</td>");

                                        //cek apakah ada XML Reserved Characters
                                        check_reserved_char = SingleReader["FinancialMeasure"].ToString();
                                        if (check_reserved_char.Contains("&")) check_reserved_char = check_reserved_char.Replace("&", "&amp;");

                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + check_reserved_char + "</td>");
                                        if (SingleReader["FinancialMeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(SingleReader["FinancialTarget"].ToString());
                                            int month_num_result = int.Parse(SingleReader["FinancialResult"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                        }
                                        else if (SingleReader["FinancialMeasureBy"].ToString() == "Numbers")
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialTarget"].ToString() + "</td>");
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialResult"].ToString() + "</td>");
                                        }
                                        else
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialTarget"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialResult"] + " " + SingleReader["FinancialMeasureBy"] + "</td>");
                                        }
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialRating"] + "%</td>");
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialWeight"] + "%</td>");
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialScore"] + "%</td>");
                                        if (SingleReader["FinancialRemarks"].Equals(""))
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'></td>");
                                        }
                                        else
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + SingleReader["FinancialRemarks"] + "</td>");
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
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + no_header + "</td>");

                                        //cek apakah ada XML Reserved Characters
                                        check_reserved_char = ShareZeroReader["FinancialMeasure"].ToString();
                                        if (check_reserved_char.Contains("&")) check_reserved_char = check_reserved_char.Replace("&", "&amp;");

                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + check_reserved_char + "</td>");
                                        if (ShareZeroReader["FinancialMeasureBy"].ToString() == "Month")
                                        {
                                            string month_name_target = "", month_name_result = "";
                                            int month_num_target = int.Parse(ShareZeroReader["FinancialTarget"].ToString());
                                            int month_num_result = int.Parse(ShareZeroReader["FinancialResult"].ToString());
                                            month_name_target = ShowMonthNameTarget(month_num_target);
                                            month_name_result = ShowMonthNameResult(month_num_result);
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                        }
                                        else if (ShareZeroReader["FinancialMeasureBy"].ToString() == "Numbers")
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialTarget"].ToString() + "</td>");
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialResult"].ToString() + "</td>");
                                        }
                                        else
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialTarget"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialResult"] + " " + ShareZeroReader["FinancialMeasureBy"] + "</td>");
                                        }
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialRating"] + "%</td>");
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialWeight"] + "%</td>");
                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialScore"] + "%</td>");
                                        if (ShareZeroReader["FinancialRemarks"].Equals(""))
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'></td>");
                                        }
                                        else
                                        {
                                            DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareZeroReader["FinancialRemarks"] + "</td>");
                                        }
                                        DataTable.Append("</tr>");
                                        no_header++;
                                        total_weight = total_weight + float.Parse(ShareZeroReader["FinancialWeight"].ToString());
                                        total_score = total_score + float.Parse(ShareZeroReader["FinancialScore"].ToString());
                                    }
                                }
                            }

                            //Financial Type = 'Share' & Financial Linked != 0
                            string string_distinct_link_value = "SELECT DISTINCT FinancialLinked FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + "AND FinancialType='Share' AND FinancialLinked<>0 AND data_status='exist'";
                            SqlCommand sql_distinct_link_value = new SqlCommand(string_distinct_link_value, conn);
                            using (SqlDataReader DistinctReader = sql_distinct_link_value.ExecuteReader())//butuh agar item yang Linked menjadi teratur
                            {
                                if (DistinctReader.HasRows)
                                {
                                    while (DistinctReader.Read())
                                    {
                                        string select_detail_share_linked = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialLinked=" + DistinctReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND FinancialType='Share' AND FinancialLinked<>0 AND data_status='exist'";
                                        SqlCommand sql_select_detail_share_linked = new SqlCommand(select_detail_share_linked, conn);
                                        using (SqlDataReader ShareLinkedReader = sql_select_detail_share_linked.ExecuteReader())
                                        {
                                            if (ShareLinkedReader.HasRows)
                                            {
                                                int loop_number = 1;
                                                while (ShareLinkedReader.Read())
                                                {
                                                    string link_count = "SELECT COUNT(FinancialLinked) FROM FinancialMeasures_Detail WHERE FinancialLinked=" + ShareLinkedReader["FinancialLinked"] + " AND FinancialHeader_ID=" + HeaderReader["FinancialHeader_ID"] + " AND data_status='exist'";
                                                    SqlCommand sql_count = new SqlCommand(link_count, conn);
                                                    int rowspan = (int)sql_count.ExecuteScalar();
                                                    DataTable.Append("<tr>");
                                                    if (loop_number == 1)
                                                    {
                                                        DataTable.Append("<td style='vertical-align:middle; padding:10px; border:1px solid black' rowspan='" + rowspan + "'>" + no_header + "</td>");
                                                    }

                                                    //cek apakah ada XML Reserved Characters
                                                    check_reserved_char = ShareLinkedReader["FinancialMeasure"].ToString();
                                                    if (check_reserved_char.Contains("&")) check_reserved_char = check_reserved_char.Replace("&", "&amp;");

                                                    DataTable.Append("<td style='padding:10px; border:1px solid black'>" + check_reserved_char + "</td>");
                                                    if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Month")
                                                    {
                                                        string month_name_target = "", month_name_result = "";
                                                        int month_num_target = int.Parse(ShareLinkedReader["FinancialTarget"].ToString());
                                                        int month_num_result = int.Parse(ShareLinkedReader["FinancialResult"].ToString());
                                                        month_name_target = ShowMonthNameTarget(month_num_target);
                                                        month_name_result = ShowMonthNameResult(month_num_result);
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + month_name_target + "</td>");
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + month_name_result + "</td>");
                                                    }
                                                    else if (ShareLinkedReader["FinancialMeasureBy"].ToString() == "Numbers")
                                                    {
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialTarget"].ToString() + "</td>");
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialResult"].ToString() + "</td>");
                                                    }
                                                    else
                                                    {
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialTarget"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialResult"] + " " + ShareLinkedReader["FinancialMeasureBy"] + "</td>");
                                                    }
                                                    DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialRating"] + "%</td>");
                                                    if (loop_number == 1)
                                                    {
                                                        DataTable.Append("<td style='vertical-align:middle; padding:10px; border:1px solid black' rowspan='" + rowspan + "'>" + ShareLinkedReader["FinancialWeight"] + "%" + "</td>");
                                                        DataTable.Append("<td style='vertical-align:middle; padding:10px; border:1px solid black' rowspan='" + rowspan + "'>" + ShareLinkedReader["FinancialScore"] + "%" + "</td>");
                                                        total_weight = total_weight + float.Parse(ShareLinkedReader["FinancialWeight"].ToString());
                                                        total_score = total_score + float.Parse(ShareLinkedReader["FinancialScore"].ToString());
                                                    }
                                                    if (ShareLinkedReader["FinancialRemarks"].Equals(""))
                                                    {
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'></td>");
                                                    }
                                                    else
                                                    {
                                                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + ShareLinkedReader["FinancialRemarks"] + "</td>");
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
                            DataTable.Append("<td colspan='5' align='center' style='background-color:yellow; padding:10px; border:1px solid black'><b>TOTAL</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_weight + "%</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>" + total_score + "%</b></td>");
                            DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'></td>");
                            DataTable.Append("</tr>");
                            DataTable.Append("</table>");
                            no_header = 1;//reset nomor header
                        }//end of while HeaderReader
                    }
                    else
                    {
                        DataTable.Append("<table align='center' width='100%' style='border-collapse:collapse; text-align:center; font-size:10pt; page-break-after: always'>");
                        DataTable.Append("<tr>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow' colspan='2'><b>Group</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'>" + group_name + "</td>");
                        DataTable.Append("</tr>");
                        DataTable.Append("<tr>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow' colspan='2'><b>Stretch Rating</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='2'>Unknown</td>");
                        DataTable.Append("</tr>");

                        DataTable.Append("<tr>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>No.</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Financial Measure</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Target</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Result</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Rating</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Weight (%)</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Score</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'><b>Remarks</b></td>");
                        DataTable.Append("</tr>");

                        DataTable.Append("<tr>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black' colspan='8'>No Financial Measures</td>");
                        DataTable.Append("</tr>");

                        DataTable.Append("<tr>");
                        DataTable.Append("<td colspan='5' align='center' style='background-color:yellow; padding:10px; border:1px solid black'><b>TOTAL</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>0%</b></td>");
                        DataTable.Append("<td align='center' style='padding:10px; border:1px solid black'><b>0%</b></td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'></td>");
                        DataTable.Append("</tr>");
                        DataTable.Append("</table>");
                    }
                }
                conn.Close();
            }


            //============================================================================================//

            StringReader sr = new StringReader(DataTable.ToString());
            xmlParse.Parse(sr);
            xmlParse.Flush();
            pdfDoc.Close();
        }//end of OnClickExportPDF

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