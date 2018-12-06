// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: SettingsDialog.xaml.cs
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

namespace BuildVersionIncrement.UI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;

	using EnvDTE;

	using Extensions;

	using Helpers;

	using Logging;

	using Microsoft.VisualStudio.Shell;

	using Model;

	using Properties;

	/// <summary>
	///     Interaction logic for SettingsDialog.xaml
	/// </summary>
	public partial class SettingsDialog
	{
		private readonly Package _package;
		private GlobalIncrementSettings _globalSettings;
		private SolutionItem _solution;

		internal SettingsDialog(Package package)
		{
			InitializeComponent();

			ShowInTaskbar = false;

			Logger.WriteEvent += Logger_WriteEvent;

			_package = package;
			_globalSettings = new GlobalIncrementSettings();

			GlobalSettingsPropertyGrid.IsCategorized = true;
			SolutionSettingsPropertyGrid.IsCategorized = true;

			try
			{
				var version =
					ReflectionHelper.GetAssemblyAttribute<AssemblyFileVersionAttribute>(
						Assembly.GetExecutingAssembly()).Version;
				var configuration =
					ReflectionHelper.GetAssemblyAttribute<AssemblyConfigurationAttribute>(
						Assembly.GetExecutingAssembly()).Configuration;

				Title = $"{Title} v{version} [{configuration}]";
			}

			catch (Exception ex)
			{
				ex.Swallow();
			}
		}

		internal IncrementSettingsBase SelectedIncrementSettings
		{
			get
			{
				var solutionItem = SolutionTreeView.SelectedItem as SolutionItem;
				if (solutionItem == null)
				{
					return null;
				}
				if (solutionItem.ItemType == SolutionItemType.Project
				    || solutionItem.ItemType == SolutionItemType.Solution)
				{
					return solutionItem.IncrementSettings;
				}
				return null;
			}
		}

		private static ImageSource FolderClosed
			=> IconReader.AddFolderIcon(FolderType.Closed).ToImageSource();

		private static ImageSource FolderOpen => IconReader.AddFolderIcon(FolderType.Open).ToImageSource()
			;

		private DTE DTE
		{
			get
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
				return (DTE)ServiceProvider.GetService(typeof(DTE));
			}
		}

		private IServiceProvider ServiceProvider => _package;

		private static void CopySettingsToAll(SolutionItem item, IncrementSettingsBase settings)
		{
			if (item.ItemType == SolutionItemType.Solution || item.ItemType == SolutionItemType.Project)
			{
				Logger.Write($"Copying increment settings to \"{item.Name}\"", LogLevel.Debug);
				item.IncrementSettings.CopyFrom(settings);
			}

			foreach (var child in item.SubItems)
			{
				CopySettingsToAll(child, settings);
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void CopyToAllProjectsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var settings = SelectedIncrementSettings;
			if (settings == null)
			{
				return;
			}
			var name = "Global settings";
			if (settings is SolutionItemIncrementSettings)
			{
				name = ((SolutionItemIncrementSettings)settings).Name;
			}

			var result = MessageBox.Show(this,
			                             $"Copy the increment settings of \"{name}\" to all other items?",
			                             "Copy to all",
			                             MessageBoxButton.YesNo,
			                             MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				CopySettingsToAll(_solution, settings);
			}
		}

		private void CopyToGlobalSettingsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var settings = SelectedIncrementSettings as SolutionItemIncrementSettings;
			if (settings == null)
			{
				return;
			}

			var result = MessageBox.Show(this,
			                             $"Set the increment settings of \"{settings.Name}\" as global settings?",
			                             "Set as global settings",
			                             MessageBoxButton.YesNo,
			                             MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				var item = (SolutionItem)SolutionTreeView.SelectedItem;
				Logger.Write($"Copying from \"{item.Name}\" to global settings", LogLevel.Debug);
				_globalSettings.CopyFrom(settings);
				GlobalSettingsPropertyGrid.Update();
			}
		}

		private void DialogWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			try
			{
				_globalSettings.Load();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while loading the global settings:\n{ex}",
				                "Global Settings Error",
				                MessageBoxButton.OK,
				                MessageBoxImage.Error);
				_globalSettings = new GlobalIncrementSettings();
			}

			LoadSolution();

			GlobalSettingsPropertyGrid.SelectedObject = _globalSettings;
			VerboseLoggingEnabledCheckbox.IsChecked = Settings.Default.IsVerboseLogEnabled;
			VersionEnabledCheckBox.IsChecked = Settings.Default.IsEnabled;
			LogTextBox.AppendText(Logger.Instance.Contents);
		}

		private void LoadSolution()
		{
			try
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				if (DTE.Solution == null || !DTE.Solution.IsOpen)
				{
					SolutionSetttingsTab.IsEnabled = false;
					return;
				}
				_solution = new SolutionItem(_package, DTE.Solution);
				_solution.IsExpanded = _solution.IsSelected = true;
				var solutions = new List<SolutionItem> {_solution};
				SolutionTreeView.ItemsSource = solutions;
				SolutionTreeView.Items.SortDescriptions.Clear();
				SolutionTreeView.Items.SortDescriptions.Add(new SortDescription("Name",
				                                                                ListSortDirection.Ascending));
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error occurred while building the solution tree.\n{ex}",
				                "Solution error",
				                MessageBoxButton.OK,
				                MessageBoxImage.Error);
			}
		}

		private void Logger_WriteEvent(object sender, Logger.WriteEventArgs e)
		{
			LogTextBox.AppendText(e.Message);
		}

		private void Okay_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			if (_globalSettings.Apply == GlobalIncrementSettings.ApplyGlobalSettings.Always)
			{
				MessageBox.Show(
					BuildVersionIncrement.Properties.Resources.GlobalMessage_alwaysApplyGlobalSettings,
					"Warning",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}

			try
			{
				_globalSettings.Save();
			}
			catch (Exception ex)
			{
				var message = $"Failed to save default settings:\n{ex}";
				Logger.Write(message, LogLevel.Error);
				MessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			try
			{
				Settings.Default.IsEnabled = VersionEnabledCheckBox.IsChecked.HasValue && VersionEnabledCheckBox.IsChecked.Value;
				Settings.Default.IsVerboseLogEnabled = VerboseLoggingEnabledCheckbox.IsChecked.HasValue && VerboseLoggingEnabledCheckbox.IsChecked.Value;
				Settings.Default.Save();
			}
			catch (Exception ex)
			{
				var message = $"Failed to save global settings:\n{ex}";
				Logger.Write(message, LogLevel.Error);
				MessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			try
			{
				_solution?.SetGlobalVariables();
				Close();
			}
			catch (Exception ex)
			{
				var message = $"Failed to save solution settings:\n{ex}";
				Logger.Write(message, LogLevel.Error);
				MessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ResetToDefaultsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var settings = SelectedIncrementSettings;
			if (settings == null)
			{
				return;
			}
			var item = (SolutionItem)SolutionTreeView.SelectedItem;
			var result = MessageBox.Show(this,
			                             $"Reset setting of \"{item.Name}\" to the defaults?",
			                             "Reset settings",
			                             MessageBoxButton.YesNo,
			                             MessageBoxImage.Question);

			if (result != MessageBoxResult.Yes)
			{
				return;
			}
			Logger.Write($"Resetting settings of \"{item.Name}\" to defaults", LogLevel.Debug);
			settings.Reset();
			SolutionSettingsPropertyGrid.Update();
		}

		private void SolutionTreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			var settings = SelectedIncrementSettings;
			if (settings == null)
			{
				e.Handled = true;
			}

			CopyToGlobalSettingsMenuItem.IsEnabled = settings is SolutionItemIncrementSettings;
		}

		private void SolutionTreeView_OnExpanded(object sender, RoutedEventArgs e)
		{
			var item = e.OriginalSource as TreeViewItem;
			var solutionItem = item?.DataContext as SolutionItem;
			if (solutionItem == null || solutionItem.ItemType != SolutionItemType.Folder)
			{
				return;
			}

			solutionItem.Icon = item.IsExpanded ? FolderOpen : FolderClosed;
		}

		private void SolutionTreeView_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			
			var solutionItem = ((FrameworkElement)e.OriginalSource).DataContext as SolutionItem;

			if (solutionItem != null)
			{
				solutionItem.IsSelected = true;
			}
		}

		private void SolutionTreeView_SelectedItemChanged(object sender,
		                                                  RoutedPropertyChangedEventArgs<object> e)
		{
			var item = e.NewValue as SolutionItem;
			if (item == null || item.ItemType == SolutionItemType.Folder
			    || item.ItemType == SolutionItemType.None)
			{
				SolutionSettingsPropertyGrid.IsEnabled = true;
				SolutionSettingsPropertyGrid.SelectedObject = null;
				return;
			}
			SolutionSettingsPropertyGrid.IsEnabled = true;
			SolutionSettingsPropertyGrid.SelectedObject = item.IncrementSettings;
		}

		private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var settings = SelectedIncrementSettings;
			if (settings == null)
			{
				return;
			}
			var item = (SolutionItem)SolutionTreeView.SelectedItem;
			var result = MessageBox.Show(this,
			                             $"Discard changes to \"{item.Name}\"?",
			                             "Undo changes",
			                             MessageBoxButton.YesNo,
			                             MessageBoxImage.Question);

			if (result != MessageBoxResult.Yes)
			{
				return;
			}
			Logger.Write($"Discarding changes to \"{item.Name}\".", LogLevel.Debug);
			settings.Load();
			SolutionSettingsPropertyGrid.Update();
		}
	}
}