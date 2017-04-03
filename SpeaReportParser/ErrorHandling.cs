using System;
using System.Windows.Forms;

namespace SpeaReportParser
{
    class ErrorHandling
    {
        public static void Create(String ErrorMessage, Boolean ShowMessage, Boolean Critical)
        {
            if (ShowMessage)
            {
                if (Critical) MessageBox.Show(ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show(ErrorMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            
        }
    }
}
