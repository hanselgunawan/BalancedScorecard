using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class user_access_rights : System.Web.UI.Page
    {
        //source: stackoverflow.com/questions/4720007/edit-row-in-gridview
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        private DataTable CreateDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Access_Rights_Code");
            dt.Columns.Add("Description");
            //dt.AcceptChanges();
            return dt;
        }

        private DataTable AddRow(GridViewRow gvRow, DataTable dt)
        {
            DataRow[] dr = dt.Select("Access_Rights_Code = '" + gvRow.Cells[1].Text + "'");
            if (dr.Length <= 0)
            {
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["Access_Rights_Code"] = gvRow.Cells[1].Text;
                //dt.AcceptChanges();
            }
            return dt;
        }

        private DataTable RemoveRow(GridViewRow gvRow, DataTable dt)
        {
            DataRow[] dr = dt.Select("Access_Rights_Code = '" + gvRow.Cells[1].Text + "'");
            if (dr.Length > 0)
            {
                dt.Rows.Remove(dr[0]);
                //dt.AcceptChanges();
            }
            return dt;
        }

        private void BindPrimaryGrid()
        {
            string query = "SELECT * FROM AccessRightsPage";
            SqlConnection con = new SqlConnection(str_connect);
            SqlDataAdapter sda = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            sda.Fill(dt);//mengisi GridView dengan data dari DB
            GridView1.DataSource = dt;//memasukkan data dari DB ke Gridview
            GridView1.DataBind();

        }

        private void GetData()
        {
            DataTable dt;
            if (ViewState["SelectedRecords"] != null)//jika sudah ada yang di-checked
                dt = (DataTable)ViewState["SelectedRecords"];
            else//jika belum ada yang checked sama sekali
                dt = CreateDataTable();
            CheckBox chkAll = (CheckBox)GridView1.HeaderRow
                                .Cells[0].FindControl("chkAll");
            for (int i = 0; i < GridView1.Rows.Count; i++)
            {
                if (chkAll.Checked)
                {
                    dt = AddRow(GridView1.Rows[i], dt);
                }
                else
                {
                    CheckBox chk = (CheckBox)GridView1.Rows[i]
                                    .Cells[0].FindControl("chk");
                    if (chk.Checked)
                    {
                        dt = AddRow(GridView1.Rows[i], dt);
                    }
                    else
                    {
                        dt = RemoveRow(GridView1.Rows[i], dt);
                    }
                }
            }
            ViewState["SelectedRecords"] = dt;
        }

        private void SetData()
        {
            CheckBox chkAll = (CheckBox)GridView1.HeaderRow.Cells[0].FindControl("chkAll");
            chkAll.Checked = true;
            if (ViewState["SelectedRecords"] != null)//jika sudah ada yang di checked
            {
                DataTable dt = (DataTable)ViewState["SelectedRecords"];
                for (int i = 0; i < GridView1.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)GridView1.Rows[i].Cells[0].FindControl("chk");
                    if (chk != null)
                    {
                        DataRow[] dr = dt.Select("Access_Rights_Code = '" + GridView1.Rows[i].Cells[1].Text + "'");
                        chk.Checked = dr.Length > 0;
                        if (!chk.Checked)//jika ditemukan satu checkbox yang tidak di checked, Header Checkbox tidak di-checked
                        {
                            chkAll.Checked = false;
                        }
                    }
                }
            }
        }

        protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            GetData();
            SetData();
        }

        protected void OnPaging(object sender, GridViewPageEventArgs e)
        {
            GetData();//mengambil data2 yang dicentang
            GridView1.PageIndex = e.NewPageIndex;
            BindPrimaryGrid();//gabungin data
            SetData();//harus di set setelah di bind
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
                ((Label)Master.FindControl("LabelUsername")).Text = Session["user_name"].ToString();//untuk akses Master Page

                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan
                                           + "WHERE Access_Rights_Code NOT IN "                       //UserGroup
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                string string_check_access_page = "SELECT Access_Rights_Code FROM GroupAccessRights "//untuk cek, apakah dia boleh akses halaman ini
                                                + "WHERE Access_Rights_Code='user_access_rights' AND "//jika diakses secara paksa
                                                + "UserGroup_ID=" + Session["user_role"].ToString() + "";

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
                    conn.Close();
                }

                BindPrimaryGrid();
            }

            if (!IsPostBack)
            {
                string query = "SELECT Access_Rights_Code FROM [GroupAccessRights] WHERE UserGroup_ID='2'";//kalau post back, langsung ke ID=2 (Director)
                SqlConnection con2 = new SqlConnection(str_connect);
                SqlDataAdapter sda2 = new SqlDataAdapter(query, con2);
                DataTable dt = new DataTable();
                sda2.Fill(dt);//untuk mengisi kolom Access Rigths Code pada DataTable
                ViewState["SelectedRecords"] = dt;
                SetData();
            }
        }

        protected void DropDownListUserGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query = "SELECT Access_Rights_Code FROM [GroupAccessRights] WHERE UserGroup_ID='"+DropDownListUserGroup.SelectedValue+"'";
            SqlConnection con2 = new SqlConnection(str_connect);
            SqlDataAdapter sda2 = new SqlDataAdapter(query, con2);
            DataTable dt = new DataTable();
            sda2.Fill(dt);
            ViewState["SelectedRecords"] = dt;
            SetData();
        }

        protected void btnsaveaccess_Click(object sender, EventArgs e)
        {
            string user_create, user_update, date_create, date_update;
            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                user_create = Session["user_name"].ToString();
                user_update = Session["user_name"].ToString();
                date_create = DateTime.Today.ToString("yyyy-MM-dd");//harus M GEDE!
                date_update = DateTime.Today.ToString("yyyy-MM-dd");

                DataTable dt = (DataTable)ViewState["SelectedRecords"];//DataTable untuk table AccessRightsPage

                if (dt.Rows.Count > 0)
                {
                    SqlCommand cmd = null;
                    string query;
                    query = "Select Access_Rights_Code FROM [GroupAccessRights] where UserGroup_ID='" + DropDownListUserGroup.SelectedValue + "'";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt_input = new DataTable();//DataTable untuk Table GroupAccessRights
                    da.Fill(dt_input);

                    foreach (DataRow dr_input in dt_input.Rows)//Table GroupAccessRights yang lama
                    {
                        int flag = 0;
                        foreach (DataRow dr in dt.Rows)//Table GroupAccessRights yang baru
                        {
                            if (dr_input[0].ToString() != dr[0].ToString())
                            {
                                flag++;//jika tidak ditemukan kesamaan diantara dua DataTable, flag++
                            }
                            if (flag == dt.Rows.Count)//jika dulunya di checked tapi sekarang di unchecked, DELETE akses rights nya
                            {
                                query = "DELETE FROM [GroupAccessRights] WHERE UserGroup_ID='" + DropDownListUserGroup.SelectedValue + "' AND  Access_Rights_Code='" + dr_input[0] + "'";
                                cmd = new SqlCommand(query, conn);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    foreach (DataRow dr in dt.Rows)//Table GroupAccessRights yang baru
                    {
                        //cek apakah sudah ada di table GroupAccessRights
                        query = "Select Access_Rights_Code from [GroupAccessRights] WHERE UserGroup_ID='" + DropDownListUserGroup.SelectedValue + "' AND  Access_Rights_Code='" + dr[0] + "'";
                        SqlCommand checkid = new SqlCommand(query, conn);
                        SqlDataReader reader = checkid.ExecuteReader();
                        if (!reader.HasRows)//jika hak akses yang dipilih belum ditemukan pada UserGroup yang mau diubah, INSERT hak akses yang dipilih
                        {                   //cth: Direktur belum ada akses ke dashboard, berarti harus di-INSERT hak akses dashboard
                            string string_insert_user = "INSERT INTO GroupAccessRights(UserGroup_ID,Access_Rights_Code)VALUES(@GroupId, @GroupAccessCode)";
                            SqlCommand sql_insert_user = new SqlCommand(string_insert_user, conn);
                            sql_insert_user.Parameters.AddWithValue("@GroupId", Convert.ToInt16(DropDownListUserGroup.SelectedValue));
                            sql_insert_user.Parameters.AddWithValue("@GroupAccessCode", dr[0]);
                            sql_insert_user.ExecuteNonQuery();
                        }
                    }
                    string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alertMessage", "alert('Access Rights Saved'); window.location='" + baseUrl + "user_access_rights.aspx';", true);
                }

                conn.Close();
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