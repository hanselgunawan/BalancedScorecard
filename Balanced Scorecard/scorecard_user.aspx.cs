using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.parser;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class scorecard_user : System.Web.UI.Page
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
                var active = Request.QueryString["active"];
                var superior = Request.QueryString["superior"];
                var role = Request.QueryString["role"];

                StringBuilder HtmlPeriod = new StringBuilder();
                int page = 0;
                decimal no_header = 0;//inisialisasi
                decimal data_per_page = 5, max_select_data = 0, max_page = 0;//untuk pagination
                string string_select_user = "";//inisialisasi agar dinamis
                string sql_string_active = "";//inisialisasi agar dynamic
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='scorecard_user' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";

                btnAddMeasure.Attributes.Add("class", "btn btn-default");
                btnImportExcel.Attributes.Add("class", "btn btn-default");
                TextBoxSearch.Attributes.Add("placeholder", "Search...");

                DropDownListSearchBy.Items.Add("Search By NIK");
                DropDownListSearchBy.Items.Add("Search By Name");
                DropDownListSearchBy.Items.Add("Search By Organization");
                DropDownListSearchBy.Items.Add("Search By Additional Group");
                DropDownListSearchBy.Items.Add("Search By Scorecard Group");
                DropDownListSearchBy.Items.Add("Search By Active Status");
                DropDownListSearchBy.Items.Add("Search By Superior NIK");
                DropDownListSearchBy.Items.Add("Search By Role");

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    SqlCommand sql_check_access_page = new SqlCommand(string_check_access_page, conn);
                    SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);
                    conn.Open();

                    using (SqlDataReader PageReader = sql_check_access_page.ExecuteReader())
                    {
                        if (!PageReader.HasRows)
                        {
                            Response.Redirect("" + baseUrl + "index.aspx");
                        }
                        PageReader.Close();
                        PageReader.Dispose();
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
                        AccessReader.Dispose();
                        AccessReader.Close();
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

                    if (period_id == null)
                    {
                        sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                        SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                        Object output_period_status = sql_active.ExecuteScalar();
                        Object output_period_id;
                        if (output_period_status == null)//jika tidak ada yang Active
                        {
                            string string_select_top_period_id = "SELECT TOP(1) Period_ID FROM BSC_Period WHERE data_status='exist'";
                            SqlCommand sql_select_top_period_id = new SqlCommand(string_select_top_period_id, conn);
                            output_period_id = sql_select_top_period_id.ExecuteScalar();
                            if (output_period_id != null)
                            {
                                period_id = sql_select_top_period_id.ExecuteScalar().ToString();
                            }
                            else
                            {
                                period_id = "0";
                            }
                            sql_string_active = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";//langsung cari Period_ID Top-nya
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

                    string sql_all_period = "SELECT * FROM BSC_Period WHERE data_status='exist' ORDER BY Start_Period ASC";//mengambil semua periode untuk DropDownList Html
                    SqlCommand sql_command = new SqlCommand(sql_string_active, conn);
                    SqlCommand sql_command_all = new SqlCommand(sql_all_period, conn);
                    using (SqlDataReader ButtonReader = sql_command.ExecuteReader())//UNTUK VIEW YANG STATUS = ACTIVE
                    {
                        if (ButtonReader.HasRows)
                        {
                            while (ButtonReader.Read())
                            {
                                string startdate_to_date, enddate_to_date, start_end_date;//butuh agar jam nya ga keluar!!
                                DateTime start_date = Convert.ToDateTime(ButtonReader["Start_Period"]);
                                DateTime end_date = Convert.ToDateTime(ButtonReader["End_Period"]);
                                startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                                enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                                start_end_date = startdate_to_date + " - " + enddate_to_date;
                                HtmlPeriod.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                HtmlPeriod.Append(start_end_date + "&nbsp;<span class='caret'></span>");
                                HtmlPeriod.Append("</button>");
                            }
                        }
                        else
                        {
                            HtmlPeriod.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                            HtmlPeriod.Append("Period Not Set &nbsp;<span class='caret'></span>");
                            HtmlPeriod.Append("</button>");
                            btnImportExcel.Attributes.Add("disabled","true");
                            btnAddMeasure.Attributes.Add("disabled", "true");
                            DropDownListSearchBy.Enabled = false;
                            ButtonSearch.Enabled = false;
                            ButtonPDF.Enabled = false;
                            ButtonExcel.Enabled = false;
                        }
                    }

                    using (SqlDataReader PeriodReader = sql_command_all.ExecuteReader())//UNTUK VIEW SEMUA PERIODE YANG ADA
                    {
                        HtmlPeriod.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                        while (PeriodReader.Read())
                        {
                            string startdate_to_date, enddate_to_date, start_end_date;
                            DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                            DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                            startdate_to_date = start_date.ToString("MMM");
                            enddate_to_date = end_date.ToString("MMM yyyy");
                            start_end_date = startdate_to_date + " - " + enddate_to_date;
                            HtmlPeriod.Append("<li role='presentation'><a role='menuitem' href='scorecard_user.aspx?page=1&period_id=" + PeriodReader["Period_ID"] + "'>");
                            HtmlPeriod.Append(start_end_date + "</a></li>");
                        }
                        HtmlPeriod.Append("</ul>");
                    }
                    PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlPeriod.ToString() });//untuk DropDown

                    btnAddMeasure.Attributes.Add("a href", "add_user.aspx?page=" + page + "&period_id=" + period_id + "");
                    btnImportExcel.Attributes.Add("a href", "import_user.aspx?page=" + page + "&period_id=" + period_id + "");

                    string get_max_data = "SELECT COUNT(user_id) FROM [Balanced Scorecard].dbo.ScorecardUser "
                                        + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                        + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                        + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + "";

                    if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                    {
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + ")sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";
                    }
                    else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                    {
                        get_max_data = get_max_data + " WHERE EmpId LIKE '" + nik + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE ScorecardUser.EmpId LIKE '" + nik + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = nik.ToString();
                        DropDownListSearchBy.SelectedIndex = 0;
                    }
                    else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                    {
                        get_max_data = get_max_data + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                                    + "WHERE OrgName LIKE '" + org + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE OrgName LIKE '" + org + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = org.ToString();
                        DropDownListSearchBy.SelectedIndex = 2;
                    }
                    else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null && active == null && superior == null && role == null)
                    {
                        get_max_data = get_max_data + " WHERE OrgAdtGroupName LIKE '" + adt_org + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE OrgAdtGroupName LIKE '" + adt_org + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = adt_org.ToString();
                        DropDownListSearchBy.SelectedIndex = 3;
                    }
                    else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null && active == null && superior == null && role == null)
                    {
                        get_max_data = get_max_data + " WHERE Group_Name LIKE '" + bsc_group + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE ScorecardGroupLink.Group_Name LIKE '" + bsc_group + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = bsc_group.ToString();
                        DropDownListSearchBy.SelectedIndex = 4;
                    }
                    else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null && active == null && superior == null && role == null)
                    {
                        get_max_data = get_max_data + " WHERE empName LIKE '" + name + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE empName LIKE '" + name + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = name.ToString();
                        DropDownListSearchBy.SelectedIndex = 1;
                    }
                    else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active != null && superior == null && role == null)
                    {
                        get_max_data = get_max_data + " WHERE empStatus LIKE '" + active + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE empStatus LIKE '" + active + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = active.ToString();
                        DropDownListSearchBy.SelectedIndex = 5;
                    }
                    else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior != null && role == null)
                    {
                        get_max_data = get_max_data + " WHERE Superior_ID LIKE '" + superior + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE Superior_ID LIKE '" + superior + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = superior.ToString();
                        DropDownListSearchBy.SelectedIndex = 6;
                    }
                    else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role != null)
                    {
                        get_max_data = get_max_data + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                                    + "WHERE BSC_UserGroup.Group_Name LIKE '" + role + "%'";
                        string_select_user = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ScorecardGroupLink.Group_Name ASC, ScorecardUser.date_create ASC) "
                                             + "AS rowNum, user_id, EmpId, empName, OrgName, OrgAdtGroupName, JobTtlName, JobLvlName, BSC_UserGroup.Group_Name role, "
                                             + "empGrade, empStatus, Superior_ID, ScorecardGroupLink.Group_Name "
                                             + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                             + "join [Human_Capital_demo].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                             + "join ScorecardGroupLink (nolock) on ScorecardGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                             + "join BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                             + "join [Human_Capital_demo].dbo.Organization on ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle on ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "join BSC_Period on ScorecardGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                             + "WHERE BSC_UserGroup.Group_Name LIKE '" + role + "%')sub "
                                             + "WHERE rowNum>=((" + page + "-1)*" + data_per_page + ")+1 AND rowNum<=" + data_per_page + "*" + page + "";

                        TextBoxSearch.Value = role.ToString();
                        DropDownListSearchBy.SelectedIndex = 7;
                    }

                    SqlCommand sql_get_max_data = new SqlCommand(get_max_data, conn);
                    max_select_data = (int)sql_get_max_data.ExecuteScalar();//untuk mengetahui banyaknya page pada pagination
                    max_page = Math.Ceiling(max_select_data / data_per_page);//mendapatkan nilai banyaknya jumlah page

                    SqlCommand sql_select_user = new SqlCommand(string_select_user, conn);
                    using (SqlDataReader UserReader = sql_select_user.ExecuteReader())
                    {
                        if (UserReader.HasRows)
                        {
                            HtmlTableData.Append("<tr><th class='centering-th2'>No.</th><th class='centering-th2'>NIK</th>"
                                                + "<th class='centering-th2'>Name</th><th class='centering-th2'>Organization</th>"
                                                + "<th class='centering-th2'>Additional Group</th><th class='centering-th2'>Job Title</th>"
                                                + "<th class='centering-th2'>Job Level</th>"
                                                + "<th width='100' class='centering-th2'>Grade</th><th width='100' class='centering-th2'>Role</th>"
                                                + "<th width='100' class='centering-th2'>Superior NIK</th><th class='centering-th2'>Scorecard Group</th>"
                                                + "<th class='centering-th2'>Active</th><th class='centering-th2'>Edit</th></tr>");
                            while (UserReader.Read())
                            {
                                string string_select_individual_scorecard = "SELECT * FROM IndividualMeasures_Header WHERE EmpId='" + UserReader["EmpId"] + "'";
                                SqlCommand sql_select_individual_scorecard = new SqlCommand(string_select_individual_scorecard, conn);
                                HtmlTableData.Append("<tr align='center' style='border-bottom:1px solid #ddd'>");
                                HtmlTableData.Append("<td class='td-align'>" + no_header + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["EmpId"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["empName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["OrgName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["OrgAdtGroupName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["JobTtlName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["JobLvlName"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["empGrade"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["role"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["Superior_ID"] + "</td>");
                                HtmlTableData.Append("<td class='td-align'>" + UserReader["Group_Name"] + "</td>");
                                if (UserReader["empStatus"].ToString() == "Yes")
                                {
                                    HtmlTableData.Append("<td class='td-align active-period'>" + UserReader["empStatus"] + "</td>");
                                }
                                else
                                {
                                    HtmlTableData.Append("<td class='td-align inactive-period'>" + UserReader["empStatus"] + "</td>");
                                }

                                if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "'>Edit</a></td>");
                                }
                                else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&nik="+nik+"'>Edit</a></td>");
                                }
                                else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&org="+org+"'>Edit</a></td>");
                                }
                                else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null && active == null && superior == null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&adt_org="+adt_org+"'>Edit</a></td>");
                                }
                                else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null && active == null && superior == null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&bsc_group="+bsc_group+"'>Edit</a></td>");
                                }
                                else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null && active == null && superior == null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&name="+name+"'>Edit</a></td>");
                                }
                                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active != null && superior == null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&active="+active+"'>Edit</a></td>");
                                }
                                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior != null && role == null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&superior="+superior+"'>Edit</a></td>");
                                }
                                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role != null)
                                {
                                    HtmlTableData.Append("<td class='td-align'><a class='btn btn-default' href='edit_user.aspx?page=" + page + "&user_id=" + UserReader["user_id"] + "&period_id=" + period_id + "&role=" + role + "'>Edit</a></td>");
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
                if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&period_id=" + period_id + "'");
                }
                else if (nik != null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&nik=" + nik + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org != null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&organization=" + org + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org != null && bsc_group == null && name == null && active == null && superior == null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&adt_organization=" + adt_org + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org == null && bsc_group != null && name == null && active == null && superior == null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&bsc_group=" + bsc_group + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name != null && active == null && superior == null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&name=" + name + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active != null && superior == null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&active=" + active + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior != null && role == null)
                {
                    Pagination.Append("href: '?page={{number}}&superior=" + superior + "&period_id=" + period_id + "'");
                }
                else if (nik == null && org == null && adt_org == null && bsc_group == null && name == null && active == null && superior == null && role != null)
                {
                    Pagination.Append("href: '?page={{number}}&role=" + role + "&period_id=" + period_id + "'");
                }
                Pagination.Append("});");
                Pagination.Append("</script>");
                PlaceHolderPaging.Controls.Add(new Literal { Text = Pagination.ToString() });//untuk Pagination
            }
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
                Response.Redirect("" + baseUrl + "scorecard_user.aspx?period_id=" + period_id + "");
            }
            else
            {
                if (DropDownListSearchBy.SelectedIndex == 0)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?nik=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 1)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?name=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 2)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?organization=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 3)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?adt_organization=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 4)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?bsc_group=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 5)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?active=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 6)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?superior=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
                else if (DropDownListSearchBy.SelectedIndex == 7)
                {
                    Response.Redirect("" + baseUrl + "scorecard_user.aspx?role=" + TextBoxSearch.Value + "&period_id=" + period_id + "");
                }
            }
        }

        protected void OnClickExportPDF(object sender, EventArgs e)
        {
            var period_id = Request.QueryString["period_id"];
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

            //Buat PDF
            Response.ContentType = "application/pdf";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWriter);
            StringReader read = new StringReader(stringWriter.ToString());
            string ImageURL = Server.MapPath(".") + "/Images/mppa.png";//untuk insert image ke PDF
            iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(ImageURL);
            png.ScaleToFit(650f, 200f);//untuk resize image 
            png.SpacingBefore = 10f;//kasih space sebelum image
            png.SpacingAfter = 10f;//kasih space sesudah image
            png.Alignment = Element.ALIGN_CENTER;

            Document pdfDoc = new Document();
            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());//agar halaman PDF Landscape
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            writer.PageEvent = new System.Data.Common.ITextEvents();
            pdfDoc.Open();

            HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
            htmlContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(false);

            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(pdfDoc, writer)));

            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser xmlParse = new XMLParser(true, worker);

            StringBuilder DataTable = new StringBuilder();

            //isi PDF
            int no_header = 1;
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
                        end_date_formatted = end_date.ToString("MMM yyyy");
                    }
                    PeriodReader.Dispose();
                    PeriodReader.Close();
                }
                conn_period.Close();
            }
            DataTable.Append("<p style='text-align:center; font-size:50px'>PT. Matahari Putra Prima Tbk.</p>");
            DataTable.Append("<p style='text-align:center; font-size:30px'>BALANCED SCORECARD USERS</p>");
            DataTable.Append("<p style='text-align:center; font-size:30px; page-break-after: always'>(" + start_date_formatted + " - " + end_date_formatted + ")</p>");
            Response.AddHeader("Content-Disposition", "attachment;filename=\"" + "Balanced Scorecard Users (" + start_date_formatted + " - " + end_date_formatted + ").pdf\"");
            pdfDoc.Add(png);
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string string_select_scorecard_user = "";
                conn.Open();

                string_select_scorecard_user = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, "
                                             + "OrgName, JobTtlName, JobLvlName, empStatus, LOWER(ScorecardUser.empEmail) as Email, "
                                             + "empGrade, Group_Name FROM ScorecardUser "
                                             + "JOIN ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode=ScorecardGroupLink.OrgAdtGroupCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle ON ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.Organization ON ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "JOIN BSC_Period ON BSC_Period.Period_ID=ScorecardGroupLink.Period_ID "
                                             + "WHERE ScorecardGroupLink.Period_ID =" + period_id + " "
                                             + "ORDER BY Group_Name ASC, ScorecardUser.date_create ASC";
                SqlCommand sql_select_scorecard_user = new SqlCommand(string_select_scorecard_user, conn);
                DataTable.Append("<table align='center' width='100%' style='border-collapse:collapse; text-align:center; font-size:10pt; page-break-after: always'>");
                DataTable.Append("<tr>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>No.</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>NIK</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>Name</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>Organization</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>Job Title</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>Job Level</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>Grade</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>E-Mail</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>Scorecard Group</b></td>");
                DataTable.Append("<td style='padding:10px; border:1px solid black; background-color:yellow'><b>Active</b></td>");
                DataTable.Append("</tr>");

                using (SqlDataReader UserReader = sql_select_scorecard_user.ExecuteReader())
                {
                    while(UserReader.Read())
                    {
                        DataTable.Append("<tr>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + no_header + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["EmpId"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["empName"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["OrgName"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["JobTtlName"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["JobLvlName"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["empGrade"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["Email"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["Group_Name"] + "</td>");
                        DataTable.Append("<td style='padding:10px; border:1px solid black'>" + UserReader["empStatus"] + "</td>");
                        DataTable.Append("</tr>");
                        no_header++;
                    }
                }
                DataTable.Append("</table>");
                conn.Close();
            }

            StringReader sr = new StringReader(DataTable.ToString());
            xmlParse.Parse(sr);
            xmlParse.Flush();
            pdfDoc.Close();
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
                        end_date_formatted = end_date.ToString("MMM yyyy");
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
            HttpContext.Current.Response.Write("<td colspan='10' align='center' style='font-size:18pt; background-color:yellow'><b>BALANCED SCORECARD USERS</b></td>");
            HttpContext.Current.Response.Write("</tr>");
            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td colspan='10' align='center' style='font-size:14pt; background-color:#f2f2f2'><b>Period: " + start_date_formatted + " - " + end_date_formatted + "</b></td>");
            HttpContext.Current.Response.Write("</tr>");

            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + "Balanced Scorecard Users (" + start_date_formatted + " - " + end_date_formatted + ").xls\"");

            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>No.</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>NIK</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>Name</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>Organization</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>Job Title</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>Job Level</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>Grade</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>E-Mail</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>Scorecard Group</b></td>");
            HttpContext.Current.Response.Write("<td align='center' style='background-color:#fde9d9'><b>Active</b></td>");
            HttpContext.Current.Response.Write("</tr>");

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                string string_select_scorecard_user = "";
                conn.Open();
                string_select_scorecard_user = "SELECT ScorecardUser.EmpId, ScorecardUser.empName, "
                                             + "OrgName, JobTtlName, JobLvlName, empStatus, LOWER(ScorecardUser.empEmail) as Email, "
                                             + "empGrade, Group_Name FROM ScorecardUser "
                                             + "JOIN ScorecardGroupLink ON ScorecardUser.empOrgAdtGroupCode=ScorecardGroupLink.OrgAdtGroupCode "
                                             + "join [Human_Capital_demo].dbo.JobTitle ON ScorecardUser.empJobTitleCode = JobTitle.JobTtlCode "
                                             + "join [Human_Capital_demo].dbo.Organization ON ScorecardUser.empOrgCode = Organization.OrgCode "
                                             + "join [Human_Capital_demo].dbo.JobLevel ON JobLevel.JobLvlCode = ScorecardUser.empGrade "
                                             + "JOIN BSC_Period ON BSC_Period.Period_ID=ScorecardGroupLink.Period_ID "
                                             + "WHERE ScorecardGroupLink.Period_ID =" + period_id + " "
                                             + "ORDER BY Group_Name ASC, ScorecardUser.date_create ASC";
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
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["JobTtlName"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["JobLvlName"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["empGrade"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Email"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["Group_Name"] + "</td>");
                        HttpContext.Current.Response.Write("<td align='center'>" + UserReader["empStatus"] + "</td>");
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
