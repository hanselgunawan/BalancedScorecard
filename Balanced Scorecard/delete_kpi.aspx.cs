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
    public partial class delete_kpi : System.Web.UI.Page
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
                var period_id = Request.QueryString["period_id"];
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                          + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                          + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                          + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                individual_scorecard_link.Attributes.Add("href", "individual_scorecard.aspx?page="+page+"&id="+period_id+"");
                back_link.Attributes.Add("href", "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                SqlDataAdapter adapter = new SqlDataAdapter();
                DataSet ds = new DataSet();

                //ASUMSI --> NIK=100
                string string_select_header = "SELECT * FROM IndividualMeasures_Header WHERE user_id=" + Session["user_id"] + " AND Period_ID=" + period_id + " AND data_status='exist'";
                SqlConnection conn = new SqlConnection(str_connect);
                conn.Open();
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
                adapter.SelectCommand = sql_select_header;
                adapter.Fill(ds);
                adapter.Dispose();
                sql_select_header.Dispose();
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
                    GridView1.Rows[0].Cells[0].Text = "No KPI To Display";
                    GridView1.Rows[0].Cells[0].HorizontalAlign = HorizontalAlign.Center;
                }
                else//jika ada Specific Objectives
                {
                    GridView1.DataSource = ds.Tables[0];
                    GridView1.DataBind();
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
                if (measure_by.Text != "-")
                {
                    if (measure_by.Text == "Month")
                    {
                        string month_num_target = target.Text;
                        string month_num_result = result.Text;
                        string month_name_target = getMonthName(month_num_target);
                        string month_name_result = getMonthName(month_num_result);
                        e.Row.Cells[2].Text = month_name_target;
                        e.Row.Cells[3].Text = month_name_result;
                    }
                    else if (measure_by.Text == "Numberts")
                    {
                        e.Row.Cells[2].Text = target.Text;
                        e.Row.Cells[3].Text = result.Text;
                    }
                }
                else if(measure_by.Text == "-")
                {
                    e.Row.Cells[2].Text = "Based On Schedule";
                    e.Row.Cells[3].Text = "-";
                }
            }
        }

        protected void OnClickDelete(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            StringBuilder sb_deleted_individual_kpi = new StringBuilder();

            //harusnya ada USERNAME!
            var period_id = Request.QueryString["period_id"];
            var page = Request.QueryString["page"];

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
                        CheckBox chkRow = (row.Cells[9].FindControl("chkCtrl") as CheckBox);
                        if (chkRow.Checked)
                        {
                            string kpi = row.Cells[1].Text;
                            string string_update_header = "UPDATE IndividualMeasures_Header SET data_status='deleted', "
                                                        + "user_update=@user_update, date_update=@date_update "
                                                        + "WHERE Period_ID="+period_id+" "
                                                        + "AND IndividualHeader_KPI='" + kpi + "'";
                            SqlCommand sql_update_header = new SqlCommand(string_update_header, conn);
                            sql_update_header.Parameters.AddWithValue("@user_update", user_update);
                            sql_update_header.Parameters.AddWithValue("@date_update", date_update);
                            conn.Open();
                            sql_update_header.ExecuteNonQuery();
                            conn.Close();
                            sb_deleted_individual_kpi.Append("  " + counter + ". " + kpi + "\\n");
                            counter++;
                        }
                    }
                }//end of ForEach(GridViewRow)

                

                if (counter > 1)
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Delete KPI:\\n" + sb_deleted_individual_kpi + "  \\nSUCCESS!'); window.location='" + baseUrl + "delete_kpi.aspx?page=" + page + "&period_id=" + period_id + "';", true);
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('There is no selected KPI to be deleted!'); window.location='" + baseUrl + "delete_kpi.aspx?page=" + page + "&period_id=" + period_id + "';", true);
                }
            }
        }

        public string getMonthName(string month_num)
        {
            string month_name = "";
            switch (month_num)
            {
                case "0":
                    month_name = "-";
                    break;
                case "1":
                    month_name = "January";
                    break;
                case "2":
                    month_name = "February";
                    break;
                case "3":
                    month_name = "March";
                    break;
                case "4":
                    month_name = "April";
                    break;
                case "5":
                    month_name = "May";
                    break;
                case "6":
                    month_name = "June";
                    break;
                case "7":
                    month_name = "July";
                    break;
                case "8":
                    month_name = "August";
                    break;
                case "9":
                    month_name = "September";
                    break;
                case "10":
                    month_name = "October";
                    break;
                case "11":
                    month_name = "November";
                    break;
                case "12":
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