using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GRIDDA
{
    enum GraphMarker
    {
        Circle,
        Square
    };

    struct GraphItem
    {
        public int mStationIndex;
        public String mName;

        public Color mMarkerFillColor;
        public Color mMarkerOutlineColor;
        public float mMarkerSize;
        public float mMarkerOutlineSize;
        public GraphMarker mMarkerShape;

        public bool mTrendLine;
        public Color mTrendLineColor;
        public float mTrendLineSize;

        public GraphItem(int stationIndex, String name, Random rand)
        {
            mStationIndex = stationIndex;
            mName = name;

            mMarkerFillColor = genRandom(rand);
            mMarkerOutlineColor = invert(mMarkerFillColor);
            mMarkerSize = 4.0f;
            mMarkerOutlineSize = 0.5f;
            mMarkerShape = GraphMarker.Circle;

            mTrendLineColor = Color.Black;
            mTrendLine = false;
            mTrendLineSize = 1.0f;
        }

        public GraphItem(int stationIndex, String name)
        {
            mStationIndex = stationIndex;
            mName = name;

            mMarkerFillColor = genRandom();
            mMarkerOutlineColor = invert(mMarkerFillColor);
            mMarkerSize = 4.0f;
            mMarkerOutlineSize = 1.0f;
            mMarkerShape = GraphMarker.Circle;

            mTrendLineColor = Color.Black;
            mTrendLine = false;
            mTrendLineSize = 1.0f;
        }

        public GraphItem(GraphItem graphItem, int stationIndex, String name)
        {
            mStationIndex = stationIndex;
            mName = name;

            mMarkerFillColor = graphItem.mMarkerFillColor;
            mMarkerOutlineColor = graphItem.mMarkerOutlineColor;
            mMarkerSize = graphItem.mMarkerSize;
            mMarkerOutlineSize = graphItem.mMarkerOutlineSize;
            mMarkerShape = graphItem.mMarkerShape;

            mTrendLineColor = graphItem.mTrendLineColor;
            mTrendLine = graphItem.mTrendLine;
            mTrendLineSize = graphItem.mTrendLineSize;
        }

        public override String ToString()
        {
            return mName;
        }

        public static Color genRandom()
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            return Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
        }
        public static Color genRandom(Random rand)
        {
            return Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
        }

        public static Color invert(Color color)
        {
            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(GraphItem))
            {
                return false;
            }

            return Equals((GraphItem)obj);
        }

        public bool Equals(GraphItem p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (mStationIndex == p.mStationIndex) && (mName == p.mName);
        }

        public override int GetHashCode()
        {
            return mStationIndex.GetHashCode();
        }
    }

    struct ExtractionPoint
    {
        public String siteName;
        public decimal longitude;
        public decimal latitude;
        public decimal weight;

        public ExtractionPoint(String siteName, decimal longitude, decimal latitude, decimal weight)
        {
            this.siteName = siteName;
            this.longitude = longitude;
            this.latitude = latitude;
            this.weight = weight;
        }

        public ExtractionPoint(String siteName, float longitude, float latitude, float weight)
        {
            this.siteName = siteName;
            this.longitude = (decimal)longitude;
            this.latitude = (decimal)latitude;
            this.weight = (decimal)weight;
        }
    }

    struct GriddedDataDetails
    {
        public decimal lowerLeftLatitude;
        public decimal lowerLeftLongitude;
        public decimal horizontalGridSize;
        public decimal verticalGridSize;
        public int columns;
        public int rows;

        public GriddedDataDetails(decimal lowerLeftLongitude, decimal lowerLeftLatitude, decimal horizontalGridSize, decimal verticalGridSize, int columns, int rows)
        {
            this.lowerLeftLongitude = lowerLeftLongitude;
            this.lowerLeftLatitude = lowerLeftLatitude;
            this.horizontalGridSize = horizontalGridSize;
            this.verticalGridSize = verticalGridSize;
            this.columns = columns;
            this.rows = rows;
        }

        public GriddedDataDetails(float lowerLeftLongitude, float lowerLeftLatitude, float horizontalGridSize, float verticalGridSize, int columns, int rows)
        {
            this.lowerLeftLongitude = (decimal)lowerLeftLongitude;
            this.lowerLeftLatitude = (decimal)lowerLeftLatitude;
            this.horizontalGridSize = (decimal)horizontalGridSize;
            this.verticalGridSize = (decimal)verticalGridSize;
            this.columns = columns;
            this.rows = rows;
        }
    }

    struct double2
    {
        public const decimal Tolerance = 0;//0.0000001;
        public decimal x;
        public decimal y;

        public double2(decimal x, decimal y)
        {
            this.x = x;
            this.y = y;
        }
        public static double2 operator -(double2 lhs, double2 rhs)
        {
            double2 diff;

            diff.x = lhs.x - rhs.x;
            diff.y = lhs.y - rhs.y;

            return diff;
        }
        public static double2 operator +(double2 lhs, double2 rhs)
        {
            double2 diff;

            diff.x = lhs.x + rhs.x;
            diff.y = lhs.y + rhs.y;

            return diff;
        }
        public static double2 operator /(double2 lhs, decimal rhs)
        {
            double2 diff;

            diff.x = lhs.x / rhs;
            diff.y = lhs.y / rhs;

            return diff;
        }
        public static double2 operator *(double2 lhs, decimal rhs)
        {
            double2 diff;

            diff.x = lhs.x * rhs;
            diff.y = lhs.y * rhs;

            return diff;
        }

        public static bool operator ==(double2 lhs, double2 rhs)
        {

            return Math.Abs(lhs.x - rhs.x) <= Tolerance && Math.Abs(lhs.y - rhs.y) <= Tolerance;
        }

        public static bool operator !=(double2 lhs, double2 rhs)
        {
            return Math.Abs(lhs.x - rhs.x) > Tolerance || Math.Abs(lhs.y - rhs.y) > Tolerance;
        }

        public bool Equals(double2 obj)
        {
            return Math.Abs(x - obj.x) <= Tolerance && Math.Abs(y - obj.y) <= Tolerance;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(double2))
            {
                return false;
            }

            return Equals((double2)obj);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode();
        }

        public decimal length()
        {
            return (decimal)Math.Sqrt((double)(x * x + y * y));
        }

        public decimal lengthSqr()
        {
            return x * x + y * y;
        }

        public bool isNan()
        {
            return false;
            //return decimal.IsNaN(x) || decimal.IsNaN(y);
        }

        public static double2 NULL()
        {
            return new double2(0, 0);
            //return new double2(decimal.NaN, decimal.NaN);
        }

        public static bool Equals(decimal lhs, decimal rhs)
        {
            return Math.Abs(lhs - rhs) <= Tolerance;
        }

        public static bool NotEquals(decimal lhs, decimal rhs)
        {
            return Math.Abs(lhs - rhs) > Tolerance;
        }

        public static bool Equals(double2 lhs, double2 rhs)
        {
            return lhs == rhs;
        }

        public static bool NotEquals(double2 lhs, double2 rhs)
        {
            return lhs != rhs;
        }
    }

    struct int2
    {
        public int x;
        public int y;

        public int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static int2 operator -(int2 lhs, int2 rhs)
        {
            int2 diff;

            diff.x = lhs.x - rhs.x;
            diff.y = lhs.y - rhs.y;

            return diff;
        }
        public static int2 operator +(int2 lhs, int2 rhs)
        {
            int2 diff;

            diff.x = lhs.x + rhs.x;
            diff.y = lhs.y + rhs.y;

            return diff;
        }
        public static int2 operator /(int2 lhs, double rhs)
        {
            int2 diff;

            diff.x = (int)(lhs.x / rhs);
            diff.y = (int)(lhs.y / rhs);

            return diff;
        }
        public static int2 operator *(int2 lhs, double rhs)
        {
            int2 diff;

            diff.x = (int)(lhs.x * rhs);
            diff.y = (int)(lhs.y * rhs);

            return diff;
        }
        public static bool operator ==(int2 lhs, int2 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(int2 lhs, int2 rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(int2))
            {
                return false;
            }

            return Equals((int2)obj);
        }

        public bool Equals(int2 p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return x == p.x && y == p.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode();
        }

        public static implicit operator int2(Point p)
        {
            return new int2(p.X, p.Y);
        }

        public static implicit operator Point(int2 i)
        {
            return new Point(i.x, i.y);
        }
    }

    struct MBR
    {
        public decimal xMin;
        public decimal xMax;
        public decimal yMin;
        public decimal yMax;

        public MBR(decimal xMin, decimal xMax, decimal yMin, decimal yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }
    }

    struct Intersection
    {
        public int2 startCell;
        public int2 currentCell;
        public int2 finalCell;

        public double2 startIntersection;
        public double2 finalIntersection;

        public int startIndice;
        public int finalIndice;

        public Intersection(int2 startCell, int2 currentCell, int2 finalCell, double2 startIntersection, double2 finalIntersection, int startIndice, int finalIndice)
        {
            this.startCell = startCell;
            this.currentCell = currentCell;
            this.finalCell = finalCell;

            this.startIntersection = startIntersection;
            this.finalIntersection = finalIntersection;

            this.startIndice = startIndice;
            this.finalIndice = finalIndice;
        }

        public void SetStart(int2 startCell, double2 startIntersection, int startIndice)
        {
            this.startCell = startCell;
            this.startIntersection = startIntersection;
            this.startIndice = startIndice;
        }

        public void SetFinal(int2 finalCell, double2 finalIntersection, int finalIndice)
        {
            this.finalCell = finalCell;
            this.finalIntersection = finalIntersection;
            this.finalIndice = finalIndice;
        }

        public void SetCurrent(int2 currentCell)
        {
            this.currentCell = currentCell;
        }
    }

    struct Shape
    {
        //Polygon shape;
        public Polygon polygon;

        // Intersecting Grid
        public DynamicGrid intersectingGrid;
        public DynamicPolygon[,] intersectingPolygons;
        public decimal[,] integralValue;
    };

    enum FileSelection { Single, Multiple, Folder };

    struct GenericCSV
    {
        public Dictionary<String, int> index;
        public List<String> columns;
        public FileSelection fileSelection;
        public String folderPath;
        public String filePath;
        public String[] filePaths;
        public bool hasHeader;
        public DataSet dataSet;
    }

    struct TimePeriod
    {
        public DateTime from;
        public DateTime to;
        public TimeZoneInfo zone;

        public TimePeriod(DateTime from, DateTime to)
        {
            this.from = from;
            this.to = to;
            this.zone = TimeZoneInfo.Utc;

            this.from = new DateTime(from.Ticks, DateTimeKind.Unspecified);
            this.to = new DateTime(to.Ticks, DateTimeKind.Unspecified);
        }
        public TimePeriod(DateTime from, DateTime to, TimeZoneInfo zone)
        {
            this.from = from;
            this.to = to;
            this.zone = zone;

            this.from = new DateTime(from.Ticks, DateTimeKind.Unspecified);
            this.to = new DateTime(to.Ticks, DateTimeKind.Unspecified);
        }
        public TimePeriod(TimePeriod copy)
        {
            this.from = copy.from;
            this.to = copy.to;
            this.zone = copy.zone;

            this.from = new DateTime(from.Ticks, DateTimeKind.Unspecified);
            this.to = new DateTime(to.Ticks, DateTimeKind.Unspecified);
        }
    }

    enum TimeUnit
    {
        hr = 0,
        da = 1,
        mo = 2,
    };

    static class TimeUnitExtensions
    {
        // Output strings, aligned with enum indicies
        static String[] toStringLong = { "hourly", "daily", "monthly" };
        static String[] toStringShort = { "hr", "da", "mo" };

        // Input strings
        static String[] hourlyStrings = { "hr", "ho", "hourly" };
        static String[] dailyStrings = { "da", "daily" };
        static String[] monthlyStrings = { "mo", "monthly", "mn" };

        public static TimeUnit FromString(String timeUnit)
        {
            if (hourlyStrings.Contains(timeUnit.ToLower()))
            {
                return TimeUnit.hr;
            }
            else if (dailyStrings.Contains(timeUnit.ToLower()))
            {
                return TimeUnit.da;
            }
            else if (monthlyStrings.Contains(timeUnit.ToLower()))
            {
                return TimeUnit.mo;
            }
            else
            {
                return TimeUnit.hr;
            }
        }

        public static string toString(this TimeUnit timeUnit)
        {
            return toStringShort[(int)timeUnit];
        }

        public static string Description(this TimeUnit timeUnit)
        {
            return toStringLong[(int)timeUnit];
        }

        public static string[] getTypes()
        {
            return toStringLong;
        }
    }
}