// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: ConfigurationStringConverter.cs
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

namespace BuildVersionIncrement.Model
{
	using System.Collections;
	using System.ComponentModel;
	using System.Linq;

	using EnvDTE;

	internal class ConfigurationStringConverter : TypeConverter
	{
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			var solutionItem = ((SolutionItemIncrementSettings)context.Instance).SolutionItem;

			var items = CreateList(solutionItem);

			return items != null ? new StandardValuesCollection(items) : null;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		private static object[] CreateList(SolutionItem solutionItem)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			var ret = new ArrayList {"Any"};

			if (solutionItem.ItemType == SolutionItemType.Solution)
			{
				var configs = solutionItem.Solution.SolutionBuild.SolutionConfigurations;

				string[] lastConfigName = {""};

				foreach (
					var config in
						configs.Cast<SolutionConfiguration>().Where(config => { Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread(); return lastConfigName[0] != config.Name; }))
				{
					ret.Add(config.Name);
					lastConfigName[0] = config.Name;
				}
			}
			else
			{
				var names = (object[])solutionItem.Project.ConfigurationManager.ConfigurationRowNames;

				foreach (var o in names)
				{
					ret.Add(o);
				}
			}

			return ret.ToArray();
		}
	}
}