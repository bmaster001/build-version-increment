// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: SolutionDependantCommandBase.cs
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

	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal abstract class SolutionDependantCommandBase : MenuCommandBase<OleMenuCommand>
	{
		internal SolutionDependantCommandBase(Package package) : base(package) {}

		protected override void InitialiseEvents(OleMenuCommand command)
		{
			command.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
			base.InitialiseEvents(command);
		}

		private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			var menuCommand = sender as OleMenuCommand;
			if (menuCommand == null)
			{
				return;
			}
			menuCommand.Visible = false;
			menuCommand.Enabled = false;

			var solution = ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;

			if (solution == null)
			{
				return;
			}

			object objLoaded;
			solution.GetProperty((int)__VSPROPID4.VSPROPID_IsSolutionFullyLoaded, out objLoaded);
			if (objLoaded == null)
			{
				return;
			}

			var loaded = Convert.ToBoolean(objLoaded);

			menuCommand.Visible = loaded;
			menuCommand.Enabled = loaded;
		}
	}
}