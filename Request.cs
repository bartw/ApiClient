using System;
using System.Net;
using System.Threading;

namespace ApiClient
{
    internal abstract class Request
    {
        public HttpWebRequest WebRequest { get; set; }
        public ManualResetEvent AllDone { get; set; }

        public Request()
        {
            AllDone = new ManualResetEvent(false);
        }
    }

    internal class ContentRequest : Request
    {
        public string Content { get; set; }
    }

    internal class CallbackRequest : Request
    {
        public Action<object> CallbackAction { get; set; }
        public Action<Exception> ExceptionAction { get; set; }
    }
}
