using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FakeServers
{
    public class FakeHttpServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly UriBuilder uriBuilder;
        private CancellationTokenSource tokenSource;
        private readonly OrderedDictionary headers;

        public FakeHttpServer()
        {
            this.uriBuilder = new UriBuilder("http", "localhost", 8080);
            this.listener = new HttpListener();
            this.headers = new OrderedDictionary();
        }

        ~FakeHttpServer()
        {
            Dispose(false);
        }

        public string Scheme
        {
            get { return uriBuilder.Scheme; }
            set { uriBuilder.Scheme = value; }
        }

        public string Host
        {
            get { return uriBuilder.Host; }
            set { uriBuilder.Host = value; }
        }

        public int Port
        {
            get { return uriBuilder.Port; }
            set { uriBuilder.Port = value; }
        }

        public string Path
        {
            get { return uriBuilder.Path; }
            set { uriBuilder.Path = value; }
        }

        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription { get; set; }

        public void AddHeader(string name, string value)
        {
            List<string> values;
            if (headers.Contains(name))
            {
                values = (List<string>)headers[name];
            }
            else
            {
                values = new List<string>();
                headers.Add(name, values);
            }
            values.Add(value);
        }

        public void RemoveHeader(string name)
        {
            headers.Remove(name);
        }

        public void ClearHeaders()
        {
            headers.Clear();
        }

        public void Listen()
        {
            listener.Prefixes.Clear();
            listener.Prefixes.Add(uriBuilder.Uri.ToString());
            listener.Start();

            tokenSource = new CancellationTokenSource();
            createRequestHandler();
        }

        private void createRequestHandler()
        {
            Task<HttpListenerContext> contextWaiter = listener.GetContextAsync();
            Task handler = contextWaiter.ContinueWith(t =>
            {
                HttpListenerContext context = t.Result;
                context.Response.StatusCode = (int)StatusCode;
                if (!String.IsNullOrWhiteSpace(StatusDescription))
                {
                    context.Response.StatusDescription = StatusDescription;
                }
                foreach (string header in headers.Keys)
                {
                    foreach (string value in (List<string>)headers[header])
                    {
                        context.Response.AppendHeader(header, value);
                    }
                }
                context.Response.Close();
            }, tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            handler.ContinueWith(t => createRequestHandler(), tokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
        }

        public void Close()
        {
            if (listener.IsListening)
            {
                if (tokenSource != null)
                {
                    try
                    {
                        tokenSource.Cancel();
                    }
                    catch
                    {
                    }
                }
                listener.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                Close();
            }
        }
    }
}
