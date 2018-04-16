using System;
using System.Collections.Generic;
using System.Text;

// No documentation done. Need help of others here.
namespace BuildVersionIncrement
{
	public class StringVersion
	{
		public string Major { get; set; }
		public string Minor { get; set; }
		public string Build { get; set; }
		public string Revision { get; set; }

		public StringVersion(string fullVersion)
		{
			string[] components = fullVersion.Split('.');
			int componentCount = components.Length;
			if (componentCount < 2 || componentCount > 4)
				throw new ArgumentException();

			Major = components[0];
			Minor = components[1];
			if (componentCount > 2)
				Build = components[2];
			if (componentCount > 3)
				Revision = components[3];
		}

		public StringVersion(string major, string minor, string build, string revision)
		{
			Major = major;
			Minor = minor;
			Build = build;
			Revision = revision;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Major + "." + Minor);

			if (Build != null)
				sb.Append("." + Build);
			if (Revision != null)
				sb.Append("." + Revision);
			return sb.ToString();
		}

		public string ToString(int componentCount)
		{
			if (componentCount < 0 || componentCount > 4)
				throw new ArgumentException();

			StringBuilder sb = new StringBuilder();
			if (componentCount > 0)
				sb.Append(Major);
			if (componentCount > 1)
				sb.Append("." + Minor);
			if (componentCount > 2)
				sb.Append("." + (Build ?? "0"));
			if (componentCount > 3)
				sb.Append("." + (Revision ?? "0"));

			return sb.ToString();
		}

		public static bool operator ==(StringVersion a, StringVersion b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
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
			StringVersion sv = obj as StringVersion;
			if ((object)sv == null)
			{
				return false;
			}

			// Return true if the fields match:
			return this == sv;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		} 
	}
}
