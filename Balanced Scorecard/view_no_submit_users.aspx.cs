using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class view_no_submit_users : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        StringBuilder HtmlTableData = new StringBuilder();//untuk Table Data-nya
        StringBuilder Pagination = new StringBuilder();//untuk Pagination
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
                var paging = Request.QueryString["page"];//untuk pagination
                var nik = Request.QueryString["nik"];
                var period_id = Request.QueryString["period_id"];
                var org = Request.QueryString["organization"];
                var adt_org = Request.QueryString["adt_organization"];
                var bsc_group = Request.QueryString["bsc_group"];
                var name = Request.QueryString["name"];

                if (period_id == null) period_id = "1";

                StringBuilder HtmlPeriod = new StringBuilder();
                int page = 0;
                decimal no_header = 0;//inisialisasi
                decimal data_per_page = 5, max_select_data = 0, max_page = 0;//untuk pagination
                string start_date_formatted = "", end_date_formatted = "", string_select_user = "", get_max_data = "";

                hrefDashboard.Attributes.Add("href", "dashboard.aspx?period_id=" + period_id + "");

                TextBoxSearch.Attributes.Add("placeholder", "Search...");

                DropDownListSearchBy.Items.Add("Search By NIK");
                DropDownListSearchBy.Items.Add("Search By Name");
                DropDownListSearchBy.Items.Add("Search By Organization");
                DropDownListSearchBy.Items.Add("Search By Additional Group");
                DropDownListSearchBy.Items.Add("Search By Scorecard Group");

                hrefBackToDashboard.Attributes.Add("href", "dashboard.aspx?period_id=" + period_id + "");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                    string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan UserGroup
                                                  + "WHERE Access_Rights_Code NOT IN "
                                                  + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                                  + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                    SqlCommand sql_select_period = new SqlCommand(string_select_period, conn);
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

                    using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                    {
                        if (PeriodReader.HasRows)
                        {
                            while (PeriodReader.Read())
                            {
                                DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                                start_date_formatted = start_date.ToString("MMM");
                                end_date_formatted = end_date.ToString("MMM yyyy");
                                LabelPeriod.Text = start_date_formatted + " - " + end_date_formatted;
                            }
                        }
                        else
                        {
                            LabelPeriod.Text = " - ";
                        }
                        PeriodReader.Dispose();
                        PeriodReader.Close();
                    }
                    if (paging == null)//untuk pertama kali Load Page
                    {
                        page = 1;
                        no_header = (1 * data_per_page) - (data_per_page - 1);//untuk no. header kolom 1 Table jika data yang ditampilkan per halaman = 5
                    }
                    else
                    {
                        page = int.Parse(paging.ToString());
                        no_header = (page * data_per_page) - (data_per_page - 1);//agar no header tidak menjadi 1 lagi ketika di Page 2++
                    }

                    if (Session["user_role"].ToString() != "1")
                    {
                        get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                            + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                            + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                            + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                            + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.user_id NOT IN "
                                            + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                    }
                    else
                    {
                        get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                            + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                            + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                            + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                            + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                            + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                    }

                    if (nik == null && org == null && adt_org == null && bsc_group == null && name == null)//untuk Default Page
                    {
                        if (Session["user_role"].ToString() != "1")
                        {
                            string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";//sama seperti LIMIT pada MySQL
                        }
                        else
                        {
                            string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";//sama seperti LIMIT pada MySQL
                        }
                    }
                    else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null)
                    {
                        if (Session["user_role"].ToString() != "1")
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                          + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.EmpId LIKE '" + nik + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.user_id LIKE '" + nik + "%' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }
                        else
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                          + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.EmpId LIKE '" + nik + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.user_id LIKE '" + nik + "%' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }

                        TextBoxSearch.Value = nik.ToString();
                        DropDownListSearchBy.SelectedIndex = 0;
                    }
                    else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null)
                    {
                        if (Session["user_role"].ToString() != "1")
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                          + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND OrgName LIKE '" + org + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE OrgName LIKE '" + org + "%' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }
                        else
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                          + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND OrgName LIKE '" + org + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE OrgName LIKE '" + org + "%' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }

                        TextBoxSearch.Value = org.ToString();
                        DropDownListSearchBy.SelectedIndex = 2;
                    }
                    else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null)
                    {
                        if (Session["user_role"].ToString() != "1")
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND OrgAdtGroupName LIKE '" + adt_org + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE OrgAdtGroupName LIKE '" + adt_org + "%' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }
                        else
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND OrgAdtGroupName LIKE '" + adt_org + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE OrgAdtGroupName LIKE '" + adt_org + "%' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }

                        TextBoxSearch.Value = adt_org.ToString();
                        DropDownListSearchBy.SelectedIndex = 3;
                    }
                    else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null)
                    {
                        if (Session["user_role"].ToString() != "1")
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND Group_Name LIKE '" + bsc_group + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE Group_Name LIKE '" + bsc_group + "%' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }
                        else
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND Group_Name LIKE '" + bsc_group + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE Group_Name LIKE '" + bsc_group + "%' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }

                        TextBoxSearch.Value = bsc_group.ToString();
                        DropDownListSearchBy.SelectedIndex = 4;
                    }
                    else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null)
                    {
                        if (Session["user_role"].ToString() != "1")
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.empName LIKE '" + name + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode  "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empJobTitleCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.empName LIKE '" + name + "%' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }
                        else
                        {
                            get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                          + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                          + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                          + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                          + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.empName LIKE '" + name + "%' AND ScorecardUser.user_id NOT IN "
                                          + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ")";
                            string_select_user = "SELECT * FROM(SELECT ROW_NUMBER() OVER(ORDER BY Group_name ASC, EmpId ASC) "
                                                 + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.empName LIKE '" + name + "%' AND ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + "))sub "
                                                 + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                        }

                        TextBoxSearch.Value = name.ToString();
                        DropDownListSearchBy.SelectedIndex = 1;
                    }

                    SqlCommand sql_get_max_data = new SqlCommand(get_max_data, conn);
                    max_select_data = (int)sql_get_max_data.ExecuteScalar();//untuk mengetahui banyaknya page pada pagination
                    max_page = Math.Ceiling(max_select_data / data_per_page);//mendapatkan nilai banyaknya jumlah page

                    SqlCommand sql_select_user = new SqlCommand(string_select_user, conn);
                    using (SqlDataReader UserReader = sql_select_user.ExecuteReader())
                    {
                        if (UserReader.HasRows)
                        {
                            HtmlTableData.Append("<tr><th class='centering-th2'>No.</th><th class='centering-th2'>NIK</th><th class='centering-th2'>Name</th><th class='centering-th2'>Organization</th><th class='centering-th2'>Additional Group</th><th class='centering-th2'>Job Title</th><th width='100' class='centering-th2'>Grade</th><th width='100' class='centering-th2'>E-Mail</th><th class='centering-th2'>Scorecard Group</th></tr>");
                            while (UserReader.Read())
                            {
                                HtmlTableData.Append("<tr align='center' style='border-bottom:1px solid #ddd'>");
                                HtmlTableData.Append("<td class='td-align'>" + no_header + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["EmpId"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["empName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["OrgName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["OrgAdtGroupName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["JobTtlName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["empGrade"] + "</td>");
                                if (!DBNull.Value.Equals(UserReader["Email"]))
                                {
                                    if (UserReader["Email"].ToString() == "")
                                    {
                                        HtmlTableData.Append("<td width='100px' class='td-align'> - </td>");
                                    }
                                    else
                                    {
                                        HtmlTableData.Append("<td width='100px' class='td-align'>" + UserReader["Email"] + "</td>");
                                    }
                                }
                                else//Jika di database tertulis NULL
                                {
                                    HtmlTableData.Append("<td class='td-align'> - </td>");
                                }

                                if (nik == null && org == null && adt_org == null && bsc_group == null && name == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a href='view_financial_scorecard_dashboard.aspx?period_id=" + period_id + "&group_name=" + UserReader["Group_Name"] + "&prev_page=" + page + "&submit_id=0'>" + UserReader["Group_Name"] + "</a></td>");
                                }
                                else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a href='view_financial_scorecard_dashboard.aspx?period_id=" + period_id + "&group_name=" + UserReader["Group_Name"] + "&prev_page=" + page + "&submit_id=0&nik=" + nik + "'>" + UserReader["Group_Name"] + "</a></td>");
                                }
                                else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a href='view_financial_scorecard_dashboard.aspx?period_id=" + period_id + "&group_name=" + UserReader["Group_Name"] + "&prev_page=" + page + "&submit_id=0&organization=" + org + "'>" + UserReader["Group_Name"] + "</a></td>");
                                }
                                else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a href='view_financial_scorecard_dashboard.aspx?period_id=" + period_id + "&group_name=" + UserReader["Group_Name"] + "&prev_page=" + page + "&submit_id=0&adt_organization=" + adt_org + "'>" + UserReader["Group_Name"] + "</a></td>");
                                }
                                else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a href='view_financial_scorecard_dashboard.aspx?period_id=" + period_id + "&group_name=" + UserReader["Group_Name"] + "&prev_page=" + page + "&submit_id=0&bsc_group=" + bsc_group + "'>" + UserReader["Group_Name"] + "</a></td>");
                                }
                                else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a href='view_financial_scorecard_dashboard.aspx?period_id=" + period_id + "&group_name=" + UserReader["Group_Name"] + "&prev_page=" + page + "&submit_id=0&name=" + name + "'>" + UserReader["Group_Name"] + "</a></td>");
                                }

                                HtmlTableData.Append("</tr>");
                                no_header++;
                            }
                        }
                        else
                        {
                            HtmlTableData.Append("<tr><th class='centering-th2'>No User To Display</th></tr>");
                        }
                    }
                    conn.Close();
                }
                PlaceHolderTable.Controls.Add(new Literal { Text = HtmlTableData.ToString() }); //untuk Table

                //Code untuk Pagination
                Pagination.Append("<ul id='my-pagination' class='pagination'></ul>");

                //Pagination JQuery
                Pagination.Append("<script>");
                Pagination.Append("$('#my-pagination').twbsPagination({");
                Pagination.Append("totalPages: " + max_page + ",");
                Pagination.Append("visiblePages: 7,");
                if (nik == null && org == null && adt_org == null && bsc_group == null && name == null)
                {
                    Pagination.Append("href: '?page={{number}}&period_id=" + period_id + "'");
                }
                else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null)
                {
                    Pagination.Append("href: '?page={{number}}&nik=" + nik + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null)
                {
                    Pagination.Append("href: '?page={{number}}&organization=" + org + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null)
                {
                    Pagination.Append("href: '?page={{number}}&adt_organization=" + adt_org + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null)
                {
                    Pagination.Append("href: '?page={{number}}&bsc_group=" + bsc_group + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null)
                {
                    Pagination.Append("href: '?page={{number}}&name=" + name + "&period_id=" + period_id + "'");
                }
                Pagination.Append("});");
                Pagination.Append("</script>");
                PlaceHolderPaging.Controls.Add(new Literal { Text = Pagination.ToString() });//untuk Pagination
            }
        }

        protected void OnClickExportExcel(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
            int no_header = 1;
            string start_date_formatted = "", end_date_formatted = "";

            if (period_id == null)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    string sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                    SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                    Object output_period_status = sql_active.ExecuteScalar();
                    if (output_period_status == null)//jika tidak ada yang Active
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
                    conn.Close();
                }
            }

            string string_select_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
            using (SqlConnection conn_period = new SqlConnection(str_connect))
            {
                conn_period.Open();
                SqlCommand sql_select_period = new SqlCommand(string_select_period, conn_period);
                using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                {
                    while (PeriodReader.Read())
                    {
                        DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                        DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                        start_date_formatted = start_date.ToString("MMM");
                        if (DateTime.Now.ToString("yyyy") != end_date.ToString("yyyy"))
                        {
                            end_date_formatted = end_date.ToString("MMM") + " " + end_date.ToString("yyyy");
                        }
                        else
                        {
                            end_date_formatted = "" + DateTime.Now.ToString("MMM") + " " + end_date.ToString("yyyy") + "";
                        }
                    }
                    PeriodReader.Dispose();
                    PeriodReader.Close();
                }
                conn_period.Close();
            }

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.ContentType = "application/xls";
            HttpContext.Current.Response.Write(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");

            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
            //sets font
            HttpContext.Current.Response.Write("<font style='font-size:12pt; font-family:Times New Roman;'>");
            HttpContext.Current.Response.Write("<BR><BR><BR>");
            //sets the table border, cell spacing, border color, font of the text, background, foreground, font height
            HttpContext.Current.Response.Write("<Table border='1' bgColor='#ffffff' " +
              "borderColor='#000000' cellSpacing='0' cellPadding='0' " +
              "style='font-size:12.0pt; font-family:'Times New Roman'; background:white;'>");

            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='9' align='center' style='font-size:16pt'><b>LIST OF BALANCED SCORECARD USERS</b></td>");
            HttpContext.Current.Response.Write("</tr>");
            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='9' align='center' style='font-size:12pt'>(HAVE <b>NOT SUBMITTED</b> SCORECARD)</td>");
            HttpContext.Current.Response.Write("</tr>");
            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='9' align='center' style='font-size:12pt'>Period: " + start_date_formatted + " - " + end_date_formatted + "</td>");
            HttpContext.Current.Response.Write("</tr>");
            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='9'></td>");
            HttpContext.Current.Response.Write("</tr>");

            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + "List of NOT SUBMITTED BSC Users (" + start_date_formatted + " - " + end_date_formatted + ").xls\"");

            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td align='center'><b>No.</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>NIK</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Name</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Organization</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Additional Group</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Job Title</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Grade</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>E-Mail</b></td>");
            HttpContext.Current.Response.Write("<td align='center'><b>Scorecard Group</b></td>");
            HttpContext.Current.Response.Write("</tr>");

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string string_select_scorecard_user = "";
                conn.Open();
                if (Session["user_role"].ToString() != "1")
                {
                    string_select_scorecard_user = "SELECT user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.Superior_ID='" + Session["user_nik"].ToString() + "' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ") ORDER BY Group_Name ASC, EmpId ASC";
                }
                else
                {
                    string_select_scorecard_user = "SELECT user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, LOWER(empEmail) as Email, "
                                                 + "empGrade, empStatus, Group_Name "
                                                 + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                                 + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                                 + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                                 + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                                 + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                                 + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                 + "WHERE ScorecardUser.empStatus='Yes' AND ScorecardUser.user_id NOT IN "
                                                 + "(SELECT user_id FROM IndividualMeasures_Header WHERE Period_ID=" + period_id + ") ORDER BY Group_Name ASC, EmpId ASC";
                }

                SqlCommand sql_select_scorecard_user = new SqlCommand(string_select_scorecard_user, conn);
                using (SqlDataReader UserReader = sql_select_scorecard_user.ExecuteReader())
                {
                    while (UserReader.Read())
                    {
                        HttpContext.Current.Response.Write("<tr>");
                        HttpContext.Current.Response.Write("<td align='center'>" + no_header + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["EmpId"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["empName"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["OrgName"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["OrgAdtGroupName"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["JobTtlName"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["empGrade"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Email"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Group_Name"] + "</td>");
                        HttpContext.Current.Response.Write("</tr>");
                        no_header++;
                    }
                }
                conn.Close();
            }
            HttpContext.Current.Response.Write("</Table>");
            HttpContext.Current.Response.Write("</font>");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        protected void OnClickSearch(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var period_id = Request.QueryString["period_id"];

            if (period_id == null)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    string sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                    SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                    Object output_period_status = sql_active.ExecuteScalar();
                    if (output_period_status == null)//jika tidak ada yang Active
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
                    conn.Close();
                }
            }

            if (TextBoxSearch.Value == "")
            {
                Response.Redirect("" + baseUrl + "view_no_submit_users.aspx?period_id=" + period_id + "");
            }
            else
            {
                if (DropDownListSearchBy.SelectedIndex == 0)
                {
                    Response.Redirect("" + baseUrl + "view_no_submit_users.aspx?nik=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 1)
                {
                    Response.Redirect("" + baseUrl + "view_no_submit_users.aspx?name=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 2)
                {
                    Response.Redirect("" + baseUrl + "view_no_submit_users.aspx?organization=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 3)
                {
                    Response.Redirect("" + baseUrl + "view_no_submit_users.aspx?adt_organization=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 4)
                {
                    Response.Redirect("" + baseUrl + "view_no_submit_users.aspx?bsc_group=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
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