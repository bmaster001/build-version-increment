using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using EnvDTE80;
using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
using System.Diagnostics;
using stdole;
using System.Drawing;

namespace Qreed.VisualStudio
{
    public class VSMenuCommand
    {
        private class ImageConverter : AxHost
        {
            // http://www.kebabshopblues.co.uk/2007/01/04/visual-studio-2005-tools-for-office-commandbarbutton-faceid-property/

            public ImageConverter()
                : base("59EE46BA-677D-4d20-BF10-8D8067CB8B33")
            { }

            public static stdole.StdPicture ImageToIPicture(Image image)
            {
                return (stdole.StdPicture)AxHost.GetIPictureDispFromPicture(image);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSMenuCommand"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="description">The description.</param>
        public VSMenuCommand(VSMenu menu, string name, string displayName, string description)
        {
            _menu = menu;
            _name = name;
            _displayName = displayName;
            _description = description;

            object[] contextGUIDS = new object[] { };

            Commands2 commands = (Commands2)menu.VSAddin.ApplicationObject.Commands;

            try
            {
                _command = commands.AddNamedCommand2(menu.VSAddin.AddInInstance,
                                                     name, displayName, description,
                                                     true, 0, ref contextGUIDS,
                                                     (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                                                     (int)vsCommandStyle.vsCommandStylePictAndText,
                                                     vsCommandControlType.vsCommandControlTypeButton);
            }
            catch
            {
                // If we get here then the command name probably already exists.
                _command = commands.Item(menu.VSAddin.AddInInstance.ProgID + "." + name, 0);
            }

            _button = (CommandBarButton)_command.AddControl(_menu.Popup.CommandBar, 1);
        }

        private event EventHandler _execute;
        /// <summary>
        /// Occurs when the command is executed.
        /// </summary>
        public event EventHandler Execute
        {
            add { _execute += value; }
            remove { _execute -= value; }
        }

        private event EventHandler<VSMenuQueryStatusEventArgs> _queryStatus;
        /// <summary>
        /// Occurs when the IDE queries the item status.
        /// </summary>
        public event EventHandler<VSMenuQueryStatusEventArgs> QueryStatus
        {
            add { _queryStatus += value; }
            remove { _queryStatus -= value; }
        }

        private VSMenu _menu;
        /// <summary>
        /// Gets the menu.
        /// </summary>
        /// <value>The menu.</value>
        public VSMenu Menu
        {
            get { return _menu; }
        }

        private string _name;
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _name; }
        }

        private string _displayName;
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName
        {
            get { return _displayName; }
        }

        private string _description;
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return _description; }
        }

        private Command _command;
        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>The command.</value>
        public Command Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>The name of the command.</value>
        public string CommandName
        {
            get { return _command.Name; }
        }

        private CommandBarButton _button;
        /// <summary>
        /// Gets the button.
        /// </summary>
        /// <value>The button.</value>
        public CommandBarButton Button
        {
            get { return _button; }
        }

        /// <summary>
        /// Called when the command is executed.
        /// </summary>
        public virtual void OnExecute()
        {
            Trace.WriteLine("OnExecute: " + CommandName);

            if (_execute != null)
                _execute(this, null);
        }

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <param name="image">The image.</param>
        public void SetImage(Image image)
        {
            StdPicture picture = ImageConverter.ImageToIPicture(image);

            Button.Picture = picture;
        }

        /// <summary>
        /// Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated
        /// </summary>
        /// <param name="neededText">Text that is needed for the command.</param>
        /// <param name="status">The state of the command in the user interface.</param>
        /// <param name="commandText">Text requested by the neededText parameter.</param>
        internal void OnQueryStatus(vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (_queryStatus != null)
            {
                VSMenuQueryStatusEventArgs e = new VSMenuQueryStatusEventArgs();
                
                e.NeededText = neededText;
                e.Status = status;
                e.CommandText = commandText;

                _queryStatus(this, e);

                status = e.Status;
            }
            else if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
                status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
        }
    }

}
