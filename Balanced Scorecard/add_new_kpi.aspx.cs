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
using System.Net.Mail;

namespace Balanced_Scorecard
{
    public partial class add_new_kpi : System.Web.UI.Page
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
                int start_date_bsc = 0, minus_one_bsc = 0, 
                    current_month = DateTime.Now.Month, 
                    month_num = 0, current_year = DateTime.Now.Year,
                    year_from_startdate = 0;


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

                //link breadcrumb
                individual_scorecard_breadcrumb.Attributes.Add("a href", "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                //untuk Button Add KPI
                FontAwesomeIcon.Attributes.Add("class","fa fa-fw fa-plus");
                ButtonAddKPI.Text = "Add";

                //ubah TextBoxTarget menjadi Input Type = "Number"
                TextBoxTarget.Attributes.Add("type", "number");
                TextBoxTarget.Attributes.Add("step", "0.01");
                TextBoxTarget.Attributes.Add("min", "0");
                TextBoxTarget.Text = "0";

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
                DropDownFormula.Items.Add("-");//digunakan karena default-nya adalah "Yes" pada Specific Objective
                                               //jika "Yes", Formula-nya berbeda
                DropDownFormula.Enabled = false;

                TextBoxTarget.Attributes.Add("disabled", "true");
                TextBoxTarget.Text = "0";
                TextBoxResult.Attributes.Add("disabled", "true");
                TextBoxResult.Value = "0";
                DropDownListMeasurement.Enabled = false;
                DropDownFormula.SelectedValue = "-";
                TextBoxRating.Attributes.Add("disabled", "true");
                TextBoxRating.Value = "0";
                TextBoxScore.Attributes.Add("disabled", "true");
                TextBoxScore.Value = "0";

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    //ambil value dari tabel BSC_Period
                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);

                    //ambil value Stretch Rating dan Review untuk Individual Scorecard. ASUMSI NIK ==> 100
                    string string_select_individual_stretch_review = "SELECT FinancialHeader_IndividualStretchRating, FinancialHeader_Review FROM ScorecardUser "
                                                                    + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                                    + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                                    + "WHERE ScorecardUser.user_id=" + Session["user_id"].ToString() + " AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                    SqlCommand sql_select_individual_stretch_review = new SqlCommand(string_select_individual_stretch_review, conn);
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
                                start_date_formatted = start_date.ToString("MMM,dd-yyyy");
                                end_date_formatted = end_date.ToString("MMM,dd-yyyy");
                                LabelStartPeriod.InnerText = start_date_formatted;
                                LabelEndPeriod.InnerText = end_date_formatted;
                                start_date_bsc = int.Parse(start_date.ToString("MM"));
                                year_from_startdate = int.Parse(start_date.ToString("yyyy"));
                            }
                            cancel_add_new_KPI.Attributes.Add("a href", "individual_scorecard.aspx?page="+page+"&id="+period_id+"");
                        }
                        else//jika periode tidak ditemukkan / disuntik langsung ke Database
                        {
                            LabelStartPeriod.InnerText = "No Period";
                            LabelEndPeriod.InnerText = "No Period";
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                            cancel_add_new_KPI.Attributes.Add("a href", "individual_scorecard.aspx?page="+page+"&id="+period_id+"");
                        }
                    }

                    using (SqlDataReader IndividualStretchReader = sql_select_individual_stretch_review.ExecuteReader())
                    {
                        if (IndividualStretchReader.HasRows)
                        {
                            while (IndividualStretchReader.Read())
                            {
                                LabelStretch.InnerText = IndividualStretchReader["FinancialHeader_IndividualStretchRating"].ToString() + "%";
                                LabelReview.InnerText = IndividualStretchReader["FinancialHeader_Review"].ToString();
                                TextBoxRating.Attributes.Add("max", IndividualStretchReader["FinancialHeader_IndividualStretchRating"].ToString());
                            }
                        }
                        else
                        {
                            LabelStretch.InnerText = "0%";
                            LabelReview.InnerText = " - ";
                            TextBoxRating.Attributes.Add("max", "0");
                        }
                    }

                    if (start_date_bsc == 1)
                    {
                        minus_one_bsc = 12;
                    }
                    else
                    {
                        minus_one_bsc = start_date_bsc - 1;
                    }

                    string string_select_active_month = "SELECT month_num FROM ScorecardReview WHERE "
                                                      + "Review_Status='Active' AND Review_Name='" + LabelReview.InnerText + "'";
                    string string_select_active_review_month = "SELECT Review_Status FROM ScorecardReview WHERE month_num=" + current_month + " AND Review_Status='Active' "
                                                              + "AND Review_Name='" + LabelReview.InnerText + "'";
                    SqlCommand sql_select_active_month = new SqlCommand(string_select_active_month, conn);
                    using (SqlDataReader MonthReader = sql_select_active_month.ExecuteReader())
                    {
                        while (MonthReader.Read())
                        {
                            month_num = int.Parse(MonthReader["month_num"].ToString());
                        }
                        MonthReader.Dispose();
                        MonthReader.Close();
                    }

                    if (current_month == start_date_bsc && current_year == year_from_startdate)
                    {
                        LabelAddRequest.Text = "Add";
                        LabelAddRequest2.Text = "Add";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-arrow-right");
                        ButtonAddKPI.Text = "Add Specific Objective";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");//default css
                        ButtonAddKPI.CssClass = "relative-to-btn-new-KPI";
                    }
                    else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)//jika mulai januari, desember udah bisa add
                    {
                        LabelAddRequest.Text = "Add";
                        LabelAddRequest2.Text = "Add";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-arrow-right");
                        ButtonAddKPI.Text = "Add Specific Objective";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");//default css
                        ButtonAddKPI.CssClass = "relative-to-btn-new-KPI";
                    }
                    else if (current_month == minus_one_bsc && minus_one_bsc != 12)//jika mulai maret, februari sudah bisa add
                    {
                        LabelAddRequest.Text = "Add";
                        LabelAddRequest2.Text = "Add";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-arrow-right");
                        ButtonAddKPI.Text = "Add Specific Objective";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");//default css
                        ButtonAddKPI.CssClass = "relative-to-btn-new-KPI";
                    }
                    else if (current_month == month_num)//untuk review
                    {
                        LabelAddRequest.Text = "Request";
                        LabelAddRequest2.Text = "Request";
                        ButtonAddKPI.Text = "Request Add";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-plus");
                        ButtonAddKPI.Text = "Request";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        SpanAddGroup.Attributes.Add("style", "width:120px; color:white");
                        ButtonAddKPI.Attributes.Add("style", "width:120px; color:white");
                        ButtonAddKPI.CssClass = "relative-to-btn";
                        ReasonTextBox.Attributes.Add("style", "visibility:visible; position:relative");
                        TextAreaReason.Attributes.Add("required", "required");
                    }
                    else
                    {
                        LabelAddRequest.Text = "Request";
                        LabelAddRequest2.Text = "Request";
                        ButtonAddKPI.Text = "Request Add";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-plus");
                        ButtonAddKPI.Text = "Request";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        SpanAddGroup.Attributes.Add("style", "width:120px; color:white");
                        ButtonAddKPI.Attributes.Add("style", "width:120px; color:white");
                        ButtonAddKPI.CssClass = "relative-to-btn";
                        ReasonTextBox.Attributes.Add("style", "visibility:visible; position:relative");
                        TextAreaReason.Attributes.Add("required", "required");
                    }
                    conn.Close();
                }
            }
        }

        protected void OnKPIChanged(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
            string check_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' "
                                          + "AND data_status='exist' AND Period_ID=" + period_id + " AND user_id=" + Session["user_id"].ToString() + "";
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_check_KPI_name = new SqlCommand(check_KPI_name, conn);
                using (SqlDataReader KPIReader = sql_check_KPI_name.ExecuteReader())
                {
                    if (KPIReader.HasRows)//jika nama KPI sudah ada
                    {
                        if (DropDownListSpecific.SelectedValue == "No")
                        {
                            check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                        }
                        else
                        {
                            check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI disabled");
                        }
                    }
                    else
                    {
                        if (DropDownListSpecific.SelectedValue == "No")
                        {
                            check_KPI_name_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-15px !important; margin-top:5px !important; color:red; font-weight:bold");
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        }
                        else
                        {
                            check_KPI_name_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-15px !important; margin-top:5px !important; color:red; font-weight:bold");
                            SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");
                        }
                    }
                }
                conn.Close();
            }
        }

        protected void OnClickAddKPI(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var period_id = Request.QueryString["period_id"];
            var page = Request.QueryString["page"];
            bool KPI_exist;
            string user_create, date_create, user_update, date_update;
            if (ButtonAddKPI.Text == "Add")//jika Header di Add tanpa Specific Objective
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    TextBoxKPI.Text = TextBoxKPI.Text.Replace("'", "''");
                    TextareaObjective.InnerText = TextareaObjective.InnerText.Replace("'", "''");

                    string check_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' "
                                          + "AND data_status='exist' AND Period_ID=" + period_id + " AND user_id=" + Session["user_id"].ToString() + "";
                    string insert_new_KPI = "INSERT INTO IndividualMeasures_Header VALUES(@KPI, @target, @result, @rating, @weight, @score, @user_create, @user_update, @objective, "
                                          + "@period_id, @formula, @measure, @data_status, @user_id, @date_create, @date_update)";
                    SqlCommand sql_insert_new_KPI = new SqlCommand(insert_new_KPI, conn);
                    SqlCommand sql_check_KPI_name = new SqlCommand(check_KPI_name,conn);
                    conn.Open();

                    user_create = Session["user_name"].ToString();
                    user_update = Session["user_name"].ToString();
                    date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    using (SqlDataReader KPIReader = sql_check_KPI_name.ExecuteReader())
                    {
                        if (KPIReader.HasRows)//jika nama KPI sudah ada
                        {
                            check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            KPI_exist = true;
                        }
                        else
                        {
                            KPI_exist = false;
                        }
                    }

                    if (KPI_exist == false)//jika nama KPI belum ada
                    {
                        sql_insert_new_KPI.Parameters.AddWithValue("@KPI", TextBoxKPI.Text);
                        sql_insert_new_KPI.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text),2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@user_create", user_create);
                        sql_insert_new_KPI.Parameters.AddWithValue("@date_create", date_create);
                        sql_insert_new_KPI.Parameters.AddWithValue("@user_update", user_update);
                        sql_insert_new_KPI.Parameters.AddWithValue("@date_update", date_update);
                        sql_insert_new_KPI.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                        sql_insert_new_KPI.Parameters.AddWithValue("@period_id", period_id);
                        sql_insert_new_KPI.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                        sql_insert_new_KPI.Parameters.AddWithValue("@measure", DropDownListMeasurement.SelectedValue);
                        sql_insert_new_KPI.Parameters.AddWithValue("@data_status", "exist");
                        sql_insert_new_KPI.Parameters.AddWithValue("@user_id", Session["user_id"]);
                        sql_insert_new_KPI.ExecuteNonQuery();

                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New KPI Added'); window.location='" + baseUrl + "individual_scorecard.aspx?page="+page+"&id=" + period_id + "';", true);
                    }
                    conn.Close();
                }
            }
            else if (ButtonAddKPI.Text == "Add Specific Objective")
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    //asumsi NIK=100
                    TextBoxKPI.Text = TextBoxKPI.Text.Replace("'", "''");
                    TextareaObjective.InnerText = TextareaObjective.InnerText.Replace("'", "''");

                    Object individual_header_id;
                    bool KPI_existence;
                    string check_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header WHERE "
                                          + "IndividualHeader_KPI='" + TextBoxKPI.Text + "' AND data_status='exist' "
                                          + "AND Period_ID=" + period_id + " AND user_id = " + Session["user_id"].ToString() + "";
                    string insert_new_KPI = "INSERT INTO IndividualMeasures_Header VALUES(@KPI, @target, @result, @rating, @weight, @score, @user_create, "
                                          + "@user_update, @objective, @period_id, @formula, @measure, @data_status, @user_id, @date_create, @date_update)";
                    string select_header_ID = "SELECT TOP 1 IndividualHeader_ID FROM IndividualMeasures_Header WHERE Period_ID="+period_id+" ORDER BY IndividualHeader_ID DESC";
                    SqlCommand sql_insert_new_KPI = new SqlCommand(insert_new_KPI, conn);
                    SqlCommand sql_select_last_inserted_header_ID = new SqlCommand(select_header_ID,conn);
                    SqlCommand sql_check_KPI_name = new SqlCommand(check_KPI_name, conn);
                    conn.Open();

                    user_create = Session["user_name"].ToString();
                    user_update = Session["user_name"].ToString();
                    date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    using (SqlDataReader KPIReader = sql_check_KPI_name.ExecuteReader())
                    {
                        if (KPIReader.HasRows)//jika nama KPI sudah ada
                        {
                            check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            KPI_existence = true;
                        }
                        else
                        {
                            KPI_existence = false;
                        }
                    }

                    if (KPI_existence == false)
                    {
                        sql_insert_new_KPI.Parameters.AddWithValue("@KPI", TextBoxKPI.Text);
                        sql_insert_new_KPI.Parameters.AddWithValue("@target", -1);//menjadi -1 karena ada Specific Objectives
                        sql_insert_new_KPI.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value),2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                        sql_insert_new_KPI.Parameters.AddWithValue("@user_create", user_create);
                        sql_insert_new_KPI.Parameters.AddWithValue("@date_create", date_create);
                        sql_insert_new_KPI.Parameters.AddWithValue("@user_update", user_update);
                        sql_insert_new_KPI.Parameters.AddWithValue("@date_update", date_update);
                        sql_insert_new_KPI.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                        sql_insert_new_KPI.Parameters.AddWithValue("@period_id", period_id);
                        sql_insert_new_KPI.Parameters.AddWithValue("@formula", "-");
                        sql_insert_new_KPI.Parameters.AddWithValue("@measure", "-");
                        sql_insert_new_KPI.Parameters.AddWithValue("@data_status", "exist");
                        sql_insert_new_KPI.Parameters.AddWithValue("@user_id", Session["user_id"]);
                        sql_insert_new_KPI.ExecuteNonQuery();

                        individual_header_id = sql_select_last_inserted_header_ID.ExecuteScalar();//mengambil ID yang barus aja di-Insert
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New KPI Added'); window.location='" + baseUrl + "add_specific_objective.aspx?page=" + page + "&header_id=" + (int)individual_header_id + "&period_id=" + period_id + "';", true);
                    }
                    conn.Close();
                }//end of SqlConnection
            }
            else if (ButtonAddKPI.Text == "Request")
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    TextBoxKPI.Text = TextBoxKPI.Text.Replace("'", "''");
                    TextareaObjective.InnerText = TextareaObjective.InnerText.Replace("'", "''");

                    bool KPI_existence;
                    string superior_id;
                    int last_request_id = 0;
                    string string_get_superior_id = "SELECT Superior_ID FROM ScorecardUser WHERE user_id=" + Session["user_id"].ToString() + "";
                    string check_KPI_name = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header "
                                          + "WHERE IndividualHeader_KPI='" + TextBoxKPI.Text + "' AND data_status='exist' "
                                          + "AND Period_ID=" + period_id + " AND user_id = " + Session["user_id"].ToString() + "";
                    string string_request_new_KPI = "INSERT INTO IndividualHeader_RequestChange VALUES(@KPI, @target, @result, @rating, @weight, "
                                                  + "@score, @objective, @formula, @measure, @user_create, @date_create, NULL,NULL,NULL,'pending', "
                                                  + "@period_id, @superior_id, "
                                                  + "@user_id, @reason, 2)";
                    string string_history_new_KPI = "INSERT INTO IndividualHeaderHistory VALUES(@KPI, @target, @result, @rating, @weight, "
                                                  + "@score, @objective, @user_create, @date_create, NULL, NULL, @request_id, @formula, "
                                                  + "@measure, @period_id)";

                    string get_last_request_id = "SELECT TOP(1) IndividualHeaderRequest_ID FROM IndividualHeader_RequestChange ORDER BY IndividualHeaderRequest_ID DESC";
                    SqlCommand sql_request_new_KPI = new SqlCommand(string_request_new_KPI, conn);
                    SqlCommand sql_history_new_KPI = new SqlCommand(string_history_new_KPI, conn);
                    SqlCommand sql_check_KPI_name = new SqlCommand(check_KPI_name, conn);
                    SqlCommand sql_get_superior_ID = new SqlCommand(string_get_superior_id, conn);
                    SqlCommand sql_get_last_request_id = new SqlCommand(get_last_request_id, conn);
                    conn.Open();

                    user_create = Session["user_name"].ToString();
                    date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    superior_id = (string)sql_get_superior_ID.ExecuteScalar();

                    using (SqlDataReader KPIReader = sql_check_KPI_name.ExecuteReader())
                    {
                        if (KPIReader.HasRows)//jika nama KPI sudah ada
                        {
                            check_KPI_name_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            KPI_existence = true;
                        }
                        else
                        {
                            KPI_existence = false;
                        }
                    }

                    if (KPI_existence == false)
                    {
                        if (DropDownListSpecific.SelectedValue == "Yes")
                        {
                            sql_request_new_KPI.Parameters.AddWithValue("@KPI", TextBoxKPI.Text);
                            sql_request_new_KPI.Parameters.AddWithValue("@target", -1);//menjadi -1 karena ada Specific Objectives
                            sql_request_new_KPI.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value),2));
                            sql_request_new_KPI.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@user_create", user_create);
                            sql_request_new_KPI.Parameters.AddWithValue("@date_create", date_create);
                            sql_request_new_KPI.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                            sql_request_new_KPI.Parameters.AddWithValue("@period_id", period_id);
                            sql_request_new_KPI.Parameters.AddWithValue("@formula", "-");
                            sql_request_new_KPI.Parameters.AddWithValue("@measure", "-");
                            sql_request_new_KPI.Parameters.AddWithValue("@superior_id", superior_id);
                            sql_request_new_KPI.Parameters.AddWithValue("@user_id", Session["user_id"]);
                            sql_request_new_KPI.Parameters.AddWithValue("@reason", TextAreaReason.InnerText);
                        }
                        else
                        {
                            sql_request_new_KPI.Parameters.AddWithValue("@KPI", TextBoxKPI.Text);
                            sql_request_new_KPI.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                            sql_request_new_KPI.Parameters.AddWithValue("@user_create", user_create);
                            sql_request_new_KPI.Parameters.AddWithValue("@date_create", date_create);
                            sql_request_new_KPI.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                            sql_request_new_KPI.Parameters.AddWithValue("@period_id", period_id);
                            sql_request_new_KPI.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                            sql_request_new_KPI.Parameters.AddWithValue("@measure", DropDownListMeasurement.SelectedValue);
                            sql_request_new_KPI.Parameters.AddWithValue("@superior_id", superior_id);
                            sql_request_new_KPI.Parameters.AddWithValue("@user_id", Session["user_id"]);
                            sql_request_new_KPI.Parameters.AddWithValue("@reason", TextAreaReason.InnerText);
                        }

                        sql_request_new_KPI.ExecuteNonQuery();

                        last_request_id = (int)sql_get_last_request_id.ExecuteScalar();

                        if (DropDownListSpecific.SelectedValue == "Yes")
                        {
                            sql_history_new_KPI.Parameters.AddWithValue("@KPI", TextBoxKPI.Text);
                            sql_history_new_KPI.Parameters.AddWithValue("@target", -1);//menjadi -1 karena ada Specific Objectives
                            sql_history_new_KPI.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                            sql_history_new_KPI.Parameters.AddWithValue("@user_create", user_create);
                            sql_history_new_KPI.Parameters.AddWithValue("@date_create", date_create);
                            sql_history_new_KPI.Parameters.AddWithValue("@request_id", last_request_id);
                            sql_history_new_KPI.Parameters.AddWithValue("@formula", "-");
                            sql_history_new_KPI.Parameters.AddWithValue("@measure", "-");
                            sql_history_new_KPI.Parameters.AddWithValue("@period_id", period_id);
                        }
                        else
                        {
                            sql_history_new_KPI.Parameters.AddWithValue("@KPI", TextBoxKPI.Text);
                            sql_history_new_KPI.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(TextBoxRating.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(TextBoxScore.Value), 2));
                            sql_history_new_KPI.Parameters.AddWithValue("@objective", TextareaObjective.InnerText);
                            sql_history_new_KPI.Parameters.AddWithValue("@user_create", user_create);
                            sql_history_new_KPI.Parameters.AddWithValue("@date_create", date_create);
                            sql_history_new_KPI.Parameters.AddWithValue("@request_id", last_request_id);
                            sql_history_new_KPI.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                            sql_history_new_KPI.Parameters.AddWithValue("@measure", DropDownListMeasurement.SelectedValue);
                            sql_history_new_KPI.Parameters.AddWithValue("@period_id", period_id);
                        }

                        sql_history_new_KPI.ExecuteNonQuery();

                        sendMail(last_request_id);

                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Your Request Has Been Sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                    }
                    conn.Close();
                }//end of SqlConnection
            }
        }

        public void sendMail(int request_id)
        {
            string strApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];
            var period_id = Request.QueryString["period_id"];

            string user_id = (string)Session["user_id"];

            string superior_email = "";
            string string_get_superior_email = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.user_id=" + user_id + " ) "
                                             + "SELECT empEmail FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";
            string string_get_user_info = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, OrgName, OrgAdtGroupName, "
                                        + "JobTtlName, LOWER(ScorecardUser.empEmail) as Email, "
                                        + "empGrade, Group_Name, FinancialHeader_IndividualStretchRating, EmpSex, EmpMaritalSt "
                                        + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                        + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                        + "join [Human_Capital_demo].dbo.Employee on ScorecardUser.EmpId = Employee.EmpId "
                                        + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                        + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                        + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                        + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                        + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                        + "AND FinancialMeasures_Header.data_status='exist' "
                                        + "WHERE FinancialMeasures_Header.Period_ID=" + period_id + " AND ScorecardUser.user_id=" + user_id + "";
            string string_get_new_KPI = "SELECT * FROM IndividualHeader_RequestChange WHERE IndividualHeaderRequest_ID=" + request_id + "";
            string user_title = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_get_user_info = new SqlCommand(string_get_user_info, conn);
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

                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Add New KPI ("+user_title+" " + UserReader["empName"].ToString() + " - " + UserReader["EmpId"].ToString() + ")");
                            sb_body_introduction.Append("Hello, my name is " + user_title + " " + UserReader["empName"].ToString() + " and this is my information: <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to add new following KPI to my individual scorecard:<br/><br/>");

                            sb_conclusion.Append("Link to Balanced Scorecard Application: " + strApplicationURL + " <br/><br/>Thank you. <br/><br/>Best Regards, <br/>" + user_title + " " + UserReader["empName"].ToString() + ""
                                               + "<br/><br/>This is an automatically generated email – please do not reply to it.");
                    }
                    UserReader.Dispose();
                    UserReader.Close();
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
                    
                    msg.Body = sb_body_introduction.ToString() + sb_new_detail.ToString() + sb_conclusion.ToString();

                    if (sb_from_email.ToString() == "" || sb_from_email.ToString() == "-")
                    {
                        msg.From = new MailAddress("message@error.com");
                    }
                    else
                    {
                        msg.From = new MailAddress(sb_from_email.ToString().ToLower());
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

        protected void OnSelectSpecific(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
            string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
            int start_date_bsc = 0, minus_one_bsc = 0,
                   current_month = DateTime.Now.Month, 
                   month_num = 0, current_year = DateTime.Now.Year,
                   year_from_startdate = 0;

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                using (SqlDataReader oReader = sql_select_period.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        DateTime start_date = Convert.ToDateTime(oReader["Start_Period"]);
                        start_date_bsc = int.Parse(start_date.ToString("MM"));
                        year_from_startdate = int.Parse(start_date.ToString("yyyy"));
                    }
                }

                if (start_date_bsc == 1)
                {
                    minus_one_bsc = 12;
                }
                else
                {
                    minus_one_bsc = start_date_bsc - 1;
                }

                string string_select_active_month = "SELECT month_num FROM ScorecardReview WHERE "
                                                  + "Review_Status='Active' AND Review_Name='" + LabelReview.InnerText + "'";
                string string_select_active_review_month = "SELECT Review_Status FROM ScorecardReview WHERE month_num=" + current_month + " AND Review_Status='Active' "
                                                          + "AND Review_Name='" + LabelReview.InnerText + "'";
                SqlCommand sql_select_active_month = new SqlCommand(string_select_active_month, conn);
                using (SqlDataReader MonthReader = sql_select_active_month.ExecuteReader())
                {
                    while (MonthReader.Read())
                    {
                        month_num = int.Parse(MonthReader["month_num"].ToString());
                    }
                    MonthReader.Dispose();
                    MonthReader.Close();
                }

                conn.Close();
            }

            if (ButtonAddKPI.Text == "Add Specific Objective" || ButtonAddKPI.Text == "Add")
            {
                if (current_month == start_date_bsc && current_year == year_from_startdate)
                {
                    if (DropDownListSpecific.SelectedValue == "Yes")
                    {
                        TextBoxTarget.Attributes.Add("disabled", "true");
                        TextBoxTarget.Text = "0";
                        TextBoxResult.Attributes.Add("disabled", "true");
                        TextBoxResult.Value = "0";
                        DropDownListMeasurement.Enabled = false;
                        DropDownFormula.Items.Add("-");
                        DropDownFormula.SelectedValue = "-";
                        DropDownFormula.Enabled = false;
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxRating.Value = "0";
                        TextBoxScore.Attributes.Add("disabled", "true");
                        TextBoxScore.Value = "0";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-arrow-right");
                        ButtonAddKPI.Text = "Add Specific Objective";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");
                        ButtonAddKPI.CssClass = "relative-to-btn-new-KPI";
                        check_target_value.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                    }
                    else if (DropDownListSpecific.SelectedValue == "No")
                    {
                        TextBoxTarget.Attributes.Remove("disabled");
                        TextBoxResult.Attributes.Add("disabled", "true");
                        DropDownListMeasurement.Enabled = true;
                        DropDownFormula.Items.Remove("-");
                        DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                        DropDownFormula.Enabled = true;
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxScore.Attributes.Add("disabled", "true");
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-plus");
                        ButtonAddKPI.Text = "Add";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        ButtonAddKPI.CssClass = "relative-to-btn";
                    }
                }
                else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                {
                    if (DropDownListSpecific.SelectedValue == "Yes")
                    {
                        TextBoxTarget.Attributes.Add("disabled", "true");
                        TextBoxTarget.Text = "0";
                        TextBoxResult.Attributes.Add("disabled", "true");
                        TextBoxResult.Value = "0";
                        DropDownListMeasurement.Enabled = false;
                        DropDownFormula.Items.Add("-");
                        DropDownFormula.SelectedValue = "-";
                        DropDownFormula.Enabled = false;
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxRating.Value = "0";
                        TextBoxScore.Attributes.Add("disabled", "true");
                        TextBoxScore.Value = "0";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-arrow-right");
                        ButtonAddKPI.Text = "Add Specific Objective";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");
                        ButtonAddKPI.CssClass = "relative-to-btn-new-KPI";
                        check_target_value.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                    }
                    else if (DropDownListSpecific.SelectedValue == "No")
                    {
                        TextBoxTarget.Attributes.Remove("disabled");
                        TextBoxResult.Attributes.Add("disabled", "true");
                        DropDownListMeasurement.Enabled = true;
                        DropDownFormula.Items.Remove("-");
                        DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                        DropDownFormula.Enabled = true;    
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxScore.Attributes.Add("disabled", "true");
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-plus");
                        ButtonAddKPI.Text = "Add";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        ButtonAddKPI.CssClass = "relative-to-btn";
                    }
                }
                else if (current_month == minus_one_bsc && minus_one_bsc != 12)//mulai maret, februari sudah bisa
                {
                    if (DropDownListSpecific.SelectedValue == "Yes")
                    {
                        TextBoxTarget.Attributes.Add("disabled", "true");
                        TextBoxTarget.Text = "0";
                        TextBoxResult.Attributes.Add("disabled", "true");
                        TextBoxResult.Value = "0";
                        DropDownListMeasurement.Enabled = false;
                        DropDownFormula.Items.Add("-");
                        DropDownFormula.SelectedValue = "-";
                        DropDownFormula.Enabled = false;
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxRating.Value = "0";
                        TextBoxScore.Attributes.Add("disabled", "true");
                        TextBoxScore.Value = "0";
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-arrow-right");
                        ButtonAddKPI.Text = "Add Specific Objective";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");
                        ButtonAddKPI.CssClass = "relative-to-btn-new-KPI";
                        check_target_value.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                    }
                    else if (DropDownListSpecific.SelectedValue == "No")
                    {
                        TextBoxTarget.Attributes.Remove("disabled");
                        TextBoxResult.Attributes.Add("disabled", "true");
                        DropDownListMeasurement.Enabled = true;
                        DropDownFormula.Items.Remove("-");
                        DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                        DropDownFormula.Enabled = true;
                        TextBoxRating.Attributes.Add("disabled", "true");
                        TextBoxScore.Attributes.Add("disabled", "true");
                        FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-plus");
                        ButtonAddKPI.Text = "Add";
                        SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                        ButtonAddKPI.CssClass = "relative-to-btn";
                    }
                }
            }
            else if (ButtonAddKPI.Text == "Request")
            {
                if (DropDownListSpecific.SelectedValue == "Yes")
                {
                    TextBoxTarget.Attributes.Add("disabled", "true");
                    TextBoxTarget.Text = "0";
                    TextBoxResult.Attributes.Add("disabled", "true");
                    TextBoxResult.Value = "0";
                    DropDownListMeasurement.Enabled = false;
                    DropDownFormula.Items.Add("-");
                    DropDownFormula.SelectedValue = "-";
                    DropDownFormula.Enabled = false;
                    TextBoxRating.Attributes.Add("disabled", "true");
                    TextBoxRating.Value = "0";
                    TextBoxScore.Attributes.Add("disabled", "true");
                    TextBoxScore.Value = "0";
                    FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-plus");
                    ButtonAddKPI.Text = "Request";
                    SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button-new-KPI");
                    ButtonAddKPI.CssClass = "relative-to-btn-new-KPI";
                    check_target_value.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
                }
                else if (DropDownListSpecific.SelectedValue == "No")
                {
                    TextBoxTarget.Attributes.Remove("disabled");
                    TextBoxResult.Attributes.Add("disabled", "true");
                    DropDownListMeasurement.Enabled = true;
                    DropDownFormula.Items.Remove("-");
                    DropDownFormula.SelectedValue = "(Result/Target) x 100%";
                    DropDownFormula.Enabled = true;
                    TextBoxRating.Attributes.Add("disabled", "true");
                    TextBoxScore.Attributes.Add("disabled", "true");
                    FontAwesomeIcon.Attributes.Add("class", "fa fa-fw fa-plus");
                    ButtonAddKPI.Text = "Request";
                    SpanAddGroup.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                    ButtonAddKPI.CssClass = "relative-to-btn";
                }
            }
        }

        protected void OnSelectMeasureBy(object sender, EventArgs e)
        {
            if (DropDownListMeasurement.SelectedValue == "Month")
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
                TextBoxTarget.Attributes.Add("step", "0.01");
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