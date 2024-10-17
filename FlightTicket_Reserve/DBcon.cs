using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlightTicket_Reserve
{
    internal class DBcon
    {

        //定义一个SqlConnection类型的公共变量My_con，用于判断数据库是否连接成功

        public static SqlConnection My_con;

        public static string M_str_sqlcon =

              @"Data Source=(local);Initial Catalog=PlaneTicket;integrated security=sspi";

        //建立数据库连接

        public static SqlConnection getcon()

        {

            My_con = new SqlConnection(M_str_sqlcon);

            //用SqlConnection对象与指定的数据库相连接

            My_con.Open();  //打开数据库连接

            return My_con;  //返回SqlConnection对象的信息

        }

        //关闭数据库连接

        public void con_close()

        {

            if (My_con.State == ConnectionState.Open)

            {//判断是否打开与数据库的连接

                My_con.Close();   //关闭数据库的连接

                My_con.Dispose();   //释放My_con变量的所有空间

            }

        }

        //获取指定表中的信息，执行提供的Select查询语句,返回SqlDataReader对象

        public SqlDataReader executeQuery(string SQLstr)

        {

            getcon();   //打开与数据库的连接

            SqlCommand My_com = My_con.CreateCommand(); //创建一个SqlCommand对象，用于执行SQL语句

            My_com.CommandText = SQLstr;    //获取指定的SQL语句

            SqlDataReader My_read = My_com.ExecuteReader(); //执行SQL语名句，生成一个SqlDataReader对象

            return My_read;

        }

        //执行insert update delete等更新语句

        public int executeUpdate(string SQLstr)

        {

            getcon();   //打开与数据库的连接

            SqlCommand SQLcom = new SqlCommand(SQLstr, My_con); //创建一个SqlCommand对象，用于执行SQL语句

            int result_len = SQLcom.ExecuteNonQuery();   //执行SQL语句

            SQLcom.Dispose();   //释放所有空间

            con_close();    //调用con_close()方法，关闭与数据库的连接

            return result_len;

        }

        //创建DataSet对象

        public DataSet getDataSet(string SQLstr, string tableName)

        {

            getcon();   //打开与数据库的连接

            SqlDataAdapter SQLda = new SqlDataAdapter(SQLstr, My_con);  //创建一个SqlDataAdapter对象，并获取指定数据表的信息

            DataSet My_DataSet = new DataSet(); //创建DataSet对象

            SQLda.Fill(My_DataSet, tableName);  //通过SqlDataAdapter对象的Fill()方法，将数据表信息添加到DataSet对象中

            con_close();    //关闭数据库的连接

            return My_DataSet;  //返回DataSet对象的信息

        }
    }
}
