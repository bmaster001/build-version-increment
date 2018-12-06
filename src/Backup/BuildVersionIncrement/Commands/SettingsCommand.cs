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

namespace BuildVersionIncrement.Commands
{
	using System;
	using System.ComponentModel.Design;

	using Microsoft.VisualStudio.Shell;

	using UI;

	internal sealed class SettingsCommand : MenuCommandBase<MenuCommand>
	{
		private readonly Package _package;

		private SettingsCommand(Package package) : base(package)
		{
			_package = package;
		}

		public static SettingsCommand Instance { get; private set; }

		public override int CommandId => Constants.COMMAND_ID_SETTINGS;

		public static void Initialize(Package package)
		{
			Instance = new SettingsCommand(package);
		}

		protected override MenuCommand GetCommand(CommandID menuCommandId)
		{
			return new MenuCommand(ShowSettingsDialog, menuCommandId);
		}

		private void ShowSettingsDialog(object sender, EventArgs e)
		{
			var dialog = new SettingsDialog(_package);
			dialog.ShowModal();
		}
	}
}