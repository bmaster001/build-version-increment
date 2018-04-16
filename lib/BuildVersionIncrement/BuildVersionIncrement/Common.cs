using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Reflection;
using EnvDTE;

namespace BuildVersionIncrement
{
    public static class Common
    {
        /// <summary>
        /// Converts a relative path to an absolute path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static string MakeAbsolutePath(string basePath, string relativePath)
        {
            char[] directorySeparatorCharArray = new char[]{Path.DirectorySeparatorChar};

            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException("basePath");

            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentNullException("relativePath");

            if (relativePath.Contains(".."))
            {
                List<string> relativeFolders = new List<string>(relativePath.Split(directorySeparatorCharArray, StringSplitOptions.RemoveEmptyEntries));
                List<string> baseFolders = new List<string>(basePath.Split(directorySeparatorCharArray, StringSplitOptions.RemoveEmptyEntries));

                for (int i = 0; i < relativeFolders.Count; i++)
                {
                    if (relativeFolders[i] == "..") // Up folder?
                    {
                        if (i == 0)
                        {
                            // If we're in the beginning of the path remove the current entry of the 
                            // relative path and the last entry of the base path (but never further than the root)

                            if(baseFolders.Count > 1)
                                baseFolders.RemoveAt(baseFolders.Count - 1);

                            relativeFolders.RemoveAt(0);

                            // Force the index back to 0

                            i = -1;
                        }
                        else
                        {
                            // Double dot specified somewhere between folders. Remove the current entry and 
                            // the one before.

                            relativeFolders.RemoveRange(i-1, 2);
                            
                            // Adjust the index

                            i -= 2;
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();

                foreach (string folder in relativeFolders)
                {
                    sb.Append(folder);
                    sb.Append(Path.DirectorySeparatorChar);
                }

                relativePath = sb.ToString().TrimEnd(directorySeparatorCharArray);

                sb.Length = 0;

                foreach (string folder in baseFolders)
                {
                    sb.Append(folder);
                    sb.Append(Path.DirectorySeparatorChar);
                }

                basePath = sb.ToString().TrimEnd(directorySeparatorCharArray);
            }

            string relativeRoot = Path.GetPathRoot(relativePath);

            // Find out if the relative path is already an absolute path (starting with a drive letter). 

            if (!string.IsNullOrEmpty(relativeRoot))
            {
                if (relativeRoot[0] == Path.DirectorySeparatorChar)
                    return Path.GetPathRoot(basePath).TrimEnd(Path.DirectorySeparatorChar) + relativePath;
                else
                    return relativePath;
            }

            return Path.Combine(basePath.TrimEnd(directorySeparatorCharArray) + Path.DirectorySeparatorChar, relativePath);
        }

        /// <summary>
        /// Creates a relative path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="targetPath">The target path.</param>
        /// <returns>The relative path</returns>
        public static string MakeRelativePath(string basePath, string targetPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException("basePath");
            
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException("targetPath");
            
            bool isRooted = Path.IsPathRooted(basePath) && Path.IsPathRooted(targetPath);

            if (isRooted)
            {
                bool isDifferentRoot = string.Compare(Path.GetPathRoot(basePath),
                                                      Path.GetPathRoot(targetPath), true) != 0;

                if (isDifferentRoot)
                    return targetPath;
            }

            StringCollection relativePath = new StringCollection();

            string[] fromDirectories = basePath.Split(Path.DirectorySeparatorChar);
            string[] toDirectories = targetPath.Split(Path.DirectorySeparatorChar);
            
            int length = Math.Min(fromDirectories.Length, toDirectories.Length);
            int lastCommonRoot = -1;

            // find common root

            for (int x = 0; x < length; x++)
            {
                if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)
                    break;

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
                return targetPath;
            
            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
                if (fromDirectories[x].Length > 0)
                    relativePath.Add("..");

            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
                relativePath.Add(toDirectories[x]);

            // create relative path
            string[] relativeParts = new string[relativePath.Count];

            relativePath.CopyTo(relativeParts, 0);
            string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);

            return newPath;
        }

        /// <summary>
        /// Gets the assembly file attribute.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static AssemblyFileVersionAttribute GetAssemblyFileAttribute(Assembly assembly)
        {
            // make sure we were passed an assy
            if (null == assembly)
                return null;

            // try and get the requested assy attribute
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);

            if (null == attributes)
                return null;

            return (AssemblyFileVersionAttribute)attributes[0];
        }

#if DEBUG

        public static string DumpProperties(Properties props)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Property prop in props)
            {
                try
                {
                    sb.Append(string.Format("Name: \"{0}\" Value: \"{1}\"\r\n", prop.Name, prop.Value));
                }
                catch
                {
                    sb.Append(string.Format("Name: \"{0}\" Value: \"(UNKNOWN)\"\r\n", prop.Name));
                }

            }

            return sb.ToString();
        }

#endif
    }
}
