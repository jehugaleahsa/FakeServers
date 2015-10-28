using System;
using System.IO;
using System.Net;
using System.Text;

namespace FakeServers.Responders
{
    public class HttpStringResponder : IHttpResponder
    {
        public HttpStringResponder(string content)
        {
            Content = content ?? String.Empty;
        }

        public string Content { get; private set; }

        public Encoding Encoding { get; set; }

        public virtual void Write(HttpListenerResponse response)
        {
            Encoding encoding = Encoding ?? Encoding.Default;
            response.ContentLength64 = encoding.GetByteCount(Content);
            StreamWriter writer = new StreamWriter(response.OutputStream, encoding);
            writer.Write(Content);
            writer.Flush();
        }
    }
}
