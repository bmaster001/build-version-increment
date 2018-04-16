using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;

namespace WindowsFormsApplication1
{
    static class Program
    {
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AssemblyFileVersionAttribute fileVersionAttr = GetAssemblyFileAttribute(Assembly.GetExecutingAssembly());

            MessageBox.Show(fileVersionAttr.Version);


            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/ 
        }
    }
}
