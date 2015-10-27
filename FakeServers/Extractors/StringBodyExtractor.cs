using System;
using System.IO;
using System.Net;

namespace FakeServers.Extractors
{
    public class StringBodyExtractor : IRequestBodyExtractor
    {
        public string Content { get; private set; }

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
            using (StreamReader reader = new StreamReader(request.InputStream))
            {
                Content = reader.ReadToEnd();
            }
        }
    }
}
