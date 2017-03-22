using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SigmaSure;

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
                if (actFI.Extension.ToLower() != ".txt") continue;
                StreamReader sr = new StreamReader(actFI.OpenRead());
                String actualLine;
                while (!sr.EndOfStream)
                {
                    actualLine = sr.ReadLine();
                    if (actualLine.Substring(0, 5) != "START") continue;

                    UnitReport UR = new UnitReport();

                    String[] ar_LineElements = actualLine.Split(';');

                    DateTime startTime = new DateTime(
                        Convert.ToInt32(ar_LineElements[6].Substring(6, 4)), 
                        Convert.ToInt32(ar_LineElements[6].Substring(0, 2)), 
                        Convert.ToInt32(ar_LineElements[6].Substring(3, 2)), 
                        Convert.ToInt32(ar_LineElements[7].Substring(0, 2)),
                        Convert.ToInt32(ar_LineElements[7].Substring(3, 2)),
                        Convert.ToInt32(ar_LineElements[7].Substring(6, 2)));

                    UR.starttime = startTime;

                    UR.Operator = new _Operator(ar_LineElements[5]);

                    ar_LineElements = sr.ReadLine().Split(';');

                    UR.Cathegory.Product.SerialNo = ar_LineElements[1];
                    UR.AddProperty("Work Order", "00000000");
                    UR.TestRun.name = "INCIRCUIT TEST";
                    UR.TestRun.starttime = startTime;

                    ar_LineElements = sr.ReadLine().Split(';');
                    while (!(ar_LineElements[0] == "BOARDRESULT"))
                    {
                        TestRunSpea actTRS = ParserFunctions.FormatLine(ar_LineElements);

                        UR.TestRun.AddTestRunChild(actTRS.TestName, startTime, startTime, actTRS.TestGrade, actTRS.MeasUnit, actTRS.MeasValue, actTRS.LowLimit, actTRS.HighLimit);

                        ar_LineElements = sr.ReadLine().Split(';');
                    }

                    UR.TestRun.grade = ar_LineElements[1];

                    ar_LineElements = sr.ReadLine().Split(';');

                    DateTime endTime = new DateTime(
                        Convert.ToInt32(ar_LineElements[2].Substring(6, 4)),
                        Convert.ToInt32(ar_LineElements[2].Substring(0, 2)),
                        Convert.ToInt32(ar_LineElements[2].Substring(3, 2)),
                        Convert.ToInt32(ar_LineElements[3].Substring(0, 2)),
                        Convert.ToInt32(ar_LineElements[3].Substring(3, 2)),
                        Convert.ToInt32(ar_LineElements[3].Substring(6, 2)));

                    foreach (_TestRunStep actTRS in UR.TestRun.Testruns)
                    {
                        actTRS.endtime = endTime;
                    }

                    UR.endtime = endTime;
                    UR.TestRun.endtime = endTime;

                    

                    Array myReport = UR.GetXMLReport(@"D:/temp/", true);

                    DoubleResultCheck.WriteResult(UR.Cathegory.Product.SerialNo, UR.starttime);
                }
            }
        }
    }
}
