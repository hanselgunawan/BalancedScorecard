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
using System.Net.Mail;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class approve_kpi : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];//untuk pagination
            var request_id = Request.QueryString["request_id"];
            var period_id = Request.QueryString["period_id"];
            string user_update, date_update;
            int header_id = 0;
            if (Session["user_name"] == null)
            {
                Response.Redirect(baseUrl + "index.aspx");
            }
            ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();
            user_update = Session["user_name"].ToString();
            date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            string string_update_old_kpi = "";
            string string_get_new_kpi = "SELECT * FROM IndividualHeader_RequestChange WHERE IndividualHeaderRequest_ID=" + request_id + "";
            string string_update_status_request = "UPDATE IndividualHeader_RequestChange SET Approval_Status='approved', "
                                                + "user_update='"+user_update+"', date_update='" + date_update + "' "
                                                + "WHERE IndividualHeaderRequest_ID=" + request_id + "";
            string string_get_header_id = "SELECT IndividualHeader_ID FROM IndividualHeader_RequestChange "
                                                + "WHERE IndividualHeaderRequest_ID=" + request_id + "";
            string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_get_new_kpi = new SqlCommand(string_get_new_kpi, conn);
                SqlCommand sql_update_request_status = new SqlCommand(string_update_status_request, conn);
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
                    AccessReader.Close();
                    AccessReader.Dispose();
                }


                using (SqlDataReader NewReader = sql_get_new_kpi.ExecuteReader())
                {
                    while (NewReader.Read())
                    {
                        if (NewReader["delete_flag"].ToString() == "0")
                        {
                            string_update_old_kpi = "UPDATE IndividualMeasures_Header SET IndividualHeader_KPI=@kpi, IndividualHeader_Target=@target, "
                                                    + "IndividualHeader_Result=@result, "
                                                    + "IndividualHeader_Rating=@rating, IndividualHeader_Weight=@weight, "
                                                    + "IndividualHeader_Score=@score, user_update=@user_update, "
                                                    + "IndividualHeader_Objective=@objective, IndividualHeader_Formula=@formula, "
                                                    + "IndividualHeader_MeasureBy=@measure_by, date_update=@date_update "
                                                    + "WHERE IndividualHeader_ID=@header_id";
                            SqlCommand sql_get_header_id = new SqlCommand(string_get_header_id, conn);
                            header_id = int.Parse(sql_get_header_id.ExecuteScalar().ToString());
                        }
                        else if (NewReader["delete_flag"].ToString() == "1")
                        {
                            string_update_old_kpi = "UPDATE IndividualMeasures_Header SET IndividualHeader_KPI=@kpi, "
                                                   + "IndividualHeader_Target=@target, IndividualHeader_Result=@result, "
                                                   + "IndividualHeader_Rating=@rating, IndividualHeader_Weight=@weight, "
                                                   + "IndividualHeader_Score=@score, user_update=@user_update, "
                                                   + "IndividualHeader_Objective=@objective, IndividualHeader_Formula=@formula, "
                                                   + "IndividualHeader_MeasureBy=@measure_by, date_update=@date_update, "
                                                   + "data_status='deleted' WHERE IndividualHeader_ID=@header_id";
                            SqlCommand sql_get_header_id = new SqlCommand(string_get_header_id, conn);
                            header_id = int.Parse(sql_get_header_id.ExecuteScalar().ToString());
                        }
                        else if (NewReader["delete_flag"].ToString() == "2")
                        {
                            string_update_old_kpi = "INSERT INTO IndividualMeasures_Header VALUES(@KPI, @target, @result, @rating, @weight, @score, @user_create, "
                                                  + "@user_update, @objective, @period_id, @formula, @measure_by, @data_status, @user_id, @date_create, @date_update)";
                        }

                        SqlCommand sql_update_old_kpi = new SqlCommand(string_update_old_kpi, conn);
                        if (NewReader["delete_flag"].ToString() == "0" || NewReader["delete_flag"].ToString() == "1")
                        {
                            sql_update_old_kpi.Parameters.AddWithValue("@kpi", NewReader["IndividualHeader_KPI"]);
                            sql_update_old_kpi.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Target"].ToString()),2));
                            sql_update_old_kpi.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Result"].ToString()), 2));
                            sql_update_old_kpi.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Rating"].ToString()), 2));
                            sql_update_old_kpi.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Weight"].ToString()), 2));
                            sql_update_old_kpi.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Score"].ToString()), 2));
                            sql_update_old_kpi.Parameters.AddWithValue("@user_update", user_update);
                            sql_update_old_kpi.Parameters.AddWithValue("@objective", NewReader["IndividualHeader_Objective"]);
                            sql_update_old_kpi.Parameters.AddWithValue("@formula", NewReader["IndividualHeader_Formula"]);
                            sql_update_old_kpi.Parameters.AddWithValue("@measure_by", NewReader["IndividualHeader_MeasureBy"]);
                            sql_update_old_kpi.Parameters.AddWithValue("@date_update", date_update);
                            sql_update_old_kpi.Parameters.AddWithValue("@header_id", header_id);
                            sendMailApprove(NewReader["delete_flag"].ToString());
                            sql_update_old_kpi.ExecuteNonQuery();
                        }
                        else
                        {
                            string string_get_kpi = "SELECT IndividualHeader_KPI FROM IndividualMeasures_Header "
                                                  + "WHERE user_id=" + NewReader["user_id"].ToString() + " AND "
                                                  + "Period_ID=" + period_id + " AND "
                                                  + "IndividualHeader_KPI='" + NewReader["IndividualHeader_KPI"].ToString() + "'";
                            SqlCommand sql_get_kpi = new SqlCommand(string_get_kpi, conn);
                            using (SqlDataReader KPIReader = sql_get_kpi.ExecuteReader())
                            {
                                if (!KPIReader.HasRows)//jika sudah ada KPI yang ingin di ADD, ga bisa ADD lagi
                                {
                                    sql_update_old_kpi.Parameters.AddWithValue("@kpi", NewReader["IndividualHeader_KPI"]);
                                    sql_update_old_kpi.Parameters.AddWithValue("@target", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Target"].ToString()), 2));
                                    sql_update_old_kpi.Parameters.AddWithValue("@result", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Result"].ToString()), 2));
                                    sql_update_old_kpi.Parameters.AddWithValue("@rating", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Rating"].ToString()), 2));
                                    sql_update_old_kpi.Parameters.AddWithValue("@weight", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Weight"].ToString()), 2));
                                    sql_update_old_kpi.Parameters.AddWithValue("@score", Math.Round(Convert.ToDouble(NewReader["IndividualHeader_Score"].ToString()), 2));
                                    sql_update_old_kpi.Parameters.AddWithValue("@user_create", user_update);//user create = user update (atasan)
                                    sql_update_old_kpi.Parameters.AddWithValue("@user_update", user_update);
                                    sql_update_old_kpi.Parameters.AddWithValue("@objective", NewReader["IndividualHeader_Objective"]);
                                    sql_update_old_kpi.Parameters.AddWithValue("@period_id", period_id);
                                    sql_update_old_kpi.Parameters.AddWithValue("@formula", NewReader["IndividualHeader_Formula"]);
                                    sql_update_old_kpi.Parameters.AddWithValue("@measure_by", NewReader["IndividualHeader_MeasureBy"]);
                                    sql_update_old_kpi.Parameters.AddWithValue("@data_status", "exist");
                                    sql_update_old_kpi.Parameters.AddWithValue("@user_id", NewReader["user_id"]);
                                    sql_update_old_kpi.Parameters.AddWithValue("@date_create", date_update);
                                    sql_update_old_kpi.Parameters.AddWithValue("@date_update", date_update);
                                    sendMailApprove(NewReader["delete_flag"].ToString());
                                    sql_update_old_kpi.ExecuteNonQuery();
                                }
                                KPIReader.Close();
                                KPIReader.Dispose();
                            }
                        }
                    }
                    NewReader.Dispose();
                    NewReader.Close();
                }

                sql_update_request_status.ExecuteNonQuery();

                
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Request has been approved'); window.location='" + baseUrl + "approval.aspx?page=" + page + "&period_id=" + period_id + "';", true);
                conn.Close();
            }
        }

        public void sendMailApprove(string delete_flag)
        {
            var request_id = Request.QueryString["request_id"];
            var period_id = Request.QueryString["period_id"];
            string sender_email = "", sender_name = "";

            string string_get_user_info = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, OrgName, JobTtlName, LOWER(ScorecardUser.empEmail) as Email, "
                                        + "empGrade, OrgAdtGroupName, Group_Name, FinancialHeader_IndividualStretchRating, empStatus, EmpSex, EmpMaritalSt "
                                        + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                        + "join IndividualHeader_RequestChange ON IndividualHeader_RequestChange.user_id = ScorecardUser.user_id "
                                        + "join IndividualHeaderHistory ON IndividualHeaderHistory.IndividualHeaderRequest_ID= IndividualHeader_RequestChange.IndividualHeaderRequest_ID "
                                        + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                        + "join [Human_Capital_demo].dbo.Employee on ScorecardUser.EmpId=Employee.EmpId "
                                        + "join [Human_Capital_demo].dbo.Organization ON ScorecardUser.empOrgCode = Organization.OrgCode "
                                        + "join [Human_Capital_demo].dbo.JobTitle ON ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                        + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                        + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                        + "join FinancialMeasures_Header ON FinancialMeasures_Header.FinancialHeader_Group = ScorecardGroupLink.Group_Name "
                                        + "AND FinancialMeasures_Header.Period_ID=" + period_id + " AND FinancialMeasures_Header.data_status='exist' "
                                        + "WHERE IndividualHeaderHistory.Period_ID=" + period_id + " AND IndividualHeaderHistory.IndividualHeaderRequest_ID=" + request_id + "";
            string string_get_sender_info = "SELECT ScorecardUser.empEmail, ScorecardUser.empName, EmpSex, EmpMaritalSt FROM ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.Employee on ScorecardUser.EmpId=Employee.EmpId "
                                          + "WHERE ScorecardUser.EmpId='" + Session["user_nik"].ToString() + "'";
            string string_get_current_kpi = "SELECT * FROM IndividualHeaderHistory WHERE IndividualHeaderRequest_ID=" + request_id + "";
            string string_get_new_kpi = "SELECT * FROM IndividualHeader_RequestChange WHERE IndividualHeaderRequest_ID=" + request_id + "";
            string user_title = "", sender_title = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_get_user_info = new SqlCommand(string_get_user_info, conn);
                SqlCommand sql_get_current_kpi = new SqlCommand(string_get_current_kpi, conn);
                SqlCommand sql_get_new_kpi = new SqlCommand(string_get_new_kpi, conn);
                SqlCommand sql_get_sender_info = new SqlCommand(string_get_sender_info, conn);
                using (SqlDataReader SenderReader = sql_get_sender_info.ExecuteReader())
                {
                    while (SenderReader.Read())
                    {
                        sender_email = SenderReader["empEmail"].ToString();
                        sender_name = SenderReader["empName"].ToString();
                        if (SenderReader["EmpSex"].ToString() == "F" && SenderReader["EmpMaritalSt"].ToString() == "NIKAH")
                        {
                            sender_title = "Ms.";
                        }
                        else if (SenderReader["EmpSex"].ToString() == "F" && SenderReader["EmpMaritalSt"].ToString() == "BELUM NIKAH")
                        {
                            sender_title = "Mrs.";
                        }
                        else if (SenderReader["EmpSex"].ToString() == "F" && DBNull.Value.Equals(SenderReader["EmpMaritalSt"]))
                        {
                            sender_title = "Ms.";
                        }
                        else if (SenderReader["EmpSex"].ToString() == "M")
                        {
                            sender_title = "Mr.";
                        }
                    }
                    SenderReader.Close();
                    SenderReader.Dispose();
                }

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

                        if (delete_flag == "0")//change
                        {
                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Change KPI has been Approved");
                            sb_body_introduction.Append("Dear, <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Name : " + user_title + " " + UserReader["empName"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to notify you that your request for change your KPI from:<br/><br/>");
                        }
                        else if (delete_flag == "1")//delete
                        {
                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Delete KPI has been Approved");
                            sb_body_introduction.Append("Dear, <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Name : " + user_title + " " + UserReader["empName"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to notify you that your request for delete your following KPI:<br/><br/>");
                        }
                        else if (delete_flag == "2")//add
                        {
                            sb_from_email.Append(UserReader["Email"].ToString());
                            sb_subject.Append("Request for Add New KPI has been Approved");
                            sb_body_introduction.Append("Dear, <br/>"
                                    + "NIK / <i>Barcode</i> : " + UserReader["EmpId"].ToString() + "<br/>"
                                    + "Name : " + user_title + " " + UserReader["empName"].ToString() + "<br/>"
                                    + "Group : " + UserReader["Group_Name"].ToString() + " (Individual Stretch Rating: " + UserReader["FinancialHeader_IndividualStretchRating"].ToString() + "%)<br/>"
                                    + "Organization : " + UserReader["OrgName"].ToString() + "<br/>"
                                    + "Additional Group : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                    + "Job Title : " + UserReader["JobTtlName"].ToString() + "<br/>"
                                    + "Grade : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                    + "I would like to notify you that your request add new following KPI for your individual scorecard:<br/><br/>");
                        }

                        sb_conclusion.Append("has been APPROVED.<br/><br/>Thank you for your coorperation. <br/><br/>Best Regards, <br/>" + sender_title + " " + sender_name + ""
                                           + "<br/><br/>This is an automatically generated email – please do not reply to it.");
                    }
                    UserReader.Dispose();
                    UserReader.Close();
                }

                if (delete_flag == "0")
                {
                    using (SqlDataReader CurrentReader = sql_get_current_kpi.ExecuteReader())
                    {
                        while (CurrentReader.Read())
                        {
                            if (CurrentReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                            {
                                string month_name_target, month_name_result;
                                month_name_target = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Target"].ToString()));
                                month_name_result = ShowMonthName(int.Parse(CurrentReader["IndividualHeader_Result"].ToString()));

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
                                string based_on_schedule = "", result = "";

                                if (CurrentReader["IndividualHeader_Target"].ToString() == "-1")
                                {
                                    based_on_schedule = "Based On Schedule";
                                    result = "-";
                                }
                                else
                                {
                                    based_on_schedule = CurrentReader["IndividualHeader_Target"].ToString() + " " + CurrentReader["IndividualHeader_MeasureBy"].ToString();
                                    result = CurrentReader["IndividualHeader_Result"].ToString() + " " + CurrentReader["IndividualHeader_MeasureBy"].ToString();
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
                                                + "<td style='border:1px solid black; padding:8px'>" + result + "</td>"
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

                using (SqlDataReader NewReader = sql_get_new_kpi.ExecuteReader())
                {
                    while (NewReader.Read())
                    {
                        if (NewReader["IndividualHeader_MeasureBy"].ToString() == "Month")
                        {
                            string month_name_target, month_name_result;
                            month_name_target = ShowMonthName(int.Parse(NewReader["IndividualHeader_Target"].ToString()));
                            month_name_result = ShowMonthName(int.Parse(NewReader["IndividualHeader_Result"].ToString()));
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
                            string based_on_schedule = "", result = "";

                            if (NewReader["IndividualHeader_Target"].ToString() == "-1")
                            {
                                based_on_schedule = "Based On Schedule";
                                result = "-";
                            }
                            else
                            {
                                based_on_schedule = NewReader["IndividualHeader_Target"].ToString() + " " + NewReader["IndividualHeader_MeasureBy"].ToString();
                                result = NewReader["IndividualHeader_Result"].ToString() + " " + NewReader["IndividualHeader_MeasureBy"].ToString();
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
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_KPI"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + NewReader["IndividualHeader_Objective"].ToString() + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + based_on_schedule + "</td>"
                                            + "<td style='border:1px solid black; padding:8px'>" + result + "</td>"
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


                SmtpClient mailclient = new SmtpClient();  //Karena FILE_LOCATION terjadi perubahan setiap di-klik, maka
                using (MailMessage msg = new MailMessage())//harus pake USING untuk CLEAR semua Resource yang pernah dipake
                {
                    /******************** SEND Email TO Users **************************************/
                    msg.Subject = sb_subject.ToString();
                    msg.Body = sb_body_introduction.ToString() + sb_current_detail.ToString() + sb_new_detail.ToString() + sb_conclusion.ToString();
                    if (sender_email == "" || sender_email == "-")
                    {
                        msg.From = new MailAddress("message@error.com");
                    }
                    else
                    {
                        msg.From = new MailAddress(sender_email.ToLower());
                    }

                    if (sb_from_email.ToString() == "" || sb_from_email.ToString() == "-")
                    {
                        msg.To.Add("message@error.com");
                    }
                    else
                    {
                        msg.To.Add(sb_from_email.ToString());//<-- E-Mail bawahan
                    }
                    msg.IsBodyHtml = true;
                    mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                    mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                    mailclient.Send(msg);
                }
                conn.Close();
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