namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class MethodCallExpressionFingerprint : ExpressionFingerprint
    {
        private MethodCallExpressionFingerprint(MethodCallExpression expression) : base(expression)
        {
            this.Method = expression.Method;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
        {
            base.AddToHashCodeCombiner(combiner);
            combiner.AddEnumerable(this.Arguments);
            combiner.AddObject(this.Method);
            combiner.AddFingerprint(this.Target);
        }

        public static MethodCallExpressionFingerprint Create(MethodCallExpression expression, ParserContext parserContext)
        {
            ReadOnlyCollection<ExpressionFingerprint> onlys = ExpressionFingerprint.Create(expression.Arguments, parserContext);
            if (onlys == null)
            {
                return null;
            }
            ExpressionFingerprint fingerprint = ExpressionFingerprint.Create(expression.Object, parserContext);
            if ((fingerprint == null) && (expression.Object != null))
            {
                return null;
            }
            return new MethodCallExpressionFingerprint(expression) { Arguments = onlys, Target = fingerprint };
        }

        public override bool Equals(object obj)
        {
            MethodCallExpressionFingerprint fingerprint = obj as MethodCallExpressionFingerprint;
            if (fingerprint == null)
            {
                return false;
            }
            return (((this.Arguments.SequenceEqual<ExpressionFingerprint>(fingerprint.Arguments) && (this.Method == fingerprint.Method)) && object.Equals(this.Target, fingerprint.Target)) && base.Equals(fingerprint));
        }

        public override Expression ToExpression(ParserContext parserContext)
        {
            Expression instance = ExpressionFingerprint.ToExpression(this.Target, parserContext);
            IEnumerable<Expression> arguments = ExpressionFingerprint.ToExpression(this.Arguments, parserContext);
            return Expression.Call(instance, this.Method, arguments);
        }

        public ReadOnlyCollection<ExpressionFingerprint> Arguments { get; private set; }

        public MethodInfo Method { get; private set; }

        public ExpressionFingerprint Target { get; private set; }
    }
}

