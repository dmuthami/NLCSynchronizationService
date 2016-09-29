using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;
using System.Data;
using System.Threading;
using System.Configuration;

using Utility.ulims.com.na;


namespace ulimsgispython.ulims.com.na
{
    /// <summary>
    /// SQLJob2.cs
    /// Connects to SQL server database and executes the relevant SQL Job
    /// The name of SQL job is read from the app.config file
    /// </summary>
    class SQLJob2 : ulimsgispython.ISQLJob2
    {
        #region Member Variables

        //Name of the job;
        string m_JobName = null; 

        //Name of the SQL Server instance
        string m_InstanceName = null;

        //Name of the server running SQL Server instance named above
        string m_ServerName = null;

       //Database Login of DBA or the pwner of the above stated job
        string m_Login = null;

        //Password for the above login
        string m_Password = null;

        #endregion

        #region Getter and Setter Methods

        /// <summary>
        /// Property : Password
        /// Wrapped up in a getter and setter
        /// </summary>
        public string Password
        {
            get
            {
                try
                {
                    return m_Password;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string Password get : ", ex);
                }
            }
            set
            {
                try
                {
                    m_Password = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string Password set : ", ex);
                }
            }
        }
        
        /// <summary>
        /// Property : Login
        /// Wrapped up in a getter and setter
        /// </summary>
        public string Login
        {
            get
            {
                try
                {
                    return m_Login;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string Login get : ", ex);
                }
            }
            set
            {
                try
                {
                    m_Login = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string Login set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : InstanceName
        /// Wrapped up in a getter and setter
        /// </summary>
        public string InstanceName
        {
            get
            {
                try
                {
                    return m_InstanceName;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string InstanceName get : ", ex);
                }
            }
            set
            {
                try
                {
                    m_InstanceName = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string InstanceName get : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : ServerName
        /// Wrapped up in a getter and setter
        /// </summary>
        public string ServerName
        {
            get
            {
                try
                {
                    return m_ServerName;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string ServerName get : ", ex);
                }
            }
            set
            {
                try
                {
                    m_ServerName = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string ServerName set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : JobName
        /// Wrapped up in a getter and setter
        /// </summary>
        public string JobName
        {
            get
            {
                try
                {
                    return m_JobName;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string JobName get : ", ex);
                }
            }
            set
            {
                try
                {
                    m_JobName = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("SQLJob2.string JobName set : ", ex);
                }
            }
        }

        #endregion

        #region  Constructors
        /// <summary>
        /// Constructor
        /// Method Name: SQLJob2
        /// </summary>
        public SQLJob2()
        {

        }

        public SQLJob2(string jobName, string instanceName, string serverName, string login, string password)
        {
            try
            {
                JobName = jobName;
                InstanceName = instanceName;
                ServerName = serverName;
                Login = login;
                Password = password;
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("SQLJob2.SQLJob2(string jobName, string instanceName, string serverName, string login, string password, IPythonLibrary iPythonLibrary) Constructor: ", ex);
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Method Name : Execute
        /// No argumeents
        /// Connects to SQL Server and fires up the SQL job
        /// </summary>
        public void Execute()
        {
            try
            {
                //Server connection with server name as parameter
                ServerConnection serverConnection = new ServerConnection(ServerName);

                Server server = new Server(); //Default instance
                try
                {
                    server.ConnectionContext.LoginSecure = false; //Set to false since we are using datbase authentication

                    //Login credentials
                    server.ConnectionContext.Login = Login;
                    server.ConnectionContext.Password = Password;
                    server.ConnectionContext.Connect();


                    //Get job instance
                    Job job = server.JobServer.Jobs[JobName];

                    //Start the job
                    job.Start();

                    /*
                     * Job execution is synchronous so lets use a thread to know when it completed
                     */

                    //Thread wait in miliseconds
                    int threadWait = 5000;

                    Thread.Sleep(threadWait); //Sleep or some time

                    job.Refresh();//Refresh the job thread.

                    //Check if the job is complete 
                    while (job.CurrentRunStatus != JobExecutionStatus.Idle /*4*/)
                    {

                        //Write to console asking for patience as the job executes
                        Logger.WriteErrorLog(String.Format("{0}Job Name : {1} is processing. Please wait for {2} milliseconds", Environment.NewLine, JobName, threadWait));
                        Thread.Sleep(threadWait); //Sleep for some time
                        job.Refresh();
                    }
                    //Log message
                    Logger.WriteErrorLog(String.Format("{0}Job Name : {1} has successfully completed", Environment.NewLine, JobName)); //Write to console saying we are done
                }
                finally
                {
                    if (server.ConnectionContext.IsOpen) //Check if still connection is open
                    {
                        server.ConnectionContext.Disconnect();//Safely disconnect SQL Server
                        Logger.WriteErrorLog(String.Format("{0}Job Name : {1} has successfully completed", Environment.NewLine, JobName)); //Write to console saying we are done

                    }

                }
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("SQLJob2.Execute() : ", ex);
            }

        }

        #endregion

    }
}
