using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Data
{
    public struct CustomParameter
    {
        public string ParameterName;
        public object ParameterValue;
        public DbType ParameterDbType;
        public ParameterDirection ParamDirection;
        public CustomParameter(string _pname, object _pvalue, Type _type) : this(_pname, _pvalue, _type, ParameterDirection.Input) { }
        public CustomParameter(string _pname, object _pvalue, Type _type, ParameterDirection direction) : this(_pname, _pvalue, TypeHelper.DbTypeMap[_type], direction) { }
        public CustomParameter(string _pname, object _pvalue, DbType _dbType) : this(_pname, _pvalue, _dbType, ParameterDirection.Input) { }
        public CustomParameter(string _pname, object _pvalue, DbType _dbType, ParameterDirection _direction)
        {
            ParameterName = _pname;
            ParameterValue = _pvalue;
            ParameterDbType = _dbType;
            ParamDirection = _direction;
        }
    }
}