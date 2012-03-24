namespace Microsoft.Web.Mvc.ExpressionUtil
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class ParserContext
    {
        public ExpressionFingerprint Fingerprint;
        public readonly List<object> HoistedValues = new List<object>();
        public static readonly ParameterExpression HoistedValuesParameter = Expression.Parameter(typeof(object[]), "hoistedValues");
        public ParameterExpression ModelParameter;
    }
}

