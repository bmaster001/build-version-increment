// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: BuildVersionIncrementPackage.cs
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

namespace BuildVersionIncrement
{
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Runtime.InteropServices;

	using Commands;

	using EnvDTE;

	using log4net;
	using log4net.Config;

	using Logging;

	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	using Model;

	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

	// Info on this package for Help/About
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules",
		"SA1650:ElementDocumentationMustBeSpelledCorrectly",
		Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
	public sealed class BuildVersionIncrementPackage : Package
	{
		public const string PackageGuidString = "d9498ed1-f738-4c84-9cbc-82ab0163d742";

		private Logger _logger;
		private BuildVersionIncrementor _buildVersionIncrementor;
		private BuildEvents _buildEvents;

		private DTE DTE => (DTE)GetService(typeof(DTE));

		#region Package Members

		protected override void Initialize()
		{
			XmlConfigurator.Configure(new FileInfo("log4net.config"));
			GlobalContext.Properties["package"] = this;
			SettingsCommand.Initialize(this);
			VersionCommand.Initialize(this);

			_logger = new Logger();
			_buildVersionIncrementor = new BuildVersionIncrementor(this);
			_buildEvents = DTE.Events.BuildEvents;
			_buildVersionIncrementor.InitializeIncrementors();

			base.Initialize();

			
		}

		#endregion
	}
}