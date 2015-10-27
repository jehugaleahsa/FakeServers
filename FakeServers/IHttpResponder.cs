using System.Net;
using System.Text;

namespace FakeServers
{
    internal interface IHttpResponder
    {
        Encoding Encoding { get; }

        void Write(HttpListenerResponse response);
    }
}
