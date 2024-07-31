Imports System.Net
Imports System.Net.Http

Public Class HttpBotClient
    Inherits HttpClient

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(handler As HttpMessageHandler)
        MyBase.New(handler)
    End Sub

    Public Property AcceptAllCertificates As Boolean = True

    Public Shared Function SetupSession(ByVal BaseUrl As String, Optional ByVal Proxy As WebProxy = Nothing) As HttpBotClient
        Dim Base As Uri = New Uri("https://auth.skyairline.com/")
        Dim httpClientHandler = New HttpClientHandler With {
                .CookieContainer = New CookieContainer(),
                .UseCookies = True,
                .UseDefaultCredentials = True,
                .PreAuthenticate = True,
                .AllowAutoRedirect = True
            }

        If Proxy IsNot Nothing Then
            httpClientHandler.Proxy = Proxy
            httpClientHandler.UseProxy = True
        End If

        httpClientHandler.CookieContainer.Add(New Cookie("a", "1") With {
                .Domain = Base.Host
            })

        Dim session As HttpBotClient = New HttpBotClient(httpClientHandler)

        ServicePointManager.FindServicePoint(Base).ConnectionLeaseTimeout = TimeSpan.FromMinutes(1).TotalMilliseconds
        ServicePointManager.DnsRefreshTimeout = TimeSpan.FromMinutes(1).TotalMilliseconds
        System.Net.ServicePointManager.SecurityProtocol = Core.Globals.SecurityProtocol
        ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf session.AcceptAllCertifications)
        session.DefaultRequestHeaders.Add("User-Agent", Core.UserAgentGenerator.GetRandomUserAgent)
        session.DefaultRequestHeaders.Add("Cache-Control", "max-age=0")
        session.DefaultRequestHeaders.Add("Accept", "application/json, */*")
        session.DefaultRequestHeaders.Add("Cookie", "tdid=; asid=; did=; clid=")
        Return session
    End Function

    Private Function AcceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
        Return AcceptAllCertificates
    End Function

End Class
