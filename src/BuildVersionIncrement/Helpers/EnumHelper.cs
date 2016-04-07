// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: EnumHelper.cs
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

namespace BuildVersionIncrement.Helpers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Forms;

	public static class EnumHelper
	{
		//public static void DataBind(Type enumType,
		//                            ListControl control,
		//                            object targetObject,
		//                            string propertyName)
		//{
		//	if (enumType == null)
		//	{
		//		throw (new ArgumentNullException("enumType"));
		//	}

		//	if (control == null)
		//	{
		//		throw (new ArgumentNullException("control"));
		//	}

		//	if (targetObject == null)
		//	{
		//		throw (new ArgumentNullException("targetObject"));
		//	}

		//	if (string.IsNullOrEmpty(propertyName))
		//	{
		//		throw (new ArgumentNullException(propertyName));
		//	}

		//	var dataList = ToList(enumType);

		//	control.DataSource = dataList;
		//	control.DisplayMember = "Value";
		//	control.ValueMember = "Key";

			
		//	var propInfo = targetObject.GetType().GetProperty(propertyName);

		//	if (propInfo == null)
		//	{
		//		throw (new ApplicationException("The targetObject doesn't contains a property named "
		//		                                + propertyName));
		//	}

		//	control.SelectedValue = propInfo.GetValue(targetObject, null);

		//	control.SelectedValueChanged +=
		//		delegate { propInfo.SetValue(targetObject, control.SelectedValue, null); };
		//}
		
		public static string GetDescription(Enum value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var description = value.ToString();

			var fieldInfo = value.GetType().GetField(description);

			var attributes =
				(DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes.Length > 0)
			{
				description = attributes[0].Description;
			}

			return description;
		}

		public static IDictionary<Enum, string> ToList(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var enumValues = Enum.GetValues(type);

			return enumValues.Cast<Enum>().ToDictionary(value => value, GetDescription);
		}
	}
}