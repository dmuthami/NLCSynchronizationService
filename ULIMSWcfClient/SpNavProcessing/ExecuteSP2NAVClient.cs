﻿using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ULIMSWcfClient.Configuration_Local;

namespace ULIMSWcfClient.SpNavProcessing
{
    public class ExecuteSP2NAVClient
    {
        private static void LogError(Exception ex)
        {
            Console.WriteLine("Exception thrown");

            string source = "ULIMS NAV Service";
            string log = "ULIMS.Services.NAV";
            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, log);

            EventLog.WriteEntry(source, ex.Message, EventLogEntryType.Error);
        }

        public bool DoSpNavProcess()
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
        public static async void DoSynchronize()
        {
            try
            {
                await Synchronize();
            }
            catch (AggregateException ae)
            {
                foreach (Exception ex in ae.Flatten().InnerExceptions)
                {
                    Console.Write(ex);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            finally
            {
                Console.WriteLine("Finished!!");
                //Console.ReadLine();
            }
        }
        private static async Task Synchronize()
        {
            try
            {
                List<Task> syncTasks = new List<Task>();
                Task task;
                string localAuthority;
                string localAuthorityCode;

                foreach (LocalAuthorityElement _element in LocalAuthorities)
                {
                    localAuthority = _element.Name;
                    localAuthorityCode = _element.Code;

                    task = Synchronize(localAuthority, localAuthorityCode);
                    syncTasks.Add(task);
                }

                Task.WaitAll(syncTasks.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static async Task Synchronize(string localAuthority, string localauthcode)
        {
            Console.WriteLine("Synchronize function");
            try
            {
                var syncErf = new SyncErf(localAuthority, localauthcode);
                var syncAppPurchase = new SyncApplicationForPurchase(localAuthority, localauthcode);
                var syncValuation = new SyncValuation(localAuthority, localauthcode);
                var syncValuationP = new SyncValuationPeriod(localAuthority, localauthcode);

                List<Task> tasks = new List<Task>()
                {
                    syncErf.Synchronize(),
                    syncAppPurchase.Synchronize(),
                    syncValuation.Synchronize(),
                    syncValuationP.Synchronize()
                };

                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static List<LocalAuthorityElement> _localAuthorities = new List<LocalAuthorityElement>();
        public static List<LocalAuthorityElement> LocalAuthorities
        {
            get
            {
                if (_localAuthorities == null || _localAuthorities.Count == 0)
                {
                    _localAuthorities = new List<LocalAuthorityElement>();
                    LocalAuthoritySection section = (LocalAuthoritySection)ConfigurationManager.GetSection("LocalAuthorities");
                    if (section != null)
                    {
                        foreach (LocalAuthorityElement element in section.LocalAuthoritiesKeys)
                        {
                            _localAuthorities.Add(element);
                        }
                    }
                }
                return _localAuthorities;
            }
        }
        private static void GetDBLogger(string strConnectionString)
        {
            StringBuilder sb = new StringBuilder();
            InstallationContext installationContext = new InstallationContext();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ConnectionString = strConnectionString;
            string strDatabase = builder.InitialCatalog;

            NLog.Targets.DatabaseTarget targetDB = new NLog.Targets.DatabaseTarget();

            targetDB.Name = "db";
            targetDB.ConnectionString = strConnectionString;

            NLog.Targets.DatabaseParameterInfo paramDB;

            paramDB = new NLog.Targets.DatabaseParameterInfo();
            paramDB.Name = string.Format("@Message");
            paramDB.Layout = string.Format("${{message}}");
            targetDB.Parameters.Add(paramDB);
            targetDB.CommandText = string.Format("INSERT INTO Logs(Message) VALUES (@message);");

            // Keep original configuration
            LoggingConfiguration config = LogManager.Configuration;
            if (config == null)
                config = new LoggingConfiguration();

            config.AddTarget(targetDB.Name, targetDB);

            LoggingRule rule = new LoggingRule("*", LogLevel.Debug, targetDB);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;

            SqlConnectionStringBuilder builder2 = new SqlConnectionStringBuilder();
            builder2.ConnectionString = strConnectionString;
            builder2.InitialCatalog = "master";

            // we have to connect to master in order to do the install because the DB may not exist
            targetDB.InstallConnectionString = builder2.ConnectionString;

            sb.AppendLine(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = N'{0}')", strDatabase));
            sb.AppendLine(string.Format("CREATE DATABASE {0}", strDatabase));

            DatabaseCommandInfo createDBCommand = new DatabaseCommandInfo();
            createDBCommand.Text = sb.ToString();
            createDBCommand.CommandType = System.Data.CommandType.Text;
            targetDB.InstallDdlCommands.Add(createDBCommand);

            // create the database if it does not exist
            targetDB.Install(installationContext);

            targetDB.InstallDdlCommands.Clear();
            sb.Clear();
            sb.AppendLine("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Logs')");
            sb.AppendLine("RETURN");
            sb.AppendLine("");
            sb.AppendLine("CREATE TABLE [dbo].[Logs](");
            sb.AppendLine("[LogId] [int] IDENTITY(1,1) NOT NULL,");
            sb.AppendLine("[Message] [nvarchar](max) NULL,");
            sb.AppendLine(" CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ");
            sb.AppendLine("(");
            sb.AppendLine("[LogId] ASC");
            sb.AppendLine(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
            sb.AppendLine(") ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

            DatabaseCommandInfo createTableCommand = new DatabaseCommandInfo();
            createTableCommand.Text = sb.ToString();
            createTableCommand.CommandType = System.Data.CommandType.Text;
            targetDB.InstallDdlCommands.Add(createTableCommand);

            // we can now connect to the target DB
            targetDB.InstallConnectionString = strConnectionString;

            // create the table if it does not exist
            targetDB.Install(installationContext);
        }
    }
}