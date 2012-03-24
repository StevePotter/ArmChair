using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Messaging;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ArmChair
{
    /// <summary>
    /// Receives msmq messages for offloaded execution, executing the code as necessary.
    /// </summary>
    public class OffloadedTaskReceiver
    {
        public string MessageQueueName { get; set; }


        public MessageQueue MessageQueue
        {
            get
            {
                if (_MessageQueue == null)
                {
                    if (!MessageQueueName.HasChars())
                    {
                        throw new SettingsPropertyNotFoundException("MessageQueueName queue name is missing.");
                    }
                    _MessageQueue = new MessageQueue(MessageQueueName);
                }
                return _MessageQueue;
            }
        }
        private MessageQueue _MessageQueue;


        public void Run()
        {
            var message = MessageQueue.Receive();
            if (message == null)
                return;
            
            //message body is xml with a bunch of <string> elements each containing a url to execute
            var body = new StreamReader(message.BodyStream).ReadToEnd();
            OffloadedInvokeParams[] invokes = (from e in XDocument.Parse(body).Descendants("string") select e.Value).Select(json => JsonConvert.DeserializeObject<OffloadedInvokeParams>(json)).ToArray();
            if (!invokes.HasItems())
            {
                foreach (var deferredMethodInvokeInfo in invokes)
                {
                    OffloadedMethodInvoking.InvokeFromSerializedInvokeInfo(deferredMethodInvokeInfo);
                }
            }
        }

     
    }

}
