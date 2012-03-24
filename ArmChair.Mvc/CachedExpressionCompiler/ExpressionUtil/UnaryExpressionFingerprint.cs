namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class UnaryExpressionFingerprint : ExpressionFingerprint
    {
        private UnaryExpressionFingerprint(UnaryExpression expression) : base(expression)
        {
            this.Method = expression.Method;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
        {
            base.AddToHashCodeCombiner(combiner);
            combiner.AddObject(this.Method);
            combiner.AddFingerprint(this.Operand);
        }

        public static UnaryExpressionFingerprint Create(UnaryExpression expression, ParserContext parserContext)
        {
            ExpressionFingerprint fingerprint = ExpressionFingerprint.Create(expression.Operand, parserContext);
            if ((fingerprint == null) && (expression.Operand != null))
            {
                return null;
            }
            return new UnaryExpressionFingerprint(expression) { Operand = fingerprint };
        }

        public override bool Equals(object obj)
        {
            UnaryExpressionFingerprint fingerprint = obj as UnaryExpressionFingerprint;
            if (fingerprint == null)
            {
                return false;
            }
            return (((this.Method == fingerprint.Method) && object.Equals(this.Operand, fingerprint.Operand)) && base.Equals(fingerprint));
        }

        public override Expression ToExpression(ParserContext parserContext)
        {
            Expression expression = ExpressionFingerprint.ToExpression(this.Operand, parserContext);
            if (base.NodeType == ExpressionType.UnaryPlus)
            {
                return Expression.UnaryPlus(expression, this.Method);
            }
            return Expression.MakeUnary(base.NodeType, expression, base.Type, this.Method);
        }

        public MethodInfo Method { get; private set; }

        public ExpressionFingerprint Operand { get; private set; }
    }
}

