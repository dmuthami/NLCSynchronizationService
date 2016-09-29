using System;
namespace ConfigLibrary
{
    public interface IConfigReader
    {
        /// <summary>
        /// Reads the app.config file. Specifically the section NamibiaLocalAuthorities and resturns a dictionary of the towns
        /// </summary>
        /// <returns>Dictionary: Contains list of towns</returns>
        System.Collections.Generic.Dictionary<string, string> readNamibiaLocalAuthoritiesSection();

        /// <summary>
        /// Property : MComputeStandNo
        /// Wrapped up in a getter and setter
        /// </summary>
        Boolean MComputeStandNo { get; set; }

        /// <summary>
        /// Property : MAutoReconcileAndPost
        /// Wrapped up in a getter and setter
        /// </summary>
        Boolean MAutoReconcileAndPost { get; set; }

        /// <summary>
        /// Property : MSQLJob
        /// Wrapped up in a getter and setter
        /// </summary>
        Boolean MSQLJob { get; set; }

        /// <summary>
        /// Property : MJobName
        /// Wrapped up in a getter and setter
        /// </summary>
        string MJobName { get; set; }

        /// <summary>
        /// Property : MUsername
        /// Wrapped up in a getter and setter
        /// </summary>
        string MUsername { get; set; }

        /// <summary>
        /// Property : MPassword
        /// Wrapped up in a getter and setter
        /// </summary>
        string MPassword { get; set; }

        /// <summary>
        /// Property : MServername
        /// Wrapped up in a getter and setter
        /// </summary>
        string MServername { get; set; }

        /// <summary>
        /// Property : MPythonFolder
        /// Wrapped up in a getter and setter
        /// </summary>
        string MPythonFolder { get; set; }

        /// <summary>
        /// Property : MTimerInterval
        /// Wrapped up in a getter and setter
        /// </summary>
        double MTimerInterval { get; set; }

    }     
}
