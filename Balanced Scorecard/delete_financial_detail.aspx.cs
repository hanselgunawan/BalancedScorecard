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
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class delete_financial_detail : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (!IsPostBack)
            {
                if (Session["user_name"] == null)
                {
                    Response.Redirect(baseUrl + "index.aspx");
                }
                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();
                var page = Request.QueryString["page"];
                var header_id = Request.QueryString["header_id"];
                var period_id = Request.QueryString["period_id"];

                financial_measure_link.Attributes.Add("href", "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "");
                back_link.Attributes.Add("href", "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                SqlDataAdapter adapter = new SqlDataAdapter();
                DataSet ds = new DataSet();
                string startdate_to_date, enddate_to_date;
                string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                string string_select_header = "SELECT FinancialHeader_Group FROM FinancialMeasures_Header WHERE FinancialHeader_ID=" + header_id +"";
                string string_select_detail = "SELECT * FROM FinancialMeasures_Detail WHERE "
                                            + "FinancialHeader_ID=" + header_id + " AND data_status='exist' "
                                            + "ORDER BY FinancialLinked";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                SqlConnection conn = new SqlConnection(str_connect);
                conn.Open();
                SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                SqlCommand sql_select_detail = new SqlCommand(string_select_detail, conn);
                SqlCommand sql_select_header = new SqlCommand(string_select_header, conn);
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

                using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                {
                    if (PeriodReader.HasRows)
                    {
                        while (PeriodReader.Read())
                        {
                            DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                            DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                            startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                            enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                            LabelPeriod.Text = startdate_to_date + " - " + enddate_to_date;
                        }
                    }
                    else
                    {
                        LabelPeriod.Text = "No Period";
                    }
                }

                GroupName.Text = sql_select_header.ExecuteScalar().ToString();
                GroupNameList.Text = sql_select_header.ExecuteScalar().ToString();
                GroupNameTitle.Text = sql_select_header.ExecuteScalar().ToString();

                adapter.SelectCommand = sql_select_detail;
                adapter.Fill(ds);
                adapter.Dispose();
                sql_select_detail.Dispose();
                conn.Close();

                if (ds.Tables[0].Rows.Count == 0)//jika tidak ada Specific Objectives
                {
                    ds.Tables[0].Rows.Add(ds.Tables[0].NewRow());
                    GridView1.DataSource = ds;
                    GridView1.DataBind();
                    int columncount = GridView1.Rows[0].Cells.Count;
                    GridView1.Rows[0].Cells.Clear();
                    GridView1.Rows[0].Cells.Add(new TableCell());
                    GridView1.Rows[0].Cells[0].ColumnSpan = columncount;
                    GridView1.Rows[0].Cells[0].Text = "No Financial Measure To Display";
                    GridView1.Rows[0].Cells[0].HorizontalAlign = HorizontalAlign.Center;
                    GridView1.Columns[2].Visible = false;
                }
                else//jika ada Specific Objectives
                {
                    int i = 0;
                    string linked_number1;
                    GridView1.DataSource = ds.Tables[0];
                    GridView1.DataBind();
                    GridView1.Columns[2].Visible = false;

                    while (i < GridView1.Rows.Count)
                    {

                        /*if (GridView1.Rows[i].Cells[4].Text == "10 Month")
                        {
                            //string month_num = GridView1.Rows[i].Cells[4].Text.Substring(0, 1);
                            //string month_name = showMonthName(month_num);
                            //GridView1.Rows[i].Cells[4].Text = "Bray";
                        }*/

                        if (GridView1.Rows[i].Cells[2].Text != "0")
                        {
                            int linked_number_counter = 0;
                            linked_number1 = GridView1.Rows[i].Cells[2].Text;
                            for (int j = i; j < GridView1.Rows.Count; j++)
                            {
                                string linked_number2;
                                linked_number2 = GridView1.Rows[j].Cells[2].Text;
                                if (linked_number2 == linked_number1)
                                {
                                    if (linked_number_counter == 0)
                                    {
                                        GridView1.Rows[j].Cells[11].Attributes.Add("style", "visibility:visible; border-color:white");
                                    }
                                    else
                                    {
                                        GridView1.Rows[j].Cells[11].Attributes.Add("style", "visibility:hidden");
                                    }
                                    linked_number_counter++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            i = i + linked_number_counter;
                        }
                        else
                        {
                            i++;
                        }
                    }

                }
            }
        }

        protected void grv_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var measure_by = (Label)e.Row.FindControl("LabelMeasureByTarget");
                var target = (Label)e.Row.FindControl("LabelTarget");
                var result = (Label)e.Row.FindControl("LabelResult");
                if (measure_by.Text == "Month")
                {
                    string month_num_target = target.Text;
                    string month_num_result = result.Text;
                    string month_name_target = getMonthName(month_num_target);
                    string month_name_result = getMonthName(month_num_result);
                    e.Row.Cells[4].Text = month_name_target;
                    e.Row.Cells[5].Text = month_name_result;
                }
                else if (measure_by.Text == "Numbers")
                {
                    e.Row.Cells[4].Text = target.Text;
                    e.Row.Cells[5].Text = result.Text;
                }
            }
        }

        protected void OnClickDelete(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            StringBuilder sb_deleted_financial_detail = new StringBuilder();
            var page = Request.QueryString["page"];
            var header_id = Request.QueryString["header_id"];
            var period_id = Request.QueryString["period_id"];

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string user_update, date_update;
                int counter = 1;
                user_update = Session["user_name"].ToString();
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                foreach (GridViewRow row in GridView1.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        CheckBox chkRow = (row.Cells[0].FindControl("chkCtrl") as CheckBox);
                        if (chkRow.Checked)
                        {
                            string detail_name = row.Cells[1].Text;
                            if (row.Cells[3].Text == "Single")
                            {
                                //asumsi header_id = 1
                                string string_update_header = "UPDATE FinancialMeasures_Detail SET data_status='deleted', "
                                                            + "user_update=@user_update, date_update=@date_update "
                                                            + "WHERE FinancialMeasure='" + detail_name + "' "
                                                            + "AND FinancialHeader_ID=" + header_id + "";
                                SqlCommand sql_update_header = new SqlCommand(string_update_header, conn);
                                sql_update_header.Parameters.AddWithValue("@user_update", user_update);
                                sql_update_header.Parameters.AddWithValue("@date_update", date_update);
                                conn.Open();
                                sql_update_header.ExecuteNonQuery();
                                conn.Close();
                                sb_deleted_financial_detail.Append("  " + counter + ". " + detail_name + "\\n");
                                counter++;
                            }
                            else//jika Financial Type='Share', maka semua FinancialLinked yang sama DIHAPUS!
                            {
                                StringBuilder sb_link_item = new StringBuilder();
                                string linked_number = row.Cells[2].Text;
                                string string_update_header_share = "UPDATE FinancialMeasures_Detail SET data_status='deleted', "
                                                                  + "user_update=@user_update, date_update=@date_update "
                                                                  + "WHERE FinancialLinked="+linked_number+"";
                                string select_linked_items = "SELECT FinancialMeasure FROM FinancialMeasures_Detail "
                                                           + "WHERE FinancialLinked=" + linked_number + " AND "
                                                           + "data_status='exist'";
                                SqlCommand sql_update_header_share = new SqlCommand(string_update_header_share, conn);
                                SqlCommand sql_select_linked_items = new SqlCommand(select_linked_items, conn);
                                conn.Open();
                                using (SqlDataReader LinkedItemReader = sql_select_linked_items.ExecuteReader())
                                {
                                    while(LinkedItemReader.Read())
                                    {
                                        sb_link_item.Append(""+LinkedItemReader["FinancialMeasure"]+", ");
                                    }
                                    LinkedItemReader.Dispose();
                                    LinkedItemReader.Close();
                                }
                                if (sb_link_item.Length > 0)
                                {
                                    sb_link_item.Remove(sb_link_item.Length - 2, 1);
                                }

                                sql_update_header_share.Parameters.AddWithValue("@user_update", user_update);
                                sql_update_header_share.Parameters.AddWithValue("@date_update", date_update);
                                sql_update_header_share.ExecuteNonQuery();
                                conn.Close();
                                sb_deleted_financial_detail.Append("  " + counter + ". " + sb_link_item.ToString() + "\\n");
                                counter++;
                            }
                        }
                    }
                }//end of ForEach(GridViewRow)

                

                if (counter > 1)
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Delete Financial Measure:\\n" + sb_deleted_financial_detail.ToString() + "  \\nSUCCESS!'); window.location='" + baseUrl + "delete_financial_detail.aspx?page=" + page + "&period_id=" + period_id + "&header_id=" + header_id + "';", true);
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('There is no selected detail to be deleted!'); window.location='" + baseUrl + "delete_financial_detail.aspx?page=" + page + "&period_id=" + period_id + "&header_id=" + header_id + "';", true);
                }
            }
        }

        public string getMonthName(string month_num)
        {
            string month_name = "";
            switch (month_num)
            {
                case "0 ":
                    month_name = "-";
                    break;
                case "1 ":
                    month_name = "January";
                    break;
                case "2 ":
                    month_name = "February";
                    break;
                case "3 ":
                    month_name = "March";
                    break;
                case "4 ":
                    month_name = "April";
                    break;
                case "5 ":
                    month_name = "May";
                    break;
                case "6 ":
                    month_name = "June";
                    break;
                case "7 ":
                    month_name = "July";
                    break;
                case "8 ":
                    month_name = "August";
                    break;
                case "9 ":
                    month_name = "September";
                    break;
                case "10 ":
                    month_name = "October";
                    break;
                case "11 ":
                    month_name = "November";
                    break;
                case "12 ":
                    month_name = "December";
                    break;
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