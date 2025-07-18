namespace XenAdmin.Controls.Ballooning
{
    partial class MemorySpinner
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MemorySpinner));
            this.Spinner = new System.Windows.Forms.NumericUpDown();
            this.SpinnerUnits = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.Spinner)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Spinner
            // 
            resources.ApplyResources(this.Spinner, "Spinner");
            this.Spinner.DecimalPlaces = 1;
            this.Spinner.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.Spinner.Name = "Spinner";
            this.Spinner.ValueChanged += new System.EventHandler(this.Spinner_ValueChanged);
            this.Spinner.Leave += new System.EventHandler(this.Spinner_Leave);
            // 
            // SpinnerUnits
            // 
            resources.ApplyResources(this.SpinnerUnits, "SpinnerUnits");
            this.SpinnerUnits.Name = "SpinnerUnits";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.SpinnerUnits, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Spinner, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // MemorySpinner
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Name = "MemorySpinner";
            ((System.ComponentModel.ISupportInitialize)(this.Spinner)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown Spinner;
        private System.Windows.Forms.Label SpinnerUnits;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
