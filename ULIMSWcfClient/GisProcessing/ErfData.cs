using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.GisProcessing
{
    public class GisErfData
    {
        public int ObjectId { get; set; }
        public int RowNumber { get; set; }
        public Guid GlobalId { get; set; }
        public Nullable<decimal> ComputedSize { get; set; }
        public string Zoning { get; set; }
        public string Density { get; set; }
        public string ErfNo { get; set; }
        public string Ownership { get; set; }
        public string Portion { get; set; }
        public string StandNo { get; set; }
        public string Status { get; set; }
        public Nullable<decimal> SurveySize { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> LastEditDate { get; set; }
        public string LocalAuthority { get; set; }
        public string Township { get; set; }
        public string Comment { get; set; }
        public string GIsParent { get; set; }
        public string Restriction { get; set; }
    }
}