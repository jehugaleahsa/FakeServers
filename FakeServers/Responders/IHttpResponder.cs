using System.Net;
using System.Text;

namespace FakeServers.Responders
{
    public interface IHttpResponder
    {
        Encoding Encoding { get; }

        void Write(HttpListenerResponse response);
    }
}
