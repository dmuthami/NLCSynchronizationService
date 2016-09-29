using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ULIMSWcfClient.SpNavProcessing;

namespace ULIMSWcfClient.ExecLogic
{    
    public class spnav_Processor
    {
        bool isSuccess = false;
        private ExecuteSP2NAVClient _spnavclient = new ExecuteSP2NAVClient();

        public bool SP2NAVExecution()
        {
            try
            {
                bool executed = _spnavclient.DoSpNavProcess();

                isSuccess = executed;

                return isSuccess;
            }
            catch (Exception e)
            {
                throw new Exception("Error Occured : Result " + isSuccess + " , Log Message : ", e.InnerException.InnerException);
            }
        }
    }
}