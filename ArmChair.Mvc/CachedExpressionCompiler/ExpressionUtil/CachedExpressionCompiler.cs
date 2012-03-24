namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using Microsoft.Web.Mvc;
    using System;
    using System.Linq.Expressions;

    internal static class CachedExpressionCompiler
    {
        public static Func<TModel, TValue> Process<TModel, TValue>(Expression<Func<TModel, TValue>> lambdaExpression)
        {
            return Processor<TModel, TValue>.GetFunc(lambdaExpression);
        }

        private static class Processor<TModel, TValue>
        {
            private static readonly Cache _cache;

            static Processor()
            {
                Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Processor<TModel, TValue>._cache = new Cache();
            }

            public static Func<TModel, TValue> GetFunc(Expression<Func<TModel, TValue>> lambdaExpression)
            {
                Func<TModel, TValue> funcFastTrack = Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Processor<TModel, TValue>.GetFuncFastTrack(lambdaExpression);
                if (funcFastTrack != null)
                {
                    return funcFastTrack;
                }
                funcFastTrack = Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Processor<TModel, TValue>.GetFuncFingerprinted(lambdaExpression);
                if (funcFastTrack != null)
                {
                    return funcFastTrack;
                }
                return Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Processor<TModel, TValue>.GetFuncSlow(lambdaExpression);
            }

            private static Func<TModel, TValue> GetFuncFastTrack(Expression<Func<TModel, TValue>> lambdaExpression)
            {
                ParameterExpression modelParameter = lambdaExpression.Parameters[0];
                Expression body = lambdaExpression.Body;
                return FastTrack<TModel, TValue>.GetFunc(modelParameter, body);
            }

            private static Func<TModel, TValue> GetFuncFingerprinted(Expression<Func<TModel, TValue>> lambdaExpression)
            {
                ParserContext context = ExpressionParser.Parse<TModel, TValue>(lambdaExpression);
                if (context.Fingerprint == null)
                {
                    return null;
                }
                object[] hoistedValues = context.HoistedValues.ToArray();
                CompiledExpressionDelegate<TModel, TValue> del = Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Processor<TModel, TValue>._cache.GetDelegate(context);
                return model => del(model, hoistedValues);
            }

            private static Func<TModel, TValue> GetFuncSlow(Expression<Func<TModel, TValue>> lambdaExpression)
            {
                return lambdaExpression.Compile();
            }

            private sealed class Cache : ReaderWriterCache<ExpressionFingerprint, CompiledExpressionDelegate<TModel, TValue>>
            {
                private static CompiledExpressionDelegate<TModel, TValue> CreateDelegate(ParserContext context)
                {
                    return Expression.Lambda<CompiledExpressionDelegate<TModel, TValue>>(context.Fingerprint.ToExpression(context), new ParameterExpression[] { context.ModelParameter, ParserContext.HoistedValuesParameter }).Compile();
                }

                public CompiledExpressionDelegate<TModel, TValue> GetDelegate(ParserContext context)
                {
                    return base.FetchOrCreateItem(context.Fingerprint, () => Microsoft.Web.Mvc.ExpressionUtil.CachedExpressionCompiler.Processor<TModel, TValue>.Cache.CreateDelegate(context));
                }
            }
        }
    }
}

