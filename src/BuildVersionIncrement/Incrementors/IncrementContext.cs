// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: IncrementContext.cs
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

namespace BuildVersionIncrement.Incrementors
{
	using System;

	using Model;

	public class IncrementContext
	{
		private StringVersion _newVersion;

		internal IncrementContext(StringVersion currentVersion,
		                          DateTime buildStartDate,
		                          DateTime projectStartDate,
		                          string projectFilename)
		{
			CurrentVersion = currentVersion;
			BuildStartDate = buildStartDate;
			ProjectStartDate = projectStartDate;
			ProjectFilename = projectFilename;

			NewVersion = new StringVersion(currentVersion.Major,
			                               currentVersion.Minor,
			                               currentVersion.Build,
			                               currentVersion.Revision);
			Continue = true;
		}

		
		public DateTime BuildStartDate { get; private set; }

		
		public bool Continue { get; set; }

		
		public StringVersion CurrentVersion { get; }

		
		public StringVersion NewVersion
		{
			get { return _newVersion; }
			set
			{
				if (value == null)
				{
					throw (new ArgumentNullException());
				}

				_newVersion = value;
			}
		}

		
		public string ProjectFilename { get; private set; }

		
		public DateTime ProjectStartDate { get; private set; }

		
		public string GetCurrentVersionComponentValue(VersionComponent component)
		{
			switch (component)
			{
				case VersionComponent.Build:
					return CurrentVersion.Build;

				case VersionComponent.Major:
					return CurrentVersion.Major;

				case VersionComponent.Minor:
					return CurrentVersion.Minor;

				case VersionComponent.Revision:
					return CurrentVersion.Revision;
			}

			return "0";
		}

		
		public void SetNewVersionComponentValue(VersionComponent component, string value)
		{
			switch (component)
			{
				case VersionComponent.Build:
					NewVersion = new StringVersion(NewVersion.Major, NewVersion.Minor, value, NewVersion.Revision);
					break;

				case VersionComponent.Major:
					NewVersion = new StringVersion(value, NewVersion.Minor, NewVersion.Build, NewVersion.Revision);
					break;

				case VersionComponent.Minor:
					NewVersion = new StringVersion(NewVersion.Major, value, NewVersion.Build, NewVersion.Revision);
					break;

				case VersionComponent.Revision:
					NewVersion = new StringVersion(NewVersion.Major, NewVersion.Minor, NewVersion.Build, value);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(component), component, null);
			}
		}
	}
}