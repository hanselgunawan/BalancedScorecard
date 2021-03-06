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
    public partial class delete_single_financial_detail : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";

            if (Session["user_name"] == null)
            {
                Response.Redirect(baseUrl + "index.aspx");
            }
            var page = Request.QueryString["page"];
            var detail_id = Request.QueryString["detail_id"];
            var period_id = Request.QueryString["period_id"];
            var linked = Request.QueryString["linked"];
            string detail_name, user_update, date_update;
            string delete_financial_detail = "";
            string select_detail_name = "SELECT FinancialMeasure FROM FinancialMeasures_Detail WHERE FinancialDetail_ID=" + detail_id + "";
            string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                          + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                          + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                          + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
            SqlConnection conn = new SqlConnection(str_connect);
            conn.Open();
            SqlCommand sql_select_detail_name = new SqlCommand(select_detail_name, conn);
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
            detail_name = (string)sql_select_detail_name.ExecuteScalar();

            user_update = Session["user_name"].ToString();
            date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (linked == "0")
            {
                delete_financial_detail = "UPDATE FinancialMeasures_Detail SET user_update=@user_update, date_update=@date_update, data_status='deleted' WHERE FinancialDetail_ID=" + detail_id + "";
                SqlCommand sql_delete_financial_detail = new SqlCommand(delete_financial_detail, conn);
                sql_delete_financial_detail.Parameters.AddWithValue("@user_update", user_update);
                sql_delete_financial_detail.Parameters.AddWithValue("@date_update", date_update);
                sql_delete_financial_detail.ExecuteNonQuery();
                conn.Close();

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Delete " + detail_name + " Success!'); window.location='" + baseUrl + "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
            }
            else
            {
                StringBuilder sb_linked_items = new StringBuilder();
                string select_linked_items = "SELECT FinancialMeasure FROM FinancialMeasures_Detail WHERE FinancialLinked=" + linked + " AND data_status='exist'";
                delete_financial_detail = "UPDATE FinancialMeasures_Detail SET user_update=@user_update, date_update=@date_update, data_status='deleted' WHERE FinancialLinked=" + linked + "";
                SqlCommand sql_select_linked_items = new SqlCommand(select_linked_items, conn);
                SqlCommand sql_delete_financial_detail = new SqlCommand(delete_financial_detail, conn);

                using (SqlDataReader LinkedItemReader = sql_select_linked_items.ExecuteReader())
                {
                    while(LinkedItemReader.Read())
                    {
                        sb_linked_items.Append("\\n - " + LinkedItemReader["FinancialMeasure"] + "");
                    }
                    LinkedItemReader.Dispose();
                    LinkedItemReader.Close();
                }

                sql_delete_financial_detail.Parameters.AddWithValue("@user_update", user_update);
                sql_delete_financial_detail.Parameters.AddWithValue("@date_update", date_update);
                sql_delete_financial_detail.ExecuteNonQuery();
                conn.Close();

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Delete: "+sb_linked_items.ToString()+"\\n Success!'); window.location='" + baseUrl + "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
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