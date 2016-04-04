#region Using directives
using BuildVersionIncrement.Incrementors;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace BuildVersionIncrement
{
    internal class BuildVersionIncrementor
    {
        private Connect _connect;
        //private DateTime _startDate = new DateTime(1975, 10, 21);
        //private string _currentBuildVersion;
        private DateTime _buildStartDate = DateTime.MinValue;

        private IncrementorCollection _incrementors = new IncrementorCollection();
        /// <summary>
        /// Gets the incrementors.
        /// </summary>
        /// <value>The incrementors.</value>
        public IncrementorCollection Incrementors
        {
            get { return _incrementors; }
        }

        private static BuildVersionIncrementor _instance;
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static BuildVersionIncrementor Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionIncrementer"/> class.
        /// </summary>
        /// <param name="connect">The connect.</param>
        public BuildVersionIncrementor(Connect connect)
        {
            _connect = connect;
            _instance = this;
        }

        /// <summary>
        /// Initializes the incrementors.
        /// </summary>
        public void InitializeIncrementors()
        {
            try
            {
                _incrementors.AddFrom(Assembly.GetExecutingAssembly());

                string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.Incrementor.dll");

                foreach (string file in files)
                {
                    Logger.Write("Loading incrementors from \"" + file + "\".", LogLevel.Debug);

                    Assembly asm = Assembly.LoadFrom(file);

                    _incrementors.AddFrom(asm);
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Exception occured while initializing incrementors.\n" + ex.ToString(), LogLevel.Error);
            }
        }

        /// <summary>
        /// Because there is not method I could use to ensure a property 
        /// exists we need manually search for it
        /// </summary>
        /// <param name="properties">Properties colection</param>
        /// <param name="propertyName">Property name which we need to find</param>
        /// <returns>true if property is found, otherwise false</returns>
        private bool PropertyExists(Properties properties, string propertyName)
        {
            foreach (Property item in properties)
            {
                if (item.Name == propertyName)
                    return true;
            }
            return false;
        } // PropertyExists

        private static Dictionary<string, DateTime> _fileDateCache = new Dictionary<string, DateTime>();

        /// <summary>
        /// My IDE hangs for few secs at build startup I think this
        /// cache will help
        /// </summary>
        private static DateTime GetCachedFileDate(string outputFileName, string fullPath)
        {
            string path = Path.Combine(fullPath, outputFileName);
			DateTime fileDate;

			if (_fileDateCache.ContainsKey(path))
				fileDate = _fileDateCache[path];
			else
			{
				fileDate = File.GetLastWriteTime(path);
				_fileDateCache.Add(path, fileDate);
			}

        	Logger.Write(String.Format("Last Build:{1} ({0})", path, fileDate), LogLevel.Debug);

			return fileDate;
        } // GetCachedDate

        /// <summary>
        /// Clear file date cache
        /// </summary>
        private static void ClearSolutionItemAndFileDateCache()
        {
            Logger.Write("Clearing date and solution cache", LogLevel.Debug);
            _fileDateCache.Clear();
            _solutionItemCache.Clear();
        } // ClearFileDateCache

        /// <summary>
        /// Check filesystem item
        /// </summary>
        private bool CheckFilesystemItem(string localPath, string itemName)
        {
            FileAttributes attributes = File.GetAttributes(localPath);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (!Directory.Exists(localPath))
                {
                    Logger.Write(String.Format(" Directory '{0}' was not found - assuming a clean build was made", itemName), LogLevel.Debug);
                    return true;
                }
            }
            else
            {
                if (!File.Exists(localPath))
                {
                    Logger.Write(String.Format(" File '{0}' was not found - assuming a clean build was made", itemName), LogLevel.Debug);
                    return true;
                }
            }
            return false;
        } // CheckFilesystemItem

        /// <summary>
        /// Method returns true if project item was modified
        /// </summary>
        private bool CheckProjectItem(ProjectItem item, DateTime outputFileDate)
        {
			// If a file does not exist, as soon as we access DateModified property's Value,
			// an exception will be thrown. So we check if the file exists, and if it doesn't
			// just return false...
            DateTime itemFileDate = DateTime.MinValue;
            if (PropertyExists(item.Properties, "LocalPath"))
			{
				Property localPathProp = item.Properties.Item("LocalPath");
				string localPath = localPathProp.Value.ToString();
                if (CheckFilesystemItem(localPath, item.Name))
                    return true;
                if (PropertyExists(item.Properties, "DateModified"))
                {
                    Property dateModifiedProp = item.Properties.Item("DateModified");
                    string itemDateString = dateModifiedProp.Value.ToString();
                    // Those two try blocks should fix
                    // reported issue #6289
                    try
                    {
                        // Note: Unfortunatelly i do not know if VS is storing those dates
                        // in a specific format. Right now I'm just convering two scenarios
                        // With current culture and the invariant one
                        // Please fix this if you find more precise information
                        itemFileDate = DateTime.Parse(itemDateString);
                    }
                    catch
                    {
                        try
                        {
                            itemFileDate = DateTime.Parse(itemDateString, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Logger.Write(String.Format("Cannot parse current item's date '{0}'", itemFileDate), LogLevel.Warning);
                        }
                    }
                } // if

			}
            else
                if (PropertyExists(item.Properties, "FullPath"))
                {
                    Property localPathProp = item.Properties.Item("FullPath");
                    string localPath = localPathProp.Value.ToString();
                    if (CheckFilesystemItem(localPath, item.Name))
                        return true;
                    itemFileDate = File.GetLastWriteTime(localPath);
                }
            if (itemFileDate > outputFileDate)
                return true;
            return false;
        } // CheckProjectItem

        /// <summary>
        /// Checks whatever project is modified or not.
        /// </summary>
        /// <param name="project">Project instance</param>
        /// <returns>true when project is modified and false otherwise</returns>
        private bool IsProjectModified(Project project, LanguageType language)
        {
            try
            {
                Logger.Write(String.Format("Checking project '{0}'...", project.Name), LogLevel.Debug);
				string outputFileName, fullPath;
                try
                {
                    Configuration activeConfiguration = project.ConfigurationManager.ActiveConfiguration;
                    outputFileName = project.Properties.Item("OutputFileName").Value.ToString();
                    fullPath = project.Properties.Item("FullPath").Value.ToString();
                    fullPath = Path.Combine(fullPath, activeConfiguration.Properties.Item("OutputPath").Value.ToString());
                }
                catch
                {
                    try
                    {
                        // If we are here, this might very well be a VCProject where output file name is
                        // handled a bit differently, but as it is still a guess, we need another try block...
                        object prj = project.Properties.Item("project").Object;
                        object configurations =
                            prj.GetType().InvokeMember("Configurations",
                                                        BindingFlags.GetProperty, null,
                                                        prj, null);
                        object cfg =
                            configurations.GetType().InvokeMember("Item",
                                                                    BindingFlags.InvokeMethod, null, configurations,
                                                                    new object[] { 1 });
                        string fullPathToOutputFile = "";
                        if (cfg != null)
                        {
                            fullPathToOutputFile =
                                (string)cfg.GetType().InvokeMember("PrimaryOutput",
                                                                    BindingFlags.GetProperty, null, cfg, null);
                        }
                        outputFileName = Path.GetFileName(fullPathToOutputFile);
                        fullPath = Path.GetDirectoryName(fullPathToOutputFile);
                        if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                            fullPath += Path.DirectorySeparatorChar;
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(String.Format("Could not get project output file date: {0}. Assumming file is modified.", ex.Message), LogLevel.Warning);
                        return true;
                    }
                }
    
                DateTime outputFileDate = GetCachedFileDate(outputFileName, fullPath);
                foreach (ProjectItem item in project.ProjectItems)
                {
                    // PhysicalFolder
                    // VirtualFolder
                    if (item.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}" ||
                        item.Kind == "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}")
                    {
                        foreach (ProjectItem innerItem in item.ProjectItems)
                        {
                            if (CheckProjectItem(innerItem, outputFileDate))
                            {
                                Logger.Write(String.Format("Project's ('{0}') item '{1}' is modified. Version will be updated.", project.Name, item.Name), LogLevel.Debug);
                                return true;
                            }
                        }
                    }
                    else if (CheckProjectItem(item, outputFileDate))
                    {
                        Logger.Write(String.Format("Project's ('{0}') item '{1}' is modified. Version will be updated.", project.Name, item.Name), LogLevel.Debug);
                        return true;
                    }
                }
                Logger.Write(String.Format("Project '{0}' is not modified", project.Name), LogLevel.Debug);
                return false;
            }
            catch (Exception ex)
            {
				Logger.Write(String.Format("Could not check if project were modified because: {0}. Assumming file is modified.", ex.Message), LogLevel.Warning);
                Logger.Write(ex.ToString(), LogLevel.Debug);
				return true;
            }
        }

        /// <summary>
        /// This cache contains information about
        /// modified items to speed up the lookup process
        /// </summary>
        private static Dictionary<string, bool> _solutionItemCache = 
            new Dictionary<string,bool>();

        /// <summary>
        /// Checks whatever solution item is modified or not
        /// </summary>
        /// <param name="solutionItem">Solution item which need to be checked</param>
        /// <returns>true when solution item is modified and false otherwise</returns>
        private bool IsSolutionItemModified(SolutionItem solutionItem)
        {
            string key = String.Format("{0}:{1}", solutionItem.ItemType, solutionItem.Name);
            if (_solutionItemCache.ContainsKey(key))
            {
                bool result = _solutionItemCache[key];
                return result;
            }

            // If not detect changes was set
            // we need to mark current item as modified
            if (!solutionItem.IncrementSettings.DetectChanges)
            {
                Logger.Write(String.Format("Detect changes disabled. Mark item '{0}' as modified.", solutionItem.Name), LogLevel.Debug);
                _solutionItemCache.Add(key, true);
                return true;
            }

            PrepareSolutionItem(solutionItem);
            switch (solutionItem.ItemType)
            {
                case SolutionItemType.Project:
                    {
                        bool result = IsProjectModified(solutionItem.Project, solutionItem.ProjectType);
                        _solutionItemCache.Add(String.Format("{0}:{1}", solutionItem.ItemType, solutionItem.Name), result);
                        return result;
                    }
                case SolutionItemType.Folder:
                case SolutionItemType.Solution:
                    {
                        bool result = false;
                        foreach (SolutionItem subItem in solutionItem.SubItems)
                        {
                            if (subItem.ItemType == SolutionItemType.Project)
                            {
                                result = IsProjectModified(subItem.Project, subItem.ProjectType);
                                _solutionItemCache.Add(String.Format("{0}:{1}", subItem.ItemType, subItem.Name), result);
                            }
                            else if (subItem.ItemType == SolutionItemType.Folder)
                                result = IsSolutionItemModified(subItem);
                            if (result)
                                break;
                        }
                        Logger.Write(String.Format("Solution/Folder '{0}' is not modified", solutionItem.Name), LogLevel.Debug);
                        _solutionItemCache.Add(key, result);
                        return result;
                    }
            }

            // Unknown items are for us modified so this will launch std
            // behavior of BVI
            Logger.Write(String.Format("Solution item '{0}' is not supported. Run standard behavior (is modified).", solutionItem.ItemType), LogLevel.Warning);
            _solutionItemCache.Add(key, true);
            return true;
        }

        /// <summary>
        /// Execute increment
        /// </summary>
        private void ExecuteIncrement()
        {
            if (!GlobalAddinSettings.Default.IsEnabled)
            {
                Logger.Write("BuildVersionIncrement disabled.", LogLevel.Info);
                return;
            }
            try
            {
                if (_currentBuildAction == vsBuildAction.vsBuildActionBuild ||
                    _currentBuildAction == vsBuildAction.vsBuildActionRebuildAll)
                {
                    if (_currentBuildScope == vsBuildScope.vsBuildScopeSolution)
                    {
                        // Solution level ...

                        Solution solution = _connect.ApplicationObject.Solution;
                        SolutionItem solutionItem = new SolutionItem(_connect, solution, true);
                        UpdateRecursive(solutionItem);
                    }
                    else
                    {
                        // Grab the active project and check all depencies
                        Array projects = (Array)_connect.ApplicationObject.ActiveSolutionProjects;
                        foreach (Project p in projects)
                        {
                            SolutionItem solutionItem = SolutionItem.ConstructSolutionItem(_connect, p, false);

                            if (solutionItem != null)
                                // Do not take any action if current solution item is
                                // not modified - just do not waste time for unchanged projects
                                if (IsSolutionItemModified(solutionItem))
                                    UpdateProject(solutionItem);
                        } // foreach
                    } // else

                    Logger.Write(string.Format("{0}-build process : Completed",
                        _currentBuildState == vsBuildState.vsBuildStateInProgress ? "Pre" : "Post"), LogLevel.Info);
                } // if
            } // try
            catch (Exception ex)
            {
                Logger.Write("Error occured while executing build version increment.\n" + ex.ToString(), LogLevel.Error);
            }
        } // ExecuteIncrement

        /// <summary>
        /// Updates all build versions for the entire solution
        /// </summary>
        private void UpdateRecursive(SolutionItem solutionItem)
        {
            try
            {
                // Do not take any action if current solution item is
                // not modified - just do not waste time for unchanged projects
                if (!IsSolutionItemModified(solutionItem))
                    return;
                if (solutionItem.IncrementSettings.UseGlobalSettings)
                    solutionItem.ApplyGlobalSettings();
                if (ActiveConfigurationMatch(solutionItem))
                {
					if (solutionItem.IncrementSettings.AutoUpdateAssemblyVersion)
						Update(solutionItem, "AssemblyVersion");
					if (solutionItem.IncrementSettings.AutoUpdateFileVersion)
						Update(solutionItem, "AssemblyFileVersion");
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.ToString(), LogLevel.Error);
            }
            foreach (SolutionItem child in solutionItem.SubItems)
                UpdateRecursive(child);
        }

        private bool ActiveConfigurationMatch(SolutionItem solutionItem)
        {
            try
            {
                if (solutionItem.ItemType == SolutionItemType.Folder)
                    return false;

                string activeConfigName;

                if (solutionItem.ItemType == SolutionItemType.Solution)
                    activeConfigName = solutionItem.Solution.SolutionBuild.ActiveConfiguration.Name;
                else
                    activeConfigName = solutionItem.Project.ConfigurationManager.ActiveConfiguration.ConfigurationName;

                if (solutionItem.IncrementSettings.ConfigurationName == "Any" ||
                    solutionItem.IncrementSettings.ConfigurationName == activeConfigName)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (!solutionItem.UniqueName.EndsWith("contentproj")) // XNA content project
                    Logger.Write("Couldn't get the active configuration name for \"" + solutionItem.UniqueName + "\": \"" + ex.Message + "\"\nSkipping ...", LogLevel.Info);
            }

            return false;
        }

        /// <summary>
        /// Updates build version for the given project and it's dependencies.
        /// </summary>
        /// <param name="solutionItem">The solution item.</param>
        private void UpdateProject(SolutionItem solutionItem)
        {
            //if we should apply them, we will...I don't care if it is a solution or a project
            if (GlobalIncrementSettings.ApplySettings == GlobalIncrementSettings.ApplyGlobalSettings.Always || solutionItem.IncrementSettings.UseGlobalSettings)
                solutionItem.ApplyGlobalSettings();

            if (_updatedItems.ContainsKey(solutionItem.UniqueName))
                return;

            if (ActiveConfigurationMatch(solutionItem))
            {
                if (solutionItem.IncrementSettings.AutoUpdateAssemblyVersion)
                    Update(solutionItem, "AssemblyVersion");

                if (solutionItem.IncrementSettings.AutoUpdateFileVersion)
                    Update(solutionItem, "AssemblyFileVersion");
            }

            try
            {
                if (solutionItem.BuildDependency != null)
                {
                    object[] references = (object[])solutionItem.BuildDependency.RequiredProjects;

                    foreach (object o in references)
                    {
                        SolutionItem dep = SolutionItem.ConstructSolutionItem(_connect, (Project)o, false);

                        if (dep != null)
                        {
                            try
                            {
                                UpdateProject(dep);
                            }
                            catch (Exception ex)
                            {
                                Logger.Write("Exception occured while updating project dependency \"" + dep.UniqueName + "\" for \"" + solutionItem.UniqueName + "\".\n" + ex.Message, LogLevel.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Failed updating dependencies for \"" + solutionItem.UniqueName + "\".\n" + ex.Message, LogLevel.Error);
            }

            _updatedItems.Add(solutionItem.UniqueName, solutionItem);
        }

        private void Update(SolutionItem solutionItem, string attribute)
        {
            // okay, let's check the build action
            if (solutionItem.IncrementSettings.BuildAction == BuildActionType.Both ||
                (solutionItem.IncrementSettings.BuildAction == BuildActionType.Build && _currentBuildAction == vsBuildAction.vsBuildActionBuild) ||
                (solutionItem.IncrementSettings.BuildAction == BuildActionType.ReBuild && _currentBuildAction == vsBuildAction.vsBuildActionRebuildAll))
            {
                //now we'll check if we're in the right mood...ahhh...event.
                //Okay, this is a little hack, let me explain...both need to be false, or true to allow incrementation.
                if ((solutionItem.IncrementSettings.IncrementBeforeBuild) == (_currentBuildState == vsBuildState.vsBuildStateInProgress))
                {
                    Logger.Write("Updating attribute " + attribute + " of project " + solutionItem.Name, LogLevel.Debug);

                    string filename = GetAssemblyInfoFilename(solutionItem);

                    if (filename != null && File.Exists(filename)) // && HandleSourceControl(solutionItem, filename))
                    {
                        switch (solutionItem.ProjectType)
                        {
                            case LanguageType.CSharp:
                            case LanguageType.VisualBasic:
                            case LanguageType.CPPManaged:
                                // Patch 4812 from Haukinger, first line is original pattern, second is my edited one.
                                //string pattern = @"^[\\[<]assembly:\\W*" + attribute + @"(Attribute)?\\W*\\(\\W*\\""(?<FullVersion>\\S+\\.\\S+(\\.(?<Version>[^\\""]+))?)\\""\\W*\\)[\\]>]";
                                UpdateVersion(solutionItem, "^[\\[<]assembly:\\s*" + attribute + "(Attribute)?\\s*\\(\\s*\"(?<FullVersion>\\S+\\.\\S+(\\.(?<Version>[^\"]+))?)\"\\s*\\)[\\]>]", filename, attribute);
                                break;
                            case LanguageType.CPPUnmanaged:
                                if (attribute == "AssemblyVersion")
                                    attribute = "ProductVersion";
                                if (attribute == "AssemblyFileVersion")
                                    attribute = "FileVersion";

                                UpdateVersion(solutionItem, "^[\\s]*VALUE\\ \"" + attribute + "\",\\ \"(?<FullVersion>\\S+[.,\\s]+\\S+[.,\\s]+\\S+[.,\\s]+[^\\s\"]+)\"", filename, attribute);
                                UpdateVersion(solutionItem, "^[\\s]*" + attribute.ToUpper() + "\\ (?<FullVersion>\\S+[.,]+\\S+[.,]+\\S+[.,]+\\S+)", filename, attribute.ToUpper());
                                break;
                            case LanguageType.None:
                                // wtf?
                                break;

                        }
                    }
                }
            }
        }

        private void UpdateVersion(SolutionItem solutionItem, string regexPattern, string assemblyFile, string debugAttribute)
        {
            string filecontent = File.ReadAllText(assemblyFile);

            try
            {
                RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase;

                Match m = Regex.Match(filecontent, regexPattern, options);
                if (!m.Success)
                {
                    Logger.Write("Failed to locate attribute \"" + debugAttribute + "\" in file \"" + assemblyFile + "\".", LogLevel.Error, assemblyFile, 1);
                    return;
                }

                Match sep = Regex.Match(m.Groups["FullVersion"].Value, "(?<Separator>[\\s,.]+)", options);
                if (!sep.Success)
                {
                    Logger.Write("Failed to fetch version separator on attribute \"" + debugAttribute + "\" in file \"" + assemblyFile + "\".", LogLevel.Error, assemblyFile, 2);
                    return;
                }

                StringVersion currentVersion = null;

                try
                {
                    // Make sure we don't have anything what is not a number in the version
                    currentVersion = new StringVersion(Regex.Replace(m.Groups["FullVersion"].Value, "[^\\d" + sep.Groups["Separator"].Value + "]+", "0").Replace(sep.Groups["Separator"].Value, "."));
                }
                catch (Exception ex)
                {
                    string msg = string.Format("Error occured while parsing value of {0} ({1}).\n{2}",
                                               debugAttribute, m.Groups["FullVersion"].Value, ex);

                    throw (new Exception(msg, ex));
                }

                StringVersion newVersion = solutionItem.IncrementSettings.VersioningStyle.Increment(currentVersion,
                    solutionItem.IncrementSettings.IsUniversalTime ? _buildStartDate.ToUniversalTime() : _buildStartDate,
                    solutionItem.IncrementSettings.StartDate,
                    solutionItem);

                if (newVersion != currentVersion)
                {
                    bool success = false;

                    if (_connect.IsCommandLineBuild)
                    {
                        filecontent = filecontent.Remove(m.Groups["FullVersion"].Index, m.Groups["FullVersion"].Length);
                        filecontent = filecontent.Insert(m.Groups["FullVersion"].Index, newVersion.ToString());

                        try
                        {
                            File.WriteAllText(assemblyFile, filecontent);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(ex.Message, LogLevel.Warning);
							success = false;
                        }
                    }
                    else
                    {
                        bool doCloseWindow = !solutionItem.DTE.ItemOperations.IsFileOpen(assemblyFile, null);

                        string replaceWith = string.Empty;

                        if (!solutionItem.IncrementSettings.ReplaceNonNumerics && Regex.IsMatch(m.Groups["FullVersion"].Value, "[^\\d" + sep.Groups["Separator"].Value + "]+"))
                        {
                            //the version number is not only pure numbers...Thread 77828 from philjones88
                            string[] mergedVersion = m.Groups["FullVersion"].Value.Replace(sep.Groups["Separator"].Value, ".").Split('.');

                            if (Regex.IsMatch(mergedVersion[0], "[\\d]+"))
                                mergedVersion[0] = newVersion.Major;
                            if (Regex.IsMatch(mergedVersion[1], "[\\d]+"))
                                mergedVersion[1] = newVersion.Minor;
                            if (Regex.IsMatch(mergedVersion[2], "[\\d]+"))
                                mergedVersion[2] = newVersion.Build;
                            if (Regex.IsMatch(mergedVersion[3], "[\\d]+"))
                                mergedVersion[3] = newVersion.Revision;

                            replaceWith = m.Value.Replace(m.Groups["FullVersion"].Value, String.Format("{0}.{1}.{2}.{3}", mergedVersion).Replace(".", sep.Groups["Separator"].Value));
                        }
                        else
                        {
                            replaceWith = m.Value.Replace(m.Groups["FullVersion"].Value, newVersion.ToString(4).Replace(".", sep.Groups["Separator"].Value));
                        }

                        ProjectItem projectItem = _connect.ApplicationObject.Solution.FindProjectItem(assemblyFile);

						if (projectItem == null)
							throw (new ApplicationException("Failed to find project item \"" + assemblyFile + "\"."));

                    	Window window = projectItem.Open(Constants.vsViewKindTextView);

						if (window == null)
							throw (new ApplicationException("Could not open project item."));

                    	Document doc = window.Document;

						if (doc == null)
							throw (new ApplicationException("Located project item & window but no document."));

						success = doc.ReplaceText(m.Value, replaceWith, 0);

                        if (doCloseWindow)
                            window.Close(vsSaveChanges.vsSaveChangesYes);
                        else
                            doc.Save(assemblyFile);
                    }
#if OLDSTUFF
                                        filecontent = filecontent.Remove(m.Groups["FullVersion"].Index, m.Groups["FullVersion"].Length);
                                        filecontent = filecontent.Insert(m.Groups["FullVersion"].Index, newVersion.ToString());

                                        success = WriteFileContent(filename, filecontent);
#endif

					string msg = solutionItem.Name + " " + debugAttribute + ": " + newVersion;
					if (success)
						msg += " [SUCCESS]";
					else
						msg += " [FAILED]";
					Logger.Write(msg, LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error occured while updating version.\n" + ex.ToString(), LogLevel.Error, assemblyFile, 1);
            }
        }

#if OLD
        /// <summary>
        /// Creates the build version.
        /// </summary>
        /// <param name="solutionItem">The solution item.</param>
        /// <param name="currentVersion">The current version as found in the assembly info.</param>
        /// <returns>The new build version</returns>
        private Version CreateBuildVersion(SolutionItem solutionItem, Version currentVersion)
        {
            //OLD_BuildVersioningStyleType buildVersioningStyleType = solutionItem.BuildVersioningStyle;
            DateTime startDate = solutionItem.StartDate;
            string dayOfyear = _buildStartDate.DayOfYear.ToString("000");
            string yearDecade = _buildStartDate.ToString("yy");

            int build = currentVersion.Build >= 0 ? currentVersion.Build : 0;
            int revision = currentVersion.Revision;

            switch (buildVersioningStyleType)
            {
                case OLD_BuildVersioningStyleType.DeltaBaseDate:
                    // this is the original style
                    TimeSpan ts = _buildStartDate.Subtract(startDate);
                    DateTime dt = DateTime.MinValue + ts;

                    build = Int32.Parse(string.Format("{0}{1:00}", dt.Year * 12 + dt.Month, dt.Day));
                    
                    // Timestamp
                    revision = Int32.Parse(string.Format("{0:00}{1:00}", dt.Hour, dt.Minute));
                    break;

                case OLD_BuildVersioningStyleType.YearDayOfYear_Timestamp:

                    // m.n.9021.2106  <= 1/21/2009 21:16

                    build = Int32.Parse(string.Format("{0}{1:000}", yearDecade, dayOfyear));
                    revision = Int32.Parse(string.Format("{0:HHmm}", _buildStartDate));
                    break;

                case OLD_BuildVersioningStyleType.YearDayOfYear_AutoIncrement:
                    build = int.Parse(string.Format("{0}{1:000}", yearDecade, dayOfyear));

                    if (build == currentVersion.Build)
                        revision = currentVersion.Revision + 1;
                    else
                        revision = 0;
                    break;

                case OLD_BuildVersioningStyleType.DeltaBaseYear:
                    int baseYear = startDate.Year;
                    int deltaYears = _buildStartDate.Year - baseYear;

                    build = Int32.Parse(string.Format("{0}{1:000}", deltaYears, dayOfyear));
                    revision = Int32.Parse(string.Format("{0:HHmm}", _buildStartDate));
                    break;

                case OLD_BuildVersioningStyleType.AutoIncrementBuildVersion:
                    build += 1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("buildVersioningStyleType", buildVersioningStyleType, "The build Versioning Style was not recognized.");
            }

            if (revision >= 0)
                return new Version(currentVersion.Major, currentVersion.Minor, build, revision);
            else
                return new Version(currentVersion.Major, currentVersion.Minor, build);
        }


        

#endif

#if OLD

        private bool HandleSourceControl(SolutionItem solutionItem, string filename)
        {
            if (solutionItem.DTE != null)
            {
                SourceControl sc = solutionItem.DTE.SourceControl;

                if (sc != null && sc.IsItemUnderSCC(filename) && !sc.IsItemCheckedOut(filename))
                {
                    _connect.Log("The file \"" + filename + "\" is under source control. Checking out ...", LogLevel.Info);

                    if (!sc.CheckOutItem(filename))
                    {
                        _connect.Log("Failed to check out \"" + filename + "\".", LogLevel.Error, filename, 1);
                        return false;
                    }
                }
            }

            return true;
        }

#endif

        private LanguageType GetLanguageType(string fileName)
        {
            switch (Path.GetExtension(fileName))
            {
                case ".cs": return LanguageType.CSharp;
                case ".vb": return LanguageType.VisualBasic;
                case ".cpp": return LanguageType.CPPManaged;
                default: return LanguageType.None;
            }
        }

        /// <summary>
        /// Because some part of the code must have the ability to react
        /// on language type this method should prepare used items
        /// as soon as it it possible to make it available
        /// </summary>
        private void PrepareSolutionItem(SolutionItem solutionItem)
        {
            // Do not update if there's already one
            if (solutionItem.ProjectType != LanguageType.None)
                return;
            string extension = Path.GetExtension(solutionItem.Filename);
            switch (extension)
            {
                case ".vbproj":
                    solutionItem.ProjectType = LanguageType.VisualBasic;
                    break;
                case ".vcproj":
                case ".vcxproj":
                    solutionItem.ProjectType = LanguageType.CPPManaged;
                    break;
                case ".csproj":
                    solutionItem.ProjectType = LanguageType.CSharp;
                    break;
            }
            ProjectItem assemblyInfo = solutionItem.FindProjectItem("AssemblyInfo.cpp");
            if (assemblyInfo == null)
            {
                // we couldn't locate the file, but, if it is a C-Project, is also could be an unmanaged project
                if (extension == ".vcproj" || extension == ".vcxproj")
                    solutionItem.ProjectType = LanguageType.CPPUnmanaged;
            }
        }

        private string GetAssemblyInfoFilename(SolutionItem solutionItem)
        {
            string filename = "AssemblyInfo";
            string ext = Path.GetExtension(solutionItem.Filename);

            solutionItem.ProjectType = LanguageType.None;

            switch (ext)
            {
                case ".vbproj":
                    filename += ".vb";
                    solutionItem.ProjectType = LanguageType.VisualBasic;
                    break;

				case ".vcproj":
				case ".vcxproj":
					filename += ".cpp";
                    solutionItem.ProjectType = LanguageType.CPPManaged;
                    break;

                case ".csproj":
                    filename += ".cs";
                    solutionItem.ProjectType = LanguageType.CSharp;
                    break;

                case ".sln":
                    if (string.IsNullOrEmpty(solutionItem.IncrementSettings.AssemblyInfoFilename))
                    {
                        Logger.Write("Can't update build version for a solution without specifying an assembly info file.", LogLevel.Error, solutionItem.Filename, 1);
                        return null;
                    }

                    solutionItem.ProjectType = GetLanguageType(solutionItem.IncrementSettings.AssemblyInfoFilename);
                    if (solutionItem.ProjectType == LanguageType.None)
                        Logger.Write("Can't infer solution's assembly info file language. Please add extension to filename.", LogLevel.Error, solutionItem.Filename, 1);

                    break;

                default:
                    Logger.Write("Unknown project file type: \"" + ext + "\"", LogLevel.Error, solutionItem.Filename, 1);
                    return null;
            }

            if (!string.IsNullOrEmpty(solutionItem.IncrementSettings.AssemblyInfoFilename))
            {
                string basePath = Path.GetDirectoryName(solutionItem.Filename);
                return Common.MakeAbsolutePath(basePath, solutionItem.IncrementSettings.AssemblyInfoFilename);
            }

            string ret = null;

            ProjectItem assemblyInfo = solutionItem.FindProjectItem(filename);

            if (assemblyInfo == null)
            {
                // we couldn't locate the file, but, if it is a C-Project, is also could be an unmanaged project
				if (ext == ".vcproj" || ext == ".vcxproj")
                {
                    filename = solutionItem.Name + ".rc";
                    assemblyInfo = solutionItem.FindProjectItem(filename);
                    solutionItem.ProjectType = LanguageType.CPPUnmanaged;
                }

                if (assemblyInfo == null)
                {
                    Logger.Write("Could not locate \"" + filename + "\" in project.", LogLevel.Warning);
                    return null;
                }
            }

            ret = assemblyInfo.get_FileNames(0);

            if (string.IsNullOrEmpty(ret))
            {
                Logger.Write("Located \"" + filename + "\" project item but failed to get filename.", LogLevel.Error);
                return null;
            }

            Logger.Write("Found \"" + ret + "\"", LogLevel.Debug);

            return ret;
        }

        private vsBuildState _currentBuildState = vsBuildState.vsBuildStateInProgress;
        private vsBuildAction _currentBuildAction = vsBuildAction.vsBuildActionClean;
        private vsBuildScope _currentBuildScope = vsBuildScope.vsBuildScopeBatch;
        private Dictionary<string, SolutionItem> _updatedItems = new Dictionary<string, SolutionItem>();

        /// <summary>
        /// Called when a build begin event has occured.
        /// </summary>
        /// <param name="Scope">The scope.</param>
        /// <param name="Action">The action.</param>
        public void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            Logger.Write("BuildEvents_OnBuildBegin scope: " + scope.ToString() + " action " + action.ToString(), LogLevel.Debug);

            _currentBuildState = vsBuildState.vsBuildStateInProgress;
            _currentBuildAction = action;
            _currentBuildScope = scope;
            _buildStartDate = DateTime.Now;

            ExecuteIncrement();
            _updatedItems.Clear();

#if OLD
            try
            {
                if (action == vsBuildAction.vsBuildActionBuild ||
                    action == vsBuildAction.vsBuildActionRebuildAll)
                {
                    _currentBuildVersion = CreateBuildVersion();

                    if (scope == vsBuildScope.vsBuildScopeSolution)
                    {
                        SolutionItem solutionItem = new SolutionItem(_connect.ApplicationObject.Solution);
                        Execute(solutionItem);
                    }
                    else
                    {
                        Array projects = (Array)_connect.ApplicationObject.ActiveSolutionProjects;
                        foreach (Project p in projects)
                        {
                            SolutionItem solutionItem = new SolutionItem(p);
                            Execute(solutionItem);
                        }
                    }
                }
            }
            
#endif
        }

        /// <summary>
        /// Called when a build has been done.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="action">The action.</param>
        public void OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            Logger.Write("BuildEvents_OnBuildDone scope: " + scope.ToString() + " action " + action.ToString(), LogLevel.Debug);
            _currentBuildState = vsBuildState.vsBuildStateDone;

            ExecuteIncrement();
            _updatedItems.Clear();
            ClearSolutionItemAndFileDateCache();
        }

#if DEBUG
        /// <summary>
        /// Called when build project config has occured.
        /// </summary>
        /// <param name="Project">The project.</param>
        /// <param name="ProjectConfig">The project config.</param>
        /// <param name="Platform">The platform.</param>
        /// <param name="SolutionConfig">The solution config.</param>
        public void OnBuildProjConfigBegin(string projectName, string projectConfig, string platform, string solutionConfig)
        {
            // Can't use this ... visual studio seems to build a list of changed files before this event.
            // Changing them here won't do a thing.

            try
            {
                Project p = _connect.ApplicationObject.Solution.Projects.Item(projectName);

                //p.CodeModel.AddAttribute("AssemblyFileAttribute", "AssemblyInfo.cs", "1.2.3.4", -1);
                Logger.Write(DumpProperties(p.Properties), LogLevel.Debug);

                /*_connect.Log("----------------------------------------", LogLevel.Debug);
                _connect.Log("OnBuildProjConfigBegin", LogLevel.Debug);
                _connect.Log("Project: \"" + Project + "\"", LogLevel.Debug);
                _connect.Log("ProjectConfig: \"" + ProjectConfig + "\"", LogLevel.Debug);
                _connect.Log("Platform: \"" + Platform + "\"", LogLevel.Debug);
                _connect.Log("SolutionConfig: \"" + SolutionConfig + "\"", LogLevel.Debug);
                _connect.Log("----------------------------------------", LogLevel.Debug);

                if ((_currentBuildAction == vsBuildAction.vsBuildActionBuild ||
                    _currentBuildAction == vsBuildAction.vsBuildActionRebuildAll))
                {
                    // ... and the project itself

                    if (!_updatedItems.ContainsKey(Project))
                    {
                        Project p = _connect.ApplicationObject.Solution.Item(Project);

                        if (p != null)
                        {
                            SolutionItem solutionItem = SolutionItem.ConstructSolutionItem(_connect, p);

                            if(solutionItem != null)
                                UpdateRecursive(solutionItem);

                            _updatedItems.Add(Project, solutionItem);
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                Logger.Write("Error occured while updating build version of project " + projectName + "\n" + ex.ToString(), LogLevel.Error);
            }
        }

        private string DumpProperties(Properties props)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Property prop in props)
            {
                try
                {
                    sb.Append(string.Format("Name: \"{0}\" Value: \"{1}\"\r\n", prop.Name, prop.Value));
                }
                catch
                {
                    sb.Append(string.Format("Name: \"{0}\" Value: \"(UNKNOWN)\"\r\n", prop.Name));
                }

            }

            return sb.ToString();
        }
#endif

    }
}
