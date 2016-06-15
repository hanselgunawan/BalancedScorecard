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
    public partial class add_group : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (!IsPostBack)
            {
                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();
                var period_id = Request.QueryString["period_id"];
                var page = Request.QueryString["page"];

                //link untuk breadcrumb
                financial_measures_breadcrumb.Attributes.Add("a href", "financial_scorecard.aspx?id=" + period_id + "");

                if (Session["user_name"] == null)
                {
                    Response.Redirect(baseUrl + "index.aspx");
                }

                //source code untuk hak akses
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
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
                    conn.Close();
                }

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                    string string_select_group_name = "SELECT Group_Name FROM ScorecardGroup";//ambil nama-nama Group
                    string string_select_review = "SELECT DISTINCT Review_Name FROM ScorecardReview";//ambil nama-nama Review
                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                    SqlCommand sql_select_group_name = new SqlCommand(string_select_group_name, conn);
                    SqlCommand sql_select_review = new SqlCommand(string_select_review, conn);
                    conn.Open();
                    using (SqlDataReader oReader = sql_select_period.ExecuteReader())
                    {
                        if (oReader.HasRows)
                        {
                            while (oReader.Read())
                            {
                                string start_date_formatted, end_date_formatted;
                                DateTime start_date = Convert.ToDateTime(oReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(oReader["End_Period"]);
                                start_date_formatted = start_date.ToString("MMM yyyy");
                                end_date_formatted = end_date.ToString("MMM yyyy");
                                LabelStartPeriod.InnerText = start_date_formatted;
                                LabelEndPeriod.InnerText = end_date_formatted;
                            }
                            cancel_add_new_group.Attributes.Add("a href", "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "");
                        }
                        else
                        {
                            LabelStartPeriod.InnerText = "No Period";
                            LabelEndPeriod.InnerText = "No Period";
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                            cancel_add_new_group.Attributes.Add("a href", "financial_scorecard.aspx?page=1&id=1");
                        }
                    }

                    using (SqlDataReader GroupReader = sql_select_group_name.ExecuteReader())
                    {
                        if (GroupReader.HasRows)
                        {
                            while (GroupReader.Read())
                            {
                                DropDownListGroup.Items.Add(GroupReader["Group_Name"].ToString());
                            }
                        }
                        else
                        {
                            DropDownListGroup.Items.Add("No Group Found");
                        }
                    }

                    using (SqlDataReader ReviewReader = sql_select_review.ExecuteReader())
                    {
                        if (ReviewReader.HasRows)
                        {
                            while (ReviewReader.Read())
                            {
                                DropDownListReview.Items.Add(ReviewReader["Review_Name"].ToString());
                            }
                        }
                        else
                        {
                            DropDownListReview.Items.Add("No Review Found");
                        }
                    }

                    conn.Close();
                }
            }
        }

        protected void OnDropdownChanged(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string check_if_group_name_exist = "SELECT FinancialHeader_Group FROM FinancialMeasures_Header WHERE Period_ID=" + period_id
                                                 + " AND FinancialHeader_Group='" + DropDownListGroup.SelectedValue + "'";
                SqlCommand sql_check_group_name = new SqlCommand(check_if_group_name_exist, conn);
                conn.Open();
                using (SqlDataReader GroupNameReader = sql_check_group_name.ExecuteReader())
                {
                    if (GroupNameReader.HasRows)
                        {
                            check_group_name.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                        }
                        else
                        {
                            check_group_name.Attributes.Add("style", "visibility:hidden; margin-bottom:0px !important; margin-top:-18px !important");
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        }
                    }
                conn.Close();
            }
        }

        protected void OnClickSubmit(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var period_id = Request.QueryString["period_id"];
            var page = Request.QueryString["page"];
            bool value_exist = true;
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                //cek apakah Financial Group uda ada di DB atau belum
                string check_if_group_name_exist = "SELECT FinancialHeader_Group FROM FinancialMeasures_Header "
                                                 + "WHERE Period_ID="+period_id+" AND data_status='exist' AND "
                                                 + "FinancialHeader_Group='"+DropDownListGroup.SelectedValue+"'";
                SqlCommand sql_check_group_name = new SqlCommand(check_if_group_name_exist, conn);
                conn.Open();
                using (SqlDataReader GroupNameReader = sql_check_group_name.ExecuteReader())
                {
                    if (GroupNameReader.HasRows)
                    {
                        check_group_name.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                        value_exist = true;
                    }
                    else
                    {
                        check_group_name.Attributes.Add("style", "visibility:hidden; margin-bottom:0px !important; margin-top:-18px !important");
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        value_exist = false;
                    }
                }
                conn.Close();
            }

            if (value_exist == false)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    string string_insert_new_group = "INSERT INTO FinancialMeasures_Header VALUES(@group_name, @stretch_rating, "
                                                   + "@review, @user_create, @user_update, @period_id, 'exist', "
                                                   + "@individual_stretch_rating, @date_create, @date_update, @review_id)";
                    string user_create, date_create, user_update, date_update;
                    SqlCommand sql_insert_group = new SqlCommand(string_insert_new_group, conn);
                    conn.Open();

                    user_create = Session["user_name"].ToString();
                    user_update = Session["user_name"].ToString();
                    date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    sql_insert_group.Parameters.AddWithValue("@group_name", DropDownListGroup.SelectedValue);
                    sql_insert_group.Parameters.AddWithValue("@stretch_rating", Math.Round(Convert.ToDouble(TextBoxStretch.Value),2));
                    sql_insert_group.Parameters.AddWithValue("@review", DropDownListReview.SelectedValue);
                    sql_insert_group.Parameters.AddWithValue("@user_create", user_create);
                    sql_insert_group.Parameters.AddWithValue("@user_update", user_update);
                    sql_insert_group.Parameters.AddWithValue("@period_id", period_id);
                    sql_insert_group.Parameters.AddWithValue("@individual_stretch_rating",Math.Round(Convert.ToDouble(TextBoxStretchIndividual.Value),2));
                    sql_insert_group.Parameters.AddWithValue("@date_create", date_create);
                    sql_insert_group.Parameters.AddWithValue("@date_update", date_update);
                    if (DropDownListReview.SelectedIndex == 0)
                    {
                        sql_insert_group.Parameters.AddWithValue("@review_id", 1);

                    }
                    else if (DropDownListReview.SelectedIndex == 1)
                    {
                        sql_insert_group.Parameters.AddWithValue("@review_id", 5);

                    }
                    else if (DropDownListReview.SelectedIndex == 2)
                    {
                        sql_insert_group.Parameters.AddWithValue("@review_id", 7);

                    }
                    sql_insert_group.ExecuteNonQuery();
                    conn.Close();
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Group Added!'); window.location='" + baseUrl + "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                }
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