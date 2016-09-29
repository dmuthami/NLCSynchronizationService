using System;
namespace ulimsgispython
{
    /// <summary>
    /// Contract: Specifiies how to connect to SQL and execute spefified job
    /// </summary>
    interface ISQLJob2
    {
        /// <summary>
        /// Method Name : Execute
        /// No argumeents
        /// Connects to SQL Server and fires up the SQL job
        /// </summary>
        void Execute();

        /// <summary>
        /// Property : InstanceName
        /// Wrapped up in a getter and setter
        /// </summary>
        string InstanceName { get; set; }

        /// <summary>
        /// Property : JobName
        /// Wrapped up in a getter and setter
        /// </summary>
        string JobName { get; set; }

        /// <summary>
        /// Property : Login
        /// Wrapped up in a getter and setter
        /// </summary>
        string Login { get; set; }

        /// <summary>
        /// Property : Password
        /// Wrapped up in a getter and setter
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Property : ServerName
        /// Wrapped up in a getter and setter
        /// </summary>
        string ServerName { get; set; }
    }
}
