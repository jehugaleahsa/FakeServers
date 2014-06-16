using System;
using System.IO;
using System.Net;
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

                server.Listen();

                WebRequest request = WebRequest.Create("http://localhost:8080/api/v1/accounts");
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Assert.AreEqual(server.StatusCode, response.StatusCode, "The wrong status code was returned.");
                    Assert.AreEqual(server.StatusDescription, response.StatusDescription, "The wrong status description was returned.");
                    Assert.AreEqual("06/16/2014", response.Headers["When"], "The wrong header was returned");
                    string responseText = getResponseString(response);
                    Assert.AreEqual("Hello, World!!!", responseText);
                }
            }
        }

        private static string getResponseString(HttpWebResponse response)
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
