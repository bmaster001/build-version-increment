using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Qreed.Reflection;

namespace BuildVersionIncrement.Incrementors
{
    internal class IncrementorCollection
    {
        private Dictionary<string, BaseIncrementor> _incrementors = new Dictionary<string, BaseIncrementor>();

        /// <summary>
        /// Gets the <see cref="BuildVersionIncrement.Incrementors.BaseIncrementor"/> with the specified name.
        /// </summary>
        /// <value>The name of the incrementor.</value>
        public BaseIncrementor this[string name]
        {
            get
            {
                if (_incrementors.ContainsKey(name))
                    return _incrementors[name];

                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="BuildVersionIncrement.Incrementors.BaseIncrementor"/> representing the specified old style enum.
        /// </summary>
        /// <value>The old style enum.</value>
        public BaseIncrementor this[OLD_IncrementStyle oldStyle]
        {
            get
            {
                return this[oldStyle.ToString()];
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return _incrementors.Keys.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementorCollection"/> class.
        /// </summary>
        public IncrementorCollection()
        {
            // Add the static null instance to the list. This ensures that property grid references the "correct" null. 
            _incrementors.Add(BuiltInBaseIncrementor.NoneIncrementor.Instance.Name,
                              BuiltInBaseIncrementor.NoneIncrementor.Instance);
        }

        /// <summary>
        /// Locates BaseIncrementors in the given assembly and add them to the collection.
        /// </summary>
        /// <param name="asm">The assembly.</param>
        public void AddFrom(Assembly asm)
        {
            Logger.Write("Locating incrementors in assembly \"" + asm.FullName + "\" ...", LogLevel.Debug);
            List<Type> types = ReflectionHelper.GetTypesThatDeriveFromType(asm, typeof(BaseIncrementor), false, false);

            Logger.Write("Located " + types.Count + " incrementors.", LogLevel.Debug);
            
            foreach (Type t in types)
            {
                if (t == typeof(BuiltInBaseIncrementor.NoneIncrementor)) // Don't add the null incrementor; this is done in the constructor.
                    continue;

                Logger.Write("Creating instance of incrementor type \"" + t.FullName + "\".", LogLevel.Info);
                BaseIncrementor incrementor = (BaseIncrementor)Activator.CreateInstance(t);
                
                _incrementors.Add(incrementor.Name, incrementor);
            }
        }

        /// <summary>
        /// Gets the incrementor names.
        /// </summary>
        /// <returns>An array containing all the names of the incrementors.</returns>
        public string[] GetIncrementorNames()
        {
            string[] ret = new string[Count];

            _incrementors.Keys.CopyTo(ret, 0);

            return ret;
        }

        /// <summary>
        /// Gets the incrementors.
        /// </summary>
        /// <returns>An array containing the incrementors</returns>
        public BaseIncrementor[] GetIncrementors()
        {
            BaseIncrementor[] ret = new BaseIncrementor[Count];

            _incrementors.Values.CopyTo(ret, 0);

            return ret;
        }
    }
}
