namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate TValue CompiledExpressionDelegate<TModel, TValue>(TModel model, object[] hoistedValues);
}

