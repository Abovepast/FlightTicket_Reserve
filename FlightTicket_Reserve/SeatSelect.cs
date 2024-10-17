using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlightTicket_Reserve
{
    public partial class SeatSelect : Form
    {
        
        /*public static string select_seat = "";
        public static int checked_num = 0;*/

        // 公共属性用于获取 checked_num 的值
        public int CheckedNum { get; private set; }

        // 公共属性用于获取 select_seat 的值
        public string SelectSeat { get; private set; }
        public int SoldTicket_Top { get; private set; }
        public int SoldTicket_Normal { get; private set; }

        public static string seat_level = "经济舱";

        public SeatSelect(string FliID, string SeatLevel, ListView listView1, string Identify)
        {
            InitializeComponent();
            label2.Text = FliID;
            seat_level = SeatLevel;
            /*label_tNum.Text = TicketNums.ToString();*/
            /*this.Identify = Identify;*/

            if (seat_level == "头等舱")
            {
                splitContainer1.Panel1.Enabled = true;
                splitContainer1.Panel2.Enabled = false;
            } else if (seat_level == "经济舱")
            {
                splitContainer1.Panel1.Enabled = false;
                splitContainer1.Panel2.Enabled = true;
            }
            LoadData(listView1);
        }

        private void LoadData(ListView listView1)
        {
            SoldTicket_Top = 0;
            SoldTicket_Normal = 0;
            DBcon dBcon = new DBcon();
            try
            {
                string sql = $"select SNSelect from Ticket where FliID = '{label2.Text}'";
                SqlDataReader reader = dBcon.executeQuery(sql);
                while (reader.Read())
                {
                    string soldSeat = reader["SNSelect"].ToString();
                    foreach (Control control in splitContainer1.Panel1.Controls)
                    {
                        if (control is CheckBox checkBox && checkBox.Text == soldSeat)
                        {
                            SoldTicket_Top += 1;
                            checkBox.Checked = true;
                            checkBox.Enabled = false;
                        }
                    }
                    foreach (Control control in splitContainer1.Panel2.Controls)
                    {
                        if (control is CheckBox checkBox && checkBox.Text == soldSeat)
                        {
                            SoldTicket_Normal += 1;
                            checkBox.Checked = true;
                            checkBox.Enabled = false;
                        }
                    }
                }

                // 遍历ListView的所有项
                foreach (ListViewItem item in listView1.Items)
                {
                    // 获取座位号字段的值（假设座位号在第二列）
                    string seatNumber = item.SubItems[3].Text;
                    foreach (Control control in splitContainer1.Panel1.Controls)
                    {
                        if (control is CheckBox checkBox && checkBox.Text == seatNumber)
                        {
                            SoldTicket_Top += 1;
                            checkBox.Checked = true;
                            checkBox.Enabled = false;
                        }
                    }

                    foreach (Control control in splitContainer1.Panel2.Controls)
                    {
                        if (control is CheckBox checkBox && checkBox.Text == seatNumber)
                        {
                            SoldTicket_Normal += 1;
                            checkBox.Checked = true;
                            checkBox.Enabled = false;
                        }
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } finally
            {
                dBcon.con_close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectSeat = "";
            CheckedNum = 0;

            if (seat_level == "头等舱")
            {
                foreach (Control control in splitContainer1.Panel1.Controls)
                {
                    if (control is CheckBox checkBox && checkBox.Checked && checkBox.Enabled == true)
                    {
                        CheckedNum += 1;
                        SelectSeat += (control as CheckBox).Text;
                    }
                }
            }

            if (seat_level == "经济舱")
            {
                foreach (Control control in splitContainer1.Panel2.Controls )
                {
                    if (control is CheckBox checkBox && checkBox.Checked && checkBox.Enabled == true)
                    {
                        CheckedNum += 1;
                        SelectSeat += (control as CheckBox).Text;
                    }
                }
            }

            if (CheckedNum > Convert.ToInt32(label_tNum.Text) || CheckedNum < Convert.ToInt32(label_tNum.Text))
            {
                MessageBox.Show("选择数量错误，一次请选择" + label_tNum.Text + "张票。");
                SelectSeat = "";
            } else 
            {
                this.DialogResult = DialogResult.OK; 
                this.Close(); 
            }

        }

        private void SeatSelect_Load(object sender, EventArgs e)
        {
            Program.CenterFormOnScreen(this);
        }
    }
}
