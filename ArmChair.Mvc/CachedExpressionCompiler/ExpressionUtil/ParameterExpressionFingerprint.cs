namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Linq.Expressions;

    internal sealed class ParameterExpressionFingerprint : ExpressionFingerprint
    {
        private ParameterExpressionFingerprint(ParameterExpression expression) : base(expression)
        {
        }

        public static ParameterExpressionFingerprint Create(ParameterExpression expression, ParserContext parserContext)
        {
            if (expression == parserContext.ModelParameter)
            {
                return new ParameterExpressionFingerprint(expression);
            }
            return null;
        }

        public override Expression ToExpression(ParserContext parserContext)
        {
            return parserContext.ModelParameter;
        }
    }
}

