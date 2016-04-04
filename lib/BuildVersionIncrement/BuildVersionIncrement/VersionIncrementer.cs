
using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using System.IO;
using System.Text.RegularExpressions;

namespace BuildVersionIncrement
{
    internal class VersionIncrementer
    {
        private Connect _connect;
        //private DateTime _startDate = new DateTime(1975, 10, 21);
        //private string _currentBuildVersion;
        private DateTime _buildStartDate = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionIncrementer"/> class.
        /// </summary>
        /// <param name="connect">The connect.</param>
        public VersionIncrementer(Connect connect)
        {
            _connect = connect;
        }

        private void ExecuteIncrement()
        {
            if (!GlobalAddinSettings.Default.IsEnabled)
            {
                Logger.Write("BuildVersionIncrement disabled.", LogLevel.Debug);
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
                                UpdateProject(solutionItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error occured while executing build version increment.\n" + ex.ToString(), LogLevel.Error);
            }
        }

        /// <summary>
        /// Updates all build versions for the entire solution
        /// </summary>
        private void UpdateRecursive(SolutionItem solutionItem)
        {
            try
            {
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
            if (solutionItem.IncrementSettings.UseGlobalSettings)
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
                                //string pattern = "^[\\[<]assembly:\\ ?" + attribute + "(Attribute)?\\(\"\\d+\\.\\d+\\.(?<Version>[^\"]+)\"\\)[\\]>]";
                                //string pattern = "^[\\[<]assembly:\\ ?" + attribute + "(Attribute)?\\(\"(?<FullVersion>\\d+\\.\\d+\\.(?<Version>[^\"]+)\"\\)[\\]>]";
                                //string pattern = "^[\\[<]assembly:\\ ?" + attribute + "(Attribute)?\\(\"(?<FullVersion>\\d+\\.\\d+(\\.(?<Version>[^\"]+))?)\"\\)[\\]>]";
                                //string pattern = "^[\\[<]assembly:\\ ?" + attribute + "(Attribute)?\\(\\ ?\"(?<FullVersion>\\S+\\.\\S+(\\.(?<Version>[^\"]+))?)\"\\ ?\\)[\\]>]";

                                // Patch 4812 from Haukinger, first line is original pattern, second is my edited one.
                                //string pattern = @"^[\\[<]assembly:\\W*" + attribute + @"(Attribute)?\\W*\\(\\W*\\""(?<FullVersion>\\S+\\.\\S+(\\.(?<Version>[^\\""]+))?)\\""\\W*\\)[\\]>]";
                                UpdateVersion(solutionItem, "^[\\[<]assembly:\\s*" + attribute + "(Attribute)?\\s*\\(\\s*\"(?<FullVersion>\\S+\\.\\S+(\\.(?<Version>[^\"]+))?)\"\\s*\\)[\\]>]", filename, attribute);
                                break;
                            case LanguageType.CPPUnmanaged:
                                if (attribute == "AssemblyVersion")
                                    attribute = "ProductVersion";
                                if (attribute == "AssemblyFileVersion")
                                    attribute = "FileVersion";

                                UpdateVersion(solutionItem, "^[\\s]*VALUE\\ \"" + attribute + "\",\\ \"(?<FullVersion>\\S+[.,\\s]+\\S+[.,\\s]+\\S+[.,\\s]+\\S+)\"", filename, attribute);
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

                Version currentVersion = null;

                try
                {
                    // Make sure we don't have anything what is not a number in the version
                    //currentVersion = new Version(m.Groups["FullVersion"].Value.Replace("*", "0"));
                    currentVersion = new Version(Regex.Replace(m.Groups["FullVersion"].Value, "[^\\d" + sep.Groups["Separator"].Value + "]+", "0").Replace(sep.Groups["Separator"].Value, "."));
                }
                catch (Exception ex)
                {
                    string msg = string.Format("Error occured while parsing value of {0} ({1}).\n{2}",
                                               debugAttribute, m.Groups["FullVersion"].Value, ex);

                    throw (new Exception(msg, ex));
                }


                StandardVersionIncrementer versionIncrementer = new StandardVersionIncrementer();

                versionIncrementer.BuildStartDate = solutionItem.IncrementSettings.IsUniversalTime ? _buildStartDate.ToUniversalTime() : _buildStartDate;
                versionIncrementer.ProjectStartDate = solutionItem.IncrementSettings.StartDate;

                Version newVersion = versionIncrementer.CreateBuildVersion(solutionItem.IncrementSettings.VersioningStyle, currentVersion);

                // CreateBuildVersion(solutionItem, currentVersion);

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
                                mergedVersion[0] = newVersion.Major.ToString();
                            if (Regex.IsMatch(mergedVersion[1], "[\\d]+"))
                                mergedVersion[1] = newVersion.Minor.ToString();
                            if (Regex.IsMatch(mergedVersion[2], "[\\d]+"))
                                mergedVersion[2] = newVersion.Build.ToString();
                            if (Regex.IsMatch(mergedVersion[3], "[\\d]+"))
                                mergedVersion[3] = newVersion.Revision.ToString();

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

                    if (success)
                    {
                        string msg = solutionItem.Name + " " + debugAttribute + ": " + newVersion.ToString();
                        Logger.Write(msg, LogLevel.Info);
                    }
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

        private string GetAssemblyInfoFilename(SolutionItem solutionItem)
        {
            if (!string.IsNullOrEmpty(solutionItem.IncrementSettings.AssemblyInfoFilename))
            {
                string basePath = Path.GetDirectoryName(solutionItem.Filename);
                return Common.MakeAbsolutePath(basePath, solutionItem.IncrementSettings.AssemblyInfoFilename);
            }

            string ext = Path.GetExtension(solutionItem.Filename);

            string filename = "AssemblyInfo";

            solutionItem.ProjectType = LanguageType.None;

            switch (ext)
            {
                case ".vbproj":
                    filename += ".vb";
                    solutionItem.ProjectType = LanguageType.VisualBasic;
                    break;

                case ".vcproj":
                    filename += ".cpp";
                    solutionItem.ProjectType = LanguageType.CPPManaged;
                    break;

                case ".csproj":
                    filename += ".cs";
                    solutionItem.ProjectType = LanguageType.CSharp;
                    break;

                case ".sln":
                    Logger.Write("Can't update build version for a solution without specifying an assembly info file.", LogLevel.Error, solutionItem.Filename, 1);
                    return null;

                default:
                    Logger.Write("Unknown project file type: \"" + ext + "\"", LogLevel.Error, solutionItem.Filename, 1);
                    return null;
            }

            string ret = null;

            ProjectItem assemblyInfo = solutionItem.FindProjectItem(filename);

            if (assemblyInfo == null)
            {
                // we couldn't locate the file, but, if it is a C-Project, is also could be an unmanaged project
                if (ext == ".vcproj")
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
