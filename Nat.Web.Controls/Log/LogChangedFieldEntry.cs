/*
 * Created by : Daniil Kovalev
 * Created    : 14.02.2008
 */

using System;

namespace Nat.Web.Controls
{
    [Serializable]
    public class LogChangedFieldEntry
    {
        #region Fields

        private String fieldName;
        private String fieldNameKz;
        private bool isDetailsViewVisible;
        private Object newValue;
        private Object oldValue;
        private String rowEntity;
        private String tableName;
        private String tableNameKz;

        #endregion


        #region Constructors

        public LogChangedFieldEntry(String tableName, String rowEntity, String fieldName, Object oldValue, Object newValue)
        {
            this.tableName = tableName;
            this.rowEntity = rowEntity;
            this.fieldName = fieldName;
            this.newValue = newValue;
            this.oldValue = oldValue;
        }

        public LogChangedFieldEntry(String tableName, String tableNameKz, String rowEntity, String fieldName, String fieldNameKz, bool isDetailsViewVisible, Object oldValue, Object newValue)
        {
            this.tableName = tableName;
            this.tableNameKz = tableNameKz;
            this.rowEntity = rowEntity;
            this.fieldName = fieldName;
            this.fieldNameKz = fieldNameKz;
            this.isDetailsViewVisible = isDetailsViewVisible;
            this.newValue = newValue;
            this.oldValue = oldValue;
        }

        #endregion


        #region Properties


        public String TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        public String TableNameKz
        {
            get { return tableNameKz; }
            set { tableNameKz = value; }
        }

        public String RowEntity
        {
            get { return rowEntity; }
            set { rowEntity = value; }
        }

        public String FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public String FieldNameKz
        {
            get { return fieldNameKz; }
            set { fieldNameKz = value; }
        }

        public bool IsDetailsViewVisible
        {
            get { return isDetailsViewVisible; }
            set { isDetailsViewVisible = value; }
        }

        public Object OldValue
        {
            get { return oldValue; }
            set { oldValue = value; }
        }

        public Object NewValue
        {
            get { return newValue; }
            set { newValue = value; }
        }

        #endregion
    }
}