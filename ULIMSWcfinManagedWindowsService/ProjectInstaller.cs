using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace wsh.ulims.com.na
{
    /// <summary>
    /// Class Name : ProjectInstaller
    /// Class inherits from installer and marked with RunInstallerAttribute set to true
    /// This allows the windows servce to be installed by instalutil.exe tool
    /// </summary>
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {

        #region Member Variables

        private ServiceProcessInstaller process; //process
        private ServiceInstaller service; //installer

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor Method: ProjectInstaller()
        /// </summary>
        public ProjectInstaller()
        {
            try
            {
                process = new ServiceProcessInstaller();
                process.Account = ServiceAccount.LocalSystem; //Run services as local system account
                service = new ServiceInstaller(); //Service installer
                service.ServiceName = "ULIMS WCF GIS Synch Service"; //Service Name
                Installers.Add(process); 
                Installers.Add(service);
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ProjectInstaller.ProjectInstaller() : ", ex);
            }
        }

        #endregion

    }
}
