using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Nat.Controls.DataGridViewTools;
using Nat.Tools.Filtering;
using Nat.Tools.Specific;
using Nat.Tools.System;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Tools
{
    using Nat.Web.Tools.Security;

    [Serializable]
    public class StorageValues
    {
        private readonly Hashtable _values = new Hashtable();
        private List<Hashtable> _valuesCircle = new List<Hashtable>();

        public StorageValues() {}

        public StorageValues(Hashtable values, List<Hashtable> valuesCircle)
        {
            _values = values;
            _valuesCircle = valuesCircle;
        }

        public int CountListValues
        {
            get { return _valuesCircle.Count; }
            set
            {
                _valuesCircle = new List<Hashtable>(new Hashtable[value]);
                for (int i = 0; i < _valuesCircle.Count; i++)
                {
                    _valuesCircle[i] = new Hashtable();
                }
            }
        }

        public void AddStorage(ColumnFilterStorage storage)
        {
            _values.Add(storage.Name, new Data(storage.Values, storage.FilterType));
        }

        public void AddStorage(ColumnFilterStorage storage, string[] textValues)
        {
            var existValue = _values.ContainsKey(storage.Name) ? (Data)_values[storage.Name] : null;
            if (existValue == null
                || (storage.Values != null && storage.Values.Any(r => r != null)
                    && (storage.FilterType == ColumnFilterType.In
                        || storage.FilterType != ColumnFilterType.In && storage.FilterType != ColumnFilterType.None)))
            {
                _values[storage.Name] = new Data(storage.Values, storage.FilterType, textValues);
            }
        }
        
        public void SetStorageValues(string name, ColumnFilterType filterType, params object[] values)
        {
            Data data = _values[name] as Data;
            if (data != null)
            {
                data.values = values;
                data.filterType = filterType;
            }
        }

        public void SetStorageValues(string name, params object[] values)
        {
            Data data = _values[name] as Data;
            if (data != null)
                data.values = values;
        }

        public void SetStorageTextValues(string name, string[] textValues)
        {
            Data data = _values[name] as Data;
            if (data != null)
                data.textValues = textValues;
        }

        public object[] GetStorageValues(string name)
        {
            Data data = _values[name] as Data;
            if (data != null)
                return data.values;
            return null;
        }

        public object[] GetCircleStorageValues(string name, int index)
        {
            Data data = _valuesCircle[index][name] as Data;
            if (data != null)
                return data.values;
            return null;
        }

        public string[] GetStorageTextValues(string name)
        {
            Data data = _values[name] as Data;
            if (data != null)
                return data.textValues;
            return null;
        }

        public string[] GetCircleStorageTextValues(string name, int index)
        {
            Data data = _valuesCircle[index][name] as Data;
            if (data != null)
                return data.textValues;
            return null;
        }

        public void SetCircleStorageValues(string name, int index, params object[] values)
        {
            Data data = _valuesCircle[index][name] as Data;
            if (data != null)
                data.values = values;
        }

        public void SetCircleStorageValues(string name, int index, string[] textValues)
        {
            Data data = _valuesCircle[index][name] as Data;
            if (data != null)
                data.textValues = textValues;
        }

        public ColumnFilterType? GetStorageFilterType(string name)
        {
            Data data = _values[name] as Data;
            if (data != null)
                return data.filterType;
            return null;
        }

        public ColumnFilterType? GetCircleStorageFilterType(string name, int index)
        {
            Data data = _valuesCircle[index][name] as Data;
            if (data != null)
                return data.filterType;
            return null;
        }

        public void SetStorage(ColumnFilterStorage storage)
        {
            Data data = _values[storage.Name] as Data;
            if(data != null)
            {
                Boolean filterValid = true;

                if (!EnumHelper.Contains(data.filterType, storage.AvailableFilters))
                {
                    filterValid = false;
                }

                if (filterValid)
                {
                    for (int i = 0; i != data.values.Length; i++)
                    {
                        if (data.values[i] == null || data.values[i].GetType() != storage.DataType)
                        {
                            filterValid = false;
                            break;
                        }
                    }
                }

                Debug.Write(filterValid, "User filter configuration not valid");

                if (filterValid)
                {
                    storage.FilterType = data.filterType;
                    storage.Values = data.values;
                }
            }
        }

        public void AddListStorage(ColumnFilterStorage storage, int index, string[] textValues)
        {
            Hashtable values;
            if (_valuesCircle.Count == index)
            {
                values = new Hashtable();
                _valuesCircle.Add(values);
            }
            else
                values = _valuesCircle[index];
            values.Add(storage.Name, new Data(storage.Values, storage.FilterType, textValues));
        }

        public void SetListStorage(ColumnFilterStorage storage, int index)
        {
            Data data = _valuesCircle[index][storage.Name] as Data;
            if(data != null)
            {
                if (data.values == null)
                    storage.Values = new object[0];
                else
                    storage.Values = data.values;
                storage.FilterType = data.filterType;
            }
        }

        public IEnumerable<string> GetStorageNames()
        {
            return _values.Keys.Cast<string>();
        }

        public IEnumerable<string> GetCircleStorageNames(int index)
        {
            return _valuesCircle[index].Keys.Cast<string>();
        }

//        public Hashtable Values
//        {
//            get { return _values; }
//            set { _values = value; }
//        }
//
//        public List<Hashtable> ValuesCircle
//        {
//            get { return _valuesCircle; }
//            set { _valuesCircle = value; }
//        }
//
        public static StorageValues GetStorageValues(string key, byte[] sid)
        {
            WebInitializer.Initialize();
            return GetStorageValues(SpecificInstances.DbFactory.CreateConnection(), SpecificInstances.DbConstants.SqlParameterPrefix, key, sid);
        }

        public static void SetStorageValues(string key, byte[] sid, StorageValues values)
        {
            WebInitializer.Initialize();
            SetStorageValues(SpecificInstances.DbFactory.CreateConnection(), SpecificInstances.DbConstants.SqlParameterPrefix, key, sid, values);
        }

        public static StorageValues GetStorageValues(DbConnection connection, string parameterPrefix, string key, byte[] sid)
        {
            ConnectionState state = connection.State;
            if((state & ConnectionState.Open) != ConnectionState.Open)
                connection.Open();

            try
            {
                DbCommand command = connection.CreateCommand();
                command.CommandText = "dbo.SYS_getFilterValues";
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = connection;

                DbParameter parameter;
                parameter = command.CreateParameter();
                parameter.ParameterName = parameterPrefix + "key";
                parameter.DbType = DbType.String;
                parameter.Value = key;
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = parameterPrefix + "BinarySid";
                parameter.DbType = DbType.Binary;
                parameter.Value = sid;
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = parameterPrefix + "sid";
                parameter.DbType = DbType.String;
                parameter.Value = User.GetSID();
                command.Parameters.Add(parameter);

                DbDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult);
                if(!reader.Read()) return null;
                object value = reader.GetValue(0);
                byte[] buffer = null;
                if(value != DBNull.Value) buffer = (byte[])value;
                if(buffer == null || buffer.Length == 0) return null;
                return Deserialize(buffer);
            }
            finally
            {
                if((state & ConnectionState.Open) != ConnectionState.Open)
                    connection.Close();
            }
        }

        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(stream, this);
                return stream.GetBuffer();
            }
        }

        public static StorageValues Deserialize(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var bf = new BinaryFormatter();
                try
                {
                    return (StorageValues)bf.Deserialize(stream);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static void SetStorageValues(DbConnection connection, string parameterPrefix, string key, byte[] sid, StorageValues values)
        {
            ConnectionState state = connection.State;
            if((state & ConnectionState.Open) != ConnectionState.Open)
                connection.Open();

            try
            {
                byte[] buffer = values == null ? new byte[0] : values.Serialize();

                DbCommand command = connection.CreateCommand();
                command.CommandText = "dbo.SYS_setFilterValues";
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = connection;

                DbParameter parameter;
                parameter = command.CreateParameter();
                parameter.ParameterName = parameterPrefix + "key";
                parameter.DbType = DbType.String;
                parameter.Value = key;
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = parameterPrefix + "BinarySid";
                parameter.DbType = DbType.Binary;
                parameter.Value = sid;
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = parameterPrefix + "UserSid";
                parameter.DbType = DbType.String;
                parameter.Value = User.GetSID();
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = parameterPrefix + "values";
                parameter.DbType = DbType.Binary;
                parameter.Value = buffer;
                parameter.IsNullable = true;
                command.Parameters.Add(parameter);

                command.ExecuteScalar();
            }
            finally
            {
                if((state & ConnectionState.Open) != ConnectionState.Open)
                    connection.Close();
            }
        }

        [Serializable]
        private class Data
        {
            public Data(object[] values, ColumnFilterType filterType)
            {
                this.values = values;
                this.filterType = filterType;
            }

            public Data(object[] values, ColumnFilterType filterType, string[] textValues)
            {
                this.values = values;
                this.filterType = filterType;
                this.textValues = textValues;
            }

            public object[] values;
            public string[] textValues;
            public ColumnFilterType filterType;
        }
    }
}