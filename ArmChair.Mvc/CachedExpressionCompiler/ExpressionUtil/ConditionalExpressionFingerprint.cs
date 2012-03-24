namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal sealed class ConditionalExpressionFingerprint : ExpressionFingerprint
    {
        private ConditionalExpressionFingerprint(ConditionalExpression expression) : base(expression)
        {
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
        {
            base.AddToHashCodeCombiner(combiner);
            combiner.AddFingerprint(this.Test);
            combiner.AddFingerprint(this.IfTrue);
            combiner.AddFingerprint(this.IfFalse);
        }

        public static ConditionalExpressionFingerprint Create(ConditionalExpression expression, ParserContext parserContext)
        {
            ExpressionFingerprint fingerprint = ExpressionFingerprint.Create(expression.Test, parserContext);
            if ((fingerprint == null) && (expression.Test != null))
            {
                return null;
            }
            ExpressionFingerprint fingerprint2 = ExpressionFingerprint.Create(expression.IfTrue, parserContext);
            if ((fingerprint2 == null) && (expression.IfTrue != null))
            {
                return null;
            }
            ExpressionFingerprint fingerprint3 = ExpressionFingerprint.Create(expression.IfFalse, parserContext);
            if ((fingerprint3 == null) && (expression.IfFalse != null))
            {
                return null;
            }
            return new ConditionalExpressionFingerprint(expression) { Test = fingerprint, IfTrue = fingerprint2, IfFalse = fingerprint3 };
        }

        public override bool Equals(object obj)
        {
            ConditionalExpressionFingerprint fingerprint = obj as ConditionalExpressionFingerprint;
            if (fingerprint == null)
            {
                return false;
            }
            return (((object.Equals(this.Test, fingerprint.Test) && object.Equals(this.IfTrue, fingerprint.IfTrue)) && object.Equals(this.IfFalse, fingerprint.IfFalse)) && base.Equals(fingerprint));
        }

        public override Expression ToExpression(ParserContext parserContext)
        {
            Expression test = ExpressionFingerprint.ToExpression(this.Test, parserContext);
            Expression ifTrue = ExpressionFingerprint.ToExpression(this.IfTrue, parserContext);
            Expression ifFalse = ExpressionFingerprint.ToExpression(this.IfFalse, parserContext);
            return Expression.Condition(test, ifTrue, ifFalse);
        }

        public ExpressionFingerprint IfFalse { get; private set; }

        public ExpressionFingerprint IfTrue { get; private set; }

        public ExpressionFingerprint Test { get; private set; }
    }
}

