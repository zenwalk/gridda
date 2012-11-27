using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GRIDDA
{
    class Delineator
    {
        String shpFilepath;
        String outDir;
        GriddedDataDetails griddedDataDetails;

        // Shape
        Shape[] shapes;

        // Grid
        Grid dataGrid;
        Grid displayGrid;

        // Display
        public Bitmap polygonBitmap;
        float xScale;
        float yScale;
        
        public Delineator(String shapeFile, GriddedDataDetails griddedDataDetails, String outDir)
        {
            shpFilepath = shapeFile;
            this.outDir = outDir;
            this.griddedDataDetails = griddedDataDetails;

            shapes = new Shape[0];
            dataGrid = new Grid(griddedDataDetails);
            calcDisplayGrid();

            polygonBitmap = new Bitmap(2048, 2048);
            Graphics gfx = Graphics.FromImage(polygonBitmap);
            gfx.Clear(Color.White);
        }

        public void Delineate(bool boundary, bool areas)
        {
            readShapeFile();
            calcDisplayGrid();

            // Calc intersection
            calcIntersect();

            // Calc integral
            calcIntegral();

            clearBitmap(polygonBitmap);
            for (int i = 0; i < shapes.Length; ++i)
            {
                // Draw all shapes
                drawShape(shapes[i], ref polygonBitmap);
                drawGrid(shapes[i], ref polygonBitmap);
            }
            
            // Output grid weights for each shape
            StreamWriter outFile = new StreamWriter(getWeightFile());

            outFile.WriteLine("shape,lon,lat,weight");

            for (int l = 0; l < shapes.Length; ++l)
            {
                for (int i = 0; i < shapes[l].intersectingPolygons.GetLength(0); ++i)
                {
                    for (int j = 0; j < shapes[l].intersectingPolygons.GetLength(1); ++j)
                    {
                        if (shapes[l].intersectingPolygons[i, j] != null && shapes[l].intersectingPolygons[i, j].Length != 0)
                        {
                            double2 vertex = shapes[l].intersectingGrid.calcGridVertex(i, j);
                            outFile.WriteLine(l + 1 + "," + (vertex.x + dataGrid.xRes / 2.0m) + "," + (vertex.y - dataGrid.yRes / 2.0m) + "," + shapes[l].integralValue[i, j]);
                        }
                    }
                }
            }
            outFile.Close();
            
            Bitmap bitmap = new Bitmap(polygonBitmap.Width, polygonBitmap.Height);
            clearBitmap(bitmap);

            Color[] colors = new Color[16];
            colors[0] = Color.LightSalmon;
            colors[1] = Color.LightSeaGreen;
            colors[2] = Color.LightGoldenrodYellow;
            colors[3] = Color.LightBlue;
            colors[4] = Color.LightCoral;
            colors[5] = Color.LightCyan;
            colors[6] = Color.LightSlateGray;
            colors[7] = Color.LightPink;
            colors[8] = Color.Aquamarine;
            colors[9] = Color.CornflowerBlue;
            colors[10] = Color.DarkSeaGreen;
            colors[11] = Color.DodgerBlue;
            colors[12] = Color.PaleVioletRed;
            colors[13] = Color.Plum;
            colors[14] = Color.LightGreen;
            colors[15] = Color.Goldenrod;

            if (boundary)
            {
                for (int i = 0; i < shapes.Length; ++i)
                {
                    // Draw intersection
                    //drawIntersection(shapes[i], ref bitmap, colors[i%colors.Length]);
                    fillShape(shapes[i], ref bitmap, colors[i % colors.Length]);
                    drawGrid(shapes[i], ref bitmap);

                }
                // Save intersection
                bitmap.Save(outDir + Path.DirectorySeparatorChar + "Shapes.png", ImageFormat.Png);
            }

            if (areas)
            {
                for (int i = 0; i < shapes.Length; ++i)
                {
                    // Draw integral
                    clearBitmap(bitmap, Color.LightBlue);
                    drawIntegral(shapes[i], ref bitmap);
                    drawShape(shapes[i], ref bitmap);
                    drawGrid(shapes[i], ref bitmap);

                    // Save integral
                    bitmap.Save(outDir + Path.DirectorySeparatorChar + "Area_" + (i + 1).ToString("0000") + ".png", ImageFormat.Png);
                }
            }
        }

        private void calcDisplayGrid()
        {
            displayGrid = new Grid(dataGrid);

            // Calculate max mbr of all shapes
            MBR mbr = new MBR(dataGrid.mbr.xMin, dataGrid.mbr.xMax, dataGrid.mbr.yMin, dataGrid.mbr.yMax);
            if (shapes.Length != 0 && shapes[0].polygon.vertices.Length != 0)
            {
                mbr = new MBR(shapes[0].polygon.mbr.xMin, shapes[0].polygon.mbr.xMax, shapes[0].polygon.mbr.yMin, shapes[0].polygon.mbr.yMax);
            }

            foreach (Shape shape in shapes)
            {
                if (shape.polygon.vertices.Length != 0)
                {
                    // Calculate new grid mbr based on shape mbr
                    mbr.xMin = shape.polygon.mbr.xMin < mbr.xMin ? shape.polygon.mbr.xMin : mbr.xMin;
                    mbr.xMax = shape.polygon.mbr.xMax > mbr.xMax ? shape.polygon.mbr.xMax : mbr.xMax;
                    mbr.yMin = shape.polygon.mbr.yMin < mbr.yMin ? shape.polygon.mbr.yMin : mbr.yMin;
                    mbr.yMax = shape.polygon.mbr.yMax > mbr.yMax ? shape.polygon.mbr.yMax : mbr.yMax;
                }
            }

            // Calculate new grid mbr based on shape mbr
            displayGrid.mbr.xMin += Math.Floor((mbr.xMin - dataGrid.mbr.xMin) / dataGrid.xRes) * dataGrid.xRes - dataGrid.xRes;
            displayGrid.mbr.yMin += Math.Floor((mbr.yMin - dataGrid.mbr.yMin) / dataGrid.yRes) * dataGrid.yRes - dataGrid.yRes;

            displayGrid.mbr.xMax += Math.Ceiling((mbr.xMax - dataGrid.mbr.xMax) / dataGrid.xRes) * dataGrid.xRes + dataGrid.xRes;
            displayGrid.mbr.yMax += Math.Ceiling((mbr.yMax - dataGrid.mbr.yMax) / dataGrid.yRes) * dataGrid.yRes + dataGrid.yRes;

            // Setup world scale
            int width = Math.Max(512, Math.Min(4096, (int)Math.Ceiling(Math.Abs((displayGrid.mbr.xMax - displayGrid.mbr.xMin) / dataGrid.xRes) * 50)));
            int height = Math.Max(512, Math.Min(4096, (int)Math.Ceiling(Math.Abs((displayGrid.mbr.yMax - displayGrid.mbr.yMin) / dataGrid.yRes) * 50)));
            polygonBitmap = new Bitmap(width, height);
            xScale = polygonBitmap.Width / (float)(displayGrid.mbr.xMax - displayGrid.mbr.xMin);
            yScale = polygonBitmap.Height / (float)(displayGrid.mbr.yMax - displayGrid.mbr.yMin);
        }

        private void readShapeFile()
        {
            // Open the three required binary files
            BinaryReader shpFile = new BinaryReader(File.Open(shpFilepath, FileMode.Open));

            // Read shp header

            // Find number of shapes
            shpFile.BaseStream.Seek(100, SeekOrigin.Begin);

            int numShapes = 0;
            Int32 recordLength;
            Int32 recordNumber;

            while (shpFile.BaseStream.Position < shpFile.BaseStream.Length)
            {

                // Get shape size and length
                recordNumber = Convert.ToInt32(ReverseBytes(shpFile.ReadUInt32()));
                recordLength = Convert.ToInt32(ReverseBytes(shpFile.ReadUInt32()));

                // Add length to current position and increment counter of shapes
                numShapes++;

                // Seek to beginning of next shape
                shpFile.BaseStream.Seek(recordLength * 2, SeekOrigin.Current);
            }

            shapes = new Shape[numShapes];
            shpFile.BaseStream.Seek(100, SeekOrigin.Begin);
            for (int i = 0; i < numShapes; ++i)
            {
                // Read record number and length
                recordNumber = Convert.ToInt32(ReverseBytes(shpFile.ReadUInt32()));
                recordLength = Convert.ToInt32(ReverseBytes(shpFile.ReadUInt32()));

                // Read shape type
                Int32 shapeType = shpFile.ReadInt32();

                switch (shapeType)
                {
                    case 5:
                        shapes[i].polygon = new Polygon(shpFile);
                        break;
                    default:
                        throw new Exception("Reading Shapefile error: Invalid shape");
                }
            }

            shpFile.Close();
        }

        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        private void clearBitmap(Bitmap bitmap)
        {
            clearBitmap(bitmap, Color.White);
        }

        private void clearBitmap(Bitmap bitmap, Color color)
        {
            // Clear bitmap
            Graphics gfx = Graphics.FromImage(bitmap);
            gfx.Clear(color);
        }

        private void calcIntersect()
        {
            // Add all intersecting points to grid
            for (int l = 0; l < shapes.Length; ++l)
            {
                shapes[l].intersectingGrid = new DynamicGrid(displayGrid);
                Point currentCell = shapes[l].intersectingGrid.calcGridCell(shapes[l].polygon.vertices[0]);

                for (int k = 0; k < shapes[l].polygon.indices.Length; ++k)
                {
                    // Calculate the number of points in the current shape (for multiple shapes in shape file)
                    int nPoints;
                    if (k + 1 == shapes[l].polygon.indices.Length)
                    {
                        nPoints = shapes[l].polygon.vertices.Length;
                    }
                    else
                    {
                        nPoints = shapes[l].polygon.indices[k + 1];
                    }

                    // Find current cell position
                    currentCell = shapes[l].intersectingGrid.calcGridCell(shapes[l].polygon.vertices[shapes[l].polygon.indices[k]]);

                    for (int i = shapes[l].polygon.indices[k]; i < nPoints; ++i)
                    {
                        // Find grid cell of next vertex
                        Point newCell = shapes[l].intersectingGrid.calcGridCell(shapes[l].polygon.vertices[i]);

                        // Crossed into new cell
                        if (currentCell != newCell)
                        {
                            shapes[l].intersectingGrid.calcIntersections(currentCell, newCell, shapes[l].polygon.vertices[i - 1], shapes[l].polygon.vertices[i], i);

                            currentCell = newCell;
                        }
                    }
                    shapes[l].intersectingGrid.intersectionList.Finish();
                }

                // Make polygons for each shape
                shapes[l].intersectingPolygons = new DynamicPolygon[shapes[l].intersectingGrid.gridPolygons.GetLength(0), shapes[l].intersectingGrid.gridPolygons.GetLength(1)];

                for (int i = 0; i < shapes[l].intersectingPolygons.GetLength(0); ++i)
                {
                    for (int j = 0; j < shapes[l].intersectingPolygons.GetLength(1); ++j)
                    {
                        Intersection[] intersections = shapes[l].intersectingGrid.intersectionList.getIntersections(new int2(i, j));

                        if (intersections.Length > 0)
                        {
                            shapes[l].intersectingPolygons[i, j] = new DynamicPolygon();

                            // For each intersection of the shape, add shape vertices and cell vertices
                            for (int k = 0; k < intersections.Length; ++k)
                            {
                                // Add first intersection
                                shapes[l].intersectingPolygons[i, j].addVertex(intersections[k].startIntersection);

                                // Add shape vertices
                                int nVertices = intersections[k].finalIndice - intersections[k].startIndice;

                                // Intersection loops around shape array
                                if (nVertices < 0)
                                {
                                    // work out which shape it comes from, start and end indices of shape

                                    int endOfShape = 0;
                                    int startOfShape = 0;
                                    for (int m = 0; m < shapes[l].polygon.indices.Length; ++m)
                                    {
                                        // if from the last shape, use total length
                                        if (m + 1 == shapes[l].polygon.indices.Length)
                                        {
                                            endOfShape = shapes[l].polygon.vertices.Length;
                                        }
                                        else
                                        {
                                            // will be true one interation before end is found
                                            if (shapes[l].polygon.indices[m + 1] < intersections[k].finalIndice)
                                            {
                                                startOfShape = shapes[l].polygon.indices[m + 1];
                                            }
                                            // if not last shape, find shape which starts after it and use as end point
                                            if (shapes[l].polygon.indices[m + 1] > intersections[k].startIndice)
                                            {
                                                endOfShape = shapes[l].polygon.indices[m + 1];
                                                continue;
                                            }
                                        }
                                    }

                                    // Add vertices from start of intersection to end of shape
                                    shapes[l].intersectingPolygons[i, j].addVertices(shapes[l].polygon.vertices, intersections[k].startIndice, endOfShape - intersections[k].startIndice);

                                    // Add vertices from start of shape to end of intersection
                                    shapes[l].intersectingPolygons[i, j].addVertices(shapes[l].polygon.vertices, startOfShape, intersections[k].finalIndice - startOfShape);
                                }

                                if (nVertices > 0)
                                {
                                    shapes[l].intersectingPolygons[i, j].addVertices(shapes[l].polygon.vertices, intersections[k].startIndice, nVertices);
                                }

                                // Add end intersection point
                                shapes[l].intersectingPolygons[i, j].addVertex(intersections[k].finalIntersection);

                                // Add grid vertices until next intersection
                                int nextIntersection = (k + 1 == intersections.Length ? 0 : k + 1);

                                // special case!!
                                // check that all intersections are not on the same side
                                bool allOneSide = false;
                                if (nextIntersection == 0)
                                {
                                    allOneSide = true;
                                    for (int m = 0; m < intersections.Length; ++m)
                                    {
                                        if (intersections[m].startCell != intersections[m].finalCell)
                                        {
                                            allOneSide = false;
                                        }
                                    }
                                }

                                // special case!
                                if (allOneSide)
                                {
                                    // if all intersections are on the same side
                                    // algorithm wont add any grid corners to polygon
                                    // this is a problem is the intersections are subtractive, not additive
                                    // subtractive intersections occur when intersection direction is CW
                                    // same direction as grid vertices
                                    // to detect, we take each intersection
                                    // if change in intersection points is the same sign (+/-)
                                    // as change grid vertices on that side (+/-)
                                    // then add ALL grid corners to polygon

                                    int delX = intersections[k].finalCell.x - i;
                                    int delY = intersections[k].finalCell.y - j;

                                    decimal cellX = 0;
                                    decimal cellY = 0;

                                    // find direction of grid cell side (+/-)(x/y)
                                    if (delX == 0 && delY == -1) { cellX = 1; }
                                    else if (delX == 1 && delY == 0) { cellY = -1; }
                                    else if (delX == 0 && delY == 1) { cellX = -1; }
                                    else if (delX == -1 && delY == 0) { cellY = +1; }

                                    allOneSide = false;

                                    // if side goes horizontal, add all intersection lengths
                                    if (delY != 0)
                                    {
                                        decimal total = 0;

                                        for (int m = 0; m < intersections.Length; ++m)
                                        {
                                            total += intersections[m].finalIntersection.x - intersections[m].startIntersection.x;
                                        }

                                        // if sum of lengths is same sign (+/-) as grid cell side, add all grid corners
                                        if (cellX * total >= 0)
                                        {
                                            allOneSide = true;
                                        }
                                    }
                                    else // if side goes vertical, add all intersection lengths
                                    {
                                        decimal total = 0;

                                        for (int m = 0; m < intersections.Length; ++m)
                                        {
                                            total += intersections[m].finalIntersection.y - intersections[m].startIntersection.y;
                                        }

                                        // if sum of lengths is same sign (+/-) as grid cell side, add all grid corners
                                        if (cellY * total >= 0)
                                        {
                                            allOneSide = true;
                                        }
                                    }

                                    // Add all corners
                                    if (allOneSide)
                                    {
                                        int2 nextCell = intersections[k].finalCell;
                                        for (int m = 0; m < 4; ++m)
                                        {
                                            // Add one grid cell vertex

                                            // Find relative position of last exiting vertex
                                            delX = nextCell.x - i;
                                            delY = nextCell.y - j;

                                            // Find which corner needs to be added
                                            int a = i;
                                            int b = j;

                                            if (delX == 0 && delY == -1) { a += 1; b += 0; nextCell.x = i + 1; nextCell.y = j + 0; }
                                            else if (delX == 1 && delY == 0) { a += 1; b += 1; nextCell.x = i; nextCell.y = j + 1; }
                                            else if (delX == 0 && delY == 1) { a += 0; b += 1; nextCell.x = i - 1; nextCell.y = j + 0; }
                                            else if (delX == -1 && delY == 0) { a += 0; b += 0; nextCell.x = i; nextCell.y = j - 1; }
                                            else
                                            {
                                                throw new Exception("Next cell is diagonally adjacent!");
                                            }

                                            shapes[l].intersectingPolygons[i, j].addVertex(shapes[l].intersectingGrid.calcGridVertex(new int2(a, b)));
                                        }

                                    }
                                }
                                else
                                {
                                    int2 nextCell = intersections[k].finalCell;
                                    while (nextCell != intersections[nextIntersection].startCell)
                                    {
                                        // Add one grid cell vertex

                                        // Find relative position of last exiting vertex
                                        int delX = nextCell.x - i;
                                        int delY = nextCell.y - j;

                                        // Find which corner needs to be added
                                        int a = i;
                                        int b = j;

                                        if (delX == 0 && delY == -1) { a += 1; b += 0; nextCell.x = i + 1; nextCell.y = j + 0; }
                                        else if (delX == 1 && delY == 0) { a += 1; b += 1; nextCell.x = i; nextCell.y = j + 1; }
                                        else if (delX == 0 && delY == 1) { a += 0; b += 1; nextCell.x = i - 1; nextCell.y = j + 0; }
                                        else if (delX == -1 && delY == 0) { a += 0; b += 0; nextCell.x = i; nextCell.y = j - 1; }
                                        else
                                        {
                                            throw new Exception("Next cell is diagonally adjacent!");
                                        }

                                        shapes[l].intersectingPolygons[i, j].addVertex(shapes[l].intersectingGrid.calcGridVertex(new int2(a, b)));
                                    }
                                }
                            }
                        }
                    }
                }


                // Make new polygon
                bool noIntersect = true;
                for (int i = 0; i < shapes[l].intersectingPolygons.GetLength(0) && noIntersect; ++i)
                {
                    for (int j = 0; j < shapes[l].intersectingPolygons.GetLength(1) && noIntersect; ++j)
                    {
                        if (shapes[l].intersectingPolygons[i, j] != null)
                        {
                            noIntersect = false;
                        }
                    }
                }
                if (noIntersect && shapes[l].polygon.vertices.Length != 0)
                {
                    int2 location = displayGrid.calcGridCell(shapes[l].polygon.vertices[0]);
                    shapes[l].intersectingPolygons[location.x, location.y] = new DynamicPolygon();
                    shapes[l].intersectingPolygons[location.x, location.y].addVertices(shapes[l].polygon.vertices);
                }


                bool polygonsAdded = false;

                double2 res = new double2(shapes[l].intersectingGrid.xRes, shapes[l].intersectingGrid.yRes);

                bool[,] examined = new bool[shapes[l].intersectingPolygons.GetLength(0), shapes[l].intersectingPolygons.GetLength(1)];


                do
                {
                    polygonsAdded = false;
                    for (int i = 0; i < shapes[l].intersectingPolygons.GetLength(0); ++i)
                    {
                        for (int j = 0; j < shapes[l].intersectingPolygons.GetLength(1); ++j)
                        {
                            if (examined[i, j] == false && shapes[l].intersectingPolygons[i, j] != null && shapes[l].intersectingPolygons[i, j].Length != 0)
                            {
                                examined[i, j] = true;
                                double2 vertex = shapes[l].intersectingGrid.calcGridVertex(new int2(i, j));

                                // Check top
                                if (j - 1 >= 0)
                                {
                                    if (shapes[l].intersectingPolygons[i, j - 1] == null)
                                    {
                                        shapes[l].intersectingPolygons[i, j - 1] = new DynamicPolygon();

                                        if (checkForNullNeighbours(shapes[l].intersectingPolygons, i, j - 1))
                                        {
                                            if (shapes[l].intersectingPolygons[i, j].checkConsecutive(vertex, vertex + new double2(res.x, 0)))
                                            {
                                                shapes[l].intersectingPolygons[i, j - 1].addGridVertices(vertex + new double2(0, res.y), res);
                                                polygonsAdded = true;
                                            }
                                        }
                                    }
                                }

                                // Check right
                                if (i + 1 < shapes[l].intersectingPolygons.GetLength(0))
                                {
                                    if (shapes[l].intersectingPolygons[i + 1, j] == null)
                                    {
                                        shapes[l].intersectingPolygons[i + 1, j] = new DynamicPolygon();
                                        if (checkForNullNeighbours(shapes[l].intersectingPolygons, i + 1, j))
                                        {
                                            if (shapes[l].intersectingPolygons[i, j].checkConsecutive(vertex + new double2(res.x, 0), vertex + new double2(res.x, -res.y)))
                                            {
                                                shapes[l].intersectingPolygons[i + 1, j].addGridVertices(vertex + new double2(res.x, 0), res);
                                                polygonsAdded = true;
                                            }
                                        }
                                    }
                                }

                                // Check bottom
                                if (j + 1 < shapes[l].intersectingPolygons.GetLength(1))
                                {
                                    if (shapes[l].intersectingPolygons[i, j + 1] == null)
                                    {
                                        shapes[l].intersectingPolygons[i, j + 1] = new DynamicPolygon();

                                        if (checkForNullNeighbours(shapes[l].intersectingPolygons, i, j + 1))
                                        {
                                            if (shapes[l].intersectingPolygons[i, j].checkConsecutive(vertex + new double2(res.x, -res.y), vertex + new double2(0, -res.y)))
                                            {
                                                shapes[l].intersectingPolygons[i, j + 1].addGridVertices(vertex + new double2(0, -res.y), res);
                                                polygonsAdded = true;
                                            }
                                        }
                                    }
                                }

                                // Check left
                                if (i - 1 >= 0)
                                {
                                    if (shapes[l].intersectingPolygons[i - 1, j] == null)
                                    {
                                        shapes[l].intersectingPolygons[i - 1, j] = new DynamicPolygon();

                                        if (checkForNullNeighbours(shapes[l].intersectingPolygons, i - 1, j))
                                        {
                                            if (shapes[l].intersectingPolygons[i, j].checkConsecutive(vertex + new double2(0, -res.y), vertex))
                                            {
                                                shapes[l].intersectingPolygons[i - 1, j].addGridVertices(vertex + new double2(-res.x, 0), res);
                                                polygonsAdded = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } while (polygonsAdded);
            }
        }

        private void calcIntegral()
        {
            for (int m = 0; m < shapes.Length; ++m)
            {
                shapes[m].integralValue = new decimal[shapes[m].intersectingGrid.gridPolygons.GetLength(0), shapes[m].intersectingGrid.gridPolygons.GetLength(1)];
                decimal size = shapes[m].intersectingGrid.xRes * shapes[m].intersectingGrid.yRes;

                for (int i = 0; i < shapes[m].intersectingPolygons.GetLength(0); ++i)
                {
                    for (int j = 0; j < shapes[m].intersectingPolygons.GetLength(1); ++j)
                    {
                        if (shapes[m].intersectingPolygons[i, j] != null && shapes[m].intersectingPolygons[i, j].Length != 0)
                        {
                            shapes[m].integralValue[i, j] = shapes[m].intersectingPolygons[i, j].calcIntegral() / size;

                            while (shapes[m].integralValue[i, j] > 1)
                            {
                                shapes[m].integralValue[i, j] -= 1;
                            }
                        }
                        else
                        {
                            shapes[m].integralValue[i, j] = 0;
                        }
                    }
                }

                //for (int i = 0; i < shapes[m].intersectingPolygons.GetLength(0); ++i)
                //{
                //    for (int j = 0; j < shapes[m].intersectingPolygons.GetLength(1); ++j)
                //    {
                //        if (shapes[m].integralValue[i, j] == 1)
                //        {
                //            if (!checkForNullNeighbours(shapes[m].intersectingPolygons, i, j))
                //            {
                //                // mistake, remove
                //                shapes[m].integralValue[i, j] = 0;
                //                shapes[m].intersectingPolygons[i, j] = new DynamicPolygon();
                //            }                            
                //        }
                //    }
                //}
            }
        }

        private bool checkForNullNeighbours(DynamicPolygon[,] intersectingPolygons, int i, int j)
        {
            if (i > 0 && intersectingPolygons[i - 1, j] != null && intersectingPolygons[i - 1, j].Length == 0)
            {
                return false;
            }
            if (i + 1 < intersectingPolygons.GetLength(0) && intersectingPolygons[i + 1, j] != null && intersectingPolygons[i + 1, j].Length == 0)
            {
                return false;
            }
            if (j > 0 && intersectingPolygons[i, j - 1] != null && intersectingPolygons[i, j - 1].Length == 0)
            {
                return false;
            }
            if (j + 1 < intersectingPolygons.GetLength(1) && intersectingPolygons[i, j + 1] != null && intersectingPolygons[i, j + 1].Length == 0)
            {
                return false;
            }
            return true;
        }

        private bool isGridVertice(double2 vertex)
        {
            return ((vertex.x - dataGrid.mbr.xMin) % dataGrid.xRes == 0 && (vertex.y - dataGrid.mbr.yMin) % dataGrid.yRes == 0);
        }

        private bool isOnGridLine(double2 vertex)
        {
            return ((vertex.x - dataGrid.mbr.xMin) % dataGrid.xRes == 0 || (vertex.y - dataGrid.mbr.yMin) % dataGrid.yRes == 0);
        }

        private Point worldToScreenCoordinates(double2 worldPoint, Bitmap bitmap)
        {
            Point screenPoint = new Point();

            screenPoint.X = (int)((double)(worldPoint.x - displayGrid.mbr.xMin) * xScale);
            screenPoint.Y = bitmap.Height - (int)((double)(worldPoint.y - displayGrid.mbr.yMin) * yScale);

            return screenPoint;
        }

        private double2 screenToWorldCoordinates(Point point, Bitmap bitmap)
        {
            double2 worldPoint = new double2();

            worldPoint.x = (decimal)(point.X / xScale) + displayGrid.mbr.xMin;
            worldPoint.y = (decimal)((bitmap.Height - point.Y) / yScale) + displayGrid.mbr.yMin;

            return worldPoint;
        }

        private void drawGrid(Shape shape, ref Bitmap bitmap)
        {
            // Initialize brush
            Graphics gfx = Graphics.FromImage(bitmap);
            Pen pen = new Pen(Color.Black, 1.0f);

            if (!Single.IsInfinity(xScale) && !Single.IsInfinity(yScale))
            {
                foreach (DynamicPolygon polygon in shape.intersectingGrid.gridPolygons)
                {
                    Point[] points = new Point[polygon.Length];

                    for (int i = 0; i < polygon.Length; ++i)
                    {
                        gfx.DrawLine(pen, worldToScreenCoordinates(polygon[i], bitmap), worldToScreenCoordinates(polygon[(i + 1) % polygon.Length], bitmap));
                        points[i] = worldToScreenCoordinates(polygon[i], bitmap);
                        points[i].X -= 1;
                        points[i].Y -= 1;
                        gfx.DrawEllipse(pen, new Rectangle(points[i], new Size(2, 2)));
                    }
                }
            }
        }

        private void drawShape(Shape shape, ref Bitmap bitmap)
        {
            // Initialize brush
            Pen pen = new Pen(Color.Blue, 1.0f);

            Graphics gfx = Graphics.FromImage(bitmap);

            if (!Double.IsInfinity(xScale) && !Double.IsInfinity(yScale))
            {
                for (int i = 0; i < shape.polygon.indices.Length; ++i)
                {
                    Point[] points;

                    if (i + 1 < shape.polygon.indices.Length)
                    {
                        points = new Point[shape.polygon.indices[i + 1] - shape.polygon.indices[i]];
                    }
                    else
                    {
                        points = new Point[shape.polygon.vertices.Length - shape.polygon.indices[i]];
                    }

                    for (int j = 0; j < points.Length; ++j)
                    {
                        points[j] = worldToScreenCoordinates(shape.polygon.vertices[j + shape.polygon.indices[i]], bitmap);

                        Point point = new Point(points[j].X, points[j].Y);
                        point.X -= 1;
                        point.Y -= 1;

                        gfx.DrawEllipse(pen, new Rectangle(point, new Size(2, 2)));
                    }

                    gfx.DrawPolygon(pen, points);
                }
            }
        }

        private void fillShape(Shape shape, ref Bitmap bitmap, Color color)
        {
            // Initialize brush
            Pen pen = new Pen(Color.Black, 1.0f);
            Brush brush = new SolidBrush(color);

            Graphics gfx = Graphics.FromImage(bitmap);

            if (!Double.IsInfinity(xScale) && !Double.IsInfinity(yScale))
            {
                for (int i = 0; i < shape.polygon.indices.Length; ++i)
                {
                    Point[] points;

                    if (i + 1 < shape.polygon.indices.Length)
                    {
                        points = new Point[shape.polygon.indices[i + 1] - shape.polygon.indices[i]];
                    }
                    else
                    {
                        points = new Point[shape.polygon.vertices.Length - shape.polygon.indices[i]];
                    }

                    for (int j = 0; j < points.Length; ++j)
                    {
                        points[j] = worldToScreenCoordinates(shape.polygon.vertices[j + shape.polygon.indices[i]], bitmap);

                        Point point = new Point(points[j].X, points[j].Y);
                        point.X -= 1;
                        point.Y -= 1;

                        gfx.DrawEllipse(pen, new Rectangle(point, new Size(2, 2)));
                    }

                    gfx.FillPolygon(brush, points);
                    gfx.DrawPolygon(pen, points);
                }
            }
        }

        private void drawIntersection(Shape shape, ref Bitmap bitmap, Color color)
        {
            Graphics gfx = Graphics.FromImage(bitmap);

            Brush brush = new SolidBrush(color);
            if (!Single.IsInfinity(xScale) && !Single.IsInfinity(yScale))
            {
                int l = 0;
                for (int i = 0; i < shape.intersectingPolygons.GetLength(0); ++i)
                {
                    for (int j = 0; j < shape.intersectingPolygons.GetLength(1); ++j, ++l)
                    {
                        if (shape.intersectingPolygons[i, j] != null && shape.intersectingPolygons[i, j].Length != 0)
                        {
                            DynamicPolygon polygon = shape.intersectingPolygons[i, j];

                            Point[] points = new Point[polygon.Length];

                            for (int k = 0; k < polygon.Length; ++k)
                            {
                                points[k] = worldToScreenCoordinates(polygon[k], bitmap);
                            }

                            gfx.FillPolygon(brush, points);
                        }
                    }
                }
            }
        }

        private void drawIntegral(Shape shape, ref Bitmap bitmap)
        {
            Graphics gfx = Graphics.FromImage(bitmap);

            if (!Single.IsInfinity(xScale) && !Single.IsInfinity(yScale))
            {
                if (shape.intersectingGrid.gridPolygons.Length == shape.integralValue.Length)
                {
                    decimal rcpMax = 1.0m;/// (0.05 * 0.05);// integralValue.Cast<double>().Max();

                    for (int i = 0; i < shape.intersectingGrid.gridPolygons.GetLength(0); ++i)
                    {
                        for (int j = 0; j < shape.intersectingGrid.gridPolygons.GetLength(1); ++j)
                        {
                            Point[] points = new Point[shape.intersectingGrid[i, j].Length];

                            for (int k = 0; k < shape.intersectingGrid[i, j].Length; ++k)
                            {
                                points[k] = worldToScreenCoordinates(shape.intersectingGrid[i, j][k], bitmap);
                            }

                            if (shape.integralValue[i, j] != 0)
                            {
                                int green = Math.Max(0, Math.Min((int)255, 255 - (int)(255 * (shape.integralValue[i, j] * rcpMax))));

                                gfx.FillPolygon(new SolidBrush(Color.FromArgb(255, green, green)), points);
                            }
                        }
                    }
                }
            }
        }

        public string getWeightFile()
        {
            return outDir + Path.DirectorySeparatorChar + "Weights.csv";
        }
    }
}
