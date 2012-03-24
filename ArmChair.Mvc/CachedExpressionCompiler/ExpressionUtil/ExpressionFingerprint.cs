using System.Linq;

namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal abstract class ExpressionFingerprint
    {
        protected ExpressionFingerprint(Expression expression)
        {
            this.NodeType = expression.NodeType;
            this.Type = expression.Type;
        }

        internal virtual void AddToHashCodeCombiner(HashCodeCombiner combiner)
        {
            combiner.AddObject(this.NodeType);
            combiner.AddObject(this.Type);
        }

        public static ReadOnlyCollection<ExpressionFingerprint> Create(IEnumerable<Expression> expressions, ParserContext parserContext)
        {
            List<ExpressionFingerprint> list = new List<ExpressionFingerprint>();
            foreach (Expression expression in expressions)
            {
                ExpressionFingerprint item = Create(expression, parserContext);
                if ((item == null) && (expression != null))
                {
                    return null;
                }
                list.Add(item);
            }
            return new ReadOnlyCollection<ExpressionFingerprint>(list);
        }

        public static ExpressionFingerprint Create(Expression expression, ParserContext parserContext)
        {
            BinaryExpression expression2 = expression as BinaryExpression;
            if (expression2 != null)
            {
                return BinaryExpressionFingerprint.Create(expression2, parserContext);
            }
            ConditionalExpression expression3 = expression as ConditionalExpression;
            if (expression3 != null)
            {
                return ConditionalExpressionFingerprint.Create(expression3, parserContext);
            }
            ConstantExpression expression4 = expression as ConstantExpression;
            if (expression4 != null)
            {
                return ConstantExpressionFingerprint.Create(expression4, parserContext);
            }
            MemberExpression expression5 = expression as MemberExpression;
            if (expression5 != null)
            {
                return MemberExpressionFingerprint.Create(expression5, parserContext);
            }
            MethodCallExpression expression6 = expression as MethodCallExpression;
            if (expression6 != null)
            {
                return MethodCallExpressionFingerprint.Create(expression6, parserContext);
            }
            ParameterExpression expression7 = expression as ParameterExpression;
            if (expression7 != null)
            {
                return ParameterExpressionFingerprint.Create(expression7, parserContext);
            }
            UnaryExpression expression8 = expression as UnaryExpression;
            if (expression8 != null)
            {
                return UnaryExpressionFingerprint.Create(expression8, parserContext);
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            ExpressionFingerprint fingerprint = obj as ExpressionFingerprint;
            if (fingerprint == null)
            {
                return false;
            }
            return (((this.NodeType == fingerprint.NodeType) && (this.Type == fingerprint.Type)) && (base.GetType() == fingerprint.GetType()));
        }

        public override int GetHashCode()
        {
            HashCodeCombiner combiner = new HashCodeCombiner();
            combiner.AddObject(base.GetType());
            this.AddToHashCodeCombiner(combiner);
            return combiner.CombinedHash;
        }

        public abstract Expression ToExpression(ParserContext parserContext);
        protected static IEnumerable<Expression> ToExpression(IEnumerable<ExpressionFingerprint> fingerprints, ParserContext parserContext)
        {
            return (from fingerprint in fingerprints select ToExpression(fingerprint, parserContext));
        }

        protected static Expression ToExpression(ExpressionFingerprint fingerprint, ParserContext parserContext)
        {
            if (fingerprint == null)
            {
                return null;
            }
            return fingerprint.ToExpression(parserContext);
        }

        public ExpressionType NodeType { get; private set; }

        public System.Type Type { get; private set; }
    }
}

