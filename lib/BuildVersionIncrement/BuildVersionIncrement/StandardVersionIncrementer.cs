using System;
using System.Collections.Generic;
using System.Text;

namespace BuildVersionIncrement
{
    internal class StandardVersionIncrementerOld : IVersionIncrementerOld
    {
        #region IVersionIncrementer Members

        private DateTime _buildStartDate;
        /// <summary>
        /// Gets or sets the build start date.
        /// </summary>
        /// <value>The build start date.</value>
        public DateTime BuildStartDate
        {
            get { return _buildStartDate; }
            set { _buildStartDate = value; }
        }

        private DateTime _projectStartDate = new DateTime(1975, 10, 21);
        /// <summary>
        /// Gets the start date.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime ProjectStartDate
        {
            get { return this._projectStartDate; }
            set { this._projectStartDate = value; }
        }

        /// <summary>
        /// Increments the specified version number.
        /// </summary>
        /// <param name="current">The current number.</param>
        /// <param name="incrementStyle">The increment style.</param>
        /// <returns>The incremented version number.</returns>
        public virtual string Increment(int current, OLD_IncrementStyle incrementStyle)
        {
            string dayOfyear = BuildStartDate.DayOfYear.ToString("000");
            int deltaYears = BuildStartDate.Year - ProjectStartDate.Year;
            string yearDecade = BuildStartDate.ToString("yy");

            if (current < 0)
                current = 0;

            switch (incrementStyle)
            {
                case OLD_IncrementStyle.None:
                    return current.ToString();

                case OLD_IncrementStyle.Increment:
                    return (current + 1).ToString();

                case OLD_IncrementStyle.TimeStamp:
					return string.Format("{0:00}{1:00}", BuildStartDate.Hour, BuildStartDate.Minute);

                /*case IncrementStyle.DateStamp:
                   
                 * This will not work; version numbers are 64 bit (4x16) so the maximum a counter can hold is 65534. Using the code below will
                 * result in a "The version specified ‘x.x.x.x′ is invalid".
                 * 
                 * return Int32.Parse(string.Format("{0:0000}{1:00}{2:00}", BuildStartDate.Year, BuildStartDate.Month, BuildStartDate.Day)); 
                    
                    */

                case OLD_IncrementStyle.YearStamp:
                    return BuildStartDate.Year.ToString();

                case OLD_IncrementStyle.DeltaBaseDate:
                    /*TimeSpan ts = BuildStartDate.Subtract(ProjectStartDate);
                    DateTime dt = DateTime.MinValue + ts;
                    return Int32.Parse(string.Format("{0}{1:00}", dt.Year * 12 + dt.Month, dt.Day));*/

                    // Fixed Yogesh Jagota's increment scheme
                    DateSpan ds = DateSpan.GetDateDifference(BuildStartDate, ProjectStartDate);
					return string.Format("{0}{1:00}", (ds.Years * 12) + ds.Months, ds.Days);

                case OLD_IncrementStyle.DeltaBaseDateInDays:
                    TimeSpan ts = BuildStartDate.Subtract(ProjectStartDate);
                    return ((int)ts.TotalDays).ToString();

                case OLD_IncrementStyle.YearDayOfYear:
                    return string.Format("{0}{1:000}", yearDecade, dayOfyear);

                case OLD_IncrementStyle.DeltaBaseYearDayOfYear:
					return string.Format("{0}{1:000}", deltaYears, dayOfyear);

                case OLD_IncrementStyle.DeltaBaseYear:
                    return deltaYears.ToString();

                case OLD_IncrementStyle.YearDecadeStamp:
                    return yearDecade;

                case OLD_IncrementStyle.MonthStamp:
					return BuildStartDate.Month.ToString();

                 case OLD_IncrementStyle.DayStamp:
					return BuildStartDate.Day.ToString();
 
                case OLD_IncrementStyle.MonthAndDayStamp:
					return string.Format("{0:00}{1:00}", BuildStartDate.Month, BuildStartDate.Day);

                 default:
                     throw (new ApplicationException("Unknown increment style: " + incrementStyle.ToString()));
             }
        }

        #endregion
    }
}
