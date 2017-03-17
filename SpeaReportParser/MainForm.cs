using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SpeaReportParser
{
    public partial class MainForm : Form
    {
        private DirectoryInfo SearchDirectory;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.SearchDirectory = new DirectoryInfo(ConfigFile.GetInitialSearchDirectory());

            Int32 n_timeIntervalMainTimer = ConfigFile.GetSearchInterval();
            if (n_timeIntervalMainTimer > -1)
            {
                this.timer_Main.Interval = n_timeIntervalMainTimer * 1000;
                this.timer_Main_Tick(new object(), new EventArgs());
                this.timer_Main.Start();
            }
            else
            {
                ErrorHandling.Create("Program could not be started. It will close now.", true, false);
                Application.Exit();
            }    
        }

        private void timer_Main_Tick(object sender, EventArgs e)
        {
            foreach (FileInfo actFI in this.SearchDirectory.GetFiles())
            {
                if (actFI.Extension.ToLower() != "txt") continue;
                StreamReader sr = new StreamReader(actFI.OpenRead());
            }
        }
    }
}
