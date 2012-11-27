using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace GRIDDA
{
    class GriddedDataFile
    {
        int nColumns; // number of columns
        int nRows; // number of rows

        decimal xLowerLeft; // longitude of lower-left corner
        decimal yLowerLeft; // latitude of lower-left corner

        decimal xCellSize; // Cell or grid size (degrees)
        decimal yCellSize; // Cell or grid size (degrees)

        float nodataValue; // null value

        bool lsbfirst; // byte order

        float[,] fltData; // [rows,columns]

        float maxValue;

        Bitmap bitmap;
        
        String filename;
        String fltFile;

        bool textFormat = false;

        public GriddedDataFile(GriddedDataDetails grid, String fltFile, bool text=false)
        {
            // Save filename
            filename = Path.GetFileNameWithoutExtension(fltFile);
            this.nColumns = grid.columns;
            this.nRows = grid.rows;
            this.xLowerLeft = grid.lowerLeftLongitude;
            this.yLowerLeft = grid.lowerLeftLatitude;
            this.xCellSize = grid.horizontalGridSize;
            this.yCellSize = grid.verticalGridSize;
            nodataValue = -9999.9f;
            lsbfirst = true;

            textFormat = text;
            this.fltFile = fltFile;
        }

        public GriddedDataFile(String txtFolder)
        {
            // Find file
            try
            {
                String txtFile = Directory.GetFiles(txtFolder)[0];

                if (File.Exists(txtFile))
                {

                    // Save filename
                    filename = Path.GetFileNameWithoutExtension(txtFile);
                    this.fltFile = txtFile;
                    textFormat = true;

                    // Read header
                    readHeader(txtFile);
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (FileNotFoundException e)
            {
                throw new Exception("Empty folder."+e.Message);
            }
        }

        private void readHeader(String hdrFile)
        {
            // Open file
            StreamReader inFile = new StreamReader(hdrFile);

            // Read values
            nColumns = Int32.Parse(inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            nRows = Int32.Parse(inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            xLowerLeft = Decimal.Parse(inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            yLowerLeft = Decimal.Parse(inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            xCellSize = Decimal.Parse(inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            yCellSize = xCellSize;
            nodataValue = Single.Parse(inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);

            if (!textFormat)
                lsbfirst = "lsbfirst" == inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1];

            // Close file
            inFile.Close();
        }

        private void readData(String fltFile)
        {
            if (!textFormat)
            {
                // Open files
                BinaryReader inFile = new BinaryReader(File.Open(fltFile, FileMode.Open, FileAccess.Read));

                // Allocate memory
                fltData = new float[nRows, nColumns];

                maxValue = 1.0f;

                // Read data from file
                for (int j = 0; j < nRows; ++j)
                {
                    for (int i = 0; i < nColumns; ++i)
                    {
                        fltData[j, i] = inFile.ReadSingle();
                        maxValue = fltData[j, i] > maxValue ? fltData[j, i] : maxValue;
                    }
                }

                // Close file
                inFile.Close();
            }
            else
            {
                // Open file
                StreamReader inFile = new StreamReader(fltFile);

                // Scroll past header
                for (int i = 0; i < 6; ++i)
                {
                    inFile.ReadLine();
                }

                // Read one row at a time
                for (int j = 0; j < nRows; ++j)
                {
                    // Separate row into columns
                    String[] inLine = inFile.ReadLine().Split();

                    // Save to array
                    for (int i = 0; i < nColumns; ++i)
                    {
                        fltData[j, i] = Single.Parse(inLine[i]);
                        maxValue = fltData[j, i] > maxValue ? fltData[j, i] : maxValue;
                    }
                }
                
                // Close file
                inFile.Close();
            }
        }

        private int longitudeToColumn(decimal longitude)
        {
            // shift based on longitude of first column, divide by size of a cell and round to integer to find index
            return (int)Math.Floor((longitude - xLowerLeft) / xCellSize);
        }

        private int latitudeToRow(decimal latitude)
        {
            // done in reverse, shift based on latitude of LAST column, divide by size of a cell and round to integer to find reversed index, subtract from last index to find actual index
            decimal a = (latitude - yLowerLeft) / yCellSize;
            return (nRows - 1) - (int)Math.Floor(a);
        }

        public float[] getDataRange(decimal longitude1, decimal latitude, decimal longitude2)
        {
            return getDataRange(longitudeToColumn(longitude1), latitudeToRow(latitude), longitudeToColumn(longitude2));
        }

        public float[] getDataRange(int x1, int y, int x2)
        {
            // Query size
            int width = Math.Abs(x1 - x2) + 1; // inclusuve

            // First value of query
            int x = Math.Min(x1, x2);

            // Allocate memory for query
            float[] data = new float[width];

            if (!textFormat)
            {
                // Open
                BinaryReader inFile = new BinaryReader(File.Open(fltFile, FileMode.Open, FileAccess.Read));

                // Seek to new read position
                inFile.BaseStream.Seek(sizeof(Single) * (y * nColumns + x), SeekOrigin.Begin);
                for (int i = 0; i < width; ++i)
                {
                    // Read data
                    data[i] = inFile.ReadSingle();
                }

                inFile.Close();
            }
            else
            {                
                // Open file
                StreamReader inFile = new StreamReader(fltFile);

                // Scroll past header
                for (int i = 0; i < 6; ++i)
                {
                    inFile.ReadLine();
                }

                // Skip rows
                for (int i = 0; i < y; ++i)
                {
                    inFile.ReadLine();
                }

                // Separate row into columns
                String[] inLine = inFile.ReadLine().Split();

                // Save to array
                for (int i = x; i < x+width; ++i)
                {
                    data[i] = Single.Parse(inLine[i]);
                }

                // Close file
                inFile.Close();
            }

            return data;
        }

        public float[,] getDataRange(decimal longitude1, decimal latitude1, decimal longitude2, decimal latitude2)
        {
            return getDataRange(longitudeToColumn(longitude1), latitudeToRow(latitude1), longitudeToColumn(longitude2), latitudeToRow(latitude2));
        }

        public float[,] getDataRange(int x1, int y1, int x2, int y2)
        {
            // Query size
            int width = Math.Abs(x1 - x2)+1; // inclusuve
            int height = Math.Abs(y1 - y2)+1; // inclusive

            // First value of query
            int x = Math.Min(x1, x2);
            int y = Math.Min(y1, y2);

            // Allocate memory for query
            float[,] data = new float[height, width];

            if (!textFormat)
            {
                // Open
                BinaryReader inFile = new BinaryReader(File.Open(fltFile, FileMode.Open, FileAccess.Read));

                for (int j = 0; j < height; ++j)
                {
                    // Seek to new read position
                    inFile.BaseStream.Seek(sizeof(Single) * ((y + j) * nColumns + x), SeekOrigin.Begin);
                    for (int i = 0; i < width; ++i)
                    {
                        // Read data
                        data[j, i] = inFile.ReadSingle();
                    }
                }

                // Close
                inFile.Close();
            }
            else
            {
                // Open file
                StreamReader inFile = new StreamReader(fltFile);

                // Scroll past header
                for (int i = 0; i < 6; ++i)
                {
                    inFile.ReadLine();
                }

                // Skip rows
                for (int i = 0; i < y; ++i)
                {
                    inFile.ReadLine();
                }

                // Read data, one row at a time
                for (int j = y; j < height; ++j)
                {
                    // Separate row into columns
                    String[] inLine = inFile.ReadLine().Split();

                    // Save to array
                    for (int i = x; i < x + width; ++i)
                    {
                        data[j, i] = Single.Parse(inLine[i]);
                    }
                }

                // Close file
                inFile.Close();
            }

            return data;
        }

        public float getData(decimal longitude, decimal latitude)
        {
            return getData(longitudeToColumn(longitude), latitudeToRow(latitude));
        }

        public float getData(int column, int row)
        {
            float data;

            if (!textFormat)
            {
                // Open
                BinaryReader inFile = new BinaryReader(File.Open(fltFile, FileMode.Open, FileAccess.Read));

                // Seek
                inFile.BaseStream.Seek(sizeof(Single) * (row * nColumns + column), SeekOrigin.Begin);

                // Read
                data = inFile.ReadSingle();

                // Close
                inFile.Close();
            }
            else
            {
                // Open file
                StreamReader inFile = new StreamReader(fltFile);

                // Scroll past header
                for (int i = 0; i < 6; ++i)
                {
                    inFile.ReadLine();
                }

                // Skip rows
                for (int i = 0; i < row; ++i)
                {
                    inFile.ReadLine();
                }

                // Separate row into columns
                String[] inLine = inFile.ReadLine().Split();

                // Get point
                data = Single.Parse(inLine[column]);

                // Close file
                inFile.Close();
            }

            // Return
            return data;
        }

        private void visualizeGrid()
        {
            if (!textFormat)
            {
                // Open file
                BinaryReader inFile = new BinaryReader(File.Open(fltFile, FileMode.Open));

                // Create bitmap
                bitmap = new Bitmap(nColumns, nRows);

                // Attached graphics device
                Graphics gfx = Graphics.FromImage(bitmap);

                // Clear bitmap
                gfx.Clear(Color.White);

                // Initialize brush
                Brush brush = Brushes.White;

                float data;
                maxValue = 100;
                for (int j = 0; j < nRows; ++j)
                {
                    for (int i = 0; i < nColumns; i += 3)
                    {
                        data = inFile.ReadSingle();
                        inFile.BaseStream.Seek(sizeof(Single) * 2, SeekOrigin.Current);

                        // Draw grid cell for random column
                        if (data == nodataValue) // If data is missing, set brush to blue
                        {
                            brush = Brushes.Blue;
                        }
                        else if (data > maxValue) // If data over max (not possible), set to full green
                        {
                            brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                        }
                        else // If data is valid, set brush colour to blend between red and green based on intensity
                        {
                            float scale = 255.0f * data / maxValue;
                            brush = new SolidBrush(Color.FromArgb((int)(255 - scale), (int)(scale), 0));
                        }
                        // Fill grid square based on brush
                        gfx.FillRectangle(brush, i, j, i + 1, j + 1);
                    }
                }

                // Close
                inFile.Close();
            }
        }

        public Bitmap getGridBitmap()
        {
            if (bitmap == null)
            {
                //readData(filename + ".flt");
                visualizeGrid();
                fltData = null;
            }
            return bitmap;
        }

        public String getFilename()
        {
            return filename;
        }
    }
}
