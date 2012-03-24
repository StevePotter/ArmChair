namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using Microsoft.Web.Mvc;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class FastTrack<TModel, TValue>
    {
        private static readonly ConstMemberLookupCache _constMemberLookupCache;
        private static Func<TModel, TValue> _identityFunc;
        private static readonly ModelMemberLookupCache _modelMemberLookupCache;

        static FastTrack()
        {
            FastTrack<TModel, TValue>._constMemberLookupCache = new ConstMemberLookupCache();
            FastTrack<TModel, TValue>._modelMemberLookupCache = new ModelMemberLookupCache();
        }

        private static Func<TModel, TValue> GetConstMemberLookupFunc(MemberInfo member, object constValue)
        {
            Func<object, TValue> innerFunc = FastTrack<TModel, TValue>._constMemberLookupCache.GetFunc(member);
            return _ => innerFunc(constValue);
        }

        public static Func<TModel, TValue> GetFunc(ParameterExpression modelParameter, Expression body)
        {
            if (modelParameter == body)
            {
                return FastTrack<TModel, TValue>.GetIdentityFunc();
            }
            ConstantExpression expression = body as ConstantExpression;
            if (expression != null)
            {
                TValue value = (TValue) expression.Value;
                return _ => value;
            }
            MemberExpression expression2 = body as MemberExpression;
            if (expression2 != null)
            {
                if (expression2.Expression == null)
                {
                    return FastTrack<TModel, TValue>.GetModelMemberLookupFunc(expression2.Member, true);
                }
                if (expression2.Expression == modelParameter)
                {
                    return FastTrack<TModel, TValue>.GetModelMemberLookupFunc(expression2.Member, false);
                }
                ConstantExpression expression3 = expression2.Expression as ConstantExpression;
                if (expression3 != null)
                {
                    return FastTrack<TModel, TValue>.GetConstMemberLookupFunc(expression2.Member, expression3.Value);
                }
            }
            return null;
        }

        private static Func<TModel, TValue> GetIdentityFunc()
        {
            if (FastTrack<TModel, TValue>._identityFunc == null)
            {
                FastTrack<TModel, TValue>._identityFunc = model => model.Compile();
            }
            return FastTrack<TModel, TValue>._identityFunc;
        }

        private static Func<TModel, TValue> GetModelMemberLookupFunc(MemberInfo member, bool isStatic)
        {
            return FastTrack<TModel, TValue>._modelMemberLookupCache.GetFunc(member, isStatic);
        }

        private sealed class ConstMemberLookupCache : ReaderWriterCache<MemberInfo, Func<object, TValue>>
        {
            private static Func<object, TValue> CreateFunc(MemberInfo member)
            {
                ParameterExpression expression;
                return Expression.Lambda<Func<object, TValue>>(Expression.MakeMemberAccess(Expression.Convert(expression = Expression.Parameter(typeof(object), "constValue"), member.DeclaringType), member), new ParameterExpression[] { expression }).Compile();
            }

            public Func<object, TValue> GetFunc(MemberInfo member)
            {
                return base.FetchOrCreateItem(member, () => FastTrack<TModel, TValue>.ConstMemberLookupCache.CreateFunc(member));
            }
        }

        private sealed class ModelMemberLookupCache : ReaderWriterCache<MemberInfo, Func<TModel, TValue>>
        {
            private static Func<TModel, TValue> CreateFunc(MemberInfo member, bool isStatic)
            {
                ParameterExpression expression = null;
                return Expression.Lambda<Func<TModel, TValue>>(Expression.MakeMemberAccess(!isStatic ? (expression = Expression.Parameter(typeof(TModel), "model")) : null, member), new ParameterExpression[] { expression }).Compile();
            }

            public Func<TModel, TValue> GetFunc(MemberInfo member, bool isStatic)
            {
                return base.FetchOrCreateItem(member, () => FastTrack<TModel, TValue>.ModelMemberLookupCache.CreateFunc(member, isStatic));
            }
        }
    }
}

