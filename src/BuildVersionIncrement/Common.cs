// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: Common.cs
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
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;

	public class Common
	{
		public static string MakeAbsolutePath(string basePath, string relativePath)
		{
			var directorySeparatorCharArray = new[] {Path.DirectorySeparatorChar};

			if (string.IsNullOrEmpty(basePath))
			{
				throw new ArgumentNullException(nameof(basePath));
			}

			if (string.IsNullOrEmpty(relativePath))
			{
				throw new ArgumentNullException(nameof(relativePath));
			}

			if (relativePath.Contains(".."))
			{
				var relativeFolders =
					new List<string>(relativePath.Split(directorySeparatorCharArray,
					                                    StringSplitOptions.RemoveEmptyEntries));
				var baseFolders =
					new List<string>(basePath.Split(directorySeparatorCharArray,
					                                StringSplitOptions.RemoveEmptyEntries));

				for (var i = 0; i < relativeFolders.Count; i++)
				{
					if (relativeFolders[i] != "..")
					{
						continue;
					}
					if (i == 0)
					{
						if (baseFolders.Count > 1)
						{
							baseFolders.RemoveAt(baseFolders.Count - 1);
						}

						relativeFolders.RemoveAt(0);

						i = -1;
					}
					else
					{
						relativeFolders.RemoveRange(i - 1, 2);

						i -= 2;
					}
				}

				var sb = new StringBuilder();

				foreach (var folder in relativeFolders)
				{
					sb.Append(folder);
					sb.Append(Path.DirectorySeparatorChar);
				}

				relativePath = sb.ToString().TrimEnd(directorySeparatorCharArray);

				sb.Length = 0;

				foreach (var folder in baseFolders)
				{
					sb.Append(folder);
					sb.Append(Path.DirectorySeparatorChar);
				}

				basePath = sb.ToString().TrimEnd(directorySeparatorCharArray);
			}

			var relativeRoot = Path.GetPathRoot(relativePath);

			if (string.IsNullOrEmpty(relativeRoot))
			{
				return Path.Combine(
					basePath.TrimEnd(directorySeparatorCharArray) + Path.DirectorySeparatorChar,
					relativePath);
			}
			if (relativeRoot[0] == Path.DirectorySeparatorChar)
			{
				return Path.GetPathRoot(basePath).TrimEnd(Path.DirectorySeparatorChar) + relativePath;
			}
			return relativePath;
		}

		public static string MakeRelativePath(string basePath, string targetPath)
		{
			if (string.IsNullOrEmpty(basePath))
			{
				throw new ArgumentNullException(nameof(basePath));
			}

			if (string.IsNullOrEmpty(targetPath))
			{
				throw new ArgumentNullException(nameof(targetPath));
			}

			var isRooted = Path.IsPathRooted(basePath) && Path.IsPathRooted(targetPath);

			if (isRooted)
			{
				var isDifferentRoot = string.Compare(Path.GetPathRoot(basePath),
				                                     Path.GetPathRoot(targetPath),
				                                     StringComparison.OrdinalIgnoreCase) != 0;

				if (isDifferentRoot)
				{
					return targetPath;
				}
			}

			var relativePath = new StringCollection();

			var fromDirectories = basePath.Split(Path.DirectorySeparatorChar);
			var toDirectories = targetPath.Split(Path.DirectorySeparatorChar);

			var length = Math.Min(fromDirectories.Length, toDirectories.Length);
			var lastCommonRoot = -1;

			for (var x = 0; x < length; x++)
			{
				if (string.Compare(fromDirectories[x], toDirectories[x], StringComparison.OrdinalIgnoreCase)
				    != 0)
				{
					break;
				}

				lastCommonRoot = x;
			}

			if (lastCommonRoot == -1)
			{
				return targetPath;
			}

			for (var x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
			{
				if (fromDirectories[x].Length > 0)
				{
					relativePath.Add("..");
				}
			}

			for (var x = lastCommonRoot + 1; x < toDirectories.Length; x++)
			{
				relativePath.Add(toDirectories[x]);
			}

			var relativeParts = new string[relativePath.Count];

			relativePath.CopyTo(relativeParts, 0);
			var newPath = string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);

			return newPath;
		}
	}
}