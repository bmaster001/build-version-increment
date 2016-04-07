// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: IconReader.cs
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
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Runtime.InteropServices;

	using Model;

	public static class IconReader
	{
		private static readonly Dictionary<string, Icon> _cache = new Dictionary<string, Icon>();

		public static Icon AddFileIcon(string filePath)
		{
			var extension = Path.GetExtension(filePath)?.TrimStart(".".ToCharArray());

			if (string.IsNullOrEmpty(extension))
			{
				return null;
			}

			if (!_cache.ContainsKey(extension))
			{
				_cache.Add(extension, GetFileIcon(filePath, IconSize.Small, false));
			}
			return _cache[extension];
		}

		public static Icon AddFolderIcon(FolderType type)
		{
			var key = $"folder_{type}";
			if (!_cache.ContainsKey(key))
			{
				_cache.Add(key, GetFolderIcon(IconSize.Small, type));
			}
			return _cache[key];
		}

		public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay)
		{
			var shfi = new Shell32.SHFILEINFO();
			var flags = Shell32.SHGFI_ICON | Shell32.SHGFI_USEFILEATTRIBUTES;

			if (linkOverlay)
			{
				flags |= Shell32.SHGFI_LINKOVERLAY;
			}

			/* Check the size specified for return. */
			if (IconSize.Small == size)
			{
				flags |= Shell32.SHGFI_SMALLICON;
			}
			else
			{
				flags |= Shell32.SHGFI_LARGEICON;
			}

			Shell32.SHGetFileInfo(name,
			                      Shell32.FILE_ATTRIBUTE_NORMAL,
			                      ref shfi,
			                      (uint)Marshal.SizeOf(shfi),
			                      flags);

			var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
			User32.DestroyIcon(shfi.hIcon);
			return icon;
		}

		public static Icon GetFolderIcon(IconSize size, FolderType folderType)
		{
			var flags = Shell32.SHGFI_ICON | Shell32.SHGFI_USEFILEATTRIBUTES;

			if (FolderType.Open == folderType)
			{
				flags |= Shell32.SHGFI_OPENICON;
			}

			if (IconSize.Small == size)
			{
				flags |= Shell32.SHGFI_SMALLICON;
			}
			else
			{
				flags |= Shell32.SHGFI_LARGEICON;
			}

			var shfi = new Shell32.SHFILEINFO();
			Shell32.SHGetFileInfo(Environment.CurrentDirectory,
			                      Shell32.FILE_ATTRIBUTE_DIRECTORY,
			                      ref shfi,
			                      (uint)Marshal.SizeOf(shfi),
			                      flags);

			Icon.FromHandle(shfi.hIcon);

			var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

			User32.DestroyIcon(shfi.hIcon);
			return icon;
		}
	}
}