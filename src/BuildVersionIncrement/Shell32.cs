// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: Shell32.cs
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
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.InteropServices;

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly",
		Scope = "namespace", Target = "Shell32")]
	public static class Shell32
	{
		public const uint BIF_BROWSEFORCOMPUTER = 0x1000;
		public const uint BIF_BROWSEFORPRINTER = 0x2000;
		public const uint BIF_BROWSEINCLUDEFILES = 0x4000;
		public const uint BIF_BROWSEINCLUDEURLS = 0x0080;
		public const uint BIF_DONTGOBELOWDOMAIN = 0x0002;
		public const uint BIF_EDITBOX = 0x0010;
		public const uint BIF_NEWDIALOGSTYLE = 0x0040;
		public const uint BIF_RETURNFSANCESTORS = 0x0008;

		public const uint BIF_RETURNONLYFSDIRS = 0x0001;
		public const uint BIF_SHAREABLE = 0x8000;
		public const uint BIF_STATUSTEXT = 0x0004;
		public const uint BIF_USENEWUI = (BIF_NEWDIALOGSTYLE | BIF_EDITBOX);
		public const uint BIF_VALIDATE = 0x0020;

		public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
		public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
		public const int MAX_PATH = 256;
		public const uint SHGFI_ADDOVERLAYS = 0x000000020;
		public const uint SHGFI_ATTR_SPECIFIED = 0x000020000;
		public const uint SHGFI_ATTRIBUTES = 0x000000800;
		public const uint SHGFI_DISPLAYNAME = 0x000000200;
		public const uint SHGFI_EXETYPE = 0x000002000;

		public const uint SHGFI_ICON = 0x000000100;
		public const uint SHGFI_ICONLOCATION = 0x000001000;
		public const uint SHGFI_LARGEICON = 0x000000000;
		public const uint SHGFI_LINKOVERLAY = 0x000008000;
		public const uint SHGFI_OPENICON = 0x000000002;
		public const uint SHGFI_OVERLAYINDEX = 0x000000040;
		public const uint SHGFI_PIDL = 0x000000008;
		public const uint SHGFI_SELECTED = 0x000010000;
		public const uint SHGFI_SHELLICONSIZE = 0x000000004;
		public const uint SHGFI_SMALLICON = 0x000000001;
		public const uint SHGFI_SYSICONINDEX = 0x000004000;
		public const uint SHGFI_TYPENAME = 0x000000400;
		public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

		[DllImport("Shell32.dll")]
		public static extern IntPtr SHGetFileInfo(string pszPath,
		                                          uint dwFileAttributes,
		                                          ref SHFILEINFO psfi,
		                                          uint cbFileInfo,
		                                          uint uFlags);

		[StructLayout(LayoutKind.Sequential)]
		public struct BROWSEINFO
		{
			public IntPtr hwndOwner;
			public IntPtr pidlRoot;
			public IntPtr pszDisplayName;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpszTitle;

			public uint ulFlags;
			public IntPtr lpfn;
			public int lParam;
			public IntPtr iImage;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ITEMIDLIST
		{
			public SHITEMID mkid;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SHFILEINFO
		{
			public const int NAMESIZE = 80;
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string szDisplayName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAMESIZE)]
			public string szTypeName;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SHITEMID
		{
			public ushort cb;

			[MarshalAs(UnmanagedType.LPArray)]
			public byte[] abID;
		}
	}
}