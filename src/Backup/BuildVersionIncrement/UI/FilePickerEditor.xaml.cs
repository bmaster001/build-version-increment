// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: FilePickerEditor.xaml.cs
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
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;

	using Microsoft.Win32;

	using Xceed.Wpf.Toolkit.PropertyGrid;
	using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

	/// <summary>
	///     Interaction logic for FilePickerEditor.xaml
	/// </summary>
	public partial class FilePickerEditor : UserControl, ITypeEditor
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
		                                                                                      typeof(
			                                                                                      string),
		                                                                                      typeof(
			                                                                                      FilePickerEditor
			                                                                                      ));

		public FilePickerEditor()
		{
			InitializeComponent();
		}

		public string Value
		{
			get { return (string)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public FrameworkElement ResolveEditor(PropertyItem propertyItem)
		{
			var binding = new Binding("Value")
			              {
				              Source = propertyItem,
				              Mode =
					              propertyItem.IsReadOnly
						              ? BindingMode.OneWay
						              : BindingMode.TwoWay
			              };
			BindingOperations.SetBinding(this, ValueProperty, binding);
			return this;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			             {
				             DefaultExt = ".cs",
				             Filter =
					             "C# files (*.cs)|*.cs|VB files (*.vb)|*.vb|All files (*.*)|*.*"
			             };

			var result = dialog.ShowDialog();

			if (!result.HasValue || !result.Value)
			{
				return;
			}
			var filename = dialog.FileName;
			Value = filename;
		}
	}
}