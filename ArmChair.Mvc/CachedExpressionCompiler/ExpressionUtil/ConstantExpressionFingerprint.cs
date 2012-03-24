namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal sealed class ConstantExpressionFingerprint : ExpressionFingerprint
    {
        private ConstantExpressionFingerprint(ConstantExpression expression) : base(expression)
        {
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
        {
            base.AddToHashCodeCombiner(combiner);
            combiner.AddInt32(this.HoistedLocalsIndex);
        }

        public static ConstantExpressionFingerprint Create(ConstantExpression expression, ParserContext parserContext)
        {
            ConstantExpressionFingerprint fingerprint2 = new ConstantExpressionFingerprint(expression) {
                HoistedLocalsIndex = parserContext.HoistedValues.Count
            };
            ConstantExpressionFingerprint fingerprint = fingerprint2;
            parserContext.HoistedValues.Add(expression.Value);
            return fingerprint;
        }

        public override bool Equals(object obj)
        {
            ConstantExpressionFingerprint fingerprint = obj as ConstantExpressionFingerprint;
            if (fingerprint == null)
            {
                return false;
            }
            return ((this.HoistedLocalsIndex == fingerprint.HoistedLocalsIndex) && base.Equals(fingerprint));
        }

        public override Expression ToExpression(ParserContext parserContext)
        {
            return Expression.Convert(Expression.ArrayIndex(ParserContext.HoistedValuesParameter, Expression.Constant(this.HoistedLocalsIndex)), base.Type);
        }

        public int HoistedLocalsIndex { get; private set; }
    }
}

