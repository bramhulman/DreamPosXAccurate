Imports Newtonsoft.Json
Imports System.IO
Imports System.Net

Public Class clsStore

    Public Class Response
        Public Property s As Boolean
        Public Property d() As List(Of Di)

        Public Class Di
            Public Property trialEnd As String
            Public Property expired As Boolean
            Public Property bgColor As String
            Public Property licenseEnd As String
            Public Property [alias] As String
            Public Property logo As String
            Public Property admin As Boolean
            Public Property id As Integer
            Public Property accessibleUntil As String
            Public Property sample As Boolean
            Public Property logoUrl As String
            Public Property trial As Boolean
        End Class
    End Class

    Public Class ResponseBranchList
        Public Property s As Boolean
        Public Property d() As List(Of BranchData)

        Public Class BranchData
            Public Property id As Long
            Public Property name As String
        End Class
    End Class


    Public Class ResponseOpenDB
        Public Property s As Boolean
        Public Property d() As List(Of String)
        Public Property dataVersion As Long
        Public Property licenseEnd As String
        Public Property session As String
        Public Property host As String
        Public Property admin As Boolean
        Public Property accessibleUntil As String
        Public Property trial As Boolean
    End Class

    Public Function getStoreList(ByVal accessToken As String)
        Dim sUrl As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim respStore As New clsStore.Response
        Dim responseContent As String = ""
        Try

            sUrl = sAccountingUrl & "/api/db-list.do"
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            myWebReq = DirectCast(WebRequest.Create(sUrl), HttpWebRequest)

            With myWebReq
                .Headers.Add("Authorization: Bearer " & accessToken)
                .Method = "GET"
                .Timeout = 30000
            End With

            Try
                Dim sr As StreamReader

                myWebResp = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
                sr = New StreamReader(myWebResp.GetResponseStream())
                Dim StatusCode As String = ""
                StatusCode = myWebResp.StatusCode

                responseContent = sr.ReadToEnd()
                sr.Dispose()
                myWebResp.Close()



            Catch ex As WebException
                responseContent = ReadErrorResponse(ex)
            End Try

        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsStore.getStoreList")
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                respStore = JsonConvert.DeserializeObject(Of clsStore.Response)(responseContent)
                '  ErrLog(responseContent, " JsonResult", False)
                errLog(responseContent, " JsonResult")
            End If
        End If

        Return respStore
    End Function

    Public Function openDBStore(ByVal accessToken As String, ByVal storeID As String)
        Dim sUrl As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim respStore As New clsStore.ResponseOpenDB
        Dim responseContent As String = ""
        Try

            sUrl = sAccountingUrl & "/api/open-db.do?id=" & storeID
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            myWebReq = DirectCast(WebRequest.Create(sUrl), HttpWebRequest)

            With myWebReq
                .Headers.Add("Authorization: Bearer " & accessToken)
                .Method = "GET"
                .Timeout = 30000
            End With

            Try
                Dim sr As StreamReader

                myWebResp = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
                sr = New StreamReader(myWebResp.GetResponseStream())
                Dim StatusCode As String = ""
                StatusCode = myWebResp.StatusCode

                responseContent = sr.ReadToEnd()
                sr.Dispose()
                myWebResp.Close()



            Catch ex As WebException
                responseContent = ReadErrorResponse(ex)
            End Try

        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsStore.openDBStore")
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                respStore = JsonConvert.DeserializeObject(Of clsStore.ResponseOpenDB)(responseContent)
                '  ErrLog(responseContent, " JsonResult", False)
            End If
        End If

        Return respStore
    End Function

    Private Function ReadErrorResponse(ByVal ex As WebException) As String
        Try
            If ex.Response Is Nothing Then Return ex.Message

            Using myWebResp As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                Using sr As New StreamReader(myWebResp.GetResponseStream())
                    Return sr.ReadToEnd()
                End Using
            End Using
        Catch
            Return ex.Message
        End Try
    End Function


    Public Function getBranchList(ByVal accessToken As String, ByVal sessionID As String, ByVal hostUrl As String) As String
        Dim sUrl As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim responseContent As String = ""
        Try
            sUrl = hostUrl & "/accurate/api/branch/list.do"
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            myWebReq = DirectCast(WebRequest.Create(sUrl), HttpWebRequest)

            With myWebReq
                .Headers.Add("Authorization: Bearer " & accessToken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "GET"
                .Timeout = 30000
            End With

            Try
                Dim sr As StreamReader

                myWebResp = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
                sr = New StreamReader(myWebResp.GetResponseStream())
                responseContent = sr.ReadToEnd()
                sr.Dispose()
                myWebResp.Close()
            Catch ex As WebException
                responseContent = ReadErrorResponse(ex)
            End Try

        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsStore.getBranchList")
            Err.Clear()
        End Try

        If responseContent IsNot Nothing AndAlso responseContent <> "" Then
            errLog("API: " & sUrl & " | JSON Response: " & responseContent, "clsStore.getBranchList")
        End If

        Return responseContent
    End Function

End Class

