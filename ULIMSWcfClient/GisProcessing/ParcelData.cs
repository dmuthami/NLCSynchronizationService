using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.GisProcessing
{
    public class GisParcelData
    {
        public int OBJECTID { get; set; }
        //public int RowNumber { get; set; }
        public Guid GlobalID { get; set; }
        public Nullable<decimal> computed_size { get; set; }
        public string zoning_id { get; set; }
        public string density { get; set; }
        public string erf_no { get; set; }
        public string ownership { get; set; }
        public string portion { get; set; }
        public string stand_no { get; set; }
        public string status { get; set; }
        public Nullable<decimal> survey_size { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public Nullable<System.DateTime> last_edited_date { get; set; }
        public string local_authority_id { get; set; }
        public string township_id { get; set; }
        public string comment { get; set; }
        public string gis_parent { get; set; }
        public string restriction { get; set; }
        public int GDB_ARCHIVE_OID { get; set; }
    }
}