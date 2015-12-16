using System.Collections.Specialized;
using System.IO;

namespace FakeServers.MultiPart
{
    internal class MultiPartSection
    {
        public NameValueCollection Headers { get; set; }

        public Stream Content { get; set; }
    }
}
