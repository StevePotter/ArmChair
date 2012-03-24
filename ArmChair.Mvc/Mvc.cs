using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Messaging;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace ArmChair
{
    public static class OffloadedExecutionHelper
    {
        public const string HttpItemsKeyForOffloadedExecutions = "ArmchairOffloadedExecutions";

        /// <summary>
        /// Defers the execution of the given expression to something outside the web server process.
        /// Invoke will happen sometime soon but it's not known exactly when.  This is nice for things like logging that don't need to happen right away.
        /// </summary>
        public static void Offload(this Controller controller, Expression<Action> action)
        {
            controller.HttpContext.Offload(action);
        }

        /// <summary>
        /// Defers the execution of the given expression to something outside the web server process.
        /// Invoke will happen sometime soon but it's not known exactly when.  This is nice for things like logging that don't need to happen right away.
        /// </summary>
        public static void Offload(this HttpContextBase context, Expression<Action> action)
        {
            context.CurrentOffloadedExecutions().Add(OffloadedMethodInvoking.CreateMethodInvokeParameters(action));
        }


        /// <summary>
        /// Gets the list of expressions that will be offloaded to a separate process.  
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static List<OffloadedInvokeParams> CurrentOffloadedExecutions(this HttpContextBase context)
        {
            var items = context.Items;
            var requests = (List<OffloadedInvokeParams>)items[HttpItemsKeyForOffloadedExecutions];
            if (requests == null)
            {
                requests = new List<OffloadedInvokeParams>();
                items[HttpItemsKeyForOffloadedExecutions] = requests;
            }
            return requests;
        }

    }


    /// <summary>
    /// Takes all the method invokes that have been deferred during a web requests and sends them to a MSMQ, where they are picked up 
    /// by a job and invoked.  
    /// </summary>
    public class SendOffloadedExecutionsAttribute : ActionFilterAttribute
    {

        public static MessageQueue MessageQueue
        {
            get
            {
                if (_MessageQueue == null)
                {
                    var setting = ConfigurationManager.AppSettings["OffloadedExecutionsQueue"];
                    if (!setting.HasChars())
                    {
                        throw new SettingsPropertyNotFoundException("OffloadedExecutionsQueue queue name is missing from <appSettings>.");
                    }
                    _MessageQueue = new MessageQueue(setting);
                }
                return _MessageQueue;
            }
        }
        private static MessageQueue _MessageQueue;

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);

            var invokes = (List<OffloadedInvokeParams>)filterContext.HttpContext.Items[OffloadedExecutionHelper.HttpItemsKeyForOffloadedExecutions];
            if (invokes.HasItems())
            {
                SendMessagesToInvoke(invokes);
                filterContext.HttpContext.Items.Remove(OffloadedExecutionHelper.HttpItemsKeyForOffloadedExecutions);//for avoiding things being sent twice for whatever reason
            }
        }

        public static void SendMessagesToInvoke(IEnumerable<OffloadedInvokeParams> invokes)
        {
            var serializedInvokes = invokes.Select(e => JsonConvert.SerializeObject(e)).ToArray();
            MessageQueue.Send(new Message(serializedInvokes)); //this overload is used as per https://connect.microsoft.com/VisualStudio/feedback/details/94943/messagequeue-send-crashes-if-called-by-multiple-threads, you gotta use a message object or else you get random exceptions
        }
    }


}
