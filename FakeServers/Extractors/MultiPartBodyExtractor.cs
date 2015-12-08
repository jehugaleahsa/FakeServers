using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using HttpMultipartParser;

namespace FakeServers.Extractors
{
    public class MultiPartBodyExtractor : IRequestBodyExtractor
    {
        private NameValueCollection parameters;
        private ILookup<string, MultiPartFile> files;

        public MultiPartBodyExtractor()
        {
            this.parameters = new NameValueCollection();
            this.files = Enumerable.Empty<int>().ToLookup(p => (string)null, p => (MultiPartFile)null);
        }

        public NameValueCollection Parameters
        {
            get { return parameters; }
        }

        public ILookup<string, MultiPartFile> Files
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
            MultipartFormDataParser parser = new MultipartFormDataParser(request.InputStream, boundary, request.ContentEncoding);

            this.files = parser.Files.Select(parsedFile => new MultiPartFile()
            {
                Name = parsedFile.Name,
                FileName = parsedFile.FileName,
                ContentType = parsedFile.ContentType,
                Contents = copyData(parsedFile.Data)
            }).ToLookup(f => f.Name, StringComparer.InvariantCultureIgnoreCase);

            NameValueCollection collection = new NameValueCollection();
            foreach (var parsedParameter in parser.Parameters)
            {
                collection.Add(parsedParameter.Name, parsedParameter.Data);
            }
            this.parameters = collection;
        }

        private byte[] copyData(Stream source)
        {
            MemoryStream destination = new MemoryStream();
            source.CopyTo(destination);
            return destination.ToArray();
        }
    }
}
