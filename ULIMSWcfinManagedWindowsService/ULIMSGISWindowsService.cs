using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ULIMSGISPython = ulimsgispython.ulims.com.na; //python class

//Utility Clacc library
using Utility.ulims.com.na;

using wcf.ulims.com.na;

namespace wsh.ulims.com.na
{
    public class ULIMSGISWindowsService : ServiceBase
    {

        #region Member Variables

        //Create a pointer to a timer as a member variable named MTimer
        private Timer mTimer = null;

        //Create pointer for ULIMS GIS service object
        private IULIMSGISService mULIMSGISService = null;

        //Create pointer for python library object
        private ULIMSGISPython.IPythonLibrary iPythonLibrary = null;

        //An instance of this class
        static ULIMSGISWindowsService uLIMSGISWindowsService = null;
        private System.ComponentModel.IContainer components;

        //Add a local variable to reference the ServiceHost Instance
        public ServiceHost serviceHost = null;

        #endregion

        #region Getter and Setter Methods

        /// <summary>
        /// Property: Timer
        /// Getter and Setter Methods
        /// </summary>
        public Timer MTimer
        {
            get
            {
                try
                {
                    return mTimer;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ULIMSGISWindowsService.Timer MTimer get : ", ex); 
                }
            }
            set
            {
                try
                {
                    mTimer = value;
                }
                catch (Exception ex)
                {
                    
                     //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ULIMSGISWindowsService.Timer MTimer set : ", ex); 
                }
            }
        }
        
        /// <summary>
        /// Property : MIULIMSGISService of type ULIMSGISService
        /// Get and Setter Methods
        /// </summary>
        public IULIMSGISService MIULIMSGISService
        {
            get
            {
                try
                {
                    return mULIMSGISService;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ULIMSGISWindowsService.IULIMSGISService MIULIMSGISService get : ", ex); 
                }
            }
            set
            {
                try
                {
                    mULIMSGISService = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ULIMSGISWindowsService.IULIMSGISService MIULIMSGISService set : ", ex); 
                }
            }
        }

        /// <summary>
        /// Property : IPythonLibrary of type ULIMSGISPython.IPythonLibrary 
        /// Get and Setter Methods
        /// </summary>
        public ULIMSGISPython.IPythonLibrary IPythonLibrary
        {
            get
            {
                try
                {
                    return iPythonLibrary;
                }
                catch (Exception ex)
                {
                    
                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ULIMSGISWindowsService.ULIMSGISPython.IPythonLibrary IPythonLibrary get : ", ex);
                }
            }
            set
            {
                try
                {
                    iPythonLibrary = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ULIMSGISWindowsService.ULIMSGISPython.IPythonLibrary IPythonLibrary set : ", ex);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor : ULIMSGISWindowsService method
        /// Defines the name of the Windows Service
        /// Calls method to instantiate needed classes into required objects
        /// </summary>
        public ULIMSGISWindowsService()
        {
            try
            {
                //Name the Windows Service Name. This is what appears in services.msc
                ServiceName = "ULIMS WCF GIS Synch Service";

                //Call method to initialze python objects and other related objects
                initializeGISObjects();
            }
            catch (Exception ex)
            {
                
                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ULIMSGISWindowsService.ULIMSGISWindowsService() : ", ex);
            }
        }

        #endregion 

        #region Windows Service Methods

        /// <summary>
        /// Main method: entry point of this program
        /// //Define the main method that calls the service base
        /// </summary>
        public static void Main()
        {
            try
            {
                uLIMSGISWindowsService = new ULIMSGISWindowsService();
                ServiceBase.Run(uLIMSGISWindowsService);
            }
            catch (Exception ex)
            {
                
                Logger.WriteErrorLog("ULIMSGISWindowsService.Main()" + Environment.NewLine + ex.ToString());//Write error to log file
            }
        }

        /// <summary>
        ///  Start the Windows service
        ///  Override the OnStart(String[]) method by creating and opening a new
        ///  ServiceHost instance as shown in the following code.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                if (serviceHost != null)
                {
                    serviceHost.Close();
                }

                // Create a ServiceHost for the ULIMSGISService type and 
                // provide the base address.
                serviceHost = new ServiceHost(typeof(ULIMSGISService));

                // Open the ServiceHostBase to create listeners and start 
                // listening for messages.
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                //Write any messages to the log file. Failure to which error is written onto the event  viewerlogs
                Logger.WriteErrorLog("ULIMSGISWindowsService.OnStart(string[] args)" + Environment.NewLine + ex.ToString());//Write error to log file
            }

            try
            {
                //Apply Timer Settings for the windows service
                timerSettings();
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.ToString());//Write error to log file
            }
        }

        /// <summary>
        /// Override the OnStop method closing the ServiceHost as shown in the following code.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                if (serviceHost != null)
                {
                    serviceHost.Close();
                    serviceHost = null;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog("ULIMSGISWindowsService.OnStop()" + Environment.NewLine + ex.ToString());//Write error to log file
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Method : initializeGISObjects
        /// Creates python object tasked to perform execution of python code from .Net
        /// instantiates the WCF Service library code
        /// </summary>
        private void initializeGISObjects()
        {
            try
            {
                /*
                 * Get instance of IPythonLibrary class
                 * Contains propoerties and methods to help with execution
                 */
                IPythonLibrary = new ULIMSGISPython.PythonLibrary();

                /*
                 * Get instance of GISService class
                 * Contains propoerties and methods to help with execution
                 */
                MIULIMSGISService = new ULIMSGISService();

                MIULIMSGISService.MIPythonLibrary = IPythonLibrary;

                //Code ensures execute path and execute directory are returned successfully
                Logger.ExecutablePath = ""; Logger.ExecutableRootDirectory = "";

                //Get path to the binary file that serializes 
                string filePathForSerializedObject = Logger.ExecutableRootDirectory + @"\" + "SavedULIMSObjects.bin";

                //Call save object
                MIULIMSGISService.saveObject(filePathForSerializedObject);

                //Write to logger
                Logger.WriteErrorLog("Executable Path :" + Environment.NewLine + Logger.ExecutablePath);
                Logger.WriteErrorLog("Execute Root Directory" + Environment.NewLine + Logger.ExecutableRootDirectory);
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ULIMSGISWindowsService.initializeGISObjects() ", ex);
            }

        }

        /// <summary>
        /// Method : timerSettings
        /// Reads time interval from the app.config files
        /// Wires the timer to its event handler and enables the timer
        /// Save binary file and tell it that GISsynch process has not started
        /// Write to log file telling it GIS GIS synch Service has started
        /// </summary>
        private void timerSettings()
        {
            try
            {
                //Initialize anew instance of the timer class
                MTimer = new Timer();

                //Get timer interval from app.config
                double timerInterval;
                timerInterval = ((ConfigLibrary.IConfigReader)(new ConfigLibrary.ConfigReader())).MTimerInterval;
                MTimer.Interval = timerInterval;
                /*
                 * Wire timer elapsed event to the timer tick handler
                 * Occurs when the interval elapses
                 */
                MTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.mTimer_Tick);

                //Enable the timer to whether to raise the elapsed event
                MTimer.Enabled = true;

                //save object file with false parameter. 
                //This stops the Windows service from running GIS synch process until instructed so by a client
                MIULIMSGISService.saveObject(false);

                //Write to log file indicating GIS service has started successfully.
                Logger.WriteErrorLog("ULIMS GIS Synchonization Service started");

            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ULIMSGISWindowsService.timerSettings() ", ex);
            }
        }

        /// <summary>
        /// Timer event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                MIULIMSGISService.readObject();

                //Code for firing by WCF client
                if (MIULIMSGISService.mULIMSSerializer.HasGISSyncProcessstarted == true)
                { }
                    //Check that elapsed event raises this event handler and executes code in it 
                    //  if and only if the previous execution has completed
                    if (MIULIMSGISService.mEexecuting)
                        return;

                    //Set MPythonLibrary is executing as true
                    MIULIMSGISService.mEexecuting = true;

                    try
                    {
                        //Indicate and write to log file shouting that execution of python code is firing from all cylinders
                        Logger.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                        job has been done fired");

                        //Call method that executes python code
                        MIULIMSGISService.executePythonCode();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteErrorLog("ULIMSGISWindowsService.executePythonCode()" + Environment.NewLine + ex.ToString());//Write error to log file
                    }
                    finally
                    {
                        //Tell the timer GIS synch process has stopped executing
                        MIULIMSGISService.mEexecuting = false;
                        Logger.WriteErrorLog(@"Tell the timer GIS synch process has stopped executing)");

                        //Tell process to stop GIS Synch Process
                        //save object file with false parameter. 
                        //This stops the Windows service from running GIS synch process until instructed so by a client
                        MIULIMSGISService.saveObject(false);

                        Logger.WriteErrorLog(@"Tell process to stop GIS Synch Process
                        save object file with false parameter. 
                        This stops the Windows service from running GIS synch process until instructed so by a client)");

                        //GIS synch process has completed successfully.//Tell the GIS to Sharepoint client to start shipping erfs to SharePoint
                        MIULIMSGISService.MGISSyncProcess = true;

                        Logger.WriteErrorLog(@"GIS synch process has completed successfully.
                        Tell the GIS to Sharepoint client to start shipping erfs to SharePoint)");

                        Logger.WriteErrorLog(@"Timer ticked and executePythonCode()  method or 
                        job has successfully completed(But with a pinch of salt-There could be errors)");

                    }
           
            }
            catch (Exception ex)
            {

                Logger.WriteErrorLog("ULIMSGISWindowsService.mTimer_Tick(object sender, ElapsedEventArgs e)" + Environment.NewLine + ex.ToString());//Write error to log file
            }

        }
   
        #endregion
    }
}
