using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;

namespace Qreed.Reflection
{
    /// <summary>
    /// Provides a static utility object of methods and properties to interact
    /// with enumerated types.
    /// </summary>
    /// <remarks>Slighty modified version based on a codeproject article by Scott Dorman.</remarks>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the <see cref="DescriptionAttribute" /> of an <see cref="Enum" />
        /// type value.
        /// </summary>
        /// <param name="value">The <see cref="Enum" /> type value.</param>
        /// <returns>A string containing the text of the
        /// <see cref="DescriptionAttribute"/>.</returns>
        public static string GetDescription(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            
            string description = value.ToString();

            FieldInfo fieldInfo = value.GetType().GetField(description);
            
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                description = attributes[0].Description;
            
            return description;
        }

        /// <summary>
        /// Converts the <see cref="Enum" /> type to an <see cref="IList" /> 
        /// compatible object.
        /// </summary>
        /// <param name="type">The <see cref="Enum"/> type.</param>
        /// <returns>An <see cref="IList"/> containing the enumerated
        /// type value and description.</returns>
        public static IList ToList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ArrayList list = new ArrayList();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                list.Add(new KeyValuePair<Enum, string>(value, GetDescription(value)));
            }

            return list;
        }

        /// <summary>
        /// Binds a <see cref="ListControl"/> to an enum value type.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="control">The control.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="propertyName">Name of the property that contains the value to bind to.</param>
        public static void DataBind(Type enumType, ListControl control, object targetObject, string propertyName)
        {
            if (enumType == null)
                throw (new ArgumentNullException("enumType"));

            if (control == null)
                throw (new ArgumentNullException("control"));

            if (targetObject == null)
                throw (new ArgumentNullException("targetObject"));

            if (string.IsNullOrEmpty(propertyName))
                throw (new ArgumentNullException(propertyName));

            IList dataList = ToList(enumType);

            control.DataSource = dataList;
            control.DisplayMember = "Value";
            control.ValueMember = "Key";

            //control.DataBindings.Add("SelectedValue", targetObject, propertyName);

            PropertyInfo propInfo = targetObject.GetType().GetProperty(propertyName);

            if (propInfo == null)
                throw (new ApplicationException("The targetObject doesn't contains a property named " + propertyName));

            control.SelectedValue = propInfo.GetValue(targetObject, null);

            control.SelectedValueChanged += delegate(object sender, EventArgs e) 
            {
                propInfo.SetValue(targetObject, control.SelectedValue, null);
            };
        }
    }
}
