using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace Qreed.VisualStudio
{
    public class VSMenuQueryStatusEventArgs : EventArgs
    {
        private vsCommandStatusTextWanted _neededText;
        /// <summary>
        /// Gets or sets the text that is needed for the command.
        /// </summary>
        /// <value>The needed text.</value>
		public vsCommandStatusTextWanted NeededText 
		{ 
			get {return this._neededText;} 
			set {this._neededText = value;} 
		}

        private vsCommandStatus  _status;
        /// <summary>
        /// Gets or sets the state of the command in the user interface.
        /// </summary>
        /// <value>The status.</value>
		public vsCommandStatus  Status 
		{ 
			get {return this._status;} 
			set {this._status = value;} 
		}

        private object _commandText;
        /// <summary>
        /// Gets or sets the text requested by the <see cref="NeededText "/> parameter.
        /// </summary>
        /// <value>The command text.</value>
        public object CommandText
        {
            get { return this._commandText; }
            set { this._commandText = value; }
        }
    }
}
