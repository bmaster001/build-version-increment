// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: ReflectionHelper.cs
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
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public static class ReflectionHelper
	{
		public static TAttribute GetAssemblyAttribute<TAttribute>(Assembly assembly)
		{
			return (TAttribute)GetAssemblyAttribute(typeof(TAttribute), assembly);
		}

		public static object GetAssemblyAttribute(Type attributeType, Assembly assembly)
		{
			if (assembly == null)
			{
				throw (new ArgumentNullException(nameof(assembly)));
			}

			var attributes = assembly.GetCustomAttributes(attributeType, true);

			return attributes.Length == 0 ? null : attributes[0];
		}

		public static List<Type> GetTypesThatDeriveFromType(Assembly asm,
		                                                    Type baseType,
		                                                    bool includeSelf,
		                                                    bool includeAbstract)
		{
			if (asm == null)
			{
				throw (new ArgumentNullException(nameof(asm), "No assembly given"));
			}

			var types = asm.GetTypes();

			if (types == null || types.Length == 0)
			{
				throw (new Exception($"Failed getting types from assembly: {asm.FullName}"));
			}

			return (from t in types
			        where t != baseType || includeSelf
			        where baseType.IsAssignableFrom(t)
			        where includeAbstract || !t.IsAbstract
			        select t).ToList();
		}
	}
}