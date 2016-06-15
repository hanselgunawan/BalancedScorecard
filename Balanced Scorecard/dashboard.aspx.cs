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
    public partial class dashboard : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            if (!IsPostBack)
            {

                int total_user = 0, total_submit = 0, total_not_submit = 0;
                var period_id = Request.QueryString["period_id"];
                string user_nik = "", user_name;
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='dashboard' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";
                if ((string)Session["user_nik"] == null)
                {
                    
                    Response.Redirect(baseUrl + "index.aspx");
                }
                else
                {
                    user_nik = (string)Session["user_nik"];
                }

                string string_select_user_name = "SELECT empName FROM ScorecardUser WHERE EmpId='" + user_nik + "'", sql_string_active = "";
                string string_count_BSC_user = "", string_select_have_submit = "", string_select_not_submit = "";
                StringBuilder HtmlDropdown = new StringBuilder();
                SqlConnection connect = new SqlConnection(str_connect);
                SqlCommand sql_select_user_name = new SqlCommand(string_select_user_name, connect);
                connect.Open();
                user_name = sql_select_user_name.ExecuteScalar().ToString();
                connect.Close();
                Session["user_name"] = user_name;

                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_check_access_page = new SqlCommand(string_check_access_page, conn);
                    SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);

                    using (SqlDataReader PageReader = sql_check_access_page.ExecuteReader())
                    {
                        if (!PageReader.HasRows)
                        {
                            Response.Redirect("" + baseUrl + "index.aspx");
                        }
                        PageReader.Dispose();
                        PageReader.Close();
                    }

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

                    if (period_id == null)
                    {
                        sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                        SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                        Object output_period_status = sql_active.ExecuteScalar();
                        if (output_period_status == null)
                        {
                            sql_string_active = "SELECT TOP(1) * FROM BSC_Period WHERE data_status='exist'";//langsung cari yang id-nya 1
                            SqlCommand sql_select_active_period_id = new SqlCommand(sql_string_active, conn);
                            using (SqlDataReader PeriodIDReader = sql_select_active_period_id.ExecuteReader())
                            {
                                if (PeriodIDReader.HasRows)
                                {
                                    while (PeriodIDReader.Read())
                                    {
                                        period_id = PeriodIDReader["Period_ID"].ToString();//harus string untuk ke object
                                    }
                                }
                                else
                                {
                                    period_id = "0";
                                }
                            }
                        }
                        else
                        {
                            string select_active_period_id = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
                            SqlCommand sql_select_active_period_id = new SqlCommand(select_active_period_id, conn);
                            using (SqlDataReader PeriodIDReader = sql_select_active_period_id.ExecuteReader())
                            {
                                while (PeriodIDReader.Read())
                                {
                                    period_id = PeriodIDReader["Period_ID"].ToString();//harus string untuk ke object
                                }
                            }
                        }
                    }
                    else
                    {
                        sql_string_active = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                    }
                    string sql_all_period = "SELECT * FROM BSC_Period WHERE data_status='exist' ORDER BY Start_Period ASC";
                    SqlCommand sql_command = new SqlCommand(sql_string_active, conn);
                    SqlCommand sql_command_all = new SqlCommand(sql_all_period, conn);
                    using (SqlDataReader ActivePeriodReader = sql_command.ExecuteReader())//UNTUK VIEW YANG STATUS = ACTIVE
                    {
                        if (ActivePeriodReader.HasRows)
                        {
                            while (ActivePeriodReader.Read())
                            {
                                string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya ga keluar!!
                                DateTime start_date = Convert.ToDateTime(ActivePeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(ActivePeriodReader["End_Period"]);
                                startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                                enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                                start_end_date = startdate_to_date + " - " + enddate_to_date;
                                HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                HtmlDropdown.Append(start_end_date + "&nbsp;<span class='caret'></span>");
                                HtmlDropdown.Append("</button>");
                            }
                        }
                        else
                        {
                            HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                            HtmlDropdown.Append("Period Not Set &nbsp;<span class='caret'></span>");
                            HtmlDropdown.Append("</button>");
                        }
                    }

                    using (SqlDataReader PeriodReader = sql_command_all.ExecuteReader())//UNTUK VIEW SEMUA PERIODE YANG ADA
                    {
                        HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                        if (PeriodReader.HasRows)
                        {
                            while (PeriodReader.Read())
                            {
                                string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya ga keluar!!
                                DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                                enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                                start_end_date = startdate_to_date + " - " + enddate_to_date;
                                HtmlDropdown.Append("<li role='presentation'><a role='menuitem' href='dashboard.aspx?period_id=" + PeriodReader["Period_ID"] + "'>");
                                HtmlDropdown.Append(start_end_date + "</a></li>");
                            }
                        }
                        else
                        {
                            HtmlDropdown.Append("<li role='presentation'>No Period</li>");
                        }
                        HtmlDropdown.Append("</ul>");
                    }
                    PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlDropdown.ToString() });//untuk DropDown
                    conn.Close();
                }

                if (Session["user_role"].ToString() != "1")
                {
                    string_count_BSC_user = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID = '" + Session["user_nik"].ToString() + "'";
                    string_select_have_submit = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID = '" + Session["user_nik"].ToString() + "' "
                                                 + "AND ScorecardUser.user_id IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                    string_select_not_submit = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID = '" + Session["user_nik"].ToString() + "' "
                                                 + "AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                }
                else
                {
                    string_count_BSC_user = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                            + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                            + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                            + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                            + "WHERE ScorecardUser.empStatus='Yes'";
                    string_select_have_submit = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "WHERE ScorecardUser.empStatus='Yes' "
                                                 + "AND ScorecardUser.user_id IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                    string_select_not_submit = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "WHERE ScorecardUser.empStatus='Yes' "
                                                 + "AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                }
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    SqlCommand sql_count_user = new SqlCommand(string_count_BSC_user, conn);
                    SqlCommand sql_count_submit_user = new SqlCommand(string_select_have_submit, conn);
                    SqlCommand sql_count_not_submit_user = new SqlCommand(string_select_not_submit, conn);

                    total_user = (int)sql_count_user.ExecuteScalar();
                    total_submit = (int)sql_count_submit_user.ExecuteScalar();
                    total_not_submit = (int)sql_count_not_submit_user.ExecuteScalar();

                    LabelHaveSubmit.Text = total_submit.ToString();
                    LabelTotalHaveSubmit.Text = total_user.ToString();
                    LabelNotSubmit.Text = total_not_submit.ToString();
                    LabelTotalNotSubmit.Text = total_user.ToString();
                    hrefSubmit.Attributes.Add("href", "view_submit_users.aspx?period_id=" + period_id + "");
                    hrefNotSubmit.Attributes.Add("href", "view_no_submit_users.aspx?period_id=" + period_id + "");

                    conn.Close();
                }

            }
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