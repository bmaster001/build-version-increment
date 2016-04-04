using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE80;
using EnvDTE;
using Extensibility;

namespace Qreed.VisualStudio
{
    public interface IVSAddin
    {
        /// <summary>
        /// Gets the application object.
        /// </summary>
        /// <value>The application object.</value>
        DTE2 ApplicationObject{get;}

        /// <summary>
        /// Gets the add in instance.
        /// </summary>
        /// <value>The add in instance.</value>
        AddIn AddInInstance{get;}

        /// <summary>
        /// Gets the name of the command bar resource.
        /// </summary>
        /// <value>The name of the command bar resource.</value>
        string CommandBarResourceName { get; }

        /// <summary>
        /// Gets the connect mode.
        /// </summary>
        /// <value>The connect mode.</value>
        ext_ConnectMode ConnectMode {get;}
    }
}
