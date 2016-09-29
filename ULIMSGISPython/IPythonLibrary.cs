using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ulimsgispython.ulims.com.na
{
    public interface IPythonLibrary
    {
        /// <summary>
        /// Method : executePythonProcess()
        /// Loops through a dictionary object listing the 10 piloting towns
        ///     For each town launches a process to perfom automatic reconcile and post
        /// </summary>
        /// 
        void executePythonProcess();

        /// <summary>
        /// Method : executePythonProcess(String townName)
        /// Creates a python process, passess it parameers and waits for completion. 
        /// Stdout from the python script is read asynchronously and captured into the .net log file
        /// </summary>
        /// <param name="townName"></param>
        /// <param name="pythonFileExecute"></param>  
        void executePythonProcessPerTown(string townName, String pythonFileToExecute);

        /// <summary>
        /// Property : mPythonCodeFolder
        /// Wrapped up in a getter and setter
        /// </summary>   
        string MPythonCodeFolder { get; set; }
    }
}