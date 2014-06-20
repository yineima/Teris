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
    public partial class set_notice : Form
    {
        public set_notice()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ((MainForm)this.Owner).label2.Text = this.comboBox1.Text;
            this.Close();
        }
    }
}
