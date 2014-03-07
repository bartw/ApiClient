using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;

namespace ApiClient
{
    public class RequestThrottler
    {
        private PriorityQueue<CallbackRequest> _queue;
        private List<CallbackRequest> _initiatedRequests;
        private int _requestsPerSecond;
        private int _timeout;
        private BackgroundWorker _requestsProcessor;

        public bool IsRunning { get; private set; }

        public RequestThrottler() : this(5, 5000) {}

        public RequestThrottler(int requestsPerSecond) : this(requestsPerSecond, 5000) { }

        public RequestThrottler(int requestsPerSecond, int timeout)
        {
            _queue = new PriorityQueue<CallbackRequest>();
            _initiatedRequests = new List<CallbackRequest>();
            IsRunning = false;
            _requestsPerSecond = requestsPerSecond;
            _timeout = timeout;
            _requestsProcessor = new BackgroundWorker();
            _requestsProcessor.DoWork += new DoWorkEventHandler(ProcessRequests);
        }

        public void Run()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                _requestsProcessor.RunWorkerAsync();
            }
        }

        public void Suspend()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _requestsProcessor.CancelAsync();
            }
        }

        public void EnqueueRequest(HttpWebRequest webRequest, Action<object> callbackAction, Action<Exception> exceptionAction, Priority priority)
        {
            var request = new CallbackRequest()
            {
                WebRequest = webRequest,
                CallbackAction = callbackAction,
                ExceptionAction = exceptionAction
            };

            _queue.Enqueue(request, priority);
        }

        private void ProcessRequests(object sender, DoWorkEventArgs e)
        {
            while (IsRunning && !e.Cancel)
            {                
                if (!_queue.IsEmpty())
                {
                    if (_initiatedRequests.Count < _requestsPerSecond)
                    {
                        var request = _queue.Dequeue();

                        _initiatedRequests.Add(request);

                        var requestProcessor = new BackgroundWorker();
                        requestProcessor.DoWork += new DoWorkEventHandler(ProcessRequest);
                        requestProcessor.RunWorkerAsync(request);
                    }
                }
            }
        }

        private void ProcessRequest(object sender, DoWorkEventArgs e)
        {
            var request = e.Argument as CallbackRequest;

            if (request == null)
            {
                return;
            }

            try
            {
                request.WebRequest.BeginGetResponse(ResponseCallback, request);

                var success = request.AllDone.WaitOne(_timeout);

                if (!success)
                {
                    request.WebRequest.Abort();
                }

                _initiatedRequests.Remove(request);
            }
            catch (Exception ex)
            {
                request.ExceptionAction.Invoke(ex);
            }
        }

        private void ResponseCallback(IAsyncResult result)
        {
            var request = result.AsyncState as CallbackRequest;

            try
            {
                using (var response = (HttpWebResponse)request.WebRequest.EndGetResponse(result))
                {
                    request.CallbackAction.Invoke(response);

                    /*
                    if (response.Headers["Date"] != null)
                    {
                        var responseTime = DateTime.Parse(response.Headers["Date"]);
                    }
                    */
                }
            }
            catch (Exception e)
            {
                request.ExceptionAction.Invoke(e);
            }
            finally
            {
                Thread.Sleep(1000);
                request.AllDone.Set();
            }
        }
    }
}