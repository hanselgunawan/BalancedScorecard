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
    public partial class link_detail_item : System.Web.UI.Page
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
                string group_name, startdate_to_date, enddate_to_date;
                var page = Request.QueryString["page"];
                var header_id = Request.QueryString["header_id"];
                var period_id = Request.QueryString["period_id"];

                //link untuk breadcrumb
                financial_measures_breadcrumb.Attributes.Add("a href", "financial_scorecard.aspx?page="+page+"&id=" + period_id + "");

                //cancel linking
                btnCancelLinking.Attributes.Add("href", "financial_scorecard.aspx?page=" + page + "&id=" + period_id + "");

                string select_detail = "SELECT FinancialMeasure FROM FinancialMeasures_Detail WHERE FinancialHeader_ID="+header_id+" AND FinancialType='Share' AND FinancialLinked=0";
                string select_header = "SELECT FinancialHeader_Group FROM FinancialMeasures_Header WHERE FinancialHeader_ID="+header_id+"";
                string select_period = "SELECT Start_Period, End_Period FROM BSC_Period WHERE Period_ID=" + period_id + "";
                string string_select_access_right = "SELECT Access_Rights_Code FROM AccessRightsPage "//mengambil hak akses berdasarkan UserGroup
                                           + "WHERE Access_Rights_Code NOT IN "
                                           + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                           + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";

                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    SqlCommand sql_select_header = new SqlCommand(select_header, conn);
                    SqlCommand sql_select_detail = new SqlCommand(select_detail,conn);
                    SqlCommand sql_select_period = new SqlCommand(select_period, conn);
                    SqlCommand sql_access_rights = new SqlCommand(string_select_access_right, conn);
                    conn.Open();
                    group_name = sql_select_header.ExecuteScalar().ToString();

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

                    using (SqlDataReader DetailReader = sql_select_detail.ExecuteReader())
                    {
                        while (DetailReader.Read())
                        {
                            ListBoxLeft.Items.Add(DetailReader["FinancialMeasure"].ToString());
                        }
                        DetailReader.Dispose();
                        DetailReader.Close();
                    }

                    using (SqlDataReader PeriodReader = sql_select_period.ExecuteReader())
                    {
                        while (PeriodReader.Read())
                        {
                            DateTime start_date = Convert.ToDateTime(PeriodReader["Start_Period"]);
                            DateTime end_date = Convert.ToDateTime(PeriodReader["End_Period"]);
                            startdate_to_date = start_date.ToString("MMM");//aslinya MM-dd-yyyy
                            enddate_to_date = end_date.ToString("MMM yyyy");//ubah format tanggal!
                            LabelPeriod.Text = startdate_to_date + " - " + enddate_to_date;
                        }
                        PeriodReader.Dispose();
                        PeriodReader.Close();
                    }

                    LabelBreadcrumb.Text = group_name;
                    LabelTitle.Text = group_name;

                    conn.Close();
                }
            }
        }

        protected void OnClickBtnLeftToRight(object sender, EventArgs e)
        {
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
            for (int i = ListBoxRight.Items.Count - 1; i >= 0; i--)
            {
                if (ListBoxRight.Items[i].Selected)
                {
                    ListBoxLeft.Items.Add(ListBoxRight.Items[i]);
                    ListBoxRight.Items.RemoveAt(i);
                }
            }
        }

        protected void OnClickDone(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            var page = Request.QueryString["page"];
            var header_id = Request.QueryString["header_id"];
            var period_id = Request.QueryString["period_id"];
            bool rating_is_zero = true;
            int j = 0;
            string user_create, date_create, user_update, date_update;

            user_create = Session["user_name"].ToString();
            user_update = Session["user_name"].ToString();
            date_create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            date_update = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (ListBoxRight.Items.Count <= 1)
            {
                ErrorMessageLink.Attributes.Add("style", "width:300px;margin-bottom:30px;visibility:visible !important");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(str_connect))
                {
                    conn.Open();
                    for (int i = 0; i < ListBoxRight.Items.Count; i++)
                    {
                        string sql_select_id = "SELECT * FROM FinancialMeasures_Detail "
                                             + "WHERE FinancialMeasure='" + ListBoxRight.Items[i].ToString() + "' "
                                             + "AND FinancialHeader_ID=" + header_id + "";
                        SqlCommand sql_command_select_id = new SqlCommand(sql_select_id, conn);
                        using (SqlDataReader DetailReader = sql_command_select_id.ExecuteReader())
                        {
                            while (DetailReader.Read())
                            {
                                if (j < ListBoxRight.Items.Count)
                                {
                                    string sql_insert_to_link = "INSERT INTO FinancialLink VALUES(@detail_id,(SELECT ISNULL(MAX(FinancialLinked),0)+1-" + j + " FROM FinancialLink), "
                                                              + "@weight_value,@user_create,@user_update, @date_create, @date_update)";//INSERT ke tabel FinancialLink terlebih dahulu
                                    string sql_update_detail = "UPDATE FinancialMeasures_Detail SET "//ambil FinancialLinked yang terakhir di-insert ke dalam tabel FinancialLink
                                                              +"FinancialMeasures_Detail.FinancialLinked=(SELECT ISNULL(MAX(FinancialLinked),0) "
                                                              +"FROM FinancialLink), FinancialMeasures_Detail.FinancialWeight=@weight_value_update "
                                                              +"FROM FinancialMeasures_Detail, FinancialLink WHERE "
                                                              +"FinancialMeasures_Detail.FinancialDetail_ID=" + DetailReader["FinancialDetail_ID"].ToString() + "";
                                    int finance_detail_id;
                                    float weight;

                                    finance_detail_id = int.Parse(DetailReader["FinancialDetail_ID"].ToString());
                                    weight = float.Parse(inputLinkWeight.Value);
                                    SqlCommand sql_command_insert = new SqlCommand(sql_insert_to_link, conn);
                                    SqlCommand sql_command_update = new SqlCommand(sql_update_detail, conn);
                                    sql_command_insert.Parameters.AddWithValue("@detail_id", finance_detail_id);
                                    sql_command_insert.Parameters.AddWithValue("@weight_value", weight);
                                    sql_command_insert.Parameters.AddWithValue("@user_create", user_create);
                                    sql_command_insert.Parameters.AddWithValue("@user_update", user_update);
                                    sql_command_insert.Parameters.AddWithValue("@date_create", date_create);
                                    sql_command_insert.Parameters.AddWithValue("@date_update", date_update);

                                    sql_command_update.Parameters.AddWithValue("@weight_value_update", weight);
                                    sql_command_insert.ExecuteNonQuery();
                                    sql_command_update.ExecuteNonQuery();
                                }
                            }
                        }
                        j++;
                    }

                    //cek apakah angka Rating dari kedua Financial Measure yang di-Linked memiliki nilai 0 atau tidak
                    //jika bukan 0, maka perlu di-update langsung FinancialScore-nya
                    string string_check_rating = "SELECT FinancialRating FROM FinancialMeasures_Detail "
                                                + "WHERE FinancialLinked=(SELECT TOP(1) FinancialLinked FROM FinancialLink ORDER BY FinancialLink_ID DESC)";
                    SqlCommand sql_check_rating = new SqlCommand(string_check_rating, conn);
                    using (SqlDataReader RatingReader = sql_check_rating.ExecuteReader())
                    {
                        while (RatingReader.Read())
                        {
                            if (RatingReader["FinancialRating"].ToString() != "0")
                            {
                                rating_is_zero = false;
                                break;
                            }
                            else
                            {
                                rating_is_zero = true;
                            }
                        }
                        RatingReader.Dispose();
                        RatingReader.Close();
                    }

                    if (rating_is_zero == false)
                    {
                        string string_update_detail_score = "UPDATE FinancialMeasures_Detail "
                                                           + "SET FinancialScore = ROUND(((SELECT SUM(FinancialRating) FROM FinancialMeasures_Detail "
                                                           + "WHERE FinancialLinked=(SELECT TOP(1) FinancialLinked FROM FinancialLink ORDER BY FinancialLink_ID DESC))"
                                                           + "/(SELECT COUNT(FinancialDetail_ID) FROM FinancialMeasures_Detail "
                                                           + "WHERE FinancialLinked=(SELECT TOP(1) FinancialLinked FROM FinancialLink ORDER BY FinancialLink_ID DESC)))"
                                                           + "*(SELECT TOP(1) FinancialWeight FROM FinancialMeasures_Detail "
                                                           + "WHERE FinancialLinked=(SELECT TOP(1) FinancialLinked FROM FinancialLink "
                                                           + "ORDER BY FinancialLink_ID DESC))*0.01,2), user_update=@user_update, date_update=@date_update "
                                                           + "WHERE FinancialLinked= (SELECT TOP(1) FinancialLinked FROM FinancialLink ORDER BY FinancialLink_ID DESC)";
                        SqlCommand sql_update_detail_score = new SqlCommand(string_update_detail_score, conn);
                        sql_update_detail_score.Parameters.AddWithValue("@user_update", user_update);
                        sql_update_detail_score.Parameters.AddWithValue("@date_update", date_update);
                        sql_update_detail_score.ExecuteNonQuery();
                    }

                    
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "redirect", "alert('Measures Linked!'); window.location='" + baseUrl + "financial_scorecard.aspx?page="+page+"&id=" + period_id + "';", true);
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