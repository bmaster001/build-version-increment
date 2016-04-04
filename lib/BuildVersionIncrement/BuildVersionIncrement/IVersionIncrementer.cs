using System;
using System.Collections.Generic;
using System.Text;

namespace BuildVersionIncrement
{
    internal interface IVersionIncrementerOld
    {
        /// <summary>
        /// Creates the build version.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="currentVersion">The current version as found in the assembly info.</param>
        /// <returns>The new version.</returns>
        // Version CreateBuildVersion(VersioningStyle settings, Version currentVersion);

        /// <summary>
        /// Gets or sets the build start date.
        /// </summary>
        /// <value>The build start date.</value>
        DateTime BuildStartDate { get; set; }

        /// <summary>
        /// Gets the project start date.
        /// </summary>
        /// <value>The project start date.</value>
        DateTime ProjectStartDate { get; set; }

        /// <summary>
        /// Increments the specified version number.
        /// </summary>
        /// <param name="current">The current number.</param>
        /// <param name="incrementStyle">The increment style.</param>
        /// <returns>The incremented version number.</returns>
        string Increment(int current, OLD_IncrementStyle incrementStyle);
    }
}
