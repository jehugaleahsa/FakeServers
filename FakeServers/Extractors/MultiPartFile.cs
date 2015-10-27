using System;

namespace FakeServers.Extractors
{
    public class MultiPartFile
    {
        public string FileName { get; private set; }

        public byte[] Contents { get; private set; }

        public string ContentType { get; private set; }
    }
}
