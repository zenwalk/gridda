using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRIDDA
{
    class IntersectionList
    {
        public Intersection[] intersections;
        public int Start;
        public int Length;

        public static int BUFFER_SIZE = 8;

        public IntersectionList()
        {
            intersections = new Intersection[1];
            Start = 0;
            Length = 0;
        }

        public Intersection this[int i]
        {
            get
            {
                if (i >= Length)
                {
                    throw new IndexOutOfRangeException("Intersection list has length "+Length+". Requested index: " + i + ".");
                }
                return intersections[i];
            }
            set
            {
                if (intersections == null)
                {
                    intersections = new Intersection[1];
                    Length = 0;
                }
                if (i >= intersections.Length)
                {
                    Intersection[] newVertices = new Intersection[Length + BUFFER_SIZE];
                    Array.Copy(intersections, newVertices, Length);
                    intersections = newVertices;
                }
                if (i >= Length)
                {
                    Length++;
                }
                intersections[i] = value;
            }
        }

        public void Clear()
        {
            intersections = new Intersection[1];
            Length = 0;
        }

        public void Finish()
        {
            // Set start of first intersection as end of the last
            if (Length != 0)
            {
                intersections[Start].startCell = intersections[Length - 1].startCell;
                intersections[Start].startIndice = intersections[Length - 1].startIndice;
                intersections[Start].startIntersection = intersections[Length - 1].startIntersection;
                intersections[Start].currentCell = intersections[Length - 1].currentCell;

                // Prepare for new shape
                Length--;
                Start = Length;
            }
        }

        public void AddIntersection(int2 previousCell, int2 nextCell, double2 intersection, int index)
        {
            // Check last intersection agrees with this intersection (last one finished where this one begins)
            if (Length != Start && intersections[Length-1].currentCell != previousCell)
            {
                throw new Exception("Cell mismatch");
            }

            if (Length == Start)
            {
                // Conclude last intersection
                this[Start] = new Intersection();
                intersections[Start].SetFinal(nextCell, intersection, index);

                // Start next intersection
                this[Start+1] = new Intersection();
                intersections[Start + 1].SetStart(previousCell, intersection, index);
                intersections[Start + 1].SetCurrent(nextCell);
            }
            else
            {
                // Conclude last intersection
                intersections[Length - 1].SetFinal(nextCell, intersection, index);

                // Start next intersection
                this[Length] = new Intersection();
                intersections[Length - 1].SetStart(previousCell, intersection, index);
                intersections[Length - 1].SetCurrent(nextCell);
            }
        }

        public Intersection[] getIntersections(int2 cell)
        {
            int count = 0;

            for (int i = 0; i < Length; ++i)
            {
                if (intersections[i].currentCell == cell)
                {
                    count++;
                }
            }

            Intersection[] cellList = new Intersection[count];
            int j = 0;

            for (int i = 0; i < Length; ++i)
            {
                if (intersections[i].currentCell == cell)
                {
                    cellList[j++] = intersections[i];
                }
            }

            return cellList;
        }
    }
}
