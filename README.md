# Fake Servers

Simulate server interactions with pre-configured responses.

Download using NuGet: [FakeServers](http://www.nuget.org/packages/FakeServers/)

## Overview
Working with a remote API can be difficult to test. In some cases, hitting a remote server could cause undesired effects or even cost money. Worse, how does your code respond to different status codes?

FakeServers aims at providing developers the ability to simulate interacting with an HTTP server. You can start up an actual HTTP server and specify what the response will be. You can override the status code, the headers and the content. The `FakeHttpServer` supports responding with strings, binary data or JSON out of the box.

For example, this code tests retrieving a simple string:

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
