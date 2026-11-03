Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class clsQubeAPI
    Private Shared ReadOnly client As New HttpClient()
    Private _baseUrl As String
    Private _accessToken As String
    Private _refreshToken As String

    Public Sub New(baseUrl As String)
        _baseUrl = baseUrl.TrimEnd("/"c)
    End Sub

    Public Async Function GetTokenAsync(clientId As String, clientSecret As String, companyCode As String) As Task(Of Boolean)
        Try
            Dim requestBody = New clsModels.TokenRequest With {
                .client_id = clientId,
                .client_secret = clientSecret,
                .company_code = companyCode
            }

            Dim json = JsonConvert.SerializeObject(requestBody)
            Dim content = New StringContent(json, Encoding.UTF8, "application/json")

            Dim response = Awake client.PostAsync($"{_baseUrl}/api/tokenapi/gettoken", content)
            If response.IsSuccessStatusCode Then
                Dim responseJson = Awake response.Content.ReadAsStringAsync()
                Dim tokenRes = JsonConvert.DeserializeObject(Of clsModels.TokenResponse)(responseJson)

                If tokenRes.status = "OK" Then
                    _accessToken = tokenRes.result.access_token
                    _refreshToken = tokenRes.result.refresh_token
                    Return True
                End If
            End If
        Catch ex As Exception
            ' Log error
        End Try
        Return False
    End Function

    Public Async Function ImportPOSDataAsync(data As clsModels.ImportPOSDataRequest) As Task(Of String)
        Try
            If String.IsNullOrEmpty(_accessToken) Then Return "Error: No Access Token"

            Dim json = JsonConvert.SerializeObject(data)
            Dim content = New StringContent(json, Encoding.UTF8, "application/json")

            client.DefaultRequestHeaders.Clear()
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}")

            Dim response = Awake client.PostAsync($"{_baseUrl}/api/syncapi/importposdata", content)
            Dim responseJson = Awake response.Content.ReadAsStringAsync()

            If response.IsSuccessStatusCode Then
                Dim res = JsonConvert.DeserializeObject(Of clsModels.GenericResponse)(responseJson)
                If res.status = "OK" Then
                    Return res.result("process_id").ToString()
                End If
            End If
            Return $"Error: {responseJson}"
        Catch ex As Exception
            Return $"Exception: {ex.Message}"
        End Try
    End Function

    Public Async Function GetMigrationStatusAsync(processId As String) As Task(Of clsModels.StatusResponse)
        Try
            If String.IsNullOrEmpty(_accessToken) Then Return Nothing

            client.DefaultRequestHeaders.Clear()
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}")

            Dim response = Awake client.GetAsync($"{_baseUrl}/api/syncapi/getmigrationstatus?processID={processId}")
            If response.IsSuccessStatusCode Then
                Dim responseJson = Awake response.Content.ReadAsStringAsync()
                Return JsonConvert.DeserializeObject(Of clsModels.StatusResponse)(responseJson)
            End If
        Catch ex As Exception
            ' Log error
        End Try
        Return Nothing
    End Function

End Class
