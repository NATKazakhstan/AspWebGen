namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Linq.Expressions;

    public interface IBaseFilterOrItem
    {
        object FilterValue { get; }
        Type FilterValueType { get; }
        Expression FilterExpression { get; }
    }
}