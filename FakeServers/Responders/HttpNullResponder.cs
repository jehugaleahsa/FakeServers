using System.Net;
using System.Text;

namespace FakeServers.Responders
{
    public class HttpNullResponder : IHttpResponder
    {
        public Encoding Encoding
        {
            get { return null; }
        }

        public void Write(HttpListenerResponse response)
        {
            response.ContentLength64 = 0;
        }
    }
}
