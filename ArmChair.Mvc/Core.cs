using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Messaging;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Mvc;
using Newtonsoft.Json;

namespace ArmChair
{
    /// <summary>
    /// Contains everything needed to perform a deferred method invoke.
    /// </summary>
    public class OffloadedInvokeParams
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public object[] Parameters { get; set; }
    }

    /// <summary>
    /// Contains everything needed to serialize a method invoke expression, then deserialize and actually invoke it later.  This is used to improve app performance, offloading things like logging 
    /// </summary>
    /// <remarks>
    /// Currently this only supports static methods without overloads.  Primitive argument types are preferred.
    /// </remarks>
    public static class OffloadedMethodInvoking
    {

        public static OffloadedInvokeParams CreateMethodInvokeParameters(Expression<Action> action)
        {
            OffloadedInvokeParams invokeParams = new OffloadedInvokeParams();
            MethodCallExpression body = action.Body as MethodCallExpression;
            if (body == null)
            {
                throw new ArgumentException("action must be a simple method call to a static method, like 'Logger");
            }
            invokeParams.Type = body.Method.DeclaringType.AssemblyQualifiedName;
            invokeParams.Method = body.Method.Name;

            ParameterInfo[] parameters = body.Method.GetParameters();
            if (parameters.Length > 0)
            {
                invokeParams.Parameters = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    Expression arg = body.Arguments[i];
                    ConstantExpression expression2 = arg as ConstantExpression;
                    if (expression2 != null)
                    {
                        invokeParams.Parameters[i] = expression2.Value;
                    }
                    else
                    {
                        invokeParams.Parameters[i] = CachedExpressionCompiler.Evaluate(arg);
                    }
                }
            }
            return invokeParams;
        }

        public static void InvokeFromSerializedInvokeInfo(OffloadedInvokeParams invokeParams)
        {
            Type declaringType = Type.GetType(invokeParams.Type);
            if (declaringType == null)
                throw new ArgumentException(string.Format("Type {0} could not be found",invokeParams.Type));
            var method = declaringType.GetMethod(invokeParams.Method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (method == null)
                throw new ArgumentException(string.Format("Method {0} could not be found", invokeParams.Type));
            if (invokeParams.Parameters.HasItems())
            {
                var methodParams = method.GetParameters();
                if (invokeParams.Parameters.Length != methodParams.Length)
                    throw new ArgumentException("Parameter count mismatch.");

                for (var i = 0; i < methodParams.Length; i++)
                {
                    if (invokeParams.Parameters[i] == null || invokeParams.Parameters[i].GetType() == methodParams[i].ParameterType)
                        continue;

                    //try to convert it to the proper type
                    string serializedJson = JsonConvert.SerializeObject(invokeParams.Parameters[i]);
                    object deserializedObject = JsonConvert.DeserializeObject(serializedJson, methodParams[i].ParameterType);
                    invokeParams.Parameters[i] = deserializedObject;
                }

                method.Invoke(null, invokeParams.Parameters);
            }
            else
            {
                method.Invoke(null, null);
            }

        }

    }


}
