using System.Collections.Generic;
using System.Linq.Expressions;

namespace Nat.Web.Controls.GenerationClasses.Filter
{
    public class FilterDataArgs
    {
        public Expression Source { get; set; }
        public Expression UpToTable { get; set; }
        public ParameterExpression TableParam { get; set; }
        public IEnumerable<Expression> FieldsToCheckReference { get; set; }
        public QueryParameters QueryParameters { get; set; }
    }
}