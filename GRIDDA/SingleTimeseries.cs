using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRIDDA
{
    class SingleTimeseries
    {
        // Header info
        private String mSiteId;

        public String SiteId { get { return mSiteId; } set { mSiteId = value; } }

        // Data
        private float[] mData;

        public SingleTimeseries()
        {
            Initialise();
        }

        public SingleTimeseries(SingleTimeseries copy)
        {
            Initialise();

            // Header info
            SiteId = copy.SiteId;

            // Data
            mData = new float[copy.mData.Length];
            Array.Copy(copy.mData, mData, copy.mData.Length);
        }

        public SingleTimeseries(String siteId)
        {
            Initialise();

            SiteId = siteId;
        }

        public SingleTimeseries(String siteId, int length)
        {
            Initialise();

            SiteId = siteId;
            Allocate(length);
        }

        public SingleTimeseries(String siteId, float[] data)
        {
            Initialise();

            SiteId = siteId;
            mData = new float[data.Length];
            Array.Copy(data, mData, data.Length);
        }

        private void Initialise()
        {
            // Default values
            SiteId = "Undefined";
            mData = new float[0];
        }

        public void Allocate(int steps)
        {
            mData = new float[steps];
            Array.Clear(mData, 0, steps);
        }

        public void Allocate(int steps, float initialValue)
        {
            mData = new float[steps];

            for (int i = 0; i < steps; ++i)
            {
                mData[i] = initialValue;
            }
        }

        public float this[int x]
        {
            get 
            {
                if (x < mData.Length)
                {
                    return mData[x];
                }
                else
                {
                    return -9999.9f;
                }
            }
            set 
            {
                if (x < mData.Length)
                {
                    mData[x] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException("Data column has length " + mData.Length + " attempted to write to index " + x);
                }
            }
        }

        public static implicit operator float[](SingleTimeseries data)
        {
            return data.mData;
        }

        public static explicit operator int(SingleTimeseries data)
        {
            return data.mData.Length;
        }

        public void Trim(int offset, int length)
        {
            // create new data
            float[] newData = new float[length];

            // pad before with -9999.9
            for (int j = 0; j < offset; j++)
            {
                newData[j] = -9999.9f;
            }

            // copy existing data
            if (length - offset >= mData.Length)
            {
                mData.CopyTo(newData, offset);
            }
            else
            {

                for (int j = offset; j < offset + mData.Length && j < length; j++)
                {
                    newData[j] = mData[j - offset];
                }
            }

            // pad after with -9999.9
            for (int j = offset + mData.Length; j < length; j++)
            {
                newData[j] = -9999.9f;
            }

            // Replace existing data with new data
            mData = newData;
        }

        public float getMin()
        {
            return mData.Min();
        }

        public float getMax()
        {
            return mData.Max();
        }

        public float getMin(float missingValue)
        {
            float replaceValue = mData.Max();
            return mData.Min(x => x != missingValue ? x : replaceValue);
        }

        public float getMin(int start, int end)
        {
            float min = float.MaxValue;
            
            for (int i = start; i < end && i < mData.Length; ++i)
            {
                min = min < mData[i] ? min : mData[i];
            }

            return min;
        }

        public float getMax(int start, int end)
        {
            float max = float.MinValue;

            for (int i = start; i < end && i < mData.Length; ++i)
            {
                max = max > mData[i] ? max : mData[i];
            }

            return max;
        }

        public float getMaxPositive()
        {
            return mData.Sum(x => x > 0 ? x : 0);
        }

        public float[] getSorted()
        {
            float[] data = new float[mData.Count(delegate(float i) { return i != -9999.9f; })];
            data = Array.FindAll(mData, delegate(float i) { return i != -9999.9f; });
            Array.Sort(data);
            return data;
        }

        public float[] getSorted(int start, int end)
        {
            float[] data = new float[end - start];
            Array.Copy(mData, start, data, 0, data.Length);
            Array.Sort(data);
            return data;
        }
    }
}
