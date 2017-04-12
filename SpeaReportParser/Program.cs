using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SpeaReportParser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //MessageBox.Show(Application.ExecutablePath);
            MainForm myForm = new MainForm();
            int res = myForm.CheckForUpdateAndInstallIt();
            //MessageBox.Show(res.ToString());
            if (res == 0)
            {
                myForm.Dispose();
                Application.Restart();
            }
            else
            {
                Application.Run();
            }            
        }
    }
}
