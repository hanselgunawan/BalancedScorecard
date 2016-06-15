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
    public partial class view_so_history_detail : System.Web.UI.Page
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

                StringBuilder sb_current_detail = new StringBuilder();
                StringBuilder sb_new_detail = new StringBuilder();
                StringBuilder HtmlUserInfo = new StringBuilder();
                StringBuilder Pagination = new StringBuilder();//untuk Pagination

                var page = Request.QueryString["page"];//untuk pagination
                var request_id = Request.QueryString["request_id"];
                var period_id = Request.QueryString["period_id"];

                var req_nik = Request.QueryString["req_nik"];
                var req_name = Request.QueryString["req_name"];
                var req_org = Request.QueryString["req_org"];
                var req_adt_org = Request.QueryString["req_adt_org"];
                var app_nik = Request.QueryString["app_nik"];
                var app_name = Request.QueryString["app_name"];

                if (req_nik == null && req_name == null && req_org == null && req_adt_org == null && app_nik == null && app_name == null)
                {
                    breadcrumb_change_request_so.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "");
                    hrefBackToSOChangeRequest.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "");
                }
                else if (req_nik != null && req_name == null && req_org == null && req_adt_org == null && app_nik == null && app_name == null)
                {
                    breadcrumb_change_request_so.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_nik="+req_nik+"");
                    hrefBackToSOChangeRequest.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_nik=" + req_nik + "");
                }
                else if (req_nik == null && req_name != null && req_org == null && req_adt_org == null && app_nik == null && app_name == null)
                {
                    breadcrumb_change_request_so.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_name="+req_name+"");
                    hrefBackToSOChangeRequest.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_name=" + req_name + "");
                }
                else if (req_nik == null && req_name == null && req_org != null && req_adt_org == null && app_nik == null && app_name == null)
                {
                    breadcrumb_change_request_so.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_org="+req_org+"");
                    hrefBackToSOChangeRequest.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_org=" + req_org + "");
                }
                else if (req_nik == null && req_name == null && req_org == null && req_adt_org != null && app_nik == null && app_name == null)
                {
                    breadcrumb_change_request_so.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_adt_org=" + req_adt_org + "");
                    hrefBackToSOChangeRequest.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&req_adt_org=" + req_adt_org + "");
                }
                else if (req_nik == null && req_name == null && req_org == null && req_adt_org == null && app_nik != null && app_name == null)
                {
                    breadcrumb_change_request_so.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&app_nik=" + app_nik + "");
                    hrefBackToSOChangeRequest.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&app_nik=" + app_nik + "");
                }
                else if (req_nik == null && req_name == null && req_org == null && req_adt_org == null && app_nik == null && app_name != null)
                {
                    breadcrumb_change_request_so.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&app_name=" + app_name + "");
                    hrefBackToSOChangeRequest.Attributes.Add("href", "request_change_specific_objective_history.aspx?page=" + page + "&period_id=" + period_id + "&app_name=" + app_name + "");
                }

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();

                    string delete_bool = "", reason = "";
                    string string_get_current_so = "SELECT * FROM IndividualDetailHistory WHERE IndividualDetailRequest_ID=" + request_id + "";
                    string string_get_new_so = "SELECT * FROM IndividualDetail_RequestChange WHERE IndividualDetailRequest_ID=" + request_id + "";
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan UserGroup
                                                  + "WHERE Access_Rights_Code NOT IN "
                                                  + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                  + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                    SqlCommand sql_get_current_so = new SqlCommand(string_get_current_so, conn);
                    SqlCommand sql_get_new_so = new SqlCommand(string_get_new_so, conn);
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

                    using (SqlDataReader DeleteBoolReader = sql_get_new_so.ExecuteReader())
                    {
                        while (DeleteBoolReader.Read())
                        {
                            delete_bool = DeleteBoolReader["delete_flag"].ToString();
                            reason = DeleteBoolReader["IndividualDetailReason"].ToString();
                        }
                        DeleteBoolReader.Dispose();
                        DeleteBoolReader.Close();
                    }

                    if (delete_bool == "0")
                    {
                        //placeholder before change
                        sb_current_detail.Append("<div class='panel panel-primary' id='Div1'>");
                        sb_current_detail.Append("<div class='panel-heading'><p class='csc_p'>Before Change</p></div>");
                        sb_current_detail.Append("<div class='table-responsive' id='BeforeTable'>");
                        sb_current_detail.Append("<table class='table table-bordered'>");
                        using (SqlDataReader CurrentReader = sql_get_current_so.ExecuteReader())
                        {
                            while (CurrentReader.Read())
                            {
                                if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                {
                                    string month_name_target, month_name_result;
                                    month_name_target = ShowMonthName(int.Parse(CurrentReader["IndividualDetail_Target"].ToString()));
                                    month_name_result = ShowMonthName(int.Parse(CurrentReader["IndividualDetail_Result"].ToString()));
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_target + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_result + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                                {
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Target"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Result"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else
                                {
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Target"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Result"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }

                            }
                            CurrentReader.Dispose();
                            CurrentReader.Close();
                        }
                        sb_current_detail.Append("</table>");
                        sb_current_detail.Append("</div>");
                        sb_current_detail.Append("</div>");
                        PlaceHolderBefore.Controls.Add(new Literal { Text = sb_current_detail.ToString() });//menampilkan table period

                        //placeholder after change
                        sb_new_detail.Append("<div class='panel panel-primary' id='Div2'>");
                        sb_new_detail.Append("<div class='panel-heading'><p class='csc_p'>After Change</p></div>");
                        sb_new_detail.Append("<div class='table-responsive' id='AfterTable'>");
                        sb_new_detail.Append("<table class='table table-bordered'>");
                        using (SqlDataReader NewReader = sql_get_new_so.ExecuteReader())
                        {
                            while (NewReader.Read())
                            {
                                LabelReason.Text = reason;
                                if (NewReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                {
                                    string month_name_target, month_name_result;
                                    month_name_target = ShowMonthName(int.Parse(NewReader["IndividualDetail_Target"].ToString()));
                                    month_name_result = ShowMonthName(int.Parse(NewReader["IndividualDetail_Result"].ToString()));
                                    sb_new_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_target + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_result + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else if (NewReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                                {
                                    sb_new_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Target"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Result"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else
                                {
                                    sb_new_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Target"].ToString() + " " + NewReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Result"].ToString() + " " + NewReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + NewReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                            }
                            NewReader.Dispose();
                            NewReader.Close();
                        }
                        sb_new_detail.Append("</table>");
                        sb_new_detail.Append("</div>");
                        sb_new_detail.Append("</div>");
                        PlaceHolderAfter.Controls.Add(new Literal { Text = sb_new_detail.ToString() });//menampilkan table period
                    }
                    else if(delete_bool == "1")//jika meminta untuk Delete Spec. Obj
                    {
                        LabelReason.Text = reason;
                        //placeholder delete spec. objective
                        sb_current_detail.Append("<div class='panel panel-primary' id='Div1'>");
                        sb_current_detail.Append("<div class='panel-heading'><p class='csc_p'>Wants to delete:</p></div>");
                        sb_current_detail.Append("<div class='table-responsive' id='BeforeTable'>");
                        sb_current_detail.Append("<table class='table table-bordered'>");
                        using (SqlDataReader CurrentReader = sql_get_current_so.ExecuteReader())
                        {
                            while (CurrentReader.Read())
                            {
                                if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                {
                                    string month_name_target, month_name_result;
                                    month_name_target = ShowMonthName(int.Parse(CurrentReader["IndividualDetail_Target"].ToString()));
                                    month_name_result = ShowMonthName(int.Parse(CurrentReader["IndividualDetail_Result"].ToString()));
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_target + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_result + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                                {
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Target"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Result"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else
                                {
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Target"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Result"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }

                            }
                            CurrentReader.Dispose();
                            CurrentReader.Close();
                        }
                        sb_current_detail.Append("</table>");
                        sb_current_detail.Append("</div>");
                        sb_current_detail.Append("</div>");
                        PlaceHolderBefore.Controls.Add(new Literal { Text = sb_current_detail.ToString() });//menampilkan table period
                    }
                    else if (delete_bool == "2")//jika meminta untuk Add Spec. Obj
                    {
                        LabelReason.Text = reason;
                        //placeholder delete spec. objective
                        sb_current_detail.Append("<div class='panel panel-primary' id='Div1'>");
                        sb_current_detail.Append("<div class='panel-heading'><p class='csc_p'>Wants to add:</p></div>");
                        sb_current_detail.Append("<div class='table-responsive' id='BeforeTable'>");
                        sb_current_detail.Append("<table class='table table-bordered'>");
                        using (SqlDataReader CurrentReader = sql_get_current_so.ExecuteReader())
                        {
                            while (CurrentReader.Read())
                            {
                                if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Month")
                                {
                                    string month_name_target, month_name_result;
                                    month_name_target = ShowMonthName(int.Parse(CurrentReader["IndividualDetail_Target"].ToString()));
                                    month_name_result = ShowMonthName(int.Parse(CurrentReader["IndividualDetail_Result"].ToString()));
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_target + "</td>"
                                                    + "<td class='td-align' align='center'>" + month_name_result + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else if (CurrentReader["IndividualDetail_MeasureBy"].ToString() == "Numbers")
                                {
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Target"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Result"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }
                                else
                                {
                                    sb_current_detail.Append("<tr>"
                                                    + "<th class='centering-th-period'>Spec. Objective</th>"
                                                    + "<th class='centering-th-period'>Target</th>"
                                                    + "<th class='centering-th-period'>Result</th>"
                                                    + "<th class='centering-th-period'>Formula</th>"
                                                    + "<th class='centering-th-period'>Rating</th>"
                                                    + "</tr>"
                                                    + "<tr>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Title"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Target"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Result"].ToString() + " " + CurrentReader["IndividualDetail_MeasureBy"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Formula"].ToString() + "</td>"
                                                    + "<td class='td-align' align='center'>" + CurrentReader["IndividualDetail_Rating"].ToString() + "%</td>"
                                                    + "</tr>");
                                }

                            }
                            CurrentReader.Dispose();
                            CurrentReader.Close();
                        }
                        sb_current_detail.Append("</table>");
                        sb_current_detail.Append("</div>");
                        sb_current_detail.Append("</div>");
                        PlaceHolderBefore.Controls.Add(new Literal { Text = sb_current_detail.ToString() });//menampilkan table period
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