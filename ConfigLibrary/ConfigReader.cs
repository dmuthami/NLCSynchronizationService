using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.ulims.com.na;

namespace ConfigLibrary
{
    public class ConfigReader : ConfigLibrary.IConfigReader
    {
        #region Member Variables

        /*
         * Member Variable : Stand No
         * If true then python script to compute stand no is executed
         */
        private Boolean mComputeStandNo;

        /*
         * Member Variable : mAutoReconcileAndPost
         * If true then python script to conduct auto reconcile and post is executed
         */
        private Boolean mAutoReconcileAndPost;

        /*
         * Member Variable : mSQLJob
         * If true then SQLjob to dump edited records and deleted records in 
         *  temp_edited_parcel_h and temp_parcel_h tables is executed
        */
        private Boolean mSQLJob;

        /*
         * Member Variable : mJobName
         * Stores the name read from SQL job tag from Windows Service Host appconfig file
         */
        private string mJobName;

        /*
         * Member Variable : mUsername
         * Stores the user name read from user name tag from Windows Service Host appconfig file
         */
        private string mUsername;

        /*
         * Member Variable : mPassword
         * Stores the password read from password tag from Windows Service Host appconfig file
         */
        private string mPassword;

        /*
         * Member Variable : mServername
         * Stores the server name read from servername tag from Windows Service Host appconfig file
         */
        private string mServername;

        /*
         * Member Variable : mPythonFolder
         * Stores the python folder read from python folder tag from Windows Service Host appconfig file
         */
        private string mPythonFolder;

        /*
         * Member Variable : mTimerInterval
         * Stores the timer interval read from timer interval tag from Windows Service Host appconfig file
         */
        private double mTimerInterval;



        #endregion

        #region Getter and Setter Methods
        /// <summary>
        /// Property : MComputeStandNo
        /// Wrapped up in a getter and setter
        /// </summary>
        public Boolean MComputeStandNo
        {
            get
            {
                try
                {
                    mComputeStandNo = Convert.ToBoolean(ConfigurationManager.AppSettings["compute_stand_no"].ToString());
                    return mComputeStandNo;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs Boolean MComputeStandNo  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mComputeStandNo = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs Boolean MComputeStandNo  set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MAutoReconcileAndPost
        /// Wrapped up in a getter and setter
        /// </summary>
        public Boolean MAutoReconcileAndPost
        {
            get
            {
                try
                {
                    mAutoReconcileAndPost = Convert.ToBoolean(ConfigurationManager.AppSettings["auto_reconcile_and_post"].ToString());
                    return mAutoReconcileAndPost;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs Boolean MAutoReconcileAndPost  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mAutoReconcileAndPost = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs Boolean MAutoReconcileAndPost  set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MSQLJob
        /// Wrapped up in a getter and setter
        /// </summary>
        public Boolean MSQLJob
        {
            get
            {
                try
                {
                    mSQLJob = Convert.ToBoolean(ConfigurationManager.AppSettings["sql_job"].ToString());
                    return mSQLJob;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs Boolean MSQLJob  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mSQLJob = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs Boolean MSQLJob  set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MJobName
        /// Wrapped up in a getter and setter
        /// </summary>
        public string MJobName
        {
            get
            {
                try
                {
                    mJobName = ConfigurationManager.AppSettings["jobName"].ToString();
                    return mJobName;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MJobName  get : ", ex);
                }
            }
            set { mJobName = value; }
        }

        /// <summary>
        /// Property : MUsername
        /// Wrapped up in a getter and setter
        /// </summary>
        public string MUsername
        {
            get
            {
                try
                {
                    mUsername = (ConfigurationManager.AppSettings["username"].ToString());
                    return mUsername;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MUsername  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mUsername = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MUsername  set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MPassword
        /// Wrapped up in a getter and setter
        /// </summary>
        public string MPassword
        {
            get
            {
                try
                {
                    mPassword = (ConfigurationManager.AppSettings["password"].ToString());
                    return mPassword;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MPassword  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mPassword = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MPassword  set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MServername
        /// Wrapped up in a getter and setter
        /// </summary>
        public string MServername
        {
            get
            {
                try
                {
                    mServername = (ConfigurationManager.AppSettings["servername"].ToString());
                    return mServername;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MServername  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mServername = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MServername  set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MPythonFolder
        /// Wrapped up in a getter and setter
        /// </summary>
        public string MPythonFolder
        {
            get
            {
                try
                {
                    mPythonFolder = ConfigurationManager.AppSettings["python_folder"];
                    return mPythonFolder;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MPythonFolder  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mPythonFolder = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs string MPythonFolder  set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MTimerInterval
        /// Wrapped up in a getter and setter
        /// </summary>
        public double MTimerInterval
        {
            get
            {
                try
                {
                    mTimerInterval = Convert.ToDouble(ConfigurationManager.AppSettings["timer_interval"].ToString());//Read  timer value from app.config
                    return mTimerInterval;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs double MTimerInterval  get : ", ex);
                }
            }
            set
            {
                try
                {
                    mTimerInterval = value;
                }
                catch (Exception ex)
                {
                    
                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("ConfigReader.cs double MTimerInterval  set : ", ex);
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="MULIMSGISService"> The python library class</param>
        public ConfigReader()
        {
            //To do Code
        }
        #endregion

        #region Methods

        /// <summary>
        /// Reads the app.config file. Specifically the section NamibiaLocalAuthorities and resturns a dictionary of the towns
        /// </summary>
        /// <returns>Dictionary: Contains list of towns</returns>
        public Dictionary<String, String> readNamibiaLocalAuthoritiesSection()
        {
            try
            {
                //Create a a dictionary object and intialized to null. This will cntain a list of the local authorities read from the appconfig file
                Dictionary<String, String> dictionary = null;

                //Read the section contain key value pairs of the namibia local authorities
                var NamibiaLocalAuthorities = ConfigurationManager.GetSection("NamibiaLocalAuthorities") as NameValueCollection;

                //Check that the returned name value collection of the Namibia local authorities is not empty
                if (NamibiaLocalAuthorities != null)
                {
                    //Call back method that converts named value collection to a dictionary.dictio
                    dictionary = hashtableToDictionary(NamibiaLocalAuthorities);

                    //Write to Log file indicating 
                    this.writeSectionToLog(dictionary);
                }

                //Pass by reference the dictionary object
                return dictionary;
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ConfigReader.Dictionary<String, String> readNamibiaLocalAuthoritiesSection() : ", ex);

            }
        }

        /// <summary>
        /// //Loop thru the name value collection and add the key, value pairs to the dictionary
        /// </summary>
        /// <param name="nameValueCollection">This is being read </param>
        /// <returns></returns>
        private Dictionary<String, String> hashtableToDictionary(NameValueCollection nameValueCollection)
        {
            try
            {
                //load items from hashtable to dictionary
                Dictionary<String, String> namibiaTownsDictionary = new Dictionary<String, String>();

                //Loop thru the collection and add the key value pairs to the dictionary
                foreach (string key in nameValueCollection)
                {
                    //Add the key,value pairs after casting to String
                    namibiaTownsDictionary.Add((String)key, (String)nameValueCollection[key]);
                }
                //return dictionary with towns in it
                return namibiaTownsDictionary;
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ConfigReader.Dictionary<String, String> hashtableToDictionary(NameValueCollection nameValueCollection) : ", ex);

            }
        }

        /// <summary>
        /// Write NamibiaLocalAuthorities section of config path to the log file
        /// </summary>
        /// <param name="dictionary"></param>
        private void writeSectionToLog(Dictionary<String, String> dictionary)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder(""); //instatntiate stringbuilder object

                //Header line for the stringbuilder object
                stringBuilder.Append(Environment.NewLine + "Local authorities read from config file" + Environment.NewLine);

                //Loop over pairs with foreach loop
                foreach (KeyValuePair<string, string> townpair in dictionary)
                {
                    //Call function to execute python process for each town        
                    stringBuilder.Append(String.Format("Town : {0}", (String)townpair.Value));
                }
                //Write to log file
                Logger.WriteErrorLog("ConfigReader.writeSectionToLog(Dictionary<String, String> dictionary) : " +
                    Environment.NewLine + stringBuilder.ToString() + Environment.NewLine);

            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("ConfigReader.writeSectionToLog(Dictionary<String, String> dictionary) : ", ex);

            }

        }

        #endregion

    }
}
