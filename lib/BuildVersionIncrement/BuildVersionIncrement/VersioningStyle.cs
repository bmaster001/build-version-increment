using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using BuildVersionIncrement.Incrementors;
using System.Diagnostics;

namespace BuildVersionIncrement
{
    /// <summary>
    /// This enum defines the supported Build Versioning Styles
    /// </summary>
    /// <remarks>
    /// If you add a enum value, you need to adjust VersionIncrementer.CreateBuildVersion()
    /// </remarks>
    internal enum OLD_BuildVersioningStyleType : int
    {
        /// <summary>
        /// DeltaBaseDate 1.0.41204.2105 (base date = 10/21/1975, this was the original style)
        /// </summary>
        DeltaBaseDate = 0,
        /// <summary>
        /// YYDDD.HHMM 1.0.9021.2106 this style avoids the uint*32 issue by formating the days portion as the day of the year (1-366)
        /// </summary>
        YearDayOfYear_Timestamp = 1,
        /// <summary>
        /// 1.0.10121.2106 2008 (microsoft style) format is (current year - base year)MMDD
        /// </summary>
        DeltaBaseYear = 2,
        /// <summary>
        ///  this style just autoincrements the version field, resets to 0 if the build part is not today
        /// </summary>
        YearDayOfYear_AutoIncrement = 3,
        /// <summary>
        /// autoincrements only the build version. Leaves revision untouched.
        /// </summary>
        AutoIncrementBuildVersion = 4
    }

    /// <summary>
    /// The type of build action
    /// </summary>
    public enum BuildActionType
    {
        /// <summary>
        /// Both
        /// </summary>
        Both,
        /// <summary>
        /// Normal build
        /// </summary>
        Build,
        /// <summary>
        /// Complete rebuild
        /// </summary>
        ReBuild
    }

    /// <summary>
    /// The increment style type
    /// </summary>
    public enum OLD_IncrementStyle
    {
        /// <summary>
        /// No increment.
        /// </summary>
        [Description("No increment")]
        None,
        /// <summary>
        /// Day stamp (dd)
        /// </summary>
        [Description("Day stamp (dd)")]
        DayStamp,
        /// <summary>
        /// Delta base date (y * 12 + month, dayOfMonth since start date)
        /// </summary>
        [Description("Delta base date (y * 12 + month, dayOfMonth since start date)")]
        DeltaBaseDate,
        /// <summary>
        /// Delta base year including day of year (years since start date, dayOfYear)
        /// </summary>
        [Description("Delta base year including day of year (years since start date, dayOfYear)")]
        DeltaBaseYearDayOfYear,
        /// <summary>
        /// Delta base year (years since start date)
        /// </summary>
        [Description("Delta base year (years since start date)")]
        DeltaBaseYear,
        /// <summary>
        /// Simple increment
        /// </summary>
        [Description("Simple increment")]
        Increment,
        /// <summary>
        /// Month stamp (mm)
        /// </summary>
        [Description("Month stamp (mm)")]
        MonthStamp,
        /// <summary>
        /// Time stamp (hh mm)
        /// </summary>
        [Description("Time stamp (hh mm)")]
        TimeStamp,
        /// <summary>
        /// Year stamp (yyyy)
        /// </summary>
        [Description("Year stamp (yyyy)")]
        YearStamp,
        /// <summary>
        /// Year followed by the day of the year (yy, dayOfYear)
        /// </summary>
        [Description("Year followed by the day of the year (yy, dayOfYear)")]
        YearDayOfYear,
        /// <summary>
        /// Year decade stamp (yy)
        /// </summary>
        [Description("Year decade stamp (yy)")]
        YearDecadeStamp,
        /// <summary>
        /// Month and Day stamp (mmdd)
        /// </summary>        
        [Description("Month and Day stamp (mmdd)")]
        MonthAndDayStamp,
        /// <summary>
        /// Delta base date in daystamp
        /// </summary>
        [Description("Delta base date in days")]
        DeltaBaseDateInDays
    }

    /// <summary>
    /// Describes the versioning style.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal class VersioningStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersioningStyle"/> class.
        /// </summary>
        public VersioningStyle(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="VersioningStyle"/> class.
        /// </summary>
        /// <param name="other">Another instances to copy the values from.</param>
        public VersioningStyle(VersioningStyle other)
        {
            Major = other.Major;
            Minor = other.Minor;
            Build = other.Build;
            Revision = other.Revision;
        }

        /// <summary>
        /// The default style (None.None.None.None)
        /// </summary>
        //public static VersioningStyle Default = new VersioningStyle();
        internal string ToGlobalVariable()
        {
           /* string startDate = string.Format("{0}/{1}/{2}",
                                             ProjectStartDate.Year,
                                             ProjectStartDate.Month,
                                             ProjectStartDate.Day);

            string versionStyle = string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);

            return string.Format("{0}|{1}", versionStyle, startDate);*/

            return ToString(); // NOTE: The project start date value is saved by the solution item
        }

        /// <summary>
        /// Initializes this instance based on the old increment enum styles.
        /// </summary>
        /// <param name="major">The major style.</param>
        /// <param name="minor">The minor style.</param>
        /// <param name="build">The build style.</param>
        /// <param name="revision">The revision style.</param>
        internal void FromOldIncrementStyle(OLD_IncrementStyle major, OLD_IncrementStyle minor, OLD_IncrementStyle build, OLD_IncrementStyle revision)
        {
            Major = BuildVersionIncrementor.Instance.Incrementors[major];
            Minor = BuildVersionIncrementor.Instance.Incrementors[minor];
            Build = BuildVersionIncrementor.Instance.Incrementors[build];
            Revision = BuildVersionIncrementor.Instance.Incrementors[revision];
        }

        /// <summary>
        /// Initializes this instances based on a global variable value.
        /// </summary>
        /// <param name="value">The value.</param>
        internal void FromGlobalVariable(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Contains("."))
                {
                    // Assuming new enum

                    string[] styles = value.Split(".".ToCharArray());

                    if (styles.Length == 4)
                    {
                        Major = BuildVersionIncrementor.Instance.Incrementors[styles[0]];
                        Minor = BuildVersionIncrementor.Instance.Incrementors[styles[1]];
                        Build = BuildVersionIncrementor.Instance.Incrementors[styles[2]];
                        Revision = BuildVersionIncrementor.Instance.Incrementors[styles[3]];

                        /*OLD_IncrementStyle major = (OLD_IncrementStyle)Enum.Parse(typeof(OLD_IncrementStyle), styles[0]);
                        OLD_IncrementStyle minor = (OLD_IncrementStyle)Enum.Parse(typeof(OLD_IncrementStyle), styles[1]);
                        OLD_IncrementStyle build = (OLD_IncrementStyle)Enum.Parse(typeof(OLD_IncrementStyle), styles[2]);
                        OLD_IncrementStyle revision = (OLD_IncrementStyle)Enum.Parse(typeof(OLD_IncrementStyle), styles[3]);

                        FromOldIncrementStyle(major, minor, build, revision);*/
                    }
                    else
                    {
                        throw (new ApplicationException("Invalid versioning style \"" + value + "\"."));
                    }
                }
                else
                {
                    // Old enum

                    OLD_BuildVersioningStyleType oldStyle = (OLD_BuildVersioningStyleType)Enum.Parse(typeof(OLD_BuildVersioningStyleType), value);

                    OLD_IncrementStyle major = OLD_IncrementStyle.None;
                    OLD_IncrementStyle minor = OLD_IncrementStyle.None;
                    OLD_IncrementStyle build = OLD_IncrementStyle.None;
                    OLD_IncrementStyle revision = OLD_IncrementStyle.None;

                    switch (oldStyle)
                    {
                        case OLD_BuildVersioningStyleType.AutoIncrementBuildVersion:
                            build = OLD_IncrementStyle.Increment;
                            break;

                        case OLD_BuildVersioningStyleType.DeltaBaseDate:
                            build = OLD_IncrementStyle.DeltaBaseDate;
                            revision = OLD_IncrementStyle.TimeStamp;
                            break;

                        case OLD_BuildVersioningStyleType.DeltaBaseYear:
                            build = OLD_IncrementStyle.DeltaBaseYearDayOfYear;
                            revision = OLD_IncrementStyle.TimeStamp;
                            break;

                        case OLD_BuildVersioningStyleType.YearDayOfYear_AutoIncrement:
                            build = OLD_IncrementStyle.YearDayOfYear;
                            revision = OLD_IncrementStyle.Increment;
                            break;

                        case OLD_BuildVersioningStyleType.YearDayOfYear_Timestamp:
                            build = OLD_IncrementStyle.YearDayOfYear;
                            revision = OLD_IncrementStyle.TimeStamp;
                            break;

                        default:
                            throw (new ApplicationException("Unknown (old) versioning type: " + oldStyle.ToString()));
                    }

                    FromOldIncrementStyle(major, minor, build, revision);
                }
            }
            else
            {
                Major = Minor = Build = Revision = null;
            }
        }

        /// <summary>
        /// Increments the specified version.
        /// </summary>
        /// <param name="currentVersion">The current version.</param>
        /// <param name="buildStartDate">The build start date.</param>
        /// <param name="projectStartDate">The project start date.</param>
        /// <returns>The incremented version.</returns>
        internal StringVersion Increment(StringVersion currentVersion, DateTime buildStartDate, DateTime projectStartDate, SolutionItem solutionItem)
        {
            IncrementContext context = new IncrementContext(currentVersion, buildStartDate, projectStartDate, solutionItem.Filename);

            BaseIncrementor[] incrementors = new BaseIncrementor[] { Major, Minor, Build, Revision };

            for (int i = 0; i < 4; i++)
            {
                BaseIncrementor incrementor = incrementors[i];

                if (incrementor == null) continue;

                VersionComponent component = (VersionComponent)i;

                incrementor.Increment(context, component);

                if (!context.Continue)
                    break;
            }

            return context.NewVersion;

            /*int major = Major == null ? currentVersion.Major : Major.Increment(currentVersion.Major, buildStartDate, projectStartDate, solutionItem.Filename);
            int minor = Minor == null ? currentVersion.Minor : Minor.Increment(currentVersion.Minor, buildStartDate, projectStartDate, solutionItem.Filename);
            int build = Build == null ? currentVersion.Build : Build.Increment(currentVersion.Build, buildStartDate, projectStartDate, solutionItem.Filename);
            int revision = Revision == null ? currentVersion.Revision : Revision.Increment(currentVersion.Revision, buildStartDate, projectStartDate, solutionItem.Filename);

            return new Version(major, minor, build, revision);*/
        }

        internal static string GetDefaultGlobalVariable()
        {
            return new VersioningStyle().ToGlobalVariable();
        }
        
        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", 
                                 Major.Name, 
                                 Minor.Name, 
                                 Build.Name, 
                                 Revision.Name);
        }

        #region Ugly PropertyGrid reflection code

        private bool ShouldSerializeMajor()
        {
            return _major != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeMinor()
        {
            return _minor != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeBuild()
        {
            return _build != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeRevision()
        {
            return _revision != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        #endregion

        private BaseIncrementor _major = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the major increment style.
        /// </summary>
        /// <value>The major increment style.</value>
        [Description("Major update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Major
        {
			get { return this._major ?? BuiltInBaseIncrementor.NoneIncrementor.Instance; }
            set 
            {
                Debug.Assert(value != null);
                this._major = value;
            }
        }

        private BaseIncrementor _minor = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the minor increment style.
        /// </summary>
        /// <value>The minor increment style.</value>
        [Description("Minor update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Minor
        {
			get { return this._minor ?? BuiltInBaseIncrementor.NoneIncrementor.Instance; }
            set 
            {
                Debug.Assert(value != null);
                this._minor = value; 
            }
        }

        private BaseIncrementor _build = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the build increment style.
        /// </summary>
        /// <value>The build increment style.</value>
        [Description("Build update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Build
        {
			get { return this._build ?? BuiltInBaseIncrementor.NoneIncrementor.Instance; }
            set 
            {
                Debug.Assert(value != null);
                this._build = value;
            }
        }

        private BaseIncrementor _revision = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the revision increment style.
        /// </summary>
        /// <value>The revision increment style.</value>
        [Description("Revision update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Revision
        {
			get { return this._revision ?? BuiltInBaseIncrementor.NoneIncrementor.Instance; }
            set
            {
                Debug.Assert(value != null);
                this._revision = value;
            }
        }
    }
}
