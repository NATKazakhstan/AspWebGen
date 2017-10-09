/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 октября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Web.Compilation;

namespace Nat.Web.Controls
{
    public static class LinqToSqlHelper
    {
        private readonly static Dictionary<Key, string> parentEntities = new Dictionary<Key, string>();

        public static string GetParentEntity(string fieldName, string tableName, Type type)
        {
            var key = new Key {TableName = tableName, Type = type};
            if (!parentEntities.ContainsKey(key))
            {
                var tableTypeName = type.Namespace + "." + tableName;
                var tableType = BuildManager.GetType(tableTypeName, true, false);
                foreach (var property in tableType.GetProperties())
                {
                    var attributes = property.GetCustomAttributes(typeof(AssociationAttribute), true);
                    if(attributes.Length == 1)
                    {
                        if(((AssociationAttribute)attributes[0]).ThisKey == fieldName)
                        {
                            parentEntities[key] = property.Name;
                        }
                    }
                }
            }
            return parentEntities[key];
        }

        public static string GetChildEntity(string fieldName, string tableName, Type type)
        {
            var key = new Key {TableName = tableName, Type = type};
            if (!parentEntities.ContainsKey(key))
            {
                var tableTypeName = type.Namespace + "." + tableName;
                var tableType = BuildManager.GetType(tableTypeName, true, false);
                foreach (var property in tableType.GetProperties())
                {
                    var attributes = property.GetCustomAttributes(typeof(AssociationAttribute), true);
                    if(attributes.Length == 1)
                    {
                        if(((AssociationAttribute)attributes[0]).ThisKey == fieldName)
                        {
                            parentEntities[key] = property.Name;
                        }
                    }

                }
            }
            return parentEntities[key];
        }

        private struct Key
        {
            public Type Type;
            public string TableName;
        }
    }
}