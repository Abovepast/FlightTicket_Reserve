using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace FlightTicket_Reserve
{
    public partial class Form1 : Form
    {
        public static bool JoinFlag = false;
        public static bool isM = false;
        public static string user = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() == "" || textBox2.Text.Trim() == "")
            {
                MessageBox.Show("请输入用户名或密码", "提示");
                return;
            }

            DBcon db = new DBcon();
            try
            {
                string sql_q = $"select password,isManager from [user] where [userID] = '{textBox1.Text.Trim()}'";
                SqlDataReader reader = db.executeQuery(sql_q);

                if (reader.HasRows)    //是否有行
                {
                    /*sdr.Read(); //一条一条访问，并自移动*/
                    if (reader.Read())
                    {
                        string pwd = reader["password"].ToString();
                        isM = reader.GetBoolean(1);

                        if (pwd == textBox2.Text.Trim())
                        {
                            JoinFlag = true;
                            user = textBox1.Text.Trim();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("密码错误!", "提示");
                            this.textBox1.Clear();
                            this.textBox2.Clear();
                        }
                    }
                    else
                    {
                        MessageBox.Show("1001");
                    }
                }
                else
                {
                    MessageBox.Show("用户未注册！", "提示");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("数据库连接不成功！" + ex.Message, "提示");
            }
            finally
            {
                db.con_close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 注册
        private void button4_Click(object sender, EventArgs e)
        {
            string username = textBox7.Text;
            string password = textBox3.Text;
            string confirmPassword = textBox6.Text;

            // 检查密码是否匹配
            if (password != confirmPassword)
            {
                MessageBox.Show("密码和确认密码不匹配");
                return;
            }

            // 插入用户表
            InsertUser(username, password);
            
        }
        // 插入用户表
        private void InsertUser(string username, string password)
        {
            DBcon dBcon = new DBcon();
            string query = "";
            try
            {
                query = $"select * from [user] where userID = '{username}'";
                SqlDataReader reader = dBcon.executeQuery(query);
                if (reader.Read())
                {
                    MessageBox.Show("注册失败，用户已存在");
                } else
                {
                    query = $"INSERT INTO [User] (UserID, Password) VALUES ('{username}', '{password}')";
                    dBcon.executeUpdate(query);
                    MessageBox.Show("注册成功！");
                    tabControl1.SelectedTab = tabPage1;
                    textBox1.Text = username ;
                    textBox2 .Text = password ;
                }
                
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { dBcon.con_close(); }
        }
}
}
