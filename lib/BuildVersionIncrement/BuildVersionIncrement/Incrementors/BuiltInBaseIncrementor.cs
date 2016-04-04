using System;
using System.Collections.Generic;
using System.Text;
using Qreed.Reflection;

namespace BuildVersionIncrement.Incrementors
{
    /// <summary>
    /// Internal base class for the default incrementers.
    /// </summary>
    /// <remarks>
    /// This is based on the old <see cref="IncrementStyle"/> enum that defined the type of increment.
    /// </remarks>
    internal abstract class BuiltInBaseIncrementor : BaseIncrementor
    {
        /// <summary>
        /// Gets the name of this incrementor.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return IncrementStyle.ToString();
            }
        }

        /// <summary>
        /// Gets the description of this incrementor.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return EnumHelper.GetDescription(IncrementStyle);
            }
        }

        /// <summary>
        /// Gets the increment style.
        /// </summary>
        /// <value>The increment style.</value>
        private OLD_IncrementStyle IncrementStyle
        {
            get
            {
                string name = this.GetType().Name;
                name = name.Substring(0, name.Length - "Incrementor".Length);

                return (OLD_IncrementStyle)Enum.Parse(typeof(OLD_IncrementStyle), name);
            }
        }

        /// <summary>
        /// Executes the increment.
        /// </summary>
        /// <param name="context">The context of the increment.</param>
        /// <param name="versionComponent">The version component that needs to be incremented.</param>
        public override void Increment(IncrementContext context, VersionComponent versionComponent)
        {
            string currentValue = context.GetCurrentVersionComponentValue(versionComponent);
            string newValue = this.Increment(currentValue, context.BuildStartDate, context.ProjectStartDate, context.ProjectFilename);

            context.SetNewVersionComponentValue(versionComponent, newValue);
        }

        /// <summary>
        /// Increments the specified value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <param name="buildStart">The build start date/time.</param>
        /// <param name="projectStart">The project start date/time.</param>
        /// <returns>The incremented value.</returns>
        private string Increment(string value, DateTime buildStart, DateTime projectStart, string projectFilePath)
        {
            string dayOfyear = buildStart.DayOfYear.ToString("000");
            int deltaYears = buildStart.Year - projectStart.Year;
            string yearDecade = buildStart.ToString("yy");

			int intValue = 0;
			Int32.TryParse(value, out intValue);
			if (intValue < 0)
				intValue = 0;

            switch (IncrementStyle)
            {
                case OLD_IncrementStyle.None:
                    return value;

                case OLD_IncrementStyle.Increment:
					return (intValue + 1).ToString();

                case OLD_IncrementStyle.TimeStamp:
					return string.Format("{0:00}{1:00}", buildStart.Hour, buildStart.Minute);

                case OLD_IncrementStyle.YearStamp:
                    return (buildStart.Year).ToString();

                case OLD_IncrementStyle.DeltaBaseDate:
                    DateSpan ds = DateSpan.GetDateDifference(buildStart, projectStart);
					return string.Format("{0}{1:00}", (ds.Years * 12) + ds.Months, ds.Days);

                case OLD_IncrementStyle.DeltaBaseDateInDays:
                    TimeSpan ts = buildStart.Subtract(projectStart);
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
					return buildStart.Month.ToString();

                case OLD_IncrementStyle.DayStamp:
                    return buildStart.Day.ToString();

                case OLD_IncrementStyle.MonthAndDayStamp:
					return string.Format("{0:00}{1:00}", buildStart.Month, buildStart.Day);

                default:
                    throw (new ApplicationException("Unknown increment style: " + IncrementStyle.ToString()));
            }
        }

        internal class NoneIncrementor : BuiltInBaseIncrementor 
        {
            /// <summary>
            /// Use this to reference a null incrementor.
            /// </summary>
            public static readonly NoneIncrementor Instance = new NoneIncrementor();
        }

        class DayStampIncrementor : BuiltInBaseIncrementor { }
        class DeltaBaseDateIncrementor : BuiltInBaseIncrementor { }
        class DeltaBaseDateInDaysIncrementor : BuiltInBaseIncrementor { }
        class DeltaBaseYearDayOfYearIncrementor : BuiltInBaseIncrementor { }
        class DeltaBaseYearIncrementor : BuiltInBaseIncrementor { }
        class IncrementIncrementor : BuiltInBaseIncrementor { }
        class MonthStampIncrementor : BuiltInBaseIncrementor { }
        class TimeStampIncrementor : BuiltInBaseIncrementor { }
        class YearStampIncrementor : BuiltInBaseIncrementor { }
        class YearDayOfYearIncrementor : BuiltInBaseIncrementor { }
        class YearDecadeStampIncrementor : BuiltInBaseIncrementor { }
        class MonthAndDayStampIncrementor : BuiltInBaseIncrementor { }
    }
}
