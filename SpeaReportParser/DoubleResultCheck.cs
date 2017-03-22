using System;
using System.IO;

namespace SpeaReportParser
{
    class DoubleResultCheck
    {
        public static void WriteResult(String SerialNumber, DateTime StartTime)
        {
            if (IsWritten(SerialNumber, StartTime))
                return;

            FileInfo myConfFile = new FileInfo("ResultLog.txt");
            StreamWriter mySW = new StreamWriter(myConfFile.Name, true);
            mySW.WriteLine(String.Concat(SerialNumber, ";", StartTime.ToString()));
            mySW.Close();
            mySW.Dispose();
        }

        public static Boolean IsWritten(String SerialNumber, DateTime StartTime)
        {
            if (!File.Exists("ResultLog.txt"))
            {
                File.Create("ResultLog.txt");
            }
            FileInfo myConfFile = new FileInfo("ResultLog.txt");
            StreamReader mySR = new StreamReader(myConfFile.OpenRead());

            while (!mySR.EndOfStream)
            {
                String[] actLineElements = mySR.ReadLine().Split(';');
                if ((actLineElements[0] == SerialNumber) && (Convert.ToDateTime(actLineElements[1]) == StartTime))
                {
                    mySR.Close();
                    mySR.Dispose();
                    return true;
                }
            }
            mySR.Close();
            mySR.Dispose();            
            return false;
        }
    }
}
