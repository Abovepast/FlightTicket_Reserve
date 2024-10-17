using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlightTicket_Reserve
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            /*            Application.Run(new Form1());*/

            Form1 form1 = new Form1();
            form1.ShowDialog();
            if (Form1.JoinFlag)
            {
                Application.Run(new MainForm(Form1.user, Form1.isM));
            }
            
        }

        public static void CenterFormOnScreen(Form form)
        {
            // 计算窗体在屏幕上的居中位置
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int formWidth = form.Width;
            int formHeight = form.Height;

            int x = (screenWidth - formWidth) / 2;
            int y = (screenHeight - formHeight) / 2;

            // 设置窗体位置
            form.Location = new Point(x, y);
        }

    }
}
