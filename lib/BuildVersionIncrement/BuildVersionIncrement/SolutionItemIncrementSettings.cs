using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.IO;

namespace BuildVersionIncrement
{
    internal class SolutionItemIncrementSettings : BaseIncrementSettings
    {
        /// <summary>
        /// Loads the settings into this instance.
        /// </summary>
        public override void Load()
        {
            try
            {
                string versioningStyle = GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
                    Resources.GlobalVarName_buildVersioningStyle,
                    VersioningStyle.GetDefaultGlobalVariable());
                VersioningStyle.FromGlobalVariable(versioningStyle);
                AutoUpdateAssemblyVersion = bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_updateAssemblyVersion, "false"));
                AutoUpdateFileVersion = bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_updateFileVersion, "false"));
                try
                {
                    BuildAction = (BuildActionType)Enum.Parse(typeof(BuildActionType), GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_buildAction, "Both"));
                }
                catch (ArgumentException)
                {
                    BuildAction = BuildActionType.Both;
                }
                StartDate = DateTime.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_startDate, "1975/10/21"));
                ReplaceNonNumerics = bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_replaceNonNumerics, "true"));
                IncrementBeforeBuild = bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_incrementBeforeBuild, "true"));
                AssemblyInfoFilename = GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_assemblyInfoFilename, "");
                ConfigurationName = GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_configurationName, "Any");
                UseGlobalSettings = bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_useGlobalSettings, (GlobalIncrementSettings.ApplySettings == GlobalIncrementSettings.ApplyGlobalSettings.AsDefault).ToString()));
                IsUniversalTime = bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_useUniversalClock, "false"));
                DetectChanges = bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_detectChanges, "true"));
            }
            catch (Exception ex)
            {
                Logger.Write("Error occured while reading BuildVersionIncrement settings from \"" + SolutionItem.Filename + "\"\n" + ex.ToString(), LogLevel.Error);
            }
        }

        /// <summary>
        /// Saves the settings of this instance.
        /// </summary>
        public override void Save()
        {
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_buildVersioningStyle,
                                              VersioningStyle.ToGlobalVariable(), VersioningStyle.GetDefaultGlobalVariable());
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_updateAssemblyVersion,
                                              AutoUpdateAssemblyVersion.ToString(), "false");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_updateFileVersion,
                                              AutoUpdateFileVersion.ToString(), "false");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_buildAction,
                                              BuildAction.ToString(), "Both");
            string startDate = string.Format("{0}/{1}/{2}",
                                             StartDate.Year,
                                             StartDate.Month,
                                             StartDate.Day);
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_startDate,
                                              startDate, "1975/10/21");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_replaceNonNumerics,
                                              ReplaceNonNumerics.ToString(), "true");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_incrementBeforeBuild,
                                              IncrementBeforeBuild.ToString(), "true");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_assemblyInfoFilename,
                                              AssemblyInfoFilename, "");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_configurationName,
                                              ConfigurationName, "Any");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_useGlobalSettings,
                                              UseGlobalSettings.ToString(), "false");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_useUniversalClock,
                                              IsUniversalTime.ToString(), "false");
            GlobalVariables.SetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_detectChanges,
                                              DetectChanges.ToString(), "true");
        }

        /// <summary>
        /// Resets the settings to the defaults
        /// </summary>
        public override void Reset()
        {
            string versioningStyle = VersioningStyle.GetDefaultGlobalVariable();
            VersioningStyle.FromGlobalVariable(versioningStyle);
            AutoUpdateAssemblyVersion = false;
            AutoUpdateFileVersion = false;
            BuildAction = BuildActionType.Both;
            StartDate = new DateTime(1975, 10, 21);
            ReplaceNonNumerics = true;
            IncrementBeforeBuild = true;
            AssemblyInfoFilename = String.Empty;
            ConfigurationName = "Any";
            UseGlobalSettings = false;
            IsUniversalTime = false;
            DetectChanges = true;
        }

        /// <summary>
        /// Copies settings from another instance.
        /// </summary>
        /// <param name="source">The source to copy the settings from.</param>
        public override void CopyFrom(BaseIncrementSettings source)
        {
            base.CopyFrom(source);
            if (source.GetType().IsAssignableFrom(typeof(SolutionItemIncrementSettings)))
            {
                SolutionItemIncrementSettings solutionItemSettings = (SolutionItemIncrementSettings)source;
                AssemblyInfoFilename = solutionItemSettings.AssemblyInfoFilename;
                ConfigurationName = solutionItemSettings.ConfigurationName;
                UseGlobalSettings = solutionItemSettings.UseGlobalSettings;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionItemIncrementSettings"/> class.
        /// </summary>
        /// <param name="solutionItem">The solution item.</param>
        public SolutionItemIncrementSettings(SolutionItem solutionItem)
        {
            _solutionItem = solutionItem;
        }

        private SolutionItem _solutionItem;
        /// <summary>
        /// Gets the associated solution item.
        /// </summary>
        /// <value>The solution item.</value>
        [Browsable(false)]
        public SolutionItem SolutionItem
        {
            get { return this._solutionItem; }
        }

		/// <summary>
		/// Gets the filename.
		/// </summary>
		/// <value>The filename.</value>
		[ReadOnly(true)]
		[Category("Project")]
		[DisplayName("Filename")]
		[Description("The project file.")]
		public string Filename
		{
			get { return SolutionItem.Filename; }
		}

#if DEBUG

        /// <summary>
        /// Gets the GUID.
        /// </summary>
        /// <value>The GUID.</value>
        [ReadOnly(true)]
        [Category("Project")]
        [DisplayName("Project Kind")]
        public string Guid
        {
            get { return SolutionItem.Guid; }
        }
#endif

        /// <summary>
        /// Gets the name of the solution item.
        /// </summary>
        /// <value>The name.</value>
        [ReadOnly(true)]
        [Category("Project")]
        [DisplayName("Project Name")]
        [Description("The name of the project.")]
        public string Name
        {
            get { return SolutionItem.Name; }
        }

        private string _assemblyInfoFilename = string.Empty;
        /// <summary>
        /// Gets or sets the assembly info filename.
        /// </summary>
        /// <value>The assembly info filename.</value>
        [Category("Increment Settings")]
        [Description("Use this value if the assembly attributes aren't saved in the default file. " +
                     "You can use this at solution level if you make use of file links in your projects.")]
        [DefaultValue("")]
        [DisplayName("Assembly Info Filename")]
        [EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string AssemblyInfoFilename
        {
            get { return _assemblyInfoFilename; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Make the path relative

                    string basePath = Path.GetDirectoryName(SolutionItem.Filename);
                    _assemblyInfoFilename = Common.MakeRelativePath(basePath, value);
                }
                else
                    _assemblyInfoFilename = string.Empty;
            }
        }

        private bool _useGlobalSettings;
        /// <summary>
        /// Gets or sets if this project should use the global settings instead of it's own.
        /// </summary>
        /// <value>The value</value>
        [Category("Increment Settings")]
        [Description("If the project should use the global settings instead of it's own.")]
        [DisplayName("Use Global Settings")]
        [DefaultValue(false)]
        public bool UseGlobalSettings
        {
            get { return _useGlobalSettings; }
            set { _useGlobalSettings = value; }
        }

        private string _configurationName = "Any";
        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        /// <value>The name of the configuration.</value>
        [Category("Condition")]
        [DefaultValue("Any")]
        [DisplayName("Configuration Name")]
        [Description("Set this to the name to of the configuration when the auto update should occur.")]
        [TypeConverter(typeof(ConfigurationStringConverter))]
        public string ConfigurationName
        {
            get { return _configurationName; }
            set { _configurationName = value; }
        }
    }
}
