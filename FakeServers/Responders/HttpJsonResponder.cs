﻿using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace FakeServers.Responders
{
    public class HttpJsonResponder
    {
        public static HttpJsonResponder<T> Create<T>(T content)
        {
            return new HttpJsonResponder<T>(content);
        }
    }

    public class HttpJsonResponder<T> : IHttpResponder
    {
        public HttpJsonResponder(T content)
        {
            Content = content;
        }

        public T Content { get; private set; }

        public Encoding Encoding { get; set; }

        public void Write(HttpListenerResponse response)
        {
            Encoding encoding = Encoding ?? Encoding.Default;
            string json = JsonConvert.SerializeObject(Content);
            response.ContentLength64 = encoding.GetByteCount(json);
            StreamWriter writer = new StreamWriter(response.OutputStream, encoding);
            writer.Write(json);
            writer.Flush();
        }
    }
}
