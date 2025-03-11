using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordAI
{
    public partial class ProgressForm : Form
    {
        internal bool isAborted = false;

        public ProgressForm()
        {
            InitializeComponent();
        }

        internal void SetProgress(int v, int totalParagraphs)
        {
            double p = ((double)v / (double)totalParagraphs) * 100;
            this.progressBar.Value = (int)p;
            Application.DoEvents();

        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            isAborted = true;
        }
    }
}
