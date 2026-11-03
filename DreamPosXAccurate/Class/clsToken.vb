Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome
Imports System.Collections.Specialized
Imports System.Text
Imports System.Threading
Imports System.Web

Public Class clsToken
    Class EntToken
        Public Property accessToken As String
        Public Property tokenType As String
        Public Property expiresIn As String
        Public Property userInfo As String
    End Class

    Class ProcessToken
        Public Function GetToken(ByVal clientID As String,
                                 ByVal userName As String,
                                 ByVal password As String) As String
            Dim entToken As New EntToken
            Try

                '765b69a3-4131-4c93-9b8c-9883a8ffccfa

                ' Set up Chrome WebDriver
                Dim driverPath As String = AppDomain.CurrentDomain.BaseDirectory
                Dim driverService = ChromeDriverService.CreateDefaultService(driverPath)
                driverService.HideCommandPromptWindow = True
                Dim options As New ChromeOptions()
                options.AddArguments("start-maximized") ' Maximize the browser window

                Using driver As New ChromeDriver(driverService, options)
                    ' Navigate to the login page
                    driver.Navigate().GoToUrl(BuildAuthorizeUrl(clientID))

                    ' Find the username and password fields and enter credentials
                    Dim usernameField = driver.FindElement(By.Id("email")) ' Replace with your username field ID
                    Dim passwordField = driver.FindElement(By.Id("password")) ' Replace with your password field ID
                    Dim loginButton = driver.FindElement(By.Id("btn-login")) ' Replace with your login button ID
                    '  driver.FindElement(By.Id())
                    usernameField.SendKeys(userName)
                    passwordField.SendKeys(password)

                    ' Click the login button
                    loginButton.Click()

                    ' You may need to wait for the login process to complete or for a redirect to occur
                    ' You can add waits here using driver.WaitForElement(), driver.Url, etc.

                    ' Once logged in, you can proceed with your desired actions, such as fetching data
                    ' ...

                    Dim queryParameters As NameValueCollection = WaitForToken(driver)

                    With entToken
                        .accessToken = queryParameters.Get("access_token")
                        .tokenType = queryParameters.Get("token_type")
                        .expiresIn = queryParameters.Get("expires_in")
                        .userInfo = queryParameters.Get("user")
                    End With

                    driver.Dispose()
                    Return entToken.accessToken
                End Using
            Catch ex As Exception
                errLog(ex.Message, "clsToken.GetToken")
                Err.Clear()
                Return ""
            End Try
        End Function

        Private Function WaitForToken(ByVal driver As ChromeDriver) As NameValueCollection
            Dim waitUntil As DateTime = DateTime.Now.AddSeconds(180)

            Do
                Try
                    Dim authorizeButton = driver.FindElement(By.Id("authorize-button"))
                    authorizeButton.Click()
                Catch ex As Exception
                    Err.Clear()
                End Try

                Dim uri As New Uri(driver.Url)
                Dim fragment As String = uri.Fragment.TrimStart("#"c)
                Dim queryParameters As NameValueCollection = HttpUtility.ParseQueryString(fragment)

                If queryParameters.Get("access_token") <> "" Then
                    Return queryParameters
                End If

                System.Windows.Forms.Application.DoEvents()
                Thread.Sleep(1000)
            Loop While DateTime.Now < waitUntil

            Throw New Exception("OAuth access_token tidak ditemukan setelah 180 detik. Pastikan halaman Accurate sudah login dan tombol Setuju/Authorize sudah diklik sampai redirect ke " & sOAuthRedirectUri & ". URL terakhir: " & driver.Url)
        End Function

        Private Function BuildAuthorizeUrl(ByVal clientID As String) As String
            Dim cleanClientID As String = clientID.Replace(vbCr, "").Replace(vbLf, "").Trim()
            Dim query As New StringBuilder()
            query.Append("client_id=").Append(HttpUtility.UrlEncode(cleanClientID))
            query.Append("&response_type=").Append(HttpUtility.UrlEncode(sOAuthResponseType))
            query.Append("&redirect_uri=").Append(HttpUtility.UrlEncode(sOAuthRedirectUri))
            query.Append("&scope=").Append(HttpUtility.UrlEncode(sOAuthScope))

            Return sAccountingUrl & "/oauth/authorize?" & query.ToString()
        End Function
    End Class


End Class

