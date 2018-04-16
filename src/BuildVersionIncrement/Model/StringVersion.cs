// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: StringVersion.cs
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

 // No documentation done. Need help of others here.

namespace BuildVersionIncrement.Model
{
	using System;
	using System.Text;

	public class StringVersion
	{
		public StringVersion(string fullVersion)
		{
			var components = fullVersion.Split('.');
			var componentCount = components.Length;
			if (componentCount < 2 || componentCount > 4)
			{
				throw new ArgumentException();
			}

			Major = components[0];
			Minor = components[1];
			if (componentCount > 2)
			{
				Build = components[2];
			}
			if (componentCount > 3)
			{
				Revision = components[3];
			}
		}

		public StringVersion(string major, string minor, string build, string revision)
		{
			Major = major;
			Minor = minor;
			Build = build;
			Revision = revision;
		}

		public string Build { get; set; }
		public string Major { get; set; }
		public string Minor { get; set; }
		public string Revision { get; set; }

		public static bool operator ==(StringVersion a, StringVersion b)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}

			// Return true if the fields match:
			return a.Major == b.Major && a.Minor == b.Minor && a.Build == b.Build && a.Revision == b.Revision;
		}

		public static bool operator !=(StringVersion a, StringVersion b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			// If parameter cannot be cast to ThreeDPoint return false:
			var sv = obj as StringVersion;
			if ((object)sv == null)
			{
				return false;
			}

			// Return true if the fields match:
			return this == sv;
		}

		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Major + "." + Minor);

			if (Build != null)
			{
				sb.Append("." + Build);
			}
			if (Revision != null)
			{
				sb.Append("." + Revision);
			}
			return sb.ToString();
		}

		public string ToString(int componentCount)
		{
			if (componentCount < 0 || componentCount > 4)
			{
				throw new ArgumentException();
			}

			var sb = new StringBuilder();
			if (componentCount > 0)
			{
				sb.Append(Major);
			}
			if (componentCount > 1)
			{
				sb.AppendFormat(".{0}", Minor);
			}
			if (componentCount > 2)
			{
				sb.AppendFormat(".{0}", (Build ?? "0"));
			}
			if (componentCount > 3)
			{
				sb.AppendFormat(".{0}", (Revision ?? "0"));
			}

			return sb.ToString();
		}
	}
}