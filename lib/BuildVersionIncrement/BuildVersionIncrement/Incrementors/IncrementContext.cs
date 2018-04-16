using System;
using System.Collections.Generic;
using System.Text;

namespace BuildVersionIncrement.Incrementors
{
    /// <summary>
    /// The increment context used by the <see cref="BaseIncrementor"/> instances.
    /// </summary>
    /// <seealso cref="BaseIncrementor.Increment"/>
    public class IncrementContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementContext"/> class.
        /// </summary>
        /// <param name="currentVersion">The current version.</param>
        /// <param name="buildStartDate">The build start date.</param>
        /// <param name="projectStartDate">The project start date.</param>
        /// <param name="projectFilename">The project filename.</param>
        internal IncrementContext(StringVersion currentVersion, DateTime buildStartDate, DateTime projectStartDate, string projectFilename)
        {
            CurrentVersion = currentVersion;
            BuildStartDate = buildStartDate;
            ProjectStartDate = projectStartDate;
            ProjectFilename = projectFilename;

            NewVersion = new StringVersion(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision);
            Continue = true;
        }

        /// <summary>
        /// Gets the current version.
        /// </summary>
        /// <value>The current version.</value>
        public StringVersion CurrentVersion { get; private set; }

        /// <summary>
        /// Gets the build start date.
        /// </summary>
        /// <value>The build start date.</value>
        public DateTime BuildStartDate { get; private set; }

        /// <summary>
        /// Gets the project start date.
        /// </summary>
        /// <value>The project start date.</value>
        public DateTime ProjectStartDate { get; private set; }

        /// <summary>
        /// Gets the project filename.
        /// </summary>
        /// <value>The project filename.</value>
        public string ProjectFilename { get; private set; }

        private StringVersion _newVersion;
        /// <summary>
        /// Gets or sets the new version.
        /// </summary>
        /// <value>The new version.</value>
        public StringVersion NewVersion 
        {
            get { return _newVersion; }
            set
            {
                if (value == null)
                    throw (new ArgumentNullException());

                _newVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the addin should continue incrementing other version components.
        /// </summary>
        /// <value><c>true</c> to continue; otherwise, <c>false</c>.</value>
        public bool Continue { get; set; }

        /// <summary>
        /// Gets the current version component value.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>The component value.</returns>
        public string GetCurrentVersionComponentValue(VersionComponent component)
        {
            switch (component)
            {
                case VersionComponent.Build:
                    return this.CurrentVersion.Build;

                case VersionComponent.Major:
                    return this.CurrentVersion.Major;

                case VersionComponent.Minor:
                    return this.CurrentVersion.Minor;

                case VersionComponent.Revision:
                    return this.CurrentVersion.Revision;
            }

            return "0";
        }

        /// <summary>
        /// Sets the specified component of the new version to a specified value.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="value">The value.</param>
        public void SetNewVersionComponentValue(VersionComponent component, string value)
        {
            switch (component)
            {
                case VersionComponent.Build:
                    this.NewVersion = new StringVersion(this.NewVersion.Major, this.NewVersion.Minor, value, this.NewVersion.Revision);
                    break;

                case VersionComponent.Major:
                    this.NewVersion = new StringVersion(value, this.NewVersion.Minor, this.NewVersion.Build, this.NewVersion.Revision);
                    break;

                case VersionComponent.Minor:
                    this.NewVersion = new StringVersion(this.NewVersion.Major, value, this.NewVersion.Build, this.NewVersion.Revision);
                    break;

                case VersionComponent.Revision:
                    this.NewVersion = new StringVersion(this.NewVersion.Major, this.NewVersion.Minor, this.NewVersion.Build, value);
                    break;
            }
        }
    }

    /// <summary>
    /// Indicates a version component of a <see cref="Version"/> object.
    /// </summary>
    public enum VersionComponent : int
    {
        /// <summary>
        /// The major component.
        /// </summary>
        Major = 0,
        /// <summary>
        /// The minor component.
        /// </summary>
        Minor = 1,
        /// <summary>
        /// The build component.
        /// </summary>
        Build = 2,
        /// <summary>
        /// The revision component.
        /// </summary>
        Revision = 3
    }
}
