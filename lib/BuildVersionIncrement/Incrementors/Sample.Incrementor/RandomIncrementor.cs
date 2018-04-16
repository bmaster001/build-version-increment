using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BuildVersionIncrement.Incrementors;
using BuildVersionIncrement;

namespace Sample.Incrementor
{
    /// <summary>
    /// Example incrementor plugin that generates a random version number.
    /// </summary>
    public class RandomIncrementor : BaseIncrementor
    {
        /// <summary>
        /// Gets the name of this incrementor.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return "Random"; }
        }

        /// <summary>
        /// Gets the description of this incrementor.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get { return "Generates a random version number."; }
        }

        private Random _rnd = new Random();

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
            Logger.Write("Setting a random version number.", LogLevel.Debug);

            // Set the requested version component of the IncrementContext to a random number

            context.SetNewVersionComponentValue(versionComponent, _rnd.Next((int)short.MaxValue).ToString());
        }
    }
}
