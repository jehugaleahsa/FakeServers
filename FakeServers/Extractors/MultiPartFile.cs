using System;

namespace FakeServers.Extractors
{
    public class MultiPartFile
    {
        public string Name { get; internal set; }

        public string FileName { get; internal set; }

        public byte[] Contents { get; internal set; }

        public string ContentType { get; internal set; }
    }
}
