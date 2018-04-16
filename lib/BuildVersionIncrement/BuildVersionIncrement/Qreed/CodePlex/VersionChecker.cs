using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using Qreed.Reflection;
using System.Net;
using System.Text.RegularExpressions;

namespace Qreed.CodePlex
{
    /// <summary>
    /// Utility class that checks a Codeplex project for a newer version of a certain assembly.
    /// </summary>
    public partial class VersionChecker : Component
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionChecker"/> class.
        /// </summary>
        public VersionChecker()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionChecker"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public VersionChecker(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public event EventHandler<VersionCheckerEventArgs> CheckForNewVersionComplete;

        private string _versionInfoUrl;
        /// <summary>
        /// Gets or sets the version info URL.
        /// </summary>
        /// <value>The version info URL.</value>
        public string VersionInfoUrl
        {
            get 
            {
                if (string.IsNullOrEmpty(_versionInfoUrl) && !string.IsNullOrEmpty(ProjectHomePage))
                {
                    string url = ProjectHomePage.TrimEnd("/".ToCharArray());
                    url += "/Release/ProjectReleases.aspx?ReleaseName=VersionInfo";
                    return url;
                }

                return this._versionInfoUrl; 
            }
            set { this._versionInfoUrl = value; }
        }

        private string _projectHomePage;
        /// <summary>
        /// Gets or sets the project home page.
        /// </summary>
        /// <value>The project home page.</value>
        [RefreshProperties(RefreshProperties.All)]
        public string ProjectHomePage
        {
            get { return this._projectHomePage; }
            set { this._projectHomePage = value; }
        }

        private Assembly _assembly;
        /// <summary>
        /// Gets or sets the assembly, which is used to get the local version from.
        /// Leave null to use the entry assembly.
        /// </summary>
        /// <value>The assembly.</value>
        [Browsable(false)]
        public Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                    return Assembly.GetEntryAssembly();

                return this._assembly;
            }
            set { this._assembly = value; }
        }

        private bool _useAssemblyFileVersion;
        /// <summary>
        /// Gets or sets a value indicating whether to use <see cref="AssemblyFileVersionAttribute"/> attribute or the <see cref="AssemblyVersionAttribute"/>.
        /// </summary>
        /// <value>
        /// 	If <c>true</c> the version will use the <see cref="AssemblyFileVersionAttribute"/> ; otherwise, the <see cref="AssemblyVersionAttribute"/>.
        /// </value>
        public bool UseAssemblyFileVersion
        {
            get { return this._useAssemblyFileVersion; }
            set { this._useAssemblyFileVersion = value; }
        }

        private string _pattern = "<div\\ class=\"wikidoc\">.*?Current\\ version.+?\\ (?<Version>.+?)</div>";
        /// <summary>
        /// Gets or sets the pattern used to grab the current version from the html page.
        /// </summary>
        /// <value>The pattern.</value>
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }

        private RegexOptions _patternOptions = RegexOptions.IgnoreCase;
        /// <summary>
        /// Gets or sets the pattern options.
        /// </summary>
        /// <value>The pattern options.</value>
        public RegexOptions PatternOptions
        {
            get { return this._patternOptions; }
            set { this._patternOptions = value; }
        }	

        /// <summary>
        /// Gets the local version.
        /// </summary>
        /// <value>The local version.</value>
        [Browsable(false)]
        public Version LocalVersion
        {
            get
            {
                if (UseAssemblyFileVersion)
                {
                    AssemblyFileVersionAttribute attr = ReflectionHelper.GetAssemblyAttribute<AssemblyFileVersionAttribute>(Assembly);
                    return new Version(attr.Version);
                }
                else
                {
                    return Assembly.GetName().Version;
                }
            }
        }

        private Version _onlineVersion;
        /// <summary>
        /// Gets the online version.
        /// </summary>
        /// <value>The online version.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Browsable(false)]
        public Version OnlineVersion
        {
            get
            {
                if (_onlineVersion == null)
                    _onlineVersion = GetOnlineVersion();

                return this._onlineVersion;
            }
        }

        /// <summary>
        /// Checks if there's a newer version online.
        /// </summary>
        /// <returns><c>true</c> if there's a newer version, otherwise <c>false</c>.</returns>
        public bool CheckForNewVersion()
        {
            return OnlineVersion > LocalVersion;
        }

        /// <summary>
        /// Checks if there's a newer version online in a background thread.
        /// Set the <see cref="CheckForNewVersionComplete"/> event to capture the result.
        /// </summary>
        public void CheckForNewVersionASync()
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = CheckForNewVersion();
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool result = false;

            try
            {
                if (e.Result != null)
                    result = (bool)e.Result;
            }
            catch
            {
                // Seems that RunWorkerCompletedEventArgs will call RaiseExceptionIfNecessary if an 
                // error has occured. We don't need the exception; we already have it :S
            }

            if (CheckForNewVersionComplete != null)
            {
                VersionCheckerEventArgs eventArgs = new VersionCheckerEventArgs(e.Error, result);
                CheckForNewVersionComplete(this, eventArgs);
            }
        }

        private Version GetOnlineVersion()
        {
            WebClient wc = new WebClient();

            if (string.IsNullOrEmpty(VersionInfoUrl))
                throw (new Exception("VersionInfoUrl not set."));

            string response = wc.DownloadString(VersionInfoUrl);

            if (string.IsNullOrEmpty(response))
                throw (new Exception("Empty response from \"" + VersionInfoUrl + "\"."));

            Match m = Regex.Match(response, Pattern, PatternOptions);

            if (!m.Success)
                throw (new Exception("Could not locate online version information from \"" + VersionInfoUrl + "\"."));

            return new Version(m.Groups["Version"].Value);
        }
    }
}
