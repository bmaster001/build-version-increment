using System;
using System.Collections.Generic;
using System.Text;

namespace BuildVersionIncrement
{
    /// <summary>
    /// DateSpan class used to computer difference of two dates
    /// </summary>
    public class DateSpan
    {
        private int _days;
        /// <summary>
        /// Difference in days
        /// </summary>
        public int Days
        {
            get { return this._days; }
            set { this._days = value; }
        }

        private int _months;
        /// <summary>
        /// Difference in months
        /// </summary>
        public int Months
        {
            get { return this._months; }
            set { this._months = value; }
        }

        private int _years;
        /// <summary>
        /// Difference in years
        /// </summary>
        public int Years
        {
            get { return this._years; }
            set { this._years = value; }
        }

		/// <summary>
        /// Gets the date difference.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="dateToCompare">The date to compare.</param>
        /// <returns>A <see>DataSpan</see> object.</returns>
        public static DateSpan GetDateDifference(DateTime date, DateTime dateToCompare)
        {
            int totalMonths = ((date.Year - dateToCompare.Year) * 12) +
                              date.Month - dateToCompare.Month;

            TimeSpan diff1 = date.Subtract(dateToCompare);

            DateTime finaldt = new DateTime(diff1.Ticks);

            DateSpan ds = new DateSpan();
            
            ds.Years = finaldt.Year - 1;
            ds.Months = finaldt.Month - 1;
            ds.Days = finaldt.Day - 1;

            return ds;
        }
    }

#if _DOTNET_FRAMEWORK_3DOT5_

    /*

    Index: System.Runtime.CompilerServices.cs
    ===================================================================
    --- System.Runtime.CompilerServices.cs	(revision 0)
    +++ System.Runtime.CompilerServices.cs	(revision 0)
    @@ -0,0 +1,7 @@
    +﻿namespace System.Runtime.CompilerServices
    +{
    +	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    +	public sealed class ExtensionAttribute : Attribute
    +	{
    +	}
    +}


    */

    /// <summary>
    /// Extensions methods which are available if you are using VS2008
    /// </summary>
    public static class DateDifferenceExtension
    {
        /// <summary>
        /// Returns difference of two dates
        /// </summary>
        /// <param name="date">DateTime on which extension method is used</param>
        /// <param name="dateToCompare">The second date to compare difference with</param>
        /// <returns>return DateSpan instance of the calculated date difference</returns>
        /// <remarks>The computation always is date - dateToCompare, so dateToCompare should be
        /// lesser than date. If it is not, the answer is always a little bit... ahem... weird.</remarks>
        public static DateSpan DateDifference(this DateTime date, DateTime dateToCompare)
        {
            int totalMonths = ((date.Year - dateToCompare.Year) * 12) +
                date.Month - dateToCompare.Month;

            int days = 0;

            if (date.Day < dateToCompare.Day)
            {
                int day, month, year;

                day = dateToCompare.Day;
                if (date.Month == 1)
                {
                    month = 12;
                    year = date.Year - 1;
                }
                else
                {
                    month = date.Month - 1;
                    year = date.Year;
                }

                DateTime dateCalculator = new DateTime(year, month, day);

                days = (date - dateCalculator).Days;

                totalMonths--;
            }
            else
            {
                days = date.Day - dateToCompare.Day;
            }

            DateSpan ds = new DateSpan();
            ds.Years = totalMonths / 12;
            ds.Months = totalMonths % 12;
            ds.Days = days;

            return ds;
        }
    }

#endif

}
