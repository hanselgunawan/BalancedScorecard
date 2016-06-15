using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.HtmlControls;

namespace Balanced_Scorecard
{
    public partial class index : System.Web.UI.Page
    {
        string str_connect = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            string password = TextBoxPassword.Text;
            TextBoxPassword.Attributes.Add("value", password);//agar valuenya tetap ada saat PostBack
            ErrorLoginMessage.Attributes.Add("style", "visibility:hidden");
        }

        protected void OnClickLogin(object sender, EventArgs e)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            string user_nik = TextBoxNIK.Text;
            string entered_pass = "";
            string nik_db = "", password_db = "", status_db = "", enter_pass = "";

            string string_select_user = "SELECT EmpId, empPassword, empStatus FROM ScorecardUser WHERE EmpId='" + user_nik + "'";
            
            SqlConnection conn = new SqlConnection(str_connect);
            SqlCommand sql_select_user = new SqlCommand(string_select_user, conn);
            conn.Open();

            using (SqlDataReader UserReader = sql_select_user.ExecuteReader())
            {
                while (UserReader.Read())
                {
                    nik_db = UserReader["EmpId"].ToString();
                    using (MD5 md5Hash = MD5.Create())
                    {
                        string user_entered_password = GetMd5Hash(md5Hash, TextBoxPassword.Text);
                        entered_pass = user_entered_password;
                    }
                    password_db = UserReader["empPassword"].ToString();
                    enter_pass = entered_pass;
                    status_db = UserReader["empStatus"].ToString();
                }
                UserReader.Dispose();
                UserReader.Close();
            }

            if (nik_db == "")
            {
                ErrorLoginMessage.Attributes.Add("style", "visibility:visible; color:yellow; font-weight:bold");
                ErrorLoginMessage.InnerText = "NIK you entered not found";
            }
            else if (password_db != enter_pass)
            {
                ErrorLoginMessage.Attributes.Add("style", "visibility:visible; color:yellow; font-weight:bold");
                ErrorLoginMessage.InnerText = "Password you entered is incorrect";
            }
            else if (nik_db != "" && password_db != enter_pass)
            {
                ErrorLoginMessage.Attributes.Add("style", "visibility:visible; color:yellow; font-weight:bold");
                ErrorLoginMessage.InnerText = "Password you entered is incorrect";
            }
            else if (nik_db == "" && password_db == "")
            {
                ErrorLoginMessage.Attributes.Add("style", "visibility:visible; color:yellow; font-weight:bold");
                ErrorLoginMessage.InnerText = "NIK and Password Incorrect";
            }
            else if (nik_db != "" && password_db == enter_pass && status_db == "No")
            {
                ErrorLoginMessage.Attributes.Add("style", "visibility:visible; color:yellow; font-weight:bold");
                ErrorLoginMessage.InnerText = "You are an inactive BSC User. Cannot Login.";
            }
            else if (nik_db != "" && password_db == enter_pass && status_db == "Yes")
            {
                Session["user_nik"] = user_nik;
                string user_role = "", user_id = "";
                string string_select_role_and_id = "SELECT ScorecardUser.UserGroup_ID, ScorecardUser.user_id FROM ScorecardUser "
                                                 + "JOIN BSC_UserGroup ON BSC_UserGroup.UserGroup_ID = ScorecardUser.UserGroup_ID "
                                                 + "WHERE EmpId = '" + user_nik + "'";
                SqlCommand sql_select_role_and_id = new SqlCommand(string_select_role_and_id, conn);
                using (SqlDataReader RoleReader = sql_select_role_and_id.ExecuteReader())
                {
                    while (RoleReader.Read())
                    {
                        user_role = RoleReader["UserGroup_ID"].ToString();
                        user_id = RoleReader["user_id"].ToString();
                    }
                }

                Session["user_role"] = user_role;
                Session["user_id"] = user_id;

                string check_access_page = "SELECT TOP(1) Access_Rights_Code FROM AccessRightsPage "
                                         + "WHERE Access_Rights_Code<>'setting' AND Access_Rights_Code<>'scorecard' "
                                         + "AND Access_Rights_Code<>'my_requests' AND Access_Rights_Code<>'approve_requests' "
                                         + "AND Access_Rights_Code<>'request_history' AND Access_Rights_Code<>'user_management' "
                                         + "AND Access_Rights_Code IN "
                                         + "(SELECT Access_Rights_Code FROM GroupAccessRights "
                                         + "WHERE UserGroup_ID=" + Session["user_role"].ToString() + ")";
                SqlCommand sql_check_access_page = new SqlCommand(check_access_page, conn);
                using (SqlDataReader AccessReader = sql_check_access_page.ExecuteReader())
                {
                    if (AccessReader.HasRows)
                    {
                        while (AccessReader.Read())
                        {
                            Response.Redirect("" + baseUrl + "" + AccessReader["Access_Rights_Code"].ToString() + ".aspx");
                        }
                    }
                    else
                    {
                        Response.Redirect("" + baseUrl + "index.aspx");
                    }
                    AccessReader.Dispose();
                    AccessReader.Close();
                }
            }
            conn.Close();
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert string menjadi byte array agar bisa di hash
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Buat StringBuilder buat tampung byte-nya
            // dan membuat string baru.
            StringBuilder sBuilder = new StringBuilder();

            // Loop setiap byte array
            // dan format semua ke nilai hexadecimal
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Mengembalikan nilai Hexadecimal
            return sBuilder.ToString();
        }

    }
}