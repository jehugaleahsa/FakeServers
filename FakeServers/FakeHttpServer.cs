using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeServers.Extractors;
using FakeServers.Responders;

namespace FakeServers
{
    public class FakeHttpServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly UriBuilder uriBuilder;
        private readonly HttpHeaderCollection headers;
        private CancellationTokenSource tokenSource;
        private IHttpResponder responder;
        private IRequestBodyExtractor extractor;

        public FakeHttpServer()
            : this(new Uri("http://localhost:8080"))
        {            
        }

        public FakeHttpServer(string uri)
            : this(new Uri(uri))
        {
        }

        public FakeHttpServer(Uri uri)
        {
            this.uriBuilder = new UriBuilder(uri);
            uriBuilder.Path = getFixedPath(uriBuilder.Path);
            this.listener = new HttpListener();
            this.headers = new HttpHeaderCollection();
            this.responder = new HttpNullResponder();
            this.StatusCode = HttpStatusCode.OK;
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
            set { uriBuilder.Path = getFixedPath(value);  }
        }

        private static string getFixedPath(string path)
        {
            if (path == null)
            {
                return "/";
            }
            path = path.Replace("\\", "/");
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            return path;
        }

        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription { get; set; }

        public HttpHeaderCollection Headers 
        { 
            get { return headers; } 
        }

        public void ReturnString(string content, Encoding encoding = null)
        {
            if (content == null)
            {
                this.responder = new HttpNullResponder();
            }
            else
            {
                this.responder = new HttpStringResponder(content) { Encoding = encoding };
            }
        }

        public void ReturnBytes(byte[] content, Encoding encoding = null)
        {
            if (content == null)
            {
                this.responder = new HttpNullResponder();
            }
            else
            {
                this.responder = new HttpByteArrayResponder(content) { Encoding = encoding };
            }
        }

        public void ReturnJson<T>(T content, Encoding encoding = null)
        {
            this.responder = new HttpJsonResponder<T>(content) { Encoding = encoding };
        }

        public void UseResponder(IHttpResponder responder)
        {
            this.responder = responder ?? new HttpNullResponder();
        }

        public void UseBodyExtractor(IRequestBodyExtractor extractor)
        {
            this.extractor = extractor;
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
                if (extractor != null)
                {
                    extractor.Extract(context.Request);
                }
                context.Response.SendChunked = false;
                context.Response.StatusCode = (int)StatusCode;
                if (!String.IsNullOrWhiteSpace(StatusDescription))
                {
                    context.Response.StatusDescription = StatusDescription;
                }
                foreach (var headerGroup in headers)
                {
                    foreach (string value in headerGroup)
                    {
                        context.Response.AppendHeader(headerGroup.Key, value);
                    }
                }
                if (responder != null)
                {
                    responder.Write(context.Response);
                    if (responder.Encoding != null)
                    {
                        context.Response.ContentEncoding = responder.Encoding;
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
