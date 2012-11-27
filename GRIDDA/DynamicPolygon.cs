using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRIDDA
{
    class DynamicPolygon:Polygon
    {
        public int Length;

        public DynamicPolygon() : base()
        {
        }

        public double2 this[int i]
        {
            get
            {
                return vertices[i];
            }
            set
            {
                if (vertices == null)
                {
                    vertices = new double2[0];
                    Length = 0;
                }
                if (i >= vertices.Length)
                {
                    double2[] newVertices = new double2[Length + 8];
                    Array.Copy(vertices, newVertices, Length);
                    vertices = newVertices;
                }
                if (i >= Length)
                {
                    Length++;
                }
                vertices[i] = value;
            }
        }

        public void insertVertex(double2 vertex, int index)
        {
            for (int i = Length; i > index; --i)
            {
                this[i] = this[i - 1];
            }
            this[index] = vertex;
        }

        public void addVertex(double2 vertex)
        {
            this[Length] = vertex;
        }

        public void addVertices(double2[] vertices)
        {
            addVertices(vertices, 0, vertices.Length);
        }

        public void addVertices(double2[] vertices, int index)
        {
            addVertices(vertices, index, vertices.Length - index);
        }

        public void addVertices(double2[] vertices, int index, int length)
        {
            int spaceRequired = length - (this.vertices.Length - Length);

            if (spaceRequired > 0)
            {
                double2[] newVertices = new double2[this.vertices.Length + spaceRequired + 8];
                Array.Copy(this.vertices, newVertices, Length);
                this.vertices = newVertices;
            }

            Array.Copy(vertices, index, this.vertices, Length, length);

            Length += length;
        }

        public void addGridVertices(double2 gridCorner, double2 gridRes)
        {
            this[Length] = gridCorner;
            this[Length] = gridCorner + new double2(gridRes.x,0);
            this[Length] = gridCorner + new double2(gridRes.x, -gridRes.y);
            this[Length] = gridCorner + new double2(0, -gridRes.y);
        }

        public decimal calcIntegral()
        {
            decimal integral = 0;

            for (int i = 0; i < Length; ++i)
            {
                integral += (vertices[i].x * vertices[(i + 1) % Length].y);
                integral -= (vertices[i].y * vertices[(i + 1) % Length].x);
            }

            integral *= 0.5m;

            return Math.Abs(integral);
        }

        public bool checkConsecutive(double2 a, double2 b)
        {
            for (int i = 0; i < Length; ++i)
            {
                if (this.vertices[i] == a && this.vertices[(i + 1) % Length] == b)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
