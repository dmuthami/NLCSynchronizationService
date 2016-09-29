using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Data
{
    public class DbExecutor
    {
        private Database database;
        public DbExecutor()
        {
            database = new Database();
        }

        #region Main Execute

        public virtual TResult Execute<TResult>(string commandText, CommandType commandType, DbParameter[] dbParameters, Func<DbCommand, TResult> executor)
        {
            return Execute<TResult>(commandText, commandType, dbParameters, database.DbSettings.EnableTransactions, executor);
        }
        public virtual TResult Execute<TResult>(string commandText, DbParameter[] dbParameters, Func<DbCommand, TResult> executor)
        {
            return Execute<TResult>(commandText, CommandType.Text, dbParameters, database.DbSettings.EnableTransactions, executor);
        }
        public virtual TResult Execute<TResult>(string commandText, CommandType commandType, DbParameter[] dbParameters, bool useTransaction, Func<DbCommand, TResult> executor)
        {
            TResult result = default(TResult);
            using (DbConnection connection = database.DbConnection)
            {
                if (commandText.Contains("["))
                {
                    commandText = commandText.Replace("[", @"""");
                    commandText = commandText.Replace("]", @"""");
                }
                connection.Open();
                DbCommand command = connection.CreateCommand();
                DbTransaction transaction = useTransaction ? connection.BeginTransaction() : null;
                command.Connection = connection;
                command.CommandType = commandType;
                command.CommandText = commandText;
                command.Transaction = transaction;
                if (dbParameters != null && dbParameters.Length > 0)
                {
                    command.Parameters.AddRange(dbParameters);
                }

                result = executor(command);

                // Commit transaction if enabled.
                if (useTransaction) transaction.Commit();
            }
            return result;
        }

        #endregion

        #region Parameters
        public DbParameter BuildParameter(string parameterName, DbType dbType, object value, ParameterDirection direction)
        {
            DbParameter parameter = database.DbParameter;
            parameter.ParameterName = database.DbSettings.ParameterPrefix + parameterName;
            parameter.DbType = dbType;
            parameter.Value = value;
            parameter.Direction = direction;
            return parameter;
        }

        public void AddParameters(IList<DbParameter> parameters, string paramName, DbType dbType, object val)
        {
            parameters.Add(BuildParameter(paramName, dbType, val, ParameterDirection.Input));
        }

        public void AddParameters(IList<DbParameter> parameters, string paramName, DbType dbType, object val, ParameterDirection direction)
        {
            parameters.Add(BuildParameter(paramName, dbType, val, direction));

        }

        public IEnumerable<DbParameter> CreateParameters(IDictionary<string, DbType> parameterNames, IEnumerable<object> values)
        {

            if (parameterNames.Count() != values.Count()) return null;
            List<DbParameter> parameters = new List<DbParameter>();
            int i = 0;
            foreach (string key in parameterNames.Keys)
            {
                AddParameters(parameters, key, parameterNames[key], values.ElementAt(i++));
            }
            return parameters;
        }
        public IEnumerable<DbParameter> CreateParameters(IEnumerable<CustomParameter> insParameters)
        {
            if (null == insParameters || insParameters.Count() < 1) return null;
            List<DbParameter> parameters = new List<DbParameter>();
            foreach (CustomParameter param in insParameters)
            {
                AddParameters(parameters, param.ParameterName, param.ParameterDbType, param.ParameterValue, param.ParamDirection);
            }
            return parameters;
        }
        #endregion

        #region ExecuteReader
        public void ExecuteReader(string commandText, Action<IDataReader> action)
        {
            ExecuteReader(commandText, action, null);
        }
        public void ExecuteReader(string commandText, CommandType commandType, Action<IDataReader> action)
        {
            ExecuteReader(commandText, commandType, action, null);
        }
        public void ExecuteReader(string commandText, IEnumerable<CustomParameter> parameters, Action<IDataReader> action)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            ExecuteReader(commandText, action, insparams == null ? null : insparams.ToArray());
        }
        public void ExecuteReader(string commandText, CommandType commandType, IEnumerable<CustomParameter> parameters, Action<IDataReader> action)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            ExecuteReader(commandText, commandType, action, insparams == null ? null : insparams.ToArray());
        }
        public void ExecuteReader(string commandText, Action<IDataReader> action, params DbParameter[] dbParameters)
        {
            ExecuteReader(commandText, CommandType.Text, action, dbParameters);
        }
        public void ExecuteReader(string commandText, CommandType commandType, Action<IDataReader> action, params DbParameter[] dbParameters)
        {
            Execute<bool>(commandText, commandType, dbParameters, command =>
            {
                IDataReader reader = command.ExecuteReader();
                action(reader);
                reader.Close();
                return true;
            });
        }
        #endregion

        #region ExecuteNonQuery
        public int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(commandText, null as DbParameter[]);
        }
        public int ExecuteNonQuery(string commandText, CommandType commandType)
        {
            return ExecuteNonQuery(commandText, commandType, null as DbParameter[]);
        }
        public int ExecuteNonQuery(string commandText, params DbParameter[] dbParameters)
        {
            return ExecuteNonQuery(commandText, CommandType.Text, dbParameters);
        }
        public int ExecuteNonQuery(string commandText, IEnumerable<CustomParameter> parameters)
        {
            return ExecuteNonQuery(commandText, CommandType.Text, parameters);
        }
        public int ExecuteNonQuery(string commandText, CommandType commandType, params DbParameter[] dbParameters)
        {
            return Execute<int>(commandText, commandType, dbParameters, command => { return command.ExecuteNonQuery(); });
        }
        public int ExecuteNonQuery(string commandText, CommandType commandType, IDictionary<string, DbType> parameterNames, IEnumerable<object> values)
        {
            IEnumerable<DbParameter> parameters = CreateParameters(parameterNames, values);
            return ExecuteNonQuery(commandText, commandType, parameters.ToArray());
        }
        public int ExecuteNonQuery(string commandText, CommandType commandType, IEnumerable<CustomParameter> insParameters)
        {
            IEnumerable<DbParameter> parameters = CreateParameters(insParameters);
            return ExecuteNonQuery(commandText, commandType, parameters.ToArray());
        }
        #endregion

        #region ExecuteNonQueryGetOutput
        public Dictionary<string, object> ExecuteNonQueryGetOutput(string commandText, params DbParameter[] dbParameters)
        {
            return ExecuteNonQueryGetOutput(commandText, CommandType.Text, dbParameters);
        }
        public Dictionary<string, object> ExecuteNonQueryGetOutput(string commandText, IEnumerable<CustomParameter> parameters)
        {
            return ExecuteNonQueryGetOutput(commandText, CommandType.Text, parameters);
        }
        public Dictionary<string, object> ExecuteNonQueryGetOutput(string commandText, CommandType commandType, IEnumerable<CustomParameter> insParameters)
        {
            IEnumerable<DbParameter> parameters = CreateParameters(insParameters);
            return ExecuteNonQueryGetOutput(commandText, commandType, parameters.ToArray());
        }
        public Dictionary<string, object> ExecuteNonQueryGetOutput(string commandText, CommandType commandType, params DbParameter[] dbParameters)
        {
            return Execute<Dictionary<string, object>>(commandText, commandType, dbParameters,
                command =>
                {
                    command.ExecuteNonQuery();
                    Dictionary<string, object> outputParams = new Dictionary<string, object>();
                    foreach (DbParameter param in command.Parameters)
                    {
                        if (param.Direction == ParameterDirection.Output)
                            outputParams[param.ParameterName] = param.Value;
                    }
                    return outputParams;
                });

        }
        #endregion

        #region ExecuteScalar
        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(commandText, null as DbParameter[]);
        }
        public object ExecuteScalar(string commandText, CommandType commandType)
        {
            return ExecuteScalar(commandText, commandType, null as DbParameter[]);
        }
        public object ExecuteScalar(string commandText, params DbParameter[] dbParameters)
        {
            return ExecuteScalar(commandText, CommandType.Text, dbParameters);
        }
        public object ExecuteScalar(string commandText, IEnumerable<CustomParameter> parameters)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            return ExecuteScalar(commandText, insparams == null ? null : insparams.ToArray());
        }
        public object ExecuteScalar(string commandText, CommandType commandType, IEnumerable<CustomParameter> parameters)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            return ExecuteScalar(commandText, commandType, insparams == null ? null : insparams.ToArray());
        }
        public object ExecuteScalar(string commandText, CommandType commandType, params DbParameter[] dbParameters)
        {
            return Execute<object>(commandText, commandType, dbParameters, command => command.ExecuteScalar());
        }
        #endregion

        #region ExecuteDataTable
        public DataTable ExecuteDataTable(string commandText)
        {
            return ExecuteDataTable(commandText, null as DbParameter[]);
        }
        public DataTable ExecuteDataTable(string commandText, CommandType commandType)
        {
            return ExecuteDataTable(commandText, commandType, null as DbParameter[]);
        }
        public DataTable ExecuteDataTable(string commandText, params DbParameter[] dbParameters)
        {
            return ExecuteDataTable(commandText, CommandType.Text, dbParameters);
        }
        public DataTable ExecuteDataTable(string commandText, IEnumerable<CustomParameter> parameters)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            return ExecuteDataTable(commandText, insparams == null ? null : insparams.ToArray());
        }
        public DataTable ExecuteDataTable(string commandText, CommandType commandType, IEnumerable<CustomParameter> parameters)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            return ExecuteDataTable(commandText, commandType, insparams == null ? null : insparams.ToArray());
        }
        public DataTable ExecuteDataTable(string commandText, CommandType commandType, params DbParameter[] dbParameters)
        {
            return Execute<DataTable>(commandText, commandType, dbParameters,
                (command) =>
                {
                    IDataReader reader = command.ExecuteReader();
                    DataTable table = new DataTable();
                    table.Load(reader);
                    return table;
                });
        }
        #endregion

        #region ExecuteDataSet
        public DataSet ExecuteDataSet(string commandText)
        {
            return ExecuteDataSet(commandText, null as DbParameter[]);
        }
        public DataSet ExecuteDataSet(string commandText, CommandType commandType)
        {
            return ExecuteDataSet(commandText, commandType, null as DbParameter[]);
        }
        public DataSet ExecuteDataSet(string commandText, params DbParameter[] dbParameters)
        {
            return ExecuteDataSet(commandText, CommandType.Text, dbParameters);
        }
        public DataSet ExecuteDataSet(string commandText, IEnumerable<CustomParameter> parameters)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            return ExecuteDataSet(commandText, insparams == null ? null : insparams.ToArray());
        }
        public DataSet ExecuteDataSet(string commandText, CommandType commandType, IEnumerable<CustomParameter> parameters)
        {
            IEnumerable<DbParameter> insparams = CreateParameters(parameters);
            return ExecuteDataSet(commandText, commandType, insparams == null ? null : insparams.ToArray());
        }
        public DataSet ExecuteDataSet(string commandText, CommandType commandType, params DbParameter[] dbParameters)
        {
            return Execute<DataSet>(commandText, commandType, dbParameters,
                (command) =>
                {
                    DbDataAdapter adapter = database.DbDataAdapter;
                    adapter.SelectCommand = command;
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);
                    return dataSet;
                });
        }
        #endregion

    }
}