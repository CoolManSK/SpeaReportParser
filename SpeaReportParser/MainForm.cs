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
using System.Deployment.Application;

namespace SpeaReportParser
{
    public partial class MainForm : Form
    {
        private DirectoryInfo SearchDirectory;

        public MainForm()
        {
            InitializeComponent();
            this.Hide();
            this.Form1_Load(new object(), new EventArgs());
        }

        private Boolean afterUpdate = false;

        public void CheckForUpdateAndInstallIt()
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = ad.CheckForDetailedUpdate();
                }
                catch (DeploymentDownloadException dde)
                {
                    MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message);
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
                    return;
                }

                if (info.UpdateAvailable)
                {
                    try
                    {
                        ad.Update();
                        //MessageBox.Show("The application has been upgraded, and will now restart.");                        
                        this.afterUpdate = true;
                        Application.Restart();
                        Application.ExitThread();
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        MessageBox.Show("Nemoze sa nainstalovat najnovsia verzia programu. \n\nProsim zavolajte testovacieho inziniera.\n\n" + dde);
                        return;
                    }

                }
            }
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

                SpeaReport[] SpeaFileReports = { };

                while (!sr.EndOfStream)
                {
                    String[] ar_LineElements = sr.ReadLine().Split(';');
                    if (ar_LineElements[0] != "START") continue;

                    SpeaReport actSpeaReport = new SpeaReport();                    

                    if (sr.EndOfStream) continue;

                    /*
                    String[] ar_NextLE = sr.ReadLine().Split(';');
                    if (ar_NextLE[0] != "SN")
                    {

                    }
                    */

                    actSpeaReport.ReportHeader.ProductID = ar_LineElements[1];
                    actSpeaReport.ReportHeader.OperatorID = ar_LineElements[5];
                    DateTime startTime = new DateTime(
                       Convert.ToInt32(ar_LineElements[6].Substring(6, 4)),
                       Convert.ToInt32(ar_LineElements[6].Substring(0, 2)),
                       Convert.ToInt32(ar_LineElements[6].Substring(3, 2)),
                       Convert.ToInt32(ar_LineElements[7].Substring(0, 2)),
                       Convert.ToInt32(ar_LineElements[7].Substring(3, 2)),
                       Convert.ToInt32(ar_LineElements[7].Substring(6, 2)));
                    actSpeaReport.ReportHeader.StartTime = startTime.ToString();

                    ar_LineElements = sr.ReadLine().Split(';');

                    if (ar_LineElements[0] == "SN")
                    {                       
                        actSpeaReport.ReportHeader.SerialNumber = ar_LineElements[1];
                        ar_LineElements = sr.ReadLine().Split(';');
                    }
                    else
                    {
                        actSpeaReport.ReportHeader.SerialNumber = "0000000000000";
                    }

                    while (actSpeaReport.ReportHeader.SerialNumber.Length < 13)
                        actSpeaReport.ReportHeader.SerialNumber = String.Concat("0", actSpeaReport.ReportHeader.SerialNumber);

                    if (DoubleResultCheck.IsWritten(actSpeaReport.ReportHeader.SerialNumber, startTime))
                    {
                        actSpeaReport.Dispose();                 
                        continue;
                    }

                    while (!(ar_LineElements[0] == "BOARDRESULT"))
                    {
                        TestRunSpea actTRS = ParserFunctions.FormatLine(ar_LineElements);

                        Array.Resize(ref actSpeaReport.ReportBody.TestRuns, actSpeaReport.ReportBody.TestRuns.Length + 1);
                        actSpeaReport.ReportBody.TestRuns.SetValue(actTRS, actSpeaReport.ReportBody.TestRuns.Length - 1);                        

                        ar_LineElements = sr.ReadLine().Split(';');
                    }

                    actSpeaReport.ReportHeader.Grade = ar_LineElements[1];

                    ar_LineElements = sr.ReadLine().Split(';');

                    DateTime endTime = new DateTime(
                        Convert.ToInt32(ar_LineElements[2].Substring(6, 4)),
                        Convert.ToInt32(ar_LineElements[2].Substring(0, 2)),
                        Convert.ToInt32(ar_LineElements[2].Substring(3, 2)),
                        Convert.ToInt32(ar_LineElements[3].Substring(0, 2)),
                        Convert.ToInt32(ar_LineElements[3].Substring(3, 2)),
                        Convert.ToInt32(ar_LineElements[3].Substring(6, 2)));

                    actSpeaReport.ReportHeader.EndTime = endTime.ToString();

                    Array.Resize(ref SpeaFileReports, SpeaFileReports.Length + 1);
                    SpeaFileReports.SetValue(actSpeaReport, SpeaFileReports.Length - 1);
                }

                SpeaReport[] SpeaReportsToProcess = { };

                if (SpeaFileReports.Length == 0) continue;

                for (Int32 i = 0; i < SpeaFileReports.Length; i++)
                {                    
                    if ((SpeaFileReports[i].ReportHeader.Grade == "PASS") && (SpeaFileReports[i].ReportHeader.SerialNumber != "0000000000000"))
                    {
                        Array.Resize(ref SpeaReportsToProcess, SpeaReportsToProcess.Length + 1);
                        SpeaReportsToProcess.SetValue(SpeaFileReports[i], SpeaReportsToProcess.Length - 1);
                        continue;
                    }
                    if (i < SpeaFileReports.Length - 1)
                    {
                        if (SpeaFileReports[i+1].ReportHeader.SerialNumber == "0000000000000")
                        {
                            if (SpeaFileReports[i + 1].ReportHeader.Grade == "PASS")
                            {
                                SpeaFileReports[i].ReportHeader.Grade = "PASS";
                            }
                            for (Int32 j = 0; j < SpeaFileReports[i].ReportBody.TestRuns.Length; j++)                                
                            {
                                if (SpeaFileReports[i].ReportBody.TestRuns[j].TestGrade == "FAIL")
                                {
                                    for (Int32 k = 0; k < SpeaFileReports[i + 1].ReportBody.TestRuns.Length; k++)                                        
                                    {
                                        if (SpeaFileReports[i].ReportBody.TestRuns[j].TestName == SpeaFileReports[i + 1].ReportBody.TestRuns[k].TestName)
                                        {
                                            SpeaFileReports[i].ReportBody.TestRuns[j] = SpeaFileReports[i + 1].ReportBody.TestRuns[k];
                                            k = SpeaFileReports[i + 1].ReportBody.TestRuns.Length;
                                        }
                                    }
                                }
                            }                            
                        }
                        Array.Resize(ref SpeaReportsToProcess, SpeaReportsToProcess.Length + 1);
                        SpeaReportsToProcess.SetValue(SpeaFileReports[i], SpeaReportsToProcess.Length - 1);
                        continue;
                    }                                        
                }

                /*
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
                    
                    while (ar_LineElements[1].Length < 13)
                    {
                        ar_LineElements[1] = String.Concat("0", ar_LineElements[1]);
                    }
                    UR.Cathegory.Product.SerialNo = ar_LineElements[1];

                    if (DoubleResultCheck.IsWritten(UR.Cathegory.Product.SerialNo, startTime))
                    {
                        continue;
                    }
                    
                    UR.AddProperty("Work Order", UR.Cathegory.Product.SerialNo.Substring(0,8));
                    UR.TestRun.name = "INCIRCUIT TEST";
                    UR.TestRun.starttime = startTime;

                    ar_LineElements = sr.ReadLine().Split(';');
                    while (!(ar_LineElements[0] == "BOARDRESULT"))
                    {
                        TestRunSpea actTRS = ParserFunctions.FormatLine(ar_LineElements);

                        UR.TestRun.AddTestRunChild(actTRS.TestName, startTime, startTime, actTRS.TestGrade, actTRS.MeasUnit, actTRS.MeasValue, actTRS.LowLimit, actTRS.HighLimit);

                        ar_LineElements = sr.ReadLine().Split(';');
                    }

                    if (ar_LineElements[1] == "INTERRUPTED")
                    {
                        UR.TestRun.grade = "TERMINATE";
                    }
                    else
                    {
                        UR.TestRun.grade = ar_LineElements[1];
                    }


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
                */
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TrayIcon.Dispose();
            Application.Exit();
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!afterUpdate)
            {
                e.Cancel = true;
                this.Hide();
                this.timer_Main.Start();
            }
            this.afterUpdate = false;
            base.OnFormClosing(e);
        }
    }
}
