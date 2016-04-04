// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: SettingsCommand.cs
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
	using System;
	using System.ComponentModel.Design;

	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	internal sealed class SettingsCommand
	{
		public const int CommandId = 0x0100;

		public static readonly Guid CommandSet = new Guid("1a42bbb0-f5ad-4882-bf32-623425c6d577");

		private readonly Package package;

		private SettingsCommand(Package package)
		{
			if (package == null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			this.package = package;

			var commandService =
				ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (commandService == null)
			{
				return;
			}
			var menuCommandId = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(ShowSettingsDialog, menuCommandId);
			commandService.AddCommand(menuItem);
		}

		public static SettingsCommand Instance { get; private set; }

		private IServiceProvider ServiceProvider => package;

		public static void Initialize(Package package)
		{
			Instance = new SettingsCommand(package);
		}

		private void ShowSettingsDialog(object sender, EventArgs e)
		{
			var dialog = new SettingsDialog();
			dialog.ShowModal();
		}
	}
}