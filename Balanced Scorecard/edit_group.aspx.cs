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
    public partial class edit_group : System.Web.UI.Page
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
                var header_id = Request.QueryString["header"];//get header id
                var period_id = Request.QueryString["period_id"];//untuk Cancel Button saja

                financial_measures_breadcrumb.Attributes.Add("a href", "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                if (!IsPostBack)//di Page_Load HARUS ADA IsPostBack biar bisa UPDATE!
                {
                    using (SqlConnection conn = new SqlConnection(str_connect))
                    {
                        string string_select_header = "SELECT * FROM FinancialMeasures_Header WHERE "
                                                    + "FinancialHeader_ID=" + header_id + "";
                        string string_select_group_name = "SELECT Group_Name FROM ScorecardGroup";//pilih nama group
                        string string_select_review = "SELECT DISTINCT Review_Name FROM ScorecardReview";
                        string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                                          + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                                          + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                          + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                        SqlCommand sql_select_header = new SqlCommand(string_select_header, conn);
                        SqlCommand sql_select_group_name = new SqlCommand(string_select_group_name, conn);
                        SqlCommand sql_select_review = new SqlCommand(string_select_review, conn);
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

                        using (SqlDataReader GroupReader = sql_select_group_name.ExecuteReader())
                        {
                            while (GroupReader.Read())
                            {
                                DropDownListGroup.Items.Add(GroupReader["Group_Name"].ToString());
                            }
                            GroupReader.Dispose();
                            GroupReader.Close();
                        }

                        using (SqlDataReader ReviewReader = sql_select_review.ExecuteReader())
                        {
                            while (ReviewReader.Read())
                            {
                                DropDownListReview.Items.Add(ReviewReader["Review_Name"].ToString());
                            }
                            ReviewReader.Dispose();
                            ReviewReader.Close();
                        }

                        using (SqlDataReader oReader = sql_select_header.ExecuteReader())
                        {
                            if (oReader.HasRows)
                            {
                                while (oReader.Read())
                                {
                                    string start_date_formatted, end_date_formatted;
                                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + oReader["Period_ID"] + "";
                                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);

                                    DropDownListGroup.SelectedValue = oReader["FinancialHeader_Group"].ToString();
                                    TextBoxStretch.Value = oReader["FinancialHeader_StretchRating"].ToString();
                                    TextBoxStretchIndividual.Value = oReader["FinancialHeader_IndividualStretchRating"].ToString();
                                    LabelBreadCrumb.Text = oReader["FinancialHeader_Group"].ToString();
                                    LabelTitle.Text = oReader["FinancialHeader_Group"].ToString();
                                    DropDownListReview.SelectedValue = oReader["FinancialHeader_Review"].ToString();

                                    using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
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
                                cancel_edit_group.Attributes.Add("a href", "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "");
                            }
                            else
                            {
                                TextBoxStretch.Value = "0";
                                TextBoxStretchIndividual.Value = "0";
                                DropDownListGroup.Enabled = false;
                                LabelStartPeriod.InnerText = "No Period";
                                LabelEndPeriod.InnerText = "No Period";
                                DropDownListReview.Enabled = false;
                                SpanEditGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                                cancel_edit_group.Attributes.Add("a href", "financial_scorecard.aspx?page=1&id=1");
                            }
                        }
                        conn.Close();
                    }
                }
            }
        }

        protected void OnDropdownChanged(object sender, EventArgs e)
        {
            var page = Request.QueryString["page"];
            var header_id = Request.QueryString["header"];//get header id
            var period_id = Request.QueryString["period_id"];
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                // <> artinya NOT EQUAL
                string check_if_group_name_exist = "SELECT FinancialHeader_Group FROM FinancialMeasures_Header "
                                                 + "WHERE Period_ID=" + period_id + " "
                                                 + "AND FinancialHeader_ID<>" + header_id + " "
                                                 + "AND FinancialHeader_Group = '" + DropDownListGroup.SelectedValue + "'";
                SqlCommand sql_check_group_name = new SqlCommand(check_if_group_name_exist, conn);
                conn.Open();
                using (SqlDataReader GroupNameReader = sql_check_group_name.ExecuteReader())
                {
                    if (GroupNameReader.HasRows)
                        {
                            check_group_name.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            SpanEditGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                        }
                        else
                        {
                            check_group_name.Attributes.Add("style", "visibility:hidden; margin-bottom:0px !important; margin-top:-18px !important");
                            SpanEditGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        }
                    }
                conn.Close();
            }
        }

        protected void OnClickEdit(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var header_id = Request.QueryString["header"];//get header id
            var period_id = Request.QueryString["period_id"];//untuk Cancel Button saja
            float stretch_rat = float.Parse(TextBoxStretch.Value);
            float individual_stretch_rat = float.Parse(TextBoxStretchIndividual.Value);
            string review_period = DropDownListReview.SelectedValue;
            bool value_exist = true;
            string user_update = Session["user_name"].ToString();
            string date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string update_individual_rating_and_score = "", string_update_rating_financial_detail = "", update_individual_detail = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
              // <> artinya NOT EQUAL
              string check_if_group_name_exist = "SELECT FinancialHeader_Group FROM FinancialMeasures_Header "
                                               + "WHERE Period_ID=" + period_id + " AND FinancialHeader_ID<>" + header_id + " "
                                               + "AND FinancialHeader_Group='" + DropDownListGroup.SelectedValue + "' AND "
                                               + "data_status='exist'";
              SqlCommand sql_check_group_name = new SqlCommand(check_if_group_name_exist, conn);
              conn.Open();
              using (SqlDataReader GroupNameReader = sql_check_group_name.ExecuteReader())
              {
                  if (GroupNameReader.HasRows)
                  {
                      value_exist = true;
                  }
                  else
                  {
                      value_exist = false;
                  }
                  GroupNameReader.Dispose();
                  GroupNameReader.Close();
              }
             conn.Close();
            }

            if (value_exist == false)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    float current_financial_stretch = 0, current_individual_stretch = 0;
                    string select_financial_detail = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + header_id + "";
                    string select_current_financial_stretch = "SELECT FinancialHeader_StretchRating FROM FinancialMeasures_Header "
                                                            + "WHERE FinancialHeader_ID=" + header_id + "";
                    string select_current_individual_stretch = "SELECT FinancialHeader_IndividualStretchRating FROM FinancialMeasures_Header "
                                                             + "WHERE FinancialHeader_ID=" + header_id + "";
                    SqlCommand sql_current_financial_stretch = new SqlCommand(select_current_financial_stretch, conn);
                    SqlCommand sql_current_individual_stretch = new SqlCommand(select_current_individual_stretch, conn);
                    current_financial_stretch = float.Parse(sql_current_financial_stretch.ExecuteScalar().ToString());
                    current_individual_stretch = float.Parse(sql_current_individual_stretch.ExecuteScalar().ToString());

                    //ASUMSI NIK = 100
                    string update_financial_header = "UPDATE FinancialMeasures_Header SET FinancialHeader_Group=@group_name, "
                                                    + "FinancialHeader_StretchRating=@stretch_rating_value, FinancialHeader_Review=@review_period, "
                                                    + "Period_ID=@periode, FinancialHeader_IndividualStretchRating=@individual_stretch_rating, "
                                                    + "user_update=@user_update, date_update=@date_update, Review_ID=@review_id "
                                                    + "WHERE FinancialHeader_ID='" + header_id + "'";
                    SqlCommand sql_update_financial_header = new SqlCommand(update_financial_header, conn);
                    sql_update_financial_header.Parameters.AddWithValue("@group_name", DropDownListGroup.SelectedValue);
                    sql_update_financial_header.Parameters.AddWithValue("@stretch_rating_value", stretch_rat);
                    sql_update_financial_header.Parameters.AddWithValue("@review_period", review_period);
                    sql_update_financial_header.Parameters.AddWithValue("@periode", period_id);
                    sql_update_financial_header.Parameters.AddWithValue("@individual_stretch_rating", individual_stretch_rat);
                    sql_update_financial_header.Parameters.AddWithValue("@user_update", user_update);
                    sql_update_financial_header.Parameters.AddWithValue("@date_update", date_update);
                    if (DropDownListReview.SelectedIndex == 0)
                    {
                        sql_update_financial_header.Parameters.AddWithValue("@review_id", 1);
                    }
                    else if (DropDownListReview.SelectedIndex == 1)
                    {
                        sql_update_financial_header.Parameters.AddWithValue("@review_id", 5);
                    }
                    else if (DropDownListReview.SelectedIndex == 2)
                    {
                        sql_update_financial_header.Parameters.AddWithValue("@review_id", 7);
                    }
                    sql_update_financial_header.ExecuteNonQuery();

                    //UPDATE Rating terlebih dahulu untuk Financial Detail dan Individual Scorecard
                    if (stretch_rat <= current_financial_stretch)
                    {
                        string_update_rating_financial_detail = "UPDATE FinancialMeasures_Detail SET FinancialRating=" + stretch_rat + " "
                                                                        + "WHERE FinancialRating>" + stretch_rat + " AND FinancialHeader_ID=" + header_id + "";
                        SqlCommand sql_update_rating_financial_detail = new SqlCommand(string_update_rating_financial_detail, conn);
                        sql_update_rating_financial_detail.ExecuteNonQuery();
                    }
                    else if (stretch_rat > current_financial_stretch)
                    {
                        SqlCommand sql_select_financial_detail = new SqlCommand(select_financial_detail, conn);
                        using (SqlDataReader FinancialDetailReader = sql_select_financial_detail.ExecuteReader())
                        {
                            if (FinancialDetailReader.HasRows)
                            {
                                while (FinancialDetailReader.Read())
                                {
                                    if (FinancialDetailReader["FinancialFormula"].ToString() == "(Result/Target) x 100%")
                                    {
                                        string_update_rating_financial_detail = "UPDATE FinancialMeasures_Detail SET FinancialRating="
                                                                                + "CASE WHEN ROUND((FinancialResult/FinancialTarget)*100,2)>" + stretch_rat + " THEN " + stretch_rat + " "
                                                                                + "ELSE ROUND((FinancialResult/FinancialTarget)*100,2) END "
                                                                                + "WHERE FinancialDetail_ID=" + FinancialDetailReader["FinancialDetail_ID"] + "";
                                    }
                                    else if (FinancialDetailReader["FinancialFormula"].ToString() == "100% - ((Result - Target)/Target)")
                                    {
                                        string_update_rating_financial_detail = "UPDATE FinancialMeasures_Detail SET FinancialRating="
                                                                                + "CASE WHEN ROUND(100-((IndividualDetail_Result-IndividualDetail_Target)/IndividualDetail_Target)*100,2)>" + stretch_rat + " THEN " + stretch_rat + " "
                                                                                + "ELSE ROUND(100-((IndividualDetail_Result-IndividualDetail_Target)/IndividualDetail_Target)*100,2) END "
                                                                                + "WHERE FinancialDetail_ID=" + FinancialDetailReader["FinancialDetail_ID"] + "";
                                    }
                                    SqlCommand sql_update_rating_financial_detail = new SqlCommand(string_update_rating_financial_detail, conn);
                                    sql_update_rating_financial_detail.ExecuteNonQuery();
                                }
                                FinancialDetailReader.Dispose();
                                FinancialDetailReader.Close();
                            }
                        }
                    }

                    //UPDATE Score untuk Financial Measure
                    SqlCommand sql_financial_detail = new SqlCommand(select_financial_detail, conn);
                    using (SqlDataReader DetailReader = sql_financial_detail.ExecuteReader())
                    {
                        if (DetailReader.HasRows)
                        {
                            while (DetailReader.Read())
                            {
                                if ((int)DetailReader["FinancialLinked"] == 0)//untuk update score yang SINGLE atau TIDAK BERELASI
                                {
                                    string update_detail_score = "UPDATE FinancialMeasures_Detail SET "
                                                                + "FinancialScore=ROUND(FinancialRating*(0.01*FinancialWeight),2) "
                                                                + "WHERE FinancialDetail_ID=" + DetailReader["FinancialDetail_ID"] + "";
                                    SqlCommand sql_update_detail_score = new SqlCommand(update_detail_score, conn);
                                    sql_update_detail_score.ExecuteNonQuery();
                                }
                                else
                                {
                                    string update_share_type_detail = "UPDATE FinancialMeasures_Detail "
                                                                    + "SET FinancialScore=ROUND((SELECT SUM(FinancialRating) "
                                                                    + "FROM FinancialMeasures_Detail WHERE FinancialLinked=" + DetailReader["FinancialLinked"] + ")/(SELECT COUNT(FinancialLinked) "
                                                                    + "FROM FinancialMeasures_Detail WHERE FinancialLinked=" + DetailReader["FinancialLinked"] + ")*(0.01*(SELECT TOP(1) FinancialWeight "
                                                                    + "FROM FinancialMeasures_Detail WHERE FinancialLinked=" + DetailReader["FinancialLinked"] + ")),2) "
                                                                    + "WHERE FinancialLinked=" + DetailReader["FinancialLinked"] + "";
                                    SqlCommand sql_update_share_type_detail = new SqlCommand(update_share_type_detail, conn);
                                    sql_update_share_type_detail.ExecuteNonQuery();
                                }
                            }
                            DetailReader.Dispose();
                            DetailReader.Close();
                        }
                    }

                    //UPDATE Rating & Score untuk Individual Scorecard
                    if (individual_stretch_rat <= current_individual_stretch)
                    {
                        string select_emp_nik = "SELECT user_id FROM ScorecardUser "
                                              + "join ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode = ScorecardGroupLink.OrgAdtGroupCode "
                                              + "AND ScorecardGroupLink.Period_ID=" + period_id + " AND ScorecardGroupLink.Group_Name='" + DropDownListGroup.SelectedValue + "'";
                        SqlCommand sql_select_emp_nik = new SqlCommand(select_emp_nik, conn);
                        using (SqlDataReader NikReader = sql_select_emp_nik.ExecuteReader())
                        {
                            if (NikReader.HasRows)
                            {
                                while (NikReader.Read())
                                {
                                    string select_individual_scorecard = "SELECT * FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + " "
                                                                       + "AND user_id='" + NikReader["user_id"].ToString() + "'";
                                    SqlCommand sql_select_individual_scorecard = new SqlCommand(select_individual_scorecard, conn);
                                    using (SqlDataReader TargetReader = sql_select_individual_scorecard.ExecuteReader())
                                    {
                                        if (TargetReader.HasRows)
                                        {
                                            while (TargetReader.Read())
                                            {
                                                //UPDATE Individual Scorecard Rating yang memiliki Specific Objective
                                                if (TargetReader["IndividualHeader_Target"].ToString() == "-1")
                                                {
                                                    //UPDATE Individual Scorecard Rating yang memiliki Specific Objective
                                                    //UPDATE Individual Detail-nya terlebih dahulu agar SUM Rating-nya memakai value yang baru
                                                    //baru UPDATE Individual Header-nya
                                                    update_individual_detail = "UPDATE IndividualMeasures_Detail SET IndividualDetail_Rating=" + individual_stretch_rat + " "
                                                                                + "WHERE IndividualHeader_ID=" + TargetReader["IndividualHeader_ID"].ToString() + " "
                                                                                + "AND IndividualDetail_Rating>" + individual_stretch_rat + "";
                                                    update_individual_rating_and_score = "exec SP_CalculateRatingandScore " + TargetReader["IndividualHeader_ID"].ToString() + "";
                                                    SqlCommand sql_update_individual_detail = new SqlCommand(update_individual_detail, conn);
                                                    SqlCommand sql_update_individual_rating = new SqlCommand(update_individual_rating_and_score, conn);
                                                    sql_update_individual_detail.ExecuteNonQuery();
                                                    sql_update_individual_rating.ExecuteNonQuery();
                                                }
                                                else//UPDATE Individual Scorecard Rating yang tidak memiliki Specific Objective
                                                {
                                                    update_individual_rating_and_score = "exec SP_CalculateRatingandScoreNoSpecificObjective " + TargetReader["IndividualHeader_ID"].ToString() + ", " + individual_stretch_rat + "";
                                                    SqlCommand sql_update_individual_rating_and_score = new SqlCommand(update_individual_rating_and_score, conn);
                                                    sql_update_individual_rating_and_score.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                        TargetReader.Dispose();
                                        TargetReader.Close();
                                    }
                                }
                            }
                            NikReader.Dispose();
                            NikReader.Close();
                        }
                    }
                    else
                    {
                        string select_emp_nik = "SELECT user_id FROM ScorecardUser "
                                              + "join ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode = ScorecardGroupLink.OrgAdtGroupCode "
                                              + "AND ScorecardGroupLink.Period_ID=" + period_id + " AND ScorecardGroupLink.Group_Name='" + DropDownListGroup.SelectedValue + "'";
                        SqlCommand sql_select_emp_nik = new SqlCommand(select_emp_nik, conn);
                        using (SqlDataReader NikReader = sql_select_emp_nik.ExecuteReader())
                        {
                            if (NikReader.HasRows)
                            {
                                while (NikReader.Read())
                                {
                                    string select_individual_scorecard = "SELECT * FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + " AND user_id='" + NikReader["user_id"].ToString() + "'";
                                    SqlCommand sql_select_individual_scorecard = new SqlCommand(select_individual_scorecard, conn);
                                    using (SqlDataReader TargetReader = sql_select_individual_scorecard.ExecuteReader())
                                    {
                                        while (TargetReader.Read())
                                        {
                                            //UPDATE Individual Scorecard Rating yang memiliki Specific Objective
                                            if (TargetReader["IndividualHeader_Target"].ToString() == "-1")
                                            {
                                                string select_detail_formula = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualHeader_ID=" + TargetReader["IndividualHeader_ID"].ToString() + "";
                                                SqlCommand sql_select_detail_formula = new SqlCommand(select_detail_formula, conn);
                                                using (SqlDataReader FormulaReader = sql_select_detail_formula.ExecuteReader())
                                                {
                                                    if (FormulaReader.HasRows)
                                                    {
                                                        while (FormulaReader.Read())
                                                        {
                                                            if (FormulaReader["IndividualDetail_Formula"].ToString() == "(Result/Target) x 100%")
                                                            {
                                                                update_individual_detail = "UPDATE IndividualMeasures_Detail SET IndividualDetail_Rating= "
                                                                                            + "CASE WHEN ROUND((IndividualDetail_Result/IndividualDetail_Target)*100,2)>" + individual_stretch_rat + " THEN " + individual_stretch_rat + " "
                                                                                            + "ELSE ROUND((IndividualDetail_Result/IndividualDetail_Target)*100,2) END "
                                                                                            + "WHERE IndividualDetail_ID=" + FormulaReader["IndividualDetail_ID"].ToString() + "";
                                                                SqlCommand sql_update_individual_detail = new SqlCommand(update_individual_detail, conn);
                                                                sql_update_individual_detail.ExecuteNonQuery();
                                                            }
                                                            else if (FormulaReader["IndividualDetail_Formula"].ToString() == "100% - ((Result - Target)/Target)")
                                                            {
                                                                update_individual_detail = "UPDATE IndividualMeasures_Detail SET IndividualDetail_Rating= "
                                                                                            + "CASE WHEN ROUND(100-((IndividualDetail_Result-IndividualDetail_Target)/IndividualDetail_Target)*100,2)>" + individual_stretch_rat + " THEN " + individual_stretch_rat + " "
                                                                                            + "ELSE ROUND(100-((IndividualDetail_Result-IndividualDetail_Target)/IndividualDetail_Target)*100,2) END "
                                                                                            + "WHERE IndividualDetail_ID=" + FormulaReader["IndividualDetail_ID"].ToString() + "";
                                                                SqlCommand sql_update_individual_detail = new SqlCommand(update_individual_detail, conn);
                                                                sql_update_individual_detail.ExecuteNonQuery();
                                                            }
                                                        }
                                                    }
                                                }
                                                update_individual_rating_and_score = "exec SP_CalculateRatingandScoreIndividual " + TargetReader["IndividualHeader_ID"] + ", " + individual_stretch_rat + "";
                                                SqlCommand sql_update_individual_rating_and_score = new SqlCommand(update_individual_rating_and_score, conn);
                                                sql_update_individual_rating_and_score.ExecuteNonQuery();
                                            }
                                            else
                                            {
                                                update_individual_rating_and_score = "exec SP_CalculateRatingandScoreNoSpecObj " + TargetReader["IndividualHeader_ID"] + ", " + individual_stretch_rat + ", '" + TargetReader["IndividualHeader_Formula"].ToString() + "'";
                                                SqlCommand sql_update_individual_rating_and_score = new SqlCommand(update_individual_rating_and_score, conn);
                                                sql_update_individual_rating_and_score.ExecuteNonQuery();
                                            }
                                        }
                                        TargetReader.Dispose();
                                        TargetReader.Close();
                                    }
                                }
                            }
                            NikReader.Dispose();
                            NikReader.Close();
                        }
                    }

                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('This Group Has Been Updated!'); window.location='" + baseUrl + "/financial_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                    conn.Close();
                }
            }//end of if(value_exist==false)
        }//end of OnClickEdit

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