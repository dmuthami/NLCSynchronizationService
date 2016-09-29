using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.ExecLogic
{
    public class gis_nav_multiProcessor
    {
        public bool runMultiProcessor()
        {
            bool isSuccess = false;

            try
            {
                var gisp = new gisp_Processor();
                var spnav = new spnav_Processor();

                isSuccess = gisp.GIS2SPExecution();

                if (isSuccess == true)
                    isSuccess = spnav.SP2NAVExecution();

                return isSuccess;

            }
            catch (Exception e)
            {
                throw new Exception("Error Occured : Result " + isSuccess + " , Log Message : ", e.InnerException.InnerException);
            }
        }
    }
}