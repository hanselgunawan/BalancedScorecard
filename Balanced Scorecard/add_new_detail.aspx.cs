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
    public partial class add_new_detail : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
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
                var page = Request.QueryString["page"];
                var header_id = Request.QueryString["header_id"];
                var period_id = Request.QueryString["period_id"];

                financial_measure_breadcrumb.Attributes.Add("a href","financial_scorecard.aspx?page="+page+"&id="+period_id+"");
                cancel_add.Attributes.Add("href", "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                //target error message
                check_target_value.Attributes.Add("style", "visibility:hidden; margin-bottom:-40px !important; margin-top:0px !important");

                //Add Item to DropDown Financial Type
                DropDownFinancialType.Items.Add("Single");
                DropDownFinancialType.Items.Add("Share");

                //ubah TextBoxTarget menjadi Input Type = "Number"
                TextBoxTarget.Attributes.Add("type", "number");
                TextBoxTarget.Attributes.Add("step", "0.01");
                TextBoxTarget.Attributes.Add("min", "0");
                TextBoxTarget.Text = "0";

                //Disable Result, Rating, dan Score
                TextBoxResult.Attributes.Add("disabled","true");
                TextBoxRating.Attributes.Add("disabled", "true");
                TextBoxScore.Attributes.Add("disabled", "true");

                //Add Item to DropDown Measurement
                DropDownMeasurement.Items.Add("%");
                //DropDownMeasurement.Items.Add("Days");
                DropDownMeasurement.Items.Add("Month");
                DropDownMeasurement.Items.Add("Million");
                DropDownMeasurement.Items.Add("Numbers");

                //Add Item to DropDown Formula
                DropDownFormula.Items.Add("(Result/Target) x 100%");
                DropDownFormula.Items.Add("100% - ((Result - Target)/Target)");
                DropDownFormula.Enabled = true;

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
                        AccessReader.Close();
                        AccessReader.Dispose();
                    }
                    conn.Close();
                }

                //For Database
                using(SqlConnection conn = new SqlConnection(str_connect))
                {
                    string select_header = "SELECT * FROM FinancialMeasures_Header WHERE FinancialHeader_ID="+header_id+"";
                    SqlCommand sql_select_header = new SqlCommand(select_header,conn);
                    conn.Open();
                    using (SqlDataReader HeaderReader = sql_select_header.ExecuteReader())
                    {
                        if (HeaderReader.HasRows)
                        {
                            while (HeaderReader.Read())
                            {
                                string start_date_formatted, end_date_formatted, string_to_period_table;
                                float stretch_rat;
                                string_to_period_table = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                                SqlCommand sql_to_period_table = new SqlCommand(string_to_period_table, conn);

                                stretch_rat = float.Parse(HeaderReader["FinancialHeader_StretchRating"].ToString());//type cast 

                                LabelGroup.InnerText = HeaderReader["FinancialHeader_Group"].ToString();
                                LabelStretch.InnerText = stretch_rat.ToString() + "%";
                                LabelReview.InnerText = HeaderReader["FinancialHeader_Review"].ToString();

                                LabelBreadcrumb.Text = HeaderReader["FinancialHeader_Group"].ToString();
                                LabelTitle.Text = HeaderReader["FinancialHeader_Group"].ToString();

                                using (SqlDataReader PeriodReader = sql_to_period_table.ExecuteReader())
                                {
                                    while (PeriodReader.Read())
                                    {
                                        DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                        DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                        start_date_formatted = start_date.ToString("MMM yyyy");
                                        end_date_formatted = end_date.ToString("MMM yyyy");
                                        LabelStartPeriod.InnerText = start_date_formatted;
                                        LabelEndPeriod.InnerText = end_date_formatted;
                                    }
                                }
                            }
                        }
                        else//error handling jika Header_ID tidak ditemukan
                        {
                            LabelStartPeriod.InnerText = "Start Period Not Found";
                            LabelEndPeriod.InnerText = "End Period Not Found";
                            LabelStretch.InnerText = "0%";
                            LabelGroup.InnerText = "Group Name Not Found";
                            LabelReview.InnerText = "Review Type Not Found";
                            LabelBreadcrumb.Text = "No Header Name";
                            LabelTitle.Text = "No Header Name";
                            SpanAddMore.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                            SpanDone.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                        }
                    }
                    conn.Close();
                }
            }
        }

        protected void OnFinancialMeasureChanged(object sender, EventArgs e)
        {
            var header_id = Request.QueryString["header_id"];
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string check_if_measure_exist = "SELECT FinancialMeasure FROM FinancialMeasures_Detail WHERE "
                                              + "FinancialHeader_ID=" + header_id + " "
                                              + "AND FinancialMeasure='" + TextBoxFinancialMeasure.Text + "'";
                SqlCommand sql_check_measure = new SqlCommand(check_if_measure_exist, conn);
                conn.Open();
                using (SqlDataReader MeasureReader = sql_check_measure.ExecuteReader())
                {
                    if (MeasureReader.HasRows)
                    {
                        check_financial_measure.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                    }
                    else
                    {
                        check_financial_measure.Attributes.Add("style", "visibility:hidden; margin-bottom:-15px !important; margin-top:0px !important");
                    }
                }
                conn.Close();
            }
        }

        protected void OnClickAddDetail(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var header_id = Request.QueryString["header_id"];
            var period_id = Request.QueryString["period_id"];
            float stretch_rat;
            bool value_exist = false;
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string get_stretch_rating = "SELECT FinancialHeader_StretchRating FROM FinancialMeasures_Header "
                                          + "WHERE FinancialHeader_ID="+header_id+"";
                string check_if_measure_exist = "SELECT FinancialMeasure FROM FinancialMeasures_Detail "
                                              + "WHERE FinancialHeader_ID=" + header_id + " "
                                              + "AND FinancialMeasure='" + TextBoxFinancialMeasure.Text + "'";
                SqlCommand sql_check_measure = new SqlCommand(check_if_measure_exist,conn);
                SqlCommand sql_get_stretch = new SqlCommand(get_stretch_rating, conn);
                conn.Open();
                stretch_rat = float.Parse(sql_get_stretch.ExecuteScalar().ToString());
                using (SqlDataReader MeasureReader = sql_check_measure.ExecuteReader())
                {
                    if (MeasureReader.HasRows)
                        {
                            check_financial_measure.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            value_exist = true;
                        }
                        else
                        {
                            check_financial_measure.Attributes.Add("style", "visibility:hidden; margin-bottom:-15px !important; margin-top:0px !important");
                            value_exist = false;
                        }
                    }
                conn.Close();
            }

            if (value_exist == false)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    string string_insert_new_item_to_group = "INSERT INTO FinancialMeasures_Detail VALUES(@header_id,@financial_measure,"
                                                            + "@financial_type,@financial_target,@financial_result, @financial_measure_by, "
                                                            + "@financial_rating, @financial_weight, @financial_link, @financial_score, "
                                                            + "@financial_formula,@financial_remarks, @user_create, @user_update, "
                                                            + "@data_status, @date_create, @date_update)";
                    string user_create, date_create, user_update, date_update;
                    SqlCommand sql_insert_new_item_to_group = new SqlCommand(string_insert_new_item_to_group, conn);
                    conn.Open();

                    user_create = Session["user_name"].ToString();
                    user_update = Session["user_name"].ToString();
                    date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    sql_insert_new_item_to_group.Parameters.AddWithValue("@header_id", header_id);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_measure", TextBoxFinancialMeasure.Text);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_type", DropDownFinancialType.SelectedItem.Value);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_target", Math.Round(Convert.ToDouble(TextBoxTarget.Text),2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_measure_by", DropDownMeasurement.SelectedItem.Value);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_link", 0);//selalu 0 pada saat Insert!
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_formula", DropDownFormula.SelectedItem.Value);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_remarks", TextareaRemarks.InnerText);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@user_create", user_create);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@user_update", user_update);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@date_create", date_create);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@date_update", date_update);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@data_status", "exist");
                    sql_insert_new_item_to_group.ExecuteNonQuery();

                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Measure Added'); window.location='" + baseUrl + "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);

                    conn.Close();
                }
            }
        }

        protected void OnClickAddMore(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var header_id = Request.QueryString["header_id"];
            var period_id = Request.QueryString["period_id"];
            float stretch_rat;
            bool value_exist = false;
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string get_stretch_rating = "SELECT FinancialHeader_StretchRating FROM FinancialMeasures_Header "
                                          + "WHERE FinancialHeader_ID=" + header_id + "";
                string check_if_measure_exist = "SELECT FinancialMeasure FROM FinancialMeasures_Detail "
                                              + "WHERE FinancialHeader_ID=" + header_id + " "
                                              + "AND FinancialMeasure='" + TextBoxFinancialMeasure.Text + "'";
                SqlCommand sql_check_measure = new SqlCommand(check_if_measure_exist, conn);
                SqlCommand sql_get_stretch = new SqlCommand(get_stretch_rating, conn);
                conn.Open();
                stretch_rat = float.Parse(sql_get_stretch.ExecuteScalar().ToString());
                using (SqlDataReader MeasureReader = sql_check_measure.ExecuteReader())
                {
                    if (MeasureReader.HasRows)
                        {
                            check_financial_measure.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            value_exist = true;
                        }
                        else
                        {
                            check_financial_measure.Attributes.Add("style", "visibility:hidden; margin-bottom:-15px !important; margin-top:0px !important");
                            value_exist = false;
                        }
                    }
                conn.Close();
            }

            if (value_exist == false)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    string string_insert_new_item_to_group = "INSERT INTO FinancialMeasures_Detail VALUES(@header_id,@financial_measure,"
                                                            +"@financial_type,@financial_target,@financial_result, @financial_measure_by, "
                                                            +"@financial_rating, @financial_weight, @financial_link, @financial_score, "
                                                            +"@financial_formula,@financial_remarks, @user_create, @user_update, "
                                                            +"@data_status, @date_create, @date_update)";
                    string user_create, date_create, user_update, date_update;
                    SqlCommand sql_insert_new_item_to_group = new SqlCommand(string_insert_new_item_to_group, conn);
                    conn.Open();

                    user_create = Session["user_name"].ToString();
                    user_update = Session["user_name"].ToString();
                    date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    sql_insert_new_item_to_group.Parameters.AddWithValue("@header_id", header_id);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_measure", TextBoxFinancialMeasure.Text);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_type", DropDownFinancialType.SelectedItem.Value);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_target", Math.Round(Convert.ToDouble(TextBoxTarget.Text),2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_measure_by", DropDownMeasurement.SelectedItem.Value);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_link", 0);//selalu 0 pada saat Insert!
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_formula", DropDownFormula.SelectedItem.Value);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@financial_remarks", TextareaRemarks.InnerText);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@user_create", user_create);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@date_create", date_create);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@user_update", user_update);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@date_update", date_update);
                    sql_insert_new_item_to_group.Parameters.AddWithValue("@data_status", "exist");
                    sql_insert_new_item_to_group.ExecuteNonQuery();
                    
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Measure Added'); window.location='" + baseUrl + "add_new_detail.aspx?page=" + page + "&header_id="+header_id+"&period_id=" + period_id + "';", true);
                    conn.Close();
                }
            }
        }

        protected void OnTargetChanged(object sender, EventArgs e)
        {
            if (DropDownMeasurement.SelectedValue == "Month")
            {
                int target_value = int.Parse(TextBoxTarget.Text);
                string month_name;
                check_target_value.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                if (target_value > 12)
                {
                    TextBoxTarget.Text = "12";
                    check_target_value.InnerText = "December";
                }
                else if (target_value < 1)
                {
                    TextBoxTarget.Text = "1";
                    check_target_value.InnerText = "January";
                }
                else
                {
                    month_name = ShowMonthName(target_value);
                    check_target_value.InnerText = month_name;
                }
            }
            else
            {
                check_target_value.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
            }
        }

        protected void OnSelectMeasureBy(object sender, EventArgs e)
        {
            if (DropDownMeasurement.SelectedValue == "Month")
            {
                int target_value = int.Parse(TextBoxTarget.Text);
                string month_name;
                check_target_value.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                TextBoxTarget.Attributes.Add("max", "12");
                TextBoxTarget.Attributes.Add("min", "1");
                TextBoxTarget.Attributes.Add("step", "1");
                if (target_value > 12)
                {
                    TextBoxTarget.Text = "12";
                    check_target_value.InnerText = "December";
                }
                else if (target_value < 1)
                {
                    TextBoxTarget.Text = "1";
                    check_target_value.InnerText = "January";
                }
                else
                {
                    month_name = ShowMonthName(target_value);
                    check_target_value.InnerText = month_name;
                }
                DropDownFormula.SelectedValue = "100% - ((Result - Target)/Target)";
                DropDownFormula.Enabled = false;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Maximum Target value for Measured By month is 12 and minimum value is 1')", true);
            }
            else
            {
                check_target_value.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                TextBoxTarget.Attributes.Remove("max");
                TextBoxTarget.Attributes.Add("min", "0");
                if (DropDownMeasurement.SelectedValue == "Numbers")
                {
                    TextBoxTarget.Attributes.Add("step", "1");
                }
                else
                {
                    TextBoxTarget.Attributes.Add("step", "0.01");
                }
                DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                DropDownFormula.Enabled = true;
            }
        }

        protected void OnSelectType(object sender, EventArgs e)
        {
            if (DropDownFinancialType.SelectedValue == "Share")
            {
                //kalau Type='Share', Weight-nya ga bisa diisi dulu!
                TextBoxWeight.Attributes.Add("disabled", "true");
                TextBoxWeight.Value = "0";
            }
            else if (DropDownFinancialType.SelectedValue == "Single")
            {
                TextBoxWeight.Attributes.Remove("disabled");
            }
        }

        public string ShowMonthName(int target_value)
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