using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULIMSSerializer
{

    /// <summary>
    /// Class performs binary serialization
    /// </summary>
    [Serializable()]
    public class ULIMSSerializer
    {

        #region Properties to Serialize

        /// <summary>
        /// Property : HasGISSyncProcessstarted
        /// Wrapped up in a getter and setter
        /// Fired up by the WCF client
        /// Stores status of whether GIS  sync process has started
        /// </summary>
       public bool  HasGISSyncProcessstarted { get; set; }

       /// <summary>
       /// Property : HasGISSyncProcessCompleted
       /// Wrapped up in a getter and setter
       /// Stores status of whether GIS  sync process has completed
       /// </summary>
       public bool HasGISSyncProcessCompleted { get; set; }

        #endregion

    }
}