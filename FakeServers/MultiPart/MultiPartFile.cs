using System.Collections.Specialized;
using System.IO;

namespace FakeServers.MultiPart
{
    public class MultiPartFile
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] Contents { get; set; }
    }
}
