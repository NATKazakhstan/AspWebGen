namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Linq.Expressions;

    public class BaseFilterParameterString<TTable> : BaseFilterParameter<TTable>
        where TTable : class
    {
        public BaseFilterParameterString()
        {
        }

        public BaseFilterParameterString(Expression<Func<TTable, string>> valueExpression)
        {
            ValueExpression = valueExpression;
        }

        public BaseFilterParameterString(Expression<Func<TTable, string>> valueExpression, Expression<Func<TTable, string>> valueTextExpressionRu, Expression<Func<TTable, string>> valueTextExpressionKz)
        {
            ValueExpression = valueExpression;
            ValueTextExpressionRu = valueTextExpressionRu;
            ValueTextExpressionKz = valueTextExpressionKz;
        }

        public Expression<Func<TTable, string>> ValueExpression { get; set; }

        public Expression<Func<TTable, string>> ValueExpressionSecond { get; set; }

        public Expression<Func<TTable, string>> ValueTextExpressionRu { get; set; }

        public Expression<Func<TTable, string>> ValueTextExpressionKz { get; set; }

        public override Expression OValueExpression
        {
            get { return ValueExpression; }
        }

        public override Expression OValueExpressionSecond
        {
            get { return ValueExpressionSecond; }
        }

        public override Expression OTextValueExpressionKz
        {
            get
            {
                return ValueTextExpressionKz ?? base.OTextValueExpressionKz;
            }
        }

        public override Expression OTextValueExpressionRu
        {
            get
            {
                return ValueTextExpressionRu ?? base.OTextValueExpressionRu;
            }
        }

        protected override Type FieldType
        {
            get
            {
                return typeof(string);
            }
        }
    }
}