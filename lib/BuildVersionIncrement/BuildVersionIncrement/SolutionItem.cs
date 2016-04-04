using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using System.IO;
using EnvDTE80;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms.Design;
using System.Drawing.Design;

namespace BuildVersionIncrement
{
    /// <summary>
    /// The type of solution item.
    /// </summary>
    internal enum SolutionItemType
    {
        /// <summary>
        /// Unknown type
        /// </summary>
        None,
        /// <summary>
        /// Folder type
        /// </summary>
        Folder,
        /// <summary>
        /// Project type
        /// </summary>
        Project,
        /// <summary>
        /// Solution type (root object)
        /// </summary>
        Solution
    }

    /// <summary>
    /// The language type of the project
    /// </summary>
    internal enum LanguageType
    {
        /// <summary>
        /// This is no work project
        /// </summary>
        None,
        /// <summary>
        /// C#
        /// </summary>
        CSharp,
        /// <summary>
        /// Visual Basic Dot Net
        /// </summary>
        VisualBasic,
        /// <summary>
        /// C++ Managed
        /// </summary>
        CPPManaged,
        /// <summary>
        /// C++ Unmanaged
        /// </summary>
        CPPUnmanaged
    }

    /// <summary>
    /// Class to wrap solution items
    /// </summary>
    internal class  SolutionItem // : ICustomTypeDescriptor 
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionItem"/> class.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public SolutionItem(Connect connect, Solution solution) : this(connect, solution, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionItem"/> class.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="recursive">if set to <c>true</c> the item will fill it's <see cref="SubItems"/>.</param>
        public SolutionItem(Connect connect, Solution solution, bool recursive)
        {
            if (connect == null)
                throw (new ArgumentNullException("connect"));

            if (solution == null)
                throw (new ArgumentNullException("solution"));

             _incrementSetting = new SolutionItemIncrementSettings(this);

            _connect = connect;
            _item = solution;
            _itemType = SolutionItemType.Solution;
            _name = Path.GetFileNameWithoutExtension(solution.FileName);
            _filename = solution.FileName;
            _uniqueName = _name;

            //_autoUpdateAssemblyVersion = bool.Parse(GetGlobalVariable(Resources.GlobalVarName_updateAssemblyVersion, "false"));
            //_autoUpdateFileVersion = bool.Parse(GetGlobalVariable(Resources.GlobalVarName_updateFileVersion, "false"));
            GetGlobalVariables();

            if (recursive)
                FillSolutionTree(connect, this, solution.Projects);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionItem"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        private SolutionItem(Connect connect, Project project) : this(connect, project, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionItem"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="recursive">if set to <c>true</c> the item will fill it's <see cref="SubItems"/>.</param>
        private SolutionItem(Connect connect, Project project, bool recursive)
        {
            if (project == null)
                throw (new ArgumentNullException("project"));

            if (connect == null)
                throw (new ArgumentNullException("connect"));

            _incrementSetting = new SolutionItemIncrementSettings(this);

            _connect = connect;
            _item = project;
            _name = project.Name;

            _filename = project.FileName;
            _uniqueName = project.UniqueName;

            if (!string.IsNullOrEmpty(_filename) && string.IsNullOrEmpty(Path.GetExtension(_filename)))
                _filename += Path.GetExtension(project.UniqueName);

            if (string.IsNullOrEmpty(project.FullName))
            {
                _itemType = SolutionItemType.Folder;

                if (recursive)
                    FillSolutionTree(connect, this, project.ProjectItems);
            }
            else
            {
                _itemType = SolutionItemType.Project;

                GetGlobalVariables();
            }
        }

        /// <summary>
        /// Constructs the solution item.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns><c>null</c> when the given project isn't supported, otherwise a fresh new <c>SolutionItem</c> will be returned.</returns>
        public static SolutionItem ConstructSolutionItem(Connect connect, Project project)
        {
            return ConstructSolutionItem(connect, project, true);
        }

        /// <summary>
        /// Constructs a solution item.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="recursive">if set to <c>true</c> the item will fill it's <see cref="SubItems"/>.</param>
        /// <returns><c>null</c> when the given project isn't supported, otherwise a fresh new <c>SolutionItem</c> will be returned.</returns>
        public static SolutionItem ConstructSolutionItem(Connect connect, Project project, bool recursive)
        {
            SolutionItem ret = null;

            if (IsValidSolutionItem(project))
                ret = new SolutionItem(connect, project, recursive);

            return ret;
        }

        private static bool IsValidSolutionItem(Project p)
        {
            bool ret = false;

            try
            {
                if (p != null && p.Object != null && !string.IsNullOrEmpty(p.Kind) &&
                    (p.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" || // CS
                     p.Kind == "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}" || // VB.NET
                     p.Kind == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}" || // C++.NET
                     p.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")) // Solution folder
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Exception occured while checking project type \"" + p.UniqueName + "\".\n" + ex.ToString(), LogLevel.Error);
            }
        
            return ret;
        }

        private static void FillSolutionTree(Connect connect, SolutionItem solutionItem, Projects projects)
        {
            if (projects == null)
                return;

            foreach (Project p in projects)
            {
                SolutionItem item = ConstructSolutionItem(connect, p);

                if (item != null)
                    solutionItem.SubItems.Add(item);
            }
        }

        private static void FillSolutionTree(Connect connect, SolutionItem solutionItem, ProjectItems projectItems)
        {
            if (projectItems == null)
                return;

            foreach (ProjectItem p in projectItems)
            {
                SolutionItem item = ConstructSolutionItem(connect, p.SubProject);

                if (item != null)
                    solutionItem.SubItems.Add(item);
            }
        }

        #endregion

        /// <summary>
        /// Finds the project item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The project item when if a match has been found, otherwise null.</returns>
        public ProjectItem FindProjectItem(string name)
        {
            ProjectItem ret = null;

            if (ItemType == SolutionItemType.Project)
            {
                ret = FindProjectItem(Project.ProjectItems, name);
            }

            return ret;
        }

        private static ProjectItem FindProjectItem(ProjectItems projectItems, string name)
        {
            if (projectItems == null)
                return null;

            foreach (ProjectItem item in projectItems)
            {
                if (string.Compare(item.Name, name, true) == 0)
                    return item;

                if (item.ProjectItems != null && item.ProjectItems.Count > 0)
                {
                    ProjectItem subItem = FindProjectItem(item.ProjectItems, name);

                    if (subItem != null)
                        return subItem;
                }
            }

            return null;
        }

        /// <summary>
        /// Applies the global settings to this instance.
        /// </summary>
        public void ApplyGlobalSettings()
        {
            GlobalIncrementSettings globalSett = new GlobalIncrementSettings();

            try
            {
                globalSett.Load();
            }
            catch (Exception ex)
            {
                throw (new ApplicationException("Exception occured while applying global settings to the solution item (" + UniqueName + ").", ex));
            }

            IncrementSettings.CopyFrom(globalSett);
        }

        #region GlobalVariables

        /// <summary>
        /// Gets the global variables.
        /// </summary>
        private void GetGlobalVariables()
        {
            IncrementSettings.Load();
        }

        /// <summary>
        /// Gets the gloval varialble.
        /// </summary>
        /// <param name="varName">Name of the var.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Variable's value if found, defaultVal otherwise</returns>
        private string GetGlobalVariable(string varName, string defaultValue)
        {
            return GlobalVariables.GetGlobalVariable(Globals, varName, defaultValue);
        }


        /// <summary>
        /// Sets the global variables.
        /// </summary>
        public void SetGlobalVariables()
        {
            IncrementSettings.Save();

            foreach (SolutionItem child in SubItems)
                child.SetGlobalVariables();
        }

        
        /// <summary>
        /// Sets the global variable.
        /// </summary>
        /// <param name="varName">Name of the var.</param>
        /// <param name="value">The value.</param>
        public void SetGlobalVariable(string varName, string value)
        {
            GlobalVariables.SetGlobalVariable(Globals, varName, value);
        }

        
        #endregion

        #region Not Browsable

        private Connect _connect;
        /// <summary>
        /// Gets the connect.
        /// </summary>
        /// <value>The connect.</value>
        [Browsable(false)]
        public Connect Connect
        {
            get { return _connect; }
        }

        private SolutionItemType _itemType;
        /// <summary>
        /// Gets or sets the type of the item.
        /// </summary>
        /// <value>The type of the item.</value>
        [Browsable(false)]
        public SolutionItemType ItemType
        {
            get { return _itemType; }
            set { _itemType = value; }
        }

        private object _item;
        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>The solution.</value>
        [Browsable(false)]
        public Solution Solution
        {
            get { return (Solution)_item; }
        }

        /// <summary>
        /// Gets the project
        /// </summary>
        /// <value>The tag.</value>
        [Browsable(false)]
        public Project Project
        {
            get { return (Project)_item; }
        }

        /// <summary>
        /// Gets the DTE.
        /// </summary>
        /// <value>The DTE.</value>
        [Browsable(false)]
        public DTE DTE
        {
            get
            {
                switch (ItemType)
                {
                    case SolutionItemType.Project:
                        return Project.DTE;

                    case SolutionItemType.Solution:
                        return Solution.DTE;

                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the globals.
        /// </summary>
        /// <value>The globals.</value>
        [Browsable(false)]
        public Globals Globals
        {
            get
            {
                switch (ItemType)
                {
                    case SolutionItemType.Solution:
                        return Solution.Globals;

                    case SolutionItemType.Project:
                        return Project.Globals;

                    default:
                        return null;
                }
            }
        }

        private string _uniqueName;
        /// <summary>
        /// Gets or sets the unique name.
        /// </summary>
        /// <value>The the unique name.</value>
        [Browsable(false)]
        public string UniqueName
        {
            get { return _uniqueName; }
            set { _uniqueName = value; }
        }

        private List<SolutionItem> _subItems = new List<SolutionItem>();
        /// <summary>
        /// Gets the sub items.
        /// </summary>
        /// <value>The sub items.</value>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubItems")]
        [Browsable(false)]
        public List<SolutionItem> SubItems
        {
            get { return _subItems; }
        }

        /// <summary>
        /// Gets the build dependency.
        /// </summary>
        /// <value>The build dependency.</value>
        [Browsable(false)]
        public BuildDependency BuildDependency
        {
            get
            {
                return DTE.Solution.SolutionBuild.BuildDependencies.Item(UniqueName);
            }
        }

        /*private IVersionIncrementer _versionIncrementer = new StandardVersionIncrementer();
        /// <summary>
        /// Gets the version incrementer.
        /// </summary>
        /// <value>The version incrementer.</value>
        [Browsable(false)]
        public IVersionIncrementer VersionIncrementer
        {
            get { return _versionIncrementer; }
        }*/

        private SolutionItemIncrementSettings _incrementSetting;
        /// <summary>
        /// Gets the increment settings.
        /// </summary>
        /// <value>The increment settings.</value>
        [Browsable(false)]
        public SolutionItemIncrementSettings IncrementSettings
        {
            get { return this._incrementSetting; }
        }			

        #endregion

        

#if DEBUG

        /// <summary>
        /// Gets the GUID.
        /// </summary>
        /// <value>The GUID.</value>
        public string Guid
        {
            get
            {
                if (ItemType != SolutionItemType.Solution)
                    return Project.Kind;

                return string.Empty;
            }
        }
#endif

        private string _name;
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _filename;
        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public string Filename
        {
            get { return _filename; }
        }
        
        private LanguageType _projectType = LanguageType.None;
        /// <summary>
        /// Gets or Sets the project language type.
        /// </summary>
        /// <value>The type.</value>
        public LanguageType ProjectType
        {
            get { return _projectType; }
            set { _projectType = value; }
        }
#if OLD
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

        private string _configurationName = "Any";
        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        /// <value>The name of the configuration.</value>
        [Category("Auto Update")]
        [DefaultValue("Any")]
        [DisplayName("Configuration Name")]
        [Description("Set this to the name to of the configuration when the auto update should occur.")]
        [TypeConverter(typeof(ConfigurationStringConverter))]
        public string ConfigurationName
        {
            get { return _configurationName; }
            set { _configurationName = value; }
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

        private static string[] BUILD_VER_STYLE_DESC = {"DeltaBaseDate takes the time elapsed since the given start date and formats the buildversion as string.Format(\"{0}{1:00}.{2:00}{3:00}\", dt.Year * 12 + dt.Month, dt.Day, dt.Hour, dt.Minute)",
                                                        "YearDayOfYear_Timestamp takes the current date and formats the build version as string.Format(\"{0}{1:000}.{2:HHmm}\", yearDecade, dayOfyear, now)",
                                                        "DeltaBaseYear takes the time elapsed since the given start date and formats the buildversion as string.Format(\"{0}{1:000}.{2:HHmm}\", deltaYears.ToString(), dayOfyear, now)",
                                                        "YearDayOfYear_AutoIncrement autoincrements the revision field per build, resets to 0 if the build part is not today. Please note that this scheme can cause conflicts if you have multiple developers working on the same project.",
                                                        "AutoIncrementBuildVersion will only increment the build field, it will leave the revision untouched. Please note that this scheme can cause conflicts if you have multiple developers working on the same project."};

        private OLD_BuildVersioningStyleType _buildVersioningStyle;
        /// <summary>
        /// Gets or sets the build versioning style.
        /// </summary>
        /// <value>The build versioning style.</value>
        [Category("Auto Update")]
        [Description("Defines the build versioning style")]
        [DisplayName("Build versioning style")]
        [DefaultValue(OLD_BuildVersioningStyleType.DeltaBaseDate)]
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(false)]
        internal OLD_BuildVersioningStyleType BuildVersioningStyle
        {
            get { return _buildVersioningStyle; }
            set { _buildVersioningStyle = value; }
        }

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
            get { return VersionIncrementer.ProjectStartDate; }
            set { VersionIncrementer.ProjectStartDate = value; }
        }

        private bool _replaceNonNumerics = true;
        /// <summary>
        /// Gets or sets if non-numbers within the version should be replaced by a zero.
        /// </summary>
        /// <value>The new value. :)</value>
        [Category("Auto Update")]
        [Description("If non-numeric values within the version should be replaced with a zero.")]
        [DisplayName("Replace Non-Numerics")]
        [DefaultValue(true)]
        public bool ReplaceNunNumerics
        {
            get { return _replaceNonNumerics; }
            set { _replaceNonNumerics = value; }
        }

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this, attributes, true);
            PropertyDescriptorCollection ret = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor p in props)
            {
                switch (p.Name)
                {
                    case "StartDate":
                        // Hide the start date if it isn't needed
                        if (BuildVersioningStyle != BuildVersioningStyleType.YearDayOfYear_Timestamp &&
                            BuildVersioningStyle != BuildVersioningStyleType.AutoIncrementBuildVersion)
                            ret.Add(p);
                        break;

                    case "BuildVersioningStyle":
                        // Hack in the custom description
                        string description = string.Format("{0}\r\n{1}", p.Description, 
                                                           BUILD_VER_STYLE_DESC[(int)BuildVersioningStyle]);
                        ret.Add(new DirtyHackPropertyDescriptor(p, description));
                        break;

                    default:
                        ret.Add(p);
                        break;
                }
            }
            
            return ret;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion


        #region DirtyHackPropertyDescriptor
        /// <summary>
        /// Dirty solution to change the description attribute at runtime. 
        /// It basically implements a <see cref="PropertyDescriptor"/> class and returns all defaults 
        /// from the given property descriptor except for the <see cref="Description"/> property.
        /// </summary>
        private class DirtyHackPropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor _p;

            /// <summary>
            /// Initializes a new instance of the <see cref="DirtyHackPropertyDescriptor"/> class.
            /// </summary>
            /// <param name="p">The original <see cref="PropertyDescriptor"/>.</param>
            /// <param name="description">The description.</param>
            public DirtyHackPropertyDescriptor(PropertyDescriptor p, string description) : base(p)
            {
                _description = description;
                _p = p;
            }

            private string _description;
            /// <summary>
            /// Gets the description of the member, as specified in the <see cref="T:System.ComponentModel.DescriptionAttribute"></see>.
            /// </summary>
            /// <value></value>
            /// <returns>The description of the member. If there is no <see cref="T:System.ComponentModel.DescriptionAttribute"></see>, the property value is set to the default, which is an empty string ("").</returns>
            public override string Description
            {
                get
                {
                    return _description;
                }
            }

            /// <summary>
            /// When overridden in a derived class, returns whether resetting an object changes its value.
            /// </summary>
            /// <param name="component">The component to test for reset capability.</param>
            /// <returns>
            /// true if resetting the component changes its value; otherwise, false.
            /// </returns>
            public override bool CanResetValue(object component)
            {
                return _p.CanResetValue(component);
            }

            /// <summary>
            /// When overridden in a derived class, gets the type of the component this property is bound to.
            /// </summary>
            /// <value></value>
            /// <returns>A <see cref="T:System.Type"></see> that represents the type of component this property is bound to. When the <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)"></see> or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)"></see> methods are invoked, the object specified might be an instance of this type.</returns>
            public override Type ComponentType
            {
                get { return _p.ComponentType; }
            }

            /// <summary>
            /// When overridden in a derived class, gets the current value of the property on a component.
            /// </summary>
            /// <param name="component">The component with the property for which to retrieve the value.</param>
            /// <returns>
            /// The value of a property for a given component.
            /// </returns>
            public override object GetValue(object component)
            {
                return _p.GetValue(component);
            }

            /// <summary>
            /// When overridden in a derived class, gets a value indicating whether this property is read-only.
            /// </summary>
            /// <value></value>
            /// <returns>true if the property is read-only; otherwise, false.</returns>
            public override bool IsReadOnly
            {
                get { return _p.IsReadOnly; }
            }

            /// <summary>
            /// When overridden in a derived class, gets the type of the property.
            /// </summary>
            /// <value></value>
            /// <returns>A <see cref="T:System.Type"></see> that represents the type of the property.</returns>
            public override Type PropertyType
            {
                get { return _p.PropertyType; }
            }

            /// <summary>
            /// When overridden in a derived class, resets the value for this property of the component to the default value.
            /// </summary>
            /// <param name="component">The component with the property value that is to be reset to the default value.</param>
            public override void ResetValue(object component)
            {
                _p.ResetValue(component);
            }

            /// <summary>
            /// When overridden in a derived class, sets the value of the component to a different value.
            /// </summary>
            /// <param name="component">The component with the property value that is to be set.</param>
            /// <param name="value">The new value.</param>
            public override void SetValue(object component, object value)
            {
                _p.SetValue(component, value);
            }

            /// <summary>
            /// When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.
            /// </summary>
            /// <param name="component">The component with the property to be examined for persistence.</param>
            /// <returns>
            /// true if the property should be persisted; otherwise, false.
            /// </returns>
            public override bool ShouldSerializeValue(object component)
            {
                return _p.ShouldSerializeValue(component);
            }

            /// <summary>
            /// Gets the type converter for this property.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// A <see cref="T:System.ComponentModel.TypeConverter"/> that is used to convert the <see cref="T:System.Type"/> of this property.
            /// </returns>
            /// <PermissionSet>
            /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode"/>
            /// </PermissionSet>
            public override TypeConverter Converter
            {
                get
                {
                    return base.Converter;
                }
            }
        }
        #endregion

#endif
    }
}
