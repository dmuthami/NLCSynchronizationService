using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ULIMSWcfClient.GisProcessing
{
    public class ExecuteGIS2SPClient
    {
        public bool DoGISProcess()
        {
            try
            {
                DoSynchronize();

                return true;
            }
            catch (Exception)
            {                
                throw;
            }
        }
        private static async void DoSynchronize()
        {
            try
            {
                //Execution of synchronize initiated
                await Synchronize();
            }
            catch (Exception ex)
            {
                throw ex.InnerException.InnerException;
            }
        }
        private static async Task Synchronize()
        {
            try
            {
                Erfs erfs = new Erfs();
                //synchronizer Method initiated 
                //[Executes search of unsynchronized erfs from Gis that will be passed as a list/array to sharepoint]
                await erfs.Synchronizer();
            }
            catch (Exception ex)
            {
                throw ex.InnerException.InnerException;
            }
        }
    }
}