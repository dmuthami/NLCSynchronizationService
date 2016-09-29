using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using Utility.ulims.com.na;//Process

using ConfigLibrary;
using System.Data.SqlClient;
using System.Data; //Reads from config file

namespace ulimsgispython.ulims.com.na
{
    /// <summary>
    /// PythonLibrary Implements IPythonLibrary
    /// Responsible for executing all python scripts
    /// Specifically, compute stand no and Auto Reconcile and Post scripts
    /// </summary>
    public class PythonLibrary : IPythonLibrary
    {

        #region Member Variables

        //Add memeber variables here
        private StringBuilder mSortOutput;

        //Variable serves as a memory pointer to the output lines being written to the console or log file
        private int mNumOutputLines;

        //Varible stores path to the python folder 
        private string mPythonCodeFolder;

        #endregion

        #region Getter and Setters

        /// <summary>
        /// Property : MSortOutput
        /// Wrapped up in a getter and setter
        /// </summary>
        public StringBuilder MSortOutput
        {
            get
            {
                try
                {
                    return mSortOutput;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("PythonLibrary.cs StringBuilder MSortOutput get : ", ex);
                }
            }
            set
            {
                try
                {
                    mSortOutput = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("PythonLibrary.cs StringBuilder MSortOutput set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MNumOutputLines
        /// Wrapped up in a getter and setter
        /// </summary>
        public int MNumOutputLines
        {
            get
            {
                try
                {
                    return mNumOutputLines;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("PythonLibrary.cs int MNumOutputLines get : ", ex);
                }
            }
            set
            {
                try
                {
                    mNumOutputLines = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("PythonLibrary.cs int MNumOutputLines set : ", ex);
                }
            }
        }

        /// <summary>
        /// Property : MPythonCodeFolder
        /// Wrapped up in a getter and setter
        /// </summary>
        public string MPythonCodeFolder
        {
            get
            {
                try
                {
                    return mPythonCodeFolder;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("PythonLibrary.cs string MPythonCodeFolder get : ", ex);
                }
            }
            set
            {
                try
                {
                    mPythonCodeFolder = value;
                }
                catch (Exception ex)
                {

                    //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                    throw new Exception("PythonLibrary.cs string MPythonCodeFolder set : ", ex);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method : executePythonProcess()
        /// Loops through a dictionary object listing the 10 piloting towns
        ///     For each town launches a process to perfom automatic reconcile and post
        /// </summary>
        public void executePythonProcess()
        {
            try
            {
                //Create a dictionary object with all the 10 piloting sites
                Dictionary<string, string> dictionary = new Dictionary<string, string>();


                //Read towns from config file
                IConfigReader iConfigReader = new ConfigReader();

                //Method returns listof towns as a dictionary
                dictionary = iConfigReader.readNamibiaLocalAuthoritiesSection();

                //Check if we can compute stand number
                Boolean computeStandNo = iConfigReader.MComputeStandNo;


                //Check if we can conduct auto reconcile and post
                Boolean autoReconcileAndPost = iConfigReader.MAutoReconcileAndPost;


                //Call code to excute SQL job
                //Check if we can fire the SQL job
                Boolean sqlJob = iConfigReader.MSQLJob;

                //Check if we can conduct sql job execution
                //Read job name from appconfig file
                string jobName = iConfigReader.MJobName;
                if (sqlJob == true)
                {
                    //get all fabrics
                    getAllFabrics();

                    /*
                     * Westlands Parcels
                     * 
                     */

                    String fabricName = "[fabric_registry_nairobi].gisadmin_fabric.WESTLANDS_PARCELS_H";
                    String deletesTable = "[fabric_registry_nairobi].sde.WESTLANDS_PARCELS_H_tmpDeleted";
                    String editsTable = "[fabric_registry_nairobi].sde.WESTLANDS_PARCELS_H_tmpEdited";

                    //get parcel fabric edits
                    getParcelFabricEdits(fabricName, deletesTable, editsTable);

                    //update fabric messages table
                    try
                    {
                        updateFabricMessagesTable("sde.parcelfabricstatus", "sde.parcelfabricstatus", "Fabrics in system");
                        updateFabricMessagesTable(deletesTable, deletesTable, "Deleted Records");
                        updateFabricMessagesTable(editsTable, editsTable, "Deleted Records");
                    }
                    catch (Exception ex)
                    {

                        throw new Exception("getParcelFabricEdits : ", ex); ;
                    }


                    /*
                     * Buruburu Parcels
                     * 
                     */

                    fabricName = "[fabric_registry_nairobi].gisadmin_fabric.BURUBURU_PARCELS_H";
                    deletesTable = "[fabric_registry_nairobi].sde.BURUBURU_PARCELS_H_tmpDeleted";
                    editsTable = "[fabric_registry_nairobi].sde.BURUBURU_PARCELS_H_tmpEdited";

                    //get parcel fabric edits
                    getParcelFabricEdits(fabricName, deletesTable, editsTable);

                    //update fabric messages table
                    try
                    {
                       
                        updateFabricMessagesTable(deletesTable, deletesTable , "Deleted Records");
                        updateFabricMessagesTable(editsTable, editsTable , "Deleted Records");
                    }
                    catch (Exception ex)
                    {

                        throw new Exception("getParcelFabricEdits : ", ex); ;
                    }

                }
                else
                {

                    string msg = String.Format("{0}Execution of Job Name : {1} has not been fired/started.", Environment.NewLine, jobName);
                    Logger.WriteErrorLog("PythonLibrary.executePythonProcess() : " + msg);

                }

            }
            catch (Exception ex)
            {
                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("PythonLibrary.executePythonProcess() : ", ex);
            }

        }

        private void getAllFabrics()
        {
            try
            {
                string connectionString = "Data Source=localhost;Initial Catalog=fabric_registry_nairobi;Integrated Security=True";
                SqlConnection sqlConnection1 = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "sde.fetchFabrics";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();

                reader = cmd.ExecuteReader();
                // Data is accessible through the DataReader object here.

                sqlConnection1.Close();

            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("getAllFabrics : ", ex);
            }
        }

        private void getParcelFabricEdits(String fabricName, String deletesTable, String editsTable)
        {
            try
            {
                string connectionString = "Data Source=localhost;Initial Catalog=fabric_registry_nairobi;Integrated Security=True";
                SqlConnection sqlConnection1 = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "sde.getFabricDeletesEdits";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@myTable", fabricName));
                cmd.Parameters.Add(new SqlParameter("@myDel", deletesTable));
                cmd.Parameters.Add(new SqlParameter("@myEdits", editsTable));
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();

                reader = cmd.ExecuteReader();
                // Data is accessible through the DataReader object here.

                sqlConnection1.Close();

                //Get the results and write to parcel messages


            }
            catch (Exception ex)
            {

                throw new Exception("getParcelFabricEdits : ", ex);
            }
        }

        private void updateFabricMessagesTable(string tableName, string title, string status)
        {
            string connectionString = "Data Source=localhost;Initial Catalog=fabric_registry_nairobi;Integrated Security=True";
            //
            // Create new SqlConnection object.
            //

            int count = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //
                // Create new SqlCommand object.
                //
                using (SqlCommand command = new SqlCommand("SELECT count(*) FROM " + tableName, connection))
                {
                    //
                    // Invoke ExecuteReader method.
                    //
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        count = reader.GetInt32(0);    // count
                    }
                    reader.Close();
                }

            }

            string messagesTable = "sde.fabric_messages2";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //Please try an insert
                try
                {
                    int tableCount = 0;
                    string[] arr = tableName.Split('.');
                    //Check if the record entry for the table exists
                    //SqlCommand command = new SqlCommand("SELECT count(*) FROM " + messagesTable + " where CONVERT(VARCHAR, origin) = " + "'" + tableName + "'"
                    using (SqlCommand command = new SqlCommand("SELECT count(*) FROM " + messagesTable + " where title = " + "'" + tableName + "'", connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            tableCount = reader.GetInt32(0);    // count
                        }
                        reader.Close();
                    }

                    if (tableCount > 0) //update
                    {
                        //"UPDATE " + messagesTable + " SET title=@title, body=@body, status=@status, origin=@origin, time=@time where CONVERT(VARCHAR, origin) = " + "'" + tableName + "'"
                        using (SqlCommand command = new SqlCommand(
                            "UPDATE " + messagesTable + " SET title=@title, body=@body, status=@status, origin=@origin, time=@time where title = " + "'" + tableName + "'", connection))
                        {
                            command.Parameters.Add(new SqlParameter("title", title.ToString()));
                            command.Parameters.Add(new SqlParameter("body", count.ToString()));
                            command.Parameters.Add(new SqlParameter("status", status.ToString()));
                            command.Parameters.Add(new SqlParameter("origin", "GIS"));
                            command.Parameters.Add(new SqlParameter("time", System.DateTime.Now));
                            command.ExecuteNonQuery();

                        }
                    }
                    else //insert
                    {
                        using (SqlCommand command = new SqlCommand(
                            "INSERT INTO " + messagesTable + " (title,body,status,origin,time) VALUES(@title, @body, @status, @origin, @time)", connection))
                        {
                            command.Parameters.Add(new SqlParameter("title", title.ToString()));
                            command.Parameters.Add(new SqlParameter("body", count.ToString()));
                            command.Parameters.Add(new SqlParameter("status", status.ToString()));
                            command.Parameters.Add(new SqlParameter("origin", "GIS"));
                            command.Parameters.Add(new SqlParameter("time", System.DateTime.Now));
                            command.ExecuteNonQuery();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.WriteErrorLog("Failed to write to fabric messages table");
                    throw new Exception("updateFabricMessagesTable : ", ex);
                }

            }
        }
        /// <summary>
        /// Method : executePythonProcess(String townName)
        /// Creates a python process, passess it parameers and waits for completion. 
        /// Stdout from the python script is read asynchronously and captured into the .net log file
        /// </summary>
        /// <param name="townName"></param>
        /// <param name="pythonFileExecute"></param>  
        public void executePythonProcessPerTown(String townName, String pythonFileToExecute)
        {
            try
            {

                //Get path of config file
                String configFilePath = "\"" + Logger.ExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\Config.ini", townName) + "\"";

                //Get path of main python file
                String pathToPythonMainFile = "\"" + Logger.ExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\{1}", mPythonCodeFolder, pythonFileToExecute) + "\"";

                //Get path of reconcile log file
                String reconcileLogFilePath = "\"" + Logger.ExecutableRootDirectory + String.Format("\\local_authorities\\{0}\\{0}_reconcile.log", townName) + "\"";

                //Set path for current directory or strictly speaking directory of interest that you want to make current
                String currDirPath = "\"" + Logger.ExecutableRootDirectory + String.Format("\\local_authorities", "") + "\"";

                //Create an instance of Python Process class
                Process process = new Process();

                /*
                 * We execute python code
                 * Ensure python path is referenced in the Environment variables
                 */
                process.StartInfo.FileName = "python.exe";

                //make sure we can read output from stdout
                process.StartInfo.UseShellExecute = false;

                // Redirect the standard output of the sort command.  
                // This stream is read asynchronously using an event handler.
                process.StartInfo.RedirectStandardOutput = true;

                //intialize pointer to memory location storing a Stringbuilder object
                MSortOutput = new StringBuilder("");

                // Set our event handler to asynchronously read the sort output.
                process.OutputDataReceived += new DataReceivedEventHandler(sortOutputHandler);

                /*
                 * Start the program with 4 parameters.NB Use of escape characters to escape spaces in file paths
                 * 
                 * First argument: Tells pyhton which python script to execute
                 * Second argument: Supplys the path to the config file
                 * Third argument: Supplys the path to the reconcile log file
                 * Fourth argument: Supplies the current directory. NB. Different from current working directory
                 */
                process.StartInfo.Arguments = pathToPythonMainFile + " " + configFilePath + " " + reconcileLogFilePath + " " + currDirPath;

                //Start the process (i.e the python program)
                process.Start();

                // To avoid deadlocks, use asynchronous read operations on at least one of the streams.
                // Do not perform a synchronous read to the end of both redirected streams.
                process.BeginOutputReadLine();

                //Wait for the python process
                process.WaitForExit();

                Logger.WriteErrorLog("PythonLibrary.executePythonProcessPerTown(String townName, String pythonFileToExecute) : " + MSortOutput.ToString());//Write to dotnet log file

                //Releases all resources by the component
                process.Close();

            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("PythonLibrary.executePythonProcessPerTown(String townName, String pythonFileToExecute): ", ex);
            }

        }

        /// <summary>
        /// event Handler : sortOutputHandler
        /// Asynchronously captures ouptput writen to console by python
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param> 
        private void sortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                // Collect the sort command output.
                if (!String.IsNullOrEmpty(outLine.Data))
                {
                    MNumOutputLines++; //concatenate

                    // Add the text to the collected output.
                    MSortOutput.Append(Environment.NewLine +
                        "[" + MNumOutputLines.ToString() + "] - " + outLine.Data);
                }
            }
            catch (Exception ex)
            {

                //In case of an error then throws it explicitly up the stack trace and add a message to the re-thrown error
                throw new Exception("PythonLibrary.sortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine) : ", ex);
            }
        }

        #endregion

    }
}
