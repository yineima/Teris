namespace Restart_tetris
{
    partial class helpdocument
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(helpdocument));
            this.helpaxAcroPDF1 = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)(this.helpaxAcroPDF1)).BeginInit();
            this.SuspendLayout();
            // 
            // helpaxAcroPDF1
            // 
            this.helpaxAcroPDF1.Enabled = true;
            this.helpaxAcroPDF1.Location = new System.Drawing.Point(44, 35);
            this.helpaxAcroPDF1.Name = "helpaxAcroPDF1";
            this.helpaxAcroPDF1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("helpaxAcroPDF1.OcxState")));
            this.helpaxAcroPDF1.Size = new System.Drawing.Size(192, 192);
            this.helpaxAcroPDF1.TabIndex = 0;
            // 
            // helpdocument
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.helpaxAcroPDF1);
            this.Name = "helpdocument";
            this.Text = "helpdocument";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.helpdocument_Load);
            ((System.ComponentModel.ISupportInitialize)(this.helpaxAcroPDF1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxAcroPDFLib.AxAcroPDF helpaxAcroPDF1;
    }
}