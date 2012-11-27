using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GRIDDA
{
    class Polygon
    {
        public double2[] vertices;
        public MBR mbr;
        public int[] indices;

        public Polygon(int length = 0)
        {
            vertices = new double2[length];
            mbr = new MBR();
            indices = new int[length];
        }

        public Polygon(BinaryReader shpFile)
        {
            mbr.xMin = (decimal)shpFile.ReadDouble();
            mbr.yMin = (decimal)shpFile.ReadDouble();
            mbr.xMax = (decimal)shpFile.ReadDouble();
            mbr.yMax = (decimal)shpFile.ReadDouble();

            indices = new int[shpFile.ReadInt32()];
            vertices = new double2[shpFile.ReadInt32()];

            for (int i = 0; i < indices.Length; ++i)
            {
                indices[i] = shpFile.ReadInt32();
            }

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i].x = (decimal)shpFile.ReadDouble();
                vertices[i].y = (decimal)shpFile.ReadDouble();
            }
        }
    }
}
