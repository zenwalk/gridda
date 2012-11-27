using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GRIDDA
{
    partial class Graph
    {
        MultipleTimeseries mDataset;
        private List<GraphItem> mGraphItems;
        private bool mPlotAccumulative;

        private DateTime mMinX;
        private DateTime mMaxX;
        private float mMinY;
        private float mMaxY;

        private String mTitle;
        private String mTitleX;
        private String mTitleY;

        private Size mCanvasSize;
        private Size mGraphSize;
        private Point mGraphOffset;
                
        private Bitmap mCanvas;
        private Bitmap mGraph;

        private float mXScale;
        private float mYScale;

        private bool mMonthlyGraph;
        
        public Graph(MultipleTimeseries dataSet, Size canvasSize, String title, String titleX, String titleY, bool monthlyGraph = false, bool plotAccumulative = false)
        {
            this.mDataset = dataSet;
            this.mPlotAccumulative = plotAccumulative;
            this.mMonthlyGraph = monthlyGraph;
            this.mMinX = DateTime.Now;
            this.mMaxX = DateTime.Now;
            this.mMinY = 0;
            this.mMaxY = 0;
            this.mGraphItems = new List<GraphItem>();

            this.mCanvasSize = canvasSize;
            this.mTitle = title;
            this.mTitleX = titleX;
            this.mTitleY = titleY;

            this.mGraphOffset = new Point((int)Math.Max(50, Math.Min(200, mCanvasSize.Width * 0.1f)), (int)Math.Max(50, Math.Min(200, mCanvasSize.Height * 0.1f)));

            Initialise();
        }

        private void Initialise()
        {
        }

        public void setHorizontalAxis(DateTime lowerBound, DateTime upperBound)
        {
            mMinX = lowerBound;
            mMaxX = upperBound;

            // Calculate factors between graph space and data space
            mXScale = mGraphSize.Width / (float)(mMaxX - mMinX).TotalHours;
        }

        public void setVerticalAxis(float lowerBound, float upperBound)
        {
            mMinY = lowerBound;
            mMaxY = upperBound;

            // Calculate factors between graph space and data space
            mYScale = mGraphSize.Height / (float)(mMaxY - mMinY);
        }

        public void setSize(Size size)
        {
            mCanvasSize = size;
        }

        public GraphItem[] getGraphItems()
        {
            return mGraphItems.ToArray();
        }

        public void addData(GraphItem graphItem)
        {
            mGraphItems.Add(graphItem);
        }

        public void addData(GraphItem[] graphItems)
        {
            foreach (GraphItem graphItem in graphItems)
            {
                mGraphItems.Add(graphItem);
            }
        }

        public void editData(GraphItem oldGraphItem, GraphItem graphItem)
        {
            int index = mGraphItems.IndexOf(oldGraphItem);

            if (index == -1)
            {
                foreach (GraphItem searchItem in mGraphItems)
                {
                    if (searchItem.mStationIndex == oldGraphItem.mStationIndex)
                    {
                        index = mGraphItems.IndexOf(searchItem);
                    }
                }
            }

            if (index != -1)
            {
                mGraphItems.RemoveAt(index);

                mGraphItems.Insert(index, graphItem);
            }
        }

        public void editDataColor(GraphItem graphItem, Color graphColor)
        {
            int index = mGraphItems.IndexOf(graphItem);

            if (index == -1)
            {
                foreach (GraphItem searchItem in mGraphItems)
                {
                    if (searchItem.mStationIndex == graphItem.mStationIndex)
                    {
                        index = mGraphItems.IndexOf(searchItem);
                    }
                }
            }

            if (index != -1)
            {
                mGraphItems.RemoveAt(index);

                graphItem.mMarkerFillColor = graphColor;
                mGraphItems.Insert(index, graphItem);
            }
        }

        public void editLineColor(GraphItem graphItem, Color graphColor)
        {
            int index = mGraphItems.IndexOf(graphItem);

            if (index == -1)
            {
                foreach (GraphItem searchItem in mGraphItems)
                {
                    if (searchItem.mStationIndex == graphItem.mStationIndex)
                    {
                        index = mGraphItems.IndexOf(searchItem);
                    }
                }
            }

            if (index != -1)
            {
                mGraphItems.RemoveAt(index);

                graphItem.mTrendLineColor = graphColor;
                mGraphItems.Insert(index, graphItem);
            }
        }

        public void editLine(GraphItem graphItem, bool drawLine)
        {
            int index = mGraphItems.IndexOf(graphItem);

            if (index == -1)
            {
                foreach (GraphItem searchItem in mGraphItems)
                {
                    if (searchItem.mStationIndex == graphItem.mStationIndex)
                    {
                        index = mGraphItems.IndexOf(searchItem);
                    }
                }
            }

            if (index != -1)
            {
                mGraphItems.RemoveAt(index);

                graphItem.mTrendLine = drawLine;
                mGraphItems.Insert(index, graphItem);
            }
        }

        public void removeData(GraphItem graphItem)
        {
            if (mGraphItems.Contains(graphItem))
            {
                mGraphItems.Remove(graphItem);
            }
            else
            {
                List<GraphItem> toRemove = new List<GraphItem>();
                foreach (GraphItem searchItem in mGraphItems)
                {
                    if (searchItem.mStationIndex == graphItem.mStationIndex)
                    {
                        toRemove.Add(searchItem);
                    }
                }
                foreach (GraphItem removeItem in toRemove)
                {
                    mGraphItems.Remove(removeItem);
                }
            }
        }

        public void clearData()
        {
            this.mGraphItems.Clear();
        }

        public Bitmap getGraph()
        {
            DrawAll();

            return mCanvas;
        }

        public void DrawAll()
        {
            RefreshAll();

            if (mTitle != "")
            {
                DrawTitle();
            }
            DrawHorizontalAxis();
            DrawVerticalAxis();
            DrawGraph();
        }

        public void DrawGraph()
        {
            // Clear graph
            RefreshGraph();

            // Allocate graphics memory for bitmap
            Graphics graphObj = Graphics.FromImage(mGraph);

            // Draw each timeseries onto graph
            foreach (GraphItem graphItem in mGraphItems)
            {
                DrawTimeseries(graphItem, graphObj);
            }

            // Draw border
            Pen myPen = new Pen(Color.Black);
            graphObj.DrawRectangle(myPen, 0, 0, mGraphSize.Width - 1, mGraphSize.Height - 1);

            // Write to graphics memory to bitmap
            graphObj.DrawImage(mGraph, new Point(0, 0));

            // Free graphics memory
            graphObj.Dispose();

            // Allocate graphics memory for bitmap
            Graphics canvasObj = Graphics.FromImage(mCanvas);

            // Write to graphics memory to bitmap
            canvasObj.DrawImage(mGraph, mGraphOffset);

            // Free graphics memory for bitmap
            canvasObj.Dispose();
        }

        private void RefreshGraph()
        {
            // Define bitmap
            mGraph = new Bitmap(mGraphSize.Width, mGraphSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            // Allocate graphics memory for bitmap
            Graphics graphObj = Graphics.FromImage(mGraph);

            // Clear graphics memory
            graphObj.Clear(Color.White);

            // Write to graphics memory to bitmap
            graphObj.DrawImage(mGraph, new Point(0, 0));

            // Free graphics memory
            graphObj.Dispose();
        }

        private void RefreshAll()
        {
            // Resize graph to fit
            mGraphSize.Width = mCanvasSize.Width - (2 * mGraphOffset.X);
            mGraphSize.Height = mCanvasSize.Height - (2 * mGraphOffset.Y);

            // Calculate factors between graph space and data space
            if (mMaxX > mMinX) mXScale = mGraphSize.Width / (float)(mMaxX - mMinX).TotalHours;
            else mXScale = Single.PositiveInfinity;
            if (mMaxY > mMinY) mYScale = mGraphSize.Height / (float)(mMaxY - mMinY);
            else mYScale = Single.PositiveInfinity;

            // Define canvas
            mCanvas = new Bitmap(mCanvasSize.Width, mCanvasSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            // Allocate graphics memory for bitmap
            Graphics canvasObj = Graphics.FromImage(mCanvas);

            // Clear graphics memory
            canvasObj.Clear(Color.White);

            // Write to graphics memory to bitmap
            canvasObj.DrawImage(mCanvas, new Point(0, 0));

            // Free graphics memory for bitmap
            canvasObj.Dispose();

            RefreshGraph();
        }

        private void DrawTitle()
        {

        }

        private void DrawHorizontalAxis()
        {
            // Allocate graphics memory for bitmap
            Graphics canvasObj = Graphics.FromImage(mCanvas);

            // Prepare font and alignment for title
            Brush myBrush = new SolidBrush(Color.Black);
            Font myFont = new Font("Arial", mGraphOffset.X * 0.30f, FontStyle.Bold);
            StringFormat xAxisFormat = new StringFormat();
            xAxisFormat.Alignment = StringAlignment.Center;
            xAxisFormat.LineAlignment = StringAlignment.Far;

            // Draw X axis title
            canvasObj.DrawString(mTitleX, myFont, myBrush, mGraphOffset.X + (mGraphSize.Width * 0.5f), mCanvasSize.Height, xAxisFormat);

            // Prepare font and alignment for axis markers
            xAxisFormat = new StringFormat();
            xAxisFormat.LineAlignment = StringAlignment.Far;
            myFont = new Font("Arial", mGraphOffset.X * 0.20f, FontStyle.Italic);

            if (mMonthlyGraph)
            {
                // Draw Axis Markers
                xAxisFormat.Alignment = StringAlignment.Near;
                canvasObj.DrawString("January", myFont, myBrush, mGraphOffset.X, mCanvasSize.Height - mGraphOffset.Y * 0.5f, xAxisFormat);
                xAxisFormat.Alignment = StringAlignment.Far;
                canvasObj.DrawString("December", myFont, myBrush, mGraphOffset.X + mGraphSize.Width, mCanvasSize.Height - mGraphOffset.Y * 0.5f, xAxisFormat);
            }
            else
            {
                // Draw Axis Markers
                xAxisFormat.Alignment = StringAlignment.Near;
                canvasObj.DrawString(mMinX.ToShortDateString(), myFont, myBrush, mGraphOffset.X, mCanvasSize.Height - mGraphOffset.Y * 0.5f, xAxisFormat);
                if (mGraphSize.Width > 200)
                {
                    xAxisFormat.Alignment = StringAlignment.Center;
                    canvasObj.DrawString(mMinX.AddHours((mMaxX - mMinX).TotalHours * 0.5f).ToShortDateString(), myFont, myBrush, mGraphOffset.X + (mGraphSize.Width * 0.5f), mCanvasSize.Height - mGraphOffset.Y * 0.5f, xAxisFormat);
                }
                xAxisFormat.Alignment = StringAlignment.Far;
                canvasObj.DrawString(mMaxX.ToShortDateString(), myFont, myBrush, mGraphOffset.X + mGraphSize.Width, mCanvasSize.Height - mGraphOffset.Y * 0.5f, xAxisFormat);
            }

            // Write to graphics memory to bitmap
            canvasObj.DrawImage(mCanvas, new Point(0, 0));

            // Free graphics memory for bitmap
            canvasObj.Dispose();
        }

        private void DrawVerticalAxis()
        {
            // Allocate graphics memory for bitmap
            Graphics canvasObj = Graphics.FromImage(mCanvas);

            // Prepare font and alignment for title
            Brush myBrush = new SolidBrush(Color.Black);
            Font myFont = new Font("Arial", mGraphOffset.X * 0.30f, FontStyle.Bold);
            StringFormat yAxisFormat = new StringFormat(StringFormatFlags.DirectionVertical);
            yAxisFormat.Alignment = StringAlignment.Center;
            yAxisFormat.LineAlignment = StringAlignment.Near;

            // Draw Y axis title
            canvasObj.DrawString(mTitleY, myFont, myBrush, 0, mGraphOffset.Y + (mGraphSize.Height * 0.5f), yAxisFormat);

            // Prepare font and alignment for axis markers
            myFont = new Font("Arial", mGraphOffset.X * 0.20f, FontStyle.Italic);
            yAxisFormat.Alignment = StringAlignment.Center;
            yAxisFormat.LineAlignment = StringAlignment.Far;

            // Draw Axis Markers
            yAxisFormat.Alignment = StringAlignment.Near;
            canvasObj.DrawString("" + mMaxY, myFont, myBrush, mGraphOffset.X - 10, mGraphOffset.Y, yAxisFormat);
            if (mGraphSize.Height > 200)
            {
                yAxisFormat.Alignment = StringAlignment.Center;
                canvasObj.DrawString("" + ((mMaxY + mMinY) * 0.5f), myFont, myBrush, mGraphOffset.X - 10, mGraphOffset.Y + (mGraphSize.Height * 0.5f), yAxisFormat);
            }
            yAxisFormat.Alignment = StringAlignment.Far;
            canvasObj.DrawString("" + mMinY, myFont, myBrush, mGraphOffset.X - 10, mGraphOffset.Y + (mGraphSize.Height), yAxisFormat);

            // Write to graphics memory to bitmap
            canvasObj.DrawImage(mCanvas, new Point(0, 0));

            // Free graphics memory for bitmap
            canvasObj.Dispose();
        }

        private void DrawTimeseries(GraphItem graphItem, Graphics graphObj)
        {
            if (graphItem.mStationIndex < mDataset.getColumns())
            {
                Point[] points = new Point[mDataset.getRows()];
                int numMissing = 0;

                // Draw timeseries
                float width = graphItem.mMarkerSize;
                float markerOffset = width * 0.5f;
                bool circle = graphItem.mMarkerShape == GraphMarker.Circle;

                if (mPlotAccumulative)
                {
                        float accumulative = 0.0f;
                    if (mDataset.mTimeUnit == TimeUnit.mo)
                    {
                        for (int i = 0; i < mDataset.getRows(); ++i)
                        {
                            points[i - numMissing].X = convertToGraphCoords(mDataset + i);
                            //if (points[i - numMissing].X > mGraphSize.Width || points[i - numMissing].X < 0)
                            //{
                            //    numMissing++;
                            //    continue;
                            //}

                            accumulative += mDataset[graphItem.mStationIndex, i];
                            points[i - numMissing].Y = mGraphSize.Height - convertToGraphCoords(accumulative);

                            //if (points[i - numMissing].Y > mGraphSize.Height || points[i - numMissing].Y < 0 || mDataset[graphItem.mStationIndex, i] < 0)
                            if (mDataset[graphItem.mStationIndex, i] < 0)
                            {
                                numMissing++;
                                accumulative -= mDataset[graphItem.mStationIndex, i];
                                continue;
                            }
                        }
                    }
                    else// for hourly and daily, use more efficient offsetting since hours and days will have a uniform distance on the x-axis
                    {
                        float offset = getTimeToSpaceOffset(mDataset.getStartDate());
                        float unit = getTimeToSpaceUnit(mDataset.getStartDate(), mDataset + 1);
                        for (int i = 0; i < mDataset.getRows(); ++i)
                        {
                            //points[i].X = convertToGraphCoords(mDataset + i);
                            points[i - numMissing].X = getGraphXCoord(i, offset, unit);
                            //if (points[i - numMissing].X > mGraphSize.Width || points[i - numMissing].X < 0)
                            //{
                            //    numMissing++;
                            //    continue;
                            //}

                            accumulative += mDataset[graphItem.mStationIndex, i];
                            points[i - numMissing].Y = mGraphSize.Height - convertToGraphCoords(accumulative);

                            //if (points[i - numMissing].Y > mGraphSize.Height || points[i - numMissing].Y < 0 || mDataset[graphItem.mStationIndex, i] < 0)
                            if (mDataset[graphItem.mStationIndex, i] < 0)
                            {
                                numMissing++;
                                accumulative -= mDataset[graphItem.mStationIndex, i];
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    if (mDataset.mTimeUnit == TimeUnit.mo)
                    {
                        for (int i = 0; i < mDataset.getRows(); ++i)
                        {
                            points[i].X = convertToGraphCoords(mDataset + i);
                            points[i].Y = mGraphSize.Height - convertToGraphCoords(mDataset[graphItem.mStationIndex, i]);
                        }
                    }
                    else// for hourly and daily, use more efficient offsetting since hours and days will have a uniform distance on the x-axis
                    {
                        float offset = getTimeToSpaceOffset(mDataset.getStartDate());
                        float unit = getTimeToSpaceUnit(mDataset.getStartDate(), mDataset + 1);
                        for (int i = 0; i < mDataset.getRows(); ++i)
                        {
                            //points[i].X = convertToGraphCoords(mDataset + i);
                            points[i].X = getGraphXCoord(i, offset, unit);
                            points[i].Y = mGraphSize.Height - convertToGraphCoords(mDataset[graphItem.mStationIndex, i]);
                        }
                    }
                }
                
                // Draw line
                if (graphItem.mTrendLine)
                {
                    Point[] trimPoints = new Point[points.Length - numMissing];
                    Array.Copy(points, trimPoints, points.Length - numMissing);

                    graphObj.DrawLines(new Pen(graphItem.mTrendLineColor, graphItem.mTrendLineSize), trimPoints);
                }

                // Set line and data colors
                Pen myPen = new Pen(graphItem.mMarkerOutlineColor, graphItem.mMarkerOutlineSize);
                Brush myBrush = new SolidBrush(graphItem.mMarkerFillColor);
                if (circle)
                {
                    for (int i = 0; i < points.Length - numMissing; ++i)
                    {
                        graphObj.FillEllipse(myBrush, points[i].X - markerOffset, points[i].Y - markerOffset, width, width);
                        graphObj.DrawEllipse(myPen, points[i].X - markerOffset, points[i].Y - markerOffset, width, width);
                    }
                }
                else
                {
                    for (int i = 0; i < points.Length - numMissing; ++i)
                    {
                        graphObj.FillRectangle(myBrush, points[i].X - markerOffset, points[i].Y - markerOffset, width, width);
                        graphObj.DrawRectangle(myPen, points[i].X - markerOffset, points[i].Y - markerOffset, width, width);
                    }
                }
            }
        }

        private float getTimeToSpaceUnit(DateTime a, DateTime b)
        {
            return (float)(b - a).TotalHours;
        }

        private float getTimeToSpaceOffset(DateTime a)
        {
            return (float)(a - mMinX).TotalHours;
        }

        private int getGraphXCoord(int x, float offset, float unit)
        {
            if (float.IsInfinity(mXScale)) return -1;
            return (int)Math.Floor((offset + x * unit) * mXScale);
        }

        private Point convertToGraphCoords(float x, float y)
        {
            Point point = new Point();

            if (float.IsInfinity(mXScale) || float.IsInfinity(mYScale)) return new Point(-1, -1);

            point.X = (int)Math.Floor(x * mXScale);
            point.Y = (int)Math.Floor((y - mMinY) * mYScale);

            return point;
        }

        private Point convertToGraphCoords(DateTime x, float y)
        {
            Point point = new Point();

            if (float.IsInfinity(mXScale) || float.IsInfinity(mYScale)) return new Point(0,0);

            point.X = (int)Math.Floor((x - mMinX).Hours * mXScale);
            point.Y = (int)Math.Floor((y - mMinY) * mYScale);

            return point;
        }

        private int convertToGraphCoords(DateTime x)
        {
            if (float.IsInfinity(mXScale)) return -1;
            return Math.Max(-1,(int)Math.Floor((x - mMinX).TotalHours * mXScale));
        }

        private int convertToGraphCoords(float y)
        {
            if (float.IsInfinity(mYScale)) return -1;
            return Math.Max(-1,(int)Math.Floor((y - mMinY) * mYScale));
        }

        public DateTime convertToTimeCoords(float x)
        {
            if (float.IsInfinity(mXScale)) return mMinX;
            return mMinX.AddHours((int)Math.Floor((x-mGraphOffset.X) / mXScale));
        }

        public float convertToValueCoords(float y)
        {
            if (float.IsInfinity(mYScale)) return mMinY;
            return mMaxY - ((y - mGraphOffset.Y) / mYScale);
        }
    }
}
