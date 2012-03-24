namespace Microsoft.Web.Mvc
{
    using Microsoft.Web.Mvc.ExpressionUtil;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public static class CachedExpressionCompiler
    {
        private static readonly ParameterExpression _unusedParameterExpr = Expression.Parameter(typeof(object), "_unused");

        public static Func<TModel, TValue> Compile<TModel, TValue>(this Expression<Func<TModel, TValue>> lambdaExpression)
        {
            if (lambdaExpression == null)
            {
                throw new ArgumentNullException("lambdaExpression");
            }
            return Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Process<TModel, TValue>(lambdaExpression);
        }

        public static object Evaluate(Expression arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }
            return Wrap(arg)(null);
        }

        private static Func<object, object> Wrap(Expression arg)
        {
            Expression<Func<object, object>> lambdaExpression = Expression.Lambda<Func<object, object>>(Expression.Convert(arg, typeof(object)), new ParameterExpression[] { _unusedParameterExpr });
            return Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Process<object, object>(lambdaExpression);
        }
    }
}

