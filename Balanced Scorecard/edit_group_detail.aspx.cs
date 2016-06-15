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
using log4net;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class edit_group_detail : System.Web.UI.Page
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(edit_group_detail));
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        static void LogString(string stringToLog)
        {
            logger.Info(stringToLog);
        }

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
                var detail_id = Request.QueryString["detail_id"];
                financial_measure_breadcrumb.Attributes.Add("a href", "financial_scorecard.aspx?page="+page+"&id=" + period_id + "");
                cancel_edit.Attributes.Add("href", "financial_scorecard.aspx?page="+page+"&id=" + period_id + "");
                //Add Item to DropDown Financial Type
                DropDownFinancialType.Items.Add("Single");
                DropDownFinancialType.Items.Add("Share");

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

                //Add Item to DropDown Measurement
                DropDownMeasurement.Items.Add("%");
                //DropDownMeasurement.Items.Add("Days");
                DropDownMeasurement.Items.Add("Month");
                DropDownMeasurement.Items.Add("Million");
                DropDownMeasurement.Items.Add("Numbers");

                //Add Item to DropDown Formula. Sudah FIX
                DropDownFormula.Items.Add("(Result/Target) x 100%");
                DropDownFormula.Items.Add("100% - ((Result - Target)/Target)");
                DropDownFormula.Enabled = false;

                //disable Target, Rating, Score, Measured By, dan Formula
                TextBoxRating.Attributes.Add("disabled", "true");
                TextBoxScore.Attributes.Add("disabled", "true");

                //For Database
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    //untuk ambil value Header-nya!
                    string select_header = "SELECT * FROM FinancialMeasures_Header WHERE FinancialHeader_ID=" + header_id + "";
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                                          + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                                          + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                          + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                    SqlCommand sql_select_header = new SqlCommand(select_header,conn);
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

                    using (SqlDataReader HeaderReader = sql_select_header.ExecuteReader())
                    {
                        if (HeaderReader.HasRows)
                        {
                            while (HeaderReader.Read())
                            {
                                string start_date_formatted, end_date_formatted, string_select_period;
                                float stretch_rat;
                                string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + HeaderReader["Period_ID"] + "";
                                SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);

                                stretch_rat = float.Parse(HeaderReader["FinancialHeader_StretchRating"].ToString());//type cast
                                LabelGroup.InnerText = HeaderReader["FinancialHeader_Group"].ToString();
                                LabelStretch.InnerText = stretch_rat.ToString() + "%";
                                LabelReview.InnerText = HeaderReader["FinancialHeader_Review"].ToString();
                                LabelBreadcrumb.Text = HeaderReader["FinancialHeader_Group"].ToString();
                                LabelTitle.Text = HeaderReader["FinancialHeader_Group"].ToString();
                                TextBoxRating.Attributes.Add("max",stretch_rat.ToString());//untuk set nilai max pada Rating

                                using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                                {
                                    while (PeriodReader.Read())//untuk ambil DATE dari Tabel BSC_Period
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
                            SpanEditDetail.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                        }
                    }

                    //untuk ambil value Detail-nya!
                    string select_detail = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialDetail_ID="+detail_id+" AND FinancialHeader_ID="+header_id+"";
                    SqlCommand sql_select_detail = new SqlCommand(select_detail,conn);
                    using (SqlDataReader DetailReader = sql_select_detail.ExecuteReader())
                    {
                        if (DetailReader.HasRows)
                        {
                            while (DetailReader.Read())//untuk Read Detail
                            {
                                string finance_type = DetailReader["FinancialType"].ToString();
                                if (finance_type == "Share")
                                {
                                    //TextBoxWeight.Attributes.Add("disabled", "true");
                                    LabelWeight.Visible = true;
                                    DropDownFinancialType.Enabled = false;
                                }
                                TextBoxFinancialMeasure.Text = DetailReader["FinancialMeasure"].ToString();
                                DropDownFinancialType.SelectedValue = DetailReader["FinancialType"].ToString();
                                TextBoxTarget.Text = DetailReader["FinancialTarget"].ToString();
                                TextBoxResult.Text = DetailReader["FinancialResult"].ToString();
                                DropDownMeasurement.SelectedValue = DetailReader["FinancialMeasureBy"].ToString();
                                if (DetailReader["FinancialMeasureBy"].ToString() == "Month")
                                {
                                    DropDownFormula.Enabled = false;
                                }
                                else
                                {
                                    DropDownFormula.Enabled = true;
                                }
                                DropDownFormula.SelectedValue = DetailReader["FinancialFormula"].ToString();
                                TextBoxRating.Value = DetailReader["FinancialRating"].ToString();
                                TextBoxWeight.Value = DetailReader["FinancialWeight"].ToString();
                                TextBoxScore.Value = DetailReader["FinancialScore"].ToString();
                                TextareaRemarks.InnerText = DetailReader["FinancialRemarks"].ToString();

                                if (DetailReader["FinancialMeasureBy"].ToString() == "Month")
                                {
                                    TextBoxResult.Attributes.Add("max", "12");
                                    TextBoxResult.Attributes.Add("step", "1");
                                    int target_value = int.Parse(TextBoxTarget.Text);
                                    string target_month_name;
                                    target_month_name = ShowMonthNameTarget(target_value);
                                    month_name_target.InnerText = target_month_name;
                                    //tampilakan Month Name
                                    month_name_target.Attributes.Add("style", "color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
                                    if (DetailReader["FinancialResult"].ToString() == "0")
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
                            TextBoxFinancialMeasure.Text = "Financial Measure Not Found";
                            TextBoxTarget.Text = "0";
                            TextBoxResult.Text = "0";
                            TextBoxRating.Value = "0";
                            TextBoxWeight.Value = "0";
                            TextBoxScore.Value = "0";
                            TextareaRemarks.InnerText = "No Remarks Found";
                            SpanEditDetail.Attributes.Add("class", "btn btn-add-group btn-add-group-container add-button disabled");
                        }
                    }
                    conn.Close();
                }
            }
        }

        protected void OnFinancialMeasureChanged(object sender, EventArgs e)
        {
            var header_id = Request.QueryString["header_id"];
            var detail_id = Request.QueryString["detail_id"];
            string check_if_measure_exist = "SELECT FinancialMeasure FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + header_id + " AND FinancialDetail_ID<>" + detail_id + "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_check_measure = new SqlCommand(check_if_measure_exist, conn);
                using (SqlDataReader MeasureReader = sql_check_measure.ExecuteReader())//cek apakah namanya sudah ada atau belum!
                {
                    while (MeasureReader.Read())
                    {
                        if (MeasureReader["FinancialMeasure"].ToString() == TextBoxFinancialMeasure.Text)
                        {
                            check_financial_measure.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
                            SpanEditDetail.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button disabled");
                            break;
                        }
                        else
                        {
                            check_financial_measure.Attributes.Add("style", "visibility:hidden; margin-bottom:-15px !important; margin-top:0px !important");
                            SpanEditDetail.Attributes.Add("class", "btn btn-add-group btn-add-group-container edit-button");
                        }
                    }
                }
                conn.Close();
            }
        }

        protected void OnResultChanged(object sender, EventArgs e)
        {
            if (DropDownMeasurement.SelectedValue == "Month")
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

       protected void OnClickEditDetail(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            try
            {
				var page = Request.QueryString["page"];
				float stretch_rat;
				var header_id = Request.QueryString["header_id"];
				var period_id = Request.QueryString["period_id"];
				var detail_id = Request.QueryString["detail_id"];
				bool value_exist = false;
				using (SqlConnection conn = new SqlConnection(str_connect))
				{
					string get_stretch_rating = "SELECT FinancialHeader_StretchRating FROM FinancialMeasures_Header WHERE FinancialHeader_ID=" + header_id + "";
					string check_if_measure_exist = "SELECT FinancialMeasure FROM FinancialMeasures_Detail WHERE FinancialHeader_ID=" + header_id + " AND FinancialDetail_ID<>" + detail_id + "";//<> --> NOT EQUAL
					SqlCommand sql_check_measure = new SqlCommand(check_if_measure_exist, conn);
					SqlCommand sql_get_stretch = new SqlCommand(get_stretch_rating, conn);
					conn.Open();
					stretch_rat = float.Parse(sql_get_stretch.ExecuteScalar().ToString());
					using (SqlDataReader MeasureReader = sql_check_measure.ExecuteReader())//cek apakah namanya sudah ada atau belum!
					{
						while (MeasureReader.Read())
						{
							if (MeasureReader["FinancialMeasure"].ToString() == TextBoxFinancialMeasure.Text)
							{
								check_financial_measure.Attributes.Add("style", "visibility:visible; margin-bottom:0px !important; margin-top:5px !important; color:red; font-weight:bold");
								value_exist = true;
								break;
							}
							else
							{
								check_financial_measure.Attributes.Add("style", "visibility:hidden; margin-bottom:-15px !important; margin-top:0px !important");
								value_exist = false;
							}
						}
					}
					conn.Close();
				}

				if (value_exist == false)
				{
					using (SqlConnection conn = new SqlConnection(str_connect))
					{
						float max_stretch_rating;
						float total_rating = 0;
						float result = float.Parse(TextBoxResult.Text);
						float target = float.Parse(TextBoxTarget.Text);
						string select_stretch_rating = "SELECT FinancialHeader_StretchRating FROM FinancialMeasures_Header WHERE FinancialHeader_ID="+header_id+"";
						string select_detail = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialDetail_ID="+detail_id+"";//buat ambil FinancialLinked dari detail ID tersebut
																															  //untuk lihat apakah Linked-nya 0 atau TIDAK
						string string_update_detail = "UPDATE FinancialMeasures_Detail SET FinancialMeasure=@financial_measure,FinancialType=@financial_type,"
													+ "FinancialTarget=@financial_target,FinancialResult=@financial_result, "
													+ "FinancialMeasureBy=@financial_measure_by, FinancialRating=@financial_rating, "
													+ "FinancialWeight=@financial_weight, FinancialScore=@financial_score, "
													+ "FinancialFormula=@financial_formula, FinancialRemarks=@financial_remarks, " 
													+ "user_update=@user_update, date_update=@date_update WHERE "
													+ "FinancialHeader_ID='"+header_id+"' AND FinancialDetail_ID='"+detail_id+"'";
						string user_update, date_update;
						SqlCommand sql_select_stretch_rating = new SqlCommand(select_stretch_rating, conn);
						SqlCommand sql_update_detail = new SqlCommand(string_update_detail, conn);
						SqlCommand sql_select_detail = new SqlCommand(select_detail, conn);
						conn.Open();

						max_stretch_rating =float.Parse(sql_select_stretch_rating.ExecuteScalar().ToString());
						user_update = Session["user_name"].ToString();
						date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

						if (DropDownFormula.SelectedValue == "(Result/Target) x 100%")//untuk perhitungan RATING
						{
							if(result==0 && target==0)
							{
								total_rating = 0;//error handling jika ada hasil yang UNLIMITED
							}
							else
							{
								total_rating = (result / target)*100;
							}
						}
                        else if (DropDownFormula.SelectedValue == "100% - ((Result - Target)/Target)")
						{
							if (result == 0 && target == 0)
							{
								total_rating = 0;
							}
                            else if (result == 0)
                            {
                                total_rating = 0;
                            }
							else
							{
								total_rating = 100 - (((result - target) / target)*100);
							}
						}

						if (total_rating > max_stretch_rating)//jika lebih dari Stretch Rating, Ratingnya menjadi sebesar Stretch Rating tersebut
						{
							total_rating = max_stretch_rating;
						}

                        sql_update_detail.Parameters.AddWithValue("@financial_measure", TextBoxFinancialMeasure.Text);          //varchar
                        sql_update_detail.Parameters.AddWithValue("@financial_type", DropDownFinancialType.SelectedItem.Value); //varchar
                        sql_update_detail.Parameters.AddWithValue("@financial_target", Math.Round(Convert.ToDouble(TextBoxTarget.Text),2));     //float
                        sql_update_detail.Parameters.AddWithValue("@financial_result", Math.Round(Convert.ToDouble(TextBoxResult.Text),2));     //float
                        sql_update_detail.Parameters.AddWithValue("@financial_measure_by", DropDownMeasurement.SelectedItem.Value); //varchar
                        sql_update_detail.Parameters.AddWithValue("@financial_rating", Math.Round(total_rating,2));//agar hanya 2 desimal di belakang koma    //float
						sql_update_detail.Parameters.AddWithValue("@financial_weight", Math.Round(Convert.ToDouble(TextBoxWeight.Value),2));        //float
						sql_update_detail.Parameters.AddWithValue("@financial_score", Math.Round(Convert.ToDouble(TextBoxScore.Value),2));      //float
						sql_update_detail.Parameters.AddWithValue("@financial_formula", DropDownFormula.SelectedItem.Value);    //varchar
						sql_update_detail.Parameters.AddWithValue("@financial_remarks", TextareaRemarks.InnerText);     //varchar
						sql_update_detail.Parameters.AddWithValue("@user_update", user_update);     
						sql_update_detail.Parameters.AddWithValue("@date_update", date_update);
						sql_update_detail.ExecuteNonQuery();

						using (SqlDataReader DetailReader = sql_select_detail.ExecuteReader())
						{
							while (DetailReader.Read())
							{
								if ((int)DetailReader["FinancialLinked"] == 0)//untuk update score yang SINGLE atau TIDAK BERELASI
								{
									string update_detail_score = "UPDATE FinancialMeasures_Detail SET "
																+"FinancialScore=ROUND(FinancialRating*(0.01*FinancialWeight),2) "
																+"WHERE FinancialDetail_ID=" + detail_id + "";
									SqlCommand sql_update_detail_score = new SqlCommand(update_detail_score, conn);
									sql_update_detail_score.ExecuteNonQuery();
								}
								else
								{
									string update_all_weight = "UPDATE FinancialMeasures_Detail SET FinancialWeight=" + TextBoxWeight.Value + " WHERE "
															 + "FinancialLinked=" + DetailReader["FinancialLinked"] + "";
									string update_share_type_detail = "UPDATE FinancialMeasures_Detail "
																	+ "SET FinancialScore=ROUND((SELECT SUM(FinancialRating) "
																	+ "FROM FinancialMeasures_Detail WHERE FinancialLinked="+DetailReader["FinancialLinked"]+")/(SELECT COUNT(FinancialLinked) "
																	+ "FROM FinancialMeasures_Detail WHERE FinancialLinked="+DetailReader["FinancialLinked"]+")*(0.01*(SELECT TOP(1) FinancialWeight FROM FinancialMeasures_Detail WHERE FinancialLinked="+DetailReader["FinancialLinked"]+")),2) "
																	+ "WHERE FinancialLinked="+DetailReader["FinancialLinked"]+"";
									SqlCommand sql_update_share_type_detail = new SqlCommand(update_share_type_detail, conn);
									SqlCommand sql_update_all_weight = new SqlCommand(update_all_weight, conn);
									sql_update_all_weight.ExecuteNonQuery();
									sql_update_share_type_detail.ExecuteNonQuery();
								}
							}
						}
                        
						ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('This Measure Has Been Updated!'); window.location='" + baseUrl + "financial_scorecard.aspx?page="+page+"&id=" + period_id + "';", true);
                        conn.Close();
                    }
                }                        
                }
				catch (Exception ex)
				{
                    LogString(ex.ToString());
				}
				finally
				{
				}
		}

       protected void OnSelectMeasureBy(object sender, EventArgs e)
       {
           if (DropDownMeasurement.SelectedValue == "Month")
           {
               int target_value = int.Parse(TextBoxTarget.Text);
               int result_value = int.Parse(TextBoxResult.Text);
               string month_name;
               month_name_target.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
               month_name_result.Attributes.Add("style", "width:300px; color:black; visibility:visible; padding-bottom:20px; margin-top:3px");
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

               if (result_value > 12)
               {
                   TextBoxResult.Text = "12";
                   month_name_result.InnerText = "December";
               }
               else if (result_value < 0)
               {
                   TextBoxResult.Text = "0";
                   month_name_result.InnerText = "January";
               }
               else
               {
                   month_name = ShowMonthNameResult(result_value);
                   month_name_result.InnerText = month_name;
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
               month_name_result.Attributes.Add("style", "width:300px; visibility:hidden; padding-bottom:0px; margin-top:0px");
               TextBoxTarget.Attributes.Remove("max");
               TextBoxTarget.Attributes.Add("min", "0");
               TextBoxTarget.Attributes.Add("step", "0.01");
               DropDownFormula.SelectedValue = "(Result/Target) x 100%";
               DropDownFormula.Enabled = true;
               TextBoxResult.Attributes.Remove("max");
               if (DropDownMeasurement.SelectedValue == "Numbers")
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
           if (DropDownMeasurement.SelectedValue == "Month")
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

        protected void OnSelectType(object sender, EventArgs e)
        {
            var header_id = Request.QueryString["header_id"];
            var detail_id = Request.QueryString["detail_id"];
            string select_detail = "SELECT * FROM FinancialMeasures_Detail WHERE FinancialDetail_ID=" + detail_id + "";
            SqlConnection conn = new SqlConnection(str_connect);
            conn.Open();
            SqlCommand sql_select_detail = new SqlCommand(select_detail, conn);
            using (SqlDataReader DetailReader = sql_select_detail.ExecuteReader())
            {
                while (DetailReader.Read())
                {
                    if (DetailReader["FinancialType"].ToString() == "Single")
                    {
                        if (DropDownFinancialType.SelectedValue == "Share")
                        {
                            //kalau Type='Share', Weight-nya ga bisa diisi dulu!
                            TextBoxWeight.Attributes.Add("disabled", "true");
                            TextBoxWeight.Value = "0";
                            TextBoxScore.Value = "0";
                        }
                        else if (DropDownFinancialType.SelectedValue == "Single")
                        {
                            TextBoxWeight.Attributes.Remove("disabled");
                            TextBoxWeight.Value = DetailReader["FinancialWeight"].ToString();
                        }
                    }
                    else
                    {
                        if (DropDownFinancialType.SelectedValue == "Share")
                        {
                            //kalau Type='Share', Weight-nya ga bisa diisi dulu!
                            TextBoxWeight.Attributes.Add("disabled", "true");
                            TextBoxWeight.Value = DetailReader["FinancialWeight"].ToString();
                        }
                        else if (DropDownFinancialType.SelectedValue == "Single")
                        {
                            TextBoxWeight.Attributes.Remove("disabled");
                            TextBoxWeight.Value = "0";
                            TextBoxScore.Value = "0";
                        }
                    }
                }//<-- end of DetailReader.Read()
            }// <-- end of using (SqlDataReader DetailReader)
            conn.Close();
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