using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildVersionIncrement.Incrementors;
using BuildVersionIncrement;

namespace Sample.Incrementor
{
    /// <summary>
    /// Example incrementor plugin that will reset the version number.
    /// </summary>
    /// <remarks>
    /// Instead of modifying one single version component (like major, minor, build or revision), this incrementor 
    /// will set a complete version number and notifies the addin to stop calling any other incrementors for this version increment.
    /// </remarks>
    public class ResetIncrementor : BaseIncrementor
    {
        /// <summary>
        /// Gets the name of this incrementor.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return "Reset"; }
        }

        /// <summary>
        /// Gets the description of this incrementor.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Resets a version number to 1.0.0.0"; }
        }

        /// <summary>
        /// Executes the increment.
        /// </summary>
        /// <param name="context">The context of the increment.</param>
        /// <param name="versionComponent">The version component that needs to be incremented.</param>
        /// <remarks>
        /// Use the method <see cref="IncrementContext.SetNewVersionComponentValue"/> to set the new version component value.
        /// Set the  <see cref="IncrementContext.Continue"/> property to <c>false</c> to skip updating the other component values.
        /// </remarks>
		public override void Increment(IncrementContext context, VersionComponent versionComponent)
        {
        	Logger.Write("Resetting version number to 1.0.0.0", LogLevel.Debug);

        	// Set all the version components

        	context.SetNewVersionComponentValue(versionComponent,
        	                                    versionComponent == VersionComponent.Major ? "1" : "0");

        	// Tell the addin to stop incrementing the other version components
        	// context.Continue = false;
        }
    }
}
