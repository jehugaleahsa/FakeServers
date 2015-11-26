using System.Net;

namespace FakeServers.Extractors
{
    public class EmptyBodyExtractor : IRequestBodyExtractor
    {
        public void Extract(HttpListenerRequest request)
        {
        }
    }
}
