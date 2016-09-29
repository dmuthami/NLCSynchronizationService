using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULIMSWcfClient.ULIMSGISServiceRef;
using ULIMSWcfClient.ExecLogic;

using Utility.ulims.com.na;

namespace ULIMSWcfClient
{
    /// <summary>
    /// WCF client
    /// WCF Service is hosted on Windows Service
    /// POTUS TRUMP
    /// </summary>
    class Program
    {
        #region Entry Method
        /// <summary>
        /// Method; Main
        /// WCF Client entry point
        /// Fires GISSynchProcess() method and waits until success
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                if (ULIMSWcfClient.SingleApplication.Run() == false)
                {
                	return;
                }

                try
                {
                    //Create instance of this class
                    Program program = new Program();
                    //Call method to start GIS Synch process
                    program.GISSynchProcess();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace); //Write to console the stack trace
                    Logger.WriteErrorLog("Program.Main(string[] args)" + ex.ToString());
                }

                gis_nav_multiProcessor g2n = new gis_nav_multiProcessor();
                g2n.runMultiProcessor();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace); //Write to console the stack trace
                Logger.WriteErrorLog("Program.Main(string[] args)" + ex.ToString());
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Method :  GISSynchProcess()
        /// Starts the GIS synch process 
        /// Waits synchronously for the GIS synch process to complete
        /// </summary>
        private void GISSynchProcess()
        {
            //retieve config settings
            try
            {
                int threadInterval = retrieveConfigSettings();

                //Service Reference
                ULIMSGISServiceClient client = new ULIMSGISServiceClient();

                //Enable timer
                string issuccess = client.startGISSyncProcess(true);
                //Write to console

                //Console.WriteLine("Default Timer Started");
                Console.WriteLine(issuccess);

                //Create infinit loop with 5 minutes break

                while (true)
                {
                    if (client.isSuccessGISSyncProcess() == true)
                    {

                        //Todo code to call ULIMS GIS Client Main

                        Console.WriteLine("GIS Sync Process has completed");
                        break;
                    }

                    //Write to console asking for patience
                    Console.WriteLine(String.Format("...Wait for {0} milliseconds for the GIS Process to Complete", threadInterval));

                    //Let cuurent thread sleep for x minutes
                    System.Threading.Thread.Sleep(threadInterval);
                }

                Console.ReadLine(); //Wait for user input
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("Program.GISSynchProcess() : ", ex);
            }
        }

        /// <summary>
        /// Reads from app.config file the time in millieseconds the thread should wait
        /// </summary>
        /// <returns> an interval in milliseconds: threadInterval </returns>
        private int retrieveConfigSettings()
        {
            try
            {
                //Get timer interval from app.config
                int threadInterval;
                //var config = ((ConfigLibrary.IConfigReader)(new ConfigLibrary.ConfigReader()));
                //threadInterval = Convert.ToInt32(config.MTimerInterval);//Read  timer value from app.config
                threadInterval= Convert.ToInt32(ConfigurationManager.AppSettings["thread_interval"].ToString());
                return threadInterval;
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("Program.retrieveConfigSettings() : ", ex);
            }
        }
    
        #endregion

    }
}

