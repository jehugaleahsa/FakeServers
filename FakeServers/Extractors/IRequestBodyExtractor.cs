using System.Net;

namespace FakeServers.Extractors
{
    public interface IRequestBodyExtractor
    {
        void Extract(HttpListenerRequest request);
    }
}
