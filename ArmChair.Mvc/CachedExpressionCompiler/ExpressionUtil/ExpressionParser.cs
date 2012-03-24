namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Linq.Expressions;

    internal static class ExpressionParser
    {
        public static ParserContext Parse<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
        {
            ParserContext context2 = new ParserContext {
                ModelParameter = expression.Parameters[0]
            };
            ParserContext parserContext = context2;
            Expression body = expression.Body;
            parserContext.Fingerprint = ExpressionFingerprint.Create(body, parserContext);
            return parserContext;
        }
    }
}

