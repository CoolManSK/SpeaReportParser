using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace SpeaReportParser
{
    class ConfigFile
    {
        public static String GetInitialSearchDirectory()
        {
            XmlDocument SC = new XmlDocument();
            if (File.Exists(@"Configurations/Station.xml"))
            {
                SC.Load(@"Configurations/Station.xml");
            }
            else
            {
                ErrorHandling.Create("Station config file not found. Please, call test engineer.", true, true);
                //ErrorHandling.Create()
                return "";
            }

            XmlNode configSearchDirectoryNode = SC.SelectSingleNode("./Configuration/SearchDirectory");
            if (configSearchDirectoryNode == null)
            {
                ErrorHandling.Create("Configuration/SearchDirectory node missing in Station config file. Please, call test engineer", true, true);
                return "";
            }
            return configSearchDirectoryNode.InnerText.Trim();
        }

        public static void SetInitialSearchDirectory(String SearchDirectory)
        {
            XmlDocument SC = new XmlDocument();
            if (File.Exists(@"Configurations/Station.xml"))
            {
                SC.Load(@"Configurations/Station.xml");
            }
            else
            {
                ErrorHandling.Create("Station config file not found. Please, call test engineer.", true, true);
                return;
            }

            XmlNode configSearchDirectoryNode = SC.SelectSingleNode("./Configuration/SearchDirectory");
            if (configSearchDirectoryNode == null)
            {
                ErrorHandling.Create("Configuration/SearchDirectory node missing in Station config file. Please, call test engineer", true, true);
                return;
            }
            configSearchDirectoryNode.InnerText = SearchDirectory;
            SC.Save(@"Configurations/Station.xml");
        }

        public static Int32 GetSearchInterval()
        {
            XmlDocument SC = new XmlDocument();
            if (File.Exists(@"Configurations/Station.xml"))
            {
                SC.Load(@"Configurations/Station.xml");
            }
            else
            {
                ErrorHandling.Create("Station config file not found. Please, call test engineer.", true, true);
                return -1;
            }

            XmlNode configSearchIntervalNode = SC.SelectSingleNode("./Configuration/SearchInterval");
            if (configSearchIntervalNode == null)
            {
                ErrorHandling.Create("Configuration/SearchInterval node missing in Station config file. Please, call test engineer.", true, true);
                return -1;
            }
            Int32 retVal = 0;
            try
            {
                retVal = Convert.ToInt32(configSearchIntervalNode.InnerText);
            }
            catch
            {
                ErrorHandling.Create("Wrong value in Configuration/SearchInterval node in Station config file. Please, call test engineer.", true, true);
                return -1;
            }
            return retVal;
        }

        public static void SetSearchInterval(Int32 SearchInterval)
        {
            XmlDocument SC = new XmlDocument();
            if (File.Exists(@"Configurations/Station.xml"))
            {
                SC.Load(@"Configurations/Station.xml");
            }
            else
            {
                ErrorHandling.Create("Station config file not found. Please, call test engineer.", true, true);
                return;
            }

            XmlNode configSearchIntervalNode = SC.SelectSingleNode("./Configuration/SearchInterval");
            if (configSearchIntervalNode == null)
            {
                ErrorHandling.Create("Configuration/SearchInterval node missing in Station config file. Please, call test engineer.", true, true);
                return;
            }

            configSearchIntervalNode.InnerText = SearchInterval.ToString().Trim();
            SC.Save(@"Configurations/Station.xml");
        }

        public static String GetOperatorPersonalNumber(String OperatorName)
        {
            String retVal = "-1";

            XmlDocument OC = new XmlDocument();
            if (File.Exists(@"Configurations/Operators.xml"))
            {
                OC.Load(@"Configurations/Operators.xml");
            }
            else
            {
                ErrorHandling.Create("Operators config file not found. Please, call test engineer.", true, true);
                return "-2";
            }

            XmlNode actualOperatorNode = OC.SelectSingleNode(String.Concat("./Operators/", OperatorName));
            if (actualOperatorNode == null)
            {
                ErrorHandling.Create(String.Concat("Operator \"", OperatorName, "\" not found in database of operators."), false, true);
            }
            else
            {
                retVal = actualOperatorNode.SelectSingleNode("./OSNumber").InnerText;
            }
            return retVal;
        }
    }
}
