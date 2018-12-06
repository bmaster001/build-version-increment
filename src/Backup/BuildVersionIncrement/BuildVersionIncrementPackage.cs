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
	using System.Runtime.InteropServices;

	using Commands;

	using EnvDTE;

	using log4net;

	using Logging;

	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

	// Info on this package for Help/About
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules",
		"SA1650:ElementDocumentationMustBeSpelledCorrectly",
		Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
	[ProvideAutoLoad(UIContextGuids80.NoSolution)]
	[ProvideAppCommandLine("IncrementVersion", typeof(BuildVersionIncrementPackage), Arguments = "0", DemandLoad = 1, PackageGuid = PackageGuidString)]
	public sealed class BuildVersionIncrementPackage : Package
	{
		public const string PackageGuidString = "d9498ed1-f738-4c84-9cbc-82ab0163d742";
		private BuildEvents _buildEvents;
		private BuildVersionIncrementor _buildVersionIncrementor;

		private DTE DTE => (DTE)GetService(typeof(DTE));
		internal bool IsCommandLine { get; set; }

		protected override void Initialize()
		{
			GlobalContext.Properties["package"] = this;
			CheckCommandLine();
			Logger.Initialise(IsCommandLine);
			SettingsCommand.Initialize(this);
			VersionCommand.Initialize(this);

			_buildVersionIncrementor = new BuildVersionIncrementor(this);
			_buildEvents = DTE.Events.BuildEvents;
			_buildVersionIncrementor.InitializeIncrementors();

			_buildEvents.OnBuildBegin += _buildVersionIncrementor.OnBuildBegin;
			_buildEvents.OnBuildDone += _buildVersionIncrementor.OnBuildDone;

			

			base.Initialize();
		}

		private void CheckCommandLine()
		{
			var commandLine = (IVsAppCommandLine)GetService(typeof(IVsAppCommandLine));

			var isPresent = 0;

			if (commandLine != null)
			{
				string optionValue;
				commandLine.GetOption("IncrementVersion", out isPresent, out optionValue);
			}

			IsCommandLine = isPresent != 0;
		}
		
	}
}