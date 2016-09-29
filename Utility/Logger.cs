using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.ulims.com.na
{
    /// <summary>
    /// Class: Logger
    /// Writes errors and messages to log file
    /// </summary>
    public static class Logger
    {
        #region Member Variables

        private static string mExecutablePath;
        private static string mExecutableRootDirectory;
        private static string mFileName = null;

        #endregion

        #region Getter and Setters

        /// <summary>
        /// Property : mExecutablePath
        /// Wrapped up in a getter and setter
        /// </summary>
        public static string ExecutablePath
        {
            get { return mExecutablePath; }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(value.ToString()) == true)
                    {
                        mExecutablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;//get path to *.exe;//get path to *.exe
                    }                    
                }
                catch (Exception)
                {

                    throw;//In case of an error then throws it up the stack trace
                }
            }
        }
        
        /// <summary>
        /// Property : mExecutableRootDirectory
        /// Wrapped up in a getter and setter
        /// </summary>
        public static string ExecutableRootDirectory
        {
            get { return mExecutableRootDirectory; }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(value.ToString()) == true)
                    {
                        mExecutableRootDirectory = System.IO.Path.GetDirectoryName(ExecutablePath); // get directory containing the *.exe; // get directory containing the *.exe
                    }
                   
                }
                catch (Exception)
                {

                    throw;//In case of an error then throws it up the stack trace
                }
            }
        }

        /// <summary>
        /// Property : MFileName
        /// Wrapped up in a getter and setter
        /// </summary>
        public static string MFileName
        {
            get { return mFileName; }
            set { mFileName = value; }
        }
        #endregion

        #region Logger Methods

        /// <summary>
        /// Create a log method (WriteErrorLog) to log the exceptions
        /// </summary>
        /// <param name="ex"></param>        
        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter streamWriter = null;
            try
            {
                //initializes a new instance of the StreamWriter class for the specified file in the location of the *.exe. Allows create or append to the file.
                streamWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "logfile.txt", true);

                //Write string followed by line terminator. Components of string is time and source & message of the exception object
                streamWriter.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() +
                    ex.Message.ToString().Trim());

                //Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
                streamWriter.Flush();

                //Closes the current StreamWriter object and the underlying stream
                streamWriter.Close();
            }
            catch
            {
                //Nothing goes here
            }
        }

        /// <summary>
        /// Create a log method (WriteErrorLog) to log the custom messages
        /// </summary>
        /// <param name="Message"></param>
        public static void WriteErrorLog(string Message)
        {
            StreamWriter streamWriter = null;
            try
            {
                //initializes a new instance of the StreamWriter class for the specified file in the location of the *.exe. Allows create or append to the file.
                streamWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "logfile.txt", true);

                //Write string followed by line terminator. Components of string is time and custom message
                streamWriter.WriteLine(DateTime.Now.ToString() + ": " + Message);

                //Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
                streamWriter.Flush();

                //Closes the current StreamWriter object and the underlying stream
                streamWriter.Close();
            }
            catch
            {
                //Nothing goes here
            }
        }
   
        #endregion

    }
}
