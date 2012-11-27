using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace GRIDDA
{
    class GriddedDataset
    {
        Dictionary<DateTime, GriddedDataFile> mFilenameLookup;
        int mDateString = 8;
        TimeUnit mTimeUnit;

        public GriddedDataset(String[] fltFiles, GriddedDataDetails grid, int dateString, TimeUnit timeUnit)
        {
            mTimeUnit = timeUnit;
            mDateString = dateString;
            mFilenameLookup = new Dictionary<DateTime, GriddedDataFile>();

            foreach (String fltFile in fltFiles)
            {
                DateTime fromDate = stripPrecision(computeDate(fltFile, dateString), mTimeUnit);

                if (mFilenameLookup.ContainsKey(fromDate))
                {
                    String oldFile = Path.GetFileNameWithoutExtension(mFilenameLookup[fromDate].getFilename());
                    throw new Exception("Duplicate entry for date " + fromDate + ". Filename1 = " + oldFile + " Filename2 = " + fltFile);
                }
                else
                {
                    mFilenameLookup.Add(fromDate, new GriddedDataFile(grid, fltFile));
                }
            }
        }

        public GriddedDataset(String[] fltFiles, GriddedDataDetails grid, int dateString, TimeUnit timeUnit, bool binaryFormat = true)
        {
            mTimeUnit = timeUnit;
            mDateString = dateString;
            mFilenameLookup = new Dictionary<DateTime, GriddedDataFile>();
            if (binaryFormat)
            {
                foreach (String fltFile in fltFiles)
                {
                    DateTime fromDate = stripPrecision(computeDate(fltFile, dateString), mTimeUnit);

                    if (mFilenameLookup.ContainsKey(fromDate))
                    {
                        String oldFile = Path.GetFileNameWithoutExtension(mFilenameLookup[fromDate].getFilename());
                        throw new Exception("Duplicate entry for date " + fromDate + ". Filename1 = " + oldFile + " Filename2 = " + fltFile);
                    }
                    else
                    {
                        mFilenameLookup.Add(fromDate, new GriddedDataFile(grid, fltFile));
                    }
                }
            }
            else
            {
                foreach (String textFolder in fltFiles)
                {
                    DateTime fromDate = stripPrecision(computeDate(textFolder, dateString), mTimeUnit);

                    GriddedDataFile newData = new GriddedDataFile(textFolder);

                    if (mFilenameLookup.ContainsKey(fromDate))
                    {
                        String oldFile = Path.GetFileNameWithoutExtension(mFilenameLookup[fromDate].getFilename());
                        throw new Exception("Duplicate entry for date " + fromDate + ". Filename1 = " + oldFile + " Filename2 = " + textFolder);
                    }
                    else
                    {
                        mFilenameLookup.Add(fromDate, newData);
                    }
                }
            }
        }

        public DateTime getFromDate()
        {
            return mFilenameLookup.Keys.Min<DateTime>();
        }

        public DateTime getToDate()
        {
            return DatePlus.CalcFromIncrement(mFilenameLookup.Keys.Max<DateTime>(), mTimeUnit, 1);
        }

        private DateTime computeDate(String fltFile, int N)
        {
            int offset = Regex.Match(fltFile, @"\d{" + N + "}").Index;
            if (offset != -1)
            {
                switch (N)
                {
                    case 8:
                        return new DateTime(int.Parse(fltFile.Substring(offset, 4)), int.Parse(fltFile.Substring(offset + 4, 2)), int.Parse(fltFile.Substring(offset + 6, 2)));
                    case 6:
                        return new DateTime(int.Parse(fltFile.Substring(offset, 4)), int.Parse(fltFile.Substring(offset + 4, 2)), 1);
                    case 4:
                        return new DateTime(int.Parse(fltFile.Substring(offset, 4)), 1, 1);
                    default:
                        break;
                }
            }
            return DateTime.MaxValue;
        }

        //private DateTime computeDate(String fltFile)
        //{
        //    String s = Path.GetFileNameWithoutExtension(fltFile);
        //    s = s.Substring(mPrefix.Length);
        //    return new DateTime(int.Parse(s.Substring(0, 4)), int.Parse(s.Substring(4, 2)), int.Parse(s.Substring(6, 2)));
        //}

        public DateTime[] getRange(DateTime from, DateTime to)
        {
            return Array.FindAll<DateTime>(mFilenameLookup.Keys.ToArray<DateTime>(), element => element >= from && element <= to);
        }

        public DateTime[] getRange(TimePeriod period)
        {
            return Array.FindAll<DateTime>(mFilenameLookup.Keys.ToArray<DateTime>(), element => element >= period.from && element <= period.to);
        }

        public String[] getFiles(DateTime from, DateTime to)
        {
            // Get keys
            DateTime[] dates = getRange(stripPrecision(from, mTimeUnit), stripPrecision(to, mTimeUnit));

            // Make string list
            List<String> files = new List<String>();

            // Add filename to string list for data associated with each date
            foreach (DateTime date in dates)
            {
                files.Add(mFilenameLookup[date].getFilename());
            }

            // Return list as array
            return files.ToArray();
        }

        //public float[] getTimeseries(DateTime from, DateTime to, float longitude, float latitude)
        //{
        //    DateTime[] lookupDates = getRange(stripPrecision(from, mTimeUnit), stripPrecision(to, mTimeUnit));
        //    Array.Sort(lookupDates);

        //    float[] data = new float[lookupDates.Length];

        //    for (int i = 0; i < lookupDates.Length; ++i)
        //    {
        //        data[i] = mFilenameLookup[lookupDates[i]].getData(longitude, latitude);
        //    }

        //    return data;
        //}

        //public float[][,] getTimeseries(DateTime from, DateTime to, float longitude1, float latitude1, float longitude2, float latitude2)
        //{
        //    DateTime[] lookupDates = getRange(stripPrecision(from, mTimeUnit), stripPrecision(to, mTimeUnit));
        //    Array.Sort(lookupDates);

        //    float[][,] data = new float[lookupDates.Length][,];

        //    for (int i = 0; i < lookupDates.Length; ++i)
        //    {
        //        data[i] = mFilenameLookup[lookupDates[i]].getDataRange(longitude1, latitude1, longitude2, latitude2);
        //    }

        //    return data;
        //}
        public List<float[]> getTimeseries(ExtractionPoint[] points, TimeUnit unit)
        {
            return getTimeseries(getFromDate(), getToDate(), points, unit);
        }

        public List<float[]> getTimeseries(DateTime from, DateTime to, ExtractionPoint[] points, TimeUnit unit)
        {
            TimePeriod period = new TimePeriod();
            period.from = from;
            period.to = to;
            period.zone = TimeZoneInfo.Utc;
            period = stripPrecision(period, unit);

            int steps = MultipleTimeseries.CalcSteps(period, unit);

            List<float[]> dataList = new List<float[]>();

            for (int j = 0; j < points.Length; ++j)
            {
                float[] data = new float[steps];
                DateTime iter = from;

                for (int i = 0; i < steps; ++i)
                {
                    if (mFilenameLookup.ContainsKey(iter))
                    {
                        data[i] = mFilenameLookup[iter].getData(points[j].longitude, points[j].latitude);
                    }
                    else
                    {
                        data[i] = -9999.9f;
                    }
                    iter = MultipleTimeseries.incrementDateTime(iter, unit, 1);
                }

                dataList.Add(data);
            }

            return dataList;
        }

        //public List<float[][,]> getTimeseries(DateTime from, DateTime to, float[] longitude1, float[] latitude1, float[] longitude2, float[] latitude2)
        //{
        //    DateTime[] lookupDates = getRange(stripPrecision(from, mTimeUnit), stripPrecision(to, mTimeUnit));
        //    Array.Sort(lookupDates);

        //    List<float[][,]> dataList = new List<float[][,]>();

        //    if (longitude1.Length == latitude1.Length && longitude1.Length == latitude2.Length && longitude1.Length == longitude2.Length)
        //    {
        //        for (int j = 0; j < longitude1.Length; ++j)
        //        {
        //            float[][,] data = new float[lookupDates.Length][,];

        //            for (int i = 0; i < lookupDates.Length; ++i)
        //            {
        //                data[i] = mFilenameLookup[lookupDates[i]].getDataRange(longitude1[j], latitude1[j],longitude2[j], latitude2[j]);
        //            }

        //            dataList.Add(data);
        //        }
        //    }

        //    return dataList;
        //}

        private static TimePeriod stripPrecision(TimePeriod period, TimeUnit unit)
            {
            switch (unit) // remove precision in dates based on unit
                {
                case TimeUnit.hr:
                    break;
                case TimeUnit.da:
                    period.from = period.from.AddHours(-period.from.Hour);
                    period.to = period.to.AddHours(-period.to.Hour);
                    break;
                case TimeUnit.mo:
                    period.from = period.from.AddDays(1-period.from.Day);
                    period.from = period.from.AddHours(-period.from.Hour);
                    period.to = period.to.AddDays(1-period.to.Day);
                    period.to = period.to.AddHours(-period.to.Hour);
                    break;
            }
            return period;
        }

        private static DateTime stripPrecision(DateTime time, TimeUnit unit)
                    {
            switch (unit) // remove precision in dates based on unit
            {
                case TimeUnit.hr:
                    break;
                case TimeUnit.da:
                    time = time.AddHours(-time.Hour);
                    break;
                case TimeUnit.mo:
                    time = time.AddDays(1-time.Day);
                    time = time.AddHours(-time.Hour);
                    break;
                    }
            return time;
                }
            }
}
