using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ULIMSWcfClient.Configuration;
using ULIMSWcfClient.ConfigurationWeb;
using ULIMSWcfClient.Data;
using ULIMSWcfClient.svcSpErf;

namespace ULIMSWcfClient.GisProcessing
{
    public class Erfs
    {
        private List<LocalAuthorityTableElement> _tables;
        private List<LocalAuthorityViewElement> _views;
        private List<LocalAuthorityElement> _element;

        //Map Gis Data from SQL Query to reader to enable list execution
        GisReader _gisReader = new GisReader();

        //get all unsynchronized data from GIS and send to Sharepoint [Instantiation of GIS-SP Execution]
        gistospExecution _gisExec = new gistospExecution();

        //Get table name using table element
        public List<LocalAuthorityTableElement> Tables
        {
            get
            {
                if (_tables == null || _tables.Count == 0)
                {
                    _tables = new List<LocalAuthorityTableElement>();
                    LocalAuthorityTableSection section = (LocalAuthorityTableSection)ConfigurationManager.GetSection("LocalAuthorityTables");
                    if (section != null)
                    {
                        foreach (LocalAuthorityTableElement element in section.LocalAuthoritiesKeys)
                        {
                            _tables.Add(element);
                        }
                    }
                }
                return _tables;
            }
        }

        //Get view name using view element
        public List<LocalAuthorityViewElement> Views
        {
            get
            {
                if (_views == null || _views.Count == 0)
                {
                    _views = new List<LocalAuthorityViewElement>();
                    LocalAuthorityViewSection section = (LocalAuthorityViewSection)ConfigurationManager.GetSection("LocalAuthorityViews");
                    if (section != null)
                    {
                        foreach (LocalAuthorityViewElement element in section.LocalAuthoritiesKeys)
                        {
                            _views.Add(element);
                        }
                    }
                }
                return _views;
            }
        }

        /// <summary>
        /// Gets Unsynchronized data and Appends to the Send synchronize 
        /// task that appends it to the sharepoint execution method.
        /// </summary>
        /// <param name="localAuthority"></param>
        /// <param name="localAuthorityCode"></param>
        /// <param name="viewname"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SynchronizeErfs(string localAuthority, string localAuthorityCode, string viewname, GisObjectState state = GisObjectState.New)
        {
            try
            {
                Console.Write(string.Format(@"Sync ERf {0}", viewname));
                IEnumerable<GisErfData> erfdata;
                bool nomorerecords = false;

                do
                {
                    erfdata = await GetUnsynchronizedItemsAsync(localAuthority, viewname, state);

                    if (erfdata.Count() < ConfigHelper.GetPageSize)
                    {
                        nomorerecords = true;
                        if (erfdata.Count() == 0)
                            break;
                    }

                    Task<bool> sendToSPTask = SendUnsynchronizedItems(erfdata, localAuthorityCode, state);

                    bool isSentSuccesfull = await sendToSPTask;
                    Console.Write(string.Format(@"isSentSuccesfull: {0}", isSentSuccesfull));
                    if (isSentSuccesfull)
                    {
                        Task<bool> flagItemsTask;

                        flagItemsTask = FlagSynchronizedItems(localAuthority, erfdata, viewname);

                        bool isFlagSuccess = await flagItemsTask;
                        Console.Write(string.Format(@"IsFlagged: {0}", isFlagSuccess));
                    }
                } while (!nomorerecords);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw ex;
            }
        }

        public async Task SynchronizeDelErfs(string localAuthority, string localAuthorityCode, string viewname, GisObjectState state = GisObjectState.New)
        {
            try
            {
                Console.Write(string.Format(@"Sync ERf {0}", viewname));
                IEnumerable<GisParcelData> parceldata;
                bool nomorerecords = false;

                do
                {
                    parceldata = await GetDeletedItemsAsync(localAuthority, viewname, state);

                    if (parceldata.Count() < ConfigHelper.GetPageSize)
                    {
                        nomorerecords = true;
                        if (parceldata.Count() == 0)
                            break;
                    }

                    Task<bool> sendToSPTask = SendDeletedItems(parceldata, localAuthorityCode, state);

                    bool isSentSuccesfull = await sendToSPTask;
                    Console.Write(string.Format(@"isSentSuccesfull: {0}", isSentSuccesfull));
                    if (isSentSuccesfull)
                    {
                        Task<bool> flagItemsTask;
                        if (state == GisObjectState.Deleted)
                        {
                            flagItemsTask = FlagDeletedItems(localAuthority, parceldata, viewname);

                            bool isFlagSuccess = await flagItemsTask;
                            Console.Write(string.Format(@"IsFlagged: {0}", isFlagSuccess));
                        }
                    }
                } while (!nomorerecords);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw ex;
            }
        }

        public async Task<bool> FlagDeletedItems(string localAuthorityDB, IEnumerable<GisParcelData> parcdata, string tablename)
        {
            //**************Update All Data Sent to SHAREPOINT SITE[set update status to 1]******************
            Console.Write(string.Format(@"flaf: {0}", tablename));
            string connectionString = ConnectionInfo.Default.ConnectionString;

            var csbuilder = new SqlConnectionStringBuilder(connectionString);
            csbuilder.InitialCatalog = localAuthorityDB;
            connectionString = csbuilder.ConnectionString;

            string query = @"update {0}
                           set [update_to_sp] = 1
                           where {1}";

            StringBuilder sb = new StringBuilder();
            bool firstLoop = true;
            foreach (GisParcelData parc in parcdata)
            {
                if (firstLoop)
                {
                    sb.Append(string.Format("([GlobalID] = '{0}')", parc.GlobalID));
                    firstLoop = false;
                }
                else
                    sb.Append(string.Format(" or ([GlobalID] = '{0}')", parc.GlobalID));
            }

            query = string.Format(query, tablename, sb.ToString());
            var asyncConnectionString = new SqlConnectionStringBuilder(connectionString)
            {
                AsynchronousProcessing = true
            }.ToString();

            using (var conn = new SqlConnection(asyncConnectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    conn.Open();
                    Task<int> returnResultTask = cmd.ExecuteNonQueryAsync();
                    int returnResult = await returnResultTask;
                    return returnResult > 0;
                }
            }
            //***********************************************************
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<GisParcelData>> GetDeletedItemsAsync(string localAuthorityDB, string tablename, GisObjectState state)
        {
            //**********************Get Deleted Async Data from GIS*************************
            try
            {
                string connectionString = ConnectionInfo.Default.ConnectionString;

                var csbuilder = new SqlConnectionStringBuilder(connectionString);
                csbuilder.InitialCatalog = localAuthorityDB;
                connectionString = csbuilder.ConnectionString;

                string recstate = state == GisObjectState.New ? "0"
                            : state == GisObjectState.Edited ? "2"
                            : state == GisObjectState.Synchonized ? "1"
                            : state == GisObjectState.Deleted ? "3"
                            : "0";

                string selectStatement = string.Format(@"SELECT TOP {0}
                                                        [OBJECTID],[GlobalID],[computed_size],[zoning_id],
                                                        [density],[erf_no],[ownership],[portion],[stand_no],
                                                        [status],[survey_size],[created_date],[last_edited_date],
                                                        [local_authority_id],[township_id],[comment],[gis_parent],	
                                                        [restriction],[GDB_ARCHIVE_OID],[update_to_sp] 
                                                        FROM {1}    
                                                        WHERE stand_no IS NOT NULL
                                                        AND [update_to_sp] = {2} AND update_to_sp <> 1"
                                                        , ConfigHelper.GetPageSize, tablename, recstate);

                var asyncConnectionString = new SqlConnectionStringBuilder(connectionString)
                {
                    AsynchronousProcessing = true
                }.ToString();

                using (var conn = new SqlConnection(asyncConnectionString))
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = selectStatement;
                        cmd.CommandType = CommandType.Text;

                        conn.Open();

                        var reader = await cmd.ExecuteReaderAsync();

                        using (reader)
                        {
                            var parceldata = reader.Select(r => _gisReader.ReaderToParcelData(r)).ToList();
                            if (reader.HasRows == true)
                            {
                                return parceldata;
                            }
                            else
                            {
                                return parceldata;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            //****************************************************************
            throw new NotImplementedException();
        }

        private async Task<bool> SendDeletedItems(IEnumerable<GisParcelData> parceldata, string localAuthorityCode, GisObjectState state)
        {
            //*********************Sending Deleted Items from GIS to SHAREPOINT*********************
            Console.Write(string.Format(@"send: {0}\n", localAuthorityCode));
            try
            {
                using (ErfClient client = new ErfClient())
                {
                    //client.ClientCredentials.UserName.UserName = "admin";
                    //client.ClientCredentials.UserName.Password = "admin";
                    //client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode =
                    //                    X509CertificateValidationMode.None;

                    //bool pong = client.Ping();

                    ErfData[] erfs = MapMeterParcelData(parceldata);

                    ObjectState objState = state == GisObjectState.New ? ObjectState.New
                                            : state == GisObjectState.Edited ? ObjectState.Update
                                            : state == GisObjectState.Synchonized ? ObjectState.Unchanged
                                            : state == GisObjectState.Deleted ? ObjectState.Delete
                                            : ObjectState.Ignore;

                    bool spExecution = _gisExec.SaveRecords(erfs, localAuthorityCode, objState);
                    bool isErfDataSent = spExecution;
                    return isErfDataSent;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw ex;
            }
            //***************************************************************
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets Unsynchronized Items from GIS erven views
        /// </summary>
        /// <param name="localAuthority"></param>
        /// <param name="viewname"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GisErfData>> GetUnsynchronizedItemsAsync(string localAuthority, string viewname, GisObjectState state)
        {
            string connectionString = ConnectionInfo.Default.ConnectionString;

            var csbuilder = new SqlConnectionStringBuilder(connectionString);
            csbuilder.InitialCatalog = localAuthority;
            connectionString = csbuilder.ConnectionString;

            string recstate = state == GisObjectState.New ? "0"
                            : state == GisObjectState.Edited ? "2"
                            : state == GisObjectState.Synchonized ? "1"
                            : "0";

            string selectStatement = string.Format(@"SELECT TOP {0} 
                                                     [reference_no],[erf_no],[zoning_id],[township_id]
                                                    ,[survey_size],[comment],[gis_parent],[update_to_sp]
                                                    ,[local_authority_id],[OBJECTID],[computed_size]
                                                    ,[restriction],[ownership],[density],[GlobalID]
                                                     FROM {1}
                                                     WHERE [update_to_sp] = {2}", ConfigHelper.GetPageSize, viewname, recstate);

            var asyncConnectionString = new SqlConnectionStringBuilder(connectionString)
            {
                AsynchronousProcessing = true
            }.ToString();

            using (var conn = new SqlConnection(asyncConnectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = selectStatement;
                    cmd.CommandType = CommandType.Text;

                    conn.Open();
                    var reader = await cmd.ExecuteReaderAsync();

                    using (reader)
                    {
                        var erfdata = reader.Select(r => _gisReader.ReaderToErfData(r)).ToList().AsQueryable();
                        if (reader.HasRows == true)
                        {
                            return erfdata;
                        }
                        else
                        {
                            return erfdata;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Maps Unsynchronized Items to generate an array list
        /// the mapped array list is eventually passed to SharePoint SaveRecords
        /// </summary>
        /// <param name="erfdata"></param>
        /// <param name="localAuthorityCode"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<bool> SendUnsynchronizedItems(IEnumerable<GisErfData> erfdata, string localAuthorityCode, GisObjectState state)
        {
            Console.Write(string.Format(@"send: {0}\n", localAuthorityCode));
            try
            {
                using (ErfClient client = new ErfClient())
                {
                    ErfData[] erfs = MapMeterErfData(erfdata);

                    ObjectState objState = state == GisObjectState.New ? ObjectState.New
                                            : state == GisObjectState.Edited ? ObjectState.Update
                                            : state == GisObjectState.Synchonized ? ObjectState.Unchanged
                                            : ObjectState.Ignore;

                    bool spExecution = _gisExec.SaveRecords(erfs, localAuthorityCode, objState);
                    bool isErfDataSent = spExecution;
                    return isErfDataSent;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// ErfMapper - maps data executed from SQL Command query
        /// </summary>
        /// <param name="erfdata"></param>
        /// <returns></returns>
        private ErfData[] MapMeterErfData(IEnumerable<GisErfData> erfdata)
        {
            List<ErfData> erfs = new List<ErfData>();

            foreach (GisErfData item in erfdata)
            {
                erfs.Add(new ErfData()
                {
                    ComputedSize = item.ComputedSize,
                    CreatedDate = item.CreatedDate,
                    Density = item.Density,
                    ErfNo = item.ErfNo,
                    GlobalId = item.GlobalId,
                    LastEditDate = item.LastEditDate,
                    LocalAuthority = item.LocalAuthority,
                    Ownership = item.Ownership,
                    Portion = item.Portion,
                    //RowNumber = item.RowNumber,
                    StandNo = item.StandNo,
                    Status = item.Status,
                    SurveySize = item.SurveySize,
                    Township = item.Township,
                    Zoning = item.Zoning
                });
            }
            return erfs.ToArray();
        }

        /// <summary>
        /// DeletedErfMapper - maps data executed from SQL Command query
        /// </summary>
        /// <param name="erfdata"></param>
        /// <returns></returns>
        private ErfData[] MapMeterParcelData(IEnumerable<GisParcelData> erfdata)
        {
            List<ErfData> erfs = new List<ErfData>();

            foreach (GisParcelData item in erfdata)
            {
                erfs.Add(new ErfData()
                {
                    ComputedSize = item.computed_size,
                    CreatedDate = item.created_date,
                    Density = item.density,
                    ErfNo = item.erf_no,
                    GlobalId = item.GlobalID,
                    LastEditDate = item.last_edited_date,
                    LocalAuthority = item.local_authority_id,
                    Ownership = item.ownership,
                    Portion = item.portion,
                    //RowNumber = item.RowNumber,
                    StandNo = item.stand_no,
                    Status = item.status,
                    SurveySize = item.survey_size,
                    Township = item.township_id,
                    Zoning = item.zoning_id
                });
            }
            return erfs.ToArray();
        }

        /// <summary>
        /// Synchronizes sent Erfs and changes their update to sharepoint state
        /// </summary>
        /// <param name="localAuthority"></param>
        /// <param name="erfdata"></param>
        /// <param name="viewname"></param>
        /// <returns></returns>
        private async Task<bool> FlagSynchronizedItems(string localAuthority, IEnumerable<GisErfData> erfdata, string viewname)
        {
            Console.Write(string.Format(@"flaf: {0}", viewname));
            string connectionString = ConnectionInfo.Default.ConnectionString;

            var csbuilder = new SqlConnectionStringBuilder(connectionString);
            csbuilder.InitialCatalog = localAuthority;
            connectionString = csbuilder.ConnectionString;

            string query = @"update {0}
                           set [update_to_sp] = 1
                           where {1}";


            StringBuilder sb = new StringBuilder();
            bool firstLoop = true;
            foreach (GisErfData erf in erfdata)
            {
                if (firstLoop)
                {
                    sb.Append(string.Format("([GlobalID] = '{0}')", erf.GlobalId));
                    firstLoop = false;
                }
                else
                    sb.Append(string.Format(" or ([GlobalID] = '{0}')", erf.GlobalId));
            }

            query = string.Format(query, viewname, sb.ToString());
            var asyncConnectionString = new SqlConnectionStringBuilder(connectionString)
            {
                AsynchronousProcessing = true
            }.ToString();

            using (var conn = new SqlConnection(asyncConnectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    conn.Open();
                    Task<int> returnResultTask = cmd.ExecuteNonQueryAsync();
                    int returnResult = await returnResultTask;
                    return returnResult > 0;
                }
            }
        }

        /// <summary>
        /// This asynchronous tasks of methods that cummulatively Gets Erfs,
        /// Sends Erfs and in turn flags the sent Erfs.
        /// This goes for all New, Edited and Deleted Erfs
        /// </summary>
        /// <returns></returns>
        public async Task Synchronizer()
        {
            try
            {
                List<Task> syncTasks = new List<Task>();
                Task newTask;
                Task updateTask;
                Task deleteTask;
                string localAuthority;
                string localAuthorityCode;
                string viewname;
                string tablename;

                foreach (LocalAuthorityViewElement element in Views)
                {
                    localAuthority = element.LocalAuthority;
                    localAuthorityCode = element.Code;
                    viewname = element.View;

                    newTask = SynchronizeErfs(localAuthority, localAuthorityCode, viewname, GisObjectState.New);
                    syncTasks.Add(newTask);

                    updateTask = SynchronizeErfs(localAuthority, localAuthorityCode, viewname, GisObjectState.Edited);
                    syncTasks.Add(updateTask);

                    try
                    {
                        Task.WaitAll(syncTasks.ToArray());
                    }
                    catch (AggregateException e)
                    {
                        Console.WriteLine("\nThe following exceptions have been thrown by WaitAll(): THIS WAS EXPECTED");
                        for (int j = 0; j < e.InnerExceptions.Count; j++)
                        {
                            Console.WriteLine("\n---------------------------------\n{0}", e.InnerExceptions[j].ToString());
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine("\n---------------------------------\n{0}", x.InnerException.ToString());
                    }
                }

                foreach (LocalAuthorityTableElement element in Tables)
                {
                    localAuthority = element.LocalAuthority;
                    localAuthorityCode = element.Code;
                    tablename = element.Table;

                    deleteTask = SynchronizeDelErfs(localAuthority, localAuthorityCode, tablename, GisObjectState.Deleted);
                    syncTasks.Add(deleteTask);

                    try
                    {
                        Task.WaitAll(syncTasks.ToArray());
                    }
                    catch (AggregateException e)
                    {
                        Console.WriteLine("\nThe following exceptions have been thrown by WaitAll(): (ON DELETION)");
                        for (int j = 0; j < e.InnerExceptions.Count; j++)
                        {
                            Console.WriteLine("\n---------------------------\n{0}", e.InnerExceptions[j].ToString());
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine("\n---------------------------------\n{0}", x.InnerException.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public enum GisObjectState
    {
        New,
        Synchonized,
        Edited,
        Deleted
    }
}