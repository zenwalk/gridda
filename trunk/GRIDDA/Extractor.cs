using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace GRIDDA
{
    class Extractor
    {
        GriddedDataDetails mGridDetails;
        TimeUnit mTimeUnit;

        ExtractionPoint[] mExtractionPoints;
        GriddedDataset mGriddedDataset;

        MultipleTimeseries mMultipleTimeseries;

        public Extractor(String dataDir, String cellWeightsFile, GriddedDataDetails gridDetails, TimeUnit timeUnit)
        {
            this.mGridDetails = gridDetails;
            this.mTimeUnit = timeUnit;

            InitializeGriddedData(dataDir);
            InitializeExtractionPoints(cellWeightsFile);
        }

        public void InitializeGriddedData(String dataDir)
        {
            String[] files = Directory.GetFiles(dataDir, "*.flt", SearchOption.AllDirectories);

            // Construct regular expression pattern
            String pattern = @"\S*";
            //if (prefixBox.Text != "") // starts with
            //{
            //    pattern = @"^" + prefixBox.Text.Replace("*", @"\S*").Replace("#", @"\d").Replace("?", @"\S") + pattern;
            //}
            //if (keywordBox.Text != "") // contains
            //{
            //    pattern += keywordBox.Text.Replace("*", @"\S*").Replace("#", @"\d").Replace("?", @"\S") + @"\S*";
            //}
            //if (suffixBox.Text != "") // ends with
            //{
            //    pattern += suffixBox.Text.Replace("*", @"\S*").Replace("#", @"\d").Replace("?", @"\S") + @"\b";
            //}

            // Construct regular expression date pattern
            String dataPattern = @"(\D\d{8}\D)|(^\d{8}\D)";

            // Select files based on patterns
            String[] selectedFiles = Array.FindAll<String>(files, element => Regex.IsMatch(Path.GetFileName(element), pattern) && Regex.IsMatch(Path.GetFileName(element), dataPattern));

            mGriddedDataset = new GriddedDataset(selectedFiles, mGridDetails, 8, mTimeUnit);
        }

        public void InitializeExtractionPoints(String cellWeightsFile)
        {
            StreamReader inFile = new StreamReader(cellWeightsFile);
            List<ExtractionPoint> pointList = new List<ExtractionPoint>();
            
            if (!inFile.EndOfStream) // skip header line
            {
                inFile.ReadLine();
            }

            while (!inFile.EndOfStream)
            {
                string[] columns = inFile.ReadLine().Split(',');

                if (columns.Length >= 4)
                {
                    pointList.Add(new ExtractionPoint(columns[0], Single.Parse(columns[1]), Single.Parse(columns[2]), Single.Parse(columns[3])));
                }
            }

            mExtractionPoints = pointList.ToArray();
            inFile.Close();
        }

        public void Extract(String outDir)
        {
            // Get data for longitude and latitude
            List<float[]> queryData = mGriddedDataset.getTimeseries(mExtractionPoints, mTimeUnit);

            // Check dataset returned for each station
            if (queryData.Count == mExtractionPoints.Length)
            {
                // Create data set
                mMultipleTimeseries = new MultipleTimeseries(mGriddedDataset.getFromDate(), mGriddedDataset.getToDate(), mTimeUnit);
                
                // Scale values
                for (int i = 0; i < queryData.Count; ++i)
                {
                    for (int j = 0; j < queryData[i].Length; ++j)
                    {
                        queryData[i][j] *= (float)mExtractionPoints[i].weight;
                    }
                }

                // Find unique stations
                string[] uniqueStations = mExtractionPoints.Select(v => v.siteName).Distinct().ToArray();
                string[] siteNames = mExtractionPoints.Select(v => v.siteName).ToArray();

                int nextIndex;
                foreach (String s in uniqueStations)
                {
                    // Find first occurance
                    int index = Array.IndexOf(siteNames, s);

                    // Add all other indices and compute normalise factor
                    nextIndex = index;
                    float[] normalise = new float[queryData[index].Length]; // unique normalise value for each data point in case of missing values

                    for (int i = 0; i < queryData[index].Length; ++i)
                    {
                        if (queryData[index][i] < 0) // if first station value is invalid, set it to zero instead of missing and set normalise to zero
                        {
                            queryData[index][i] = 0;
                            normalise[i] = 0;
                        }
                        else // otherwise begin the normalise constant
                        {
                            normalise[i] = (float)mExtractionPoints[index].weight;
                        }
                    }

                    // Find next occurance
                    nextIndex = Array.IndexOf(siteNames, s, nextIndex + 1);

                    while (nextIndex != -1)
                    {
                        // Combine data sources for each point and compute normalisation
                        for (int i = 0; i < queryData[index].Length; ++i)
                        {
                            // if valid data point, add it to the normalise value
                            if (queryData[nextIndex][i] >= 0)
                            {
                                queryData[index][i] += queryData[nextIndex][i];
                                normalise[i] += (float)mExtractionPoints[nextIndex].weight;
                            }
                        }

                        // Find next occurance
                        nextIndex = Array.IndexOf(siteNames, s, nextIndex + 1);
                    }

                    // Normalise data
                    for (int i = 0; i < queryData[index].Length; ++i)
                    {
                        if (normalise[i] > 0)
                        {
                            queryData[index][i] /= normalise[i];
                        }
                        else
                        {
                            queryData[index][i] = -9999.9f;
                        }
                    }

                    // Add to data set
                    mMultipleTimeseries.AddColumn(new SingleTimeseries(s, queryData[index]));
                }

                // Save data
                mMultipleTimeseries.writeToFile(outDir + Path.DirectorySeparatorChar + "Data.csv");
            }
        }

        public Bitmap ProducePlot(Size canvasSize)
        {
            GraphItem graphItem = new GraphItem();
            graphItem.mMarkerFillColor = Color.Red;
            graphItem.mMarkerOutlineColor = Color.Black;
            graphItem.mMarkerSize = 4.0f;
            graphItem.mMarkerOutlineSize = 1.0f;
            graphItem.mMarkerShape = GraphMarker.Circle;
            graphItem.mTrendLine = true;
            graphItem.mTrendLineColor = Color.Blue;
            graphItem.mTrendLineSize = 1.0f;

            Graph graph = new Graph(mMultipleTimeseries, canvasSize, "Timeseries", "Time", "Amount");
            graph.setHorizontalAxis(mMultipleTimeseries.getStartDate(), mMultipleTimeseries.getEndDate());
            graph.setVerticalAxis(0, mMultipleTimeseries.getMax());
            graph.setSize(new Size(2048, 2048));

            for (int i = 0; i < mMultipleTimeseries.getColumns(); ++i)
            {
                graph.addData(new GraphItem(graphItem, i, ""));
            }

            return graph.getGraph();
        }

        public void ProduceStatPlots(Size canvasSize, String outDir, bool savePlots, bool saveStats, bool saveStatPlots)
        {
            GraphItem graphItem = new GraphItem();
            graphItem.mMarkerFillColor = Color.Red;
            graphItem.mMarkerOutlineColor = Color.Black;
            graphItem.mMarkerSize = 4.0f;
            graphItem.mMarkerOutlineSize = 1.0f;
            graphItem.mMarkerShape = GraphMarker.Circle;
            graphItem.mTrendLine = true;
            graphItem.mTrendLineColor = Color.Blue;
            graphItem.mTrendLineSize = 1.0f;

            // Get monthly statistics
            MultipleTimeseries[] stats = mMultipleTimeseries.getStatistics();

            // Save statistics and statistic plots
            for (int i = 0; i < stats.Length; ++i)
            {
                // Plot stats
                if (saveStats)
                {
                    stats[i].writeToFile(outDir + Path.DirectorySeparatorChar + "Stats_" + (i + 1).ToString("0000") + ".csv");
                }

                if (savePlots)
                {
                    // Plot timeseries
                    Graph graph = new Graph(mMultipleTimeseries, canvasSize, mMultipleTimeseries[i].SiteId + " Timeseries", "Time", "Amount");
                    graph.addData(new GraphItem(graphItem, i, ""));
                    graph.setSize(new Size(2048, 2048));
                    graph.setHorizontalAxis(mMultipleTimeseries.getStartDate(), mMultipleTimeseries.getEndDate());
                    graph.setVerticalAxis(0, mMultipleTimeseries[i].getMax());
                    graph.getGraph().Save(outDir + Path.DirectorySeparatorChar + "Timeseries_" + (i + 1).ToString("0000") + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }

                if (saveStatPlots)
                {
                    Graph graph = new Graph(stats[i], canvasSize, mMultipleTimeseries[i].SiteId + " Statistics", "Month", "Amount", true);
                    graph.addData(new GraphItem(graphItem, 1, "Mean"));
                    graph.addData(new GraphItem(graphItem, 2, "Min"));
                    graph.addData(new GraphItem(graphItem, 3, "Max"));
                    graph.setSize(new Size(2048, 2048));
                    graph.setHorizontalAxis(stats[i].getStartDate(), stats[i].getEndDate().AddMonths(-1));
                    graph.setVerticalAxis(0, mMultipleTimeseries[i].getMax());
                    graph.getGraph().Save(outDir + Path.DirectorySeparatorChar + "Stats_" + (i + 1).ToString("0000") + "_MeanMinMax.png", System.Drawing.Imaging.ImageFormat.Png);

                    graph = new Graph(stats[i], canvasSize, mMultipleTimeseries[i].SiteId + " Statistics", "Month", "Amount", true);
                    graph.addData(new GraphItem(graphItem, 4, "10%"));
                    graph.addData(new GraphItem(graphItem, 5, "25%"));
                    graph.addData(new GraphItem(graphItem, 6, "50%"));
                    graph.addData(new GraphItem(graphItem, 7, "75%"));
                    graph.addData(new GraphItem(graphItem, 8, "90%"));
                    graph.setSize(new Size(2048, 2048));
                    graph.setHorizontalAxis(stats[i].getStartDate(), stats[i].getEndDate().AddMonths(-1));
                    graph.setVerticalAxis(0, mMultipleTimeseries[i].getMax());
                    graph.getGraph().Save(outDir + Path.DirectorySeparatorChar + "Stats_" + (i + 1).ToString("0000") + "_10-25-50-75-90.png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }
    }
}
