using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace FakeServers.Extractors
{
    public class JsonBodyExtractor<T> : IRequestBodyExtractor
    {
        private readonly JsonSerializerSettings settings;

        public JsonBodyExtractor()
            : this(new JsonSerializerSettings())
        {
        }

        public JsonBodyExtractor(JsonSerializerSettings settings)
        {
            this.settings = settings;
        }

        public T Result { get; private set; }

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
            using (StreamReader reader = new StreamReader(request.InputStream))
            {
                string content = reader.ReadToEnd();
                Result = JsonConvert.DeserializeObject<T>(content, settings);
            }
        }
    }
}
