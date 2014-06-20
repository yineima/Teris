using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Restart_tetris
{
    public partial class helpdocument : Form
    {
        public helpdocument()
        {
            InitializeComponent();
        }

        private void helpdocument_Load(object sender, EventArgs e)
        {
            //helpaxAcroPDF1.LoadFile(Application.StartupPath + "GPIB连接仪器.pdf");
            //helpaxAcroPDF1.Dock = DockStyle.Fill;
            System.Diagnostics.Process.Start(Application.StartupPath + "\\GPIB连接仪器.pdf");
        }
    }
}
