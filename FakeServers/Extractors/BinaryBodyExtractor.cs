using System;
using System.IO;
using System.Net;
using System.Text;

namespace FakeServers.Extractors
{
    public class BinaryBodyExtractor : IRequestBodyExtractor
    {
        public BinaryBodyExtractor()
        {
        }

        public byte[] Contents { get; private set; }

        public Encoding ContentEncoding { get; private set; }

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
            MemoryStream stream = new MemoryStream();
            request.InputStream.CopyTo(stream);
            Contents = stream.ToArray();
            ContentEncoding = request.ContentEncoding;
        }
    }
}
