Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports System.IO
Imports System.Net

Public Class clsPayment
    Public Class Request


        Public Property data() As List(Of Datum)

        Public Class Datum
            Public Property accountType As String
            Public Property asOf As String
            Public Property currencyCode As String
            Public Property name As String
            Public Property no As String

        End Class

    End Class

    Public Class RequestUpdate


        Public Property data() As List(Of Datum)

        Public Class Datum
            Public Property accountType As String
            Public Property asOf As String
            Public Property currencyCode As String
            Public Property name As String
            Public Property no As String
            Public Property id As Integer
        End Class

    End Class



    Public Class RequestDataSearch
        Public Property fields As String
        Public Property keywords As New List(Of filter)

        Public Class filter
            Public Property op As String
            Public Property val As List(Of String)
            Public Property sp As List(Of spsp)
        End Class

        Public Class spsp
            Public Property pageSize As Integer
        End Class
    End Class

    Public Class ResponseDataSearch


        Public Property s As Boolean
        Public Property d() As List(Of Dd)
        Public Property sp As Sps

        Public Class Sps
            Public Property page As Integer
            Public Property sort As Object
            Public Property pageSize As Integer
            Public Property pageCount As Integer
            Public Property rowCount As Integer
            Public Property start As Integer
            Public Property limit As Object
        End Class

        Public Class Dd
            Public Property name As String
            Public Property id As Integer
            Public Property no As String
        End Class

    End Class
    Public Class Response


        Public Property s As Boolean
        Public Property d() As List(Of Dd)

        Public Class Dd
            Public Property d 'As String
            Public Property s As Boolean
            '      Public Property r As R
        End Class



    End Class

    Public Sub InsertMissingPaymentMaster()
        Dim cCMD As New clsCMD(sConnection)
        Try
            With cCMD
                .resetCmd("INSERT INTO cld_syncpaymaster (flag, paycoaname, paycoa, custid, payno, payname) SELECT DISTINCT 0, '', '', custid, 0, payname FROM cld_sumpaymentdaily WHERE payname NOT IN (SELECT PayName FROM cld_syncpaymaster)")
                .executeNonQuery()
                .dispose()
            End With
        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsPayment: InsertMissingPaymentMaster")
            Err.Clear()
        End Try
    End Sub

    Private Sub LogBulkSaveResult(ByVal processName As String, ByVal accountNo As String, ByVal accountName As String, ByVal actionName As String, ByVal responseContent As String, ByVal resultTrans As clsPayment.Response)
        Try
            If responseContent Is Nothing OrElse responseContent.Trim() = "" Then
                errLog(actionName & " akun " & accountNo & " - " & accountName & " tidak mengembalikan response dari Accurate", processName)
                Return
            End If

            If resultTrans Is Nothing OrElse resultTrans.d Is Nothing OrElse resultTrans.d.Count = 0 Then
                errLog(actionName & " akun " & accountNo & " - " & accountName & " tidak bisa dibaca. Response: " & responseContent, processName)
                Return
            End If

            Dim firstResult = resultTrans.d(0)
            Dim detail As String = ""
            If firstResult.d IsNot Nothing Then
                detail = firstResult.d.ToString()
            End If

            If firstResult.s Then
                errLog(actionName & " akun berhasil: " & accountNo & " - " & accountName & ". Detail: " & detail, processName)
            ElseIf IsDuplicateAccountResponse(detail) Then
                errLog(actionName & " akun dilewati karena sudah tersedia di Accurate: " & accountNo & " - " & accountName & ". Detail: " & detail, processName)
            Else
                errLog(actionName & " akun GAGAL: " & accountNo & " - " & accountName & ". Detail: " & detail & ". Response: " & responseContent, processName)
            End If
        Catch ex As Exception
            errLog(ex.Message, processName & ".LogBulkSaveResult")
            Err.Clear()
        End Try
    End Sub

    Private Function IsDuplicateAccountResponse(ByVal detail As String) As Boolean
        If detail Is Nothing Then Return False
        Return detail.IndexOf("Sudah ada data lain dengan Nomor", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Sub LogBulkSaveSummary(ByVal processName As String, ByVal actionName As String, ByVal accounts As List(Of Request.Datum), ByVal responseContent As String, ByVal resultTrans As clsPayment.Response)
        Try
            If accounts Is Nothing OrElse accounts.Count = 0 Then
                errLog(actionName & " tidak memiliki akun untuk diproses", processName)
                Return
            End If

            If responseContent Is Nothing OrElse responseContent.Trim() = "" Then
                errLog(actionName & " tidak mengembalikan response dari Accurate. Jumlah akun: " & accounts.Count, processName)
                Return
            End If

            If resultTrans Is Nothing OrElse resultTrans.d Is Nothing OrElse resultTrans.d.Count = 0 Then
                errLog(actionName & " response tidak bisa dibaca. Jumlah akun: " & accounts.Count & ". Response: " & responseContent, processName)
                Return
            End If

            Dim successCount As Integer = 0
            Dim failedCount As Integer = 0
            Dim maxIndex As Integer = Math.Min(accounts.Count, resultTrans.d.Count)

            For idx As Integer = 0 To maxIndex - 1
                Dim account = accounts(idx)
                Dim itemResult = resultTrans.d(idx)
                Dim detail As String = ""
                If itemResult.d IsNot Nothing Then
                    detail = itemResult.d.ToString()
                End If

                If itemResult.s Then
                    successCount += 1
                    errLog(actionName & " berhasil: " & account.no & " - " & account.name & " (" & account.accountType & "). Detail: " & detail, processName)
                ElseIf IsDuplicateAccountResponse(detail) Then
                    successCount += 1
                    errLog(actionName & " dilewati karena akun sudah tersedia di Accurate: " & account.no & " - " & account.name & " (" & account.accountType & "). Detail: " & detail, processName)
                Else
                    failedCount += 1
                    errLog(actionName & " GAGAL: " & account.no & " - " & account.name & " (" & account.accountType & "). Detail: " & detail, processName)
                End If
            Next

            If resultTrans.d.Count <> accounts.Count Then
                errLog(actionName & " jumlah response Accurate (" & resultTrans.d.Count & ") berbeda dari jumlah akun request (" & accounts.Count & "). Response: " & responseContent, processName)
            End If

            errLog(actionName & " selesai. Berhasil: " & successCount & ", gagal: " & failedCount & ", total request: " & accounts.Count, processName)
        Catch ex As Exception
            errLog(ex.Message, processName & ".LogBulkSaveSummary")
            Err.Clear()
        End Try
    End Sub

    Public Function SaveAccount(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cTransSave As New Request
        Dim cTransUpdate As New RequestUpdate
        Dim listItemSave As New List(Of Request.Datum)
        Dim responseContent As String = ""
        Dim resultTrans2 As New ResponseDataSearch
        Dim resultTrans As New clsPayment.Response
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Try
            With cCMD
                ' Flag 2-bit: bit1=brand1 sudah sync, bit2=brand2 sudah sync
                ' Brand 1: ambil yg belum sync ke brand1 (flag=0 atau flag=2)
                ' Brand 2: ambil yg belum sync ke brand2 (flag=0 atau flag=1)
                Dim payFlagFilter As String
                If sGlobalActiveBrandId = 1 Then
                    payFlagFilter = "(flag = 0 OR flag = 2)"
                ElseIf sGlobalActiveBrandId = 2 Then
                    payFlagFilter = "(flag = 0 OR flag = 1)"
                Else
                    payFlagFilter = "flag = 0"
                End If
                .resetCmd("SELECT DISTINCT paycoa, paycoaname FROM cld_syncpaymaster WHERE " & payFlagFilter)
                ds = .selectDataset
                .dispose()
            End With

            If ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
                errLog("Mulai sync payment/account. Jumlah akun lokal yang perlu dicek: " & ds.Tables(0).Rows.Count, "clsPayment.SaveAccount")
                resultTrans2 = SearchPaymentAccount(accesstoken, sessionID, hostUrl, ds)
                For Each a As DataRow In ds.Tables(0).Rows

                    Dim foundData = (From b In resultTrans2.d
                                     Where b.no = a.Item("paycoa")
                                     Select b).FirstOrDefault()

                    If foundData IsNot Nothing Then
                        errLog("Akun payment sudah ada di Accurate, update data: " & a.Item("paycoa").ToString() & " - " & a.Item("paycoaname").ToString() & " (ID " & foundData.id & ")", "clsPayment.SaveAccount")
                        With cTransUpdate
                            .data = getAccountUpdate(a.Item("paycoaname"), a.Item("paycoa"), foundData.id)
                        End With

                        sJson = JsonConvert.SerializeObject(cTransUpdate)


                        Dim dataserialize As Byte() = encoding.GetBytes(sJson)
                        sURL = hostUrl & "/accurate/api/glaccount/bulk-save.do"
                        ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
                        myWebReq = DirectCast(WebRequest.Create(sURL), HttpWebRequest)

                        With myWebReq
                            .ContentType = "application/json"
                            .Headers.Add("Authorization: Bearer " & accesstoken)
                            .Headers.Add("X-Session-ID:" & sessionID)
                            .Method = "POST"
                            .Timeout = 30000
                        End With

                        Dim myStream As Stream = myWebReq.GetRequestStream()

                        With myStream
                            If dataserialize.Length > 0 Then
                                .Write(dataserialize, 0, dataserialize.Length)
                                .Close()
                            End If
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
                            If ex.Response IsNot Nothing Then
                                Dim myWebResp1 As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                                Dim sr1 As New StreamReader(myWebResp1.GetResponseStream())
                                Dim err As String = sr1.ReadToEnd()
                                responseContent = err
                            Else
                                responseContent = "{""s"":false,""d"":[{""d"":[""" & ex.Message.Replace("""", "\""") & """],""s"":false}]}"
                            End If
                            errLog(ex.Message, "clsPayment.SaveAccount " & responseContent.ToString)

                        End Try


                        If responseContent IsNot Nothing Then
                            If responseContent <> "" Then
                                resultTrans = JsonConvert.DeserializeObject(Of clsPayment.Response)(responseContent)
                                LogBulkSaveResult("clsPayment.SaveAccount", a.Item("paycoa").ToString(), a.Item("paycoaname").ToString(), "Update payment/account", responseContent, resultTrans)
                            End If
                        End If

                        Dim b = resultTrans.d

                        With cCMD
                            ' Update flag dengan OR-bit: brand1→bit1, brand2→bit2
                            Dim payBit As Integer = If(sGlobalActiveBrandId = 2, 2, 1)
                            .resetCmd("UPDATE cld_syncpaymaster SET flag = (flag | " & payBit & ") WHERE paycoaname='" & a.Item("paycoaname") & "'")
                            .executeNonQuery()
                            .dispose()

                            If b(0).s = False Then
                                .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('" & a.Item("paycoaname") & "','" & SafeSqlText(b(0).d(0).ToString) & "','failed','" & Now.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Paymode' ,'" & solid & "')")
                                .executeNonQuery()
                                .dispose()
                            End If
                        End With



                    Else
                        errLog("Akun payment belum ada di Accurate, create data: " & a.Item("paycoa").ToString() & " - " & a.Item("paycoaname").ToString(), "clsPayment.SaveAccount")
                        With cTransSave
                            .data = getAccount(a.Item("paycoaname"), a.Item("paycoa"))
                        End With

                        sJson = JsonConvert.SerializeObject(cTransSave)


                        Dim dataserialize As Byte() = encoding.GetBytes(sJson)
                        sURL = hostUrl & "/accurate/api/glaccount/bulk-save.do"
                        ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
                        myWebReq = DirectCast(WebRequest.Create(sURL), HttpWebRequest)

                        With myWebReq
                            .ContentType = "application/json"
                            .Headers.Add("Authorization: Bearer " & accesstoken)
                            .Headers.Add("X-Session-ID:" & sessionID)
                            .Method = "POST"
                            .Timeout = 30000
                        End With

                        Dim myStream As Stream = myWebReq.GetRequestStream()

                        With myStream
                            If dataserialize.Length > 0 Then
                                .Write(dataserialize, 0, dataserialize.Length)
                                .Close()
                            End If
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
                            If ex.Response IsNot Nothing Then
                                Dim myWebResp1 As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                                Dim sr1 As New StreamReader(myWebResp1.GetResponseStream())
                                Dim err As String = sr1.ReadToEnd()
                                responseContent = err
                            Else
                                responseContent = "{""s"":false,""d"":[{""d"":[""" & ex.Message.Replace("""", "\""") & """],""s"":false}]}"
                            End If
                        End Try


                        If responseContent IsNot Nothing Then
                            If responseContent <> "" Then
                                resultTrans = JsonConvert.DeserializeObject(Of clsPayment.Response)(responseContent)
                                LogBulkSaveResult("clsPayment.SaveAccount", a.Item("paycoa").ToString(), a.Item("paycoaname").ToString(), "Create payment/account", responseContent, resultTrans)
                            End If
                        End If

                        Dim b = resultTrans.d
                        If b(0).s = True Then
                            With cCMD
                                ' Update flag dengan OR-bit: brand1→bit1, brand2→bit2
                                Dim payBit2 As Integer = If(sGlobalActiveBrandId = 2, 2, 1)
                                .resetCmd("UPDATE cld_syncpaymaster SET flag = (flag | " & payBit2 & ") WHERE paycoaname='" & a.Item("paycoaname") & "'")
                                .executeNonQuery()
                                .dispose()
                            End With
                        End If
                    End If

                Next
                errLog("Selesai sync payment/account", "clsPayment.SaveAccount")
            Else
                errLog("Tidak ada payment/account baru yang perlu disync dari cld_syncpaymaster", "clsPayment.SaveAccount")
            End If





        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsPayment: save account")
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                resultTrans = JsonConvert.DeserializeObject(Of clsPayment.Response)(responseContent)
                errLog(responseContent, " JsonResult")
            End If
        End If

        Return resultTrans

    End Function


    Public Function getAccount(ByVal paycoaName As String, ByVal payCoa As String) 'As List(Of Request.Datum)
        '  Dim cCMD As New clsCMD(connUrl)
        ' Dim ds As New DataSet
        Dim listData As New List(Of Request.Datum)
        Dim SalesID As String = ""
        Try
            '   With cCMD

            With listData
                .Add(New Request.Datum With
                          {
                            .accountType = "CASH_BANK",
                            .asOf = Now.ToString("dd/MM/yyyy"),
                            .currencyCode = "IDR",
                            .name = paycoaName,
                            .no = payCoa
                          })
            End With

            ' End With
        Catch ex As Exception
            errLog(Err.Description, "clsPayment.getAccount ")
            Err.Clear()
        End Try
        Return listData
    End Function

    Public Function SaveJournalAccounts(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String)
        Dim resultTrans As New clsPayment.Response
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Dim responseContent As String = ""

        Try
            With cCMD
                .resetCmd("SELECT no, MIN(name) name, accountType FROM (" &
                          "SELECT majorcoa no, CONCAT('Penjualan ', name) name, 'REVENUE' accountType FROM cld_majorgroup WHERE majorcoa IS NOT NULL AND majorcoa <> '' " &
                          "UNION ALL SELECT coa no, name, CASE " &
                          "WHEN name IN ('PB1','service') THEN 'OTHER_CURRENT_LIABILITY' " &
                          "WHEN name = 'rounding' THEN 'EXPENSE' " &
                          "ELSE 'REVENUE' END accountType FROM cld_syncother WHERE coa IS NOT NULL AND coa <> '' " &
                          "UNION ALL SELECT paycoa no, paycoaname name, 'CASH_BANK' accountType FROM cld_syncpaymaster WHERE paycoa IS NOT NULL AND paycoa <> '' " &
                          ") journal_accounts GROUP BY no, accountType")
                ds = .selectDataset()
                .dispose()
            End With

            If ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then
                errLog("Tidak ada akun jurnal yang perlu dicek dari mapping lokal", "clsPayment.SaveJournalAccounts")
                Return resultTrans
            End If

            errLog("Mulai cek akun jurnal. Jumlah akun mapping lokal: " & ds.Tables(0).Rows.Count, "clsPayment.SaveJournalAccounts")

            Dim accurateAccounts As ResponseDataSearch = SearchPaymentAccount(accesstoken, sessionID, hostUrl, ds)
            Dim existingAccounts = If(accurateAccounts.d, New List(Of ResponseDataSearch.Dd))
            Dim missingAccounts As New List(Of Request.Datum)
            Dim existingCount As Integer = 0

            For Each row As DataRow In ds.Tables(0).Rows
                Dim accountNo As String = row.Item("no").ToString()
                Dim foundData = (From account In existingAccounts
                                 Where account.no = accountNo
                                 Select account).FirstOrDefault()

                If foundData Is Nothing Then
                    missingAccounts.Add(New Request.Datum With {
                        .accountType = row.Item("accountType").ToString(),
                        .asOf = Now.ToString("dd/MM/yyyy"),
                        .currencyCode = "IDR",
                        .name = row.Item("name").ToString(),
                        .no = accountNo
                    })
                Else
                    existingCount += 1
                End If
            Next

            errLog("Hasil cek akun jurnal. Sudah ada: " & existingCount & ", perlu dibuat: " & missingAccounts.Count, "clsPayment.SaveJournalAccounts")

            If missingAccounts.Count = 0 Then
                errLog("Semua akun jurnal sudah tersedia di Accurate", "clsPayment.SaveJournalAccounts")
                Return resultTrans
            End If

            errLog("Mulai create akun jurnal: " & String.Join(", ", missingAccounts.Select(Function(account) account.no & " - " & account.name).ToArray()), "clsPayment.SaveJournalAccounts")

            Dim cTransSave As New Request With {.data = missingAccounts}
            Dim sJson As String = JsonConvert.SerializeObject(cTransSave)
            Dim encoding As New System.Text.UTF8Encoding
            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            Dim sURL As String = hostUrl & "/accurate/api/glaccount/bulk-save.do"

            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            Dim myWebReq As HttpWebRequest = DirectCast(WebRequest.Create(sURL), HttpWebRequest)
            With myWebReq
                .ContentType = "application/json"
                .Headers.Add("Authorization: Bearer " & accesstoken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "POST"
                .Timeout = 30000
            End With

            Using myStream As Stream = myWebReq.GetRequestStream()
                If dataserialize.Length > 0 Then
                    myStream.Write(dataserialize, 0, dataserialize.Length)
                End If
            End Using

            Try
                Using myWebResp As HttpWebResponse = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
                    Using sr As New StreamReader(myWebResp.GetResponseStream())
                        responseContent = sr.ReadToEnd()
                    End Using
                End Using
            Catch ex As WebException
                If ex.Response IsNot Nothing Then
                    Using myWebResp As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                        Using sr As New StreamReader(myWebResp.GetResponseStream())
                            responseContent = sr.ReadToEnd()
                        End Using
                    End Using
                Else
                    responseContent = ex.Message
                End If
            End Try

            If responseContent <> "" Then
                resultTrans = JsonConvert.DeserializeObject(Of clsPayment.Response)(responseContent)
                LogBulkSaveSummary("clsPayment.SaveJournalAccounts", "Create akun jurnal", missingAccounts, responseContent, resultTrans)
            End If
        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsPayment.SaveJournalAccounts " & responseContent)
            Err.Clear()
        Finally
            cCMD.dispose()
        End Try

        Return resultTrans
    End Function

    Public Function getAccountUpdate(ByVal paycoaName As String, ByVal payCoa As String, ByVal id As Integer) 'As List(Of Request.Datum)
        '  Dim cCMD As New clsCMD(connUrl)
        ' Dim ds As New DataSet
        Dim listData As New List(Of RequestUpdate.Datum)
        Dim SalesID As String = ""
        Try
            '   With cCMD

            With listData
                .Add(New RequestUpdate.Datum With
                          {
                            .accountType = "CASH_BANK",
                            .asOf = Now.ToString("dd/MM/yyyy"),
                            .currencyCode = "IDR",
                            .name = paycoaName,
                            .no = payCoa,
                            .id = id
                          })
            End With

            ' End With
        Catch ex As Exception
            errLog(Err.Description, "clsPayment.getAccountUpdate ")
            Err.Clear()
        End Try
        Return listData
    End Function

    Public Function getkeywords(ByVal ds As DataSet) 'As List(Of Request.Datum)
        ' Dim cCMD As New clsCMD(connUrl)
        '    Dim ds As New DataSet
        Dim listData As New List(Of RequestDataSearch.filter)
        Dim listVal As New List(Of String)
        Dim SalesID As String = ""
        Try

            '.resetCmd("SELECT * FROM cld_syncpaymaster where payno=25")
            'ds = .selectDataset()
            '.dispose()

            For Each dtrw As DataRow In ds.Tables(0).Rows
                With listVal
                    .Add(dtrw.Item("paycoaname"))
                End With
                With listData
                    .Add(New RequestDataSearch.filter With
                          {
                           .op = "EQUAL",
                            .val = listVal,
                            .sp = getSp()
                          })
                End With

            Next


        Catch ex As Exception
            errLog(Err.Description, "clsPayment.getkeywords ")
            Err.Clear()
        End Try
        Return listData
    End Function


    Public Function getSp() 'As List(Of Request.Datum)
        ' Dim cCMD As New clsCMD(connUrl)
        '    Dim ds As New DataSet
        Dim listData As New List(Of RequestDataSearch.spsp)
        Dim listVal As New List(Of String)
        Dim SalesID As String = ""
        Try

            '.resetCmd("SELECT * FROM cld_syncpaymaster where payno=25")
            'ds = .selectDataset()
            '.dispose()

            'For Each dtrw As DataRow In ds.Tables(0).Rows

            With listData
                .Add(New RequestDataSearch.spsp With
                          {
                          .pageSize = 100
                          })
            End With

            'Next


        Catch ex As Exception

        End Try
        Return listData
    End Function

    Public Function SearchPaymentAccount(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal ds As DataSet)
        Dim resultTrans As New clsPayment.ResponseDataSearch
        Dim allData As New List(Of ResponseDataSearch.Dd)
        Dim currentPage As Integer = 1
        Dim totalPages As Integer = 1

        Try
            Do
                Dim sURL As String = hostUrl & "/accurate/api/glaccount/list.do" & "?sp.page=" & currentPage & "&sp.pageSize=100&fields=id,name,no"
                Dim responseContent As String = ""
                
                ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
                Dim myWebReq As HttpWebRequest = DirectCast(WebRequest.Create(sURL), HttpWebRequest)

                With myWebReq
                    .Headers.Add("Authorization: Bearer " & accesstoken)
                    .Headers.Add("X-Session-ID:" & sessionID)
                    .Method = "GET"
                    .Timeout = 30000
                End With

                Try
                    Dim myWebResp As HttpWebResponse = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
                    Using sr As New StreamReader(myWebResp.GetResponseStream())
                        responseContent = sr.ReadToEnd()
                    End Using
                    myWebResp.Close()
                Catch ex As WebException
                    If ex.Response IsNot Nothing Then
                        Using sr As New StreamReader(ex.Response.GetResponseStream())
                            responseContent = sr.ReadToEnd()
                        End Using
                    End If
                End Try

                If responseContent <> "" Then
                    Dim pageResult = JsonConvert.DeserializeObject(Of clsPayment.ResponseDataSearch)(responseContent)
                    If pageResult IsNot Nothing AndAlso pageResult.d IsNot Nothing Then
                        allData.AddRange(pageResult.d)
                        If pageResult.sp IsNot Nothing Then
                            totalPages = pageResult.sp.pageCount
                        End If
                    End If
                    If currentPage = 1 Then
                        errLog(responseContent, " JsonResult")
                    End If
                End If

                currentPage += 1
            Loop While currentPage <= totalPages

            resultTrans.d = allData
            resultTrans.s = True

        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsPayment: SearchPaymentAccount")
            Err.Clear()
        End Try

        Return resultTrans
    End Function

    Private Function SafeSqlText(ByVal value As String) As String
        If value Is Nothing Then Return ""
        Dim result As String = value.Replace("'", "''")
        If result.Length > 255 Then result = result.Substring(0, 255)
        Return result
    End Function

End Class
