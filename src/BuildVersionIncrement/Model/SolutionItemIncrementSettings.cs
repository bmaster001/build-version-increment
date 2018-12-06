// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: SolutionItemIncrementSettings.cs
// ----------------------------------------------------------------------
// Created and maintained by Paul J. Melia.
// Copyright © 2016 Paul J. Melia.
// All rights reserved.
// ----------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR 
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// ----------------------------------------------------------------------

namespace BuildVersionIncrement.Model
{
	using System;
	using System.ComponentModel;
	using System.Drawing.Design;
	using System.IO;
	using System.Windows.Forms.Design;

	using Logging;
	using Microsoft.VisualStudio.Shell;
	using Properties;

	using UI;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	internal class SolutionItemIncrementSettings : IncrementSettingsBase
	{
		private string _assemblyInfoFilename = string.Empty;

		public SolutionItemIncrementSettings(SolutionItem solutionItem)
		{
			SolutionItem = solutionItem;
		}

		[Category("Increment Settings")]
		[Description(
			"Use this value if the assembly attributes aren't saved in the default file. "
			+ "You can use this at solution level if you make use of file links in your projects.")]
		[DefaultValue("")]
		[DisplayName("Assembly Info Filename")]
		[Editor(typeof(FilePickerEditor), typeof(FilePickerEditor))]
		public string AssemblyInfoFilename
		{
			get { return _assemblyInfoFilename; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					var basePath = Path.GetDirectoryName(SolutionItem.Filename);
					_assemblyInfoFilename = Common.MakeRelativePath(basePath, value);
				}
				else
				{
					_assemblyInfoFilename = string.Empty;
				}
			}
		}

		[Category("Condition")]
		[DefaultValue("Any")]
		[DisplayName("Configuration Name")]
		[Description("Set this to the name to of the configuration when the auto update should occur.")]
		[TypeConverter(typeof(ConfigurationStringConverter))]
		public string ConfigurationName { get; set; } = "Any";

		[ReadOnly(true)]
		[Category("Project")]
		[DisplayName("Filename")]
		[Description("The project file.")]
		public string Filename => SolutionItem.Filename;

#if DEBUG

		[ReadOnly(true)]
		[Category("Project")]
		[DisplayName("Project Kind")]
		[ItemsSource(typeof(ProjectKindItemsSource))]
		public string Guid
		{
			get
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				return SolutionItem.Guid;
			}
		}
#endif

		[ReadOnly(true)]
		[Category("Project")]
		[DisplayName("Project Name")]
		[Description("The name of the project.")]
		public string Name => SolutionItem.Name;

		[Browsable(false)]
		public SolutionItem SolutionItem { get; }

		[Category("Increment Settings")]
		[Description("If the project should use the global settings instead of it's own.")]
		[DisplayName("Use Global Settings")]
		[DefaultValue(false)]
		public bool UseGlobalSettings { get; set; }

		public override void CopyFrom(IncrementSettingsBase source)
		{
			base.CopyFrom(source);
			if (!source.GetType().IsAssignableFrom(typeof(SolutionItemIncrementSettings)))
			{
				return;
			}
			var solutionItemSettings = (SolutionItemIncrementSettings)source;
			AssemblyInfoFilename = solutionItemSettings.AssemblyInfoFilename;
			ConfigurationName = solutionItemSettings.ConfigurationName;
			UseGlobalSettings = solutionItemSettings.UseGlobalSettings;
		}

		public override void Load()
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			try
			{
				var versioningStyle = GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
				                                                        Resources
					                                                        .GlobalVarName_buildVersioningStyle,
				                                                        VersioningStyle.GetDefaultGlobalVariable
					                                                        ());
				VersioningStyle.FromGlobalVariable(versioningStyle);
				AutoUpdateAssemblyVersion =
					bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                             Resources.GlobalVarName_updateAssemblyVersion,
					                                             "false"));
				AutoUpdateFileVersion =
					bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                             Resources.GlobalVarName_updateFileVersion,
					                                             "false"));
				try
				{
					BuildAction =
						(BuildActionType)
						Enum.Parse(typeof(BuildActionType),
						           GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
						                                             Resources.GlobalVarName_buildAction,
						                                             "Both"));
				}
				catch (ArgumentException)
				{
					BuildAction = BuildActionType.Both;
				}
				StartDate =
					DateTime.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                                 Resources.GlobalVarName_startDate,
					                                                 "2000/01/01"));
				ReplaceNonNumerics =
					bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                             Resources.GlobalVarName_replaceNonNumerics,
					                                             "true"));
				IncrementBeforeBuild =
					bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                             Resources.GlobalVarName_incrementBeforeBuild,
					                                             "true"));
				AssemblyInfoFilename = GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
				                                                         Resources
					                                                         .GlobalVarName_assemblyInfoFilename,
				                                                         "");
				ConfigurationName = GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
				                                                      Resources.GlobalVarName_configurationName,
				                                                      "Any");
				UseGlobalSettings =
					bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                             Resources.GlobalVarName_useGlobalSettings,
					                                             (GlobalIncrementSettings.ApplySettings
					                                              == GlobalIncrementSettings.ApplyGlobalSettings
					                                                                        .AsDefault).ToString()));
				IsUniversalTime =
					bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                             Resources.GlobalVarName_useUniversalClock,
					                                             "false"));
				DetectChanges =
					bool.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals,
					                                             Resources.GlobalVarName_detectChanges,
					                                             "true"));
			}
			catch (Exception ex)
			{
				Logger.Write(
					$"Error occured while reading BuildVersionIncrement settings from \"{SolutionItem.Filename}\"\n{ex}",
					LogLevel.Error);
			}
		}

		public override void Reset()
		{
			var versioningStyle = VersioningStyle.GetDefaultGlobalVariable();
			VersioningStyle.FromGlobalVariable(versioningStyle);
			AutoUpdateAssemblyVersion = false;
			AutoUpdateFileVersion = false;
			BuildAction = BuildActionType.Both;
			StartDate = new DateTime(2000, 1, 1);
			ReplaceNonNumerics = true;
			IncrementBeforeBuild = true;
			AssemblyInfoFilename = String.Empty;
			ConfigurationName = "Any";
			UseGlobalSettings = false;
			IsUniversalTime = false;
			DetectChanges = true;
		}

		public override void Save()
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_buildVersioningStyle,
			                                  VersioningStyle.ToGlobalVariable(),
			                                  VersioningStyle.GetDefaultGlobalVariable());
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_updateAssemblyVersion,
			                                  AutoUpdateAssemblyVersion.ToString(),
			                                  "false");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_updateFileVersion,
			                                  AutoUpdateFileVersion.ToString(),
			                                  "false");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_buildAction,
			                                  BuildAction.ToString(),
			                                  "Both");
			var startDate = $"{StartDate.Year}/{StartDate.Month}/{StartDate.Day}";
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_startDate,
			                                  startDate,
			                                  "2000/01/01");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_replaceNonNumerics,
			                                  ReplaceNonNumerics.ToString(),
			                                  "true");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_incrementBeforeBuild,
			                                  IncrementBeforeBuild.ToString(),
			                                  "true");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_assemblyInfoFilename,
			                                  AssemblyInfoFilename,
			                                  "");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_configurationName,
			                                  ConfigurationName,
			                                  "Any");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_useGlobalSettings,
			                                  UseGlobalSettings.ToString(),
			                                  "false");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_useUniversalClock,
			                                  IsUniversalTime.ToString(),
			                                  "false");
			GlobalVariables.SetGlobalVariable(SolutionItem.Globals,
			                                  Resources.GlobalVarName_detectChanges,
			                                  DetectChanges.ToString(),
			                                  "true");
		}
	}
}