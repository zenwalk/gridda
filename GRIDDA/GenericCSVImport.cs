using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GRIDDA
{
    partial class GenericCSVImport : Form
    {
        public GenericCSV genericCSV;

        public GenericCSVImport()
        {
            InitializeComponent();
            genericCSV.dataSet = new DataSet();
        }

        public GenericCSVImport(GenericCSV genericCSV)
        {
            this.genericCSV = genericCSV;
            genericCSV.dataSet = new DataSet();
            InitializeComponent();
        }

        private void UpdatePreview()
        {
            // Update data preview
            if (genericCSV.dataSet.Tables.Count > 0)
            {
                csvDataGridView.DataSource = genericCSV.dataSet.Tables[fileSelectionBar.Value].DefaultView;
                csvDataGridView.AutoSize = true;
                csvDataGridView.AutoResizeColumns();
            }
        }

        private void LoadFiles()
        {
            // Clear old data
            genericCSV.dataSet = new DataSet();

            // Create table for each file
            switch (genericCSV.fileSelection)
            {
                case (FileSelection.Single):
                    genericCSV.dataSet.Tables.Add(openFileDialog1.FileName);
                    break;
                case (FileSelection.Multiple):
                    foreach (String filename in openFileDialog1.FileNames)
                    {
                        genericCSV.dataSet.Tables.Add(filename);
                    }
                    break;
                case (FileSelection.Folder):
                    foreach (String filename in Directory.GetFiles(folderBrowserDialog1.SelectedPath))
                    {
                        genericCSV.dataSet.Tables.Add(filename);
                    }
                    break;
            }

            // Load data for each file
            foreach (DataTable table in genericCSV.dataSet.Tables)
            {
                // Get filename
                String filename = table.TableName;

                // Open file
                StreamReader inFile = new StreamReader(filename);

                // Read file
                while (!inFile.EndOfStream)
                {
                    // Read line from file
                    String inLine = inFile.ReadLine();

                    // Break into columns
                    String[] inColumns = inLine.Split(new char[2] { ',', '\t' });

                    // Add extra columns to table if needed
                    while (genericCSV.dataSet.Tables[filename].Columns.Count < inColumns.Length)
                    {
                        genericCSV.dataSet.Tables[filename].Columns.Add("unnamed" + (genericCSV.dataSet.Tables[filename].Columns.Count + 1));
                    }
                    
                    // Add row
                    genericCSV.dataSet.Tables[filename].Rows.Add(inColumns);
                }
            }

            // Set file selector length
            fileSelectionBar.Maximum = genericCSV.dataSet.Tables.Count-1;
        }

        private void GenericCSVImport_Load(object sender, EventArgs e)
        {
            // Setup for file selection
            switch (genericCSV.fileSelection)
            {
                case (FileSelection.Single):
                    selectFileButton.Text = "Select file";
                    break;
                case (FileSelection.Multiple):
                    selectFileButton.Text = "Select files";
                    break;
                case (FileSelection.Folder):
                    selectFileButton.Text = "Select folder";
                    break;
            }

            // Initilise column index
            genericCSV.index = new Dictionary<string, int>();
            foreach (String s in genericCSV.columns)
            {
                genericCSV.index.Add(s, -1);
            }

            // Initialise column gui
            indexLayoutPanel.RowCount = genericCSV.columns.Count;

            for (int i = 0; i < genericCSV.columns.Count; ++i)
            {
                Label label = new Label();
                label.Text = genericCSV.columns[i] + " column: ";
                label.TextAlign = ContentAlignment.TopRight;
                label.Dock = DockStyle.Fill;

                NumericUpDown upDown = new NumericUpDown();
                upDown.Name = "index" + i;
                upDown.Minimum = -1;
                upDown.Maximum = 1000;
                upDown.Value = -1;
                upDown.TextAlign = HorizontalAlignment.Left;
                upDown.ValueChanged += updateHeaders;

                indexLayoutPanel.Controls.Add(label, 0, i);
                indexLayoutPanel.Controls.Add(upDown, 1, i);
            }
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            // Get filename
            switch (genericCSV.fileSelection)
            {
                case (FileSelection.Single):
                    genericCSV.filePath = openFileDialog1.FileName;
                    break;
                case (FileSelection.Multiple):
                    genericCSV.filePaths = openFileDialog1.FileNames;
                    break;
                case (FileSelection.Folder):
                    genericCSV.folderPath = folderBrowserDialog1.SelectedPath;
                    break;
            }

            // Get header info
            genericCSV.hasHeader = hasHeaderCheckbox.Checked;

            // Get column indices
            for (int i = 0; i < genericCSV.columns.Count; ++i)
            {
                int index = (int)((NumericUpDown)indexLayoutPanel.Controls.Find("index" + i, true)[0]).Value;
                genericCSV.index[genericCSV.columns[i]] = index;
            }

            if (genericCSV.index.Values.Min() < 0)
            {
                if (MessageBox.Show("Not all column values are valid (must be non-negative). Leave anyway?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
            else if (genericCSV.filePath == "" && genericCSV.filePaths == null && genericCSV.folderPath == "")
            {
                if (MessageBox.Show("File not selected. Leave anyway?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void selectFileButton_Click(object sender, EventArgs e)
        {
            switch (genericCSV.fileSelection)
            {
                case (FileSelection.Single):
                    openFileDialog1.Multiselect = false;
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        // Load files
                        LoadFiles();
                    }
                    break;
                case (FileSelection.Multiple):
                    openFileDialog1.Multiselect = true;
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        // Load files
                        LoadFiles();
                    }
                    break;
                case (FileSelection.Folder):
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        // Load files
                        LoadFiles();
                    }
                    break;
            }

            // Update preview
            UpdatePreview();
        }

        private void fileSelectionBar_Scroll(object sender, ScrollEventArgs e)
        {
            previewGroupBox.Text = "Preview: " + genericCSV.dataSet.Tables[fileSelectionBar.Value].TableName;
            csvDataGridView.DataSource = genericCSV.dataSet.Tables[fileSelectionBar.Value].DefaultView;
        }

        private void updateHeaders(object sender, EventArgs e)
        {
            // Update dictionary from ui
            for (int i = 0; i < genericCSV.columns.Count; ++i)
            {
                int index = (int)((NumericUpDown)indexLayoutPanel.Controls.Find("index" + i, true)[0]).Value;
                genericCSV.index[genericCSV.columns[i]] = index;
            }

            if (genericCSV.dataSet != null)
            {
                // Go through each column
                foreach (String columnName in genericCSV.index.Keys)
                {
                    // Get index
                    int index = genericCSV.index[columnName];

                    // Go through each file
                    foreach (DataTable table in genericCSV.dataSet.Tables)
                    {
                        // Remove name off previous column if set
                        if (table.Columns.Contains(columnName))
                        {
                            table.Columns[columnName].ColumnName = "unnamed" + (table.Columns[columnName].Ordinal + 1);
                        }

                        // If value is valid, set column index to this name
                        if (index > -1)
                        {
                            if (table.Columns.Count > index)
                            {
                                table.Columns[index].ColumnName = columnName;
                            }
                        }
                    }
                }
            }

            // Update preview on screen
            UpdatePreview();
        }

        private void hasHeaderCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            // Search tables for matching vlues
            if (hasHeaderCheckbox.Checked)
            {
                if (genericCSV.dataSet != null)
                {
                    foreach (DataTable table in genericCSV.dataSet.Tables)
                    {
                        foreach (String columnName in genericCSV.columns)
                        {
                            if (table.Rows.Count > 0)
                            {
                                for (int i = 0; i < table.Columns.Count; ++i)
                                {
                                    if (!table.Rows[0].IsNull(i))
                                    {
                                        object entry = table.Rows[0][i];
                                        String entryValue = (String)entry;
                                        if (entryValue.Trim().ToUpper().Equals(columnName.ToUpper()))
                                        {
                                            ((NumericUpDown)indexLayoutPanel.Controls.Find("index" + genericCSV.columns.IndexOf(columnName), true)[0]).Value = i;
                                            genericCSV.index[columnName] = i;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
