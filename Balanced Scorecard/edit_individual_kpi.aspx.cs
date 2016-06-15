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
    public partial class edit_individual_kpi : System.Web.UI.Page
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

                string user_nik = Session["user_nik"].ToString();

                int current_month, current_year;
                current_month = DateTime.Now.Month;
                current_year = DateTime.Now.Year;

                int start_date_bsc = 0, minus_one_bsc = 0, year_from_startdate = 0;

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
                        year_from_startdate = int.Parse(start_date.ToString("yyyy"));
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
                individual_scorecard_breadcrumb.Attributes.Add("a href", "individual_scorecard.aspx?page="+page+"&id="+period_id+"");

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

                //Disable Target, Measured By, Formula, Rating, dan Score
                TextBoxTarget.Attributes.Add("disabled", "true");
                DropDownListMeasurement.Enabled = false;
                DropDownFormula.Enabled = false;
                TextBoxScore.Attributes.Add("disabled", "true");
                TextBoxRating.Attributes.Add("disabled", "true");
                TextBoxKPI.Attributes.Add("disabled", "true");
                TextareaObjective.Attributes.Add("disabled", "true");
                TextBoxWeight.Attributes.Add("disabled", "true");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    //ASUMSI NIK ==> 100
                    string string_select_individual_stretch_review = "SELECT FinancialHeader_IndividualStretchRating, FinancialHeader_Review "
                                                                    + "FROM ScorecardUser "
                                                                    + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode "
                                                                    + "AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                                    + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                                    + "WHERE ScorecardUser.EmpId='"+user_nik+"' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID="+period_id+"";
                    string select_individual_header = "SELECT * FROM IndividualMeasures_Header WHERE IndividualHeader_ID="+header_id+" AND data_status='exist'";
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                                    + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                                    + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                    + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
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
                            cancel_add_new_KPI.Attributes.Add("a href", "individual_scorecard.aspx?page="+page+"&id="+period_id+"");
                        }
                        else//jika periode tidak ditemukkan / disuntik langsung ke Database
                        {
                            LabelStartPeriod.InnerText = "No Period";
                            LabelEndPeriod.InnerText = "No Period";
                            SpanEditKPI.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                            cancel_add_new_KPI.Attributes.Add("a href", "individual_scorecard.aspx?page=1&id=1");
                        }
                    }

                    using (SqlDataReader IndividualHeaderReader = sql_select_individual_header.ExecuteReader())
                    {
                        if (IndividualHeaderReader.HasRows)
                        {
                            while (IndividualHeaderReader.Read())
                            {
                                if (float.Parse(IndividualHeaderReader["IndividualHeader_Target"].ToString()) == -1)
                                {
                                    if (minus_one_bsc == 12)
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            DropDownListSpecific.Enabled = true;
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Items.Add("-");
                                            DropDownFormula.Items.Add("-");
                                            DropDownListSpecific.SelectedValue = "Yes";//default button "Use" jika targetnya = -1
                                            TextBoxTarget.Text = "0";
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownFormula.SelectedValue = "-";
                                            DropDownListMeasurement.SelectedValue = "-";
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                        }
                                        else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                                        {
                                            DropDownListSpecific.Enabled = true;
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Items.Add("-");
                                            DropDownFormula.Items.Add("-");
                                            DropDownListSpecific.SelectedValue = "Yes";//default button "Use" jika targetnya = -1
                                            TextBoxTarget.Text = "0";
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownFormula.SelectedValue = "-";
                                            DropDownListMeasurement.SelectedValue = "-";
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                        }
                                        else if (current_month == minus_one_bsc && minus_one_bsc != 12)//maret, februari sudah bisa edit
                                        {
                                            DropDownListSpecific.Enabled = true;
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Items.Add("-");
                                            DropDownFormula.Items.Add("-");
                                            DropDownListSpecific.SelectedValue = "Yes";//default button "Use" jika targetnya = -1
                                            TextBoxTarget.Text = "0";
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownFormula.SelectedValue = "-";
                                            DropDownListMeasurement.SelectedValue = "-";
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                        }
                                        else
                                        {
                                            DropDownListSpecific.Enabled = false;
                                            TextBoxKPI.Attributes.Add("disabled", "true");
                                            TextareaObjective.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Attributes.Add("disabled", "true");
                                            DropDownListMeasurement.Items.Add("-");
                                            DropDownFormula.Items.Add("-");
                                            DropDownListSpecific.SelectedValue = "Yes";//default button "Use" jika targetnya = -1
                                            TextBoxTarget.Text = "0";
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownFormula.SelectedValue = "-";
                                            DropDownListMeasurement.SelectedValue = "-";
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            LabelUpdateEdit.Text = "Update";
                                            LabelUpdateEdit2.Text = "Update";
                                            ButtonAddKPI.Text = "Update";
                                        }
                                    }
                                    else
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            DropDownListSpecific.Enabled = true;
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Items.Add("-");
                                            DropDownFormula.Items.Add("-");
                                            DropDownListSpecific.SelectedValue = "Yes";//default button "Use" jika targetnya = -1
                                            TextBoxTarget.Text = "0";
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownFormula.SelectedValue = "-";
                                            DropDownListMeasurement.SelectedValue = "-";
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                        }
                                        else if (current_month == minus_one_bsc)
                                        {
                                            DropDownListSpecific.Enabled = true;
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Items.Add("-");
                                            DropDownFormula.Items.Add("-");
                                            DropDownListSpecific.SelectedValue = "Yes";//default button "Use" jika targetnya = -1
                                            TextBoxTarget.Text = "0";
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownFormula.SelectedValue = "-";
                                            DropDownListMeasurement.SelectedValue = "-";
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                        }
                                        else
                                        {
                                            DropDownListSpecific.Enabled = false;
                                            TextBoxKPI.Attributes.Add("disabled", "true");
                                            TextareaObjective.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Attributes.Add("disabled", "true");
                                            DropDownListMeasurement.Items.Add("-");
                                            DropDownFormula.Items.Add("-");
                                            DropDownListSpecific.SelectedValue = "Yes";//default button "Use" jika targetnya = -1
                                            TextBoxTarget.Text = "0";
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownFormula.SelectedValue = "-";
                                            DropDownListMeasurement.SelectedValue = "-";
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            LabelUpdateEdit.Text = "Update";
                                            LabelUpdateEdit2.Text = "Update";
                                            ButtonAddKPI.Text = "Update";
                                        }
                                    }
                                }
                                else
                                {
                                    if (minus_one_bsc == 12)
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                            DropDownListSpecific.Enabled = true;
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            TextBoxWeight.Attributes.Remove("disabled");

                                            DropDownListSpecific.SelectedValue = "No";
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
                                                DropDownFormula.Enabled = false;
                                            }
                                            else
                                            {
                                                DropDownFormula.Enabled = true;
                                            }
                                            DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
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
                                                //tampilakan Month Name
                                                month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                                month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                            }
                                        }
                                        else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                                        {
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                            DropDownListSpecific.Enabled = true;
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            TextBoxWeight.Attributes.Remove("disabled");

                                            DropDownListSpecific.SelectedValue = "No";
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
                                                DropDownFormula.Enabled = false;
                                            }
                                            else
                                            {
                                                DropDownFormula.Enabled = true;
                                            }
                                            DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
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
                                                //tampilakan Month Name
                                                month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                                month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                            }
                                        }
                                        else if (current_month == minus_one_bsc && minus_one_bsc != 12)//jika mulai maret, februari bisa edit
                                        {
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                            DropDownListSpecific.Enabled = true;
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            TextBoxWeight.Attributes.Remove("disabled");

                                            DropDownListSpecific.SelectedValue = "No";
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
                                                DropDownFormula.Enabled = false;
                                            }
                                            else
                                            {
                                                DropDownFormula.Enabled = true;
                                            }
                                            DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
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
                                                //tampilakan Month Name
                                                month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                                month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                            }
                                        }
                                        else
                                        {
                                            LabelUpdateEdit.Text = "Update";
                                            LabelUpdateEdit2.Text = "Update";
                                            ButtonAddKPI.Text = "Update";
                                            DropDownListSpecific.Enabled = false;
                                            TextBoxKPI.Attributes.Add("disabled", "true");
                                            TextareaObjective.Attributes.Add("disabled", "true");
                                            TextBoxTarget.Attributes.Add("disabled", "true");
                                            TextBoxResult.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Enabled = false;
                                            DropDownFormula.Enabled = false;
                                            TextBoxWeight.Attributes.Add("disabled", "true");

                                            DropDownListSpecific.SelectedValue = "No";
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                            DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
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
                                                //tampilakan Month Name
                                                month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                                month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                            DropDownListSpecific.Enabled = true;
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            TextBoxWeight.Attributes.Remove("disabled");

                                            DropDownListSpecific.SelectedValue = "No";
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
                                                DropDownFormula.Enabled = false;
                                            }
                                            else
                                            {
                                                DropDownFormula.Enabled = true;
                                            }
                                            DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
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
                                                //tampilakan Month Name
                                                month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                                month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                            }
                                        }
                                        else if (current_month == minus_one_bsc)
                                        {
                                            LabelUpdateEdit.Text = "Edit";
                                            LabelUpdateEdit2.Text = "Edit";
                                            ButtonAddKPI.Text = "Edit";
                                            DropDownListSpecific.Enabled = true;
                                            TextBoxKPI.Attributes.Remove("disabled");
                                            TextareaObjective.Attributes.Remove("disabled");
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            TextBoxWeight.Attributes.Remove("disabled");

                                            DropDownListSpecific.SelectedValue = "No";
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
                                                DropDownFormula.Enabled = false;
                                            }
                                            else
                                            {
                                                DropDownFormula.Enabled = true;
                                            }
                                            DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
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
                                                //tampilakan Month Name
                                                month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                                month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                            }
                                        }
                                        else
                                        {
                                            LabelUpdateEdit.Text = "Update";
                                            LabelUpdateEdit2.Text = "Update";
                                            ButtonAddKPI.Text = "Update";
                                            DropDownListSpecific.Enabled = false;
                                            TextBoxKPI.Attributes.Add("disabled", "true");
                                            TextareaObjective.Attributes.Add("disabled", "true");
                                            TextBoxTarget.Attributes.Add("disabled", "true");
                                            TextBoxResult.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Enabled = false;
                                            DropDownFormula.Enabled = false;
                                            TextBoxWeight.Attributes.Add("disabled", "true");

                                            DropDownListSpecific.SelectedValue = "No";
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                            DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();

                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                            {
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
                                                //tampilakan Month Name
                                                month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                                month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                                            }
                                        }
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

            string select_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' "
                                   + "AND IndividualHeader_ID<>" + header_id + " AND data_status='exist' AND Period_ID=" + period_id + " "
                                   + "AND user_id=" + Session["user_id"] + "";
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_select_KPI_name = new SqlCommand(select_KPI_name, conn);
                using (SqlDataReader KPIReader = sql_select_KPI_name.ExecuteReader())//mengecek apakah KPI sudah ada atau belum
                {
                    if (KPIReader.HasRows)
                    {
                        check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SpanEditKPI.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                    }
                    else
                    {
                        check_KPI_name_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SpanEditKPI.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                    }
                } //end of KPIReader
                conn.Close();
            }
        }

        protected void OnClickEditKPI(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];

            string user_nik = Session["user_nik"].ToString();
            string select_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' "
                                   + "AND IndividualHeader_ID<>" + header_id + " AND data_status='exist' AND Period_ID=" + period_id + " "
                                   + "AND user_id=" + Session["user_id"] + "";
            bool KPI_name_exist;
            float total_rating = 0, total_score, max_stretch;

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
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
                    if (TextBoxResult.Text == "0")
                    {
                        total_rating = 0;
                    }
                    else
                    {
                        total_rating = 100 - (((float.Parse(TextBoxResult.Text) - float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text)) * 100);
                    }
                }

                //mengambil nilai max stretch rating dari tabel Financial Header. ASUMSI NIK ==> 100
                string string_select_max_stretch = "SELECT FinancialHeader_IndividualStretchRating FROM ScorecardUser "
                                                  + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                  + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                  + "WHERE ScorecardUser.EmpId='"+user_nik+"' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                SqlCommand sql_select_max_stretch = new SqlCommand(string_select_max_stretch, conn);
                max_stretch = float.Parse(sql_select_max_stretch.ExecuteScalar().ToString());

                if (total_rating > max_stretch)//agar tidak melebihi stretch rating yang sudah ditentukan
                {
                    total_rating = max_stretch;
                }

                total_score = (total_rating * (float.Parse(TextBoxWeight.Value))) / 100;

                SqlCommand sql_select_KPI_name = new SqlCommand(select_KPI_name, conn);
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

                if (KPI_name_exist == false)
                {
                    //KALAU SPEC OBJ = NO, UPDATE Header dan Hapus Anaknya!
                    if (DropDownListSpecific.SelectedValue == "No")
                    {
                        //ASUMSI --> NIK=100
                        string update_individual_header = "UPDATE IndividualMeasures_Header SET IndividualHeader_KPI=@header_KPI, "
                                                        + "IndividualHeader_Target=@target, IndividualHeader_Result=@result, "
                                                        + "IndividualHeader_Rating=ROUND(@rating,2), IndividualHeader_Weight=@weight, "
                                                        + "IndividualHeader_Score=ROUND(@score,2), user_update=@user_update, date_update=@date_update, "
                                                        + "IndividualHeader_Objective=@objective, Period_ID=@period_id, IndividualHeader_Formula=@formula, "
                                                        + "IndividualHeader_MeasureBy=@measure, data_status=@data_status, user_id=@user_id "
                                                        + "WHERE IndividualHeader_ID=" + header_id + "";
                        string user_update, date_update;
                        SqlCommand sql_update_individual_header = new SqlCommand(update_individual_header, conn);

                        user_update = Session["user_name"].ToString();
                        date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                        sql_update_individual_header.Parameters.AddWithValue("@header_KPI", TextBoxKPI.Text);
                        sql_update_individual_header.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text),2));
                        sql_update_individual_header.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Text), 2));
                        sql_update_individual_header.Parameters.AddWithValue("@rating", Math.Round(total_rating,2));
                        sql_update_individual_header.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                        sql_update_individual_header.Parameters.AddWithValue("@score", Math.Round(total_score,2));
                        sql_update_individual_header.Parameters.AddWithValue("@user_update", user_update);
                        sql_update_individual_header.Parameters.AddWithValue("@date_update", date_update);
                        sql_update_individual_header.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                        sql_update_individual_header.Parameters.AddWithValue("@period_id", period_id);
                        sql_update_individual_header.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                        sql_update_individual_header.Parameters.AddWithValue("@measure", DropDownListMeasurement.SelectedValue);
                        sql_update_individual_header.Parameters.AddWithValue("@data_status", "exist");
                        sql_update_individual_header.Parameters.AddWithValue("@user_id", Session["user_id"]);
                        sql_update_individual_header.ExecuteNonQuery();

                        string check_individual_detail = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualHeader_ID=" + header_id + " AND data_status='exist'";
                        SqlCommand sql_individual_detail = new SqlCommand(check_individual_detail, conn);
                        using (SqlDataReader DetailReader = sql_individual_detail.ExecuteReader())
                        {
                            if (DetailReader.HasRows)//cek apakah sudah ada Detail atau Belum. Jika sudah ada, DELETE Detail-nya
                            {
                                string delete_individual_detail = "UPDATE IndividualMeasures_Detail SET data_status='deleted' WHERE IndividualHeader_ID=" + header_id + "";
                                SqlCommand sql_delete_individual_detail = new SqlCommand(delete_individual_detail, conn);
                                sql_delete_individual_detail.ExecuteNonQuery();
                            }
                        }

                        
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('KPI Updated!'); window.location='" + baseUrl + "individual_scorecard.aspx?page="+page+"&id=" + period_id + "';", true);
                    }
                    else if (DropDownListSpecific.SelectedValue == "Yes")//Jika ada Specific Objectives
                    {
                        float score;
                        string update_individual_header = "UPDATE IndividualMeasures_Header SET IndividualHeader_KPI=@header_KPI, "
                                                        + "IndividualHeader_Target=@target, IndividualHeader_Result=@result, IndividualHeader_Rating=@rating, "
                                                        + "IndividualHeader_Weight=@weight, IndividualHeader_Score=ROUND(@score,2), user_update=@user_update, "
                                                        + "date_update=@date_update, IndividualHeader_Objective=@objective, Period_ID=@period_id, "
                                                        + "IndividualHeader_Formula=@formula, IndividualHeader_MeasureBy=@measure, data_status=@data_status, "
                                                        + "user_id=@user_id WHERE IndividualHeader_ID=" + header_id + "";
                        string user_update, date_update;
                        SqlCommand sql_update_individual_header = new SqlCommand(update_individual_header, conn);

                        user_update = Session["user_name"].ToString();
                        date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        score = float.Parse(TextBoxRating.Value) * (float.Parse(TextBoxWeight.Value) / 100);

                        sql_update_individual_header.Parameters.AddWithValue("@header_KPI", TextBoxKPI.Text);
                        sql_update_individual_header.Parameters.AddWithValue("@target", -1);
                        sql_update_individual_header.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Text), 2));
                        sql_update_individual_header.Parameters.AddWithValue("@rating", TextBoxRating.Value);
                        sql_update_individual_header.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                        sql_update_individual_header.Parameters.AddWithValue("@score", Math.Round(score,2));
                        sql_update_individual_header.Parameters.AddWithValue("@user_update", user_update);
                        sql_update_individual_header.Parameters.AddWithValue("@date_update", date_update);
                        sql_update_individual_header.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                        sql_update_individual_header.Parameters.AddWithValue("@period_id", period_id);
                        sql_update_individual_header.Parameters.AddWithValue("@formula", "-");
                        sql_update_individual_header.Parameters.AddWithValue("@measure", "-");
                        sql_update_individual_header.Parameters.AddWithValue("@data_status", "exist");
                        sql_update_individual_header.Parameters.AddWithValue("@user_id", Session["user_id"]);
                        sql_update_individual_header.ExecuteNonQuery();

                        
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('KPI Updated!'); window.location='" + baseUrl + "individual_scorecard.aspx?page="+page+"&id=" + period_id + "';", true);
                    }
                }
                conn.Close();
            }//end of SqlConnection
        }

        protected void OnSelectSpecific(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];

            int current_month, current_year;
            current_month = DateTime.Now.Month;
            current_year = DateTime.Now.Year;

            int start_date_bsc = 0, minus_one_bsc = 0, year_from_startdate = 0;

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
                    year_from_startdate = int.Parse(start_date.ToString("yyyy"));
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

            string select_individual_header = "SELECT * FROM IndividualMeasures_Header WHERE IndividualHeader_ID="+header_id+"";
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
                            TextBoxRating.Attributes.Add("disabled", "true");
                            TextBoxRating.Value = HeaderReader["IndividualHeader_Rating"].ToString();
                            TextBoxScore.Attributes.Add("disabled", "true");
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
                    TextBoxRating.Attributes.Add("disabled", "true");
                    TextBoxRating.Value = "0";
                    TextBoxScore.Attributes.Add("disabled", "true");
                    TextBoxScore.Value = "0";
                    TextBoxRating.Value = "0";
                    TextBoxWeight.Value = "0";
                    DropDownListMeasurement.Items.Add("-");
                    DropDownFormula.Items.Add("-");
                    DropDownListMeasurement.SelectedValue = "-";
                    DropDownFormula.SelectedValue = "-";
                    month_name_target.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
                    month_name_result.Attributes.Add("style", "visibility:hidden; padding-bottom:0px; margin-top:0px");
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
                                    if (minus_one_bsc == 12)
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            TextBoxTarget.Text = "0";
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;

                                            DropDownListMeasurement.Items.Remove("-");
                                            DropDownListMeasurement.SelectedValue = "%";

                                            DropDownFormula.Items.Remove("-");
                                            DropDownFormula.SelectedValue = "(Result/Target) x 100%";

                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = "0";
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Value = "0";
                                            TextBoxScore.Value = "0";
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                        }
                                        else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                                        {
                                            TextBoxTarget.Text = "0";
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;

                                            DropDownListMeasurement.Items.Remove("-");
                                            DropDownListMeasurement.SelectedValue = "%";

                                            DropDownFormula.Items.Remove("-");
                                            DropDownFormula.SelectedValue = "(Result/Target) x 100%";

                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = "0";
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Value = "0";
                                            TextBoxScore.Value = "0";
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                        }
                                        else if (current_month == minus_one_bsc && current_year == year_from_startdate)//maret, februari bisa edit
                                        {
                                            TextBoxTarget.Text = "0";
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;

                                            DropDownListMeasurement.Items.Remove("-");
                                            DropDownListMeasurement.SelectedValue = "%";

                                            DropDownFormula.Items.Remove("-");
                                            DropDownFormula.SelectedValue = "(Result/Target) x 100%";

                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = "0";
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Value = "0";
                                            TextBoxScore.Value = "0";
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                        }
                                        else
                                        {
                                            TextBoxTarget.Text = "0";
                                            DropDownListMeasurement.Enabled = false;
                                            DropDownFormula.Enabled = false;

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
                                    }
                                    else
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            TextBoxTarget.Text = "0";
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;

                                            DropDownListMeasurement.Items.Remove("-");
                                            DropDownListMeasurement.SelectedValue = "%";

                                            DropDownFormula.Items.Remove("-");
                                            DropDownFormula.SelectedValue = "(Result/Target) x 100%";

                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = "0";
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Value = "0";
                                            TextBoxScore.Value = "0";
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                        }
                                        else if (current_month == minus_one_bsc)
                                        {
                                            TextBoxTarget.Text = "0";
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;

                                            DropDownListMeasurement.Items.Remove("-");
                                            DropDownListMeasurement.SelectedValue = "%";

                                            DropDownFormula.Items.Remove("-");
                                            DropDownFormula.SelectedValue = "(Result/Target) x 100%";

                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = "0";
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Value = "0";
                                            TextBoxScore.Value = "0";
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                        }
                                        else
                                        {
                                            TextBoxTarget.Text = "0";
                                            DropDownListMeasurement.Enabled = false;
                                            DropDownFormula.Enabled = false;

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
                                    }
                                }
                                else//dari "NO" ke "NO"
                                {
                                    if (minus_one_bsc == 12)
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                            {
                                                DropDownListMeasurement.SelectedValue = "%";
                                            }
                                            else
                                            {
                                                DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                    month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                }
                                            }

                                            if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                            {
                                                DropDownFormula.SelectedValue = "(Result/Target) x 100%";//untuk yang dari "Use" menjadi "Don't Use"
                                            }
                                            else
                                            {
                                                DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();//untuk yang dari "Don'Use" jadi "Use"
                                            }
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();
                                        }
                                        else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                                        {
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                            {
                                                DropDownListMeasurement.SelectedValue = "%";
                                            }
                                            else
                                            {
                                                DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                    month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                }
                                            }

                                            if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                            {
                                                DropDownFormula.SelectedValue = "(Result/Target) x 100%";//untuk yang dari "Use" menjadi "Don't Use"
                                            }
                                            else
                                            {
                                                DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();//untuk yang dari "Don'Use" jadi "Use"
                                            }
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();
                                        }
                                        else if (current_month == minus_one_bsc && current_year == year_from_startdate)//maret, februari bisa edit
                                        {
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                            {
                                                DropDownListMeasurement.SelectedValue = "%";
                                            }
                                            else
                                            {
                                                DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                    month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                }
                                            }

                                            if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                            {
                                                DropDownFormula.SelectedValue = "(Result/Target) x 100%";//untuk yang dari "Use" menjadi "Don't Use"
                                            }
                                            else
                                            {
                                                DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();//untuk yang dari "Don'Use" jadi "Use"
                                            }
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();
                                        }
                                        else
                                        {
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxTarget.Attributes.Add("disabled", "true");
                                            TextBoxResult.Attributes.Remove("disabled");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.Enabled = false;
                                            DropDownFormula.Enabled = false;
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                            {
                                                DropDownListMeasurement.SelectedValue = "%";
                                            }
                                            else
                                            {
                                                DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                    month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                }
                                            }

                                            if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                            {
                                                DropDownFormula.SelectedValue = "(Result/Target) x 100%";//untuk yang dari "Use" menjadi "Don't Use"
                                            }
                                            else
                                            {
                                                DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();//untuk yang dari "Don'Use" jadi "Use"
                                            }
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            TextBoxWeight.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (current_month == start_date_bsc)
                                        {
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                            {
                                                DropDownListMeasurement.SelectedValue = "%";
                                            }
                                            else
                                            {
                                                DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                    month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                }
                                            }

                                            if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                            {
                                                DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                                            }
                                            else
                                            {
                                                DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            }
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();
                                        }
                                        else if (current_month == minus_one_bsc)
                                        {
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxTarget.Attributes.Remove("disabled");
                                            TextBoxResult.Attributes.Add("disabled", "true");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.Enabled = true;
                                            DropDownFormula.Enabled = true;
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                            {
                                                DropDownListMeasurement.SelectedValue = "%";
                                            }
                                            else
                                            {
                                                DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                    month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                }
                                            }

                                            if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                            {
                                                DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                                            }
                                            else
                                            {
                                                DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            }
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            TextBoxWeight.Attributes.Remove("disabled");
                                            TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();
                                        }
                                        else
                                        {
                                            TextBoxTarget.Text = IndividualHeaderReader["IndividualHeader_Target"].ToString();
                                            TextBoxTarget.Attributes.Add("disabled", "true");
                                            TextBoxResult.Attributes.Remove("disabled");
                                            TextBoxResult.Text = IndividualHeaderReader["IndividualHeader_Result"].ToString();
                                            DropDownListMeasurement.Enabled = false;
                                            DropDownFormula.Enabled = false;
                                            if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "-")
                                            {
                                                DropDownListMeasurement.SelectedValue = "%";
                                            }
                                            else
                                            {
                                                DropDownListMeasurement.SelectedValue = IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString();
                                                if (IndividualHeaderReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                                {
                                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                    month_name_result.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                                }
                                            }

                                            if (IndividualHeaderReader["IndividualHeader_Formula"].ToString() == "-")
                                            {
                                                DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                                            }
                                            else
                                            {
                                                DropDownFormula.SelectedValue = IndividualHeaderReader["IndividualHeader_Formula"].ToString();
                                            }
                                            TextBoxRating.Attributes.Add("disabled", "true");
                                            TextBoxRating.Value = IndividualHeaderReader["IndividualHeader_Rating"].ToString();
                                            TextBoxScore.Attributes.Add("disabled", "true");
                                            TextBoxScore.Value = IndividualHeaderReader["IndividualHeader_Score"].ToString();
                                            TextBoxWeight.Attributes.Add("disabled", "true");
                                            TextBoxWeight.Value = IndividualHeaderReader["IndividualHeader_Weight"].ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }//end of SqlConnection
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