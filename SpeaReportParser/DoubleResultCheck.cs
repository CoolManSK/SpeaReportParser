using System;
using System.IO;

namespace SpeaReportParser
{
    class DoubleResultCheck
    {
        const String ResultLogFile = @"C:/Users/Public/Documents/SpeaProcesedResultsDoNotDelete.txt";

        public static void WriteResult(String SerialNumber, DateTime StartTime)
        {
            if (IsWritten(SerialNumber, StartTime))
                return;

            StreamWriter mySW = new StreamWriter(ResultLogFile, true);
            mySW.WriteLine(String.Concat(SerialNumber, ";", StartTime.ToString()));
            mySW.Close();
            mySW.Dispose();
        }

        public static Boolean IsWritten(String SerialNumber, DateTime StartTime)
        {
            StreamReader mySR;
            if (!File.Exists(ResultLogFile))
            {
                mySR = new StreamReader(File.Create(ResultLogFile));
            }
            else
            {
                mySR = new StreamReader(File.OpenRead(ResultLogFile));
            }

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

        public static void ResetResultLog()
        {
            if (File.Exists(ResultLogFile))
            {
                File.Delete(ResultLogFile);                
            }
            File.Create(ResultLogFile);
        }
    }
}
