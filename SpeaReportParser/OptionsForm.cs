using System;
using System.Windows.Forms;

namespace SpeaReportParser
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
        }

        private void Options_Load(object sender, EventArgs e)
        {
            this.tb_SearchDirectory.Text = ConfigFile.GetInitialSearchDirectory();
            this.nud_ScanInterval.Value = Convert.ToInt32(ConfigFile.GetSearchInterval());
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            ConfigFile.SetInitialSearchDirectory(this.tb_SearchDirectory.Text.Trim());
            ConfigFile.SetSearchInterval((Int32)this.nud_ScanInterval.Value);
            this.Close();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
