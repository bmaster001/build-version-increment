// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: VersioningStyle.cs
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
	using System;
	using System.ComponentModel;
	using System.Diagnostics;

	using Incrementors;

	using UI;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	[TypeConverter(typeof(ExpandableObjectConverter))]
	internal class VersioningStyle
	{
		private IncrementorBase _build = BuiltInIncrementorBase.None;

		private IncrementorBase _major = BuiltInIncrementorBase.None;

		private IncrementorBase _minor = BuiltInIncrementorBase.None;

		private IncrementorBase _revision = BuiltInIncrementorBase.None;

		public VersioningStyle() {}

		public VersioningStyle(VersioningStyle other)
		{
			Major = other.Major;
			Minor = other.Minor;
			Build = other.Build;
			Revision = other.Revision;
		}

		[Description("Build update style")]
		[NotifyParentProperty(true)]
		[ItemsSource(typeof(IncrementorItemsSource))]
		[PropertyOrder(3)]
		public IncrementorBase Build
		{
			get { return _build ?? BuiltInIncrementorBase.None; }
			set
			{
				Debug.Assert(value != null);
				_build = value;
			}
		}

		[Description("Major update style")]
		[NotifyParentProperty(true)]
		[ItemsSource(typeof(IncrementorItemsSource))]
		[PropertyOrder(1)]
		public IncrementorBase Major
		{
			get { return _major ?? BuiltInIncrementorBase.None; }
			set
			{
				Debug.Assert(value != null);
				_major = value;
			}
		}

		[Description("Minor update style")]
		[NotifyParentProperty(true)]
		[ItemsSource(typeof(IncrementorItemsSource))]
		[PropertyOrder(2)]
		public IncrementorBase Minor
		{
			get { return _minor ?? BuiltInIncrementorBase.None; }
			set
			{
				Debug.Assert(value != null);
				_minor = value;
			}
		}

		[Description("Revision update style")]
		[NotifyParentProperty(true)]
		[ItemsSource(typeof(IncrementorItemsSource))]
		[PropertyOrder(4)]
		public IncrementorBase Revision
		{
			get { return _revision ?? BuiltInIncrementorBase.None; }
			set
			{
				Debug.Assert(value != null);
				_revision = value;
			}
		}

		public override string ToString()
		{
			return $"{Major.Name}.{Minor.Name}.{Build.Name}.{Revision.Name}";
		}

		internal static string GetDefaultGlobalVariable()
		{
			return new VersioningStyle().ToGlobalVariable();
		}

		internal void FromGlobalVariable(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				var styles = value.Split(".".ToCharArray());

				if (styles.Length == 4)
				{
					Major = BuildVersionIncrementor.Instance.Incrementors[styles[0]];
					Minor = BuildVersionIncrementor.Instance.Incrementors[styles[1]];
					Build = BuildVersionIncrementor.Instance.Incrementors[styles[2]];
					Revision = BuildVersionIncrementor.Instance.Incrementors[styles[3]];
				}
				else
				{
					throw (new ApplicationException($"Invalid versioning style \"{value}\"."));
				}
			}
			else
			{
				Major = Minor = Build = Revision = null;
			}
		}

		internal StringVersion Increment(StringVersion currentVersion,
		                                 DateTime buildStartDate,
		                                 DateTime projectStartDate,
		                                 SolutionItem solutionItem)
		{
			var context = new IncrementContext(currentVersion,
			                                   buildStartDate,
			                                   projectStartDate,
			                                   solutionItem.Filename);

			var incrementors = new[] {Major, Minor, Build, Revision};

			for (var i = 0; i < 4; i++)
			{
				var incrementor = incrementors[i];

				if (incrementor == null)
				{
					continue;
				}

				var component = (VersionComponent)i;

				incrementor.Execute(context, component);

				if (!context.Continue)
				{
					break;
				}
			}

			return context.NewVersion;
		}

		internal string ToGlobalVariable()
		{
			return ToString();
		}

		#region Ugly PropertyGrid reflection code

		private bool ShouldSerializeMajor()
		{
			return _major != BuiltInIncrementorBase.None;
		}

		private bool ShouldSerializeMinor()
		{
			return _minor != BuiltInIncrementorBase.None;
		}

		private bool ShouldSerializeBuild()
		{
			return _build != BuiltInIncrementorBase.None;
		}

		private bool ShouldSerializeRevision()
		{
			return _revision != BuiltInIncrementorBase.None;
		}

		#endregion
	}
}