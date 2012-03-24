namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class BinaryExpressionFingerprint : ExpressionFingerprint
    {
        private BinaryExpressionFingerprint(BinaryExpression expression) : base(expression)
        {
            this.IsLiftedToNull = expression.IsLiftedToNull;
            this.Method = expression.Method;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
        {
            base.AddToHashCodeCombiner(combiner);
            combiner.AddInt32(this.IsLiftedToNull.GetHashCode());
            combiner.AddFingerprint(this.Left);
            combiner.AddObject(this.Method);
            combiner.AddFingerprint(this.Right);
        }

        public static BinaryExpressionFingerprint Create(BinaryExpression expression, ParserContext parserContext)
        {
            if (expression.Conversion != null)
            {
                return null;
            }
            ExpressionFingerprint fingerprint = ExpressionFingerprint.Create(expression.Left, parserContext);
            if ((fingerprint == null) && (expression.Left != null))
            {
                return null;
            }
            ExpressionFingerprint fingerprint2 = ExpressionFingerprint.Create(expression.Right, parserContext);
            if ((fingerprint2 == null) && (expression.Right != null))
            {
                return null;
            }
            return new BinaryExpressionFingerprint(expression) { Left = fingerprint, Right = fingerprint2 };
        }

        public override bool Equals(object obj)
        {
            BinaryExpressionFingerprint fingerprint = obj as BinaryExpressionFingerprint;
            if (fingerprint == null)
            {
                return false;
            }
            return ((((this.IsLiftedToNull == fingerprint.IsLiftedToNull) && object.Equals(this.Left, fingerprint.Left)) && ((this.Method == fingerprint.Method) && object.Equals(this.Right, fingerprint.Right))) && base.Equals(fingerprint));
        }

        public override Expression ToExpression(ParserContext parserContext)
        {
            Expression left = ExpressionFingerprint.ToExpression(this.Left, parserContext);
            Expression right = ExpressionFingerprint.ToExpression(this.Right, parserContext);
            return Expression.MakeBinary(base.NodeType, left, right, this.IsLiftedToNull, this.Method);
        }

        public bool IsLiftedToNull { get; private set; }

        public ExpressionFingerprint Left { get; private set; }

        public MethodInfo Method { get; private set; }

        public ExpressionFingerprint Right { get; private set; }
    }
}

