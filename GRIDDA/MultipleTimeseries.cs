using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GRIDDA
{
    class MultipleTimeseries
    {
        private List<SingleTimeseries> mColumns;
        
        private TimePeriod mTimePeriod;
        private DateTime mFromDate { get { return mTimePeriod.from; } set { mTimePeriod.from = value; } }
        private DateTime mToDate { get { return mTimePeriod.to; } set { mTimePeriod.to = value; } }
        public int mTimeMultiplier;
        public TimeUnit mTimeUnit;
        private int mSteps;
        public string mMeasurementUnits;

        public List<String> mHeader;
        private String mFilepath;

        public static string NumberFormat = "0.######E+00  ";
        public static string LineBreak = "#----------------------------------------------------------------\n";
        public static string AuthorLine = "#-- Processed using GRIDDA by Phil Ward\n";

        public MultipleTimeseries()
        {
            mHeader = new List<string>();
            mColumns = new List<SingleTimeseries>();
            mSteps = 0;
            mMeasurementUnits = "";
            mFilepath = String.Empty;
            mTimeMultiplier = 1;
        }

        public MultipleTimeseries(String filepath)
        {
            mHeader = new List<string>();
            mColumns = new List<SingleTimeseries>();

            this.mFilepath = filepath;

            readFromFile(filepath);
            mTimeMultiplier = 1;
        }

        public MultipleTimeseries(DateTime from, DateTime to, TimeUnit units)
        {
            mSteps = CalcSteps(from, to, units);
            mHeader = new List<string>();
            mColumns = new List<SingleTimeseries>();
            mMeasurementUnits = "";

            mTimeUnit = units;
            mTimePeriod = new TimePeriod(from, to);
            mFilepath = String.Empty;
            mTimeMultiplier = 1;
        }

        public MultipleTimeseries(DateTime from, DateTime to, TimeUnit units, String measurementUnit, String filename)
        {
            mSteps = CalcSteps(from, to, units);
            mHeader = new List<string>();
            mColumns = new List<SingleTimeseries>();
            mMeasurementUnits = measurementUnit;
            mFilepath = filename;

            mTimeUnit = units;
            mTimePeriod = new TimePeriod(from, to);
            mTimeMultiplier = 1;
        }

        public MultipleTimeseries(DateTime from, int steps, TimeUnit units)
        {
            mHeader = new List<string>();
            mColumns = new List<SingleTimeseries>();
            mMeasurementUnits = "";
            
            mTimeUnit = units;
            mSteps = steps;
            mTimePeriod = new TimePeriod(from, incrementDateTime(from, mTimeUnit, getRows()));
            mFilepath = String.Empty;
            mTimeMultiplier = 1;
        }

        public MultipleTimeseries(MultipleTimeseries copy, bool copyData = true)
        {
            // copy values
            mTimePeriod = new TimePeriod(copy.getTimePeriod());
            mTimeUnit = copy.mTimeUnit;
            mSteps = copy.mSteps;
            mMeasurementUnits = copy.mMeasurementUnits;

            // copy lists
            mHeader = new List<string>(copy.mHeader);
            mColumns = new List<SingleTimeseries>();
            foreach (SingleTimeseries data in copy.mColumns)
            {
                if (copyData)
                {
                    mColumns.Add(new SingleTimeseries(data));
                }
            }
            mFilepath = copy.getFilepath();
            mTimeMultiplier = 1;
        }

        private void Initialise()
        {
            mColumns = new List<SingleTimeseries>();
            mHeader = new List<string>();
            mTimePeriod = new TimePeriod();
            mSteps = 0;
            mFilepath = String.Empty;
            mTimeMultiplier = 1;
        }

        public void AddColumn(SingleTimeseries newColumn)
        {
            if ((int)newColumn == mSteps)
            {
                mColumns.Add(newColumn);
            }
        }

        public void RemoveColumn(String siteId)
        {
            int index = this[siteId];

            if (index != -1)
            {
                mColumns.Remove(mColumns[index]);
            }
        }

        public void RemoveColumn(SingleTimeseries column)
        {
            if (mColumns.Contains(column))
            {
                mColumns.Remove(column);
            }
        }

        public void RemoveAllColumns()
        {
            mColumns.Clear();
        }

        public int getColumns()
        {
            return mColumns.Count;
        }

        public int getRows()
        {
            return mSteps;
        }

        public SingleTimeseries this[int column]
        {
            get 
            { 
                return mColumns[column]; 
            }
        }

        public float this[String siteId, int row]
        {
            get { return mColumns[findColumn(siteId)][row]; }
            set { mColumns[findColumn(siteId)][row] = value; }
        }

        public float this[int column, int row]
        {
            get { return mColumns[column][row]; }
            set { mColumns[column][row] = value; }
        }

        public List<float[]> getData()
        {
            List<float[]> data = new List<float[]>();

            foreach (SingleTimeseries column in mColumns)
            {
                data.Add(column);
            }

            return data;
        }

        public DateTime getStartDate()
        {
            return mTimePeriod.from;
        }

        public DateTime getEndDate()
        {
            return mTimePeriod.to;
        }

        public TimePeriod getTimePeriod()
        {
            return mTimePeriod;
        }

        public void setPeriod(TimePeriod period)
        {
            mTimePeriod = new TimePeriod(period);
            mSteps = Math.Max(0,DatePlus.CalcSteps(period, mTimeUnit));
        }

        public void setPeriod(DateTime from, DateTime to)
        {
            mTimePeriod = new TimePeriod(from, to);
            mSteps = Math.Max(0,DatePlus.CalcSteps(mTimePeriod, mTimeUnit));
        }

        public void setPeriod(DateTime from, DateTime to, TimeZoneInfo zone)
        {
            mTimePeriod = new TimePeriod(from, to, zone);
            mSteps = Math.Max(0,DatePlus.CalcSteps(mTimePeriod, mTimeUnit));
        }

        public int this[String siteId]
        {
            get
            {
                int index = -1;
                for (int i = 0; i < getColumns(); ++i)
                {
                    if (mColumns[i].SiteId == siteId)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
        }

        public void ReplaceSiteIds(String[] siteIds)
        {
            if (siteIds.Length == getColumns())
            {
                for (int i = 0; i < getColumns(); ++i)
                {
                    mColumns[i].SiteId = siteIds[i];
                }
            }
        }

        public String[] getSiteIds()
        {
            String[] siteIds = new String[getColumns()];

            for (int i = 0; i < getColumns(); ++i)
            {
                siteIds[i] = mColumns[i].SiteId;
            }

            return siteIds;
        }

        public String getSiteId(int column)
        {
            if (column < getColumns())
            {
                return mColumns[column].SiteId;
            }
            else
            {
                return String.Empty;
            }
        }

        public int findColumn(String header)
        {
            for (int i = 0; i < getColumns(); ++i)
            {
                if (mColumns[i].SiteId == header)
                {
                    return i;
                }
            }
            return -1;
        }
                
        public String getFilepath()
        {
            if (mFilepath == null)
                mFilepath = String.Empty;

            return mFilepath;
        }

        public String getFilename()
        {
            if (mFilepath == null)
                mFilepath = String.Empty;

            return Path.GetFileName(mFilepath);
        }

        public void setFilepath(String filepath)
        {
            mFilepath = filepath;
        }

        public void setFilename(String filename)
        {
            if (mFilepath == "")
            {
                mFilepath = filename;
            }
            else
            {
                mFilepath = Path.GetDirectoryName(mFilepath) + "//" + filename;
            }
        }

        public void saveToFile()
        {
            writeToFile(mFilepath);
        }

        public void saveToFile(String filepath)
        {
            this.mFilepath = filepath;
            writeToFile(filepath);
        }

        public void writeToFile(String filepath)
        {
            // Create folder
            if (filepath.IndexOf(Path.DirectorySeparatorChar) > 0)
            {
                String s = filepath.Substring(0, filepath.LastIndexOf(Path.DirectorySeparatorChar));
                if (!Directory.Exists(s))
                {
                    Directory.CreateDirectory(s);
                }
            }

            // Create File
            StreamWriter outFile = new StreamWriter(filepath);

            // Write out header
            writeFileHeaderToStream(outFile);

            // Write out column headers
            writeColumnHeadersToStream(outFile);

            // Write out data
            writeDataToStream(outFile);

            // Close File
            outFile.Close();

            // Record filepath
            mFilepath = filepath;
        }

        public void readFromFile(String filepath)
        {
            SingleTimeseries[] inData = new SingleTimeseries[0];

            // Open File
            StreamReader inFile;
            try { inFile = new StreamReader(filepath); }
            catch (Exception) { throw (new Exception("Could not open file")); }

            // Read header
            try { inData = getFileHeaderFromStream(inFile, inData); }
            catch (Exception) { throw (new Exception("Could not read file header.")); }

            // Read station nodes and names
            try { inData = getColumnHeadersFromStream(inFile, inData); }
            catch (Exception) { throw (new Exception("Could not read column headers")); }

            // Read data
            try { inData = getDataFromStream(inFile, inData); }
            catch (Exception) { throw (new Exception("Could not read file data")); }

            // Close File
            try { inFile.Close(); }
            catch (Exception) { throw (new Exception("Could not close file")); }

            mColumns.AddRange(inData);
        }

        public void writeFileHeaderToStream(StreamWriter outFile)
        {
            // Time units
            outFile.Write(String.Format("{0,-21}", "\"" + mTimeUnit.toString() + "\"") + "!Time Units\n");

            // Time steps
            outFile.Write(String.Format("{0,-21}", getRows()) + "!Time steps\n");

            // No. stations
            outFile.Write(String.Format("{0,-21}", getColumns()) + "!No. of sites\n");

            // From period
            outFile.Write(String.Format("{0,-21}", "" + mFromDate.ToString("mm HH dd MM yyyy")) + "!from (data period)\n");

            // To Period
            DateTime toDate = DatePlus.CalcFromIncrement(mFromDate, mTimeUnit, mSteps-1);
            outFile.Write(String.Format("{0,-21}", "" + toDate.ToString("mm HH dd MM yyyy")) + "!to (data period)\n");

            // header break
            outFile.Write(AuthorLine);
        }

        public void writeColumnHeadersToStream(StreamWriter outFile)
        {
            // Write header
            outFile.Write("Step");

            foreach (SingleTimeseries s in mColumns)
            {
                outFile.Write(","+s.SiteId);
            }

            outFile.Write("\n");
        }

        public void writeDataToStream(StreamWriter outFile)
        {
            // Write out data
            for (int i = 0; i < getRows(); i += 1)
            {
                // Write step
                outFile.Write(i+1);

                // Write data
                for (int j = 0; j < getColumns(); j++)
                {
                    outFile.Write(","+mColumns[j][i].ToString(NumberFormat));
                }

                // End line
                outFile.Write("\n");
            }
        }

        public SingleTimeseries[] getFileHeaderFromStream(StreamReader inFile, SingleTimeseries[] inData)
        {
            // clear old header if it exists
            mHeader.Clear();

            // temporary header
            String tempInLine = "";

            // read header into temp list for processing
            // stop if reached end of file
            while (inFile.Peek() != -1)
            {
                // read line
                tempInLine = inFile.ReadLine();

                // add line to list for processing
                mHeader.Add(tempInLine);

                // check if we have reached end of header
                if (tempInLine.StartsWith("#--"))//.Substring(0, 2).Equals("#-"))
                {
                    break;
                }
            }

            if (inFile.Peek() != -1)
            {
                // time units
                tempInLine = inFile.ReadLine();
                mTimeUnit = TimeUnitExtensions.FromString(tempInLine.Trim(new char[3] { '"', '\t', ' ' }).Substring(0, 2).ToUpper());
                mHeader.Add(tempInLine);

                // time steps
                tempInLine = inFile.ReadLine();
                mSteps = Int32.Parse(tempInLine.Substring(0, tempInLine.IndexOf('!')).Trim().Trim('"'));
                mHeader.Add(tempInLine);

                // measurement units
                tempInLine = inFile.ReadLine();
                mMeasurementUnits = tempInLine.Substring(0, tempInLine.IndexOf('!')).Trim().Trim('"');
                mHeader.Add(tempInLine);

                // no of stations
                tempInLine = inFile.ReadLine();
                inData = new SingleTimeseries[Int32.Parse(tempInLine.Substring(0, tempInLine.IndexOf('!')).Trim().Trim('"'))];
                mHeader.Add(tempInLine);

                // repetitive data
                tempInLine = inFile.ReadLine();
                // not stored, left as is and rewritten to output file
                mHeader.Add(tempInLine);

                // from date
                tempInLine = inFile.ReadLine(); 
                //mFromDate = DateTime.ParseExact(tempInLine.Substring(0,"mm hh dd MM yyyy".Length), "mm hh dd MM yyyy", null);  
                String[] tempLine = tempInLine.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                // convert date strings into integers
                int mm = Convert.ToInt32(tempLine[0]);
                int hh = Convert.ToInt32(tempLine[1]);
                int dd = Convert.ToInt32(tempLine[2]);
                int mo = Convert.ToInt32(tempLine[3]);
                int yyyy = Convert.ToInt32(tempLine[4]);
                // create date object from integers
                mFromDate = new DateTime(yyyy, mo, dd, hh, mm, 0);
                mHeader.Add(tempInLine);
            }

            // read remainder of header
            while (inFile.Peek() != -1)
            {
                // read line
                tempInLine = inFile.ReadLine();

                // add line to list for processing
                mHeader.Add(tempInLine);

                // check if we have reached end of header
                if (tempInLine.StartsWith("#--"))//.Substring(0, 2).Equals("#-"))
                {
                    break;
                }
            }

            if (inFile.Peek() == -1)
            {
                throw (new Exception("End of file reached before end of header"));
            }

            return inData;
        }

        public SingleTimeseries[] getColumnHeadersFromStream(StreamReader inFile, SingleTimeseries[] inData)
        {
            // read line from file till start of data
            String tempLine = "";
            while (!tempLine.Trim(new char[2] { '\t', ' ' }).ToUpper().StartsWith("NODE") && !tempLine.Trim(new char[2] { '\t', ' ' }).ToUpper().StartsWith("STEP") && inFile.Peek() != -1)
            {
                tempLine = inFile.ReadLine();
            }

            if (inFile.Peek() != -1)
            {
                // split into array
                String[] tempLineArray = tempLine.Trim(new char[2] { '\t', ' ' }).Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                // check if station name line
                if (tempLineArray[0].ToUpper().Equals("STEP") && tempLineArray.Length - 1 == inData.Length)
                {
                    // read each token and add to array
                    for (int i = 1; i < tempLineArray.Length; i++)
                    {
                        if (inData[i - 1] != null)
                        {
                            inData[i - 1].SiteId = tempLineArray[i];
                        }
                        else
                        {
                            inData[i - 1] = new SingleTimeseries(tempLineArray[i]);
                        }
                    }
                }
            }
            return inData;
        }

        public SingleTimeseries[] getDataFromStream(StreamReader inFile, SingleTimeseries[] inData)
        {
            foreach (SingleTimeseries column in inData)
            {
                // create float list for each site
                column.Allocate(getRows());
            }

            // read remainder of file
            for (int i = 0; inFile.Peek() != -1; i++)
            {
                // read line from file
                String[] tempLine = inFile.ReadLine().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                // ignore comments and length
                if (i == getRows())
                {
                    return inData;
                }

                // only parse line if it has the correct number of tokens
                if (tempLine.Length == inData.Count() + 1)
                {
                    // read each column
                    for (int j = 0; j < inData.Count(); j++)
                    {
                        float number;
                        // Check if number is valid
                        if (Single.TryParse(tempLine[j + 1], out number))
                        {
                            inData[j][i] = number; // yes, store
                        }
                        else
                        {
                            inData[j][i] = -9999.9f; // no, replace with -9999.9
                        }
                    }
                }
            }

            // set to date
            mToDate = incrementDateTime(mFromDate, mTimeUnit, getRows());

            return inData;
        }

        public void fillGaps(List<MultipleTimeseries> datasets)
        {
            // list of other data sets for same station name
            List<MultipleTimeseries> otherSources = new List<MultipleTimeseries>();
            List<int> otherIndices = new List<int>();

            // for each station
            for (int j = 0; j < getColumns(); j++)
            {
                // check other datasets provider
                foreach (MultipleTimeseries d in datasets)
                {
                    // check each station in other data sets
                    for (int k = 0; k < d.getColumns(); k++)
                    {
                        // if same scale
                        if (d.mTimeUnit == mTimeUnit)
                        {
                            // if same station
                            if (d.getSiteId(k).ToUpper().Contains(getSiteId(j).ToUpper()))
                            {
                                // add to list of other data sets for this station to compare with
                                otherSources.Add(d);
                                otherIndices.Add(k);
                            }
                        }
                    }
                }

                // for each data point for this station
                for (int i = 0; i < getRows(); i++)
                {
                    // if value is missing
                    if (mColumns[j][i] < 0)
                    {
                        // check each other data set for this station
                        for (int k = 0; k < otherSources.Count; k++)
                        {
                            MultipleTimeseries otherSource = otherSources[k];
                            DateTime index = incrementDateTime(mFromDate, mTimeUnit, i);

                            // if the other data set contains this date
                            if (otherSource.getStartDate() <= index && otherSource.getEndDate() >= index)
                            {
                                // calculate the position of this date in other data set
                                int offset = CalcSteps(otherSource.getStartDate(), index, mTimeUnit);

                                // if the other data set contains a valid value for this date
                                if (otherSource.mColumns[otherIndices[k]][offset] > 0)
                                {
                                    // add the value to this data set
                                    mColumns[j][i] = otherSource.mColumns[otherIndices[k]][offset];

                                    // stop looking for values (assume datasets is ordered by priority)
                                    break;
                                }
                            }
                        }                         
                    }
                }
            }
        }

        private static MultipleTimeseries consolidateData(MultipleTimeseries inputData, TimePeriod outputPeriod, TimeUnit outputTimeUnits, bool aggregate, int tolerance, TimeZoneInfo inputZone, bool discardIncomplete)
        {
            // Create utc output period
            TimePeriod outputUtcPeriod = new TimePeriod();
            outputUtcPeriod.from = TimeZoneInfo.ConvertTimeToUtc(outputPeriod.from, outputPeriod.zone);
            outputUtcPeriod.to = TimeZoneInfo.ConvertTimeToUtc(outputPeriod.to, outputPeriod.zone);
            outputUtcPeriod.zone = TimeZoneInfo.Utc;

            // Create utc input period
            TimePeriod inputUtcPeriod = new TimePeriod();
            inputUtcPeriod.from = TimeZoneInfo.ConvertTimeToUtc(inputData.getStartDate(), inputZone);
            inputUtcPeriod.to = TimeZoneInfo.ConvertTimeToUtc(inputData.getEndDate(), inputZone);
            inputUtcPeriod.zone = TimeZoneInfo.Utc;

            // Set up new data set
            MultipleTimeseries outputData = new MultipleTimeseries(inputData, false);
            outputData.mTimeUnit = outputTimeUnits;
            outputData.setPeriod(outputPeriod);

            // Calculate offset and length of input data to use
            int inputOffset = CalcSteps(inputUtcPeriod.from, outputUtcPeriod.from, inputData.mTimeUnit);
            int outputOffset = CalcSteps(inputUtcPeriod.from, outputUtcPeriod.from, outputData.mTimeUnit);
            int inputLength = inputData.getRows();// - Math.Max(inputOffset, 0);

            // Allocate memory for data
            for (int i = 0; i < outputData.getColumns(); i++)
            {
                outputData[i].Allocate(outputData.getRows());
            }

            // Assign data values
            for (int j = 0; j < outputData.getColumns(); j++)
            {
                // Set up index to traverse data structures
                // index refers to position in input data
                // index - offset refers to position in output data
                int outputIndex = outputOffset;
                int inputIndex = Math.Max(inputOffset, 0);
                DateTime inputDateIndex = inputUtcPeriod.from;
                DateTime outputDateIndex = outputUtcPeriod.from;

                // Pad empty start space with zeros
                // only required if offset is negative
                for (; outputIndex < 0; outputIndex++)
                {
                    outputData[j,outputIndex - outputOffset] = -9999.9f;
                }

                // Transfer data
                inputDateIndex = CalcUtcFromLocalIncrement(inputDateIndex, inputData.mTimeUnit, inputIndex, inputZone);
                outputDateIndex = CalcUtcFromLocalIncrement(outputDateIndex, outputData.mTimeUnit, outputIndex - outputOffset, outputPeriod.zone);

                for (; inputIndex < inputLength && outputIndex - outputOffset < outputData.getRows(); outputIndex++)
                {
                    // calc number of input points expected for this output point
                    int pointsToConsolidate = CalcSteps(inputDateIndex, CalcUtcFromLocalIncrement(outputDateIndex, outputData.mTimeUnit, 1, outputPeriod.zone), inputData.mTimeUnit);
                    int pointsFound = 0;
                    int pointsMissing = 0;
                    for (int i = 0; i < pointsToConsolidate && i + inputIndex < inputLength; i++)
                    {
                        if (inputData[j,inputIndex + i] >= 0)
                        {
                            outputData[j,outputIndex - outputOffset] += inputData[j,inputIndex + i]; // accumulate input data if not missing
                            pointsFound++;
                        }
                        else
                        {
                            pointsMissing++; // count missing data points
                        }
                        if (pointsMissing > tolerance) // if over tolerance set to missing
                        {
                            outputData[j,outputIndex - outputOffset] = -9999.9f;
                            break;
                        }
                    }

                    if (pointsFound + pointsMissing != pointsToConsolidate) // for partially complete days at start and end of period
                    {
                        outputData[j,outputIndex - outputOffset] = -9999.9f;
                    }


                    if (!aggregate && outputData[j,outputIndex - outputOffset] >= 0) // if not missing and not aggregating, divide for average
                    {
                        outputData[j,outputIndex - outputOffset] /= Math.Max((pointsFound),1); // avoid divide by zero
                    }
                    else if (outputData[j,outputIndex - outputOffset] >= 0 && pointsMissing > 0) // if aggregating, compensate for missing values
                    {
                        outputData[j, outputIndex - outputOffset] *= pointsToConsolidate / (float)(pointsFound);
                    }

                    // DEBUG
                    //outputData[j,outputIndex - outputOffset] = outputIndex;

                    // increment all index pointers
                    inputIndex += pointsToConsolidate;
                    inputDateIndex = CalcUtcFromLocalIncrement(inputDateIndex, inputData.mTimeUnit, pointsToConsolidate, inputZone);
                    outputDateIndex = CalcUtcFromLocalIncrement(outputDateIndex, outputData.mTimeUnit, 1, outputPeriod.zone);
                }

                // Pad empty end space with zeros
                for (; outputIndex - outputOffset < outputData.getRows(); outputIndex++)
                {
                    outputData[j,outputIndex - outputOffset] = -9999.9f;
                }
            }

            // Return new data set
            return outputData;
        }

        // merge datasets with a as priority
        public static MultipleTimeseries operator+(MultipleTimeseries priority, MultipleTimeseries supplementary)
        {
            if (priority.mTimeUnit != supplementary.mTimeUnit)
            {
                return null;
            }

            // copy a
            MultipleTimeseries output = new MultipleTimeseries(priority);

            // calc offset for adding b
            int offset = CalcSteps(output.getStartDate(), supplementary.getStartDate(), output.mTimeUnit);
            int length = supplementary.getRows() + Math.Min(0,CalcSteps(supplementary.getEndDate(), output.getEndDate(), output.mTimeUnit));

            // for each station in b
            for (int j = 0; j < supplementary.getColumns(); j++)
            {
                // check if a already has this station
                int index = output[supplementary.getSiteId(j)];
                if (index == -1)
                {
                    // if station doesn't exist add it
                    SingleTimeseries newData = new SingleTimeseries(supplementary.getSiteId(j));
                    newData.Allocate(output.getRows());

                    int i;
                    // add any initial missing values
                    for (i = 0; i < offset; i++)
                    {
                        newData[i] = -9999.9f;
                    }
                    /// add data from source b
                    for (; i < length; i++)
                    {
                        newData[i] = supplementary[j,i - offset];
                    }
                    // add any final missing values
                    for (; i < output.getRows(); i++)
                    {
                        newData[i] = -9999.9f;
                    }
                    // add new dataset to output
                    output.AddColumn(newData);
                }
                else
                {
                    for (int i = 0; i < output.getRows(); i++)
                    {
                        // if within an overlapping record period of b
                        if (i >= offset && i < length)
                        {
                            // if value in a is missing
                            if (output[index, i] < 0)
                            {
                                // replace with value in b
                                output[index, i] = supplementary[j, i - offset];
                            }
                        }
                    }
                }
            }

            return output;
        }

        // diff datasets
        public static MultipleTimeseries operator -(MultipleTimeseries lhs, MultipleTimeseries rhs)
        {
            if (lhs.mTimeUnit != rhs.mTimeUnit)
            {
                return null;
            }

            // copy lhs
            MultipleTimeseries output = new MultipleTimeseries(lhs);

            // calc offset for adding rhs
            int offset = CalcSteps(output.getStartDate(), rhs.getStartDate(), output.mTimeUnit);
            int length = rhs.getRows() + Math.Min(0, CalcSteps(rhs.getEndDate(), output.getEndDate(), output.mTimeUnit));

            // for each station in rhs
            for (int j = 0; j < rhs.getColumns(); j++)
            {
                // check if lhs already has this station
                int index = output[rhs.getSiteId(j)];
                if (index != -1)
                {
                    for (int i = 0; i < output.getRows(); i++)
                    {
                        // if within an overlapping record period of rhs
                        if (i >= offset && i < length)
                        {
                            // if neither value is missing
                            if (output[index, i] >= 0 && rhs[j, i - offset] >= 0)
                            {
                                // replace with value in b
                                output[index, i] -= rhs[j, i - offset];
                            }
                            else // otherwise set to missing
                            {
                                output[index, i] = -9999.9f;
                            }
                        }
                        else // set to missing
                        {
                            output[index, i] = -9999.9f;
                        }
                    }
                }
            }

            return output;
        }

        public static MultipleTimeseries Scale(MultipleTimeseries dataset, float factor)
        {
            return dataset *= factor;
        }

        // multiple all values by factor
        public static MultipleTimeseries operator *(MultipleTimeseries dataset, float factor)
        {
            MultipleTimeseries output = new MultipleTimeseries(dataset);

            for (int j = 0; j < output.getColumns(); j++)
            {
                for (int i = 0; i < output.getRows(); i++)
                {
                    output[j,i] *= factor;
                }
            }
            return output;
        }

        // divide all values by factor
        public static MultipleTimeseries operator /(MultipleTimeseries dataset, float factor)
        {
            MultipleTimeseries output = new MultipleTimeseries(dataset);
            if (factor != 0)
            {
                float rcpFactor = 1 / factor;
                for (int j = 0; j < output.getColumns(); j++)
                {
                    for (int i = 0; i < output.getRows(); i++)
                    {
                        output[j, i] /= factor;
                    }
                }
            }
            return output;
        }

        // all values = initial values ^ factor
        public static MultipleTimeseries operator ^(MultipleTimeseries dataset, float factor)
        {
            MultipleTimeseries output = new MultipleTimeseries(dataset);

            for (int j = 0; j < output.getColumns(); j++)
            {
                for (int i = 0; i < output.getRows(); i++)
                {
                    output[j,i] = (float)Math.Pow(output[j,i], factor);
                }
            }
            return output;
        }

        public static void MultiplyByDaysInMonth(MultipleTimeseries dataset)
        {
            for (int j = 0; j < dataset.getColumns(); j++)
            {
                // Set up index to traverse data structures
                DateTime outputDateIndex = dataset.getStartDate();
                for (int outputIndex = 0; outputIndex < dataset.getRows(); outputIndex++)
                {
                    // calc number of input points expected for this output point
                    dataset[j,outputIndex] *= DateTime.DaysInMonth(outputDateIndex.Year, outputDateIndex.Month);
                    outputDateIndex = CalcUtcFromLocalIncrement(outputDateIndex, dataset.mTimeUnit, 1, TimeZoneInfo.Utc);
                }
            }
        }

        public static void MultiplyByDaysInMonth(MultipleTimeseries dataset, TimeZoneInfo timezone)
        {
            for (int j = 0; j < dataset.getColumns(); j++)
            {
                // Set up index to traverse data structures
                DateTime outputDateIndex = dataset.getStartDate();
                for (int outputIndex = 0; outputIndex < dataset.getRows(); outputIndex++)
                {
                    // calc number of input points expected for this output point
                    dataset[j,outputIndex] *= DateTime.DaysInMonth(outputDateIndex.Year, outputDateIndex.Month);
                    outputIndex++;
                    outputDateIndex = CalcUtcFromLocalIncrement(outputDateIndex, dataset.mTimeUnit, 1, timezone);
                }
            }
        }

        public static void DivideByDaysInMonth(MultipleTimeseries dataset)
        {
            for (int j = 0; j < dataset.getColumns(); j++)
            {
                // Set up index to traverse data structures
                DateTime outputDateIndex = dataset.getStartDate();
                for (int outputIndex = 0; outputIndex < dataset.getRows(); outputIndex++)
                {
                    // calc number of input points expected for this output point
                    dataset[j,outputIndex] /= DateTime.DaysInMonth(outputDateIndex.Year, outputDateIndex.Month);
                    outputIndex++;
                    outputDateIndex = CalcUtcFromLocalIncrement(outputDateIndex, dataset.mTimeUnit, 1, TimeZoneInfo.Utc);
                }
            }
        }

        public static void DivideByDaysInMonth(MultipleTimeseries dataset, TimeZoneInfo timezone)
        {
            for (int j = 0; j < dataset.getColumns(); j++)
            {
                // Set up index to traverse data structures
                DateTime outputDateIndex = dataset.getStartDate();
                for (int outputIndex = 0; outputIndex < dataset.getRows(); outputIndex++)
                {
                    // calc number of input points expected for this output point
                    dataset[j,outputIndex] /= DateTime.DaysInMonth(outputDateIndex.Year, outputDateIndex.Month);
                    outputIndex++;
                    outputDateIndex = CalcUtcFromLocalIncrement(outputDateIndex, dataset.mTimeUnit, 1, timezone);
                }
            }
        }

        public enum ConsolidateStationsConflictMethod
        {
            Max = 0,
            Min = 1,
            LeftToRight = 2,
            RightToLeft = 3,
            Product = 4,
            Sum = 5,
            Average = 6,
        };

        public static DateTime incrementDateTime(DateTime date, TimeUnit timeUnits, int increment)
        {
            switch (timeUnits)
            {
                case (TimeUnit.da):
                    return date.AddDays(increment);
                case (TimeUnit.hr):
                    return date.AddHours(increment);
                case (TimeUnit.mo):
                    return date.AddMonths(increment);
            }
            return date.Date;
        }

        protected static long CalcUtcTicks(DateTime fromDate, TimeUnit timeUnit, long quantity)
        {
            switch (timeUnit)
            {
                case (TimeUnit.hr):
                    quantity *= TimeSpan.TicksPerHour;
                    break;
                case (TimeUnit.da):
                    quantity *= TimeSpan.TicksPerDay;
                    break;
                case (TimeUnit.mo):
                    quantity *= DateTime.DaysInMonth(fromDate.Year, fromDate.Month) * TimeSpan.TicksPerDay;
                    break;
            }

            return quantity;
        }

        protected static long CalcLocalTicks(DateTime utcFromDate, TimeUnit timeUnit, TimeZoneInfo timeZone, int quantity)
        {
            TimeSpan span = TimeSpan.Zero;

            switch (timeUnit)
            {
                case (TimeUnit.hr):
                    span = TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddHours(quantity), timeZone) - utcFromDate;
                    break;
                case (TimeUnit.da):
                    span = TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddDays(quantity), timeZone) - utcFromDate;
                    break;
                case (TimeUnit.mo):
                    for (int i = 0; i < Math.Abs(quantity); i++)
                    {
                        if (quantity > 0)
                        {
                            // requires days in current month to progress in time
                            span += TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddDays(DateTime.DaysInMonth(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).Year, TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).Month)), timeZone) - utcFromDate;
                        }
                        else
                        {
                            // requires days in previous month to regress in time
                            span -= TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddDays(DateTime.DaysInMonth(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddMonths(-1).Year, TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddMonths(-1).Month)), timeZone) - utcFromDate;
                        }
                    }
                    break;
            }

            return span.Ticks;
        }

        protected static int CalcSteps(DateTime from, DateTime to, TimeUnit units)
        {
            int steps = 0;

            switch (units)
            {
                case (TimeUnit.hr):
                    steps = Convert.ToInt32((to - from).TotalHours);
                    break;
                case (TimeUnit.da):
                    steps = Convert.ToInt32((to - from).TotalDays);
                    break;
                case (TimeUnit.mo):
                    steps = Convert.ToInt32((to.Month - from.Month) + (12 * (to.Year - from.Year)));
                    break;
            }

            return steps;
        }

        protected static int CalcStepsDown(DateTime from, DateTime to, TimeUnit units)
        {
            int steps = 0;

            switch (units)
            {
                case (TimeUnit.hr):
                    steps = Convert.ToInt32(Math.Truncate((to - from).TotalHours));
                    break;
                case (TimeUnit.da):
                    steps = Convert.ToInt32(Math.Truncate((to - from).TotalDays));
                    break;
                case (TimeUnit.mo):
                    steps = Convert.ToInt32(Math.Truncate((to.Month - from.Month) + (12.0f * (to.Year - from.Year))));
                    break;
            }

            return steps;
        }

        protected static int CalcStepsUp(DateTime from, DateTime to, TimeUnit units)
        {
            int steps = 0;

            switch (units)
            {
                case (TimeUnit.hr):
                    steps = Convert.ToInt32(Math.Round((to - from).TotalHours, MidpointRounding.AwayFromZero));
                    break;
                case (TimeUnit.da):
                    steps = Convert.ToInt32(Math.Round((to - from).TotalDays, MidpointRounding.AwayFromZero));
                    break;
                case (TimeUnit.mo):
                    steps = Convert.ToInt32(Math.Round((to.Month - from.Month) + (12.0f * (to.Year - from.Year)), MidpointRounding.AwayFromZero));
                    break;
            }

            return steps;
        }

        protected static int CalcSteps(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo zone)
        {
            return CalcSteps(from, to, units, zone, zone);
        }

        protected static int CalcStepsDown(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo zone)
        {
            return CalcStepsDown(from, to, units, zone, zone);
        }

        protected static int CalcStepsUp(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo zone)
        {
            return CalcStepsUp(from, to, units, zone, zone);
        }

        protected static int CalcSteps(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo fromZone, TimeZoneInfo toZone)
        {
            to = TimeZoneInfo.ConvertTimeToUtc(to, toZone);
            from = TimeZoneInfo.ConvertTimeToUtc(from, fromZone);
            return CalcSteps(from, to, units);
        }

        protected static int CalcStepsDown(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo fromZone, TimeZoneInfo toZone)
        {
            to = TimeZoneInfo.ConvertTimeToUtc(to, toZone);
            from = TimeZoneInfo.ConvertTimeToUtc(from, fromZone);
            return CalcStepsDown(from, to, units);
        }

        protected static int CalcStepsUp(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo fromZone, TimeZoneInfo toZone)
        {
            to = TimeZoneInfo.ConvertTimeToUtc(to, toZone);
            from = TimeZoneInfo.ConvertTimeToUtc(from, fromZone);
            return CalcStepsUp(from, to, units);
        }

        public static int CalcSteps(TimePeriod intersectUtcDataPeriod, TimeUnit timeUnit)
        {
            return CalcSteps(intersectUtcDataPeriod.from, intersectUtcDataPeriod.to, timeUnit, intersectUtcDataPeriod.zone);
        }

        protected static int CalcStepsDown(TimePeriod intersectUtcDataPeriod, TimeUnit timeUnit)
        {
            return CalcStepsDown(intersectUtcDataPeriod.from, intersectUtcDataPeriod.to, timeUnit, intersectUtcDataPeriod.zone);
        }

        protected static int CalcStepsUp(TimePeriod intersectUtcDataPeriod, TimeUnit timeUnit)
        {
            return CalcStepsUp(intersectUtcDataPeriod.from, intersectUtcDataPeriod.to, timeUnit, intersectUtcDataPeriod.zone);
        }

        protected static int CalcIncrementStepsFromUtc(DateTime utcFromDate, TimeUnit incrementTimeUnit, TimeUnit stepsTimeUnit, int increment, TimeZoneInfo timeZone)
        {
            DateTime localFromDate = TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone);
            TimeSpan span = TimeSpan.Zero;

            switch (incrementTimeUnit)
            {
                case (TimeUnit.hr):
                    span = TimeZoneInfo.ConvertTimeToUtc(localFromDate.AddHours(increment), timeZone) - TimeZoneInfo.ConvertTimeToUtc(localFromDate, timeZone);
                    break;
                case (TimeUnit.da):
                    span = TimeZoneInfo.ConvertTimeToUtc(localFromDate.AddDays(increment), timeZone) - TimeZoneInfo.ConvertTimeToUtc(localFromDate, timeZone);
                    break;
                case (TimeUnit.mo):
                    span = TimeZoneInfo.ConvertTimeToUtc(localFromDate.AddMonths(increment), timeZone) - TimeZoneInfo.ConvertTimeToUtc(localFromDate, timeZone);
                    break;
            }

            switch (stepsTimeUnit)
            {
                case (TimeUnit.hr):
                    return Convert.ToInt32(span.TotalHours);
                case (TimeUnit.da):
                    return Convert.ToInt32(span.TotalDays);
                case (TimeUnit.mo):
                    return Convert.ToInt32(TimeZoneInfo.ConvertTimeToUtc(localFromDate.AddMonths(increment), timeZone).Month - TimeZoneInfo.ConvertTimeToUtc(localFromDate, timeZone).Month + (12 * (TimeZoneInfo.ConvertTimeToUtc(localFromDate.AddMonths(increment), timeZone).Year - TimeZoneInfo.ConvertTimeToUtc(localFromDate, timeZone).Year)));
            }

            return 0;
        }

        public static DateTime CalcUtcFromLocalIncrement(DateTime utcFromDate, TimeUnit incrementTimeUnit, int increment, TimeZoneInfo timeZone)
        {
            switch (incrementTimeUnit)
            {
                case (TimeUnit.hr):
                    return TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddHours(increment), timeZone);
                case (TimeUnit.da):
                    return TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddDays(increment), timeZone);
                case (TimeUnit.mo):
                    return TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utcFromDate, timeZone).AddMonths(increment), timeZone);
            }
            return utcFromDate;
        }

        public static TimeUnit CalcTimeUnitFromInterval(DateTime point1, DateTime point2)
        {
            if ((point2 - point1).Days > 1)
            {
                return TimeUnit.mo;
            }
            else if ((point2 - point1).Hours > 1)
            {
                return TimeUnit.da;
            }
            else
            {
                return TimeUnit.hr;
            }
        }

        public MultipleTimeseries[] getStatistics()
        {
            if (mTimeUnit == TimeUnit.mo)
            {
                MultipleTimeseries[] outData = new MultipleTimeseries[getColumns()];

                int dataPoints = 12;
                int offset = getStartDate().Month-1;

                for (int i = 0; i < outData.Length; ++i)
                {
                    // Set header
                    outData[i] = new MultipleTimeseries(new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), mTimeUnit, "", getSiteId(i));
                    outData[i].mMeasurementUnits = mMeasurementUnits;

                    // Add columns
                    outData[i].AddColumn(new SingleTimeseries("Count", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("Mean", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("Min", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("Max", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("10%", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("25%", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("50%", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("75%", dataPoints));
                    outData[i].AddColumn(new SingleTimeseries("90%", dataPoints));
                    
                    // Compute sum and count
                    for (int j = 0; j < getRows(); ++j)
                    {
                        outData[i][0][(offset + j) % dataPoints] += 1;
                        outData[i][1][(offset + j) % dataPoints] += this[i][j];
                    }

                    // Convert sum to mean
                    for (int j = 0; j < dataPoints; ++j)
                    {
                        outData[i][1][j] /= outData[i][0][j];
                    }

                    // Compute percentages
                    SingleTimeseries[] sortedData = new SingleTimeseries[dataPoints];
                    for (int j = 0; j < dataPoints; j++)
                    {
                        sortedData[j] = new SingleTimeseries();
                        sortedData[j].Allocate((int)Math.Ceiling((float)getRows() / (float)dataPoints),-9999.9f);
                    }

                    for (int j = 0; j < getRows(); ++j)
                    {
                        sortedData[(j + offset) % dataPoints][(j + offset) / dataPoints] = this[i][j];
                    }

                    for (int j = 0; j < dataPoints; ++j)
                    {
                        float[] orderedPoints = sortedData[j].getSorted();

                        outData[i][2][j] = sortedData[j].getMin(-9999.9f);
                        outData[i][3][j] = sortedData[j].getMax();
                        outData[i][4][j] = orderedPoints[(int)(orderedPoints.Length * 0.1)];
                        outData[i][5][j] = orderedPoints[(int)(orderedPoints.Length * 0.25)];
                        outData[i][6][j] = orderedPoints[(int)(orderedPoints.Length * 0.5)];
                        outData[i][7][j] = orderedPoints[(int)(orderedPoints.Length * 0.75)];
                        outData[i][8][j] = orderedPoints[(int)(orderedPoints.Length * 0.9)];
                    }


                }
                return outData;
            }
            else
            {
                throw new Exception("Only implemented for monthly data");
            }
        }

        public static DateTime operator +(MultipleTimeseries data, int a)
        {
            switch (data.mTimeUnit)
            {
                case TimeUnit.hr:
                    return data.getStartDate().AddHours(a);
                case TimeUnit.da:
                    return data.getStartDate().AddDays(a);
                case TimeUnit.mo:
                    return data.getStartDate().AddMonths(a);
            }

            return data.getStartDate();
        }

        public static int operator /(MultipleTimeseries data, TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.hr:
                    return (int)(data.getEndDate() - data.getStartDate()).TotalHours;
                case TimeUnit.da:
                    return (int)(data.getEndDate() - data.getStartDate()).TotalDays;
                case TimeUnit.mo:
                    int years = (data.getEndDate().Year - data.getStartDate().Year);
                    return (years * 12) + (data.getEndDate().Month - data.getStartDate().Month);
            }

            return 0;
        }

        public float getMax()
        {
            float max = 0;

            foreach (SingleTimeseries data in mColumns)
            {
                max = data.getMax() > max ? data.getMax() : max;
            }

            return max;
        }
    }
}
