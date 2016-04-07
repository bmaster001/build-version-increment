// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: GlobalVariables.cs
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
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	using EnvDTE;

	public static class GlobalVariables
	{
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "globals")]
		public static string GetGlobalVariable(Globals globals, string varName, string defaultValue)
		{
			if (globals == null)
			{
				return defaultValue;
			}

			var names = (object[])globals.VariableNames;

			if (!globals.VariableExists[varName])
			{
				return defaultValue;
			}
			if (names.Any(name => name.ToString() == varName))
			{
				return (string)globals[varName];
			}

			return defaultValue;
		}

		public static void SetGlobalVariable(Globals globals,
		                                     string variableName,
		                                     string value,
		                                     string defaultValue)
		{
			if (globals == null)
			{
				return;
			}

			var globalValue = GetGlobalVariable(globals, variableName, null);

			if (globalValue != null)
			{
				if (string.Compare(globalValue, value, StringComparison.OrdinalIgnoreCase) != 0)
				{
					SetGlobalVariable(globals, variableName, value);
				}
			}
			else if (string.Compare(value, defaultValue, StringComparison.OrdinalIgnoreCase) != 0)
			{
				SetGlobalVariable(globals, variableName, value);
			}
		}

		public static void SetGlobalVariable(Globals globals, string varName, string value)
		{
			globals[varName] = value;

			globals.VariablePersists[varName] = true;
		}
	}
}