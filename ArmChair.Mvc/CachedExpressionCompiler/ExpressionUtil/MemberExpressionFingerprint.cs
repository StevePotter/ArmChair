namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class MemberExpressionFingerprint : ExpressionFingerprint
    {
        private MemberExpressionFingerprint(MemberExpression expression) : base(expression)
        {
            this.Member = expression.Member;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
        {
            base.AddToHashCodeCombiner(combiner);
            combiner.AddObject(this.Member);
            combiner.AddFingerprint(this.Target);
        }

        public static MemberExpressionFingerprint Create(MemberExpression expression, ParserContext parserContext)
        {
            ExpressionFingerprint fingerprint = ExpressionFingerprint.Create(expression.Expression, parserContext);
            if ((fingerprint == null) && (expression.Expression != null))
            {
                return null;
            }
            return new MemberExpressionFingerprint(expression) { Target = fingerprint };
        }

        public override bool Equals(object obj)
        {
            MemberExpressionFingerprint fingerprint = obj as MemberExpressionFingerprint;
            if (fingerprint == null)
            {
                return false;
            }
            return (((this.Member == fingerprint.Member) && object.Equals(this.Target, fingerprint.Target)) && base.Equals(fingerprint));
        }

        public override Expression ToExpression(ParserContext parserContext)
        {
            return Expression.MakeMemberAccess(ExpressionFingerprint.ToExpression(this.Target, parserContext), this.Member);
        }

        public MemberInfo Member { get; private set; }

        public ExpressionFingerprint Target { get; private set; }
    }
}

