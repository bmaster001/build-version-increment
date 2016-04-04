using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BuildVersionIncrement.UnitTest
{
    [TestFixture]
    public class StandardVersionIncrementerTest
    {
        [Test]
        public void MakeAbsolutePathTest()
        {
            MakeAbsolutePathTest(@"..\SGA\My Project\AssemblyInfo.vb", 
                                 @"C:\Dev\Production\Projects\SGA\Common", 
                                 @"C:\Dev\Production\Projects\SGA\SGA\My Project\AssemblyInfo.vb");

            MakeAbsolutePathTest(@"..\..\..\AssemblyInfo.vb",
                                 @"C:\Dev\Production\Projects\SGA\Common",
                                 @"C:\Dev\Production\AssemblyInfo.vb");

            MakeAbsolutePathTest(@"..\Dummy\..\AssemblyInfo.vb",
                                 @"C:\Dev\Production\Projects\SGA\Common",
                                 @"C:\Dev\Production\Projects\SGA\AssemblyInfo.vb");

            MakeAbsolutePathTest(@"\AssemblyInfo.vb",
                                 @"C:\Dev\Production\Projects\SGA\Common",
                                 @"C:\AssemblyInfo.vb");

            MakeAbsolutePathTest(@"C:\Dummy\AssemblyInfo.vb",
                                 @"C:\Dev\Production\Projects\SGA\Common",
                                 @"C:\Dummy\AssemblyInfo.vb");

            MakeAbsolutePathTest(@"C:\Dummy\..\AssemblyInfo.vb",
                                 @"C:\Dev\Production\Projects\SGA\Common",
                                 @"C:\AssemblyInfo.vb");

            MakeAbsolutePathTest(@"\..\..\..\AssemblyInfo.vb",
                                 @"C:\",
                                 @"C:\AssemblyInfo.vb");

        }

        private static void MakeAbsolutePathTest(string relativePath, string basePath, string expected)
        {
            string absolutePath = Common.MakeAbsolutePath(basePath, relativePath);

            Assert.IsTrue(string.Compare(absolutePath, expected, true) == 0);
        }
    }
}
