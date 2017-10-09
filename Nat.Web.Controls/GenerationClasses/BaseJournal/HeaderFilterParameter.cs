namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;
    using Nat.Web.Controls.Properties;

    public abstract class HeaderFilterParameter<THeaderTable> : BaseFilterParameter<THeaderTable>
        where THeaderTable : class
    {
        protected HeaderFilterParameter(BaseJournalCrossTable crossTable)
        {
            CrossTable = crossTable;
        }
    }

    public class HeaderFilterParameter<THeaderTable, TField> : HeaderFilterParameter<THeaderTable>
        where THeaderTable : class
        where TField : struct
    {
        public HeaderFilterParameter(Expression<Func<THeaderTable, TField?>> valueExpression, BaseJournalCrossTable crossTable)
            : base(crossTable)
        {
            ValueExpression = valueExpression;
        }

        public Expression<Func<THeaderTable, TField?>> ValueExpression { get; set; }

        public override Expression OValueExpression
        {
            get { return ValueExpression; }
        }

        protected override Type FieldType
        {
            get
            {
                return typeof(TField);
            }
        }
    }

    public class HeaderFilterParameter<THeaderTable, TField, TCrossTable, TDataContext> : HeaderFilterParameter<THeaderTable, TField>
        where THeaderTable : class
        where TField : struct
        where TCrossTable : class
        where TDataContext : DataContext
    {
        public HeaderFilterParameter(Expression<Func<THeaderTable, TField?>> valueExpression, Expression<Func<TCrossTable, TDataContext, IQueryable<THeaderTable>>> getHeaderTableByCrossTable, BaseJournalCrossTable crossTable)
            : base(valueExpression, crossTable)
        {
            CorssDataToHeader = getHeaderTableByCrossTable;
            if (crossTable != null && crossTable.CrossTableType != typeof(TCrossTable)) 
                throw new Exception("Несоответствие типов кросс таблицы");
        }
    }

    public class TreeHeaderFilterParameter<THeaderTable> : HeaderFilterParameter<THeaderTable>
        where THeaderTable : class
    {
        private readonly bool isStartLevel;
        private QueryParameters valuesForQParams;

        public TreeHeaderFilterParameter(bool isStartLevel) : base(null)
        {
            this.isStartLevel = isStartLevel;
            if (isStartLevel)
            {
                FilterName = TreeStartLevelFilterName + typeof(THeaderTable).Name;
                Header = Resources.SStartHeaderFromLevel;
                Type = FilterHtmlGenerator.FilterType.Reference;
                Mandatory = true;
                StartHeaderValues = new List<string>(1);
            }
            else
            {
                FilterName = TreeMaxLevelFilterName + typeof(THeaderTable).Name;
                Header = Resources.SMaxRecursionInHeader;
                Type = FilterHtmlGenerator.FilterType.Numeric;
                Mandatory = true;
                AllowAddFilter = false;
            }
        }

        public bool IsStartLevel
        {
            get { return isStartLevel; }
        }

        public bool IsMaxRecursion
        {
            get { return !isStartLevel; }
        }

        public int MaxRecursion { get; protected set; }

        public List<string> StartHeaderValues { get; protected set; }
        
        protected override IQueryable OnFilter(IQueryable query, Enum filtertype, string value1, string value2)
        {
            if (IsStartLevel)
            {
                StartHeaderValues.Clear();
                if (!string.IsNullOrEmpty(value1))
                    StartHeaderValues.Add(value1);
            }
            else
                MaxRecursion = Convert.ToInt32(value1);
            return query;
        }

        protected internal override Expression OnFilter(Enum filtertype, FilterItem filterItem, QueryParameters queryParameters)
        {
            if (IsStartLevel)
            {
                if (valuesForQParams != queryParameters || queryParameters == null)
                {
                    StartHeaderValues.Clear();
                    valuesForQParams = queryParameters;
                }

                if (!string.IsNullOrEmpty(filterItem.Value1))
                    StartHeaderValues.Add(filterItem.Value1);
            }
            else
                MaxRecursion = Convert.ToInt32(filterItem.Value1);
            return null;
        }
    }

    public class TreeHeaderFilterParameter<THeaderTable, TField, TCrossTable> : HeaderFilterParameter<THeaderTable, TField>
        where THeaderTable : class
        where TField : struct
        where TCrossTable : class
    {
        public TreeHeaderFilterParameter(Expression<Func<TCrossTable, TField?>> fieldReferenceFromCrossToHeader, BaseJournalCrossTable crossTable)
            : base(null, crossTable)
        {
            FilterName = "TreeSelectedData" + typeof(THeaderTable).Name;
            FieldReferenceFromCrossToHeader = fieldReferenceFromCrossToHeader;
            if (crossTable != null && crossTable.CrossTableType != typeof(TCrossTable))
                throw new Exception("Не соответствие типов кросс таблицы");
        }

        protected override IQueryable OnFilter(IQueryable query, Enum filtertype, string value1, string value2)
        {
            return query;
        }
    }
}
