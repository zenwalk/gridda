using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRIDDA
{
    class DynamicGrid:Grid
    {
        public DynamicPolygon[,] gridPolygons;
        public Dictionary<double2, int> gridIntersections;
        public IntersectionList intersectionList;

        public DynamicGrid(MBR mbr, decimal xRes, decimal yRes) 
            : base(mbr, xRes, yRes)
        {
            gridIntersections = new Dictionary<double2, int>();
            gridPolygons = new DynamicPolygon[0, 0];
            intersectionList = new IntersectionList();
        }

        public DynamicGrid(Grid grid)
            : base(grid)
        {
            gridIntersections = new Dictionary<double2, int>();

            int nColumns = (int)Math.Ceiling((mbr.xMax - mbr.xMin - double2.Tolerance) / xRes);
            int nRows = (int)Math.Ceiling((mbr.yMax - mbr.yMin - double2.Tolerance) / yRes)+1;

            gridPolygons = new DynamicPolygon[nColumns, nRows];

            for (int i = 0; i < nColumns; ++i)
            {
                for (int j = 0; j < nRows; ++j)
                {
                    gridPolygons[i, j] = new DynamicPolygon();
                    gridPolygons[i, j][0] = new double2(mbr.xMin + ((i) * xRes), mbr.yMax - ((j) * yRes));
                    gridPolygons[i, j][1] = new double2(mbr.xMin + ((i + 1) * xRes), mbr.yMax - ((j) * yRes));
                    gridPolygons[i, j][2] = new double2(mbr.xMin + ((i + 1) * xRes), mbr.yMax - ((j + 1) * yRes));
                    gridPolygons[i, j][3] = new double2(mbr.xMin + ((i) * xRes), mbr.yMax - ((j + 1) * yRes));
                }
            }

            intersectionList = new IntersectionList();
        }

        new public DynamicPolygon this[int2 i]
        {
            get
            {
                return gridPolygons[i.x, i.y];
            }
            set
            {
                gridPolygons[i.x, i.y] = value;
            }
        }

        new public DynamicPolygon this[int i, int j]
        {
            get
            {
                return gridPolygons[i, j];
            }
            set
            {
                gridPolygons[i, j] = value;
            }
        }

        public void calcIntersections(int2 originalCell, int2 finalCell, double2 pointA, double2 pointB, int shapeIndex)
        {
            // Calc number of intersections to be found
            int nPoints = (int)(Math.Abs(originalCell.x - finalCell.x) + Math.Abs(originalCell.y - finalCell.y));

            // Calc equation of line: 
            // F = Shape(0)*(1-t) + Shape(1)*(t)
            // tx = (Fx - pointAx) / (pointBx - pointAx)
            // ty = (Fy - pointAy) / (pointBy - pointAy)
            //
            // F = grid cell corner where Fx and Fy lines meet
            // Fx = horizontal grid cell line
            // Fy = vertical grid cell line
            // Lowest t wins! t < 0 or t > 1 is disqualified.
            // if pointBx-pointAx == 0, line is vertical, 

            // Denominators
            double2 delLine;
            delLine.x = pointB.x - pointA.x;
            delLine.y = pointB.y - pointA.y;

            // Find grid cell corner which is the intersection of the two lines most likely to be intersected (F in above equations)
            double2 intersectionVertex = calcGridVertex(originalCell);           
            int2 delCell;
            delCell.x = (finalCell.x - originalCell.x > 0) ? 1 : -1;
            delCell.y = (finalCell.y - originalCell.y > 0) ? 1 : -1;
            intersectionVertex.x += delCell.x == 1 ? xRes : 0;
            intersectionVertex.y += delCell.y == 1 ? -yRes : 0;

            int2 cellIter = originalCell;
            
            for (int i = 0; i < nPoints; ++i)
            {
                decimal tx; 
                decimal ty;

                if (delLine.x == 0) 
                    tx = -1;
                else
                    tx = (intersectionVertex.x - pointA.x) / delLine.x;

                if (delLine.y == 0)
                    ty = -1;
                else
                    ty = (intersectionVertex.y - pointA.y) / delLine.y;

                if (tx > 1 + double2.Tolerance || tx < 0 - double2.Tolerance) { tx = 10; }
                if (ty > 1 + double2.Tolerance || ty < 0 - double2.Tolerance) { ty = 10; }

                // Hit vertical line at tx first
                if (tx < ty)
                {
                    double2 intersection;
                    intersection.x = intersectionVertex.x;
                    intersection.y = (tx * delLine.y) + pointA.y; //Fy in above equations if ty = tx;

                    intersectionList.AddIntersection(cellIter, cellIter + new int2(delCell.x, 0), intersection, shapeIndex);

                    cellIter.x += delCell.x;
                    intersectionVertex.x += delCell.x * xRes;
                }

                // Hit horizontal line at ty first
                if (ty < tx)
                {
                    double2 intersection;
                    intersection.y = intersectionVertex.y;
                    intersection.x = (ty * delLine.x) + pointA.x; //Fx in above equations if tx = ty;

                    intersectionList.AddIntersection(cellIter, cellIter + new int2(0,delCell.y), intersection, shapeIndex);

                    cellIter.y += delCell.y;
                    intersectionVertex.y += delCell.y * -yRes;
                }
            }

            //// Calc direction of horizontal traversal
            //int direction;
            //direction = finalCell.x - originalCell.x;
            //direction = direction == 0 ? 0 : (direction > 0 ? 1 : -1);

            //// Calc relevant coordinates and directions
            //double2 gridVertex = calcGridVertex(originalCell);

            //// Calc line gradient
            //double2 gradient;
            //if (pointB.x - pointA.x == 0)
            //    gradient.x = (pointB.y - pointA.y);
            //else
            //    gradient.x = (pointB.y - pointA.y) / (pointB.x - pointA.x);

            //if (pointB.y - pointA.y == 0)
            //    gradient.y = (pointB.x - pointA.x);
            //else
            //    gradient.y = (pointB.x - pointA.x) / (pointB.y - pointA.y);

            //double offset = direction > 0 ? xRes : 0;

            //for (int i = 0; i < Math.Abs(finalCell.x - originalCell.x); ++i)
            //{
            //    double2 intersection;
            //    intersection.x = gridVertex.x + offset + (direction * i * xRes);
            //    intersection.y = pointA.y + gradient.x * (intersection.x - pointA.x);

            //    int yOffset = (int)Math.Floor(Math.Abs(intersection.y - pointA.y) / yRes) * direction;

            //    AddIntersection(intersection, shapeIndex);
            //    AddIntersection(intersection, new int2(originalCell.x + (direction * i), originalCell.y + yOffset));
            //    AddIntersection(intersection, new int2(originalCell.x + (direction * (i+1)), originalCell.y + yOffset));
            //}

            //// Calc direction of vertical traversal
            //direction = finalCell.y - originalCell.y;
            //direction = direction == 0 ? 0 : (direction > 0 ? 1 : -1);

            //// Calc relevant coordinates and directions
            //offset = direction > 0 ? yRes : 0;

            //for (int i = 0; i < Math.Abs(finalCell.y - originalCell.y); ++i)
            //{
            //    double2 intersection;
            //    intersection.y = gridVertex.y + offset + (direction * i * yRes);
            //    intersection.x = pointA.x + gradient.y * (intersection.y - pointA.y);

            //    int xOffset = (int)Math.Floor(Math.Abs(intersection.x - pointA.x) / xRes)*direction;

            //    AddIntersection(intersection, shapeIndex);
            //    AddIntersection(intersection, new int2(originalCell.x + xOffset, originalCell.y + (direction * i)));
            //    AddIntersection(intersection, new int2(originalCell.x + xOffset, originalCell.y + (direction * (i + 1))));
            //}
        }
    }
}
