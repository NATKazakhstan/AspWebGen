using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nat.Web.Tools;

namespace Nat.Web.Controls.GenerationClasses
{
    public class BaseRow : IRow, IDataRow
    {
        #region IRow Members

        public virtual long id 
        {
            get { return Convert.ToInt64(Value); }
            set { Value = value.ToString(); }
        }

        public virtual string nameRu { get; set; }

        public virtual string nameKz { get; set; }

        public virtual string[] GetAdditionalValues(SelectParameters selectParameters)
        {
            return AdditionalValues ?? new string[0];
        }

        public virtual object[] GetAdditionalValues()
        {
            return new object[0];
        }

        protected internal virtual string[] AdditionalValues { get; set; }

        #endregion

        #region IDataRow Members

        public string Value { get; set; }
        
        public string Name
        {
            get { return LocalizationHelper.IsCultureKZ ? nameKz : nameRu; }
        }

        public virtual bool CanAddChild { get; set; }
        public virtual bool CanEdit { get; set; }
        public virtual bool CanDelete { get; set; }

        #endregion

        /// <summary>
        /// Используется для хранения информации об изменениях, при редактировании таблицы на форме Edit.
        /// т.е. использлвание AdditionalField_Grid.
        /// </summary>
        public bool Dirty { get; set; }

        public virtual int CountChildsData()
        {
            return 0;
        }

        public virtual void SetValue(string strValue)
        {
        }

        public bool IsInArchive { get; set; }
    }
}
