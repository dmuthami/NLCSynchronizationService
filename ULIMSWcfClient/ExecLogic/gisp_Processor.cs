using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ULIMSWcfClient.GisProcessing;

namespace ULIMSWcfClient.ExecLogic
{
    public class gisp_Processor
    {
        bool isSuccess = false;
        private ExecuteGIS2SPClient _gisclient = new ExecuteGIS2SPClient();
        public bool GIS2SPExecution()
        {
            try
            {
                bool executed = _gisclient.DoGISProcess();

                isSuccess = executed;

                return isSuccess;
            }
            catch (Exception e)
            {                
                throw new Exception("Error Occured : Result "+ isSuccess +" , Log Message : ", e.InnerException.InnerException);
            }
        }
    }
}