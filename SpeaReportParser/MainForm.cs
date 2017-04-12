using System;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using SigmaSure;
using System.Deployment.Application;

namespace SpeaReportParser
{
    public partial class MainForm : Form
    {
        private DirectoryInfo SearchDirectory;
        private BelMES BelMesObj;

        public MainForm()
        {
            InitializeComponent();
            this.Hide();
            this.Form1_Load(new object(), new EventArgs());
        }

        private Boolean afterUpdate = false;

        public int CheckForUpdateAndInstallIt()
        {
            int retVal = -1;

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
                    return -101;
                }
                catch (InvalidDeploymentException ide)
                {
                    MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                    return -102;
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
                    return -103;
                }

                if (info.UpdateAvailable)
                {
                    try
                    {
                        ad.Update();
                        //MessageBox.Show("The application has been upgraded, and will now restart.");                        
                        this.afterUpdate = true;
                        return 0;
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        MessageBox.Show("Nemoze sa nainstalovat najnovsia verzia programu. \n\nProsim zavolajte testovacieho inziniera.\n\n" + dde);
                        return -200;
                    }

                }
                else
                {
                    return retVal;
                }
               
            }
            return retVal;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            String SearchDirectory = ConfigFile.GetInitialSearchDirectory();
            if (!Directory.Exists(SearchDirectory))
            {
                ErrorHandling.Create("Search Directory not found. Please call Test Engineer.", true, true);
                return;                
            }
            this.SearchDirectory = new DirectoryInfo(ConfigFile.GetInitialSearchDirectory());
            this.BelMesObj = new BelMES();

            if (!this.BelMesObj.Activated)
            {
                ErrorHandling.Create("Unable to make connection with BelMes server. Application will close now. Please call Test Engineer", true, true);
            }

            Int32 n_timeIntervalMainTimer = ConfigFile.GetSearchInterval();
            if (n_timeIntervalMainTimer > 0)
            {
                
                this.timer_Main.Interval = n_timeIntervalMainTimer * 1000;
                this.timer_Main_Tick(new object(), new EventArgs());
                this.timer_Main.Start();
            }
            else
            {
                ErrorHandling.Create("Program could not be started. It will close now. Please call Test Engineer.", true, true);
                Application.Exit();
            }

            
        }

        private void timer_Main_Tick(object sender, EventArgs e)
        {
            String str_actualTrayIconText = this.TrayIcon.Text;
            this.TrayIcon.Text = "Working. Be patient.";

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
                    DateTime startTime;

                    try
                    {
                        startTime = new DateTime(
                           Convert.ToInt32(ar_LineElements[6].Substring(6, 4)),
                           Convert.ToInt32(ar_LineElements[6].Substring(0, 2)),
                           Convert.ToInt32(ar_LineElements[6].Substring(3, 2)),
                           Convert.ToInt32(ar_LineElements[7].Substring(0, 2)),
                           Convert.ToInt32(ar_LineElements[7].Substring(3, 2)),
                           Convert.ToInt32(ar_LineElements[7].Substring(6, 2)));
                    }
                    catch
                    {
                        startTime = new DateTime(
                           Convert.ToInt32(ar_LineElements[6].Substring(6, 4)),
                           Convert.ToInt32(ar_LineElements[6].Substring(3, 2)),
                           Convert.ToInt32(ar_LineElements[6].Substring(0, 2)),
                           Convert.ToInt32(ar_LineElements[7].Substring(0, 2)),
                           Convert.ToInt32(ar_LineElements[7].Substring(3, 2)),
                           Convert.ToInt32(ar_LineElements[7].Substring(6, 2)));
                    }
                    if (startTime < new DateTime(2017, 4, 6, 4, 0, 0)) continue; // only new reports are allowed to process

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

                    if (actSpeaReport.ReportHeader.SerialNumber.Length != 13) continue;                    

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
                    if ((SpeaFileReports[i].ReportHeader.SerialNumber == "0000000000000") && (SpeaReportsToProcess.Length == 0)) continue;
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

                foreach (SpeaReport actSRTP in SpeaReportsToProcess)
                {
                    UnitReport UR = new UnitReport();

                    UR.starttime = Convert.ToDateTime(actSRTP.ReportHeader.StartTime);
                    UR.endtime = Convert.ToDateTime(actSRTP.ReportHeader.EndTime);
                    
                    UR.Operator = new _Operator(ConfigFile.GetOperatorPersonalNumber(actSRTP.ReportHeader.OperatorID));

                    UR.Cathegory.Product.SerialNo = actSRTP.ReportHeader.SerialNumber;
                    UR.Cathegory.Product.PartNo = actSRTP.ReportHeader.ProductID;

                    UR.AddProperty("Work Order", UR.Cathegory.Product.SerialNo.Substring(0, 8));

                    UR.TestRun.name = "ICT";

                    foreach (TestRunSpea actTRS in actSRTP.ReportBody.TestRuns)
                    {
                        UR.TestRun.AddTestRunChild(actTRS.TestName, Convert.ToDateTime(actSRTP.ReportHeader.StartTime), Convert.ToDateTime(actSRTP.ReportHeader.EndTime), actTRS.TestGrade, actTRS.MeasUnit, actTRS.MeasValue, actTRS.LowLimit, actTRS.HighLimit);
                    }

                    UR.Cathegory.name = "Default";

                    UR.mode = "P";

                    UR.TestNumberPrefix = false;
                    
                    Array actReport = UR.GetXMLReport();
                    String str_actReport = "";
                    foreach (String actLine in actReport)
                    {
                        str_actReport = String.Concat(str_actReport, actLine);
                    }

                    DoubleResultCheck.WriteResult(UR.Cathegory.Product.SerialNo, UR.starttime);

                    if (!this.BelMesObj.EmployeeVerification(UR.Operator.ToString()))
                    {
                        ErrorHandling.Create("Neznamy operator.", false, false);
                    }
                    else
                    {
                        if (this.BelMesObj.BelMESAuthorization(UR.Cathegory.Product.SerialNo, "ICT", UR.Cathegory.Product.PartNo, ""))
                        {
                            Thread.Sleep(500);

                            if (this.BelMesObj.SetActualResult(UR.Cathegory.Product.SerialNo, "ICT", UR.TestRun.grade, str_actReport))
                            {
                                //DoubleResultCheck.WriteResult(UR.Cathegory.Product.SerialNo, UR.starttime);
                            }
                        }
                    }   
                    
                }

                this.TrayIcon.Text = str_actualTrayIconText;
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

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm myOF = new OptionsForm();
            myOF.ShowDialog();
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

            }
        }
    }
}
