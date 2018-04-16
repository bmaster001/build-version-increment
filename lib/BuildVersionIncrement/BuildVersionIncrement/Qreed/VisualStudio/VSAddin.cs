using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using Extensibility;
using EnvDTE80;
using System.Diagnostics;

namespace Qreed.VisualStudio
{
    public abstract class VSAddin : IVSAddin, IDTExtensibility2, IDTCommandTarget
    {
        /// <summary>
        /// Override this to create your menu items here.
        /// </summary>
        /// <returns>The root menu item.</returns>
        protected virtual VSMenu SetupMenuItems()
        {
            return null;
        }

        private VSMenu _rootMenu;
        /// <summary>
        /// Gets the root menu.
        /// </summary>
        /// <value>The root menu.</value>
        protected VSMenu RootMenu
        {
            get { return _rootMenu; }
        }

        #region IVSAddin Members

        private DTE2 _applicationObject;
        /// <summary>
        /// Gets the application object.
        /// </summary>
        /// <value>The application object.</value>
        public virtual DTE2 ApplicationObject
        {
            get { return _applicationObject; }
        }

        private AddIn _addInInstance;
        /// <summary>
        /// Gets the addin instance.
        /// </summary>
        /// <value>The addin instance.</value>
        public virtual AddIn AddInInstance
        {
            get { return _addInInstance; }
        }

        /// <summary>
        /// Gets the name of the command bar resource.
        /// </summary>
        /// <value>The name of the command bar resource.</value>
        public virtual string CommandBarResourceName
        {
            get { throw new NotImplementedException(); }
        }

        private ext_ConnectMode _connectMode;
        /// <summary>
        /// Gets the connect mode.
        /// </summary>
        /// <value>The connect mode.</value>
        public ext_ConnectMode ConnectMode
        {
            get { return _connectMode; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is command line build.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is command line build; otherwise, <c>false</c>.
        /// </value>
        public bool IsCommandLineBuild
        {
            get { return _connectMode == ext_ConnectMode.ext_cm_CommandLine; }
        }
        
        #endregion

        #region IDTExtensibility2 Members

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public virtual void OnAddInsUpdate(ref Array custom){}

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public virtual void OnBeginShutdown(ref Array custom){}

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public virtual void OnConnection(object Application, ext_ConnectMode connectMode, object AddInInst, ref Array custom)
        {
            _applicationObject = (DTE2)Application;
            _addInInstance = (AddIn)AddInInst;
            _connectMode = connectMode;

            if (ConnectMode == ext_ConnectMode.ext_cm_Startup)
            {
                _rootMenu = SetupMenuItems();
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public virtual void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom){}

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public virtual void OnStartupComplete(ref Array custom){}

        #endregion

        #region IDTCommandTarget Members

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='cmdName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        public virtual void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            Trace.WriteLine("Exec " + commandName + " " + executeOption);

            if(RootMenu != null)
                RootMenu.Exec(commandName, executeOption, ref varIn, ref varOut, ref handled);
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public virtual void QueryStatus(string CmdName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object CommandText)
        {
            Trace.WriteLine("QueryStatus " + CmdName + " " + neededText);

            if (RootMenu != null)
                RootMenu.QueryStatus(CmdName, neededText, ref status, ref CommandText);
        }

        #endregion
    }
}
