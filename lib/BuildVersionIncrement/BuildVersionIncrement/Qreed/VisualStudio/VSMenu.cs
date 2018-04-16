using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Resources;
using System.Globalization;
using Microsoft.VisualStudio.CommandBars;
using EnvDTE;

namespace Qreed.VisualStudio
{
    public class VSMenu
    {
        private string _menuName;

        private IVSAddin _vsAddin;
        /// <summary>
        /// Gets the connect.
        /// </summary>
        /// <value>The connect.</value>
        public IVSAddin VSAddin
        {
            get { return _vsAddin; }
        }

        private string _toolsMenuName;
        /// <summary>
        /// Gets the name of the tools menu.
        /// </summary>
        /// <value>The name of the tools menu.</value>
        private string ToolsMenuName
        {
            get
            {
                if (_toolsMenuName == null)
                {
                    try
                    {
                        Assembly asm = Assembly.GetExecutingAssembly(); // Fix me!
                        ResourceManager resourceManager = new ResourceManager(VSAddin.CommandBarResourceName, asm);
                        int localID = VSAddin.ApplicationObject.LocaleID;
                        CultureInfo cultureInfo = new System.Globalization.CultureInfo(localID);
                        string resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
                        _toolsMenuName = resourceManager.GetString(resourceName);
                    }
                    catch
                    {
                        //We tried to find a localized version of the word Tools, but one was not found.
                        //  Default to the en-US word, which may work for the current culture.
                        _toolsMenuName = "Tools";
                    }
                }

                return _toolsMenuName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSMenu"/> class.
        /// </summary>
        /// <param name="connect">The connect.</param>
        public VSMenu(IVSAddin vsAddin, string menuName)
        {
            _vsAddin = vsAddin;
            _menuName = menuName;
        }

        private List<VSMenuCommand> _menuCommands = new List<VSMenuCommand>();
        /// <summary>
        /// Gets the menu commands.
        /// </summary>
        /// <value>The menu commands.</value>
        public List<VSMenuCommand> MenuCommands
        {
            get { return _menuCommands; }
        }

        private CommandBarPopup _popup;
        /// <summary>
        /// Gets the popup.
        /// </summary>
        /// <value>The popup.</value>
        public CommandBarPopup Popup
        {
            get
            {
                if (_popup == null)
                {
                    CommandBar menuBarCommandBar = ((CommandBars)VSAddin.ApplicationObject.CommandBars)["MenuBar"];
                    CommandBarControl toolsControl = menuBarCommandBar.Controls[ToolsMenuName];
                    CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                    // Create a submenu

                    _popup = (CommandBarPopup)toolsPopup.Controls.Add(MsoControlType.msoControlPopup,
                                                                      Type.Missing, Type.Missing, 1, true);
                    _popup.Caption = _menuName;
                }

                return _popup;
            }
        }

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public VSMenuCommand AddCommand(string commandName, string displayName, string description)
        {
            VSMenuCommand vsCmd = new VSMenuCommand(this, commandName, displayName, description);
            MenuCommands.Add(vsCmd);

            return vsCmd;
        }

        /// <summary>
        /// Execs the specified command name.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="executeOption">The execute option.</param>
        /// <param name="varIn">The var in.</param>
        /// <param name="varOut">The var out.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                VSMenuCommand cmd = this[commandName];

                if (cmd != null)
                {
                    cmd.OnExecute();
                    handled = true;
                    return;
                }
            }
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            // TODO: route to the child menu item

            VSMenuCommand command = this[commandName];

            if (command != null)
            {
                command.OnQueryStatus(neededText, ref status, ref commandText);
            }
        }

        /// <summary>
        /// Gets the <see cref="NUnitit.VSMenuCommand"/> with the specified command name.
        /// </summary>
        /// <value></value>
        public VSMenuCommand this[string commandName]
        {
            get
            {
                foreach (VSMenuCommand cmd in _menuCommands)
                {
                    if (cmd.CommandName == commandName)
                        return cmd;
                }

                return null;
            }
        }
    }
}
