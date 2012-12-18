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
    public partial class GUI : Form
    {
        Delineator mDelineator;
        Extractor mExtractor;

        public GUI()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                shapeFileBox.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                dataDirBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(shapeFileBox.Text))
            {
                MessageBox.Show("Shapefile doesn't exist");
                return;
            }
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                mDelineator = new Delineator(shapeFileBox.Text, getGridInfo(), folderBrowserDialog1.SelectedPath);
                mDelineator.Delineate(true, true);
                weightFileBox.Text = mDelineator.getWeightFile();
                pictureBox1.Image = mDelineator.polygonBitmap;
            }
        }

        private GriddedDataDetails getGridInfo()
        {
            return new GriddedDataDetails((float)numericUpDown2.Value, (float)numericUpDown1.Value, (float)numericUpDown3.Value, (float)numericUpDown4.Value, (int)numericUpDown5.Value, (int)numericUpDown6.Value);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!File.Exists(weightFileBox.Text))
            {
                    MessageBox.Show("Weight file must exist.");
                    return;
            }
            else
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    mExtractor = new Extractor(dataDirBox.Text, weightFileBox.Text, getGridInfo(), (TimeUnit)listBox1.SelectedIndex);
                    mExtractor.Extract(saveFileDialog1.FileName);
                    mExtractor.ProduceStatPlots(new Size(2048, 2048), folderBrowserDialog1.SelectedPath, true, true, true);
                    pictureBox1.Image = mExtractor.ProducePlot(new Size(2048, 2048));
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                weightFileBox.Text = openFileDialog1.FileName;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void GUI_Load(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = 2;
        }
    }
}
