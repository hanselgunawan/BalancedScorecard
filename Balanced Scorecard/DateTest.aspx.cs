using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Balanced_Scorecard
{
    public partial class DateTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void OnClickSave(object sender, EventArgs e)
        {
            int start_month, end_month, month_counter;
            StringBuilder Month_sb = new StringBuilder();
            DateTime start_date = Convert.ToDateTime(StartDate.Value);
            DateTime end_date = Convert.ToDateTime(EndDate.Value);
            start_month = start_date.Month;
            end_month = end_date.Month;
            month_counter = start_month;

            while(month_counter < end_month)
            {
                if (month_counter == start_month)
                {
                    month_counter = month_counter + 2; 
                    //month_counter = month_counter + 5;
                }
                else
                {
                    month_counter = month_counter + 3;
                    //month_counter = month_counter + 6;
                }
                if (month_counter > end_month) break;
                Month_sb.Append("" + month_counter.ToString() + ", ");
            }

            LabelDate.Text = Month_sb.ToString();
        }
    }
}