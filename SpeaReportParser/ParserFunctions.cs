using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SpeaReportParser
{
    struct TestRunSpea
    {
        public String TestName;
        public String TestGrade;
        public Double LowLimit;
        public Double HighLimit;
        public Double MeasValue;
        public String MeasUnit;
    }

    struct SpeaReportHeader
    {
        public String ProductID;
        public String SerialNumber;
        public String OperatorID;
        public String StartTime;
        public String EndTime;
        public String Grade;
    }

        

    class SpeaReport : IDisposable
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
        public class SpeaReportBody
        {
            public TestRunSpea[] TestRuns = { };
        }

        public SpeaReportHeader ReportHeader;
        public SpeaReportBody ReportBody;
        public SpeaReport()
        {
            this.ReportHeader = new SpeaReportHeader();
            this.ReportHeader.Grade = "";
            this.ReportHeader.EndTime = "";
            this.ReportHeader.OperatorID = "";
            this.ReportHeader.ProductID = "";
            this.ReportHeader.SerialNumber = "";
            this.ReportHeader.StartTime = "";

            this.ReportBody = new SpeaReportBody();            
        }        

        public void AddTestRun(String[] LineElements)
        {
            Array.Resize(ref this.ReportBody.TestRuns, this.ReportBody.TestRuns.Length + 1);
            this.ReportBody.TestRuns.SetValue(ParserFunctions.FormatLine(LineElements), this.ReportBody.TestRuns.Length - 1);
        }
    }

    class ParserFunctions
    {
        public static TestRunSpea FormatLine(String[] LineElements)
        {
            TestRunSpea retVal = new TestRunSpea();

            retVal.TestName = LineElements[5];
            if (LineElements[7].Substring(0, 4) == "FAIL")
            {
                retVal.TestGrade = LineElements[7].Substring(0, 4);
            }
            else
            {
                retVal.TestGrade = LineElements[7];
            }            
            retVal.LowLimit = ConvertToDouble(LineElements[9]);
            retVal.HighLimit = ConvertToDouble(LineElements[10]);
            retVal.MeasValue = ConvertToDouble(LineElements[8]);
            retVal.MeasUnit = LineElements[11];

            return retVal;
        }

        private static Double ConvertToDouble(String inputString)
        {
            Double retVal = 0;

            inputString = inputString.Replace('.', ',');

            Int32 ePos = inputString.IndexOf('e');
            Boolean isEPositive;
            if (inputString.Substring(ePos + 1, 1) == "+")
                isEPositive = true;
            else if (inputString.Substring(ePos + 1, 1) == "-")
                isEPositive = false;
            else
            {
                ErrorHandling.Create("Value convert error", false, false);
                return 0;
            }
            Int32 Evalue = Convert.ToInt32(inputString.Substring(ePos + 2));

            retVal = Convert.ToDouble(inputString.Substring(0, ePos));

            if (isEPositive)
            {
                retVal = retVal * (Math.Pow(10, Evalue));
            }
            else
            {
                retVal = retVal / (Math.Pow(10, Evalue));
            }
            return retVal;
        }
    }
}
