using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlightTicket_Reserve
{
    public partial class PayForm : Form
    {
        double price = 0;
        private DataSet ds_Passenger;
        private int soldTicket_top = 0;
        private int soldTicket_normal = 0;

        public bool Flag {  get; private set; }

        public PayForm(string user, string Fstart, string Fend, string Price, string FliID, string dtime, string pname = "")
        {
            InitializeComponent();
            textBox2.Text = user;
            textBox1.Text = Fstart;
            textBox5.Text = Fend;
            /*label14.Text = Price;*/
            price = Convert.ToDouble( Price );
            
            textBox6.Text = dtime;
            label20.Text = FliID;
            /*comboBox2.Text = SeatLevel;*/
            comboBox1.Text = pname;
            LoadData();

            Flag = false;
        }

        private void LoadData()
        {
            // 填充下拉列表
            DBcon dbCon = new DBcon();
            string sql = $"select PasID, pname, identify, contact from Passenger where [userID] = '{textBox2.Text}'";
            try
            {
                SqlDataReader reader = dbCon.executeQuery(sql);
                ds_Passenger = dbCon.getDataSet(sql, "viewPas");
                // 先清空数据
                comboBox1.Items.Clear();
                foreach (DataRow dr in ds_Passenger.Tables["viewPas"].Rows) 
                {
                    // 填充乘客
                    comboBox1.Items.Add(dr["pname"]);
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } finally
            {
                dbCon.con_close();
            }
            //姓名、证件、联系方式
            foreach (DataRow dr in ds_Passenger.Tables["viewPas"].Rows)
            {
                // 显示乘客详细信息
                if (dr["pname"].ToString() == comboBox1.Text)
                {
                    textBox3.Text = dr["pname"].ToString();
                    textBox12.Text = dr["identify"].ToString();
                    textBox4.Text = dr["contact"].ToString();
                }
            }
        }
        // 选择支付方式
        private void PayForm_Load(object sender, EventArgs e)
        {
            Program.CenterFormOnScreen(this);
            // 为支付选项添加共享事件处理程序
            foreach (RadioButton radioButton in groupBox1.Controls.OfType<RadioButton>())
            {
                radioButton.CheckedChanged += PaymentOption_CheckedChanged;
            }

        }
        private void PaymentOption_CheckedChanged(object sender, EventArgs e)
        {
            // 当用户选择支付选项时，更新 Label 显示的文本
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                label19.Text = radioButton.Text;
                panel2.Visible = true;
            }
        }
        // 确认
        private void button2_Click(object sender, EventArgs e)
        {

            int buy_Top = soldTicket_top;
            int buy_Normal = soldTicket_normal;
            if (listView1.Items.Count != Convert.ToInt32(label21.Text))
            {
                MessageBox.Show("票数与记录不匹配");
            } else if (buy_Top <= 20 && buy_Normal <= 80 )
            {
                //有票
                panel3.Visible = true;
            } else if (buy_Top > 20 || buy_Top < 1 || buy_Normal > 80 || buy_Normal <1)
            {
                panel3.Visible = false;
                MessageBox.Show("余票不足或输入有误");
            }
            
        }
        // 完成支付
        private void button4_Click(object sender, EventArgs e)
        {
            DBcon dBcon = new DBcon();
            try
            {
                // 添加机票
                foreach (ListViewItem item in listView1.Items)
                {
                    // 数据初始化
                    string ok_name = item.SubItems[0].Text;
                    string ok_identify = item.SubItems[1].Text;
                    string SeatLevel = item.SubItems[2].Text;
                    string SNSelect = item.SubItems[3].Text;
                    int ok_FliID = Convert.ToInt32(item.SubItems[4].Text);
                    // 自动生成的，要查表
                    int ok_PayID = 0;
                    int ok_PasID = 0;
                    int ok_SeaID = 0;
                    int ticketTop = 0;
                    int ticketNormal = 0;
                    int num_buy_t = 1;  //一次购买一张

                    // 查余票
                    string sql_top = $"select SeatTop, SeatNomal from Flight where FliID = '{ok_FliID}'";
                    SqlDataReader reader_top = dBcon.executeQuery(sql_top);
                    while (reader_top.Read())
                    {
                        ticketTop = (int)reader_top["SeatTop"];
                        ticketNormal = (int)reader_top["SeatNomal"];
                    }
                    string sql = "";
                    // 判断是否够售出
                    if ((SeatLevel == "头等舱" && ticketTop >= num_buy_t) || (SeatLevel == "经济舱" && ticketNormal >= num_buy_t))
                    {
                        sql = $"SELECT TOP 1 * FROM Payment ORDER BY PayID DESC";
                        SqlDataReader reader = dBcon.executeQuery(sql);
                        while (reader.Read())
                        {
                            ok_PayID = (int)reader["PayID"];
                        }

                        sql = $"select PasID from Passenger where pname = '{ok_name}'";
                        reader = dBcon.executeQuery(sql);
                        while (reader.Read())
                        {
                            ok_PasID = (int)reader["PasID"];
                        }

                        sql = $"select SeaID from Seat where SNumber = '{SNSelect}'";
                        reader = dBcon.executeQuery(sql);
                        while (reader.Read())
                        {
                            ok_SeaID = (int)reader["SeaID"];
                        }

                        sql = $"insert into Ticket(PasID, FliID, PayID, SNSelect, SeaID) values('{ok_PasID}','{ok_FliID}', '{ok_PayID}', '{SNSelect}', '{ok_SeaID}')";
                        dBcon.executeUpdate(sql);

                        if (SeatLevel == "头等舱")
                        {
                            sql = $"update Flight set SeatTop = SeatTop - '{num_buy_t}' where FliID = '{ok_FliID}'";
                        }
                        else if (SeatLevel == "经济舱")
                        {
                            sql = $"update Flight set SeatNomal = SeatNomal - '{num_buy_t}' where FliID = '{ok_FliID}'";
                        }
                        dBcon.executeUpdate(sql);

                    }
                    
                    
                }
                // 添加支付信息
                string ok_PayWay = label19.Text;
                double ok_PayMoney = Convert.ToDouble(label17.Text);
                string sql_pay = $"insert into Payment(PWay, PMoney, Ptime) values('{ok_PayWay}', '{ok_PayMoney}', '{DateTime.Now}')";
                dBcon.executeUpdate(sql_pay);

                // 支付成功
                Flag = true;
                this.Close();
                MessageBox.Show("机票购买成功！");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                dBcon.con_close();
            }
            
            



        }
        // 选择座位
        private void button3_Click(object sender, EventArgs e)
        {
            bool itemExists = false;
            // 遍历ListView的所有项
            foreach (ListViewItem item in listView1.Items)
            {
                string identify = item.SubItems[1].Text;
                if (identify == textBox12.Text)
                {
                    itemExists = true;
                    break;
                }
            }
            if (itemExists) //判断乘客是否重复购票
            {
                MessageBox.Show("该用户已购票");
            } else if (textBox3.Text == "")
            {
                MessageBox.Show("请选择乘客");
            } else 
            {
                SeatSelect seatSelect = new SeatSelect(label20.Text, comboBox2.Text, listView1, textBox12.Text);
                DialogResult dialogResult = seatSelect.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    string selectSeatFromChild = seatSelect.SelectSeat;
                    // 已售出票数
                    soldTicket_top = seatSelect.SoldTicket_Top;
                    soldTicket_normal = seatSelect.SoldTicket_Normal;

                    ListViewItem item = new ListViewItem();
                    item.Text = textBox3.Text;  //姓名
                    item.SubItems.Add(textBox12.Text); //证件号
                    item.SubItems.Add(comboBox2.Text);  //舱位
                    item.SubItems.Add(selectSeatFromChild); // 座位号
                    item.SubItems.Add(label20.Text);  // 航班号
                                                      //价格
                    if (comboBox2.Text == "经济舱")
                    {
                        item.SubItems.Add(price.ToString());
                    }
                    if (comboBox2.Text == "头等舱")
                    {
                        double price_temp = price * 1.5;
                        item.SubItems.Add(price_temp.ToString());
                    }
                    listView1.Items.Add(item);
                }
                
            }

        }
        // 显示选中信息
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (DataRow dr in ds_Passenger.Tables["viewPas"].Rows)
            {
                // 显示乘客详细信息
                if (dr["pname"].ToString() == comboBox1.SelectedItem.ToString())
                {
                    textBox3.Text = dr["pname"].ToString();
                    textBox12.Text = dr["identify"].ToString();
                    textBox4.Text = dr["contact"].ToString();
                }
            }
        }
        // 计算价格
        private void button1_Click(object sender, EventArgs e)
        {
            // 总价
            decimal totalPrice = 0;
            int rowCount = listView1.Items.Count;

            if (listView1.Items.Count > 0)
            {
                // 遍历listview计算价格
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.SubItems.Count >= 6 && decimal.TryParse(item.SubItems[5].Text, out decimal price))
                    {
                        totalPrice += price;
                    }
                }

                label14.Text = $"{totalPrice.ToString("0.00")}";
                label21.Text = $"{rowCount}";

                // 税，随机数值
                Random random = new Random(Environment.TickCount);
                price = Convert.ToDouble(label14.Text);
                //机场税，票价2%~7%
                double randomNumber = random.NextDouble() * (0.07 - 0.02) + 0.02;
                double p_tax = randomNumber * price;
                label12.Text = p_tax.ToString("0.00");
                //燃油税，票价5%~10%
                randomNumber = random.NextDouble() * (0.1 - 0.05) + 0.05;
                double l_tax = randomNumber * price;
                label11.Text = l_tax.ToString("0.00");

                //总金额
                double price_all = price + p_tax + l_tax;
                label10.Text = (price_all).ToString("0.00");
                label17.Text = label10.Text;

                button2.Visible = true;
            }
            else
            {
                MessageBox.Show("请选座位。");
            }
        }
        // 删除购物车记录
        private void button5_Click(object sender, EventArgs e)
        {
            // 确保有选中的项
            if (listView1.SelectedItems.Count > 0)
            {
                // 获取选中项的索引
                int selectedIndex = listView1.SelectedIndices[0];

                // 移除选中的项
                listView1.Items.RemoveAt(selectedIndex);
            }
        }
    }
}
