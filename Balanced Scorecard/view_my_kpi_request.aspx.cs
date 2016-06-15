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
    public partial class view_my_kpi_request : System.Web.UI.Page
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
                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page

                StringBuilder HtmlCurrentKPI = new StringBuilder();
                StringBuilder HtmlNewKPI = new StringBuilder();
                StringBuilder Pagination = new StringBuilder();//untuk Pagination

                var page = Request.QueryString["page"];//untuk pagination
                var request_id = Request.QueryString["request_id"];
                var period_id = Request.QueryString["period_id"];

                breadcrumb_change_request_kpi.Attributes.Add("href", "request_kpi.aspx?page=" + page + "&period_id=" + period_id + "");
                hrefBackToKPIChangeRequest.Attributes.Add("href", "request_kpi.aspx?page=" + page + "&period_id=" + period_id + "");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();

                    string delete_bool = "", reason = "";
                    string string_get_current_kpi = "SELECT * FROM IndividualHeaderHistory WHERE IndividualHeaderRequest_ID=" + request_id + "";
                    string string_get_new_kpi = "SELECT * FROM IndividualHeader_RequestChange WHERE IndividualHeaderRequest_ID=" + request_id + "";
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan UserGroup
                                                   + "WHERE Access_Rights_Code NOT IN "
                                                   + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                   + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                    SqlCommand sql_get_current_kpi = new SqlCommand(string_get_current_kpi, conn);
                    SqlCommand sql_get_new_kpi = new SqlCommand(string_get_new_kpi, conn);
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

                    using (SqlDataReader DeleteBoolReader = sql_get_new_kpi.ExecuteReader())
                    {
                        while (DeleteBoolReader.Read())
                        {
                            delete_bool = DeleteBoolReader["delete_flag"].ToString();
                            reason = DeleteBoolReader["IndividualHeaderReason"].ToString();
                        }
                        DeleteBoolReader.Dispose();
                        DeleteBoolReader.Close();
                    }

                    if (delete_bool == "0")
                    {
                        HtmlCurrentKPI.Append("<div class='panel panel-primary' id='Div1'>");
                        HtmlCurrentKPI.Append("<div class='panel-heading'><p class='csc_p'>Before Change</p></div>");
                        HtmlCurrentKPI.Append("<div class='table-responsive' id='BeforeTable'>");
                        HtmlCurrentKPI.Append("<table class='table table-bordered'>");

                        using (SqlDataReader CurrentReader = sql_get_current_kpi.ExecuteReader())
                        {
                            if (CurrentReader.HasRows)
                            {
                                HtmlCurrentKPI.Append("<tr>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>KPI</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Objective</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Target</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Result</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Formula</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Rating</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Weight</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Score</th>");
                                HtmlCurrentKPI.Append("</tr>");
                                while (CurrentReader.Read())
                                {
                                    if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                    {
                                        string month_name_target, month_name_result;
                                        month_name_target = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Target"].ToString()));
                                        month_name_result = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Result"].ToString()));

                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + month_name_target + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + month_name_result + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");
                                    }
                                    else if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                                    {
                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Target"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Result"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");
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

                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + based_on_schedule + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + string_result + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");

                                    }
                                }//end of While
                            }//end of if(HasRows)
                            else//jika tidak ada data period sama sekali
                            {
                                HtmlCurrentKPI.Append("<tr align='center'>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period' colspan='9'>There is no data to display</th>");
                                HtmlCurrentKPI.Append("</tr>");
                            }
                            HtmlCurrentKPI.Append("</table>");
                            HtmlCurrentKPI.Append("</div>");
                            HtmlCurrentKPI.Append("</div>");
                            CurrentReader.Dispose();
                            CurrentReader.Close();
                        }//end of SqlDataReader CurrentReader
                        PlaceHolderBefore.Controls.Add(new Literal { Text = HtmlCurrentKPI.ToString() });//menampilkan table period

                        HtmlNewKPI.Append("<div class='panel panel-primary' id='Div2'>");
                        HtmlNewKPI.Append("<div class='panel-heading'><p class='csc_p'>After Change</p></div>");
                        HtmlNewKPI.Append("<div class='table-responsive' id='AfterTable'>");
                        HtmlNewKPI.Append("<table class='table table-bordered'>");
                        using (SqlDataReader NewReader = sql_get_new_kpi.ExecuteReader())
                        {
                            if (NewReader.HasRows)
                            {
                                HtmlNewKPI.Append("<tr>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>KPI</th>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>Objective</th>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>Target</th>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>Result</th>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>Formula</th>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>Rating</th>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>Weight</th>");
                                HtmlNewKPI.Append("<th class='centering-th-period'>Score</th>");
                                HtmlNewKPI.Append("</tr>");
                                while (NewReader.Read())
                                {
                                    LabelReason.Text = NewReader["IndividualHeaderReason"].ToString();
                                    if (NewReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                    {
                                        string month_name_target, month_name_result;
                                        month_name_target = ShowMonthName(int.Parse(NewReader["IndividualHeader_Target"].ToString()));
                                        month_name_result = ShowMonthName(int.Parse(NewReader["IndividualHeader_Result"].ToString()));

                                        HtmlNewKPI.Append("<tr>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + month_name_target + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + month_name_result + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlNewKPI.Append("</tr>");
                                    }
                                    else if (NewReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                                    {
                                        HtmlNewKPI.Append("<tr>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Target"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Result"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlNewKPI.Append("</tr>");
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

                                        HtmlNewKPI.Append("<tr>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + based_on_schedule + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + string_result + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlNewKPI.Append("<td class='td-align' align='center'>" + NewReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlNewKPI.Append("</tr>");
                                    }
                                }//end of While
                            }//end of if(HasRows)
                            else//jika tidak ada data period sama sekali
                            {
                                HtmlNewKPI.Append("<tr align='center'>");
                                HtmlNewKPI.Append("<th class='centering-th-period' colspan='9'>There is no data to display</th>");
                                HtmlNewKPI.Append("</tr>");
                            }
                            HtmlNewKPI.Append("</table>");
                            HtmlNewKPI.Append("</div>");
                            HtmlNewKPI.Append("</div>");
                            NewReader.Dispose();
                            NewReader.Close();
                        }//end of SqlDataReader NewReader

                        PlaceHolderAfter.Controls.Add(new Literal { Text = HtmlNewKPI.ToString() });//menampilkan table period
                    }
                    else if (delete_bool == "1")
                    {
                        LabelReason.Text = reason;
                        //placeholder delete KPI
                        HtmlCurrentKPI.Append("<div class='panel panel-primary' id='Div1'>");
                        HtmlCurrentKPI.Append("<div class='panel-heading'><p class='csc_p'>You want to delete:</p></div>");
                        HtmlCurrentKPI.Append("<div class='table-responsive' id='BeforeTable'>");
                        HtmlCurrentKPI.Append("<table class='table table-bordered'>");

                        using (SqlDataReader CurrentReader = sql_get_current_kpi.ExecuteReader())
                        {
                            if (CurrentReader.HasRows)
                            {
                                HtmlCurrentKPI.Append("<tr>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>KPI</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Objective</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Target</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Result</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Formula</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Rating</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Weight</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Score</th>");
                                HtmlCurrentKPI.Append("</tr>");
                                while (CurrentReader.Read())
                                {
                                    if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                    {
                                        string month_name_target, month_name_result;
                                        month_name_target = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Target"].ToString()));
                                        month_name_result = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Result"].ToString()));

                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + month_name_target + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + month_name_result + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");
                                    }
                                    else if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                                    {
                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Target"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Result"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");
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

                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + based_on_schedule + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + string_result + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");

                                    }
                                }//end of While
                            }//end of if(HasRows)
                            else//jika tidak ada data period sama sekali
                            {
                                HtmlCurrentKPI.Append("<tr align='center'>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period' colspan='9'>There is no data to display</th>");
                                HtmlCurrentKPI.Append("</tr>");
                            }
                            HtmlCurrentKPI.Append("</table>");
                            HtmlCurrentKPI.Append("</div>");
                            HtmlCurrentKPI.Append("</div>");
                            CurrentReader.Dispose();
                            CurrentReader.Close();
                        }//end of SqlDataReader CurrentReader
                        PlaceHolderBefore.Controls.Add(new Literal { Text = HtmlCurrentKPI.ToString() });//menampilkan table period
                    }
                    else if (delete_bool == "2")
                    {
                        LabelReason.Text = reason;
                        //placeholder delete KPI
                        HtmlCurrentKPI.Append("<div class='panel panel-primary' id='Div1'>");
                        HtmlCurrentKPI.Append("<div class='panel-heading'><p class='csc_p'>You want to add:</p></div>");
                        HtmlCurrentKPI.Append("<div class='table-responsive' id='BeforeTable'>");
                        HtmlCurrentKPI.Append("<table class='table table-bordered'>");

                        using (SqlDataReader CurrentReader = sql_get_current_kpi.ExecuteReader())
                        {
                            if (CurrentReader.HasRows)
                            {
                                HtmlCurrentKPI.Append("<tr>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>KPI</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Objective</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Target</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Result</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Formula</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Rating</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Weight</th>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period'>Score</th>");
                                HtmlCurrentKPI.Append("</tr>");
                                while (CurrentReader.Read())
                                {
                                    if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                                    {
                                        string month_name_target, month_name_result;
                                        month_name_target = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Target"].ToString()));
                                        month_name_result = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Result"].ToString()));

                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + month_name_target + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + month_name_result + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");
                                    }
                                    else if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Numbers")
                                    {
                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Target"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Result"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");
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

                                        HtmlCurrentKPI.Append("<tr>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_KPI"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Objective"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + based_on_schedule + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + string_result + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Formula"] + "</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Rating"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Weight"] + "%</td>");
                                        HtmlCurrentKPI.Append("<td class='td-align' align='center'>" + CurrentReader["IndividualHeader_Score"] + "%</td>");
                                        HtmlCurrentKPI.Append("</tr>");

                                    }
                                }//end of While
                            }//end of if(HasRows)
                            else//jika tidak ada data period sama sekali
                            {
                                HtmlCurrentKPI.Append("<tr align='center'>");
                                HtmlCurrentKPI.Append("<th class='centering-th-period' colspan='9'>There is no data to display</th>");
                                HtmlCurrentKPI.Append("</tr>");
                            }
                            HtmlCurrentKPI.Append("</table>");
                            HtmlCurrentKPI.Append("</div>");
                            HtmlCurrentKPI.Append("</div>");
                            CurrentReader.Dispose();
                            CurrentReader.Close();
                        }//end of SqlDataReader CurrentReader
                        PlaceHolderBefore.Controls.Add(new Literal { Text = HtmlCurrentKPI.ToString() });//menampilkan table period
                    }
                    conn.Close();
                }//end of SqlConnection
            }
        }
        
        public string ShowMonthName(int result_value)
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