using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;

namespace Balanced_Scorecard
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void OnClickSendMail(object sender, EventArgs e)
        {
            sendMail();
        }

        public void sendMail()
        {
            var period_id = "1";
            var header_id = "3";
            var detail_id = "1";

            string string_get_user_info = "SELECT ScorecardUser.empNIK, empName, empOrg, OrgAdtGroupName, empJobTitle, LOWER(empEmail) as Email, "
                                        + "empGrade, Group_Name, IndividualHeader_KPI "
                                        + "FROM [Balanced Scorecard].dbo.ScorecardUser "
                                        + "join [Human Capital].dbo.OrgAdtGroup on ScorecardUser.empOrgAdtGroupCode=OrgAdtGroup.OrgAdtCode "
                                        + "join FinancialGroupLink (nolock) on FinancialGroupLink.OrgAdtGroupCode = ScorecardUser.empOrgAdtGroupCode "
                                        + "join BSC_Period on FinancialGroupLink.Period_ID = BSC_Period.Period_ID and BSC_period.Period_ID=" + period_id + " "
                                        + "join IndividualMeasures_Header on ScorecardUser.empNIK = IndividualMeasures_Header.empNIK "
                                        + "WHERE ScorecardUser.empNIK=100 AND IndividualHeader_ID=" + header_id + "";
            string string_get_current_specific_objective = "SELECT * FROM IndividualMeasures_Detail WHERE IndividualDetail_ID=" + detail_id + "";
            string string_get_new_specific_objective = "SELECT * FROM IndividualDetail_RequestChange WHERE IndividualDetail_ID=" + detail_id + "";

            using (SqlConnection conn = new SqlConnection(str_connect))
            {
                conn.Open();
                SqlCommand sql_get_user_info = new SqlCommand(string_get_user_info, conn);
                SqlCommand sql_get_current_specific_objective = new SqlCommand(string_get_current_specific_objective, conn);
                SqlCommand sql_get_new_specific_objective = new SqlCommand(string_get_new_specific_objective, conn);

                StringBuilder sb_subject = new StringBuilder();
                StringBuilder sb_body_introduction = new StringBuilder();
                StringBuilder sb_from_email = new StringBuilder();

                using (SqlDataReader UserReader = sql_get_user_info.ExecuteReader())
                {
                    while (UserReader.Read())
                    {
                        sb_from_email.Append(UserReader["Email"].ToString());
                        sb_subject.Append("Request for Change KPI's Specific Objective (" + UserReader["empName"].ToString() + " - " + UserReader["empNIK"].ToString() + ")");
                        sb_body_introduction.Append("Hello, my name is <b>" + UserReader["empName"].ToString() + "</b> and this is my information: <br/>"
                                + "<b>NIK / <i>Barcode</i></b> : " + UserReader["empNIK"].ToString() + "<br/>"
                                + "<b>Group</b> : " + UserReader["empGrade"].ToString() + "<br/>"
                                + "<b>Organization</b> : " + UserReader["empOrg"].ToString() + "<br/>"
                                + "<b>Additional Organization</b> : " + UserReader["OrgAdtGroupName"].ToString() + "<br/>"
                                + "<b>Job Title</b> : " + UserReader["empJobTitle"].ToString() + "<br/>"
                                + "<b>Grade</b> : " + UserReader["empGrade"].ToString() + "<br/><br/>"
                                + "I would like to change my " + UserReader["IndividualHeader_KPI"].ToString() + "'s Specific Objective from:<br/>");
                    }
                }

                SmtpClient mailclient = new SmtpClient();  //Karena FILE_LOCATION terjadi perubahan setiap di-klik, maka
                using (MailMessage msg = new MailMessage())//harus pake USING untuk CLEAR semua Resource yang pernah dipake
                {
                    /******************** SEND Email TO Users **************************************/
                    msg.Subject = sb_subject.ToString();
                    msg.Body = sb_body_introduction.ToString();
                    msg.From = new MailAddress(sb_from_email.ToString());
                    //msg.To.Add("ishak.kurniawan@gmail.com");//<-- E-Mail atasan
                    msg.IsBodyHtml = true;
                    mailclient.Host = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                    mailclient.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                    mailclient.Send(msg);
                }
                conn.Close();
            }

        }
    }
}