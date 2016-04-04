using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using EnvDTE80;
using System.Collections;
using System.ComponentModel;

namespace BuildVersionIncrement
{
    /// <summary>
    /// Storage for the global settings.
    /// </summary>
    /// <remarks>
    /// Global settings are stored in the user scope <see cref="GlobalAddinSettings"/>.
    /// </remarks>
    /// <seealso cref="IncrementSettings"/>
    internal class GlobalIncrementSettings : BaseIncrementSettings
    {
        /// <summary>
        /// When the Global Settings should be applied to projects.
        /// </summary>
        public enum ApplyGlobalSettings
        {
            /// <summary>
            /// Only if explicit chosen to use them
            /// </summary>
            OnlyWhenChosen,
            /// <summary>
            /// Trigger the UseGlobalSettings to true for new projects
            /// </summary>
            AsDefault,
            /// <summary>
            /// Override the UseGlobalSettings flag to true whatever happens.
            /// I'll show some warnings if that one is active.
            /// </summary>
            Always
        }

        private ApplyGlobalSettings _apply;
        /// <summary>
        /// Gets or Sets the setting when to apply the Global Settings.
        /// </summary>
        [Browsable(true)]
        [Category("Global")]
        [DisplayName("Apply Global Settings")]
        [Description("The setting when to use the Global Settings")]
        [DefaultValue(typeof(ApplyGlobalSettings), "OnlyWhenChosen")]
        public ApplyGlobalSettings Apply
        {
            get { return _apply; }
            set { _apply = value; }
        }

        public static ApplyGlobalSettings ApplySettings
        {
            get { return (ApplyGlobalSettings)Enum.Parse(typeof(ApplyGlobalSettings), GlobalAddinSettings.Default.GlobalApply); }
        }

        /// <summary>
        /// Loads the settings into this instance.
        /// </summary>
        public override void Load()
        {
            // set the increment style

            VersioningStyle vs = new VersioningStyle();
            vs.FromGlobalVariable(GlobalAddinSettings.Default.GlobalMajor + "." + GlobalAddinSettings.Default.GlobalMinor + "." +
                                  GlobalAddinSettings.Default.GlobalBuild + "." + GlobalAddinSettings.Default.GlobalRevision);
            this.VersioningStyle = vs;

            // get the rest

            BuildAction = (BuildActionType)Enum.Parse(typeof(BuildActionType), GlobalAddinSettings.Default.GlobalBuildAction);
            AutoUpdateAssemblyVersion = GlobalAddinSettings.Default.GlobalAutoUpdateAssemblyVersion;
            AutoUpdateFileVersion = GlobalAddinSettings.Default.GlobalAutoUpdateFileVersion;
            ReplaceNonNumerics = GlobalAddinSettings.Default.GlobalReplaceNonNumeric;
            IsUniversalTime = GlobalAddinSettings.Default.GlobalUseUniversalClock;
            IncrementBeforeBuild = GlobalAddinSettings.Default.GlobalIncrementBeforeBuild;
            StartDate = GlobalAddinSettings.Default.GlobalStartDate;

            /* ?? No idea why this was done by parsing
             * 
             * DateTime parsedValue = new DateTime(1975, 10, 21);

            try
            {
                parsedValue = DateTime.ParseExact(GlobalAddinSettings.Default.GlobalStartDate, "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.None);
            }
            catch (Exception ex)
            {
                Logger.Write("Error occured while parsing the global start date \"" + GlobalAddinSettings.Default.GlobalStartDate + "\".\r\n" + ex.ToString(), LogLevel.Error); 
            }

            StartDate = parsedValue;*/
            DetectChanges = GlobalAddinSettings.Default.DetectChanges;
            Apply = (ApplyGlobalSettings)Enum.Parse(typeof(ApplyGlobalSettings), GlobalAddinSettings.Default.GlobalApply);
        }

        /// <summary>
        /// Saves the settings of this instance.
        /// </summary>
        public override void Save()
        {
            GlobalAddinSettings.Default.GlobalMajor = VersioningStyle.Major.Name;
            GlobalAddinSettings.Default.GlobalMinor = VersioningStyle.Minor.Name;
            GlobalAddinSettings.Default.GlobalBuild = VersioningStyle.Build.Name;
            GlobalAddinSettings.Default.GlobalRevision = VersioningStyle.Revision.Name;

            GlobalAddinSettings.Default.GlobalBuildAction = BuildAction.ToString();
            GlobalAddinSettings.Default.GlobalAutoUpdateAssemblyVersion = AutoUpdateAssemblyVersion;
            GlobalAddinSettings.Default.GlobalAutoUpdateFileVersion = AutoUpdateFileVersion;
            GlobalAddinSettings.Default.GlobalStartDate = StartDate; // StartDate.ToString("yyyy/MM/dd");
            GlobalAddinSettings.Default.GlobalReplaceNonNumeric = ReplaceNonNumerics;
            GlobalAddinSettings.Default.GlobalUseUniversalClock = IsUniversalTime;
            GlobalAddinSettings.Default.GlobalIncrementBeforeBuild = IncrementBeforeBuild;
            GlobalAddinSettings.Default.DetectChanges = DetectChanges;

            GlobalAddinSettings.Default.GlobalApply = Apply.ToString();

            GlobalAddinSettings.Default.Save();
        }

        /// <summary>
        /// Resets the settings to it's defaults.
        /// </summary>
        public override void Reset()
        {
            AutoUpdateAssemblyVersion = false;
            AutoUpdateFileVersion = false;

            BuildAction = BuildActionType.Both;

            string versioningStyle = VersioningStyle.GetDefaultGlobalVariable();
            VersioningStyle.FromGlobalVariable(versioningStyle);

            IsUniversalTime = false;
            StartDate = new DateTime(1975, 10, 21);
            ReplaceNonNumerics = true;
            IncrementBeforeBuild = true;
            DetectChanges = true;

            Apply = ApplyGlobalSettings.OnlyWhenChosen;
        }
    }
}
