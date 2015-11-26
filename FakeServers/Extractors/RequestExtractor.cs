using System;
using System.Collections.Specialized;
using System.Net;

namespace FakeServers.Extractors
{
    public class RequestExtractor : IRequestBodyExtractor
    {
        private readonly IRequestBodyExtractor bodyExtractor;

        public RequestExtractor()
            : this(new EmptyBodyExtractor())
        {
        }

        public RequestExtractor(IRequestBodyExtractor extractor)
        {
            if (extractor == null)
            {
                throw new ArgumentNullException("extractor");
            }
            this.bodyExtractor = extractor;
        }

        public string[] AcceptTypes { get; private set; }

        public string ContentType { get; private set; }

        public CookieCollection Cookies { get; private set; }

        public NameValueCollection Headers { get; private set; }

        public bool IsAuthenticated { get; private set; }

        public bool IsLocal { get; private set; }

        public bool IsSecureConnection { get; private set; }

        public bool IsWebSocketRequest { get; private set; }

        public bool KeepAlive { get; private set; }

        public string Method { get; private set; }

        public Version ProtocolVersion { get; private set; }

        public NameValueCollection QueryString { get; private set; }

        public Uri Url { get; private set; }

        public Uri UrlReferrer { get; private set; }

        public string UserAgent { get; private set; }

        public void Extract(HttpListenerRequest request)
        {
            this.AcceptTypes = request.AcceptTypes;
            this.ContentType = request.ContentType;
            this.Cookies = request.Cookies;
            this.Headers = new NameValueCollection(request.Headers);
            this.IsAuthenticated = request.IsAuthenticated;
            this.IsLocal = request.IsLocal;
            this.IsSecureConnection = request.IsSecureConnection;
            this.IsWebSocketRequest = request.IsWebSocketRequest;
            this.KeepAlive = request.KeepAlive;
            this.ProtocolVersion = request.ProtocolVersion;
            this.Method = request.HttpMethod;
            this.QueryString = request.QueryString;
            this.Url = request.Url;
            this.UrlReferrer = request.UrlReferrer;
            this.UserAgent = request.UserAgent;
            bodyExtractor.Extract(request);
        }
    }
}
