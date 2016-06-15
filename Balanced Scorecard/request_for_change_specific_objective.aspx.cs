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
    public partial class request_for_change_specific_objective : System.Web.UI.Page
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
                var period_id = Request.QueryString["period_id"];
                var header_id = Request.QueryString["header_id"];
                var detail_id = Request.QueryString["detail_id"];
                int current_month, current_year;
                int start_month_bsc = 0, year_from_start_date = 0, minus_one_bsc = 0;

                string user_nik = Session["user_nik"].ToString();

                current_month = DateTime.Now.Month;
                current_year = DateTime.Now.Year;

                SqlConnection period_conn = new SqlConnection(str_connect);
                period_conn.Open();
                string string_select_start_period_month = "SELECT Start_Period FROM BSC_Period WHERE Period_ID=" + period_id + "";
                SqlCommand sql_select_start_period_month = new SqlCommand(string_select_start_period_month, period_conn);
                using (SqlDataReader StartMonthReader = sql_select_start_period_month.ExecuteReader())
                {
                    while (StartMonthReader.Read())
                    {
                        DateTime start_date = Convert.ToDateTime(StartMonthReader["Start_Period"]);
                        start_month_bsc = int.Parse(start_date.ToString("MM"));
                        year_from_start_date = int.Parse(start_date.ToString("yyyy"));
                    }
                    StartMonthReader.Dispose();
                    StartMonthReader.Close();
                }
                period_conn.Close();

                if (start_month_bsc == 1)
                {
                    minus_one_bsc = 12;
                }
                else
                {
                    minus_one_bsc = start_month_bsc - 1;
                }

                //link breadcrumb
                individual_scorecard_breadcrumb.Attributes.Add("a href", "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                //untuk item pada Drop Down Measurement
                DropDownListMeasurement.Items.Add("%");
                //DropDownListMeasurement.Items.Add("Days");
                DropDownListMeasurement.Items.Add("Month");
                DropDownListMeasurement.Items.Add("Million");
                DropDownListMeasurement.Items.Add("Numbers");

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

                //Add Item to DropDown Formula
                DropDownFormula.Items.Add("(Result/Target) x 100%");
                DropDownFormula.Items.Add("100% - ((Result - Target)/Target)");
                DropDownFormula.Enabled = true;

                //TextBoxRating
                TextBoxRating.Attributes.Add("disabled", "true");

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
                    string select_individual_header = "SELECT * FROM IndividualMeasures_Header WHERE IndividualHeader_ID=" + header_id + "";
                    string select_individual_detail = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualDetail_ID=" + detail_id + " AND IndividualHeader_ID=" + header_id + "";
                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                    SqlCommand sql_select_individual_header = new SqlCommand(select_individual_header, conn);
                    SqlCommand sql_select_individual_detail = new SqlCommand(select_individual_detail, conn);
                    SqlCommand sql_select_individual_stretch_review = new SqlCommand(string_select_individual_stretch_review, conn);
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

                    using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                    {
                        if (PeriodReader.HasRows)
                        {
                            while (PeriodReader.Read())
                            {
                                string start_date_formatted, end_date_formatted;
                                DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                start_date_formatted = start_date.ToString("MMM,dd-yyyy");
                                end_date_formatted = end_date.ToString("MMM,dd-yyyy");
                                LabelStartPeriod.InnerText = start_date_formatted;
                                LabelEndPeriod.InnerText = end_date_formatted;
                                cancel_edit_specific.Attributes.Add("href", "individual_scorecard.aspx?page=" + page + "&id=" + PeriodReader["Period_ID"] + "");
                            }
                        }
                        else//jika periode tidak ditemukkan / disuntik langsung ke Database
                        {
                            LabelStartPeriod.InnerText = "No Period";
                            LabelEndPeriod.InnerText = "No Period";
                            SpanEditSpecific.Attributes.Add("class", "btn btn-add-more btn-add-more-container add-button disabled");
                            cancel_edit_specific.Attributes.Add("href", "individual_scorecard.aspx?page=1&id=1");
                        }
                    }

                    using (SqlDataReader HeaderReader = sql_select_individual_header.ExecuteReader())
                    {
                        if (HeaderReader.HasRows)//Error Handling jika ada yang inject langsung
                        {
                            while (HeaderReader.Read())
                            {
                                LabelKPI.InnerText = HeaderReader["IndividualHeader_KPI"].ToString();
                                LabelBreadcrumb.Text = HeaderReader["IndividualHeader_KPI"].ToString();
                                LabelTitle.Text = HeaderReader["IndividualHeader_KPI"].ToString();
                            }
                        }
                        else//jika Header tidak ditemukkan / disuntik langsung ke Database
                        {
                            LabelKPI.InnerText = "KPI Not Found";
                            LabelBreadcrumb.Text = "Unknown";
                            LabelTitle.Text = "Unknown";
                            SpanEditSpecific.Attributes.Add("class", "btn btn-add-more btn-add-more-container add-button disabled");
                            cancel_edit_specific.Attributes.Add("href", "individual_scorecard.aspx?page=1&id=1");
                        }
                    }

                    using (SqlDataReader SpecificReader = sql_select_individual_detail.ExecuteReader())
                    {
                        if (SpecificReader.HasRows)//jika Specific Detail ID ditemukan
                        {
                            while (SpecificReader.Read())
                            {
                                TextBoxSpecificObjective.Text = SpecificReader["IndividualDetail_Title"].ToString();
                                TextBoxTarget.Text = SpecificReader["IndividualDetail_Target"].ToString();
                                TextBoxResult.Text = SpecificReader["IndividualDetail_Result"].ToString();
                                DropDownListMeasurement.SelectedValue = SpecificReader["IndividualDetail_MeasureBy"].ToString();
                                DropDownFormula.SelectedValue = SpecificReader["IndividualDetail_Formula"].ToString();
                                TextBoxRating.Value = SpecificReader["IndividualDetail_Rating"].ToString();

                                if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                {
                                    if (current_month == start_month_bsc)
                                    {
                                        int target_value = int.Parse(TextBoxTarget.Text);
                                        string target_month_name;
                                        target_month_name = ShowMonthNameTarget(target_value);
                                        month_name_target.InnerText = target_month_name;
                                        TextBoxTarget.Attributes.Add("max", "12");
                                        TextBoxTarget.Attributes.Add("step", "1");
                                        month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                    }
                                    else if (current_month == minus_one_bsc && current_year == year_from_start_date - 1)
                                    {
                                        int target_value = int.Parse(TextBoxTarget.Text);
                                        string target_month_name;
                                        target_month_name = ShowMonthNameTarget(target_value);
                                        month_name_target.InnerText = target_month_name;
                                        TextBoxTarget.Attributes.Add("max", "12");
                                        TextBoxTarget.Attributes.Add("step", "1");
                                        month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                    }
                                    else
                                    {
                                        DropDownFormula.Enabled = false;
                                        TextBoxResult.Attributes.Add("max", "12");
                                        TextBoxResult.Attributes.Add("step", "1");
                                        int target_value = int.Parse(TextBoxTarget.Text);
                                        string target_month_name;
                                        target_month_name = ShowMonthNameTarget(target_value);
                                        month_name_target.InnerText = target_month_name;
                                        //tampilakan Month Name
                                        month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                        if (SpecificReader["IndividualDetail_Result"].ToString() == "0")
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
                                }
                                else
                                {
                                    //tampilakan Month Name
                                    month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                    month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                }
                            }
                        }
                        else//jika Specific Detail ID TIDAK ditemukan
                        {
                            DropDownListMeasurement.SelectedValue = "%";
                            DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                            SpanEditSpecific.Attributes.Add("class", "btn btn-add-more btn-add-more-container add-button disabled");
                            cancel_edit_specific.Attributes.Add("href", "individual_scorecard.aspx?page=1&id=1");
                        }
                    }
                    conn.Close();
                }
            }//end of if(!IsPostBack)
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

        protected void OnSelectMeasureBy(object sender, EventArgs e)
        {
            if (DropDownListMeasurement.SelectedValue == "Month")
            {
                int target_value = int.Parse(TextBoxTarget.Text);
                int result_value = int.Parse(TextBoxResult.Text);
                string month_name;
                month_name_target.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                month_name_result.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
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
                    month_name_target.InnerText = "December";
                }
                else if (result_value < 0)
                {
                    TextBoxResult.Text = "0";
                    month_name_target.InnerText = "";
                }
                else
                {
                    month_name = ShowMonthNameResult(result_value);
                    month_name_result.InnerText = month_name;
                }
                DropDownFormula.SelectedValue = "100% - ((Result - Target)/Target)";
                DropDownFormula.Enabled = false;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Maximum Target value for Measured By month is 12 and minimum value is 1')", true);
            }
            else
            {
                month_name_result.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                month_name_target.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
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

        protected void OnSpecificChanged(object sender, EventArgs e)
        {
            var header_id = Request.QueryString["header_id"];
            var detail_id = Request.QueryString["detail_id"];
            string check_specific_objective = "SELECT IndividualDetail_Title FROM IndividualMeasures_Detail "
                                            + "WHERE IndividualDetail_Title='" + TextBoxSpecificObjective.Text + "' "
                                            + "AND data_status='exist' AND IndividualDetail_ID<>" + detail_id + " "
                                            + "AND IndividualHeader_ID=" + header_id + "";
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                using (SqlDataReader SOReader = check_SO.ExecuteReader())
                {
                    if (SOReader.HasRows)//jika nama KPI sudah ada
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                    }
                    else
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px !important; margin-top:5px !important; color:red; font-weight:bold");
                    }
                }
                conn.Close();
            }
        }

        protected void OnClickRequestSpecific(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            var detail_id = Request.QueryString["detail_id"];
            float total_rating = 0, max_stretch;
            bool SO_exist, req_exist = true, delete_specific = false;
            string user_update, date_update, user_create, date_create, superior_id = "";
            string user_nik = Session["user_nik"].ToString();

            string string_get_superior_id = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.EmpId='" + user_nik + "' ) "
                                             + "SELECT EmpId FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";

            string check_specific_objective = "SELECT IndividualDetail_Title FROM IndividualMeasures_Detail "
                                            + "WHERE IndividualDetail_Title='" + TextBoxSpecificObjective.Text + "' "
                                            + "AND data_status='exist' AND IndividualDetail_ID<>" + detail_id + " "
                                            + "AND IndividualHeader_ID=" + header_id + "";

            //untuk PERHITUNGAN
            if (DropDownFormula.SelectedValue == "(Result/Target) x 100%")
            {
                if (TextBoxResult.Text == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else
                {
                    total_rating = (float.Parse(TextBoxResult.Text) / float.Parse(TextBoxTarget.Text)) * 100;
                }
            }
            else if (DropDownFormula.SelectedValue == "100% - ((Result - Target)/Target)")
            {
                if (TextBoxResult.Text == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else if (TextBoxResult.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else
                {
                    total_rating = 100 - (((float.Parse(TextBoxResult.Text) - float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text)) * 100);
                }
            }

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                string string_check_request = "SELECT * FROM IndividualDetail_RequestChange WHERE IndividualDetail_ID=" + detail_id + " "
                                            + "AND Approval_Status='pending' AND Period_ID=" + period_id + "";
                string string_select_max_stretch = "SELECT FinancialHeader_IndividualStretchRating FROM ScorecardUser "
                                                  + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                  + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                  + "WHERE ScorecardUser.EmpId='" + user_nik + "' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                SqlCommand sql_select_max_stretch = new SqlCommand(string_select_max_stretch, conn);
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

                max_stretch = float.Parse(sql_select_max_stretch.ExecuteScalar().ToString());

                if (total_rating > max_stretch)//agar tidak melebihi stretch rating yang sudah ditentukan
                {
                    total_rating = max_stretch;
                }

                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                SqlCommand sql_check_request = new SqlCommand(string_check_request, conn);

                user_create = Session["user_name"].ToString();
                user_update = Session["user_name"].ToString();
                date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                using (SqlDataReader SOReader = check_SO.ExecuteReader())
                {
                    if (SOReader.HasRows)//jika nama KPI sudah ada
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SO_exist = true;//jika nama SO yang sama ditemukan
                    }
                    else
                    {
                        SO_exist = false;
                    }
                }

                using (SqlDataReader RequestReader = sql_check_request.ExecuteReader())
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

                if (SO_exist == false)//jika nama SO tidak ditemukan / unique
                {
                    if (req_exist == false)
                    {
                        //int request_id;
                        TextBoxSpecificObjective.Text = TextBoxSpecificObjective.Text.Replace("'","''");
                        TextareaReason.InnerText = TextareaReason.InnerText.Replace("'", "''");
                        string insert_request_detail = "exec SP_InsertChangeRequestSpecificObjective '" + TextBoxSpecificObjective.Text + "', " + TextBoxTarget.Text + ", "
                                                     + "" + TextBoxResult.Text + ", '" + DropDownListMeasurement.SelectedValue + "', " + total_rating + ", '" + DropDownFormula.SelectedValue + "', "
                                                     + "'pending', " + detail_id + ", '" + user_create + "', '" + date_create + "', '" + user_update + "', '" + date_update + "', " + period_id + ", "
                                                     + "'" + superior_id + "', " + Session["user_id"].ToString() + ", " + header_id + ", '" + TextareaReason.InnerText + "', 0";

                        SqlCommand sql_insert_request_detail = new SqlCommand(insert_request_detail, conn);
                        sql_insert_request_detail.ExecuteNonQuery();
                        sendMail(delete_specific);
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Your Request Has Been Sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Request Has Been Sent. Cannot Request Change for This Item Again.')", true);
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Specific Objective Already Exist.')", true);
                }
                conn.Close();
            }//end of SqlConnection
        }

        protected void OnClickDeleteSpecific(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            var detail_id = Request.QueryString["detail_id"];
            float total_rating = 0, max_stretch;
            bool SO_exist, req_exist = true, delete_specific = true;
            string user_update, date_update, user_create, date_create, superior_id = "";
            string user_nik = Session["user_nik"].ToString();

            string string_get_superior_id = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.EmpId='" + user_nik + "' ) "
                                             + "SELECT EmpId FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";

            string check_specific_objective = "SELECT IndividualDetail_Title FROM IndividualMeasures_Detail WHERE IndividualDetail_Title='" + TextBoxSpecificObjective.Text + "' "
                                            + "AND data_status='exist' AND IndividualDetail_ID<>" + detail_id + " AND IndividualHeader_ID=" + header_id + "";

            //untuk PERHITUNGAN
            if (DropDownFormula.SelectedValue == "(Result/Target) x 100%")
            {
                if (TextBoxResult.Text == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else
                {
                    total_rating = (float.Parse(TextBoxResult.Text) / float.Parse(TextBoxTarget.Text)) * 100;
                }
            }
            else if (DropDownFormula.SelectedValue == "100% - ((Result - Target)/Target)")
            {
                if (TextBoxResult.Text == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else if (TextBoxResult.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else
                {
                    total_rating = 100 - (((float.Parse(TextBoxResult.Text) - float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text)) * 100);
                }
            }

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                string string_check_request = "SELECT * FROM IndividualDetail_RequestChange WHERE IndividualDetail_ID=" + detail_id + " AND Approval_Status='pending' AND Period_ID=" + period_id + "";
                string string_select_max_stretch = "SELECT FinancialHeader_IndividualStretchRating FROM ScorecardUser "
                                                  + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                  + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                  + "WHERE ScorecardUser.EmpId='" + user_nik + "' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                SqlCommand sql_select_max_stretch = new SqlCommand(string_select_max_stretch, conn);
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

                max_stretch = float.Parse(sql_select_max_stretch.ExecuteScalar().ToString());

                if (total_rating > max_stretch)//agar tidak melebihi stretch rating yang sudah ditentukan
                {
                    total_rating = max_stretch;
                }

                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                SqlCommand sql_check_request = new SqlCommand(string_check_request, conn);

                user_create = Session["user_name"].ToString();
                user_update = Session["user_name"].ToString();
                date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                using (SqlDataReader SOReader = check_SO.ExecuteReader())
                {
                    if (SOReader.HasRows)//jika nama KPI sudah ada
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SO_exist = true;//jika nama SO yang sama ditemukan
                    }
                    else
                    {
                        SO_exist = false;
                    }
                }

                using (SqlDataReader RequestReader = sql_check_request.ExecuteReader())
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

                if (SO_exist == false)//jika nama SO tidak ditemukan / unique
                {
                    if (req_exist == false)
                    {
                        TextBoxSpecificObjective.Text = TextBoxSpecificObjective.Text.Replace("'", "''");
                        TextareaReason.InnerText = TextareaReason.InnerText.Replace("'", "''");
                        string insert_request_detail = "exec SP_InsertChangeRequestSpecificObjective '" + TextBoxSpecificObjective.Text + "', " + TextBoxTarget.Text + ", "
                                                     + "" + TextBoxResult.Text + ", '" + DropDownListMeasurement.SelectedValue + "', " + total_rating + ", '" + DropDownFormula.SelectedValue + "', "
                                                     + "'pending', " + detail_id + ", '" + user_create + "', '" + date_create + "', '" + user_update + "', '" + date_update + "', " + period_id + ", "
                                                     + "'" + superior_id + "', " + Session["user_id"].ToString() + ", " + header_id + ", '" + TextareaReason.InnerText + "', 1";

                        SqlCommand sql_insert_request_detail = new SqlCommand(insert_request_detail, conn);
                        sql_insert_request_detail.ExecuteNonQuery();
                        sendMail(delete_specific);
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Your Request Has Been Sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Request Has Been Sent. Cannot Request Change for This Item Again.')", true);
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Your Specific Objective Already Exist.')", true);
                }
                conn.Close();
            }//end of SqlConnection
        }

        public void sendMail(bool delete_specific)
        {
            string strApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];

            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            var detail_id = Request.QueryString["detail_id"];

            string superior_email = "";
            string string_get_superior_email = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.EmpId='" + Session["user_nik"].ToString() + "' ) "
                                             + "SELECT empEmail FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";
            string string_get_user_info = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(ScorecardUser.empEmail) as Email, "
                                        + "empGrade, Group_Name, IndividualHeader_KPI, FinancialHeader_IndividualStretchRating, EmpSex, EmpMaritalSt "
                                        + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                        + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                        + "join [Human_Capital_demo].dbo.Employee on ScorecardUser.EmpId=Employee.EmpId "
                                        + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                        + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                        + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                        + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                        + "join IndividualMeasures_Header on ScorecardUser.user_id = IndividualMeasures_Header.user_id "
                                        + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                        + "AND FinancialMeasures_Header.data_status='exist' "
                                        + "WHERE IndividualHeader_ID=" + header_id + " AND FinancialMeasures_Header.Period_ID=" + period_id + "";
            string string_get_current_specific_objective = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualDetail_ID=" + detail_id + "";
            string string_get_new_specific_objective = "SELECT * FROM IndividualDetail_RequestChange WHERE IndividualDetail_ID=" + detail_id + " AND Period_ID=" + period_id + " "
                                                     + "AND Approval_Status='pending'";
            string user_title = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_get_user_info = new SqlCommand(string_get_user_info, conn);
                SqlCommand sql_get_current_specific_objective = new SqlCommand(string_get_current_specific_objective, conn);
                SqlCommand sql_get_new_specific_objective = new SqlCommand(string_get_new_specific_objective, conn);

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

                        if (delete_specific == false)
                        {
                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Change KPI's Specific Objective (" + user_title + " " + UserReader["empName"].ToString() + " - " + UserReader["EmpId"].ToString() + ")");
                            sb_body_introduction.Append("Hello, my name is " + user_title + " " + UserReader["empName"].ToString() + " and this is my information: <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to change my " + UserReader["IndividualHeader_KPI"].ToString() + "'s Specific Objective from:<br/><br/>");
                        }
                        else
                        {
                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Delete KPI's Specific Objective (" + user_title + " " + UserReader["empName"].ToString() + " - " + UserReader["EmpId"].ToString() + ")");
                            sb_body_introduction.Append("Hello, my name is " + user_title + " " + UserReader["empName"].ToString() + " and this is my information: <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to delete my following " + UserReader["IndividualHeader_KPI"].ToString() + "'s Specific Objective:<br/><br/>");
                        }

                        sb_conclusion.Append("Link to Balanced Scorecard Application: " + strApplicationURL + " <br/>Thank you. <br/><br/>Best Regards, <br/>" + user_title + "" + UserReader["empName"].ToString() + ""
                                           + "<br/><br/>This is an automatically generated email – please do not reply to it.");
                    }
                    UserReader.Dispose();
                    UserReader.Close();
                }

                if (delete_specific == false)
                {
                    using (SqlDataReader CurrentReader = sql_get_current_specific_objective.ExecuteReader())
                    {
                        while (CurrentReader.Read())
                        {
                            if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                            {
                                string month_name_target, month_name_result;
                                month_name_target = ShowMonthNameTarget(int.Parse(CurrentReader["IndividualDetail_Target"].ToString()));
                                month_name_result = ShowMonthNameResult(int.Parse(CurrentReader["IndividualDetail_Result"].ToString()));
                                sb_current_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                                + "<tr>"
                                                + "<th style='border:1px solid black; padding:8px'>Spec. Objective</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                                + "</tr>"
                                                + "<tr>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + month_name_target + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + month_name_result + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                + "</tr>"
                                                + "</table><br/> to:");
                            }
                            else if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                            {
                                sb_current_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                                + "<tr>"
                                                + "<th style='border:1px solid black; padding:8px'>Spec. Objective</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                                + "</tr>"
                                                + "<tr>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Target"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Result"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                + "</tr>"
                                                + "</table><br/> to:");
                            }
                            else
                            {
                                sb_current_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                                + "<tr>"
                                                + "<th style='border:1px solid black; padding:8px'>Spec. Objective</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                                + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                                + "</tr>"
                                                + "<tr>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Target"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Result"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                + "<td style='border:1px solid black; padding:8px'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                + "</tr>"
                                                + "</table><br/> to:");
                            }

                        }
                        CurrentReader.Dispose();
                        CurrentReader.Close();
                    }
                }

                using (SqlDataReader NewReader = sql_get_new_specific_objective.ExecuteReader())
                {
                    while (NewReader.Read())
                    {
                        if (NewReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                        {
                            string month_name_target, month_name_result;
                            month_name_target = ShowMonthNameTarget(int.Parse(NewReader["IndividualDetail_Target"].ToString()));
                            month_name_result = ShowMonthNameResult(int.Parse(NewReader["IndividualDetail_Result"].ToString()));
                            sb_new_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                            + "<tr>"
                                            + "<th style='border:1px solid black; padding:8px'>Spec. Objective</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                            + "</tr>"
                                            + "<tr>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Title"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + month_name_target + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + month_name_result + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Formula"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                            + "</tr>"
                                            + "</table>with reason: " + NewReader["IndividualDetailReason"].ToString() + "<br/><br/>");
                        }
                        else if (NewReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                        {
                            sb_new_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                            + "<tr>"
                                            + "<th style='border:1px solid black; padding:8px'>Spec. Objective</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                            + "</tr>"
                                            + "<tr>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Title"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Target"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Result"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Formula"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                            + "</tr>"
                                            + "</table>with reason: " + NewReader["IndividualDetailReason"].ToString() + "<br/><br/>");
                        }
                        else
                        {
                            sb_new_detail.Append("<table style='border:1px solid black; border-collapse:collapse'>"
                                            + "<tr>"
                                            + "<th style='border:1px solid black; padding:8px'>Spec. Objective</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Target</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Result</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Formula</th>"
                                            + "<th style='border:1px solid black; padding:8px'>Rating</th>"
                                            + "</tr>"
                                            + "<tr>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Title"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Target"].ToString() + " " + NewReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Result"].ToString() + " " + NewReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Formula"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                            + "</tr>"
                                            + "</table>with reason: " + NewReader["IndividualDetailReason"].ToString() + "<br/><br/>");
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
                    if (delete_specific == false)
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