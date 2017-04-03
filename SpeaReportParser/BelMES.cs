using System;
using System.Windows.Forms;
using System.IO;
using BelMESCommon;

namespace SpeaReportParser
{
    public class BelMES
    {
        public clEnvironment Env = new clEnvironment();
        public clEmployee Emp = new clEmployee();
        public clAuthorization Authorization = new clAuthorization();
        private String LogFilePath = @"\\dcafs3\share\Manufacturing_Engineering\Public\Kolman Vladimir\BelMESCommon\SSMRG_BELLogs\";
        private String LogFileName = "";
        public Boolean Activated = false;
        public String Mode = "D";

        public BelMES()
        {
            try
            {
                //String userName = "trska";
                //this.Env = this.Env.SetEnvironment(userName);
                this.Env = this.Env.SetEnvironment();
                if (this.Env != null)
                {
                    if ((this.Env.strComputer == "") || (this.Env.strComputer == null))
                        this.LogFileName = String.Concat(Environment.MachineName, "_", String.Format("{0:yyyMMdd}", DateTime.Now), ".txt");
                    else
                        this.LogFileName = String.Concat(this.Env.strComputer, "_", String.Format("{0:yyyyMMdd}", DateTime.Now), ".txt");
                    if (this.Env.blnAuthorizationPaused != false) this.Env.blnAuthorizationPaused = false;
                    this.Activated = true;
                    this.Env.Employee = this.Emp;
                    this.WriteLogData("");
                }
                else
                {
                    this.LogFileName = String.Concat(Environment.MachineName, "_", String.Format("{0:yyyyMMdd}", DateTime.Now), ".txt");
                }

                if (!File.Exists(String.Concat(this.LogFilePath, this.LogFileName)))
                {
                    File.Create(String.Concat(this.LogFilePath, this.LogFileName));
                }

            }
            catch
            {
                this.WriteLogData("");
                this.Activated = false;
            }
        }
        public Boolean BelMESAuthorization(String SerialNumber, String TestType, String ProductName, String XmlContent)
        {
            if (TestType == "Adjustement") TestType = "Adjustment";
            return this.BelMESAuthorization(SerialNumber, TestType, ProductName, XmlContent, false);
        }

        public Boolean BelMESAuthorization(String SerialNumber, String TestType, String ProductName, String XmlContent, Boolean ForceTerminated)
        {
            if (TestType == "Adjustement") TestType = "Adjustment";

            if (this.Env.Employee.strEmployeeNumber == null)
            {
                this.Env.Employee = this.Emp;
            }

            if ((this.Authorization.strWO_SerialNumber != null) && ForceTerminated)
            {
                this.Authorization = this.Authorization.TryAuthorization(this.Authorization.strWO_SerialNumber, this.Authorization.strTestKind, "Terminated", this.Env, true, false, this.Mode, XmlContent);
            }
            if ((SerialNumber.Length == 13) && (TestType != ""))
            {
                this.Authorization = this.Authorization.TryAuthorization(SerialNumber, TestType, "", this.Env, true, true, this.Mode);
                this.Authorization.strTestKind = TestType;
            }
            this.WriteLogData(SerialNumber);
            Boolean b_Verified = true;
            if (this.Authorization.blnAuthorized)
            {
                if (this.Authorization.strItem != ProductName)
                    b_Verified = false;
            }
            else
            {
                b_Verified = false;
            }
            return b_Verified;
        }

        public Boolean SetActualResult(String Result, String XmlReportString, String SerialNumber)
        {
            Boolean retVal = false;

            if (this.Env.Employee.strEmployeeNumber == null)
            {
                this.Env.Employee = this.Emp;
            }

            try
            {
                if (this.Authorization.strWO_SerialNumber != null)
                {
                    if (this.Authorization.strWO_SerialNumber != "")
                    {
                        this.Authorization = this.Authorization.TryAuthorization(this.Authorization.strWO_SerialNumber, this.Authorization.strTestKind, Result, this.Env, true, true, this.Mode, XmlReportString);
                    }
                    else
                    {
                        this.Authorization = this.Authorization.TryAuthorization(SerialNumber, this.Authorization.strTestKind, Result, this.Env, true, true, this.Mode, XmlReportString);
                    }
                }
                else
                {
                    this.Authorization = this.Authorization.TryAuthorization(SerialNumber, this.Authorization.strTestKind, Result, this.Env, true, true, this.Mode, XmlReportString);
                }
                /*
                if ((this.Authorization.strWO_SerialNumber != null))
                {
                    this.Authorization = this.Authorization.TryAuthorization(this.Authorization.strWO_SerialNumber, this.Authorization.strTestKind, Result, this.Env, true, false, XmlReportString);
                }
                else
                {
                    retVal = false;
                }
                */
            }
            catch
            {
                this.WriteLogData("");
                return false;
            }
            this.WriteLogData("");
            return retVal;
        }

        public Boolean SetActualResult(String SerialNumber, String TestKind, String Result, String XmlReportString)
        {
            Boolean retVal = false;

            if (this.Env.Employee.strEmployeeNumber == null)
            {
                this.Env.Employee = this.Emp;
            }

            if (TestKind == "Adjustement") TestKind = "Adjustment";
            try
            {
                this.Authorization = this.Authorization.TryAuthorization(SerialNumber, TestKind, Result, this.Env, true, true, this.Mode, XmlReportString);
                /*
                if ((this.Authorization.strWO_SerialNumber != null))
                {
                    this.Authorization = this.Authorization.TryAuthorization(this.Authorization.strWO_SerialNumber, this.Authorization.strTestKind, Result, this.Env, true, false, XmlReportString);
                }
                else
                {
                    retVal = false;
                }
                */
            }
            catch
            {
                this.WriteLogData("");
                return false;
            }
            this.WriteLogData("");
            return retVal;
        }

        public Boolean EmployeeVerification(String EmployeeNumber)
        {
            try
            {
                this.Emp = this.Emp.EmployeeVerify(EmployeeNumber, this.Env.DB_Resource, this.Env.ESD_DB_Resource);
                if (this.Emp == null)
                {
                    this.Activated = false;
                    //MessageBox.Show("Employee Number does not exist");  
                    this.WriteLogData(EmployeeNumber);
                    return false;
                }
                else
                {
                    if (this.Emp.shtEmployeeID > 0 && string.IsNullOrEmpty(this.Emp.strEmployeeCodeInfo))
                    {
                        this.Env.Employee = new clEmployee(this.Emp.shtEmployeeID, this.Emp.strEmployeeName, EmployeeNumber, this.Emp.strEmployeeDepartment, this.Emp.strEmployeeProduction, this.Emp.strEmployeeCodeInfo);
                        this.WriteLogData(EmployeeNumber);
                    }
                    else
                    {
                        this.Activated = false;
                        //MessageBox.Show(this.Emp.strEmployeeCodeInfo);  
                        this.WriteLogData(EmployeeNumber);
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                this.WriteLogData(EmployeeNumber);
                this.Activated = false;
                return false;
            }
        }

        private void WriteLogData(String ExtendedInfo)
        {
            try
            {
                StreamWriter sr = new StreamWriter(String.Concat(this.LogFilePath, this.LogFileName), true);
                String newLine = String.Concat(
                    DateTime.Now.Hour.ToString("D2"), ":", DateTime.Now.Minute.ToString("D2"), ":", DateTime.Now.Second.ToString("D2"), ";",
                    (this.Env.strTracePoint == null) ? "nullTracePoint" : this.Env.strTracePoint.ToString(), ";",
                    (this.Env.Employee.strEmployeeNumber == null) ? "nullEmpNumber" : this.Env.Employee.strEmployeeNumber.ToString(), ";",
                    (this.Authorization.strSerialNumber == null) ? "nullSN1" : this.Authorization.strSerialNumber.ToString(), ";",
                    (this.Authorization.strWO_SerialNumber == null) ? "nullSN2" : this.Authorization.strWO_SerialNumber.ToString(), ";",
                    this.Authorization.blnMustTraced, ";",
                    this.Authorization.blnTraceItemStart.ToString(), ";",
                    (this.Authorization.strTestKind == null) ? "nullTestKind" : this.Authorization.strTestKind.ToString(), ";",
                    (this.Authorization.strStatus == null) ? "nullStatus" : this.Authorization.strStatus.ToString(), ";",
                    (this.Authorization.strResult == null) ? "nullResult" : this.Authorization.strResult.ToString(), ";",
                    (this.Env.DB_Resource.strSQLDatabase == null) ? "nullDatabase" : this.Env.DB_Resource.strSQLDatabase, ";",
                    (this.Env.DB_Resource.strSQLServer == null) ? "nullServer" : this.Env.DB_Resource.strSQLServer, ";",
                    this.Env.blnAuthorizationPaused, ";", this.Activated, ";", ExtendedInfo);
                sr.WriteLine(newLine);
                sr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Concat(ex.Message, "/n", ex.Data.ToString()));
            }
        }
    }
}
