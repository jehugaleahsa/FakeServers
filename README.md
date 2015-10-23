# Fake Servers

Simulate server interactions with pre-configured responses.

Download using NuGet: [FakeServers](http://www.nuget.org/packages/FakeServers/)

## Overview
Working with a remote API can be difficult to test. In some cases, hitting a remote server could cause undesired effects or even cost money. Worse, how does your code respond to different status codes or errors?

FakeServers aims at providing developers the ability to simulate interacting with an HTTP server. You can start up an actual HTTP server and specify what the response will be. You can override the status code, the headers and the content. The `FakeHttpServer` supports responding with strings, binary data or JSON out of the box.

In the following example, we test retrieving a simple string:

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

# Goals
I would like to support additional HTTP response formats.
I would like to eventually add support for other protocols: FTP, SMTP, AMQP, etc. 

## License
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
