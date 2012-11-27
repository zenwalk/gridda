using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRIDDA
{
    class DatePlus
    {
        public DateTime mDate;
        public TimeUnit mUnit;
        public TimeZoneInfo mZone;

        public DatePlus()
        {
            mZone = TimeZoneInfo.Utc;
            mUnit = TimeUnit.hr;
            mDate = DateTime.MinValue;
        }

        public DatePlus(DatePlus copy)
        {
            mDate = new DateTime(copy.mDate.Ticks);
            mUnit = copy.mUnit;
            mZone = copy.mZone;
        }

        public DatePlus(DateTime date, TimeUnit unit)
        {
            mDate = new DateTime(date.Ticks);
            mUnit = unit;
            mZone = TimeZoneInfo.Utc;
        }

        public DatePlus(DateTime date, TimeUnit unit, TimeZoneInfo zone)
        {
            mDate = new DateTime(date.Ticks);
            mUnit = unit;
            mZone = zone;
        }

        public DatePlus(MultipleTimeseries data, bool from = true)
        {
            if (from)
                mDate = mDate = new DateTime(data.getStartDate().Ticks);
            else
                mDate = mDate = new DateTime(data.getEndDate().Ticks);

            mUnit = data.mTimeUnit;
            mZone = TimeZoneInfo.Utc;
        }

        public void AddHours(int increment)
        {
            mDate = mDate.AddHours(increment);
        }

        public void AddDays(int increment)
        {
            mDate = mDate.AddDays(increment);
        }

        public void AddMonths(int increment)
        {
            mDate = mDate.AddMonths(increment);
        }

        public static DatePlus operator ++(DatePlus date)
        {
            switch (date.mUnit)
            {
                case (TimeUnit.da):
                    date.AddDays(1);
                    break;
                case (TimeUnit.hr):
                    date.AddHours(1);
                    break;
                case (TimeUnit.mo):
                    date.AddMonths(1);
                    break;
            }
            return date;
        }

        public static DatePlus operator --(DatePlus date)
        {
            switch (date.mUnit)
            {
                case (TimeUnit.da):
                    date.AddDays(-1);
                    break;
                case (TimeUnit.hr):
                    date.AddHours(-1);
                    break;
                case (TimeUnit.mo):
                    date.AddMonths(-1);
                    break;
            }
            return date;
        }

        public static bool operator <(DatePlus a, DatePlus b)
        {
            return TimeZoneInfo.ConvertTimeToUtc(a.mDate, a.mZone) < TimeZoneInfo.ConvertTimeToUtc(b.mDate, b.mZone);
        }

        public static bool operator >(DatePlus a, DatePlus b)
        {
            return TimeZoneInfo.ConvertTimeToUtc(a.mDate, a.mZone) > TimeZoneInfo.ConvertTimeToUtc(b.mDate, b.mZone);
        }

        public static bool operator <=(DatePlus a, DatePlus b)
        {
            return TimeZoneInfo.ConvertTimeToUtc(a.mDate, a.mZone) <= TimeZoneInfo.ConvertTimeToUtc(b.mDate, b.mZone);
        }

        public static bool operator >=(DatePlus a, DatePlus b)
        {
            return TimeZoneInfo.ConvertTimeToUtc(a.mDate, a.mZone) >= TimeZoneInfo.ConvertTimeToUtc(b.mDate, b.mZone);
        }

        public static bool operator ==(DatePlus a, DatePlus b)
        {
            return TimeZoneInfo.ConvertTimeToUtc(a.mDate, a.mZone) == TimeZoneInfo.ConvertTimeToUtc(b.mDate, b.mZone);
        }

        public static bool operator !=(DatePlus a, DatePlus b)
        {
            return TimeZoneInfo.ConvertTimeToUtc(a.mDate, a.mZone) != TimeZoneInfo.ConvertTimeToUtc(b.mDate, b.mZone);
        }

        public static DatePlus operator +(DatePlus a, int add)
        {
            DatePlus output = new DatePlus(a);

            switch (output.mUnit)
            {
                case (TimeUnit.da):
                    output.AddDays(add);
                    break;
                case (TimeUnit.hr):
                    output.AddHours(add);
                    break;
                case (TimeUnit.mo):
                    output.AddMonths(add);
                    break;
            }

            return output;
        }

        public static int operator -(DatePlus a, DatePlus b)
        {
            switch (a.mUnit)
            {
                case (TimeUnit.hr):
                    return Convert.ToInt32((a.mDate - b.mDate).TotalHours);
                case (TimeUnit.da):
                    return Convert.ToInt32((a.mDate - b.mDate).TotalDays);
                case (TimeUnit.mo):
                    return Convert.ToInt32((a.mDate.Month - b.mDate.Month) + (12 * (a.mDate.Year - b.mDate.Year)));
            }

            return 0;
        }

        public static int operator -(DatePlus a, DateTime b)
        {
            switch (a.mUnit)
            {
                case (TimeUnit.hr):
                    return Convert.ToInt32((a.mDate - b).TotalHours);
                case (TimeUnit.da):
                    return Convert.ToInt32((a.mDate - b).TotalDays);
                case (TimeUnit.mo):
                    return Convert.ToInt32((a.mDate.Month - b.Month) + (12 * (a.mDate.Year - b.Year)));
            }

            return 0;
        }

        public static DatePlus operator -(DatePlus a, int subtract)
        {
            DatePlus output = new DatePlus(a);

            switch (output.mUnit)
            {
                case (TimeUnit.da):
                    output.AddDays(subtract);
                    break;
                case (TimeUnit.hr):
                    output.AddHours(subtract);
                    break;
                case (TimeUnit.mo):
                    output.AddMonths(subtract);
                    break;
            }

            return output;
        }

        public static int CalcSteps(DateTime from, DateTime to, TimeUnit units)
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

        public static int CalcStepsDown(DateTime from, DateTime to, TimeUnit units)
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

        public static int CalcStepsUp(DateTime from, DateTime to, TimeUnit units)
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

        public static int CalcIncrementStepsFromUtc(DateTime utcFromDate, TimeUnit incrementTimeUnit, TimeUnit stepsTimeUnit, int increment, TimeZoneInfo timeZone)
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


        public static DateTime CalcFromIncrement(DateTime utcFromDate, TimeUnit incrementTimeUnit, int increment)
        {
            switch (incrementTimeUnit)
            {
                case (TimeUnit.hr):
                    return utcFromDate.AddHours(increment);
                case (TimeUnit.da):
                    return utcFromDate.AddDays(increment);
                case (TimeUnit.mo):
                    return utcFromDate.AddMonths(increment);
            }
            return utcFromDate;
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

        public static explicit operator DateTime(DatePlus date)
        {
            return date.mDate;
        }

        public static explicit operator DatePlus(MultipleTimeseries dataset)
        {
            return new DatePlus(dataset, true);
        }

        public static DatePlus Min(bool from, params MultipleTimeseries[] datasets)
        {
            List<DatePlus> dates = new List<DatePlus>();

            foreach (MultipleTimeseries data in datasets)
            {
                dates.Add(new DatePlus(data, from));
            }

            return Min(dates.ToArray<DatePlus>());
        }

        public static DatePlus Min(params DatePlus[] dates)
        {
            DatePlus min = new DatePlus(DateTime.MaxValue, TimeUnit.hr);

            foreach (DatePlus date in dates)
            {
                min = min < date ? min : date;
            }

            return min;
        }

        public static DatePlus Max(bool from, params MultipleTimeseries[] datasets)
        {
            List<DatePlus> dates = new List<DatePlus>();

            foreach (MultipleTimeseries data in datasets)
            {
                dates.Add(new DatePlus(data, from));
            }

            return Max(dates.ToArray<DatePlus>());
        }

        public static DatePlus Max(params DatePlus[] dates)
        {
            DatePlus max = new DatePlus(DateTime.MinValue, TimeUnit.hr);

            foreach (DatePlus date in dates)
            {
                max = max > date ? max : date;
            }

            return max;
        }

        public static int CalcStepsUp(TimePeriod intersectUtcDataPeriod, TimeUnit timeUnit)
        {
            return CalcStepsUp(intersectUtcDataPeriod.from, intersectUtcDataPeriod.to, timeUnit, intersectUtcDataPeriod.zone);
        }

        public static int CalcStepsUp(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo zone)
        {
            return CalcStepsUp(from, to, units, zone, zone);
        }

        public static int CalcStepsUp(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo fromZone, TimeZoneInfo toZone)
        {
            to = TimeZoneInfo.ConvertTimeToUtc(to, toZone);
            from = TimeZoneInfo.ConvertTimeToUtc(from, fromZone);
            return CalcStepsUp(from, to, units);
        }

        public static int CalcStepsDown(TimePeriod intersectUtcDataPeriod, TimeUnit timeUnit)
        {
            return CalcStepsDown(intersectUtcDataPeriod.from, intersectUtcDataPeriod.to, timeUnit, intersectUtcDataPeriod.zone);
        }

        public static int CalcStepsDown(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo zone)
        {
            return CalcStepsDown(from, to, units, zone, zone);
        }

        public static int CalcStepsDown(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo fromZone, TimeZoneInfo toZone)
        {
            to = TimeZoneInfo.ConvertTimeToUtc(to, toZone);
            from = TimeZoneInfo.ConvertTimeToUtc(from, fromZone);
            return CalcStepsDown(from, to, units);
        }

        public static int CalcSteps(TimePeriod intersectDataPeriod, TimeUnit timeUnit)
        {
            return CalcSteps(intersectDataPeriod.from, intersectDataPeriod.to, timeUnit, intersectDataPeriod.zone);
        }

        public static int CalcSteps(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo zone)
        {
            return CalcSteps(from, to, units, zone, zone);
        }

        public static int CalcSteps(DateTime from, DateTime to, TimeUnit units, TimeZoneInfo fromZone, TimeZoneInfo toZone)
        {
            to = TimeZoneInfo.ConvertTimeToUtc(to, toZone);
            from = TimeZoneInfo.ConvertTimeToUtc(from, fromZone);
            return CalcSteps(from, to, units);
        }

        public override bool Equals(Object a)
        {
            return TimeZoneInfo.ConvertTimeToUtc(mDate,mZone).Equals(a);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static TimeZoneInfo VicTime()
        {
            // Create alternate Melbourne Time to include historical time zone information
            //
            // Declare necessary TimeZoneInfo.AdjustmentRule objects for time zone
            TimeSpan delta = new TimeSpan(1, 0, 0);
            TimeZoneInfo.AdjustmentRule adjustment;
            List<TimeZoneInfo.AdjustmentRule> adjustmentList = new List<TimeZoneInfo.AdjustmentRule>();
            // Declare transition time variables to hold transition time information
            TimeZoneInfo.TransitionTime transitionRuleStart, transitionRuleEnd;

            // Define rule (1916-1917)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 1, 1, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1916, 12, 30), new DateTime(1917, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1941-1942)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 1, 1, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 5, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1941, 12, 30), new DateTime(1942, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1942-1943)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 9, 4, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1942, 9, 1), new DateTime(1943, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1943-1944)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 1, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1943, 9, 28), new DateTime(1944, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1971-1972)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 2, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1971, 10, 1), new DateTime(1972, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1972-1973)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1972, 10, 1), new DateTime(1973, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1973-1976)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1973, 10, 1), new DateTime(1976, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1976-1979)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1976, 10, 1), new DateTime(1979, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1979-1982)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1979, 10, 1), new DateTime(1982, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1982-1984)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1982, 10, 1), new DateTime(1984, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1984-1985)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1984, 10, 1), new DateTime(1985, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1985-1986)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 3, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1985, 10, 1), new DateTime(1986, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1986-1987)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 03, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 3, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1986, 10, 1), new DateTime(1987, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1987-1988)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 3, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1987, 10, 1), new DateTime(1988, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1988-1990)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 3, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1988, 10, 1), new DateTime(1990, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1990-1993)
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1990, 10, 1), new DateTime(1993, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1993-1994 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1993, 10, 01), new DateTime(1994, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1994-1995 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1994, 10, 01), new DateTime(1995, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1995-1996 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 5, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1995, 10, 01), new DateTime(1996, 4, 1), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1996-1998 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 5, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1996, 10, 01), new DateTime(1998, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1998-1999 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1998, 10, 01), new DateTime(1999, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (1999-2000 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1999, 10, 01), new DateTime(2000, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2000-2001 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2000, 10, 01), new DateTime(2001, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2001-2003 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 5, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2001, 10, 01), new DateTime(2003, 4, 1), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2003-2004 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2003, 10, 01), new DateTime(2004, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2004-2005 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2004, 10, 01), new DateTime(2005, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2005-2006 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 4, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2005, 10, 01), new DateTime(2006, 4, 30), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2006-2007 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 05, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 3, 4, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2006, 10, 01), new DateTime(2007, 3, 31), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2007-2008 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 04, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 4, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2007, 10, 01), new DateTime(2008, 4, 30), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2008-2009 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 01, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 4, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2008, 10, 01), new DateTime(2009, 4, 30), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2009-2010 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 01, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 4, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2009, 10, 01), new DateTime(2010, 4, 30), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);
            // Define rule (2010-2011 )  
            transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 10, 01, DayOfWeek.Sunday);
            transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 4, 1, DayOfWeek.Sunday);
            adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2010, 10, 01), new DateTime(2011, 4, 30), delta, transitionRuleStart, transitionRuleEnd);
            adjustmentList.Add(adjustment);

            // Create custom Australian Eastern Standard Time zone         
            return TimeZoneInfo.CreateCustomTimeZone("Historial Victorian Time", new TimeSpan(10, 0, 0),
                            "(GMT+10:00) Australian Eastern Standard Time", "Eastern Standard Time",
                            "Eastern Daylight Time", adjustmentList.ToArray());
        }

        public string ToString(string p)
        {
            return mDate.ToString(p);
        }
    }
}
