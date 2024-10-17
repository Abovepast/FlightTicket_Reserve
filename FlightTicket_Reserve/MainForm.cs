using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FlightTicket_Reserve
{
    public partial class MainForm : Form
    {
        string basePrice = "0";
        int seatTopNum = 0;
        int seatNormalNum = 0;
        private Timer timer = new Timer();
        Boolean textbox1HasText = false;
        Boolean textbox2HasText = false;
        Boolean textbox3HasText = false;
        Boolean textbox4HasText = false;
        Boolean textbox5HasText = false;
        DataSet ds_list = null;
        private DataSet ds_info;
        private bool isManager = false;

        public MainForm(string user, bool isM)
        {
            InitializeComponent();

            label5.Text = user;

            // 设置 计时器 
            timer.Interval = 1000;
            // 绑定 Tick 事件处理程序
            timer.Tick += Timer_Tick;
            timer.Start();

            isManager = isM;
            // 根据权限，设置管理员模式的显示和隐藏
            if (isManager)
            {
                tabPage5.Parent = tabControl1;
            }
            else
            {
                tabPage5.Parent = null;
            }
        }
        // 当前时间
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 在每个 Tick 中更新 Label 的文本为当前时间
            label25.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.CenterFormOnScreen(this);
            //去除首列
            dataGridView1.RowHeadersVisible = false;
            dataGridView2.RowHeadersVisible = false;
            dataGridView3.RowHeadersVisible = false;
            dataGridView4.RowHeadersVisible = false;
            //加载数据
            LoadData();
            button1_Click(sender, e);
            panel2.Visible = false;

            toolTip1.SetToolTip(this.textBox19, "1-航班计划，2-航班起飞，3-航班结束");
        }

        // 加载下拉菜单数据
        private void LoadData()
        {
            DBcon dbCon = new DBcon();
            try
            {
                string query_start = "SELECT DISTINCT FStart FROM Flight";
                string query_end = "SELECT DISTINCT FEnd FROM Flight";

                SqlDataReader reader_s = dbCon.executeQuery(query_start);
                SqlDataReader reader_e = dbCon.executeQuery(query_end);

                comboBox1.Items.Clear();
                comboBox2.Items.Clear();

                while (reader_s.Read())
                {
                    string value = reader_s["FStart"].ToString();
                    comboBox1.Items.Add(value);
                }

                while (reader_e.Read())
                {
                    string value = reader_e["FEnd"].ToString();
                    comboBox2.Items.Add(value);
                }

            } catch (Exception ex)
            {
                MessageBox.Show("Error loading ComboBox data: " + ex.Message);
            } finally
            {
                dbCon.con_close();
            }

            LoadData_listBox();
        }
        // 资源回收
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 在窗体关闭时停止 Timer,防止内存泄露
            timer.Stop();
        }
        // 交换城市
        private void button5_Click(object sender, EventArgs e)
        {
            // 获取 comboBox1 和 comboBox2 的当前文本内容
            string textComboBox1 = comboBox1.Text;
            string textComboBox2 = comboBox2.Text;

            // 交换两个 ComboBox 的文本内容
            comboBox1.Text = textComboBox2;
            comboBox2.Text = textComboBox1;
        }

        //查询
        private void button4_Click(object sender, EventArgs e)
        {
            DBcon dbCon = new DBcon();
            string comboBox1Value = comboBox1.Text; // 始发地
            string comboBox2Value = comboBox2.Text; // 到达地
            string dateTimePickerValue = dateTimePicker1.Value.ToString("yyyy年M月d日"); // DateTimePicker 的时间值

            try
            {
                string sqlQuery = $"SELECT FliID as 航班号, FStart as 出发城市, FEnd as 到达城市, " +
                  $"FORMAT(StartTime, 'yyyy年MM月dd日 HH:mm:ss') as 出发时间, Space as '预计花费时间(h)'" +
                  $"FROM Flight " +
                  $"WHERE [State] = 1 and FStart LIKE '%{comboBox1Value}%' AND FEnd LIKE '%{comboBox2Value}%' ";
                string sqlAddDate = $"AND FORMAT(StartTime, 'yyyy年M月d日') = '{dateTimePickerValue}'";
                string sql = "";

                if (checkBox3.Checked)
                {
                    sql = sqlQuery + sqlAddDate;
                } else
                {
                    sql = sqlQuery;
                }

                DataSet ds = dbCon.getDataSet(sql, "Flight");
                this.dataGridView1.DataSource = ds.Tables["Flight"];
                dataGridView1.Columns["出发时间"].Width = 200;
                dataGridView1.Columns["预计花费时间(h)"].Width = 155;
            } catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            } finally
            {
                dbCon.con_close();
            }
        }
        // 刷新
        private void button1_Click(object sender, EventArgs e)
        {
            // 刷新航班状态
            RefreshSQL();
            // 刷新主页
            RefreshHome();
        }
        // 刷新主页
        private void RefreshHome()
        {
            DBcon dbCon = new DBcon();
            try
            {
                string sqlQuery = $"SELECT FliID as 航班号, FStart as 出发城市, FEnd as 到达城市, " +
                  $"FORMAT(StartTime, 'yyyy年MM月dd日 HH:mm:ss') as 出发时间, Space as '预计花费时间(h)'" +
                  $"FROM Flight where [State] = 1 ";
                DataSet ds = dbCon.getDataSet(sqlQuery, "Flight");
                this.dataGridView1.DataSource = ds.Tables["Flight"];
                dataGridView1.Columns["出发时间"].Width = 200;
                dataGridView1.Columns["预计花费时间(h)"].Width = 155;
                dataGridView1.Sort(dataGridView1.Columns["出发时间"], ListSortDirection.Ascending);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + "\ndateTimePicker1.Text=" + dateTimePicker1.Text.Trim());
            }
            finally
            {
                dbCon.con_close();
            }
        }

        // 信息提示
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("1、此功能暂只支持中国境内直达、经停航班，自预约之日起180天内成人经济舱价格（暂不提供儿童、婴儿）的机票低价提醒。\r\n\r\n2、提交预约成功后，匹配到您所需价格，我们会将低价航班信息通过短信或邮件通知您，若您的明珠账号绑定南方航空微信号，微信将推送通知消息。每个预约单在有效期内最多通知您3次，您最多同时有5张预约单。\r\n\r\n3、若当前时间超过您的最晚出发时间，系统自动将您的预约单设置为无效。\r\n\r\n4、因机票价格具有较强时效性，收到短信、邮件或微信后请尽快下单，以免错过最低价格。\r\n\r\n5、此功能仅为低价提醒服务，并不承诺一定能匹配到您所需的价格，短信、邮件、微信通知仅供参考，实际价格请以航班查询页面为准。", "机票预约服务说明");
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("南航基于不同的业务场景，可能会以下述方式收集以下个人信息：\r\n\r\n1. 您向南航提供的信息\r\n\r\n为履行我们的航空运输及相关服务协议，或履行我们的法定义务，或为保障公共利益和安全，防控突发卫生公共事件，保障您或其他人的生命健康和财产安全所需，或在欧盟或欧洲经济区境内为保障我们的合法正当利益，我们会请您提供如下个人信息：\r\n\r\n● 身份信息以及联系方式：您的性别、出生日期、身份证号码、护照号码、国籍、常住国，您的姓名、地址、电话号码、电子邮件地址、传真号码，以及与您同行的其他人的个人信息（包括同行人的联系方式），用于将航班及订单消息（包括航班出行、安检、登机、报销、航班延误、保险业务以及事故通知）通知到您或相关联系人，为您安排行程、为您寄送行程单或其他产品、验证您的身份、邀请您反馈服务质量、接受您的投诉建议、为您提供“南航账户”服务、南航快乐飞服务、登录服务、在线认证服务以及南航的其他各类产品。请您注意，在您为其他人预订相关服务时，您需要提交该旅客的个人信息，向我们提供该旅客的个人信息之前，请您确保您已经取得本人的同意，并确保其已知晓并接受本隐私政策。\r\n\r\n● 明珠会员信息：您的会员账号及航班信息、里程兑换的受让人信息、受托人信息、未成年会员的监护人信息，用于管理我们的明珠会员、验证会员身份，以及为您办理积分累计、奖励、兑换和其他常客计划中的服务。\r\n\r\n● 业务服务所需的身份证件信息及图像：身份证、护照、签证页、符合民航局乘机要求的证件、有效期、签发机关、年龄或出生日期、性别，以及可能需要的对应证件图像，按照法规要求将用于您在办理机票预订、订座、值机、乘机、审核办理出入境手续、航空保险服务、提供“南航账户”服务以及其他为您提供的服务（如受理投诉）的过程中验证您的身份。\r\n\r\n● 地理位置信息：诸如您在填写订单时，为帮助您更快、更便捷的填写出行地址时我们会在征得您同意的情况获取您的位置信息。请您注意，这会涉及收集您的敏感个人信息。\r\n\r\n● 支付信息：您的银行卡号、账单地址、信用卡有效期、在线支付平台（支付宝、微信）提供给我们的的账户个人信息（如支付宝、微信支付）、“南航钱包”信息、“南航钱包”余额、订单和操作记录、日志、风控信息，以便于我们管理您购票时的支付信息、验证您的身份、提供“南航钱包”服务、南航快乐飞服务。\r\n\r\n● 有助于改进旅行服务或其他服务的信息：紧急联系人、特殊服务需求、个人喜好（机上餐食偏好、飞机座位偏好、机上服务偏好），用于提升我们产品和服务的相关性，改进我们的服务以及为您提供更贴合您要求的服务，以及在征得您同意的情况下可能会定向投放我们的营销广告。\r\n\r\n● 合同中所包含的个人信息：合同中的联系人姓名、职务、地址和联系方式（其中之一或多个信息），以便我们与企业的联系人沟通和寄送合同或相关文件材料。\r\n\r\n● 其他个人信息：与其他航空公司的积分、里程活动相关的交易性事务及在线事务相关的个人信息；通过监控我们的产品及服务的使用情况，如自助设备、航班状态通知和网上办理登机手续获得的个人信息；通过问卷调查、意见征集或其他营销调查活动收集到的个人信息；提供给南航用于调查或解决问题的个人信息，例如验证您的身份的信息。企业级客户员工部分，我们还会收集和验证企业员工的个人信息，包括姓名、身份证件号、联系方式以及所在企业/单位名称，用于企业级客户预定目的。\r\n\r\n我们使用这些个人信息来为您的旅行提供便利，为您提供其他旅行相关的商品和服务，管理我们的“南航明珠俱乐部”，开展市场营销，并向您提供关于我们的信息和联系方式。我们向您征询的个人信息及具体原因将在我们向您征询您的个人信息时向您明示。\r\n\r\n2. 我们自动收集的信息\r\n\r\n当您访问我们的网站、App、小程序、微信公众号，我们会因技术、设备方面的原因，自动地从您的设备中收集与您相关的特定信息。这些我们自动收集的信息将包含您的：\r\n\r\n● 设备信息：设备型号、操作系统版本、浏览器版本号、IMEI号、IMSI、Android ID、IDFA、Mac地址、IP地址、端口信息、DNS、移动运营商、数据网络制式、ROOT标识、网络接入方式、登录渠道、APP版本号、登录时间、运行中的进程、传感器、安装列表、硬件编号及其他与用户网络及系统、设备有关的日志信息、技术参数信息。\r\n\r\n● 设备的广泛地理位置（包括国家或者城市级别的位置）的信息，以及其他的技术信息。我们也会收集有关您的设备如何与我们交互的信息，包括您访问的网页和点击的链接。\r\n\r\n请您理解，这些信息是用于在国双、淘宝、MTA、极光推送、支付宝这些第三方分析判断设备唯一性及安全风控所必须收集的信息。我们将这些信息用于运营之目的，以帮助我们更好的理解我们的访客群体，以提升我们网站的水平和对访客的相关性。我们会在您打开南方航空App时即开始收集这些信息。如您拒绝上述信息收集，我们将无法为您提供在线环境上（App、官方网站、小程序或微信公众号上）的基础功能或服务。\r\n\r\n这些信息中的一部分会使用Cookies和类似的追踪技术收集，如下所述“四、南航如何使用Cookies及类似技术”。\r\n\r\n3. 我们从第三方处获得的信息\r\n\r\n在您选用第三方和我们合作的产品或服务时，我们可能也会不时地从第三方来源收到有关您的个人信息，但这只应发生在：第三方履行与您签署相关的产品或服务协议所需，或为公共利益或防控公共卫生事件所需（如疫情防控所需），或者第三方依法被许可或被要求向我们披露您的个人信息的情况，或取得您的同意时。\r\n\r\n我们从第三方处收集的信息，涵盖了关联公司、业务合作伙伴，如专业旅行机构和其他协助订票、安排旅程的机构提供给的信息，以及代理人或机构通过其他网站为您订票、安排行程而提供给我们的信息。我们将这些从第三方处收集的信息用于为您提供服务，以及用于保证我们所掌握的关于您的相关记录的准确性。此外，请您注意，专业旅行机构可能有其自身专门的隐私政策来说明其如何收集和处理您的个人信息，该等隐私政策不构成南航的隐私政策的一部分，请您自行审慎阅读后作出是否同意的决定。\r\n\r\n4. 我们将获取的设备权限清单\r\n\r\n以下是我们启动时会向您获取的设备权限及对应的说明，以便向您提供更优质的服务，届时会弹窗提示，建议您予以许可。在获取对应的权限前，我们会征询您的同意。您也可以通过设备操作系统的设置，关闭对应的授权。", "隐私通知");
        }
        // 显示选中数据
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox_fliID.Text = this.dataGridView1[0, this.dataGridView1.CurrentCell.RowIndex].Value.ToString().Trim();
            DateTime dt = Convert.ToDateTime(this.dataGridView1[3, this.dataGridView1.CurrentCell.RowIndex].Value);
            textBox_fstart.Text = dt.ToString("HH:mm:ss").Trim();
            textBox13.Text = this.dataGridView1[1, this.dataGridView1.CurrentCell.RowIndex].Value.ToString().Trim();
            textBox14.Text = this.dataGridView1[2, this.dataGridView1.CurrentCell.RowIndex].Value.ToString().Trim();
            DBcon dbCon = new DBcon();
            try
            {
                string sql = $"select EndTime, SeatTop, SeatNomal, PriceBase from Flight where FliID = {textBox_fliID.Text}";
                SqlDataReader reader = dbCon.executeQuery(sql);
                while (reader.Read())
                {
                    textBox_fend.Text = Convert.ToDateTime(reader["EndTime"]).ToString("HH:mm:ss");
                    seatNormalNum = (int)reader["SeatNomal"];
                    seatTopNum = (int)reader["SeatTop"];
                    
                    basePrice = reader["PriceBase"].ToString();
                    comboBox_level_SelectedIndexChanged(sender, e);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + "\ndateTimePicker1.Text=" + dateTimePicker1.Text.Trim());
            }
            finally
            {
                dbCon.con_close();
            }
        }
        // 实时改变票价
        private void comboBox_level_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_level.Text == "头等舱")
            {        
                int topPrice = (int)(Convert.ToDouble(basePrice) *1.5);
                textBox_price.Text = topPrice.ToString();
                textBox_num.Text = seatTopNum.ToString();
            }
            if (comboBox_level.Text == "经济舱")
            {
                textBox_price.Text = ((int)Convert.ToDouble(basePrice)).ToString();
                textBox_num.Text = seatNormalNum.ToString();    
            }
        }
        // 出发日期有效性
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                dateTimePicker1.Enabled = true;
            }
            else
            {
                dateTimePicker1.Enabled = false;
            }
        }
        // 去买票
        private void button10_Click(object sender, EventArgs e)
        {
            if (textBox_fliID.Text == "")
            {
                MessageBox.Show("请选择航班");
            } else
            {
                DateTime dtime = Convert.ToDateTime(this.dataGridView1[3, this.dataGridView1.CurrentCell.RowIndex].Value);
                PayForm payForm = new PayForm(label5.Text, textBox13.Text, textBox14.Text, textBox_price.Text, textBox_fliID.Text, dtime.ToString());
                payForm.ShowDialog();
            }

            button1_Click(sender, e);

        }
        // 航班动态
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox_fliID.Text.Length == 0)
            {
                MessageBox.Show("请选择航班");
            }
            else 
            {
                searchFlightActive(textBox_fliID.Text);
            }
        }

        // 查询航班动态，更新面板数据
        private void searchFlightActive(string FliID)
        {
            // 更新进度条，取值
            DBcon dBcon = new DBcon();
            try
            {
                string sql = $"SELECT * FROM Flight WHERE FliID = '{FliID}'";
                SqlDataReader reader = dBcon.executeQuery(sql);
                if (reader.Read())
                {
                    // 航班号、出发到达城市
                    label17.Text = FliID;
                    city1.Text = reader["FStart"].ToString();
                    city2.Text = reader["FEnd"].ToString();

                    // 出发日期时间
                    DateTime startTime_active = Convert.ToDateTime(reader["StartTime"]);
                    DateTime StartTime = startTime_active;
                    monthCalendar1.SetDate( StartTime ); 
                    label21.Text = StartTime.ToString("HH:mm:ss");
                    label9.Text = StartTime.ToString("yyyy年MM月dd日");

                    // 预计花费时间
                    label18.Text = reader["Space"].ToString();

                    // 到达日期时间
                    DateTime endTime_active = Convert.ToDateTime(reader["EndTime"]);
                    DateTime EndTime = endTime_active;
                    label23.Text = EndTime.ToString("HH:mm:ss");
                    label13.Text = EndTime.ToString("yyyy年MM月dd日");

                    // 进度条、航班状态
                    string FState = reader["State"].ToString();
                    updateProgressBar(startTime_active, endTime_active, FState);

                    label61.Visible = false;
                    panel2.Visible = true;
                } else 
                {
                    /*MessageBox.Show("航班不存在");*/
                    label61.Text = $"{FliID}次航班不存在!";
                    label61.Visible = true;
                    panel2.Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { dBcon.con_close(); }

            // 切换到第二个标签页
            tabControl1.SelectedTab = tabPage2;
            
        }
        // 更新进度条
        private void updateProgressBar(DateTime startTime_active, DateTime endTime_active, string state)
        {
            // 计算时间差
            TimeSpan timeToStart = DateTime.Now - startTime_active;
            TimeSpan flightDuration = endTime_active - startTime_active;

            // 计算总时间
            double totalSeconds = flightDuration.TotalSeconds;

            // 计算进度百分比
            double progressPercentage = (timeToStart.TotalSeconds / totalSeconds) * 100;

            // 设置进度条的值
            // 设置进度条的值
            if (progressPercentage < 0)
            {
                progressBar2.Value = 0;
                pictureBox4.Visible = false;
                UpdatePictureBoxPosition();
            }
            else if (progressPercentage > 100)
            {
                progressBar2.Value = 100;
                pictureBox4.Visible = true;
                UpdatePictureBoxPosition();
            }
            else
            {
                progressBar2.Value = (int)progressPercentage;
                pictureBox4.Visible = false;
                UpdatePictureBoxPosition();
            }


            if (state == "1") { label19.Text = "航班计划"; panel3.BackColor = Color.DeepSkyBlue; }
            if (state == "2") { label19.Text = " 进行中 "; panel3.BackColor = Color.PaleGreen; }
            if (state == "3") { label19.Text = " 已到达 "; panel3.BackColor = Color.MediumPurple; }
        }
        // 更新小飞机位置
        private void UpdatePictureBoxPosition()
        {
            pictureBox2.Location = new Point(195, 115);
            // 根据进度条的当前值计算 PictureBox 的新横坐标位置
            int progressBarValue = progressBar2.Value;
            int pictureBoxX = pictureBox2.Location.X + (int)((float)progressBarValue / (progressBar2.Maximum - progressBar2.Minimum) * progressBar2.Width);

            // 设置 PictureBox 的新位置
            pictureBox2.Location = new Point(pictureBoxX, 115);

        }
        // 给monthCalendar赋值
        private void SetCalendarDate(DateTime date)
        {
            // 创建一个新的日期范围
            SelectionRange range = new SelectionRange(date, date);

            // 设置 MonthCalendar 的日期范围
            monthCalendar1.SelectionRange = range;
        }
        // 切换标签时刷新数据
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //航线动态
            if (tabControl1.SelectedTab == tabPage2)
            {
                RefreshHome();
                RefreshSQL();
            }
            // 机票预约
            if (tabControl1.SelectedTab == tabPage3)
            {
                RefreshOrder();
                RefreshSQL();
            }
            //管理员模式
            if (tabControl1.SelectedTab == tabPage5)
            {
                RefreshData();
                RefreshOrderData();
                RefreshSQL();
            }
            // 个人中心
            if (tabControl1.SelectedTab == tabPage4)
            {
                LoadData_listBox();
                groupBox1.Visible = true;
                groupBox2.Visible = false;
                LoadData_info();
                RefreshSQL();
            }
        }
        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (tabControl3.SelectedTab == tabPage10)
            {
                RefreshOrder();
            }
        }
        private void RefreshOrder()
        {   
            DBcon dBcon = new DBcon();

            try
            {
                string sql =
                    $"SELECT Ticket.TicID as 机票号, Ticket.FliID as 航班号, FORMAT(Payment.Ptime, 'yyyy年MM月dd日 HH:mm:ss') as 下单时间, Passenger.Pname as 乘客姓名, Seat.SType as 座位类型, " +
                    $"Ticket.SNSelect as 座位号, Flight.FStart as 出发地点, Flight.FEnd as 到达地点," +
                    $"FORMAT(Flight.StartTime, 'yyyy年MM月dd日 HH:mm:ss') as 出发时间, Payment.PMoney as 金额, Payment.PWay as 支付方式 " +
                    $"FROM Ticket " +
                    $"JOIN Passenger ON Ticket.PasID = Passenger.PasID " +
                    $"JOIN Flight ON Ticket.FliID = Flight.FliID " +
                    $"JOIN Seat ON Ticket.SeaID = Seat.SeaID " +
                    $"JOIN Payment ON Ticket.PayID = Payment.PayID " +
                    $"where Passenger.[userID] = '{label5.Text}' ";

                this.dataGridView2.DataSource = null;
                DataSet ds = dBcon.getDataSet(sql, "Ticket_All");
                this.dataGridView2.DataSource = ds.Tables["Ticket_All"];
                this.dataGridView2.Columns["下单时间"].Width = 200; //单独设置列宽使其显示完全
                this.dataGridView2.Columns["出发时间"].Width = 200;

                ListSortDirection defaultSortDirection = ListSortDirection.Descending;
                this.dataGridView2.Sort(this.dataGridView2.Columns[2], defaultSortDirection);// 进行默认排序

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
        private void tabControl4_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 用户列表
            if (tabControl4.SelectedTab == tabPage7)
            {
                LoadData_listBox();
                groupBox1.Visible = true;
                groupBox2.Visible = false;
            }
            // 消息
            if (tabControl4.SelectedTab == tabPage9)
            {
                LoadData_info();
                groupBox1.Visible = false;
                groupBox2.Visible = true;
            }
            //修改密码
            if (tabControl4.SelectedTab == tabPage11)
            {
                groupBox1.Visible = false;
                groupBox2.Visible = false;
            }
        }
        private void LoadData_info()
        {
            DBcon dBcon = new DBcon();
            try
            {
                string sql = $"select * from info where [userID] = '{label5.Text}'";
                SqlDataReader reader = dBcon.executeQuery(sql);
                listBox2.Items.Clear();
                while (reader.Read())
                {
                    listBox2.Items.Add(reader["title"]+"："+reader["info_id"]);
                }
                ds_info = dBcon.getDataSet(sql, "listB");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { dBcon.con_close(); }
        }
        // 列表项选则改变内容
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != -1)
            {
                groupBox2.Visible = true;
                groupBox1.Visible = false;
                foreach(DataRow row in ds_info.Tables["listB"].Rows)
                {
                    if (row["title"]+"："+row["info_id"] == listBox2.SelectedItem.ToString())
                    {
                        richTextBox1.Text = row["content"].ToString();
                    }
                }

                if (listBox2.SelectedItem.ToString().Substring(0,7) == "预约处理成功！")
                {
                    button17.Visible = true;
                } else
                {
                    button17.Visible = false;
                }
            }
        }
        private void tabControl5_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 航班管理
            if (tabControl5.SelectedTab == tabPage12)
            {
                RefreshData();
            }
            // 订单管理
            if (tabControl5.SelectedTab == tabPage13)
            {
                RefreshOrderData();
            }
        }
        private void RefreshOrderData()
        {
            DBcon dBcon = new DBcon();
            try
            {
                string sql_s =
                    $"SELECT AA.AppID as 预约单号, FORMAT(AA.SubmitTime, 'yyyy年MM月dd日 HH:mm:ss') as 提交时间, PP.[userID] as 用户名, PP.pname as 乘客姓名, " +
                    $"PP.contact as 联系方式, PP.identify as 证件号码, AA.AFStart as 出发地点, " +
                    $"AA.AFEnd as 到达地点, AA.TimeEar as 最早出发, AA.TimeLat as 最晚出发, " +
                    $"AA.ExpectPrice as 预算, AA.AState as 订单状态 " +
                    $"FROM Appointment as AA JOIN Passenger as PP ON AA.PasID = PP.PasID ";

                this.dataGridView4.DataSource = null;
                DataSet ds_1 = dBcon.getDataSet(sql_s, "Appointment_All");
                this.dataGridView4.DataSource = ds_1.Tables["Appointment_All"];
                this.dataGridView4.Columns["提交时间"].Width = 200; //单独设置列宽使其显示完全
                ListSortDirection defaultSortDirection_1 = ListSortDirection.Descending;// 进行默认排序
                this.dataGridView4.Sort(this.dataGridView4.Columns[1], defaultSortDirection_1);

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

        // 加载用户乘客列表
        private void LoadData_listBox()
        {
            textBox2.Text = label5.Text;
            DBcon dBcon = new DBcon();
            try
            {
                string sql = $"select * from Passenger where [userID] = '{label5.Text}'";
                SqlDataReader reader = dBcon.executeQuery(sql);
                listBox1.Items.Clear();
                comboBox3.Items.Clear();
                while (reader.Read())
                {
                    listBox1.Items.Add(reader["pname"]);
                    comboBox3.Items.Add(reader["pname"]);
                }
                ds_list = dBcon.getDataSet(sql, "listP");
                /*dataGridView5.DataSource = ds_list.Tables["listP"];*/
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { dBcon.con_close(); }
        }
        

        // 输入框提示字符模板
        private void TextBox_Enter(object sender, System.Windows.Forms.TextBox textBoxControl, ref bool textBoxHasText, string defaultText)
        {
            if (!textBoxHasText)
            {
                textBoxControl.Text = "";
                textBoxControl.ForeColor = Color.Black;
            }
        }
        private void TextBox_Leave(object sender, System.Windows.Forms.TextBox textBoxControl, ref bool textBoxHasText, string defaultText)
        {
            if (textBoxControl.Text == "")
            {
                textBoxControl.Text = defaultText;
                textBoxControl.ForeColor = Color.DarkGray;
                textBoxHasText = false;
            }
            else
            {
                textBoxHasText = true;
            }
        }
        // 调用模板，实现输入框提示字符
        private void textBox21_Enter(object sender, EventArgs e)
        {
            TextBox_Enter(sender, textBox21, ref textbox1HasText, "20");
        }
        private void textBox21_Leave(object sender, EventArgs e)
        {
            TextBox_Leave(sender, textBox21, ref textbox1HasText, "20");
        }
        private void textBox20_Enter(object sender, EventArgs e)
        {
            TextBox_Enter(sender, textBox20, ref textbox2HasText, "80");
        }
        private void textBox20_Leave(object sender, EventArgs e)
        {
            TextBox_Leave(sender, textBox20, ref textbox2HasText, "80");
        }
        private void textBox19_Enter(object sender, EventArgs e)
        {
            TextBox_Enter(sender, textBox19, ref textbox3HasText, "1");
        }
        private void textBox19_Leave(object sender, EventArgs e)
        {
            TextBox_Leave(sender, textBox19, ref textbox3HasText, "1");
        }
        private void textBox18_Enter(object sender, EventArgs e)
        {
            TextBox_Enter(sender, textBox18, ref textbox4HasText, "添加时，自动生成");
        }
        private void textBox18_Leave(object sender, EventArgs e)
        {
            TextBox_Leave(sender, textBox18, ref textbox4HasText, "添加时，自动生成");
        }
        private void textBox1_Enter(object sender, EventArgs e)
        {
            TextBox_Enter(sender, textBox1, ref textbox5HasText, "请输入或选择航班号");
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            TextBox_Leave(sender, textBox1, ref textbox5HasText, "请输入或选择航班号");
        }

        // 修改航班
        private void button11_Click(object sender, EventArgs e)
        {
            // 获取 dataGridView3 的数据源
            DataTable dataTable = (DataTable)dataGridView3.DataSource;

            // 遍历每一行
            foreach (DataRow row in dataTable.Rows)
            {
                string flightId = row["航班号"].ToString();
                string fStartPlace = row["出发城市"].ToString();
                string FEndPlace = row["到达城市"].ToString();
                string StartTime = Convert.ToDateTime(row["出发时间"]).ToString();
                /*string TimeStart = row["出发时间"].ToString();*/
                string SpaceTime = row["预计花费时间(h)"].ToString();
                string EndTime = Convert.ToDateTime(row["预计到达时间"]).ToString();
                string FlightState = row["航班状态"].ToString();
                string SeatTop = row["头等舱票数"].ToString();
                string SeatNomal = row["经济舱票数"].ToString();
                string PriceBase = row["基础票价"].ToString();
                
                //更新一行
                UpdateFlightInfo(flightId, fStartPlace, FEndPlace, StartTime, SpaceTime, EndTime, FlightState, SeatTop, SeatNomal, PriceBase);
            }

            MessageBox.Show("修改成功!");
            RefreshSQL();
            RefreshData();
        }
        // 添加航班
        private void button6_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBox18.Text) || string.IsNullOrEmpty(textBox16.Text) || string.IsNullOrEmpty(textBox15.Text)
            || string.IsNullOrEmpty(textBox17.Text) || string.IsNullOrEmpty(textBox19.Text) || string.IsNullOrEmpty(textBox21.Text)
            || string.IsNullOrEmpty(textBox20.Text) || string.IsNullOrEmpty(textBox22.Text))
            {
                // 提示用户输入不完整
                MessageBox.Show("请确保所有输入框都已填写。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } else
            {
                DBcon dBcon = new DBcon();
                try
                {
                    string FliID = textBox18.Text;
                    string FStart = textBox16.Text;
                    string FEnd = textBox15.Text;
                    // 计算时间
                    DateTime dateTime = dateTimePicker4.Value;
                    /*string FDate = dateTime.ToString("yyyy-MM-dd");
                    string FTime = dateTime.ToString("HH:mm:ss");*/
                    string StartTime = dateTime.ToString();
                    string Space = textBox17.Text;
                    double SpaceTime = Convert.ToDouble(Space);
                    string EndTime = dateTime.AddHours(SpaceTime).ToString();

                    string FState = textBox19.Text;
                    string SeatTop = textBox21.Text;
                    string SeatNomal = textBox20.Text;
                    string PriceBase = textBox22.Text;

                    string sql =
                        $"insert into Flight(FStart, FEnd, StartTime, Space, EndTime, State, SeatTop, SeatNomal, PriceBase ) " +
                        $"values('{FStart}','{FEnd}','{StartTime}','{Space}','{EndTime}'," +
                        $"'{FState}','{SeatTop}','{SeatNomal}','{PriceBase}')";
                    dBcon.executeUpdate(sql);
                    sql = $"select * from Appointment";

                    MessageBox.Show("添加航班成功！");
                    RefreshSQL();
                    RefreshData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "添加失败"); }
                finally { dBcon.con_close(); }
            }
        }
        // 查询航班
        private void button12_Click(object sender, EventArgs e)
        {
            DBcon dBcon = new DBcon();

            string FliID = "";
            if (checkBox8.Checked) { FliID = textBox18.Text; }

            string FStart = textBox16.Text;

            string FEnd = textBox15.Text;

            DateTime dateTime = dateTimePicker4.Value;
            string FDate = "";
            if (checkBox9.Checked) { FDate = dateTime.ToString("yyyy年MM月dd日"); }

            string Space = textBox17.Text;

            string FState = "";
            if (checkBox5.Checked) { FState = textBox19.Text; }

            string SeatTop = "";
            if (checkBox7.Checked) { SeatTop = textBox21.Text; }

            string SeatNomal = "";
            if (checkBox6.Checked) { SeatNomal= textBox20.Text; }

            string PriceBase = textBox22.Text;

            try
            {
                string sqlQuery =
                    $"SELECT FliID as 航班号, FStart as 出发城市, FEnd as 到达城市, " +
                    $"FORMAT(StartTime, 'yyyy年MM月dd日 HH:mm:ss') as 出发时间, Space as '预计花费时间(h)', " +
                    $"FORMAT(EndTime, 'yyyy年MM月dd日 HH:mm:ss') as 预计到达时间, State as 航班状态, SeatTop as 头等舱票数, SeatNomal as 经济舱票数, " +
                    $"PriceBase as 基础票价 " +
                    $"FROM Flight " +
                    $"WHERE FliID LIKE '%{FliID}%' " +
                    $"AND FStart LIKE '%{FStart}%' " +
                    $"AND FEnd LIKE '%{FEnd}%' " +
                    $"AND FORMAT(StartTime, 'yyyy年MM月dd日') LIKE '%{FDate}%' " +
                    $"AND [State] LIKE '%{FState}%' " +
                    $"AND SeatTop LIKE '%{SeatTop}%' " +
                    $"AND SeatNomal LIKE '%{SeatNomal}%' " +
                    $"AND PriceBase LIKE '%{PriceBase}%' " +
                    $"AND Space LIKE '%{Space}%'";

                DataSet ds = dBcon.getDataSet(sqlQuery, "FlightManager");
                this.dataGridView3.DataSource = ds.Tables["FlightManager"];
                this.dataGridView3.Columns["出发时间"].Width = 200;
                this.dataGridView3.Columns["预计到达时间"].Width = 200;
                this.dataGridView3.Sort(this.dataGridView3.Columns[3], ListSortDirection.Ascending);
                dataGridView3.Columns["航班号"].ReadOnly = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "发生错误"); }
            finally { dBcon.con_close(); }
        }
        // 删除航班
        private void button13_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentRow != null)
            {
                DialogResult result = MessageBox.Show("你确定要删除吗？", "确认", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    int rowIndex = dataGridView3.CurrentRow.Index;
                    // 获取航班号
                    string flightID = dataGridView3.Rows[rowIndex].Cells["航班号"].Value.ToString();
                    DeleteFlight(flightID);
                }
            }
            else
            {
                MessageBox.Show("请先选择要删除的航班行");
            }
        }

        // 删除操作函数
        private void DeleteFlight(string selectedFliID)
        {
            DBcon dBcon = new DBcon();

            try
            {
                string sqlQuery = $"select * from Ticket where FliID = '{selectedFliID}'";
                SqlDataReader reader = dBcon.executeQuery(sqlQuery);
                if (reader.Read()) 
                {
                    MessageBox.Show("该航班已有机票售出，不能删除！");
                } else
                {
                    string sqlDelete = $"DELETE FROM Flight WHERE FliID = '{selectedFliID}'";
                    dBcon.executeUpdate(sqlDelete);

                    MessageBox.Show("删除成功！");
                    // 更新显示的航班信息
                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除航班时发生错误：{ex.Message}", "错误");
            }
            finally
            {
                dBcon.con_close();
            }
        }
        // 刷新数据表datagridview3
        private void RefreshData()
        {
            DBcon dBcon = new DBcon();
            try
            {
                string sql = $"select * from Flight";
                SqlDataReader reader = dBcon.executeQuery (sql);
                while (reader.Read())
                {
                    string FliID = reader["FliID"].ToString();
                    DateTime stime = Convert.ToDateTime( reader["StartTime"] );
                    double space = Convert.ToDouble(reader["Space"].ToString());
                    DateTime eTime = stime.AddHours(space);
                    sql = $"update Flight set EndTime = '{eTime}' where FliID = '{FliID}'";
                    dBcon.executeUpdate(sql);
                }

                string sqlQuery = $"SELECT FliID as 航班号, FStart as 出发城市, FEnd as 到达城市, " +
                $"FORMAT(StartTime, 'yyyy年MM月dd日 HH:mm:ss') as 出发时间, Space as '预计花费时间(h)', " +
                $"FORMAT(EndTime, 'yyyy年MM月dd日 HH:mm:ss') as 预计到达时间, State as 航班状态, SeatTop as 头等舱票数, SeatNomal as 经济舱票数, " +
                $"PriceBase as 基础票价 " +
                $"FROM Flight ";
                DataSet ds = dBcon.getDataSet(sqlQuery, "FlightManager");
                this.dataGridView3.DataSource = ds.Tables["FlightManager"];
                this.dataGridView3.Columns["出发时间"].Width = 200;
                this.dataGridView3.Columns["预计到达时间"].Width = 200;
                this.dataGridView3.Sort(this.dataGridView3.Columns[3], ListSortDirection.Ascending);
                dataGridView3.Columns["航班号"].ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                dBcon.con_close();
            }
        }
        // 更新修改至数据库
        private void UpdateFlightInfo(string flightId, string fStartPlace, string FEndPlace, string StartTime, string SpaceTime, string EndTime, string FlightState, string SeatTop, string SeatNomal, string PriceBase)
        {
            DBcon dBcon = new DBcon();
            try
            {
                string sqlUpdate =
                    $"UPDATE Flight SET FStart = '{fStartPlace}', FEnd = '{FEndPlace}', StartTime = '{StartTime}', Space = '{SpaceTime}', " +
                    $"EndTime = '{EndTime}', State = '{FlightState}', SeatTop = '{SeatTop}', SeatNomal = '{SeatNomal}', PriceBase = '{PriceBase}'" +
                    $"WHERE FliID = '{flightId}'";
                dBcon.executeUpdate(sqlUpdate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "修改失败");
            }
            finally
            {
                dBcon.con_close();
            }
        }
        // 数据有效性
        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                textBox21.Enabled = true;
            }
            if (!checkBox7.Checked) 
            {
                textBox21.Enabled = false;
            }
        }
        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                textBox20.Enabled = true;
            }
            if (!checkBox6.Checked)
            {
                textBox20.Enabled = false;
            }
        }
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                textBox19.Enabled = true;
            }
            if (!checkBox5.Checked)
            {
                textBox19.Enabled = false;
            }
        }
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                textBox18.Enabled = true;
            }
            if (!checkBox8.Checked)
            {
                textBox18.Enabled = false;
            }
        }
        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox9.Checked)
            {
                dateTimePicker4.Enabled = false;
            }

            if (checkBox9.Checked)
            {
                dateTimePicker4.Enabled = true;
            }
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                button9.Enabled = true;
            }
            if (!checkBox2.Checked)
            {
                button9.Enabled = false;
            }
        }
        private void button9_EnabledChanged(object sender, EventArgs e)
        {
            if (button9.Enabled)
            {
                // 当按钮启用时，设置背景颜色为红色
                button9.BackColor = Color.FromArgb(238, 30, 46);
            }
            else
            {
                // 当按钮禁用时，设置背景颜色为灰红色
                button9.BackColor = Color.Gray;
            }
        }
        // 查询动态
        private void button14_Click(object sender, EventArgs e)
        {
            string FliID = textBox1.Text;
            if (FliID == "请输入或选择航班号")
            {
                FliID = "";
            } else
            {
                FliID = textBox1.Text;
                searchFlightActive(FliID);
            }
            
        }

        // 提交预约订单
        private void button9_Click(object sender, EventArgs e)
        {
            if (textBox23.Text == "" || textBox24.Text == "" || textBox8.Text=="" || textBox9.Text=="" 
                || textBox10.Text=="" || textBox11.Text=="")
            {
                MessageBox.Show("请将信息填写完整");
            } else if (dateTimePicker5.Value > dateTimePicker6.Value)
            {
                MessageBox.Show("最早出发时间不可晚于最晚出发时间");
            } else
            {
                DBcon dBcon = new DBcon();
                try
                {
                    string PasID = "";
                    string pname = textBox9.Text;
                    string contact = textBox10.Text;
                    string PUser = label5.Text;
                    string identify = textBox11.Text;

                    // 先判断该乘客是否存在
                    string sql_s1 = $"select * from Passenger where [userID] = '{label5.Text}' and pname = '{textBox9.Text}'";
                    SqlDataReader sqlDataReader = dBcon.executeQuery(sql_s1);

                    if (sqlDataReader.Read())
                    {
                        PasID = sqlDataReader["PasID"].ToString();
                        pname = sqlDataReader["pname"].ToString();
                        contact = sqlDataReader["contact"].ToString();
                        PUser = sqlDataReader["userID"].ToString();
                        identify = sqlDataReader["identify"].ToString();
                    }
                    else
                    {
                        // 插入新数据
                        sql_s1 = $"insert into Passenger(pname, contact, [userID], identify) values('{textBox9.Text}', '{textBox10.Text}', '{label5.Text}', '{textBox11.Text}')";
                        dBcon.executeUpdate(sql_s1);

                        // 查PasID
                        sql_s1 = $"select PasID from Passenger where [userID] = '{PUser}' and pname = '{pname}'";
                        SqlDataReader reader = dBcon.executeQuery(sql_s1);
                        if (reader.Read())
                        {
                            PasID = reader["PasID"].ToString();
                        }
                    }

                    string AFStart = textBox23.Text;
                    string AFEnd = textBox24.Text;
                    string TimeEar = dateTimePicker5.Value.ToString("yyyy-MM-dd");
                    string TimeLat = dateTimePicker6.Value.ToString("yyyy-MM-dd");
                    string ExpectPrice = textBox8.Text;
                    /*bool isVacant = checkBox1.Checked;*/
                    DateTime SubmitTime = DateTime.Now;

                    // 插入订单数据
                    sql_s1 = $"insert into Appointment(PasID, AFStart, AFEnd, TimeEar, TimeLat, ExpectPrice, SubmitTime) " +
                        $"values('{PasID}', '{AFStart}', '{AFEnd}', '{TimeEar}', '{TimeLat}', '{ExpectPrice}', '{SubmitTime}')";
                    dBcon.executeUpdate(sql_s1);
                    MessageBox.Show("预约成功！");

                    sql_s1 = "SELECT TOP 1 * FROM Appointment ORDER BY AppID DESC";
                    SqlDataReader dr = dBcon.executeQuery(sql_s1);
                    
                    if (dr.Read())
                    {
                        string AppID = dr["AppID"].ToString();
                        // 消息内容
                        string time = DateTime.Now.ToString();
                        string content =
                            $"\n    尊敬的 {label5.Text} 用户，您的预约订单已成功提交,单号已生成。\n" +
                            $"    预约订单号：{AppID}\t\n" +
                            $"    出发地点：{AFStart}\t到达地点：{AFEnd}\n" +
                            $"    最早出发时间：{TimeEar}\n" +
                            $"    最晚出发时间：{TimeLat}" +
                            $"    预  算：{ExpectPrice}\n" +
                            $"    我们将尽快为您安排航班，请耐心等待，感谢您选择我们的服务，祝您旅途愉快！" +
                            $"\n\n    时间：{time}";

                        sql_s1 = $"insert into info([userID], [title], content) values('{label5.Text}', '预约成功！', '{content}')";
                        dBcon.executeUpdate(sql_s1);  
                    }
                }
                catch (Exception ex) { MessageBox.Show("预约失败" + ex.Message); }
                finally { dBcon.con_close(); }
            }
        }
        // 刷新状态
        public void RefreshSQL()
        {
            DBcon dBcon = new DBcon();

            try
            {
                // 查询数据库获取所有航班信息
                string sqlQuery = "SELECT FliID, StartTime, EndTime FROM Flight";
                DataSet ds = dBcon.getDataSet(sqlQuery, "AllFlightInfo");
                foreach (DataRow row in ds.Tables["AllFlightInfo"].Rows)
                {
                    // 从数据库中获取开始时间和结束时间
                    DateTime startTime = Convert.ToDateTime(row["StartTime"].ToString());
                    DateTime endTime = Convert.ToDateTime(row["EndTime"].ToString());
                    // 获取当前时间
                    DateTime currentTime = DateTime.Now;
                    // 根据当前时间更新航班状态
                    string newState = GetFlightState(currentTime, startTime, endTime);
                    // 获取航班ID
                    string flightID = row["FliID"].ToString();
                    // 更新数据库中的航班状态
                    string updateSql = $"UPDATE Flight SET State = '{newState}' WHERE FliID = '{flightID}'";
                    dBcon.executeUpdate(updateSql); 
                }

                /*MessageBox.Show("所有航班状态更新成功！");*/
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生错误：{ex.Message}");
            }
            finally
            {
                dBcon.con_close();
            }
        }
        // 获取当前时间下航班状态
        private string GetFlightState(DateTime currentTime, DateTime startTime, DateTime endTime)
        {

            if (currentTime < startTime)
            {
                return "1"; //未开始
            }
            else if (currentTime >= startTime && currentTime <= endTime)
            {
                return "2"; //进行中
            }
            else
            {
                return "3"; //已结束
            }
        }
        // 乘客切换信息显示
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            textBox2.Text = label5.Text;
            if (listBox1.SelectedIndex != -1)
            {
                foreach (DataRow row in ds_list.Tables["listP"].Rows)
                {
                    if (row["pname"].ToString() == listBox1.SelectedItem.ToString())
                    {
                        textBox3.Text = row["pname"].ToString();
                        textBox12.Text = row["identify"].ToString();
                        textBox4.Text = row["contact"].ToString();
                    }
                }
            }
        }
        // 清空乘客
        private void button16_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
            textBox12.Text = "";
            textBox4.Text = "";
            LoadData_listBox();
        }
        // 添加乘客
        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                MessageBox.Show("用户名为空");
            } else if (textBox12.Text =="" || textBox3.Text=="" || textBox4.Text=="")
            {
                MessageBox.Show("请将信息补充完整");
            } else
            {
                DBcon dBcon = new DBcon();
                try
                {
                    string sql_query = $"select * from Passenger where [userID] = '{textBox2.Text}' and identify = '{textBox12.Text}'";
                    SqlDataReader reader = dBcon.executeQuery(sql_query);
                    if (!reader.Read())
                    {
                        string sql = $"insert into Passenger([userID], pname, contact, identify) " +
                        $"values('{textBox2.Text}', '{textBox3.Text}', '{textBox4.Text}', '{textBox12.Text}')";
                        dBcon.executeUpdate(sql);

                        MessageBox.Show("添加成功");
                    }
                    else
                    {
                        MessageBox.Show("identify重复");
                    }
                    LoadData_listBox();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                finally { dBcon.con_close(); }
            }
            
        }
        // 修改乘客
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                MessageBox.Show("用户名为空");
            }
            else
            {
                DBcon dBcon = new DBcon();
                try
                {
                    string sql = $"update Passenger set pname = '{textBox3.Text}', " +
                        $"contact = '{textBox4.Text}' " +
                        $"where [userID] = '{textBox2.Text}' and [identify] = '{textBox12.Text}'";
                    int result_len = dBcon.executeUpdate(sql);
                    if (result_len > 0)
                    {
                        MessageBox.Show("修改成功");
                    }
                    else
                    {
                        MessageBox.Show("修改失败,查无此人");
                    }

                    LoadData_listBox();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                finally { dBcon.con_close(); }
            }
            }
        // 删除乘客
        private void button15_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                MessageBox.Show("用户名为空");
            }else if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择要删除的乘客。");
            } else
            {
                DBcon dBcon = new DBcon();
                string sql = "";
                try
                {
                    sql = $"select * from Ticket JOIN Passenger ON Ticket.PasID = Passenger.PasID where Passenger.identify = '{textBox12.Text}'";
                    SqlDataReader reader = dBcon.executeQuery(sql);
                    if (reader.Read())
                    {
                        MessageBox.Show("该乘客已买票，不可删除！");
                    }
                    else
                    {
                        sql = $"delete from Passenger where [userID] = '{textBox2.Text}' and identify = '{textBox12.Text}'";
                        dBcon.executeUpdate(sql);

                        MessageBox.Show("删除成功");
                        LoadData_listBox();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                finally { dBcon.con_close(); }
            }
            
        }

        // 修改用户密码（根据三个TextBox的值来进行修改密码逻辑，textBox5:当前密码，textBox6:新密码，textBox7:确认密码；
        private void button8_Click(object sender, EventArgs e)
        {
            string currentPassword = textBox5.Text;
            string newPassword = textBox6.Text;
            string confirmPassword = textBox7.Text;
            string currentUsername = label5.Text;

            // 检查当前密码是否正确
            if (CheckCurrentPassword(currentUsername, currentPassword))
            {
                // 检查新密码和确认密码是否匹配
                if (newPassword == confirmPassword)
                {
                    // 更新密码
                    if (UpdatePassword(currentUsername, newPassword))
                    {
                        MessageBox.Show("密码修改成功！");
                    }
                    else
                    {
                        MessageBox.Show("密码修改失败，请重试！");
                    }
                }
                else
                {
                    MessageBox.Show("新密码和确认密码不匹配！");
                }
            }
            else
            {
                MessageBox.Show("当前密码不正确！");
            }
        }
        // 更新密码
        private bool UpdatePassword(string currentUsername, string newPassword)
        {
            DBcon dBcon = new DBcon();
            string updateSql = $"UPDATE [user] SET password = '{newPassword}' WHERE [userID] = '{currentUsername}'";
            bool update_check = false;
            try
            {
                update_check = dBcon.executeUpdate(updateSql) > 0;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { dBcon.con_close(); }
            return update_check;
        }
        // 检查密码
        private bool CheckCurrentPassword(string currentUsername, string currentPassword)
        {
            DBcon dBcon = new DBcon();
            string sql = $"SELECT password FROM [user] WHERE [userID] = '{currentUsername}'";
            bool check_check = false;
            try
            {
                SqlDataReader reader = dBcon.executeQuery(sql) ;
                if (reader.Read())
                {
                    check_check = (currentPassword == reader["password"].ToString());
                };
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { dBcon.con_close(); }
            return check_check;
        }
        // 订单处理
        private void button_catch_Click(object sender, EventArgs e)
        {
            DBcon dBcon = new DBcon();
            bool check_money = false;
            bool check_place = false;
            bool check_time = false;
            bool check_state = false;
            try
            {
                int successCount = 0;
                int needProcessCount = 0;

                foreach (DataGridViewRow row in dataGridView4.Rows)
                {
                    if (row.Cells[11].Value.ToString().Trim() == "进行中")
                    {
                        // 获取订单数据
                        string AppID = row.Cells[0].Value.ToString().Trim();
                        string AFStart = row.Cells[6].Value.ToString().Trim();
                        string AFEnd = row.Cells[7].Value.ToString().Trim();
                        string AState = row.Cells[11].Value.ToString().Trim();
                        double ExpectPrice = Convert.ToDouble(row.Cells[10].Value.ToString().Trim());
                        DateTime TimeEar = Convert.ToDateTime(row.Cells[8].Value);
                        DateTime TimeLat = Convert.ToDateTime(row.Cells[9].Value);
                        TimeLat = TimeLat.AddDays(1);
                        TimeLat = TimeLat.AddSeconds(-1);

                        //查询有无符合的航班
                        string sql = $"select * from Flight where [State] = '1'";
                        string FliID = "";
                        string FStart = "";
                        string FEnd = "";
                        DateTime StartTime;
                        double PriceBase = 0;
                        string FState = "";
                        // 余票是否充足
                        int SeatTop = 0;
                        int SeatNomal = 0;
                        // 统计需要处理的订单
                        needProcessCount++;

                        SqlDataReader reader = dBcon.executeQuery(sql);
                        string update_sql = "";
                        
                        check_money = false;
                        check_place = false;
                        check_time = false;
                        while (reader.Read())
                        {
                            int up_num = 0;
                            FliID = reader["FliID"].ToString().Trim();
                            FStart = reader["FStart"].ToString().Trim();
                            FEnd = reader["FEnd"].ToString().Trim();
                            PriceBase = Convert.ToDouble(reader["PriceBase"]);
                            StartTime = Convert.ToDateTime(reader["StartTime"]);
                            SeatTop = Convert.ToInt32(reader["SeatTop"]);
                            SeatNomal = Convert.ToInt32(reader["SeatNomal"]);
                            FState = reader["state"].ToString().Trim();

                            if (FStart == AFStart && FEnd == AFEnd)
                            {
                                check_place = true;
                                if (StartTime >= TimeEar && StartTime <= TimeLat)
                                {
                                    check_time = true;
                                    // 判断预算
                                    if (ExpectPrice < PriceBase)    //买不到，钱不够
                                    {
                                        check_money = false;
                                    }
                                    if (ExpectPrice >= PriceBase && ExpectPrice < PriceBase * 1.5) //够买经济舱
                                    {
                                        if (SeatNomal > 0)
                                        {
                                            check_money = true;
                                        }
                                        else
                                        {
                                            check_money = false;
                                        }
                                    }
                                    if (ExpectPrice >= PriceBase * 1.5)   //购买头等舱
                                    {
                                        if (SeatTop > 0)
                                        {
                                            check_money = true;
                                        }
                                        else if (SeatNomal > 0)
                                        {
                                            check_money = true;
                                        }
                                        else { check_money = false; }
                                    }
                                    
                                    if (check_money)
                                    {
                                        if (FState == "1")
                                        {
                                            check_state = true;
                                            // 处理成功，订单状态修改
                                            update_sql = $"update Appointment set AState = '已完成' where AppID = '{AppID}'";
                                            up_num = dBcon.executeUpdate(update_sql);
                                            // 处理成功计数,需处理订单减1
                                            successCount++;
                                            needProcessCount--;
                                        }   
                                    }
                                }
                            }
                            // 修改生效
                            if (up_num > 0)
                            {
                                // 消息发送
                                DBcon dBcon1 = new DBcon();
                                listBox3.Items.Add(AppID + "号处理成功，推荐航班号："+ FliID);
                                try
                                {   // 消息内容
                                    string time = DateTime.Now.ToString();
                                    string content =
                                        $"\n    尊敬的 {label5.Text} 用户，您的预约订单已成功处理。\n" +
                                        $"    预约订单号：{AppID}\t航班号：{FliID}\n" +
                                        $"    出发地点：{FStart}\t到达地点：{FEnd}\n" +
                                        $"    出发时间：{StartTime}\n" +
                                        $"    基础票价：{PriceBase}\n" +
                                        $"    您可以点击下方‘去购买’按钮前往支付订单，感谢您选择我们的服务，祝您拥有一次愉快的旅行！" +
                                        $"\n\n    时间：{time}";
                                    update_sql = $"insert into info([userID], [title], content) values('{label5.Text}', '预约处理成功！', '{content}')";
                                    dBcon1.executeUpdate(update_sql);  // 使用正确的 SQL 查询
                                }
                                catch (Exception ex) { listBox3.Items.Add("Error" + ex.Message); }
                                finally { dBcon1.con_close(); }
                            }
                        }
                        if (!check_place) { listBox3.Items.Add(AppID + "号未处理，原因：无匹配航段的航班"); }
                        else if (!check_time) { listBox3.Items.Add(AppID + "号未处理，原因：无满足时间的航班"); }
                        else if (!check_money) { listBox3.Items.Add(AppID + "号未处理，原因：预算不足"); }
                        else if (!check_state) { listBox3.Items.Add(AppID + "号未处理，原因：航班已起飞"); }
                    }
                }

                int notProcessedCount = needProcessCount - successCount;
                // 显示处理结果
                if (successCount > 0)
                {
                    listBox3.Items.Add($"成功处理订单数：{successCount}");
                }
                if (notProcessedCount > 0)
                {
                    listBox3.Items.Add($"未处理订单数：{notProcessedCount}"); 
                }
                if (needProcessCount == 0)
                {
                    listBox3.Items.Add("所有订单已经处理完成!");
                }
                
                RefreshData();
            }
            catch (Exception ex) { listBox3.Items.Add("添加失败" + ex.Message); }
            finally { 
                dBcon.con_close();
            }
        }
        // 去购买
        private void button17_Click(object sender, EventArgs e)
        {
            // 获取RichTextBox的文本内容
            string richTextBoxText = richTextBox1.Text;

            // 寻找label5.Text的位置
            int label5Index = richTextBoxText.IndexOf("尊敬的") + 4;
            int label5EndIndex = richTextBoxText.IndexOf("用户");
            string label5Value = richTextBoxText.Substring(label5Index, label5EndIndex - label5Index).Trim();
            // FStart
            int fstartIndex = richTextBoxText.IndexOf("出发地点：") + 5;
            int fstartEndIndex = richTextBoxText.IndexOf("到达地点：");
            string fstartValue = richTextBoxText.Substring(fstartIndex, fstartEndIndex - fstartIndex).Trim();
            // FEnd
            int fendIndex = richTextBoxText.IndexOf("到达地点：") + 5;
            int fendEndIndex = richTextBoxText.IndexOf("出发时间：");
            string fendValue = richTextBoxText.Substring(fendIndex, fendEndIndex - fendIndex).Trim();
            // 寻找FliID的位置
            int fliIDIndex = richTextBoxText.IndexOf("航班号：") + 4;
            int fliIDEndIndex = richTextBoxText.IndexOf("出发地点：");
            string fliIDValue = richTextBoxText.Substring(fliIDIndex, fliIDEndIndex - fliIDIndex).Trim();
            // StartTime
            int startTimeIndex = richTextBoxText.IndexOf("出发时间：") + 5;
            int startTimeEndIndex = richTextBoxText.IndexOf("基础票价：");
            DateTime startTimeValue = Convert.ToDateTime(richTextBoxText.Substring(startTimeIndex, startTimeEndIndex - startTimeIndex).Trim());
            // PriceBase
            int priceBaseIndex = richTextBoxText.IndexOf("基础票价：") + 5;
            int priceBaseEndIndex = richTextBoxText.IndexOf("您可以点击");
            string priceBaseValue = richTextBoxText.Substring(priceBaseIndex, priceBaseEndIndex - priceBaseIndex).Trim();

            PayForm payForm = new PayForm(label5Value, fstartValue, fendValue, priceBaseValue, fliIDValue, startTimeValue.ToString());
            payForm.ShowDialog();
        }
        // 改签
        private void button19_Click(object sender, EventArgs e)
        {
            // 选中待改签航班
            // 输入需改航班号
            if (textBox25.Text == "" || textBox26.Text == "")
            {
                listBox4.Items.Add("请先选择航班或输入需改航班号！");
            } else
            {
                //数据初始化
                string Old_FliID = textBox25.Text;
                string FliID = textBox26.Text;
                string FStart = "";
                string FEnd = "";
                string StartTime = "";
                string PriceBase = "";
                string TicID = this.dataGridView2[0, this.dataGridView2.CurrentCell.RowIndex].Value.ToString().Trim();
                string pname = this.dataGridView2[3, this.dataGridView2.CurrentCell.RowIndex].Value.ToString().Trim();
                string State = "";

                DBcon dBcon = new DBcon();
                string sql = "";
                try
                {
                    // 查表，完善数据
                    sql = $"select * from Flight where FliID = '{FliID}'";
                    SqlDataReader reader = dBcon.executeQuery(sql);
                    if (reader.Read())
                    {
                        FStart = reader["FStart"].ToString();
                        FEnd = reader["FEnd"].ToString();
                        StartTime = reader["StartTime"].ToString();
                        PriceBase = reader["PriceBase"].ToString();
                        State = reader["state"].ToString();
                    }
                    listBox4.Items.Add("数据获取成功！正在前往买票");
                }
                catch (Exception ex) { listBox4.Items.Add(ex.Message); }
                finally { dBcon.con_close(); }

                if (State != "1") { MessageBox.Show("航班在飞或已结束，不可买票"); }
                else
                {
                    // 进入支付页面购买新航班
                    PayForm payForm = new PayForm(label5.Text, FStart, FEnd, PriceBase, FliID, StartTime, pname);
                    payForm.ShowDialog();
                    // 是否支付成功
                    bool check_ok = payForm.Flag;
                    if (check_ok)
                    {
                        // 删除机票
                        sql = $"delete from Ticket where TicID = '{TicID}'";
                        // 发送改签消息（）
                        int num = dBcon.executeUpdate(sql);
                        if (num > 0)
                        {
                            // 发送改签消息
                            string time = DateTime.Now.ToString();
                            string content =
                                $"\n    尊敬的 {label5.Text} 用户，您的航班已成功改签,原航班({Old_FliID})机票已退款。\n" +
                                $"    新航班号为：{FliID}\n" +
                                $"    始发地：{FStart}---开往-->{FEnd}\n" +
                                $"    出发时间：{StartTime}\n" +
                                $"    请提前规划好时间安排，感谢您选择我们的服务，祝您旅途愉快！" +
                                $"\n\n    时间：{time}";
                            sql = $"insert into info([userID], [title], content) values('{label5.Text}', '改签成功！', '{content}')";
                            dBcon.executeUpdate(sql);  // 使用正确的 SQL 查询

                            listBox4.Items.Add($"改签成功！更改了{num}条记录，原航班号:'{Old_FliID}'-->新航班号:'{FliID}'");
                            listBox4.Items.Add("消息发送成功，请注意查收。");
                        }
                        else
                        {
                            listBox4.Items.Add($"找不到原订单");
                        }
                    }
                }
                
            }
            RefreshOrder();

        }
        // 退票
        private void button18_Click(object sender, EventArgs e)
        {
            // 删除机票
            string TicID = this.dataGridView2[0, this.dataGridView2.CurrentCell.RowIndex].Value.ToString().Trim();
            string FliID = this.dataGridView2[1, this.dataGridView2.CurrentCell.RowIndex].Value.ToString().Trim();
            DBcon dBcon = new DBcon();
            string sql = "";
            try
            {
                sql = $"delete from Ticket where TicID = '{TicID}'";

                DialogResult result = MessageBox.Show("你确定要退票吗？", "确认", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    int num = dBcon.executeUpdate(sql);
                    if (num > 0) 
                    {
                        // 发送退票消息
                        string pname = dataGridView2[3, dataGridView2.CurrentCell.RowIndex].Value.ToString().Trim();
                        string time = DateTime.Now.ToString();
                        string content = 
                            $"\n    尊敬的 {label5.Text} 用户，您的航班（航班号: {FliID}）退票申请已成功处理,机票已退款。" +
                            $"\n    感谢您选择我们的服务，欢迎再次使用！\n" +
                            $"\n    时间：{time}";
                        sql = $"insert into info([userID], [title], content) values('{label5.Text}', '退票成功！', '{content}')";
                        dBcon.executeUpdate(sql);  // 使用正确的 SQL 查询
                        listBox4.Items.Add($"删除成功！删除了{num}条记录，航班号:{FliID}，乘客:{pname}");
                        listBox4.Items.Add("消息发送成功，请注意查收。");
                    } else
                    {
                        listBox4.Items.Add($"找不到订单");
                    }
                }

            } catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { dBcon.con_close(); RefreshOrder(); }
        }
        // 用户点击cell显示数据
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox25.Text = this.dataGridView2[1, this.dataGridView2.CurrentCell.RowIndex].Value.ToString().Trim();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (comboBox3.SelectedIndex != -1)
            {
                foreach (DataRow row in ds_list.Tables["listP"].Rows)
                {
                    if (row["pname"].ToString() == comboBox3.SelectedItem.ToString())
                    {
                        textBox9.Text = row["pname"].ToString();
                        textBox11.Text = row["identify"].ToString();
                        textBox10.Text = row["contact"].ToString();
                    }
                }
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            
            tabPage5.Parent = tabControl1;
        }
    }
}
