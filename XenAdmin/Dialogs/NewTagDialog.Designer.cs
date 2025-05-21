namespace XenAdmin.Dialogs
{
    partial class NewTagDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewTagDialog));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.NewTagLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.addButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tagsDataGrid = new XenAdmin.Controls.DataGridViewEx.DataGridViewEx();
            this.ColumnChecked = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.tagsDataGrid)).BeginInit();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // NewTagLabel
            // 
            resources.ApplyResources(this.NewTagLabel, "NewTagLabel");
            this.NewTagLabel.Name = "NewTagLabel";
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            this.textBox1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.textBox1_PreviewKeyDown);
            // 
            // addButton
            // 
            resources.ApplyResources(this.addButton, "addButton");
            this.addButton.Name = "addButton";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // tagsDataGrid
            // 
            resources.ApplyResources(this.tagsDataGrid, "tagsDataGrid");
            this.tagsDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.tagsDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tagsDataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.tagsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.tagsDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnChecked,
            this.ColumnTags});
            this.mainTableLayoutPanel.SetColumnSpan(this.tagsDataGrid, 3);
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tagsDataGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.tagsDataGrid.MultiSelect = true;
            this.tagsDataGrid.Name = "tagsDataGrid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tagsDataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.tagsDataGrid.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.tagsDataGrid_CellMouseClick);
            this.tagsDataGrid.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.tagsDataGrid_SortCompare);
            this.tagsDataGrid.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tagsDataGrid_KeyPress);
            // 
            // ColumnChecked
            // 
            this.ColumnChecked.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            resources.ApplyResources(this.ColumnChecked, "ColumnChecked");
            this.ColumnChecked.Name = "ColumnChecked";
            this.ColumnChecked.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ColumnChecked.ThreeState = true;
            // 
            // ColumnTags
            // 
            this.ColumnTags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.ColumnTags, "ColumnTags");
            this.ColumnTags.Name = "ColumnTags";
            this.ColumnTags.ReadOnly = true;
            // 
            // mainTableLayoutPanel
            // 
            resources.ApplyResources(this.mainTableLayoutPanel, "mainTableLayoutPanel");
            this.mainTableLayoutPanel.Controls.Add(this.cancelButton, 2, 2);
            this.mainTableLayoutPanel.Controls.Add(this.tagsDataGrid, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.textBox1, 1, 0);
            this.mainTableLayoutPanel.Controls.Add(this.addButton, 2, 0);
            this.mainTableLayoutPanel.Controls.Add(this.okButton, 1, 2);
            this.mainTableLayoutPanel.Controls.Add(this.NewTagLabel, 0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            // 
            // NewTagDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.mainTableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NewTagDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Activated += new System.EventHandler(this.NewTagDialog_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.tagsDataGrid)).EndInit();
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label NewTagLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private Controls.DataGridViewEx.DataGridViewEx tagsDataGrid;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnChecked;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTags;
    }
}
