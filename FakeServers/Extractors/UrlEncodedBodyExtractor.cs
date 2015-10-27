using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;

namespace FakeServers.Extractors
{
    public class UrlEncodedBodyExtractor : IRequestBodyExtractor
    {
        public NameValueCollection Parameters { get; private set; }

        public void Extract(HttpListenerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (!request.HasEntityBody)
            {
                return;
            }
            if (request.ContentType != "application/x-www-form-urlencoded")
            {
                return;
            }
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string body = reader.ReadToEnd();
                Uri uri = new Uri("http://localhost/?" + body);
                Parameters = uri.ParseQueryString();
            }
        }
    }
}
