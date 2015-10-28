using System;
using System.Net;
using System.Text;

namespace FakeServers.Responders
{
    public class HttpByteArrayResponder : IHttpResponder
    {
        public HttpByteArrayResponder(byte[] content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            Content = content;
        }

        public byte[] Content { get; private set; }

        public Encoding Encoding { get; set; }

        public void Write(HttpListenerResponse response)
        {
            response.ContentLength64 = Content.Length;
            response.OutputStream.Write(Content, 0, Content.Length);
        }
    }
}
