using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace FakeServers.Extractors
{
    public class MultiPartBodyExtractor : IRequestBodyExtractor
    {
        private readonly List<MultiPartFile> files;

        public MultiPartBodyExtractor()
        {
            this.files = new List<MultiPartFile>();
        }

        public NameValueCollection Parameters { get; private set; }

        public IEnumerable<MultiPartFile> Files
        {
            get { return files; }
        }

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
        }
    }
}
