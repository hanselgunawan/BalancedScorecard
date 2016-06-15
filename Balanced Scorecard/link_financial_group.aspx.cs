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
    public partial class link_financial_group : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        string str_connect_HC = ConfigurationManager.ConnectionStrings["HumanCapitalConnection"].ConnectionString;
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
                StringBuilder HtmlDropdown = new StringBuilder();
                var period_id = Request.QueryString["period_id"];
                var selected_group_name_index = Request.QueryString["group_index"];
                var empty_flag = Request.QueryString["empty_flag"];//jika kotak kanan kosong, harus redirect dan munculin error message-nya
                                                                  //ini diperlukan karena dropdown list period adalah HTML, bukan dari DDL Toolbox
                int loop = 0;
                string string_select_group_name = "SELECT Group_Name FROM ScorecardGroup ORDER BY Group_ID ASC";//pilih nama group
                string string_select_all_period = "SELECT * FROM BSC_Period WHERE data_status='exist' "
                                                + "ORDER BY Start_Period ASC";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                          + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                          + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                          + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='link_financial_group' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";
                //string sql_string_active = "";

                if (empty_flag == "1")
                {
                    ErrorMessageLink.Attributes.Add("style", "visibility:visible !important");
                }
                else
                {
                    ErrorMessageLink.Attributes.Add("style", "visibility:hidden !important");
                }

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    SqlCommand sql_select_group_name = new SqlCommand(string_select_group_name, conn);
                    SqlCommand sql_select_all_period = new SqlCommand(string_select_all_period, conn);
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

                    using (SqlDataReader GroupReader = sql_select_group_name.ExecuteReader())
                    {
                        while (GroupReader.Read())
                        {
                            DropDownListGroup.Items.Add(GroupReader["Group_Name"].ToString());
                        }
                    }

                    using(SqlDataReader PeriodReader = sql_select_all_period.ExecuteReader())
                    {
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
                                if (loop == 0)
                                {
                                    if (period_id == null)
                                    {
                                        string string_select_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//jika NULL, langsung pilih yang Active jika ada
                                        SqlCommand sql_select_active = new SqlCommand(string_select_active, conn);
                                        using (SqlDataReader ActiveReader = sql_select_active.ExecuteReader())
                                        {
                                            if (ActiveReader.HasRows)
                                            {
                                                while (ActiveReader.Read())
                                                {
                                                    string startdate_to_date3, enddate_to_date3, start_end_date3;//butuh agar jam nya ga keluar!!
                                                    DateTime start_date3 = Convert.ToDateTime(ActiveReader["Start_Period"]);
                                                    DateTime end_date3 = Convert.ToDateTime(ActiveReader["End_Period"]);
                                                    startdate_to_date3 = start_date3.ToString("MMM");//aslinya MM-dd-yyyy
                                                    enddate_to_date3 = end_date3.ToString("MMM yyyy");//ubah format tanggal!
                                                    start_end_date3 = startdate_to_date3 + " - " + enddate_to_date3;
                                                    HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                                    HtmlDropdown.Append(start_end_date3 + "&nbsp;<span class='caret'></span>");
                                                    HtmlDropdown.Append("</button>");
                                                    HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                                    period_id = ActiveReader["Period_ID"].ToString();
                                                }
                                            }
                                            else
                                            {
                                                HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                                HtmlDropdown.Append(start_end_date + "&nbsp;<span class='caret'></span>");
                                                HtmlDropdown.Append("</button>");
                                                HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                                period_id = PeriodReader["Period_ID"].ToString();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string string_selected_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                                        SqlCommand sql_selected_period = new SqlCommand(string_selected_period, conn);
                                        using (SqlDataReader SelectedPeriodReader = sql_selected_period.ExecuteReader())
                                        {
                                            while (SelectedPeriodReader.Read())
                                            {
                                                string startdate_to_date2, enddate_to_date2, start_end_date2;//butuh agar jam nya ga keluar!!
                                                DateTime start_date2 = Convert.ToDateTime(SelectedPeriodReader["Start_Period"]);
                                                DateTime end_date2 = Convert.ToDateTime(SelectedPeriodReader["End_Period"]);
                                                startdate_to_date2 = start_date2.ToString("MMM");//aslinya MM-dd-yyyy
                                                enddate_to_date2 = end_date2.ToString("MMM yyyy");//ubah format tanggal!
                                                start_end_date2 = startdate_to_date2 + " - " + enddate_to_date2;
                                                HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                                HtmlDropdown.Append(start_end_date2 + "&nbsp;<span class='caret'></span>");
                                                HtmlDropdown.Append("</button>");
                                                HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                            }
                                        }
                                    }
                                    loop++;
                                }
                                HtmlDropdown.Append("<li role='presentation'><a role='menuitem' href='link_financial_group.aspx?period_id=" + PeriodReader["Period_ID"] + "&group_index=" + DropDownListGroup.SelectedIndex + "'>");
                                HtmlDropdown.Append(start_end_date + "</a></li>");
                            }
                            HtmlDropdown.Append("</ul>");
                        }
                        else
                        {
                            HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                            HtmlDropdown.Append("Period Not Set&nbsp;<span class='caret'></span>");
                            HtmlDropdown.Append("</button>");
                            HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                            HtmlDropdown.Append("</ul>");
                            SpanDone.Attributes.Add("class", "btn btn-done-linking btn-done-linking-container done-button disabled");
                            period_id = "0";
                        }
                    }
                    conn.Close();
                }

                PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlDropdown.ToString() });

                if (selected_group_name_index == null)//agar pilihan yang dipilih berubah pada saat Index Changed
                {
                    DropDownListGroup.SelectedIndex = 0;
                }
                else
                {
                    DropDownListGroup.SelectedIndex = int.Parse(selected_group_name_index);
                }

                using (SqlConnection conn = new SqlConnection(str_connect_HC))
                {
                    using (SqlConnection conn2 = new SqlConnection(str_connect))
                    {
                        string string_select_left_additional_group_name = "SELECT OrgAdtGroup.OrgAdtCode, OrgAdtGroup.OrgAdtname "
                                                                        + "FROM [Human_Capital_demo].[dbo].[OrgAdtGroup] OrgAdtGroup "
                                                                        + "LEFT JOIN [Balanced Scorecard].[dbo].[ScorecardGroupLink] ScorecardGroup "
                                                                        + "ON ScorecardGroup.OrgAdtGroupCode = OrgAdtGroup.OrgAdtCode "
                                                                        + "AND Period_ID=" + period_id + " "
                                                                        + "WHERE ScorecardGroup.OrgAdtGroupCode IS NULL";//pilih nama Additional Group
                        string string_select_right_additional_group_name = "SELECT ScorecardGroup.OrgAdtGroupCode, ScorecardGroup.OrgAdtGroupName "
                                                                         + "FROM [Balanced Scorecard].[dbo].[ScorecardGroupLink] ScorecardGroup "
                                                                         + "JOIN [Human_Capital_demo].[dbo].[OrgAdtGroup] OrgAdtGroup "
                                                                         + "ON ScorecardGroup.OrgAdtGroupCode = OrgAdtGroup.OrgAdtCode "
                                                                         + "WHERE ScorecardGroup.Group_Name ='" + DropDownListGroup.SelectedValue + "' "
                                                                         + "AND Period_ID=" + period_id + " ORDER BY OrgAdtGroupCode ASC";
                        SqlCommand sql_select_left_additional_group_name = new SqlCommand(string_select_left_additional_group_name, conn);
                        SqlCommand sql_select_right_additional_group_name = new SqlCommand(string_select_right_additional_group_name, conn);
                        conn.Open();
                        using (SqlDataReader AdditionalLeftReader = sql_select_left_additional_group_name.ExecuteReader())
                        {
                            while (AdditionalLeftReader.Read())
                            {
                                ListBoxLeft.Items.Add(AdditionalLeftReader["OrgAdtCode"].ToString() + " | " + AdditionalLeftReader["OrgAdtName"].ToString());
                            }
                        }

                        using (SqlDataReader AdditionalRightReader = sql_select_right_additional_group_name.ExecuteReader())
                        {
                            while (AdditionalRightReader.Read())
                            {
                                ListBoxRight.Items.Add(AdditionalRightReader["OrgAdtGroupCode"].ToString() + " | " + AdditionalRightReader["OrgAdtGroupName"].ToString());
                            }
                        }

                        conn2.Close();
                    }
                    conn.Close();
                }
            }//end of if(!IsPostBack)
        }

        protected void OnClickBtnLeftToRight(object sender, EventArgs e)
        {
            StringBuilder HtmlDropdown = new StringBuilder();
            var period_id = Request.QueryString["period_id"];
            int loop = 0;
            string string_select_all_period = "SELECT * FROM BSC_Period WHERE data_status='exist' ORDER BY Start_Period ASC";
            //string sql_string_active = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                SqlCommand sql_select_all_period = new SqlCommand(string_select_all_period, conn);
                conn.Open();

                using (SqlDataReader PeriodReader = sql_select_all_period.ExecuteReader())
                {
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
                            if (loop == 0)
                            {
                                if (period_id == null)
                                {
                                    string string_select_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//jika NULL, langsung pilih yang Active jika ada
                                    SqlCommand sql_select_active = new SqlCommand(string_select_active, conn);
                                    using (SqlDataReader ActiveReader = sql_select_active.ExecuteReader())
                                    {
                                        if (ActiveReader.HasRows)
                                        {
                                            while (ActiveReader.Read())
                                            {
                                                string startdate_to_date3, enddate_to_date3, start_end_date3;//butuh agar jam nya ga keluar!!
                                                DateTime start_date3 = Convert.ToDateTime(ActiveReader["Start_Period"]);
                                                DateTime end_date3 = Convert.ToDateTime(ActiveReader["End_Period"]);
                                                startdate_to_date3 = start_date3.ToString("MMM");//aslinya MM-dd-yyyy
                                                enddate_to_date3 = end_date3.ToString("MMM yyyy");//ubah format tanggal!
                                                start_end_date3 = startdate_to_date3 + " - " + enddate_to_date3;
                                                HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                                HtmlDropdown.Append(start_end_date3 + "&nbsp;<span class='caret'></span>");
                                                HtmlDropdown.Append("</button>");
                                                HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                                period_id = ActiveReader["Period_ID"].ToString();
                                            }
                                        }
                                        else
                                        {
                                            HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                            HtmlDropdown.Append(start_end_date + "&nbsp;<span class='caret'></span>");
                                            HtmlDropdown.Append("</button>");
                                            HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                            period_id = PeriodReader["Period_ID"].ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    string string_selected_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                                    SqlCommand sql_selected_period = new SqlCommand(string_selected_period, conn);
                                    using (SqlDataReader SelectedPeriodReader = sql_selected_period.ExecuteReader())
                                    {
                                        while (SelectedPeriodReader.Read())
                                        {
                                            string startdate_to_date2, enddate_to_date2, start_end_date2;//butuh agar jam nya ga keluar!!
                                            DateTime start_date2 = Convert.ToDateTime(SelectedPeriodReader["Start_Period"]);
                                            DateTime end_date2 = Convert.ToDateTime(SelectedPeriodReader["End_Period"]);
                                            startdate_to_date2 = start_date2.ToString("MMM");//aslinya MM-dd-yyyy
                                            enddate_to_date2 = end_date2.ToString("MMM yyyy");//ubah format tanggal!
                                            start_end_date2 = startdate_to_date2 + " - " + enddate_to_date2;
                                            HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                            HtmlDropdown.Append(start_end_date2 + "&nbsp;<span class='caret'></span>");
                                            HtmlDropdown.Append("</button>");
                                            HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                        }
                                    }
                                }
                                loop++;
                            }
                            HtmlDropdown.Append("<li role='presentation'><a role='menuitem' href='link_financial_group.aspx?period_id=" + PeriodReader["Period_ID"] + "&group_index=" + DropDownListGroup.SelectedIndex + "'>");
                            HtmlDropdown.Append(start_end_date + "</a></li>");
                        }
                        HtmlDropdown.Append("</ul>");
                    }
                    else
                    {
                        HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                        HtmlDropdown.Append("Period Not Set&nbsp;<span class='caret'></span>");
                        HtmlDropdown.Append("</button>");
                        HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                        HtmlDropdown.Append("</ul>");
                        SpanDone.Attributes.Add("class", "btn btn-done-linking btn-done-linking-container done-button disabled");
                    }
                }
                conn.Close();
            }
            PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlDropdown.ToString() });

            for (int i = ListBoxLeft.Items.Count - 1; i >= 0; i--)
            {
                if (ListBoxLeft.Items[i].Selected)
                {
                    ListBoxRight.Items.Add(ListBoxLeft.Items[i]);
                    ListBoxLeft.Items.RemoveAt(i);
                }
            }
        }

        protected void OnClickBtnRightToLeft(object sender, EventArgs e)
        {
            StringBuilder HtmlDropdown = new StringBuilder();
            var period_id = Request.QueryString["period_id"];
            int loop = 0;
            string string_select_all_period = "SELECT * FROM BSC_Period WHERE data_status='exist' ORDER BY Start_Period ASC";
            //string sql_string_active = "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                SqlCommand sql_select_all_period = new SqlCommand(string_select_all_period, conn);
                conn.Open();

                using (SqlDataReader PeriodReader = sql_select_all_period.ExecuteReader())
                {
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
                            if (loop == 0)
                            {
                                if (period_id == null)
                                {
                                    string string_select_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//jika NULL, langsung pilih yang Active jika ada
                                    SqlCommand sql_select_active = new SqlCommand(string_select_active, conn);
                                    using (SqlDataReader ActiveReader = sql_select_active.ExecuteReader())
                                    {
                                        if (ActiveReader.HasRows)
                                        {
                                            while (ActiveReader.Read())
                                            {
                                                string startdate_to_date3, enddate_to_date3, start_end_date3;//butuh agar jam nya ga keluar!!
                                                DateTime start_date3 = Convert.ToDateTime(ActiveReader["Start_Period"]);
                                                DateTime end_date3 = Convert.ToDateTime(ActiveReader["End_Period"]);
                                                startdate_to_date3 = start_date3.ToString("MMM");//aslinya MM-dd-yyyy
                                                enddate_to_date3 = end_date3.ToString("MMM yyyy");//ubah format tanggal!
                                                start_end_date3 = startdate_to_date3 + " - " + enddate_to_date3;
                                                HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                                HtmlDropdown.Append(start_end_date3 + "&nbsp;<span class='caret'></span>");
                                                HtmlDropdown.Append("</button>");
                                                HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                                period_id = ActiveReader["Period_ID"].ToString();
                                            }
                                        }
                                        else
                                        {
                                            HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                            HtmlDropdown.Append(start_end_date + "&nbsp;<span class='caret'></span>");
                                            HtmlDropdown.Append("</button>");
                                            HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                            period_id = PeriodReader["Period_ID"].ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    string string_selected_period = "SELECT * FROM BSC_Period WHERE Period_ID=" + period_id + "";
                                    SqlCommand sql_selected_period = new SqlCommand(string_selected_period, conn);
                                    using (SqlDataReader SelectedPeriodReader = sql_selected_period.ExecuteReader())
                                    {
                                        while (SelectedPeriodReader.Read())
                                        {
                                            string startdate_to_date2, enddate_to_date2, start_end_date2;//butuh agar jam nya ga keluar!!
                                            DateTime start_date2 = Convert.ToDateTime(SelectedPeriodReader["Start_Period"]);
                                            DateTime end_date2 = Convert.ToDateTime(SelectedPeriodReader["End_Period"]);
                                            startdate_to_date2 = start_date2.ToString("MMM");//aslinya MM-dd-yyyy
                                            enddate_to_date2 = end_date2.ToString("MMM yyyy");//ubah format tanggal!
                                            start_end_date2 = startdate_to_date2 + " - " + enddate_to_date2;
                                            HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                                            HtmlDropdown.Append(start_end_date2 + "&nbsp;<span class='caret'></span>");
                                            HtmlDropdown.Append("</button>");
                                            HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                                        }
                                    }
                                }
                                loop++;
                            }
                            HtmlDropdown.Append("<li role='presentation'><a role='menuitem' href='link_financial_group.aspx?period_id=" + PeriodReader["Period_ID"] + "&group_index=" + DropDownListGroup.SelectedIndex + "'>");
                            HtmlDropdown.Append(start_end_date + "</a></li>");
                        }
                        HtmlDropdown.Append("</ul>");
                    }
                    else
                    {
                        HtmlDropdown.Append("<button class='btn btn-default dropdown-toggle' type='button' data-toggle='dropdown' aria-expanded='false'>");
                        HtmlDropdown.Append("Period Not Set&nbsp;<span class='caret'></span>");
                        HtmlDropdown.Append("</button>");
                        HtmlDropdown.Append("<ul class='dropdown-menu customize-btn-dropdown customize-btn-dropdown-width' role='menu'>");
                        HtmlDropdown.Append("</ul>");
                        SpanDone.Attributes.Add("class", "btn btn-done-linking btn-done-linking-container done-button disabled");
                    }
                }
                conn.Close();
            }
            PlaceHolderPeriod.Controls.Add(new Literal { Text = HtmlDropdown.ToString() });

            for (int i = ListBoxRight.Items.Count - 1; i >= 0; i--)
            {
                if (ListBoxRight.Items[i].Selected)
                {
                    ListBoxLeft.Items.Add(ListBoxRight.Items[i]);
                    ListBoxRight.Items.RemoveAt(i);
                }
            }
        }

        protected void OnSelectGroupName(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var period_id = Request.QueryString["period_id"];
            if (period_id == null)
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    string sql_string_active = "SELECT * FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";//UNTUK CARI YANG AKTIF
                    SqlCommand sql_active = new SqlCommand(sql_string_active, conn);
                    conn.Open();
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
                    conn.Close();
                }
            }
            Response.Redirect("" + baseUrl + "link_financial_group.aspx?period_id=" + period_id + "&group_index=" + DropDownListGroup.SelectedIndex + "");
        }

        protected void OnClickDone(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var period_id = Request.QueryString["period_id"];
            int additional_group_counter;//digunakan untuk mengetahui apakah user add additional group baru ATAU delete additional group yang ada
            string user_create, date_create, user_update, date_update;
            string selected_group_name = DropDownListGroup.SelectedValue;

            user_create = Session["user_name"].ToString();
            user_update = Session["user_name"].ToString();
            date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (ListBoxRight.Items.Count == 0)//jika Additional Group tidak ada, tidak bisa insert!
            {
                Response.Redirect("link_financial_group.aspx?period_id=" + period_id + "&group_index=" + DropDownListGroup.SelectedIndex + "&empty_flag=1");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    //select period_ID yang Active terlebih dahulu ketika pertama kali Load Page
                    //string select_active_period = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active'";
                    //SqlCommand sql_select_active_period = new SqlCommand(select_active_period, conn);
                    //if (period_id == null) period_id = sql_select_active_period.ExecuteScalar().ToString();

                    if (period_id == null)
                    {
                        string string_select_active_period = "SELECT Period_ID FROM BSC_Period WHERE Period_Status='Active' AND data_status='exist'";
                        SqlCommand sql_select_active_period = new SqlCommand(string_select_active_period, conn);
                        using (SqlDataReader ActiveReader = sql_select_active_period.ExecuteReader())
                        {
                            if (ActiveReader.HasRows)
                            {
                                while (ActiveReader.Read())
                                {
                                    period_id = ActiveReader["Period_ID"].ToString();
                                }
                            }
                            else
                            {
                                period_id = "1";
                            }
                        }
                    }

                    string count_additional_group = "SELECT COUNT(Group_Name) FROM ScorecardGroupLink WHERE Period_ID=" + period_id + " AND Group_Name='" + selected_group_name + "'";

                    SqlCommand sql_count_additional_group = new SqlCommand(count_additional_group, conn);
                    additional_group_counter = int.Parse(sql_count_additional_group.ExecuteScalar().ToString());

                    //jika ListBox kanan lebih banyak dari data yang ada di DB, berarti user ADD Additional Group baru
                    if (ListBoxRight.Items.Count > additional_group_counter)
                    {
                        string insert_to_financial_group_link = "INSERT INTO ScorecardGroupLink VALUES(@group_name, @additional_code, @additional_name, @user_create, "
                                                              + "@user_update, @period_id, @date_create, @date_update, @group_id)";
                        for (int i = 0; i < ListBoxRight.Items.Count; i++)
                        {
                            string check_existed_additional_name = "SELECT OrgAdtGroupCode FROM ScorecardGroupLink WHERE Period_ID=" + period_id + " AND OrgAdtGroupCode='" + ListBoxRight.Items[i].ToString().Substring(0, 3) + "' AND Group_Name='" + selected_group_name + "'";
                            string get_additional_group = "SELECT * FROM ScorecardGroupLink WHERE Period_ID="+period_id+" AND Group_Name='"+selected_group_name+"'";
                            SqlCommand sql_get_additional_group = new SqlCommand(get_additional_group, conn);
                            SqlCommand sql_insert_to_financial_group_link = new SqlCommand(insert_to_financial_group_link, conn);
                            SqlCommand sql_check_additional_name = new SqlCommand(check_existed_additional_name, conn);

                            using (SqlDataReader AdditionalGroupReader = sql_check_additional_name.ExecuteReader())
                            {
                                if (AdditionalGroupReader.HasRows)
                                {
                                    while (AdditionalGroupReader.Read())
                                    {
                                        if (!AdditionalGroupReader["OrgAdtGroupCode"].Equals(ListBoxRight.Items[i].ToString().Substring(0, 3)))
                                        {
                                            //DELETE Group Name tersebut dari Database
                                            string delete_group = "DELETE FROM ScorecardGroupLink WHERE OrgAdtGroupCode='" + AdditionalGroupReader["OrgAdtGroupCode"] + "' AND Period_ID=" + period_id + "";
                                            SqlCommand sql_delete_group = new SqlCommand(delete_group, conn);
                                            sql_delete_group.ExecuteNonQuery();

                                            //INSERT Group Name yang baru
                                            string insert_group = "INSERT INTO ScorecardGroupLink VALUES(@group, @add_code, @add_name, @u_create, @u_update, @p_id, @d_create, @d_update, @group_id)";
                                            SqlCommand sql_insert_group = new SqlCommand(insert_group, conn);

                                            sql_insert_group.Parameters.Clear();
                                            sql_insert_group.Parameters.AddWithValue("@group", selected_group_name);
                                            sql_insert_group.Parameters.AddWithValue("@add_code", ListBoxRight.Items[i].ToString().Substring(0, 3));//ambil 3 kata pertama / Code
                                            sql_insert_group.Parameters.AddWithValue("@add_name", ListBoxRight.Items[i].ToString().Substring(6, ListBoxRight.Items[i].ToString().Length - 6));//ambil nama additional-nya saja
                                            sql_insert_group.Parameters.AddWithValue("@u_create", user_create);
                                            sql_insert_group.Parameters.AddWithValue("@u_update", user_update);
                                            sql_insert_group.Parameters.AddWithValue("@d_create", date_create);
                                            sql_insert_group.Parameters.AddWithValue("@d_update", date_update);
                                            sql_insert_group.Parameters.AddWithValue("@p_id", period_id);
                                            sql_insert_group.Parameters.AddWithValue("@group_id", DropDownListGroup.SelectedIndex + 1);
                                            sql_insert_group.ExecuteNonQuery();
                                        }
                                    }//end of WHILE(AdditionalGroupReader.Read())
                                }
                                else
                                {
                                    sql_insert_to_financial_group_link.Parameters.Clear();//reset query pada saat LOOPING
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@group_name", selected_group_name);
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@additional_code", ListBoxRight.Items[i].ToString().Substring(0, 3));//ambil 3 kata pertama / Code
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@additional_name", ListBoxRight.Items[i].ToString().Substring(6, ListBoxRight.Items[i].ToString().Length - 6));//ambil nama additional-nya saja
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@user_create", user_create);
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@user_update", user_update);
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@date_create", date_create);
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@date_update", date_update);
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@period_id", period_id);
                                    sql_insert_to_financial_group_link.Parameters.AddWithValue("@group_id", DropDownListGroup.SelectedIndex + 1);
                                    sql_insert_to_financial_group_link.ExecuteNonQuery();
                                }
                            }
                        }

                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Additional Group(s) Linked!'); window.location='" + baseUrl + "link_financial_group.aspx?period_id=" + period_id + "&group_index=" + DropDownListGroup.SelectedIndex + "';", true);
                    }
                    else if (ListBoxRight.Items.Count <= additional_group_counter)
                    {
                        string insert_to_financial_group = "INSERT INTO ScorecardGroupLink VALUES(@group, @add_code, @add_name, @u_create, @u_update, @p_id, getdate(), getdate(), @group_id)";
                        string delete_exist_group_name = "DELETE FROM ScorecardGroupLink WHERE Group_Name='" + selected_group_name + "' AND Period_ID=" + period_id + "";
                        SqlCommand sql_delete_group_name = new SqlCommand(delete_exist_group_name, conn);
                        SqlCommand sql_insert_to_financial_group = new SqlCommand(insert_to_financial_group, conn);
                        sql_delete_group_name.ExecuteNonQuery();

                        for (int i = 0; i < ListBoxRight.Items.Count; i++)
                        {
                            sql_insert_to_financial_group.Parameters.Clear();//reset query pada saat LOOPING
                            sql_insert_to_financial_group.Parameters.AddWithValue("@group", selected_group_name);
                            sql_insert_to_financial_group.Parameters.AddWithValue("@add_code", ListBoxRight.Items[i].ToString().Substring(0, 3));//ambil 3 kata pertama / Code
                            sql_insert_to_financial_group.Parameters.AddWithValue("@add_name", ListBoxRight.Items[i].ToString().Substring(6, ListBoxRight.Items[i].ToString().Length - 6));//ambil nama additional-nya saja
                            sql_insert_to_financial_group.Parameters.AddWithValue("@u_create", user_create);
                            //sql_insert_to_financial_group.Parameters.AddWithValue("@d_create", date_create);
                            sql_insert_to_financial_group.Parameters.AddWithValue("@p_id", period_id);
                            sql_insert_to_financial_group.Parameters.AddWithValue("@u_update", user_update);
                           // sql_insert_to_financial_group.Parameters.AddWithValue("@d_update", date_update);
                            sql_insert_to_financial_group.Parameters.AddWithValue("@group_id", DropDownListGroup.SelectedIndex + 1);
                            sql_insert_to_financial_group.ExecuteNonQuery();
                        }
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Additional Group(s) Linked!'); window.location='" + baseUrl + "link_financial_group.aspx?period_id=" + period_id + "&group_index=" + DropDownListGroup.SelectedIndex + "';", true);
                    }
                    conn.Close();
                }
            }//end of ELSE
        }//end of OnClickDone

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