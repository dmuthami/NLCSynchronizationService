using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ulimsgispython.ulims.com.na;

namespace wcf.ulims.com.na
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(Namespace = "http://wcf.ulims.com.na")]
    public interface IULIMSGISService
    {

        /// <summary>
        /// Indicated as a service contract method
        /// Exposed to the WCF client
        /// </summary>
        /// <param name="shallStart"></param>
        /// <returns></returns>
        [OperationContract]
        string startGISSyncProcess(bool shallStart);

        /// <summary>
        /// Property : isSuccessGISSyncProcess
        /// Exposed to the WCF client
        /// Wrapped up in a getter and setter
        /// </summary>
        [OperationContract]
        bool isSuccessGISSyncProcess();

        /// <summary>
        /// Property : mExecuting
        /// Checks that 
        /// Wrapped up in a getter and setter
        /// </summary>       
        bool mEexecuting { get; set; }

        /// <summary>
        /// Property : MGISSyncProcess
        /// Wrapped up in a getter and setter
        /// </summary>
        bool MGISSyncProcess { get; set; }

        /// <summary>
        /// Property : HasGISSyncProcessstarted
        /// Wrapped up in a getter and setter
        /// </summary>
        bool HasGISSyncProcessstarted { get; set; }

        /// <summary>
        /// Property : ULIMSSerializer.ULIMSSerializer mULIMSSerializer
        /// Instances the ULIMSSerializer.cs
        /// Wrapped up in a getter and setter
        /// </summary>
        ULIMSSerializer.ULIMSSerializer mULIMSSerializer { get; set; }

        /// <summary>
        /// Method : readObject()
        /// </summary>
        void readObject();

        /// <summary>
        /// Method saveObject
        /// Pass appropriate state as an argument 
        /// </summary>
        /// <param name="state1"></param>
        void saveObject(bool state1);

        /// <summary>
        /// Method saveObject
        /// Pass appropriate state as an argument 
        /// </summary>
        /// <param name="filePathForSerializedObject"> path to the binary file serializing the object</param>
        void saveObject(string filePathForSerializedObject);

        /// <summary>
        /// Property : MIPythonLibrary
        /// Wrapped up in a getter and setter
        /// </summary>
        IPythonLibrary MIPythonLibrary { get; set; }

        /// <summary>
        /// executePythonCode Method: Calls other functions to execute python code
        /// </summary>
        void executePythonCode();
    }

}
