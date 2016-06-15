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
using System.Net.Mail;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class request_for_change_KPI : System.Web.UI.Page
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
                var period_id = Request.QueryString["period_id"];
                var header_id = Request.QueryString["header_id"];
                var page = Request.QueryString["page"];
                string user_nik = (string)Session["user_nik"];

                int current_month;
                current_month = DateTime.Now.Month;
                int start_date_bsc = 0, minus_one_bsc = 0;

                SqlConnection period_conn = new SqlConnection(str_connect);
                period_conn.Open();
                string select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                SqlCommand sql_select_period_start_month = new SqlCommand(select_period, period_conn);
                using (SqlDataReader StartMonthReader = sql_select_period_start_month.ExecuteReader())
                {
                    while (StartMonthReader.Read())
                    {
                        DateTime start_date = Convert.ToDateTime(StartMonthReader["Start_Period"]);
                        start_date_bsc = int.Parse(start_date.ToString("MM"));
                    }
                    StartMonthReader.Dispose();
                    StartMonthReader.Close();
                }
                period_conn.Close();

                if (start_date_bsc == 1)
                {
                    minus_one_bsc = 12;
                }
                else
                {
                    minus_one_bsc = start_date_bsc - 1;
                }

                //link breadcrumb
                individual_scorecard_breadcrumb.Attributes.Add("a href", "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                //ubah TextBoxTarget menjadi Input Type = "Number"
                TextBoxTarget.Attributes.Add("type", "number");
                TextBoxTarget.Attributes.Add("step", "0.01");
                TextBoxTarget.Attributes.Add("min", "0");
                TextBoxTarget.Text = "0";

                //ubah TextBoxResult menjadi Input Type = "Number"
                TextBoxResult.Attributes.Add("type", "number");
                TextBoxResult.Attributes.Add("step", "0.01");
                TextBoxResult.Attributes.Add("min", "0");
                TextBoxResult.Text = "0";

                //untuk item pada Drop Down Measurement
                DropDownListMeasurement.Items.Add("%");
                //DropDownListMeasurement.Items.Add("Days");
                DropDownListMeasurement.Items.Add("Month");
                DropDownListMeasurement.Items.Add("Million");
                DropDownListMeasurement.Items.Add("Numbers");

                //untuk item pada DropDown Specific Objective
                DropDownListSpecific.Items.Add("Yes");
                DropDownListSpecific.Items.Add("No");

                //Add Item to DropDown Formula
                DropDownFormula.Items.Add("(Result/Target) x 100%");
                DropDownFormula.Items.Add("100% - ((Result - Target)/Target)");
                DropDownFormula.Enabled = false;

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan UserGroup
                                          + "WHERE Access_Rights_Code NOT IN "                       
                                          + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                          + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                    string string_select_individual_stretch_review = "SELECT FinancialHeader_IndividualStretchRating, FinancialHeader_Review FROM ScorecardUser "
                                                                    + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                                    + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                                    + "WHERE ScorecardUser.EmpId='" + user_nik + "' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                    string select_individual_header = "SELECT * FROM IndividualMeasures_Header "
                                                    + "WHERE IndividualHeader_ID=" + header_id + " AND data_status='exist'";
                    SqlCommand sql_select_individual_stretch_review = new SqlCommand(string_select_individual_stretch_review, conn);
                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                    SqlCommand sql_select_individual_header = new SqlCommand(select_individual_header, conn);
                    SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);
                    conn.Open();

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

                    using (SqlDataReader StretchReviewReader = sql_select_individual_stretch_review.ExecuteReader())
                    {
                        if (StretchReviewReader.HasRows)
                        {
                            while (StretchReviewReader.Read())
                            {
                                LabelStretch.InnerText = StretchReviewReader["FinancialHeader_IndividualStretchRating"].ToString() + "%";
                                LabelReview.InnerText = StretchReviewReader["FinancialHeader_Review"].ToString();
                                TextBoxRating.Attributes.Add("max", StretchReviewReader["FinancialHeader_IndividualStretchRating"].ToString());
                            }
                        }
                        else
                        {
                            LabelStretch.InnerText = "0%";
                            LabelReview.InnerText = "-";
                            TextBoxRating.Attributes.Add("max", "0");
                        }
                    }

                    using (SqlDataReader oReader = sql_select_period.ExecuteReader())
                    {
                        if (oReader.HasRows)
                        {
                            while (oReader.Read())
                            {
                                string start_date_formatted, end_date_formatted;
                                DateTime start_date = Convert.ToDateTime(oReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(oReader["End_Period"]);
                                start_date_formatted = start_date.ToString("MMM,dd-yyyy");
                                end_date_formatted = end_date.ToString("MMM,dd-yyyy");
                                LabelStartPeriod.InnerText = start_date_formatted;
                                LabelEndPeriod.InnerText = end_date_formatted;
                            }
                            cancel_request_KPI.Attributes.Add("a href", "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "");
                        }
                        else//jika periode tidak ditemukkan / disuntik langsung ke Database
                        {
                            LabelStartPeriod.InnerText = "No Period";
                            LabelEndPeriod.InnerText = "No Period";
                            SpanEditKPI.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                            cancel_request_KPI.Attributes.Add("a href", "individual_scorecard.aspx?page=1&id=1");
                        }
                    }

                    using (SqlDataReader IndividualHeaderReader = sql_select_individual_header.ExecuteReader())
                    {
                        if (IndividualHeaderReader.HasRows)
                        {
                            while (IndividualHeaderReader.Read())
                            {
                                DropDownListSpecific.Enabled = true;
                                TextBoxKPI.Attributes.Remove("disabled");
                                TextareaObjective.Attributes.Remove("disabled");
                                TextBoxTarget.Attributes.Remove("disabled");
                                TextBoxResult.Attributes.Remove("disabled");
                                DropDownListMeasurement.Enabled = true;
                                //DropDownFormula.Enabled = false;
                                TextBoxWeight.Attributes.Remove("disabled");

                                if (IndividualHeaderReader["IndividualHeader_Target"].ToString() == "-1")
                                {
                                    DropDownListSpecific.SelectedValue = "Yes";
                                    TextBoxTarget.Attributes.Add("disabled", "true");
                                    TextBoxResult.Attributes.Add("disabled", "true");
                                    DropDownListMeasurement.Enabled = false;
                                    DropDownFormula.Enabled = false;
                                    TextBoxRating.Attributes.Add("readonly", "true");
                                    TextBoxScore.Attributes.Add("readonly", "true");
                                    TextBoxTarget.Text = "0";
                                    TextBoxResult.Text = "0";
                                    DropDownListMeasurement.Items.Add("-");
                                    DropDownFormula.Items.Add("-");
                                    DropDownListMeasurement.SelectedValue = "-";
                                    DropDownFormula.SelectedValue = "-";
                                }
                                else
                                {
                                    DropDownListSpecific.SelectedValue = "No";
                                    TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                    TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                    DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                    DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                }
                                TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                {
                                    DropDownFormula.Enabled = false;
                                    TextBoxTarget.Attributes.Add("max", "12");
                                    TextBoxTarget.Attributes.Add("step", "1");
                                    TextBoxResult.Attributes.Add("max", "12");
                                    TextBoxResult.Attributes.Add("step", "1");
                                    int target_value = int.Parse(TextBoxTarget.Text);
                                    string target_month_name;
                                    target_month_name = ShowMonthNameTarget(target_value);
                                    month_name_target.InnerText = target_month_name;
                                    //tampilakan Month Name
                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                    if (IndividualHeaderReader["IndividualHeader_Result"].ToString() == "0")
                                    {
                                        month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                    }
                                    else
                                    {
                                        int result_value = int.Parse(TextBoxResult.Text);
                                        string result_month_name;
                                        result_month_name = ShowMonthNameResult(result_value);
                                        month_name_result.InnerText = result_month_name;
                                        month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                    }
                                }
                                else
                                {
                                    if (IndividualHeaderReader["IndividualHeader_Target"].ToString() != "-1")//jika tidak ada SO
                                    {
                                        DropDownFormula.Enabled = true;
                                        //tampilakan Month Name
                                        month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                        month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                    }
                                }
                                TextBoxKPI.Text = IndividualHeaderReader["IndividualHeader_KPI"].ToString();
                                TextareaObjective.InnerText = IndividualHeaderReader["IndividualHeader_Objective"].ToString();
                                TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();

                                LabelBreadcrumb.Text = IndividualHeaderReader["IndividualHeader_KPI"].ToString();
                                LabelTitle.Text = IndividualHeaderReader["IndividualHeader_KPI"].ToString();
                            }
                        }
                    }
                    conn.Close();
                }//end of SqlConnection
            }
        }

        protected void OnSelectMeasureBy(object sender, EventArgs e)
        {
            if (DropDownListMeasurement.SelectedValue == "Month")
            {
                int target_value = int.Parse(TextBoxTarget.Text);
                int result_value = int.Parse(TextBoxResult.Text);
                string month_name;
                month_name_target.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                TextBoxTarget.Attributes.Add("max", "12");
                TextBoxTarget.Attributes.Add("min", "1");
                TextBoxTarget.Attributes.Add("step", "1");

                TextBoxResult.Attributes.Add("max", "12");
                TextBoxResult.Attributes.Add("min", "0");
                TextBoxResult.Attributes.Add("step", "1");

                if (target_value > 12)
                {
                    TextBoxTarget.Text = "12";
                    month_name_target.InnerText = "December";
                }
                else if (target_value < 1)
                {
                    TextBoxTarget.Text = "1";
                    month_name_target.InnerText = "January";
                }
                else
                {
                    month_name = ShowMonthNameTarget(target_value);
                    month_name_target.InnerText = month_name;
                }

                if (result_value > 12)
                {
                    TextBoxResult.Text = "12";
                    month_name_result.InnerText = "December";
                }
                else if (result_value < 0)
                {
                    TextBoxResult.Text = "0";
                    month_name_result.InnerText = "";
                }
                else
                {
                    month_name = ShowMonthNameResult(target_value);
                    month_name_result.InnerText = month_name;
                }
                DropDownFormula.SelectedValue = "100% - ((Result - Target)/Target)";
                DropDownFormula.Enabled = false;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Maximum Target value for Measured By month is 12 and minimum value is 1')", true);
            }
            else
            {
                month_name_target.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                month_name_result.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");

                TextBoxTarget.Attributes.Remove("max");
                TextBoxTarget.Attributes.Add("min", "0");


                TextBoxResult.Attributes.Remove("max");
                TextBoxResult.Attributes.Add("min", "0");

                if (DropDownListMeasurement.SelectedValue == "Numbers")
                {
                    TextBoxTarget.Attributes.Add("step", "1");
                    TextBoxResult.Attributes.Add("step", "1");
                }
                else
                {
                    TextBoxTarget.Attributes.Add("step", "0.01");
                    TextBoxResult.Attributes.Add("step", "0.01");
                }

                DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                DropDownFormula.Enabled = true;
            }
        }

        protected void OnTargetChanged(object sender, EventArgs e)
        {
            if (DropDownListMeasurement.SelectedValue == "Month")
            {
                int target_value = int.Parse(TextBoxTarget.Text);
                string month_name;
                month_name_target.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                if (target_value > 12)
                {
                    TextBoxTarget.Text = "12";
                    month_name_target.InnerText = "December";
                }
                else if (target_value < 1)
                {
                    TextBoxTarget.Text = "1";
                    month_name_target.InnerText = "January";
                }
                else
                {
                    month_name = ShowMonthNameTarget(target_value);
                    month_name_target.InnerText = month_name;
                }
            }
            else
            {
                month_name_target.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
            }
        }

        protected void OnResultChanged(object sender, EventArgs e)
        {
            if (DropDownListMeasurement.SelectedValue == "Month")
            {
                int result_value = int.Parse(TextBoxResult.Text);
                string month_name;
                month_name_result.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                if (result_value > 12)
                {
                    TextBoxResult.Text = "12";
                    month_name_result.InnerText = "December";
                }
                else
                {
                    month_name = ShowMonthNameResult(result_value);
                    if (month_name == "-")
                    {
                        month_name_result.Attributes.Add("style", "width:300px; color:black; visibility:hidden; padding-bottom:0px; margin-top:0px");
                    }
                    else
                    {
                        month_name_result.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                        month_name_result.InnerText = month_name;
                    }
                }
            }
            else
            {
                month_name_result.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
            }
        }

        protected void OnKPIChanged(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            string user_nik = Session["user_nik"].ToString();
            string select_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header "
                                    + "WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' "
                                    + "AND IndividualHeader_ID<>" + header_id + " AND data_status='exist' "
                                    + "AND Period_ID=" + period_id + " AND user_id=" + Session["user_id"] + "";
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_select_KPI_name = new SqlCommand(select_KPI_name, conn);
                using (SqlDataReader KPIReader = sql_select_KPI_name.ExecuteReader())//mengecek apakah KPI sudah ada atau belum
                {
                    if (KPIReader.HasRows)
                    {
                        check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                    }
                    else
                    {
                        check_KPI_name_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px !important; margin-top:5px !important; color:red; font-weight:bold");
                    }
                } 
                conn.Close();
            }
        }

        protected void OnClickRequestChange(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            string superior_id = "";
            string user_nik = Session["user_nik"].ToString();
            string string_get_superior_id = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.EmpId='" + user_nik + "' ) "
                                             + "SELECT EmpId FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";
            string string_check_request = "SELECT * FROM IndividualHeader_RequestChange WHERE IndividualHeader_ID=" + header_id + " AND Approval_Status='pending' AND Period_ID=" + period_id + "";
            string select_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' "
                                    + "AND IndividualHeader_ID<>" + header_id + " AND data_status='exist' AND Period_ID=" + period_id + " AND user_id=" + Session["user_id"] + "";
            bool KPI_name_exist, req_exist = true, delete_kpi = false;
            float total_rating = 0, total_score, max_stretch;
            string user_update, date_update, date_create, user_create;

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();

                SqlCommand sql_get_superior_id = new SqlCommand(string_get_superior_id, conn);
                using (SqlDataReader SuperiorReader = sql_get_superior_id.ExecuteReader())
                {
                    if (SuperiorReader.HasRows)
                    {
                        while (SuperiorReader.Read())
                        {
                            superior_id = SuperiorReader["EmpId"].ToString();
                        }
                    }
                    SuperiorReader.Dispose();
                    SuperiorReader.Close();
                }

                if (superior_id == null) superior_id = "-";

                if (DropDownFormula.SelectedValue == "(Result/Target) x 100%")
                {
                    if (float.Parse(TextBoxResult.Text) == 0 && float.Parse(TextBoxTarget.Text) == 0)
                    {
                        total_rating = 0;//error handling untuk hasil yang UNLIMITED
                    }
                    else
                    {
                        total_rating = (float.Parse(TextBoxResult.Text) / float.Parse(TextBoxTarget.Text)) * 100;
                    }
                }
                else if (DropDownFormula.SelectedValue == "100% - ((Result - Target)/Target)")
                {
                    if (float.Parse(TextBoxResult.Text) == 0 && float.Parse(TextBoxTarget.Text) == 0)
                    {
                        total_rating = 0;//error handling untuk hasil yang UNLIMITED
                    }
                    else if (float.Parse(TextBoxResult.Text) == 0)
                    {
                        total_rating = 0;//error handling untuk hasil yang UNLIMITED
                    }
                    else
                    {
                        total_rating = 100 - (((float.Parse(TextBoxResult.Text) - float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text)) * 100);
                    }
                }
                else if (DropDownFormula.SelectedValue == "-")
                {
                    total_rating = float.Parse(TextBoxRating.Value);
                }

                //mengambil nilai max stretch rating dari tabel Financial Header. ASUMSI NIK ==> 100
                string string_select_max_stretch = "SELECT FinancialHeader_IndividualStretchRating FROM ScorecardUser "
                                                  + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                  + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                  + "WHERE ScorecardUser.EmpId='" + user_nik + "' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                SqlCommand sql_select_max_stretch = new SqlCommand(string_select_max_stretch, conn);
                max_stretch = float.Parse(sql_select_max_stretch.ExecuteScalar().ToString());

                if (total_rating > max_stretch)//agar tidak melebihi stretch rating yang sudah ditentukan
                {
                    total_rating = max_stretch;
                }

                total_score = (total_rating * (float.Parse(TextBoxWeight.Value))) / 100;

                SqlCommand sql_select_KPI_name = new SqlCommand(select_KPI_name, conn);
                SqlCommand sql_check_KPI_request = new SqlCommand(string_check_request, conn);
                using (SqlDataReader KPIReader = sql_select_KPI_name.ExecuteReader())//mengecek apakah KPI sudah ada atau belum
                {
                    if (KPIReader.HasRows)
                    {
                        check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        KPI_name_exist = true;
                    }
                    else
                    {
                        KPI_name_exist = false;
                    }
                } //end of KPIReader

                using (SqlDataReader RequestReader = sql_check_KPI_request.ExecuteReader())
                {
                    if (RequestReader.HasRows)
                    {
                        req_exist = true;
                    }
                    else
                    {
                        req_exist = false;
                    }
                    RequestReader.Dispose();
                    RequestReader.Close();
                }

                user_create = Session["user_name"].ToString();
                date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                user_update = Session["user_name"].ToString();
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (KPI_name_exist == false)
                {
                    if (DropDownListSpecific.SelectedValue == "No")
                    {
                        if (req_exist == false)
                        {
                            //int request_id;
                            TextBoxKPI.Text = TextBoxKPI.Text.Replace("'", "''");
                            TextareaObjective.InnerText = TextareaObjective.InnerText.Replace("'", "''");
                            TextareaReason.InnerText = TextareaReason.InnerText.Replace("'", "''");

                            string insert_request_change_header = "exec SP_InsertChangeRequestKPI '" + TextBoxKPI.Text + "', " + TextBoxTarget.Text + ", " + TextBoxResult.Text + ", "
                                                                + "" + total_rating + ", " + TextBoxWeight.Value + ", " + total_score + ", '" + TextareaObjective.InnerText + "', "
                                                                + "'" + DropDownFormula.SelectedValue + "', '" + DropDownListMeasurement.SelectedValue + "', '" + user_create + "', "
                                                                + "'" + date_create + "', '" + user_update + "', '" + date_update + "', " + header_id + ", 'pending', " + period_id + ", "
                                                                + "'" + superior_id + "', " + Session["user_id"].ToString() + ", '" + TextareaReason.InnerText + "', 0";
                            SqlCommand sql_insert_request_change_header = new SqlCommand(insert_request_change_header, conn);
                            sql_insert_request_change_header.ExecuteNonQuery();

                            sendMail(delete_kpi);

                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Your Request Has Been Sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                        }
                        else
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Request Has Been Sent. Cannot Request Change for This Item Again.')", true);
                        }
                    }
                    else if (DropDownListSpecific.SelectedValue == "Yes")//Jika ada Specific Objectives
                    {
                        if (req_exist == false)
                        {
                            string insert_request_change_header = "exec SP_InsertChangeRequestKPI '" + TextBoxKPI.Text + "', -1, " + TextBoxResult.Text + ", "
                                                                + "" + total_rating + ", " + TextBoxWeight.Value + ", " + total_score + ", '" + TextareaObjective.InnerText + "', "
                                                                + "'" + DropDownFormula.SelectedValue + "', '" + DropDownListMeasurement.SelectedValue + "', '" + user_create + "', "
                                                                + "'" + date_create + "', '" + user_update + "', '" + date_update + "', " + header_id + ", 'pending', " + period_id + ", "
                                                                + "'" + superior_id + "', " + Session["user_id"].ToString() + ", '" + TextareaReason.InnerText + "', 0";

                            SqlCommand sql_insert_request_change_header = new SqlCommand(insert_request_change_header, conn);

                            sql_insert_request_change_header.ExecuteNonQuery();

                            sendMail(delete_kpi);

                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Your Request Has Been Sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                        }
                        else
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Request Has Been Sent. Cannot Request Change for This Item Again.')", true);
                        }
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your KPI Already Exist.')", true);
                }
                conn.Close();
            }//end of SqlConnection
        }

        protected void OnClickRequestDelete(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            string superior_id = "";
            string user_nik = Session["user_nik"].ToString();
            string string_get_superior_id = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.EmpId='" + user_nik + "' ) "
                                             + "SELECT EmpId FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";
            string string_check_request = "SELECT * FROM IndividualHeader_RequestChange WHERE IndividualHeader_ID=" + header_id + " AND Approval_Status='pending' AND Period_ID=" + period_id + "";
            string select_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' "
                                    + "AND IndividualHeader_ID<>" + header_id + " AND data_status='exist' AND Period_ID=" + period_id + " AND user_id=" + Session["user_id"] + "";
            bool KPI_name_exist, req_exist = true, delete_kpi = true;
            float total_rating = 0, total_score, max_stretch;
            string user_update, date_update, date_create, user_create;

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();

                SqlCommand sql_get_email_superior = new SqlCommand(string_get_superior_id, conn);
                using (SqlDataReader SuperiorReader = sql_get_email_superior.ExecuteReader())
                {
                    if (SuperiorReader.HasRows)
                    {
                        while (SuperiorReader.Read())
                        {
                            superior_id = SuperiorReader["EmpId"].ToString();
                        }
                    }
                    SuperiorReader.Dispose();
                    SuperiorReader.Close();
                }

                if (superior_id == null) superior_id = "-";

                if (DropDownFormula.SelectedValue == "(Result/Target) x 100%")
                {
                    if (float.Parse(TextBoxResult.Text) == 0 && float.Parse(TextBoxTarget.Text) == 0)
                    {
                        total_rating = 0;//error handling untuk hasil yang UNLIMITED
                    }
                    else
                    {
                        total_rating = (float.Parse(TextBoxResult.Text) / float.Parse(TextBoxTarget.Text)) * 100;
                    }
                }
                else if (DropDownFormula.SelectedValue == "100% - ((Result - Target)/Target)")
                {
                    if (float.Parse(TextBoxResult.Text) == 0 && float.Parse(TextBoxTarget.Text) == 0)
                    {
                        total_rating = 0;//error handling untuk hasil yang UNLIMITED
                    }
                    else if (float.Parse(TextBoxResult.Text) == 0)
                    {
                        total_rating = 0;//error handling untuk hasil yang UNLIMITED
                    }
                    else
                    {
                        total_rating = 100 - (((float.Parse(TextBoxResult.Text) - float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text)) * 100);
                    }
                }
                else if (DropDownFormula.SelectedValue == "-")
                {
                    total_rating = float.Parse(TextBoxRating.Value);
                }

                //mengambil nilai max stretch rating dari tabel Financial Header. ASUMSI NIK ==> 100
                string string_select_max_stretch = "SELECT FinancialHeader_IndividualStretchRating FROM ScorecardUser "
                                                  + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                  + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                  + "WHERE ScorecardUser.EmpId='" + user_nik + "' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                SqlCommand sql_select_max_stretch = new SqlCommand(string_select_max_stretch, conn);
                max_stretch = float.Parse(sql_select_max_stretch.ExecuteScalar().ToString());

                if (total_rating > max_stretch)//agar tidak melebihi stretch rating yang sudah ditentukan
                {
                    total_rating = max_stretch;
                }

                total_score = (total_rating * (float.Parse(TextBoxWeight.Value))) / 100;

                SqlCommand sql_select_KPI_name = new SqlCommand(select_KPI_name, conn);
                SqlCommand sql_check_KPI_request = new SqlCommand(string_check_request, conn);
                using (SqlDataReader KPIReader = sql_select_KPI_name.ExecuteReader())//mengecek apakah KPI sudah ada atau belum
                {
                    if (KPIReader.HasRows)
                    {
                        check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        KPI_name_exist = true;
                    }
                    else
                    {
                        KPI_name_exist = false;
                    }
                } //end of KPIReader

                using (SqlDataReader RequestReader = sql_check_KPI_request.ExecuteReader())
                {
                    if (RequestReader.HasRows)
                    {
                        req_exist = true;
                    }
                    else
                    {
                        req_exist = false;
                    }
                    RequestReader.Dispose();
                    RequestReader.Close();
                }

                user_create = Session["user_name"].ToString();
                date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                user_update = Session["user_name"].ToString();
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (KPI_name_exist == false)
                {
                    if (DropDownListSpecific.SelectedValue == "No")
                    {
                        if (req_exist == false)
                        {
                            //int request_id;
                            TextBoxKPI.Text = TextBoxKPI.Text.Replace("'", "''");
                            TextareaObjective.InnerText = TextareaObjective.InnerText.Replace("'", "''");
                            TextareaReason.InnerText = TextareaReason.InnerText.Replace("'", "''");

                            string insert_request_change_header = "exec SP_InsertChangeRequestKPI '" + TextBoxKPI.Text + "', " + TextBoxTarget.Text + ", " + TextBoxResult.Text + ", "
                                                                + "" + total_rating + ", " + TextBoxWeight.Value + ", " + total_score + ", '" + TextareaObjective.InnerText + "', "
                                                                + "'" + DropDownFormula.SelectedValue + "', '" + DropDownListMeasurement.SelectedValue + "', '" + user_create + "', "
                                                                + "'" + date_create + "', '" + user_update + "', '" + date_update + "', " + header_id + ", 'pending', " + period_id + ", "
                                                                + "'" + superior_id + "', " + Session["user_id"].ToString() + ", '" + TextareaReason.InnerText + "', 1";
                            SqlCommand sql_insert_request_change_header = new SqlCommand(insert_request_change_header, conn);
                            sql_insert_request_change_header.ExecuteNonQuery();

                            sendMail(delete_kpi);

                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Your Request Has Been Sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                        }
                        else
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Request Has Been Sent. Cannot Request Change for This Item Again.')", true);
                        }
                    }
                    else if (DropDownListSpecific.SelectedValue == "Yes")//Jika ada Specific Objectives
                    {
                        if (req_exist == false)
                        {
                            string insert_request_change_header = "exec SP_InsertChangeRequestKPI '" + TextBoxKPI.Text + "', -1, " + TextBoxResult.Text + ", "
                                                                + "" + total_rating + ", " + TextBoxWeight.Value + ", " + total_score + ", '" + TextareaObjective.InnerText + "', "
                                                                + "'" + DropDownFormula.SelectedValue + "', '" + DropDownListMeasurement.SelectedValue + "', '" + user_create + "', "
                                                                + "'" + date_create + "', '" + user_update + "', '" + date_update + "', " + header_id + ", 'pending', " + period_id + ", "
                                                                + "'" + superior_id + "', " + Session["user_id"].ToString() + ", '" + TextareaReason.InnerText + "', 1";

                            SqlCommand sql_insert_request_change_header = new SqlCommand(insert_request_change_header, conn);

                            sql_insert_request_change_header.ExecuteNonQuery();

                            sendMail(delete_kpi);

                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Your Request Has Been Sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                        }
                        else
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Request Has Been Sent. Cannot Request Change for This Item Again.')", true);
                        }
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your KPI Already Exist.')", true);
                }
                conn.Close();
            }//end of SqlConnection
        }

        protected void OnSelectSpecific(object sender, EventArgs e)
        {
            var header_id = Request.QueryString["header_id"];
            string select_individual_header = "SELECT * FROM IndividualMeasures_Header WHERE IndividualHeader_ID=" + header_id + "";
            if (DropDownListSpecific.SelectedValue == "Yes")//jika ada SO
            {
                double check_target;
                string string_get_target = "SELECT IndividualHeader_Target FROM IndividualMeasures_Header WHERE IndividualHeader_ID=" + header_id + "";
                SqlConnection conn = new SqlConnection(str_connect);
                SqlCommand sql_get_target = new SqlCommand(string_get_target, conn);
                conn.Open();
                check_target = (double)sql_get_target.ExecuteScalar();
                conn.Close();

                if (check_target == -1)//dari "YES" ke "YES" lagi
                {
                    string string_select_header_value = "SELECT * FROM IndividualMeasures_Header WHERE IndividualHeader_ID=" + header_id + "";
                    SqlConnection header_connect = new SqlConnection(str_connect);
                    header_connect.Open();
                    SqlCommand sql_select_header_value = new SqlCommand(string_select_header_value, header_connect);
                    using (SqlDataReader HeaderReader = sql_select_header_value.ExecuteReader())
                    {
                        while (HeaderReader.Read())//kalau pilih "YES" dan sudah ada data di dalamnya
                        {
                            TextBoxTarget.Attributes.Add("disabled", "true");
                            TextBoxTarget.Text = "0";
                            TextBoxResult.Attributes.Add("disabled", "true");
                            TextBoxResult.Text = HeaderReader["IndividualHeader_Result"].ToString();
                            DropDownListMeasurement.Enabled = false;
                            DropDownFormula.Enabled = false;
                            DropDownListMeasurement.Items.Add("-");
                            DropDownListMeasurement.SelectedValue = "-";
                            DropDownFormula.Items.Add("-");
                            DropDownFormula.SelectedValue = "-";
                            TextBoxRating.Attributes.Add("readonly", "true");
                            TextBoxRating.Value = HeaderReader["IndividualHeader_Rating"].ToString();
                            TextBoxScore.Attributes.Add("readonly", "true");
                            TextBoxScore.Value = HeaderReader["IndividualHeader_Score"].ToString();
                            TextBoxWeight.Value = HeaderReader["IndividualHeader_Weight"].ToString();
                        }
                    }
                    header_connect.Close();
                }
                else//Dari "NO" ke "YES"
                {
                    TextBoxTarget.Attributes.Add("disabled", "true");
                    TextBoxTarget.Text = "0";
                    TextBoxResult.Attributes.Add("disabled", "true");
                    TextBoxResult.Text = "0";
                    DropDownListMeasurement.Enabled = false;
                    DropDownFormula.Enabled = false;
                    TextBoxRating.Attributes.Add("readonly", "true");
                    TextBoxRating.Value = "0";
                    TextBoxScore.Attributes.Add("readonly", "true");
                    TextBoxScore.Value = "0";
                    TextBoxRating.Value = "0";
                    TextBoxWeight.Value = "0";
                    DropDownListMeasurement.Items.Add("-");
                    DropDownFormula.Items.Add("-");
                    DropDownListMeasurement.SelectedValue = "-";
                    DropDownFormula.SelectedValue = "-";
                    if (DropDownListMeasurement.SelectedValue == "-")
                    {
                        month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                        month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                    }
                }
            }
            else if (DropDownListSpecific.SelectedValue == "No")//jika tidak ada SO
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    SqlCommand sql_select_individual_header = new SqlCommand(select_individual_header, conn);
                    conn.Open();
                    using (SqlDataReader IndividualHeaderReader = sql_select_individual_header.ExecuteReader())
                    {
                        if (IndividualHeaderReader.HasRows)
                        {
                            while (IndividualHeaderReader.Read())
                            {
                                if (IndividualHeaderReader["IndividualHeader_Target"].ToString() == "-1")//dari "YES" ke "NO"
                                {
                                    TextBoxTarget.Text = "0";
                                    TextBoxTarget.Attributes.Remove("disabled");
                                    DropDownListMeasurement.Enabled = true;
                                    DropDownFormula.Enabled = true;

                                    DropDownListMeasurement.Items.Remove("-");
                                    DropDownListMeasurement.SelectedValue = "%";

                                    DropDownFormula.Items.Remove("-");
                                    DropDownFormula.SelectedValue = "(Result/Target) x 100%";

                                    TextBoxResult.Attributes.Remove("disabled");
                                    TextBoxRating.Value = "0";
                                    TextBoxRating.Attributes.Add("disabled", "true");
                                    TextBoxWeight.Value = "0";
                                    TextBoxScore.Value = "0";
                                    TextBoxScore.Attributes.Add("disabled", "true");
                                }
                                else//dari "NO" ke "NO"
                                {
                                    TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                    TextBoxTarget.Attributes.Remove("disabled");
                                    TextBoxResult.Attributes.Remove("disabled");
                                    TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                    DropDownListMeasurement.Enabled = true;
                                    DropDownFormula.Enabled = true;
                                    DropDownListMeasurement.Items.Remove("-");
                                    DropDownFormula.Items.Remove("-");

                                    if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                    {
                                        DropDownListMeasurement.SelectedValue = "%";
                                    }
                                    else
                                    {
                                        DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();//untuk yang dari "Don'Use" jadi "Use"
                                    }

                                    if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                    {
                                        DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                                    }
                                    else
                                    {
                                        DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();//untuk yang dari "Don'Use" jadi "Use"
                                    }
                                    TextBoxRating.Attributes.Add("disabled", "true");
                                    TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                    TextBoxScore.Attributes.Add("disabled", "true");
                                    TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                    if (DropDownListMeasurement.SelectedValue == "Month")
                                    {
                                        month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                        month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }//end of SqlConnection
            }
        }

        public void sendMail(bool delete_kpi)
        {
            string strApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];

            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];

            string user_nik = (string)Session["user_nik"];

            string superior_email = "";
            string string_get_superior_email = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.EmpId='" + user_nik + "' ) "
                                             + "SELECT empEmail FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";
            string string_get_user_info = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(ScorecardUser.empEmail) as Email, "
                                        + "empGrade, Group_Name, IndividualHeader_KPI, FinancialHeader_IndividualStretchRating, EmpSex, EmpMaritalSt "
                                        + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                        + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                        + "join [Human_Capital_demo].dbo.Employee on ScorecardUser.EmpId=Employee.EmpId "
                                        + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                        + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                        + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                        + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                        + "join IndividualMeasures_Header on ScorecardUser.user_id = IndividualMeasures_Header.user_id "
                                        + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                        + "AND FinancialMeasures_Header.data_status='exist' "
                                        + "WHERE IndividualHeader_ID=" + header_id + " AND FinancialMeasures_Header.Period_ID=" + period_id + "";
            string string_get_current_KPI = "SELECT * FROM IndividualMeasures_Header WHERE IndividualHeader_ID=" + header_id + "";
            string string_get_new_KPI = "SELECT * FROM IndividualHeader_RequestChange WHERE IndividualHeader_ID=" + header_id + " AND Period_ID=" + period_id + " "
                                      + "AND Approval_Status='pending'";
            string user_title = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_get_user_info = new SqlCommand(string_get_user_info, conn);
                SqlCommand sql_get_current_KPI = new SqlCommand(string_get_current_KPI, conn);
                SqlCommand sql_get_new_KPI = new SqlCommand(string_get_new_KPI, conn);

                StringBuilder sb_subject = new StringBuilder();
                StringBuilder sb_body_introduction = new StringBuilder();
                StringBuilder sb_from_email = new StringBuilder();
                StringBuilder sb_current_detail = new StringBuilder();
                StringBuilder sb_new_detail = new StringBuilder();
                StringBuilder sb_conclusion = new StringBuilder();

                using (SqlDataReader UserReader = sql_get_user_info.ExecuteReader())
                {
                    while (UserReader.Read())
                    {
                        if (UserReader["EmpSex"].ToString() == "F" && UserReader["EmpMaritalSt"].ToString() == "NIKAH")
                        {
                            user_title = "Ms.";
                        }
                        else if (UserReader["EmpSex"].ToString() == "F" && UserReader["EmpMaritalSt"].ToString() == "BELUM NIKAH")
                        {
                            user_title = "Mrs.";
                        }
                        else if (UserReader["EmpSex"].ToString() == "F" && DBNull.Value.Equals(UserReader["EmpMaritalSt"]))
                        {
                            user_title = "Ms.";
                        }
                        else if (UserReader["EmpSex"].ToString() == "M")
                        {
                            user_title = "Mr.";
                        }

                        if (delete_kpi == false)
                        {
                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Change KPI (" + user_title + " " + UserReader["empName"].ToString() + " - " + UserReader["EmpId"].ToString() + ")");
                            sb_body_introduction.Append("Hello, my name is " + user_title + " " + UserReader["empName"].ToString() + " and this is my information: <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to change my KPI from:<br/><br/>");
                        }
                        else
                        {
                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Delete KPI (" + user_title + " " + UserReader["empName"].ToString() + " - " + UserReader["EmpId"].ToString() + ")");
                            sb_body_introduction.Append("Hello, my name is " + user_title + " " + UserReader["empName"].ToString() + " and this is my information: <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to delete the following KPI:<br/><br/>");
                        }

                        sb_conclusion.Append("Link to Balanced Scorecard Application: " + strApplicationURL + " <br/><br/>Thank you. <br/><br/>Best Regards, <br/>"+user_title+" " + UserReader["empName"].ToString() + ""
                                           + "<br/><br/>This is an automatically generated email – please do not reply to it.");
                    }
                    UserReader.Dispose();
                    UserReader.Close();
                }

                if (delete_kpi == false)
                {
                    using (SqlDataReader CurrentReader = sql_get_current_KPI.ExecuteReader())
                    {
                        while (CurrentReader.Read())
                        {
                            if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                            {
                                string month_name_target, month_name_result;
                                month_name_target = ShowMonthNameTarget(int.Parse(CurrentReader["IndividualHeader_Target"].ToString()));
                                month_name_result = ShowMonthNameResult(int.Parse(CurrentReader["IndividualHeader_Result"].ToString()));

                                sb_current_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                                + "<tr>"
                                                + "<th style='border:1px solid black; padding:8px'>KPI</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Objective</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Weight</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Score</th>"
                                                + "</tr>"
                                                + "<tr>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_KPI"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Objective"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + month_name_target + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + month_name_result + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Formula"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Rating"].ToString() + "%</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Weight"].ToString() + "%</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Score"].ToString() + "%</td>"
                                                + "</tr>"
                                                + "</table><br/> to:");
                            }
                            else if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                            {
                                sb_current_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                                + "<tr>"
                                                + "<th style='border:1px solid black; padding:8px'>KPI</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Objective</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Weight</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Score</th>"
                                                + "</tr>"
                                                + "<tr>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_KPI"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Objective"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Target"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Result"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Formula"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Rating"].ToString() + "%</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Weight"].ToString() + "%</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Score"].ToString() + "%</td>"
                                                + "</tr>"
                                                + "</table><br/> to:");
                            }
                            else
                            {
                                string based_on_schedule = "", string_result = "";

                                if (CurrentReader["IndividualHeader_Target"].ToString() == "-1")
                                {
                                    based_on_schedule = "Based On Schedule";
                                }
                                else
                                {
                                    based_on_schedule = CurrentReader["IndividualHeader_Target"].ToString() + " " + CurrentReader["IndividualHeader_MeasureBy"].ToString();
                                }

                                if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                {
                                    string_result = "-";
                                }
                                else
                                {
                                    string_result = CurrentReader["IndividualHeader_Result"].ToString() + " " + CurrentReader["IndividualHeader_MeasureBy"].ToString();
                                }

                                sb_current_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                                + "<tr>"
                                                + "<th style='border:1px solid black; padding:8px'>KPI</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Objective</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Weight</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Score</th>"
                                                + "</tr>"
                                                + "<tr>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_KPI"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Objective"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + based_on_schedule + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Result"].ToString() + " " + CurrentReader["IndividualHeader_MeasureBy"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Formula"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Rating"].ToString() + "%</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Weight"].ToString() + "%</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualHeader_Score"].ToString() + "%</td>"
                                                + "</tr>"
                                                + "</table><br/> to:");
                            }

                        }
                        CurrentReader.Dispose();
                        CurrentReader.Close();
                    }
                }

                using (SqlDataReader NewReader = sql_get_new_KPI.ExecuteReader())
                {
                    while (NewReader.Read())
                    {
                        if (NewReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                        {
                            string month_name_target, month_name_result;
                            month_name_target = ShowMonthNameTarget(int.Parse(NewReader["IndividualHeader_Target"].ToString()));
                            month_name_result = ShowMonthNameResult(int.Parse(NewReader["IndividualHeader_Result"].ToString()));
                            sb_new_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                            + "<tr>"
                                            + "<th style='border:1px solid black; padding:8px'>KPI</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Objective</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Weight</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Score</th>"
                                            + "</tr>"
                                            + "<tr>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_KPI"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Objective"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + month_name_target + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + month_name_result + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Formula"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Rating"].ToString() + "%</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Weight"].ToString() + "%</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Score"].ToString() + "%</td>"
                                            + "</tr>"
                                            + "</table>with reason: " + NewReader["IndividualHeaderReason"].ToString() + "<br/><br/>");
                        }
                        else if (NewReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                        {
                            sb_new_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                            + "<tr>"
                                            + "<th style='border:1px solid black; padding:8px'>KPI</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Objective</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Weight</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Score</th>"
                                            + "</tr>"
                                            + "<tr>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_KPI"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Objective"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Target"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Result"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Formula"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Rating"].ToString() + "%</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Weight"].ToString() + "%</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Score"].ToString() + "%</td>"
                                            + "</tr>"
                                            + "</table>with reason: " + NewReader["IndividualHeaderReason"].ToString() + "<br/><br/>");
                        }
                        else
                        {
                            string based_on_schedule = "", string_result = "";

                            if (NewReader["IndividualHeader_Target"].ToString() == "-1")
                            {
                                based_on_schedule = "Based On Schedule";
                            }
                            else
                            {
                                based_on_schedule = NewReader["IndividualHeader_Target"].ToString() + " " + NewReader["IndividualHeader_MeasureBy"].ToString();
                            }

                            if (NewReader["IndividualHeader_MeasureBy"].ToString() == "-")
                            {
                                string_result = "-";
                            }
                            else
                            {
                                string_result = NewReader["IndividualHeader_Result"].ToString() + " " + NewReader["IndividualHeader_MeasureBy"].ToString();
                            }

                            sb_new_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                            + "<tr>"
                                            + "<th style='border:1px solid black; padding:8px'>KPI</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Objective</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Weight</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Score</th>"
                                            + "</tr>"
                                            + "<tr>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_KPI"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Objective"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + based_on_schedule + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + string_result + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Formula"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Rating"].ToString() + "%</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Weight"].ToString() + "%</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Score"].ToString() + "%</td>"
                                            + "</tr>"
                                            + "</table>with reason: " + NewReader["IndividualHeaderReason"].ToString() + "<br/><br/>");
                        }
                    }
                    NewReader.Dispose();
                    NewReader.Close();
                }

                SqlCommand sql_get_email_superior = new SqlCommand(string_get_superior_email, conn);
                using (SqlDataReader SuperiorReader = sql_get_email_superior.ExecuteReader())
                {
                    if (SuperiorReader.HasRows)
                    {
                        while (SuperiorReader.Read())
                        {
                            superior_email = SuperiorReader["empEmail"].ToString();
                        }
                    }
                    SuperiorReader.Dispose();
                    SuperiorReader.Close();
                }

                SmtpClient mailclient = new SmtpClient();  //Karena FILE_LOCATION terjadi perubahan setiap di-klik, maka
                using (MailMessage msg = new MailMessage())//harus pake USING untuk CLEAR semua Resource yang pernah dipake
                {
                    /******************** SEND Email TO Users **************************************/
                    msg.Subject = sb_subject.ToString();
                    if (delete_kpi == false)
                    {
                        msg.Body = sb_body_introduction.ToString() + sb_current_detail.ToString() + sb_new_detail.ToString() + sb_conclusion.ToString();
                    }
                    else
                    {
                        msg.Body = sb_body_introduction.ToString() + sb_new_detail.ToString() + sb_conclusion.ToString();
                    }

                    if (sb_from_email.ToString() == "" || sb_from_email.ToString() == "-")
                    {
                        msg.From = new MailAddress("message@error.com");
                    }
                    else
                    {
                        msg.From = new MailAddress(sb_from_email.ToString());
                    }
                    if (superior_email == "" || superior_email == "-")
                    {
                        msg.To.Add("message@error.com");//<-- E-Mail atasan
                    }
                    else
                    {
                        msg.To.Add(superior_email);//<-- E-Mail atasan
                    }
                    msg.IsBodyHtml = true;
                    mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                    mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                    mailclient.Send(msg);
                }
                conn.Close();
            }
        }

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