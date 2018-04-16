using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace BuildVersionIncrement
{
    /// <summary>
    /// Utility class for global variables stored in Visual Studio solutions and project items.
    /// </summary>
    public static class GlobalVariables
    {
        /// <summary>
        /// Get a global variable from a array of global variables
        /// </summary>
        /// <param name="globals">Array of global variables</param>
        /// <param name="varName">Variable name</param>
        /// <param name="defaultVal">Default value</param>
        /// <returns>Variable's value if found, defaultVal otherwise</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "globals")]
        public static string GetGlobalVariable(Globals globals, string varName, string defaultValue)
        {
            if (globals == null)
                return defaultValue;

            object[] names = (object[])globals.VariableNames;

            if (globals.get_VariableExists(varName))
            {
                foreach (object name in names)
                {
                    if (name.ToString() == varName)
                    {
                        return (string)globals[varName];
                    }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets a global variable to an array of global variables.
        /// </summary>
        /// <param name="globals">The globals.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <remarks>
        /// This overload will make sure that no changes are made to the project file when given value is the same as
        /// the current global value.
        /// </remarks>
        public static void SetGlobalVariable(Globals globals, string variableName, string value, string defaultValue)
        {
            if (globals == null)
                return;

            // This function makes sure that global values are only set when they differ from the default
            string globalValue = GetGlobalVariable(globals, variableName, null);

            if (globalValue != null)
            {
                // Variable already exists ... Compare the current value with the given first so we
                // don't write unnecessary to the project file. (prevents a checkout)

                if (string.Compare(globalValue, value, true) != 0) // ... but doesn't matches our current value 
                    SetGlobalVariable(globals, variableName, value);
            }
            else if (string.Compare(value, defaultValue, true) != 0)
            {
                // Variable doesn't exists yet and the current value is differs from the default
                SetGlobalVariable(globals, variableName, value);
            }
        }

        /// <summary>
        /// Set a global variable to an array of global variables
        /// </summary>
        /// <param name="globals">Array of global variables</param>
        /// <param name="varName">Variable name</param>
        /// <param name="value">Value to save</param>
        public static void SetGlobalVariable(Globals globals, string varName, string value)
        {
            globals[varName] = value;
            // Note from one_eddie: I tried fix #6684 and I found this is
            // the place where the issue exists. In my opinion we currently mark
            // the variable as modified (need to be persistent) every time we set them
            // but some values may be already set and we wan't update them and this causes 
            // VS to save them in different order.
            // Fix: To fix this we should always update all varaibles (calls in same order) even they 
            // ware not modified. 
            // I don't have much time and do not feel strong enough to make this change so I 
            // leave this description. I hope this description is clear enougt for future fix implementor
            globals.set_VariablePersists(varName, true);
        }
    }
}
