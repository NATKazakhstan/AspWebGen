using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public interface IParentDataSourceViews
    {
        IEnumerable<ParentDataSourceViewInfo> GetParentCollection();

        void GetExpressionCollection(List<BaseFilterExpression> list, Type getForType, Expression upToTable, IEnumerable<Expression> fieldsToCheckReference);

        void GetEditExpressionCollection(List<BaseFilterExpression> list, Type getForType, Expression upToTable, IEnumerable<Expression> fieldsToCheckReference);

        void GetDeleteExpressionCollection(List<BaseFilterExpression> list, Type getForType, Expression upToTable, IEnumerable<Expression> fieldsToCheckReference);

        Expression GetEditExpression<T>(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference)
            where T : class;
        Expression GetDeleteExpression<T>(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference)
            where T : class;
        Expression GetEditAndDeleteExpression<T>(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference)
            where T : class;
        Expression GetAddChildExpression(Expression source, Type getForType, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference);
    }
}
