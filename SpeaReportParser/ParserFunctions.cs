using System;
using System.Globalization;

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

    class ParserFunctions
    {
        public static TestRunSpea FormatLine(String[] LineElements)
        {
            TestRunSpea retVal = new TestRunSpea();

            retVal.TestName = LineElements[5];
            retVal.TestGrade = LineElements[7];
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
