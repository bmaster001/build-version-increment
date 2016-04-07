// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: VersionCommand.cs
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

namespace BuildVersionIncrement.Commands
{
	using System;
	using System.ComponentModel.Design;

	using Logging;

	using Microsoft.VisualStudio.Shell;

	internal sealed class VersionCommand : SolutionDependantCommandBase
	{
		public VersionCommand(Package package) : base(package) {}

		public static VersionCommand Instance { get; private set; }
		public override int CommandId => Constants.COMMAND_ID_VERSION;

		public static void Initialize(Package package)
		{
			Logger.Write("Initialising package", LogLevel.Debug);
			Instance = new VersionCommand(package);
		}

		protected override OleMenuCommand GetCommand(CommandID menuCommandId)
		{
			return new OleMenuCommand(ManuallyVersionAssemblies, menuCommandId);
		}

		private static void ManuallyVersionAssemblies(object sender, EventArgs e) {}
	}
}