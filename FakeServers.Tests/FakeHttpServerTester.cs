using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FakeServers.Extractors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeServers.Tests
{
    [TestClass]
    public class FakeHttpServerTester
    {
        [TestMethod]
        public void ShouldRespondWithStringBasedOnConfiguration()
        {
            using (FakeHttpServer server = new FakeHttpServer())
            {
                server.Host = "localhost";
                server.Port = 8080;
                server.Path = "api/v1/accounts";

                server.ReturnString("Hello, World!!!");
                server.StatusCode = HttpStatusCode.OK;
                server.StatusDescription = "Okey, Dokey";
                server.Headers.Add("When", new DateTime(2014, 06, 16).ToString("MM/dd/yyyy"));

                StringBodyExtractor extractor = new StringBodyExtractor();
                server.UseBodyExtractor(extractor);

                server.Listen();

                WebRequest request = WebRequest.Create("http://localhost:8080/api/v1/accounts");
                request.Method = "POST";
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write("Hello, Universe!!!");
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Assert.AreEqual(server.StatusCode, response.StatusCode, "The wrong status code was returned.");
                    Assert.AreEqual(server.StatusDescription, response.StatusDescription, "The wrong status description was returned.");
                    Assert.AreEqual("06/16/2014", response.Headers["When"], "The wrong header was returned");
                    string responseText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    Assert.AreEqual("Hello, World!!!", responseText);
                    Assert.AreEqual("Hello, Universe!!!", extractor.Content);
                }
            }
        }

        [TestMethod]
        public void ShouldExtractUrlEncodedData()
        {
            using (FakeHttpServer server = new FakeHttpServer())
            {
                server.Host = "localhost";
                server.Port = 8080;
                server.Path = "api/v1/accounts";
                server.StatusCode = HttpStatusCode.OK;
                server.StatusDescription = "Success";

                UrlEncodedBodyExtractor extractor = new UrlEncodedBodyExtractor();
                server.UseBodyExtractor(extractor);

                server.Listen();

                WebRequest request = WebRequest.Create("http://localhost:8080/api/v1/accounts");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write("name=bob&age=31");
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    NameValueCollection collection = extractor.Parameters;
                    Assert.AreEqual("bob", collection.Get("name"), "Could not extract the name parameter.");
                    Assert.AreEqual("31", collection.Get("age"), "Could not extract the age parameter.");
                }
            }
        }

        [TestMethod]
        public void ShouldExtractMultiPartFormData()
        {
            using (FakeHttpServer server = new FakeHttpServer())
            {
                server.Host = "localhost";
                server.Port = 8080;
                server.Path = "api/v1/accounts";
                server.StatusCode = HttpStatusCode.OK;
                server.StatusDescription = "Success";

                MultiPartBodyExtractor extractor = new MultiPartBodyExtractor();
                server.UseBodyExtractor(extractor);

                server.Listen();

                WebRequest request = WebRequest.Create("http://localhost:8080/api/v1/accounts");
                request.Method = "POST";
                request.ContentType = "multipart/form-data; boundary=taco";
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write("--taco\r\n");
                    writer.Write("Content-Disposition: form-data; name=name\r\n");
                    writer.Write("\r\n");
                    writer.Write("bob\r\n");
                    writer.Write("--taco\r\n");
                    writer.Write("Content-Disposition: form-data; name=file; filename=help.txt\r\n");
                    writer.Write("Content-Type: text/plain\r\n");
                    writer.Write("\r\n");
                    writer.Write("These are the contents of the file.\r\n");
                    writer.Write("--taco--\r\n");
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    NameValueCollection collection = extractor.Parameters;
                    Assert.AreEqual("bob", collection.Get("name"), "Could not extract the name parameter.");
                    var files = extractor.Files.GetFiles("file");
                    Assert.AreEqual(1, files.Count(), "The wrong number of files were extracted.");
                    var file = files.First();
                    Assert.AreEqual("file", file.Name, "The name of the file part was not stored.");
                    Assert.AreEqual("help.txt", file.FileName, "The file name was not stored.");
                    Assert.AreEqual("text/plain", file.ContentType, "The file content type was not stored.");
                    Assert.AreEqual("These are the contents of the file.", Encoding.Default.GetString(file.Contents), "The file contents were wrong.");
                }
            }
        }
    }
}
