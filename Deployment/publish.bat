C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild ../FakeServers.sln /p:Configuration=Release
nuget pack ../FakeServers/FakeServers.csproj -Properties Configuration=Release
nuget push *.nupkg
del *.nupkg