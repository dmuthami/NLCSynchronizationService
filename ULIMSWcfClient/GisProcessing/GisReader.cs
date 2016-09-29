using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.GisProcessing
{
    public class GisReader
    {
        public GisErfData ReaderToErfData(SqlDataReader reader)
        {
            GisErfData erfdata = new GisErfData();
            if (reader["computed_size"] is DBNull)
                erfdata.ComputedSize = null;
            else
                erfdata.ComputedSize = decimal.Parse(reader["computed_size"].ToString());

            erfdata.Density = reader["density"] is DBNull ? null : reader["density"].ToString();
            erfdata.ErfNo = reader["erf_no"] is DBNull ? null : reader["erf_no"].ToString();
            erfdata.GlobalId = Guid.Parse(reader["GlobalID"].ToString());
            erfdata.LocalAuthority = reader["local_authority_id"] is DBNull ? null : reader["local_authority_id"].ToString();
            erfdata.ObjectId = int.Parse(reader["OBJECTID"].ToString());
            erfdata.Ownership = reader["ownership"] is DBNull ? null : reader["ownership"].ToString();
            //erfdata.Portion = reader["portion"] is DBNull ? null : reader["portion"].ToString();
            erfdata.StandNo = reader["reference_no"] is DBNull ? null : reader["reference_no"].ToString();
            erfdata.Comment = reader["comment"] is DBNull ? null : reader["comment"].ToString();
            erfdata.GIsParent = reader["gis_parent"] is DBNull ? null : reader["gis_parent"].ToString();
            erfdata.Restriction = reader["restriction"] is DBNull ? null : reader["restriction"].ToString();
            //erfdata.Status = reader["status"] is DBNull ? null : reader["status"].ToString();

            if (reader["survey_size"] is DBNull)
                erfdata.SurveySize = null;
            else
                erfdata.SurveySize = decimal.Parse(reader["survey_size"].ToString());

            erfdata.Township = reader["township_id"] is DBNull ? null : reader["township_id"].ToString();
            erfdata.Zoning = reader["zoning_id"] is DBNull ? null : reader["zoning_id"].ToString();
            return erfdata;
        }
        public GisParcelData ReaderToParcelData(SqlDataReader reader)
        {
            GisParcelData parceldata = new GisParcelData();
            if (reader["computed_size"] is DBNull)
                parceldata.computed_size = null;
            else
                parceldata.computed_size = decimal.Parse(reader["computed_size"].ToString());

            parceldata.density = reader["density"] is DBNull ? null : reader["density"].ToString();
            parceldata.erf_no = reader["erf_no"] is DBNull ? null : reader["erf_no"].ToString();
            parceldata.GlobalID = Guid.Parse(reader["GlobalID"].ToString());
            parceldata.local_authority_id = reader["local_authority_id"] is DBNull ? null : reader["local_authority_id"].ToString();
            parceldata.OBJECTID = int.Parse(reader["OBJECTID"].ToString());
            parceldata.ownership = reader["ownership"] is DBNull ? null : reader["ownership"].ToString();
            parceldata.stand_no = reader["stand_no"] is DBNull ? null : reader["stand_no"].ToString();
            parceldata.comment = reader["comment"] is DBNull ? null : reader["comment"].ToString();
            parceldata.gis_parent = reader["gis_parent"] is DBNull ? null : reader["gis_parent"].ToString();
            parceldata.restriction = reader["restriction"] is DBNull ? null : reader["restriction"].ToString();

            if (reader["survey_size"] is DBNull)
                parceldata.survey_size = null;
            else
                parceldata.survey_size = decimal.Parse(reader["survey_size"].ToString());

            parceldata.township_id = reader["township_id"] is DBNull ? null : reader["township_id"].ToString();
            parceldata.zoning_id = reader["zoning_id"] is DBNull ? null : reader["zoning_id"].ToString();
            return parceldata;
        }
    }
}