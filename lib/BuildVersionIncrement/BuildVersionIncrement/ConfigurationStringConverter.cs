using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using EnvDTE;

namespace BuildVersionIncrement
{
    /// <summary>
    /// TypeConverter used for creating a list of the available build configurations for a certain <see cref="SolutionItem"/>.
    /// </summary>
    internal class ConfigurationStringConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <returns>
        /// true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> should be called to find a common set of values the object supports; otherwise, false.
        /// </returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> is an exclusive list of possible values, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <returns>
        /// true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"/> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> is an exhaustive list of possible values; false if other values are possible.
        /// </returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"/> that holds a standard set of valid values, or null if the data type does not support a standard set of values.
        /// </returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {

            SolutionItem solutionItem = ((SolutionItemIncrementSettings)context.Instance).SolutionItem;

            object[] items = CreateList(solutionItem);

            if (items != null)
                return new StandardValuesCollection(items);

            return null;
        }

        /// <summary>
        /// Creates the list.
        /// </summary>
        /// <param name="solutionItem">The solution item.</param>
        /// <returns>An array of configuration names.</returns>
        private object[] CreateList(SolutionItem solutionItem)
        {
            ArrayList ret = new ArrayList();

            ret.Add("Any");

            if (solutionItem.ItemType == SolutionItemType.Solution)
            {
                SolutionConfigurations configs = solutionItem.Solution.SolutionBuild.SolutionConfigurations;

                string lastConfigName = "";

                foreach (SolutionConfiguration config in configs)
                {
                    if (lastConfigName != config.Name)
                    {
                        ret.Add(config.Name);
                        lastConfigName = config.Name;
                    }
                }
            }
            else
            {
                object[] names = (object[])solutionItem.Project.ConfigurationManager.ConfigurationRowNames;

                foreach (object o in names)
                    ret.Add(o);
            }

            return ret.ToArray();
        }
    }
}
