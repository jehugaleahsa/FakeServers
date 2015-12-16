using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FakeServers.MultiPart;

namespace FakeServers.Extractors
{
    public class MultiPartBodyExtractor : IRequestBodyExtractor
    {
        private NameValueCollection parameters;
        private MultiPartFileLookup files;

        public MultiPartBodyExtractor()
        {
            this.parameters = new NameValueCollection();
            this.files = new MultiPartFileLookup();
        }

        public NameValueCollection Parameters
        {
            get { return parameters; }
        }

        public MultiPartFileLookup Files
        {
            get { return files; }
        }

        public void Extract(HttpListenerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (!request.HasEntityBody)
            {
                return;
            }
            // Make sure it is a multi-part request
            string[] parts = request.ContentType.Split(';').Select(s => s.Trim()).ToArray();
            if (!parts[0].Equals("multipart/form-data", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            // Parse the content type parameters
            var contentTypeParameters = parts
                .Skip(1)
                .Select(p => p.Split(new char[] { '=' }, 2))
                .Where(p => p.Length == 2)
                .ToLookup(p => p[0], p => p[1], StringComparer.InvariantCultureIgnoreCase);
            // Check the boundary is specified, and only once
            if (contentTypeParameters["boundary"].Count() != 1)
            {
                return;
            }
            string boundary = contentTypeParameters["boundary"].First();

            using (Stream responseStream = request.InputStream)
            {
                Encoding encoding = request.ContentEncoding;
                StreamingMultiPartParser parser = new StreamingMultiPartParser(responseStream, encoding, boundary);

                parser.SectionFound += (o, e) =>
                {
                    var data = getSectionData(e);
                    if (data == null)
                    {
                        return;
                    }
                    if (String.IsNullOrWhiteSpace(data.FileName))
                    {
                        string value = encoding.GetString(data.Contents);
                        this.parameters.Add(data.Name, value);
                    }
                    else
                    {
                        var file = new MultiPartFile()
                        {
                            Name = data.Name,
                            FileName = data.FileName,
                            ContentType = data.ContentType,
                            Contents = data.Contents
                        };
                        this.files.Add(file.Name, file);
                    }
                };

                parser.Parse().Wait();
            }
        }

        private static SectionData getSectionData(MultiPartSection section)
        {
            string contentDisposition = section.Headers["Content-Disposition"];
            if (contentDisposition == null)
            {
                return null;
            }
            string[] parts = contentDisposition.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();
            var lookup = parts
                .Select(p => p.Split(new char[] { '=' }, 2))
                .Where(p => p.Length == 2)
                .ToLookup(p => p[0], p => p[1].Trim(' ', '"'), StringComparer.CurrentCultureIgnoreCase);
            SectionData data = new SectionData();
            data.Name = getName(lookup["name"].FirstOrDefault());
            data.FileName = getName(lookup["filename"].FirstOrDefault());
            data.ContentType = section.Headers["Content-Type"];
            data.Contents = copyData(section.Content);
            return data;
        }

        private static string getName(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return value;
        }

        private static byte[] copyData(Stream source)
        {
            MemoryStream destination = new MemoryStream();
            source.CopyTo(destination);
            return destination.ToArray();
        }

        private class SectionData
        {
            public string Name { get; set; }

            public string FileName { get; set; }

            public string ContentType { get; set; }

            public byte[] Contents { get; set; }
        }
    }
}
