namespace GRIDDA
{
    partial class GenericCSVImport
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
            this.selectFileButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.selectFileGroup = new System.Windows.Forms.GroupBox();
            this.fileSelectionBar = new System.Windows.Forms.HScrollBar();
            this.openFileButton = new System.Windows.Forms.Button();
            this.hasHeaderCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.indexLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.previewGroupBox = new System.Windows.Forms.GroupBox();
            this.csvDataGridView = new System.Windows.Forms.DataGridView();
            this.selectFileGroup.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.previewGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.csvDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // selectFileButton
            // 
            this.selectFileButton.Location = new System.Drawing.Point(12, 12);
            this.selectFileButton.Name = "selectFileButton";
            this.selectFileButton.Size = new System.Drawing.Size(75, 23);
            this.selectFileButton.TabIndex = 0;
            this.selectFileButton.Text = "Open";
            this.selectFileButton.UseVisualStyleBackColor = true;
            this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // selectFileGroup
            // 
            this.selectFileGroup.Controls.Add(this.fileSelectionBar);
            this.selectFileGroup.Controls.Add(this.openFileButton);
            this.selectFileGroup.Controls.Add(this.hasHeaderCheckbox);
            this.selectFileGroup.Controls.Add(this.selectFileButton);
            this.selectFileGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.selectFileGroup.Location = new System.Drawing.Point(0, 0);
            this.selectFileGroup.Name = "selectFileGroup";
            this.selectFileGroup.Size = new System.Drawing.Size(458, 65);
            this.selectFileGroup.TabIndex = 2;
            this.selectFileGroup.TabStop = false;
            // 
            // fileSelectionBar
            // 
            this.fileSelectionBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.fileSelectionBar.LargeChange = 1;
            this.fileSelectionBar.Location = new System.Drawing.Point(3, 40);
            this.fileSelectionBar.Maximum = 0;
            this.fileSelectionBar.Name = "fileSelectionBar";
            this.fileSelectionBar.Size = new System.Drawing.Size(452, 22);
            this.fileSelectionBar.TabIndex = 3;
            this.fileSelectionBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.fileSelectionBar_Scroll);
            // 
            // openFileButton
            // 
            this.openFileButton.Location = new System.Drawing.Point(180, 14);
            this.openFileButton.Name = "openFileButton";
            this.openFileButton.Size = new System.Drawing.Size(75, 23);
            this.openFileButton.TabIndex = 2;
            this.openFileButton.Text = "Done";
            this.openFileButton.UseVisualStyleBackColor = true;
            this.openFileButton.Click += new System.EventHandler(this.openFileButton_Click);
            // 
            // hasHeaderCheckbox
            // 
            this.hasHeaderCheckbox.AutoSize = true;
            this.hasHeaderCheckbox.Location = new System.Drawing.Point(93, 16);
            this.hasHeaderCheckbox.Name = "hasHeaderCheckbox";
            this.hasHeaderCheckbox.Size = new System.Drawing.Size(81, 17);
            this.hasHeaderCheckbox.TabIndex = 1;
            this.hasHeaderCheckbox.Text = "Has header";
            this.hasHeaderCheckbox.UseVisualStyleBackColor = true;
            this.hasHeaderCheckbox.CheckedChanged += new System.EventHandler(this.hasHeaderCheckbox_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.indexLayoutPanel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 65);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 19);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = ".CSV Column Indices";
            // 
            // indexLayoutPanel
            // 
            this.indexLayoutPanel.AutoSize = true;
            this.indexLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.indexLayoutPanel.ColumnCount = 2;
            this.indexLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.indexLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.indexLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indexLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.indexLayoutPanel.Name = "indexLayoutPanel";
            this.indexLayoutPanel.RowCount = 1;
            this.indexLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.indexLayoutPanel.Size = new System.Drawing.Size(452, 0);
            this.indexLayoutPanel.TabIndex = 0;
            // 
            // previewGroupBox
            // 
            this.previewGroupBox.Controls.Add(this.csvDataGridView);
            this.previewGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewGroupBox.Location = new System.Drawing.Point(0, 84);
            this.previewGroupBox.Name = "previewGroupBox";
            this.previewGroupBox.Size = new System.Drawing.Size(458, 484);
            this.previewGroupBox.TabIndex = 4;
            this.previewGroupBox.TabStop = false;
            this.previewGroupBox.Text = "File preview";
            // 
            // csvDataGridView
            // 
            this.csvDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.csvDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.csvDataGridView.Location = new System.Drawing.Point(3, 16);
            this.csvDataGridView.Name = "csvDataGridView";
            this.csvDataGridView.Size = new System.Drawing.Size(452, 465);
            this.csvDataGridView.TabIndex = 1;
            // 
            // GenericCSVImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 568);
            this.Controls.Add(this.previewGroupBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.selectFileGroup);
            this.Name = "GenericCSVImport";
            this.Text = "Open CSV";
            this.Load += new System.EventHandler(this.GenericCSVImport_Load);
            this.selectFileGroup.ResumeLayout(false);
            this.selectFileGroup.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.previewGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.csvDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button selectFileButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox selectFileGroup;
        private System.Windows.Forms.Button openFileButton;
        private System.Windows.Forms.CheckBox hasHeaderCheckbox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel indexLayoutPanel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox previewGroupBox;
        private System.Windows.Forms.DataGridView csvDataGridView;
        private System.Windows.Forms.HScrollBar fileSelectionBar;
    }
}