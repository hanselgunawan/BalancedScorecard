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
using System.Net.Mail;

namespace Balanced_Scorecard
{
    public partial class add_specific_objective : System.Web.UI.Page
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
                string user_nik = Session["user_nik"].ToString();

                int start_date_bsc = 0, minus_one_bsc = 0,
                    current_month = DateTime.Now.Month,
                    month_num = 0, current_year = DateTime.Now.Year,
                    year_from_startdate = 0;

                //link breadcrumb
                individual_scorecard_breadcrumb.Attributes.Add("a href", "individual_scorecard.aspx?page="+page+"&id="+period_id+"");

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

                //Add Item to DropDown Formula
                DropDownFormula.Items.Add("(Result/Target) x 100%");
                DropDownFormula.Items.Add("100% - ((Result - Target)/Target)");
                DropDownFormula.Enabled = true;
                
                //disabled Result dan Score, karena yang hanya bisa di-input adalah Target & SO
                TextBoxResult.Attributes.Add("disabled", "true");
                TextBoxRating.Attributes.Add("disabled", "true");

                cancel_add.Attributes.Add("href", "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "");

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
                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID="+period_id+"";
                    string select_individual_header = "SELECT * FROM IndividualMeasures_Header "
                                                    + "WHERE IndividualHeader_ID="+header_id+"";
                    string string_select_individual_stretch_review = "SELECT FinancialHeader_IndividualStretchRating, FinancialHeader_Review FROM ScorecardUser "
                                                                    + "join ScorecardGroupLink ON ScorecardGroupLink.OrgAdtGroupCode=ScorecardUser.empOrgAdtGroupCode AND ScorecardGroupLink.Period_ID=" + period_id + " "
                                                                    + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                                                    + "WHERE ScorecardUser.EmpId='" + user_nik + "' AND FinancialMeasures_Header.Period_ID=" + period_id + "";
                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
                    SqlCommand sql_select_individual_header = new SqlCommand(select_individual_header, conn);
                    SqlCommand sql_select_individual_stretch_review = new SqlCommand(string_select_individual_stretch_review, conn);
                    conn.Open();

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
                                start_date_bsc = int.Parse(start_date.ToString("MM"));
                                year_from_startdate = int.Parse(start_date.ToString("yyyy"));
                            }
                        }
                        else//jika periode tidak ditemukkan / disuntik langsung ke Database
                        {
                            LabelStartPeriod.InnerText = "No Period";
                            LabelEndPeriod.InnerText = "No Period";
                            SpanDone.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                            SpanAddMore.Attributes.Add("class", "btn btn-add-more btn-add-more-container add-button disabled");
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
                    string string_select_active_review_month = "SELECT Review_Status FROM ScorecardReview WHERE month_num=" + current_month + " "
                                                              + "AND Review_Status='Active' "
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
                        LabelRequestAdd.Text = "Add";
                        LabelRequestAdd2.Text = "Add";
                    }
                    else if (current_month == minus_one_bsc && current_year == year_from_startdate - 1)
                    {
                        LabelRequestAdd.Text = "Add";
                        LabelRequestAdd2.Text = "Add";
                    }
                    else if (current_month == minus_one_bsc && minus_one_bsc != 12)//jika maret, februari sudah bisa add
                    {
                        LabelRequestAdd.Text = "Add";
                        LabelRequestAdd2.Text = "Add";
                    }
                    else if (current_month == month_num)//untuk review
                    {
                        LabelRequestAdd.Text = "Request Add";
                        LabelRequestAdd2.Text = "Request Add";
                        SpanDone.Attributes.Add("style", "visibility:hidden");
                        ButtonAddMore.Text = "Request";
                        ReasonTextBox.Attributes.Add("style", "visibility:visible; position:relative");
                        TextAreaReason.Attributes.Add("required", "required");
                    }
                    else
                    {
                        LabelRequestAdd.Text = "Request Add";
                        LabelRequestAdd2.Text = "Request Add";
                        SpanDone.Attributes.Add("style", "visibility:hidden");
                        ButtonAddMore.Text = "Request";
                        ReasonTextBox.Attributes.Add("style", "visibility:visible; position:relative");
                        TextAreaReason.Attributes.Add("required", "required");
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
                        else
                        {
                            LabelKPI.InnerText = "KPI Not Found";
                            LabelBreadcrumb.Text = "Unknown";
                            LabelTitle.Text = "Unknown";
                        }
                    }

                    conn.Close();
                }
            }//end of if(!IsPostBack)
        }

        protected void OnSpecificChanged(object sender, EventArgs e)
        {
            var header_id = Request.QueryString["header_id"];
            string check_specific_objective = "SELECT IndividualDetail_Title FROM IndividualMeasures_Detail "
                                            + "WHERE IndividualDetail_Title='" + TextBoxSpecificObjective.Text + "' AND data_status='exist' "
                                            + "AND IndividualHeader_ID=" + header_id + "";
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                using (SqlDataReader SOReader = check_SO.ExecuteReader())
                {
                    if (SOReader.HasRows)//jika nama Specific Objective sudah ada
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SpanAddMore.Attributes.Add("class", "btn btn-add-more-finance btn-add-more-container add-button disabled");
                        SpanDone.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                    }
                    else
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:hidden; margin-bottom:-20px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SpanAddMore.Attributes.Add("class", "btn btn-add-more-finance btn-add-more-container add-button");
                        SpanDone.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button");
                    }
                }
                conn.Close();
            }
        }

        protected void OnClickAddMore(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            float total_header_score, sum_individual_rating, count_individual_rating_data_by_header;
            float total_rating = 0;
            int last_insert_request_id = 0;
            bool SO_exist;
            string user_create, date_create, user_update, date_update, superior_id, individual_header;
            string select_individual_header = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header "
                                            + "WHERE IndividualHeader_ID=" + header_id + "";
            string string_insert_individual_detail_request = "INSERT INTO IndividualDetail_RequestChange VALUES(@title, @target, @result, "
                                                           + "@measure_by, @rating, @formula, 'pending', NULL, @date_create, @user_create, NULL,NULL, @period_id, "
                                                           + "@superior_id, @user_id, @reason, 2)";
            string string_insert_individual_detail_history = "INSERT INTO IndividualDetailHistory VALUES(NULL, @header_id, @title, @target, @result, "
                                                           + "@measure_by, @rating, @formula, @user_create, @date_create, @user_update, @date_update, "
                                                           + "@request_id, @period_id, @header_kpi)";
            string string_get_superior_id = "SELECT Superior_ID FROM ScorecardUser WHERE user_id=" + Session["user_id"].ToString() + "";
            string string_get_last_request_id = "SELECT TOP(1) IndividualDetailRequest_ID "
                                              + "FROM IndividualDetail_RequestChange ORDER BY IndividualDetailRequest_ID DESC";
            string check_specific_objective = "SELECT IndividualDetail_Title FROM IndividualMeasures_Detail "
                                            + "WHERE IndividualDetail_Title='" + TextBoxSpecificObjective.Text + "' AND data_status='exist' "
                                            + "AND IndividualHeader_ID=" + header_id + "";
            string insert_specific_objective = "INSERT INTO IndividualMeasures_Detail VALUES(@header_id, @title, @target, @result, "
                                             + "@measure, ROUND(@rating,2), @user_create, @user_update, @formula, @data_status, "
                                             + "@date_create, @date_update)";
            string sum_detail_rating = "SELECT SUM(IndividualDetail_Rating) FROM IndividualMeasures_Detail "
                                     + "WHERE IndividualHeader_ID=" + header_id + " AND data_status='exist'";
            string count_individual_rating = "SELECT COUNT(*) FROM IndividualMeasures_Detail WHERE "
                                           + "IndividualHeader_ID=" + header_id + " AND data_status='exist'";
            string header_total_score = "UPDATE IndividualMeasures_Header SET IndividualHeader_Rating=ROUND(@total_header_score,2), "
                                      + "IndividualHeader_Score=ROUND((@total_header_score*(IndividualHeader_Weight/100)),2), "
                                      + "user_update=@user_update, date_update=@date_update WHERE IndividualHeader_ID=" + header_id + "";

            //untuk PERHITUNGAN
            if (DropDownFormula.SelectedValue == "(Result/Target) x 100%")
            {
                if (TextBoxResult.Value == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else
                {
                    total_rating = float.Parse(TextBoxResult.Value) / float.Parse(TextBoxTarget.Text);
                }
            }
            else if (DropDownFormula.SelectedValue == "100% - ((Result - Target)/Target)")
            {
                if (TextBoxResult.Value == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else if(TextBoxResult.Value == "0")
                {
                    total_rating = 0;
                }
                else
                {
                    total_rating = 100 - (((float.Parse(TextBoxResult.Value) - float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text))*100);
                }
            }

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                SqlCommand sql_insert_specific_objective = new SqlCommand(insert_specific_objective, conn);
                SqlCommand sql_insert_new_specific_objective_request = new SqlCommand(string_insert_individual_detail_request, conn);
                SqlCommand sql_insert_specific_objective_history = new SqlCommand(string_insert_individual_detail_history, conn);
                SqlCommand sql_get_superior_id = new SqlCommand(string_get_superior_id, conn);
                SqlCommand sql_get_last_req_id = new SqlCommand(string_get_last_request_id, conn);
                SqlCommand sql_sum_individual_rating = new SqlCommand(sum_detail_rating, conn);
                SqlCommand sql_count_individual_rating = new SqlCommand(count_individual_rating, conn);
                SqlCommand sql_update_header = new SqlCommand(header_total_score, conn);
                SqlCommand sql_get_individual_header = new SqlCommand(select_individual_header, conn);
                conn.Open();

                using (SqlDataReader SOReader = check_SO.ExecuteReader())
                {
                    if (SOReader.HasRows)//jika nama KPI sudah ada
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SO_exist = true;
                    }
                    else
                    {
                        SO_exist = false;
                    }
                }

                individual_header = (string)sql_get_individual_header.ExecuteScalar();
                superior_id = (string)sql_get_superior_id.ExecuteScalar();
                user_create = Session["user_name"].ToString();
                user_update = Session["user_name"].ToString();
                date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (SO_exist == false)//jika Specific Objective tidak ditemukan
                {
                    if (ButtonAddMore.Text == "Add More")
                    {
                        sql_insert_specific_objective.Parameters.AddWithValue("@header_id", header_id);
                        sql_insert_specific_objective.Parameters.AddWithValue("@title", TextBoxSpecificObjective.Text);
                        sql_insert_specific_objective.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text),2));
                        sql_insert_specific_objective.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                        sql_insert_specific_objective.Parameters.AddWithValue("@measure", DropDownListMeasurement.SelectedValue);
                        sql_insert_specific_objective.Parameters.AddWithValue("@rating", Math.Round(total_rating,2));
                        sql_insert_specific_objective.Parameters.AddWithValue("@user_create", user_create);
                        sql_insert_specific_objective.Parameters.AddWithValue("@date_create", date_create);
                        sql_insert_specific_objective.Parameters.AddWithValue("@user_update", user_update);
                        sql_insert_specific_objective.Parameters.AddWithValue("@date_update", date_update);
                        sql_insert_specific_objective.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                        sql_insert_specific_objective.Parameters.AddWithValue("@data_status", "exist");

                        sql_insert_specific_objective.ExecuteNonQuery();//insert Specific Objective

                        //untuk meng-update Header-nya
                        sum_individual_rating = float.Parse(sql_sum_individual_rating.ExecuteScalar().ToString());
                        count_individual_rating_data_by_header = float.Parse(sql_count_individual_rating.ExecuteScalar().ToString());
                        total_header_score = sum_individual_rating / count_individual_rating_data_by_header;

                        sql_update_header.Parameters.AddWithValue("@total_header_score", total_header_score);//untuk mengupdate header
                        sql_update_header.Parameters.AddWithValue("@user_update", user_update);
                        sql_update_header.Parameters.AddWithValue("@date_update", date_update);

                        sql_update_header.ExecuteNonQuery();//update Header

                        
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Specific Objective Added!'); window.location='" + baseUrl + "add_specific_objective.aspx?page=" + page + "&period_id=" + period_id + "&header_id=" + header_id + "';", true);
                    }
                    else if (ButtonAddMore.Text == "Request")
                    {
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@title", TextBoxSpecificObjective.Text);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text), 2));
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@measure_by", DropDownListMeasurement.SelectedValue);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@rating", Math.Round(total_rating,2));
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@date_create", date_create);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@user_create", user_create);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@period_id", period_id);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@superior_id", superior_id);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@user_id", Session["user_id"]);
                        sql_insert_new_specific_objective_request.Parameters.AddWithValue("@reason", TextAreaReason.InnerText);
                        sql_insert_new_specific_objective_request.ExecuteNonQuery();

                        last_insert_request_id = (int)sql_get_last_req_id.ExecuteScalar();

                        sql_insert_specific_objective_history.Parameters.AddWithValue("@header_id", header_id);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@title", TextBoxSpecificObjective.Text);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(TextBoxTarget.Text), 2));
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(TextBoxResult.Value), 2));
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@measure_by", DropDownListMeasurement.SelectedValue);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@rating", Math.Round(total_rating,2));
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@user_create", user_create);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@date_create", date_create);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@user_update", user_update);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@date_update", date_update);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@request_id", last_insert_request_id);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@period_id", period_id);
                        sql_insert_specific_objective_history.Parameters.AddWithValue("@header_kpi", individual_header);
                        sql_insert_specific_objective_history.ExecuteNonQuery();

                        sendMail(last_insert_request_id);

                        
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('You request has been sent. Please wait for the approval.'); window.location='" + baseUrl + "individual_scorecard.aspx?page=" + page + "&id=" + period_id + "';", true);
                    }
                }
                conn.Close();
            }
        }

        public void sendMail(int request_id)
        {
            string strApplicationURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationURL"];

            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];

            string superior_email = "";
            string string_get_superior_email = "with SuperiorInfo(Superior_ID) AS "
                                             + "( SELECT su.Superior_ID FROM ScorecardUser su WHERE su.user_id=" + Session["user_id"].ToString() + " ) "
                                             + "SELECT empEmail FROM ScorecardUser WHERE ScorecardUser.EmpId "
                                             + "= (SELECT Superior_ID FROM SuperiorInfo)";
            string string_get_user_info = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, OrgName, OrgAdtGroupName, JobTtlName, "
                                        + "LOWER(ScorecardUser.empEmail) as Email, "
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
            string string_get_new_specific_objective = "SELECT * FROM IndividualDetail_RequestChange WHERE IndividualDetailRequest_ID=" + request_id + "";
            string user_title = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_get_user_info = new SqlCommand(string_get_user_info, conn);
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

                        sb_from_email.Append(UserReader["Email"].ToString());
                        sb_subject.Append("Request for Add New KPI's Specific Objective ("+user_title+" " + UserReader["empName"].ToString() + " - " + UserReader["EmpId"].ToString() + ")");
                        sb_body_introduction.Append("Hello, my name is "+user_title+" " + UserReader["empName"].ToString() + " and this is my information: <br/>"
                                + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                + "I would like to add new the following Specific Objective to my " + UserReader["IndividualHeader_KPI"].ToString() + " KPI:<br/><br/>");
                        sb_conclusion.Append("Link to Balanced Scorecard Application: " + strApplicationURL + " <br/>Thank you. <br/><br/>Best Regards, <br/>" + user_title + " " + UserReader["empName"].ToString() + ""
                                           + "<br/><br/>This is an automatically generated email – please do not reply to it.");
                    }
                    UserReader.Dispose();
                    UserReader.Close();
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

        protected void OnClickDone(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var period_id = Request.QueryString["period_id"];
            var header_id = Request.QueryString["header_id"];
            float total_header_score, sum_individual_rating, count_individual_rating_data_by_header;
            float total_rating = 0;
            bool SO_exist;
            string user_create, date_create, user_update, date_update;
            string check_specific_objective = "SELECT IndividualDetail_Title FROM IndividualMeasures_Detail WHERE "
                                            + "IndividualDetail_Title='" + TextBoxSpecificObjective.Text + "' AND data_status='exist' "
                                            + "AND IndividualHeader_ID=" + header_id + "";
            string insert_specific_objective = "INSERT INTO IndividualMeasures_Detail VALUES(@header_id, @title, @target, @result, @measure, ROUND(@rating,2), @user_create, @user_update, @formula, @data_status, @date_create, @date_update)";
            string sum_detail_rating = "SELECT SUM(IndividualDetail_Rating) FROM IndividualMeasures_Detail WHERE IndividualHeader_ID=" + header_id + " AND data_status='exist'";
            string count_individual_rating = "SELECT COUNT(*) FROM IndividualMeasures_Detail WHERE IndividualHeader_ID=" + header_id + " AND data_status='exist'";
            string header_total_score = "UPDATE IndividualMeasures_Header SET IndividualHeader_Rating=ROUND(@total_header_score,2), IndividualHeader_Score=ROUND((@total_header_score*(IndividualHeader_Weight/100)),2) WHERE IndividualHeader_ID=" + header_id + "";

            //untuk PERHITUNGAN
            if (DropDownFormula.SelectedValue == "(Result/Target) x 100%")
            {
                if (TextBoxResult.Value == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else
                {
                    total_rating = float.Parse(TextBoxResult.Value) / float.Parse(TextBoxTarget.Text);
                }
            }
            else if (DropDownFormula.SelectedValue == "100% - ((Result - Target)/Target)")
            {
                if (TextBoxResult.Value == "0" && TextBoxTarget.Text == "0")
                {
                    total_rating = 0;//untuk handle jika ada jawaban yang UNLIMITED
                }
                else if (TextBoxResult.Value == "0")
                {
                    total_rating = 0;
                }
                else
                {
                    total_rating = 100 - (((float.Parse(TextBoxResult.Value) / float.Parse(TextBoxTarget.Text)) / float.Parse(TextBoxTarget.Text))*100);
                }
            }

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                SqlCommand check_SO = new SqlCommand(check_specific_objective, conn);
                SqlCommand sql_insert_specific_objective = new SqlCommand(insert_specific_objective, conn);
                SqlCommand sql_sum_individual_rating = new SqlCommand(sum_detail_rating, conn);
                SqlCommand sql_count_individual_rating = new SqlCommand(count_individual_rating, conn);
                SqlCommand sql_update_header = new SqlCommand(header_total_score, conn);
                conn.Open();

                user_create = Session["user_name"].ToString();
                user_update = Session["user_name"].ToString();
                date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                using (SqlDataReader SOReader = check_SO.ExecuteReader())
                {
                    if (SOReader.HasRows)//jika nama KPI sudah ada
                    {
                        specific_objective_error_message.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                        SO_exist = true;//jika SO ditemukan
                    }
                    else
                    {
                        SO_exist = false;
                    }
                }

                if (SO_exist == false)//jika SO tidak ditemukan
                {
                    sql_insert_specific_objective.Parameters.AddWithValue("@header_id", header_id);
                    sql_insert_specific_objective.Parameters.AddWithValue("@title", TextBoxSpecificObjective.Text);
                    sql_insert_specific_objective.Parameters.AddWithValue("@target", TextBoxTarget.Text);
                    sql_insert_specific_objective.Parameters.AddWithValue("@result", TextBoxResult.Value);
                    sql_insert_specific_objective.Parameters.AddWithValue("@measure", DropDownListMeasurement.SelectedValue);
                    sql_insert_specific_objective.Parameters.AddWithValue("@rating", total_rating);
                    sql_insert_specific_objective.Parameters.AddWithValue("@user_create", user_create);
                    sql_insert_specific_objective.Parameters.AddWithValue("@date_create", date_create);
                    sql_insert_specific_objective.Parameters.AddWithValue("@user_update", user_update);
                    sql_insert_specific_objective.Parameters.AddWithValue("@date_update", date_update);
                    sql_insert_specific_objective.Parameters.AddWithValue("@formula", DropDownFormula.SelectedValue);
                    sql_insert_specific_objective.Parameters.AddWithValue("@data_status", "exist");

                    sql_insert_specific_objective.ExecuteNonQuery();//insert Specific Objective

                    //untuk meng-update Header-nya
                    sum_individual_rating = float.Parse(sql_sum_individual_rating.ExecuteScalar().ToString());
                    count_individual_rating_data_by_header = float.Parse(sql_count_individual_rating.ExecuteScalar().ToString());
                    total_header_score = sum_individual_rating / count_individual_rating_data_by_header;

                    sql_update_header.Parameters.AddWithValue("@total_header_score", total_header_score);//untuk mengupdate header

                    sql_update_header.ExecuteNonQuery();//update Header

                    
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('New Specific Objective Added!'); window.location='" + baseUrl + "individual_scorecard.aspx?page="+page+"&id=" + period_id + "';", true);
                }
                conn.Close();
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