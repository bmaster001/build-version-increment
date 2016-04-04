using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace BuildVersionIncrement
{
    /// <summary>
    /// Abtract class containing all common increment settings (global/solutionitem).
    /// </summary>
    /// <seealso cref="SolutionItemIncrementSettings"/>
    /// <seealso cref="GlobalIncrementSettings"/>
    public abstract class IncrementSettings
    {
        /// <summary>
        /// Loads the settings into this instance.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Saves the settings of this instance.
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// Resets the settings to the defaults
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Copies settings from another instance.
        /// </summary>
        /// <param name="source">The source to copy the settings from.</param>
        public virtual void CopyFrom(IncrementSettings source)
        {
            VersioningStyle = new VersioningStyle(source.VersioningStyle);
            AutoUpdateAssemblyVersion = source.AutoUpdateAssemblyVersion;
            AutoUpdateFileVersion = source.AutoUpdateFileVersion;
            BuildAction = source.BuildAction;
            StartDate = source.StartDate;
            IsUniversalTime = source.IsUniversalTime;
            ReplaceNonNumerics = source.ReplaceNonNumerics;
            IncrementBeforeBuild = source.IncrementBeforeBuild;
        }

        private VersioningStyle _versioningStyle = new VersioningStyle();
        /// <summary>
        /// Gets or sets the increment settings.
        /// </summary>
        /// <value>The increment settings.</value>
        [Browsable(true)]
        [Category("Auto Update")]
        [DisplayName("Versioning Style")]
        [Description("The version increment style settings.")]
        [DefaultValue("None.None.None.None")]
        public VersioningStyle VersioningStyle
        {
            get { return this._versioningStyle; }
            set { this._versioningStyle = value; }
        }

        private bool _autoUpdateFileVersion;
        /// <summary>
        /// Gets or sets a value indicating whether to auto update the file version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to auto update the file version; otherwise, <c>false</c>.
        /// </value>
        [Category("Auto Update")]
        [Description("Auto update the file version. Note that setting this to true on solution level will have no effect on building individual projects.")]
        [DisplayName("Update AssemblyFileVersion")]
        [DefaultValue(false)]
        public bool AutoUpdateFileVersion
        {
            get { return _autoUpdateFileVersion; }
            set { _autoUpdateFileVersion = value; }
        }

        private bool _autoUpdateAssemblyVersion;
        /// <summary>
        /// Gets or sets a value indicating whether to auto update the assembly version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to auto update the assembly version; otherwise, <c>false</c>.
        /// </value>
        [Category("Auto Update")]
        [Description("Auto update the assembly version. Note that setting this to true on solution level will have no effect on building individual projects.")]
        [DisplayName("Update AssemblyVersion")]
        [DefaultValue(false)]
        public bool AutoUpdateAssemblyVersion
        {
            get { return _autoUpdateAssemblyVersion; }
            set { _autoUpdateAssemblyVersion = value; }
        }

        private BuildActionType _buildAction;
        /// <summary>
        /// Gets or sets the build action
        /// </summary>
        /// <value>The build action on which the auto update should occur.</value>
        [Category("Auto Update")]
        [DefaultValue(BuildActionType.Both)]
        [DisplayName("Build Action")]
        [Description("Set this to the desired build action when the auto update should occur.")]
        public BuildActionType BuildAction
        {
            get { return _buildAction; }
            set { _buildAction = value; }
        }

        private DateTime _projectStartDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        [Category("Auto Update")]
        [Description("The start date to use.")]
        [DisplayName("Start Date")]
        [DefaultValue(typeof(DateTime), "1975/10/21")]
        public DateTime StartDate
        {
            get { return _projectStartDate; }
            set { _projectStartDate = value; }
        }

        private bool _isUniversalTime;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is using UTC.
        /// </summary>
        /// <value><c>true</c> if this instance is using UTC; otherwise, <c>false</c>.</value>
        [Category("Auto Update")]
        [Description("Indicates wheter to use Coordinated Universal Time (UTC) time stamps.")]
        [DisplayName("Use Coordinated Universal Time")]
        [DefaultValue(false)]
        public bool IsUniversalTime
        {
            get { return this._isUniversalTime; }
            set { this._isUniversalTime = value; }
        }

        private bool _replaceNonNumerics;
        /// <summary>
        /// Gets or sets if non-numbers within the version should be replaced by a zero.
        /// </summary>
        /// <value>The new value. :)</value>
        [Category("Auto Update")]
        [Description("If non-numeric values within the version should be replaced with a zero.")]
        [DisplayName("Replace Non-Numerics")]
        [DefaultValue(true)]
        public bool ReplaceNonNumerics
        {
            get { return _replaceNonNumerics; }
            set { _replaceNonNumerics = value; }
        }

        private bool _incrementBeforeBuild;
        /// <summary>
        /// Gets or set if the increment should happen before or after the current build.
        /// </summary>
        /// <remarks>WorkItem 3589 from PeteBSC</remarks>
        /// <value>The new value for this property.</value>
        [Category("Auto Update")]
        [Description("If the increment should be executed before the build.")]
        [DisplayName("Increment Before Build")]
        [DefaultValue(true)]
        public bool IncrementBeforeBuild
        {
            get { return _incrementBeforeBuild; }
            set { _incrementBeforeBuild = value; }
        }
    }
}
