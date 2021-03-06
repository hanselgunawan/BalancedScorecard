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
    public partial class edit_individual_specific : System.Web.UI.Page
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

                string user_nik = Session["user_nik"].ToString();

                int current_month, current_year;
                int start_month_bsc = 0, year_from_start_date = 0, minus_one_bsc = 0;

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
                individual_scorecard_breadcrumb.Attributes.Add("a href", "individual_scorecard.aspx?page="+page+"&id=" + period_id + "");

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
                
                //Disable Target, Rating, Measured By, dan Formula
                if (minus_one_bsc == 12)
                {
                    if (current_month == start_month_bsc)
                    {
                        LabelUpdateEdit.Text = "Edit";
                        LabelUpdateEdit2.Text = "Edit";
                        ButtonEditDetail.Text = "Edit";
                        TextBoxSpecificObjective.Attributes.Remove("disabled");
                        DropDownFormula.Enabled = true;
                        DropDownListMeasurement.Enabled = true;
                        TextBoxTarget.Attributes.Remove("disabled");
                        TextBoxResult.Attributes.Add("disabled", "true");
                        TextBoxRating.Attributes.Add("disabled", "true");
                    }
                    else if (current_month == minus_one_bsc && current_year == year_from_start_date - 1)
                    {
                        LabelUpdateEdit.Text = "Edit";
                        LabelUpdateEdit2.Text = "Edit";
                        ButtonEditDetail.Text = "Edit";
                        TextBoxSpecificObjective.Attributes.Remove("disabled");
                        DropDownFormula.Enabled = true;
                        DropDownListMeasurement.Enabled = true;
                        TextBoxTarget.Attributes.Remove("disabled");
                        TextBoxResult.Attributes.Add("disabled", "true");
                        TextBoxRating.Attributes.Add("disabled", "true");
                    }
                    else
                    {
                        LabelUpdateEdit.Text = "Update";
                        LabelUpdateEdit2.Text = "Update";
                        ButtonEditDetail.Text = "Update";
                        TextBoxSpecificObjective.Attributes.Add("disabled", "true");
                        DropDownFormula.Enabled = false;
                        DropDownListMeasurement.Enabled = false;
                        TextBoxTarget.Attributes.Add("disabled", "true");
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxResult.Attributes.Remove("disabled");
                    }
                }
                else
                {
                    if (current_month == start_month_bsc)
                    {
                        LabelUpdateEdit.Text = "Edit";
                        LabelUpdateEdit2.Text = "Edit";
                        ButtonEditDetail.Text = "Edit";
                        TextBoxSpecificObjective.Attributes.Remove("disabled");
                        DropDownFormula.Enabled = true;
                        DropDownListMeasurement.Enabled = true;
                        TextBoxTarget.Attributes.Remove("disabled");
                        TextBoxResult.Attributes.Add("disabled", "true");
                        TextBoxRating.Attributes.Add("disabled", "true");
                    }
                    else if (current_month == minus_one_bsc && current_year == year_from_start_date)
                    {
                        LabelUpdateEdit.Text = "Edit";
                        LabelUpdateEdit2.Text = "Edit";
                        ButtonEditDetail.Text = "Edit";
                        TextBoxSpecificObjective.Attributes.Remove("disabled");
                        DropDownFormula.Enabled = true;
                        DropDownListMeasurement.Enabled = true;
                        TextBoxTarget.Attributes.Remove("disabled");
                        TextBoxResult.Attributes.Add("disabled", "true");
                        TextBoxRating.Attributes.Add("disabled", "true");
                    }
                    else
                    {
                        LabelUpdateEdit.Text = "Update";
                        LabelUpdateEdit2.Text = "Update";
                        ButtonEditDetail.Text = "Update";
                        TextBoxSpecificObjective.Attributes.Add("disabled", "true");
                        DropDownFormula.Enabled = false;
                        DropDownListMeasurement.Enabled = false;
                        TextBoxTarget.Attributes.Add("disabled", "true");
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxResult.Attributes.Remove("disabled");
                    }
                }

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    string string_select_individual_stretch_review = "SELECT FinancialHeader_IndividualStretchRating, FinancialHeader_Review FROM ScorecardUser "
                                                                    + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                                    + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                                    + "WHERE ScorecardUser.EmpId='" + user_nik + "' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID="+period_id+"";
                    string select_individual_header = "SELECT * FROM IndividualMeasures_Header WHERE IndividualHeader_ID="+header_id+"";
                    string select_individual_detail = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualDetail_ID="+detail_id+" AND IndividualHeader_ID="+header_id+"";
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                                    + "WHERE Access_Rights_Code NOT IN "                 //UserGroup
                                                    + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                    + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
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
                        AccessReader.Close();
                        AccessReader.Dispose();
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
                                cancel_edit_specific.Attributes.Add("href","individual_scorecard.aspx?page="+page+"&id="+PeriodReader["Period_ID"]+"");
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
                                        if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                        {
                                            DropDownFormula.Enabled = false;
                                        }
                                        else
                                        {
                                            DropDownFormula.Enabled = true;
                                        }
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
                                        if (SpecificReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                        {
                                            DropDownFormula.Enabled = false;
                                        }
                                        else
                                        {
                                            DropDownFormula.Enabled = true;
                                        }
                                    }
                                    else
                                    {
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
                                        DropDownFormula.Enabled = false;
                                    }
                                }
                                else
                                {
                                    if (current_month == start_month_bsc)
                                    {
                                        month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                        month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                        DropDownFormula.Enabled = true;
                                    }
                                    else if (current_month == minus_one_bsc && current_year == year_from_start_date - 1)
                                    {
                                        month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                        month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                        DropDownFormula.Enabled = true;
                                    }
                                    else//untuk bagian Update
                                    {
                                        month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                        month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                        DropDownFormula.Enabled = false;
                                    }
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
                string month_name;
                month_name_target.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                TextBoxTarget.Attributes.Add("max", "12");
                TextBoxTarget.Attributes.Add("min", "1");
                TextBoxTarget.Attributes.Add("step", "1");
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
                DropDownFormula.SelectedValue = "100% - ((Result - Target)/Target)";
                DropDownFormula.Enabled = false;
                TextBoxResult.Attributes.Add("max", "12");
                TextBoxResult.Attributes.Add("step", "1");
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Maximum Target value for Measured By month is 12 and minimum value is 1')", true);
            }
            else
            {
                month_name_target.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                TextBoxTarget.Attributes.Remove("max");
                TextBoxTarget.Attributes.Add("min", "0");
                TextBoxTarget.Attributes.Add("step", "0.01");
                DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                DropDownFormula.Enabled = true;
                TextBoxResult.Attributes.Remove("max");
                if (DropDownListMeasurement.SelectedValue == "Numbers")
                {
                    TextBoxResult.Attributes.Add("step", "1");
                    TextBoxTarget.Attributes.Add("step", "1");
                }
                else
                {
                    TextBoxResult.Attributes.Add("step", "0.01");
                    TextBoxTarget.Attributes.Add("step", "0.01");
                }
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
                                            + "AND data_status='exist' AND IndividualDetail_ID<>" + detail_id + " AND IndividualHeader_ID=" + header_id + "";
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                using (SqlDataReader SOReader = check_SO.ExecuteReader())
                {
                    if (SOReader.HasRows)//jika nama KPI sudah ada
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SpanEditSpecific.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
                    }
                    else
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SpanEditSpecific.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button");
                    }
                }
                conn.Close();
            }
        }

        protected void OnClickEditSpecific(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            var detail_id = Request.QueryString["detail_id"];

            float total_header_score, sum_individual_rating, count_individual_rating_data_by_header;
            float total_rating = 0, max_stretch;
            bool SO_exist;

            string user_nik = Session["user_nik"].ToString();
            string user_update, date_update;
            string check_specific_objective = "SELECT IndividualDetail_Title FROM IndividualMeasures_Detail "
                                            + "WHERE IndividualDetail_Title='" + TextBoxSpecificObjective.Text + "' "
                                            + "AND data_status='exist' AND IndividualDetail_ID<>" + detail_id + " AND IndividualHeader_ID=" + header_id + "";
            string update_specific_objective = "UPDATE IndividualMeasures_Detail SET IndividualDetail_Title=@title, IndividualDetail_Target=@target, "
                                             + "IndividualDetail_Result=@result, IndividualDetail_MeasureBy=@measure, IndividualDetail_Rating=ROUND(@rating,2), "
                                             + "user_update=@user_update, date_update=@date_update, IndividualDetail_Formula=@formula "
                                             + "WHERE IndividualDetail_ID="+detail_id+"";
            string sum_detail_rating = "SELECT SUM(IndividualDetail_Rating) FROM IndividualMeasures_Detail "
                                     + "WHERE IndividualHeader_ID="+header_id+" AND data_status='exist'";
            string count_individual_rating = "SELECT COUNT(*) FROM IndividualMeasures_Detail WHERE IndividualHeader_ID="+header_id+" AND data_status='exist'";
            string header_total_score = "UPDATE IndividualMeasures_Header SET IndividualHeader_Rating=ROUND(@total_header_score,2), "
                                      + "IndividualHeader_Score=ROUND((@total_header_score*(IndividualHeader_Weight/100)),2) "
                                      + "WHERE IndividualHeader_ID=" + header_id + "";

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
                    total_rating = 0;
                }
                else
                {
                    total_rating = 100 - (((float.Parse(TextBoxResult.Text) - float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text))*100);
                }
            }

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
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

                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                SqlCommand sql_update_specific_objective = new SqlCommand(update_specific_objective, conn);
                SqlCommand sql_sum_individual_rating = new SqlCommand(sum_detail_rating, conn);
                SqlCommand sql_count_individual_rating = new SqlCommand(count_individual_rating, conn);
                SqlCommand sql_update_header = new SqlCommand(header_total_score, conn);

                user_update = Session["user_name"].ToString();
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

                if (SO_exist == false)//jika nama SO tidak ditemukan / unique
                {
                    sql_update_specific_objective.Parameters.AddWithValue("@title", TextBoxSpecificObjective.Text);
                    sql_update_specific_objective.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text),2));
                    sql_update_specific_objective.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Text), 2));
                    sql_update_specific_objective.Parameters.AddWithValue("@measure", DropDownListMeasurement.SelectedValue);
                    sql_update_specific_objective.Parameters.AddWithValue("@rating", Math.Round(total_rating,2));
                    sql_update_specific_objective.Parameters.AddWithValue("@user_update", user_update);
                    sql_update_specific_objective.Parameters.AddWithValue("@date_update", date_update);
                    sql_update_specific_objective.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                    
                    sql_update_specific_objective.ExecuteNonQuery();//update specific objective

                    //untuk meng-update Individual Header
                    sum_individual_rating = float.Parse(sql_sum_individual_rating.ExecuteScalar().ToString());
                    count_individual_rating_data_by_header = float.Parse(sql_count_individual_rating.ExecuteScalar().ToString());
                    total_header_score = sum_individual_rating / count_individual_rating_data_by_header;

                    sql_update_header.Parameters.AddWithValue("@total_header_score", total_header_score);

                    sql_update_header.ExecuteNonQuery();//update header

                    
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Specific Objective Updated!'); window.location='" + baseUrl + "individual_scorecard.aspx?page="+page+"&id=" + period_id + "';", true);
                }
                conn.Close();
            }//end of SqlConnection
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