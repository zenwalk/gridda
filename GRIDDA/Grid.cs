using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRIDDA
{
    class Grid
    {
        public MBR mbr;
        public decimal xRes;
        public decimal yRes;

        public Grid(Grid grid)
        {
            this.mbr = grid.mbr;
            this.xRes = grid.xRes;
            this.yRes = grid.yRes;
        }

        public Grid(MBR mbr, decimal xRes, decimal yRes)
        {
            this.mbr = mbr;
            this.xRes = xRes;
            this.yRes = yRes;
        }

        public Grid(decimal xMin, decimal yMin, decimal xMax, decimal yMax, decimal xRes, decimal yRes)
        {
            this.xRes = xRes;
            this.yRes = yRes;

            this.mbr.xMin = xMin;
            this.mbr.yMin = yMin;
            this.mbr.xMax = xMax;
            this.mbr.yMax = yMax;
        }

        public Grid(GriddedDataDetails gridDetails)
        {
            this.xRes = gridDetails.horizontalGridSize;
            this.yRes = gridDetails.verticalGridSize;

            this.mbr.xMin = gridDetails.lowerLeftLatitude;
            this.mbr.yMin = gridDetails.lowerLeftLongitude;
            this.mbr.xMax = this.mbr.xMin + (this.xRes * (decimal)gridDetails.columns);
            this.mbr.yMax = this.mbr.yMin + (this.yRes * (decimal)gridDetails.rows);
        }

        public double2 this[int2 i]
        {
            get
            {
                return calcGridVertex(i);
            }
        }

        public double2 this[int i, int j]
        {
            get
            {
                return calcGridVertex(i, j);
            }
        }

        public double2 calcGridVertex(int2 cell)
        {
            return calcGridVertex(cell.x, cell.y);
        }

        public double2 calcGridVertex(int column, int row)
        {
            double2 vertex = new double2();

            vertex.x = mbr.xMin + (column * xRes);
            vertex.y = mbr.yMax - (row * yRes);

            return vertex;
        }

        public int2 calcGridCell(double2 vertex)
        {
            int column = (int)Math.Floor((vertex.x - mbr.xMin) / xRes);
            int row = (int)Math.Floor((mbr.yMax - vertex.y) / yRes);

            return new int2(column, row);
        }
    }
}
