Imports DreamPosXAccurate.clsTransactions.Request
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Serialization
Imports OpenQA.Selenium
Imports System.IO
Imports System.Net

Public Class clsTransactions
    Private Function ReadWebExceptionResponse(ByVal ex As WebException) As String
        If ex Is Nothing Then Return ""

        Try
            If ex.Response Is Nothing Then
                Return ex.Message
            End If

            Using resp As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                Using sr As New StreamReader(resp.GetResponseStream())
                    Dim body As String = sr.ReadToEnd()
                    If body <> "" Then Return body
                    Return ex.Message
                End Using
            End Using
        Catch readEx As Exception
            Return ex.Message
        End Try
    End Function

    Public Class Request


        Public Property data() As List(Of Datum)


        Public Class Datum
            Public Property customerNo As String
            Public Property detailItem() As List(Of Detailitem)
            Public Property transDate As String
            <JsonProperty(NullValueHandling:=NullValueHandling.Ignore)> Public Property branchName As String
            Public Property inclusiveTax As Boolean
            Public Property number As String
            Public Property taxable As Boolean
            Public Property saveAsStatusType As String
            '  Public Property cashDiscount As Integer
            Public Property detailExpense As List(Of Detailexpense)
        End Class

        Public Class Detailitem
            Public Property itemNo As String
            Public Property unitPrice As Decimal
            Public Property itemUnitName As String
            Public Property quantity As Decimal
            Public Property useTax1 As Boolean
            Public Property warehouseName As String
        End Class

        Public Class Detailexpense
            Public Property accountNo As String

            Public Property expenseAmount As Decimal
            Public Property expenseName As String
            Public Property departmentName As String
        End Class


    End Class

    Public Class RequestJU


        Public Property data() As List(Of Datum)
        Public Class Datum


            Public Property detailJournalVoucher() As List(Of detailJournalVoucher)
            Public Property transDate As String
            <JsonProperty(NullValueHandling:=NullValueHandling.Ignore)> Public Property branchName As String
            'Public Property inclusiveTax As Boolean
            Public Property number As String
            'Public Property taxable As Boolean
            Public Property description As String
            '  Public Property cashDiscount As Integer
            'Public Property detailExpense As List(Of Detailexpense)
        End Class
        Public Class detailJournalVoucher
            Public Property accountNo As String
            Public Property amount As Decimal
            Public Property amountType As String
        End Class


    End Class

    Public Class ResponseSaveTransSumSalesDaily


        Public Property s As Boolean
        Public Property d() As List(Of Dd)

        Public Class Dd
            Public Property d As JToken 'List(Of String)
            Public Function GetValue_d() As String
                If d Is Nothing Then
                    Return Nothing
                End If

                Dim value_d As String = Nothing


                If d.Type = JTokenType.Array AndAlso d.HasValues Then
                    value_d = d.First.Value(Of String)()
                Else
                    value_d = d.Value(Of String)()
                End If

                Return value_d
            End Function

            Public Property s As Boolean
            'Public Property r As Rr
        End Class



    End Class
    Public Class ResponseSaveTransReceiptSumSalesDaily


        Public Property s As Boolean
        Public Property d() As List(Of Dd)

        Public Class Dd
            Public Property d As JToken
            Public Function GetValue_d() As String
                If d Is Nothing Then
                    Return Nothing
                End If

                If d.Type = JTokenType.Array AndAlso d.HasValues Then
                    Return d.First.ToString()
                End If

                Return d.ToString()
            End Function
            Public Property s As Boolean
            Public Property r As Rr
        End Class
        Public Class Rr
            Public Property charField8 As String
            Public Property charField9 As String
            Public Property charField6 As String
            Public Property charField7 As String

        End Class



    End Class
    Public Class Response
        'Public Property s As Boolean
        'Public Property d() As List(Of Dd)

        'Public Class Dd
        '    Public Property d As String
        '    Public Property s As Boolean
        '    '      Public Property r As R
        'End Class

    End Class
    Public Class RequestSalesReceipt

        Public Property data() As List(Of Datum)

        Public Class Datum
            Public Property bankNo As String
            Public Property chequeAmount As Double
            Public Property customerNo As String
            Public Property detailInvoice() As List(Of Detailinvoice)
            Public Property transDate As String
            Public Property currencyCode As String
            Public Property paymentMethod As String
            Public Property number As String
            <JsonProperty(NullValueHandling:=NullValueHandling.Ignore)> Public Property branchName As String

        End Class

        Public Class Detailinvoice
            Public Property invoiceNo As String
            Public Property paymentAmount As Double
            <JsonIgnore>
            Public Property detailDiscount() As List(Of Detaildiscount)

        End Class

        Public Class Detaildiscount
            Public Property accountNo As String
            Public Property amount As Double


        End Class

    End Class


    Public Function getDetailItem(ByVal connUrl As String, ByVal smno As String) As List(Of Request.Detailitem)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of Request.Detailitem)
        Dim SalesID As String = ""

        Try
            With cCMD
                .resetCmd("select * from cld_salesdetail where smno='" & smno & "' and itemgroup <> 'Discount' and price > 0 and qty > 0")
                ds = .selectDataset
                .dispose()

                For Each dtrw As DataRow In ds.Tables(0).Rows

                    With listData
                        .Add(New Request.Detailitem With
                                         {
                                             .itemNo = dtrw.Item("itemNo"),
                                             .itemUnitName = "PCS",
                                             .quantity = dtrw.Item("qty"),
                                             .unitPrice = dtrw.Item("price"),
                                             .warehouseName = "",
                                             .useTax1 = True
                                         })


                    End With
                Next
            End With

        Catch ex As Exception

        End Try
        Return listData
    End Function
    Public Function getDetailItemSalesSumDaily(ByVal connUrl As String, ByVal dFrom As String, ByVal sOlNumber As String) As List(Of Request.Detailitem)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of Request.Detailitem)
        Dim SalesID As String = ""

        Try
            With cCMD
                .resetCmd("SELECT itemno, SUM(qty) qty, price " &
                          "FROM cld_sumsalesdetaildaily " &
                          "WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' " &
                          "AND Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' " &
                          "AND itemno <> 'Disc Amt' AND Price >= 0 and qty > 0 AND olid = " & solid & " AND olnumber = " & sOlNumber & " " &
                          "GROUP BY itemno, price " &
                          "ORDER BY itemno")
                ds = .selectDataset
                .dispose()

                For Each dtrw As DataRow In ds.Tables(0).Rows

                    With listData
                        .Add(New Request.Detailitem With
                                         {
                                             .itemNo = dtrw.Item("itemNo"),
                                             .itemUnitName = "PCS",
                                             .quantity = dtrw.Item("qty"),
                                             .unitPrice = dtrw.Item("price"),
                                             .warehouseName = "",
                                             .useTax1 = True
                                         })


                    End With
                Next
            End With

        Catch ex As Exception

        End Try
        Return listData
    End Function
    Public Function getDetailexpense(ByVal a As DataRow) As List(Of Request.Detailexpense)
        Dim listData As New List(Of Request.Detailexpense)
        Try
            With listData
                If a.Item("RoundOff") <> 0 Then
                    Dim roundVal As Double = CDbl(a.Item("roundoff"))
                    .Add(New Request.Detailexpense With
                     {
                         .accountNo = rounding_coa,
                         .expenseAmount = roundVal,
                         .expenseName = "Short/ Over",
                         .departmentName = ""
                     })
                End If

                If a.Item("Discount") <> 0 Then
                    .Add(New Request.Detailexpense With
                   {
                       .accountNo = Discount_coa,
                       .expenseAmount = -a.Item("discount"),
                       .expenseName = "Sales Discount Food",
                       .departmentName = ""
                   })
                End If
            End With
        Catch ex As Exception

        End Try
        Return listData
    End Function

    'Public Function getDetailItem2(ByVal connUrl As String, ByVal smNo As String) As List(Of Request2.Detailitem2)
    '    Dim cCMD As New clsCMD(connUrl)
    '    Dim ds As New DataSet
    '    Dim listData As New List(Of Request2.Detailitem2)
    '    Dim SalesID As String = ""

    '    Try
    '        With cCMD
    '            .resetCmd("select * from cld_salesdetail where smno='" & smNo & "' and itemgroup <> 'Discount' and price > 0")
    '            ds = .selectDataset
    '            .dispose()

    '            For Each dtrw As DataRow In ds.Tables(0).Rows

    '                With listData
    '                    .Add(New Request2.Detailitem2 With
    '                                     {
    '                                         .itemNo = dtrw.Item("itemNo"),
    '                                         .itemUnitName = "PCS",
    '                                         .quantity = dtrw.Item("qty"),
    '                                         .unitPrice = dtrw.Item("price"),
    '                                         .warehouseName = ""
    '                                     })


    '                End With
    '            Next
    '        End With

    '    Catch ex As Exception

    '    End Try
    '    Return listData
    'End Function

    'Public Function getDetailserialnumber() As List(Of Request.Detailserialnumber)
    '    '  Dim cCMD As New clsCMD(connUrl)
    '    Dim ds As New DataSet
    '    Dim listData As New List(Of Request.Detailserialnumber)
    '    Dim SalesID As String = ""
    '    Try

    '        With listData
    '            .Add(New Request.Detailserialnumber With
    '                         {
    '                           .id = 0,
    '                           .quantity = 0,
    '                           .expiredDate = "",
    '                           .serialNumberNo = 0,
    '                           .status = ""
    '                         })


    '        End With
    '    Catch ex As Exception

    '    End Try
    '    Return listData
    'End Function

    Public Function getInvoiceDetail(ByVal smNo As String, payAmt As String) As List(Of RequestSalesReceipt.Detailinvoice)
        '  Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of RequestSalesReceipt.Detailinvoice)
        Dim SalesID As String = ""
        Try

            With listData
                .Add(New RequestSalesReceipt.Detailinvoice With
                             {
                             .invoiceNo = smNo,
                             .paymentAmount = payAmt
                             })


            End With
        Catch ex As Exception

        End Try
        Return listData
    End Function

    Public Function getDetailDisc(ByVal smNo As String, payAmt As String) As List(Of RequestSalesReceipt.Detaildiscount)
        '  Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of RequestSalesReceipt.Detaildiscount)
        Dim SalesID As String = ""
        Try

            With listData
                .Add(New RequestSalesReceipt.Detaildiscount With
                             {
                           .accountNo = "1235",
                           .amount = 0
                             })


            End With
        Catch ex As Exception

        End Try
        Return listData
    End Function


    Public Function SaveTrans(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal connUrl As String, ByVal dFrom As String)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cTransSave As New Request
        Dim listItemSave As New List(Of Request.Datum)
        Dim responseContent As String = ""
        Dim resultTrans As New clsItem.Response
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Try
            With cTransSave
                .data = getTrans(connUrl, dFrom)
            End With

            If cTransSave.data Is Nothing OrElse cTransSave.data.Count = 0 Then
                errLog("Tidak ada data faktur penjualan untuk tanggal " & CDate(dFrom).ToString("yyyy-MM-dd") & ". Request ke Accurate dibatalkan.", "clsTransactions.SaveTrans")
                Return resultTrans
            End If

            errLog("Mulai sync faktur penjualan tanggal " & CDate(dFrom).ToString("yyyy-MM-dd") & ". Jumlah invoice: " & cTransSave.data.Count, "clsTransactions.SaveTrans")

            '    Dim settings As New JsonSerializerSettings With {
            '    .ContractResolver = New Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            '    .ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            '    .NullValueHandling = NullValueHandling.Ignore
            '}
            sJson = JsonConvert.SerializeObject(cTransSave)


            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            sURL = hostUrl & "/accurate/api/sales-invoice/bulk-save.do"
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            myWebReq = DirectCast(WebRequest.Create(sURL), HttpWebRequest)

            With myWebReq
                .ContentType = "application/json"
                .Headers.Add("Authorization: Bearer " & accesstoken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "POST"
                .Timeout = 60000
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
                responseContent = ReadWebExceptionResponse(ex)
                errLog("Sync faktur penjualan gagal request ke Accurate. Detail: " & responseContent, "clsTransactions.SaveTrans")
            End Try

        Catch ex As Exception
            errLog(ex.Message, "clsTransactions.SaveTrans")
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                Try
                    resultTrans = JsonConvert.DeserializeObject(Of clsItem.Response)(responseContent)
                Catch jsonEx As Exception
                    errLog("Sync faktur penjualan gagal membaca response Accurate. Detail: " & jsonEx.Message & " | Response: " & responseContent, "clsTransactions.SaveTrans")
                    Return resultTrans
                End Try

                errLog("Response sync faktur penjualan: " & responseContent, "clsTransactions.SaveTrans")

                If resultTrans.s Then
                    With cCMD
                        .resetCmd("")
                    End With
                End If
                '  ErrLog(responseContent, " JsonResult", False)
            End If
        End If

        Return resultTrans

    End Function

    Public Function getTrans(ByVal connUrl As String, ByVal dfrom As String) 'As List(Of Request.Datum)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of Request.Datum)
        Dim SalesID As String = ""
        Dim sBranchName As String = ""
        Try
            Try
                cCMD.resetCmd("SELECT kode2 FROM cld_outlet WHERE ID = " & solid)
                sBranchName = cCMD.executeScaler()
            Catch ex As Exception
                sBranchName = ""
            End Try
            If String.IsNullOrEmpty(sBranchName) Then
                errLog("Cabang belum didaftar di cld_outlet.kode2 untuk olid " & solid & ". Proses di-skip.", "clsTransactions.getTrans")
                Return listData
            End If
            With cCMD
                .resetCmd("SELECT * FROM cld_salesmaster where flag = 1 and  transdate >= '" & CDate(dfrom).ToString("yyyy-MM-dd") & " 00:01' and transdate <= '" & CDate(dfrom).ToString("yyyy-MM-dd") & " 23:59'")
                ds = .selectDataset()
                .dispose()

                For Each dtrw As DataRow In ds.Tables(0).Rows
                    Dim currentDetailItem = getDetailItem(connUrl, dtrw.Item("number"))
                    If CDbl(dtrw.Item("service")) <> 0 Then
                        currentDetailItem.Add(New Request.Detailitem With
                            {
                                .itemNo = "100001",
                                .itemUnitName = "PCS",
                                .quantity = 1,
                                .unitPrice = dtrw.Item("service"),
                                .warehouseName = "",
                                .useTax1 = True
                            })
                    End If

                    With listData
                        .Add(New Request.Datum With
                      {
                        .customerNo = customer_no,
                        .detailItem = currentDetailItem,
                        .branchName = sBranchName,
                        .inclusiveTax = False,
                        .number = dtrw.Item("number"),
                        .transDate = CDate(dfrom).ToString("dd/MM/yyyy"),
                        .saveAsStatusType = "UNAPPROVED",
                        .detailExpense = getDetailexpense(dtrw)
                      })
                    End With
                Next
            End With
        Catch ex As Exception

        End Try
        Return listData
    End Function

    Public Function getTransReceipt(ByVal connUrl As String, ByVal dfrom As String) As List(Of RequestSalesReceipt.Datum)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of RequestSalesReceipt.Datum)
        Dim SalesID As String = ""
        Dim paymentNo As String = ""
        Dim smNo As String = ""
        Dim ds2 As New DataSet
        Dim sBranchName As String = ""
        Try
            Try
                cCMD.resetCmd("SELECT kode2 FROM cld_outlet WHERE ID = " & solid)
                sBranchName = cCMD.executeScaler()
            Catch ex As Exception
                sBranchName = ""
            End Try
            If String.IsNullOrEmpty(sBranchName) Then
                errLog("Cabang belum didaftar di cld_outlet.kode2 untuk olid " & solid & ". Proses di-skip.", "clsTransactions.getTransReceipt")
                Return listData
            End If
            With cCMD

                .resetCmd("SELECT number FROM cld_salesmaster where flag = 1 and  transdate >= '" & CDate(dfrom).ToString("yyyy-MM-dd") & " 00:01' and transdate <= '" & CDate(dfrom).ToString("yyyy-MM-dd") & " 23:59'")
                ds2 = .selectDataset
                .dispose()

                For Each a As DataRow In ds2.Tables(0).Rows
                    smNo = a.Item("number")

                    .resetCmd("select * from cld_payment where smno='" & smNo & "'")
                    ds = .selectDataset()
                    .dispose()

                    '.resetCmd("SELECT paycoa FROM cld_syncpaymaster WHERE payname = (SELECT paymentname FROM cld_payment WHERE smno='" & smNo & "')")
                    'paymentNo = .executeScaler
                    '.dispose()

                    For Each dtrw As DataRow In ds.Tables(0).Rows
                        .resetCmd("SELECT paycoa FROM cld_syncpaymaster WHERE payname = '" & dtrw.Item("paymentname") & "'")
                        paymentNo = .executeScaler
                        .dispose()
                        If dtrw.Item("paymentname") = "CASH" Then
                            With listData
                                .Add(New RequestSalesReceipt.Datum With
                              {
                                .customerNo = customer_no,
                              .branchName = sBranchName,
                              .chequeAmount = dtrw.Item("total"),
                              .currencyCode = "IDR",
                              .paymentMethod = "CASH_OTHER",
                              .transDate = CDate(dfrom).ToString("dd/MM/yyyy"),
                              .bankNo = paymentNo,
                              .number = paymentNo & CDate(dfrom).ToString("dd/MM/yyyy"),
                              .detailInvoice = getInvoiceDetail(smNo, dtrw.Item("total"))
                              })
                            End With
                        Else
                            With listData
                                .Add(New RequestSalesReceipt.Datum With
                              {
                                .customerNo = customer_no,
                              .branchName = sBranchName,
                              .chequeAmount = dtrw.Item("total"),
                              .currencyCode = "IDR",
                              .paymentMethod = "OTHERS",
                              .transDate = CDate(dfrom).ToString("dd/MM/yyyy"),
                              .bankNo = paymentNo,
                               .number = paymentNo & CDate(dfrom).ToString("dd/MM/yyyy"),
                              .detailInvoice = getInvoiceDetail(smNo, dtrw.Item("total"))
                              })
                            End With
                        End If



                    Next


                Next



            End With
        Catch ex As Exception

        End Try
        Return listData
    End Function


    Public Function SaveTransReceipt(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal connUrl As String, ByVal dfrom As String)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cTransSave As New RequestSalesReceipt
        Dim listItemSave As New List(Of RequestSalesReceipt.Datum)
        Dim responseContent As String = ""
        Dim resultTrans As New clsTransactions.ResponseSaveTransReceiptSumSalesDaily
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Try
            With cTransSave
                .data = getTransReceipt(connUrl, dfrom)
            End With

            If cTransSave.data Is Nothing OrElse cTransSave.data.Count = 0 Then
                errLog("Tidak ada data payment/sales receipt untuk tanggal " & CDate(dfrom).ToString("yyyy-MM-dd") & ". Request ke Accurate dibatalkan.", "clsTransactions.SaveTransReceipt")
                Return resultTrans
            End If

            errLog("Mulai sync payment/sales receipt tanggal " & CDate(dfrom).ToString("yyyy-MM-dd") & ". Jumlah receipt: " & cTransSave.data.Count, "clsTransactions.SaveTransReceipt")

            sJson = JsonConvert.SerializeObject(cTransSave)
            errLog("FULL JSON PAYLOAD PAYMENT: " & sJson, "clsTransactions.SaveTransReceipt")
            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            sURL = hostUrl & "/accurate/api/sales-receipt/bulk-save.do"
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
                responseContent = ReadWebExceptionResponse(ex)
                errLog("Sync payment/sales receipt gagal request ke Accurate. Detail: " & responseContent, "clsTransactions.SaveTransReceipt")
            End Try

        Catch ex As Exception
            ' ErrLog(ex.Message.ToString(), "clsProc: syncTransactions", False)
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                Try
                    resultTrans = JsonConvert.DeserializeObject(Of clsTransactions.ResponseSaveTransReceiptSumSalesDaily)(responseContent)
                Catch jsonEx As Exception
                    Dim msg As String = "Sync payment/sales receipt gagal membaca response Accurate." & vbCrLf &
                                        "Kemungkinan: jumlah data bulk melebihi batas Accurate." & vbCrLf &
                                        "Response: " & responseContent
                    errLog(jsonEx.Message & " | " & responseContent, "clsTransactions.SaveTransReceipt")
                    Return resultTrans
                End Try
            End If
        End If
        Dim b = resultTrans.d
        If b Is Nothing OrElse b.Count = 0 Then
            Dim msg As String = "Sync payment/sales receipt tidak berhasil. Response Accurate kosong atau tidak berisi detail." & vbCrLf &
                                "Response: " & responseContent
            With cCMD
                Dim tgl1 As Date = Now.Date()
                .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & SafeSqlText(responseContent) & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Detail' ,'" & solid & "')")
                .executeNonQuery()
                .dispose()
            End With
            errLog(msg, "clsTransactions.SaveTransReceipt")
            Return resultTrans
        End If

        If b(0).s = True Then
            With cCMD
                Dim tgl1 As Date = Now.Date()
                .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & SafeSqlText(b(0).GetValue_d()) & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Payment' ,'" & solid & "')")
                .executeNonQuery()
                .dispose()
            End With

        Else
            Dim errorMessage As String = b(0).GetValue_d()
            errLog(errorMessage, "clsTransactions.SaveTransReceipt")
        End If

        Return resultTrans

    End Function


    Public Function SaveTransSumSalesDaily(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal connUrl As String, ByVal dFrom As Date)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cTransSave As New Request
        Dim listItemSave As New List(Of Request.Datum)
        Dim responseContent As String = ""
        Dim resultTrans As New clsTransactions.ResponseSaveTransSumSalesDaily
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Try
            With cTransSave
                .data = getTransSumSalesDaily(connUrl, dFrom)
            End With

            If cTransSave.data Is Nothing OrElse cTransSave.data.Count = 0 Then
                errLog("Tidak ada data faktur penjualan harian untuk tanggal " & CDate(dFrom).ToString("yyyy-MM-dd") & ". Request ke Accurate dibatalkan.", "clsTransactions.SaveTransSumSalesDaily")
                Return resultTrans
            End If

            For Each invoiceData In cTransSave.data
                errLog("[REQUEST_INVOICE] BrandID=" & sGlobalActiveBrandId & " | OutletId=" & solid & " | MultiBranch=" & sGlobalActiveBrandIsMultiBranch.ToString() &
                       " | Branch=" & If(invoiceData.branchName, "<kosong>") & " | Number=" & invoiceData.number & " | Date=" & invoiceData.transDate, "clsTransactions.SaveTransSumSalesDaily")
                errLog("Payload faktur penjualan harian " & invoiceData.number & " tanggal " & invoiceData.transDate &
                       ". Detail barang: " & If(invoiceData.detailItem Is Nothing, 0, invoiceData.detailItem.Count) &
                       ", biaya: " & If(invoiceData.detailExpense Is Nothing, 0, invoiceData.detailExpense.Count),
                       "clsTransactions.SaveTransSumSalesDaily")
            Next

            '    Dim settings As New JsonSerializerSettings With {
            '    .ContractResolver = New Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            '    .ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            '    .NullValueHandling = NullValueHandling.Ignore
            '}
            sJson = JsonConvert.SerializeObject(cTransSave)
            ' errLog("FULL JSON PAYLOAD FAKTUR: " & sJson, "clsTransactions.SaveTransSumSalesDaily")

            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            sURL = hostUrl & "/accurate/api/sales-invoice/bulk-save.do"
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            myWebReq = DirectCast(WebRequest.Create(sURL), HttpWebRequest)

            With myWebReq
                .ContentType = "application/json"
                .Headers.Add("Authorization: Bearer " & accesstoken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "POST"
                .Timeout = 80000
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
                responseContent = ReadWebExceptionResponse(ex)
                errLog("Sync sales invoice summary gagal request ke Accurate. Detail: " & responseContent, "clsTransactions.SaveTransSumSalesDaily")
            End Try

        Catch ex As Exception
            errLog(Err.Description, "clsTransactions.SaveTransSumSalesDaily " & If(responseContent, ""))
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                Try
                    resultTrans = JsonConvert.DeserializeObject(Of clsTransactions.ResponseSaveTransSumSalesDaily)(responseContent)
                    errLog("Response sync faktur penjualan harian: " & responseContent.ToString, "clsTransactions.SaveTransSumSalesDaily")
                Catch jsonEx As Exception
                    errLog("Sync faktur penjualan harian gagal membaca response Accurate. Detail: " & jsonEx.Message & " | Response: " & responseContent, "clsTransactions.SaveTransSumSalesDaily")
                    With cCMD
                        Dim tgl1 As Date = Now.Date()
                        .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & SafeSqlText(responseContent) & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Detail' ,'" & solid & "')")
                        .executeNonQuery()
                        .dispose()
                    End With
                    Return resultTrans
                End Try
            End If
        End If

        Dim b = resultTrans.d
        If b Is Nothing OrElse b.Count = 0 Then
            With cCMD
                Dim tgl1 As Date = Now.Date()
                .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & SafeSqlText(responseContent) & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Detail' ,'" & solid & "')")
                .executeNonQuery()
                .dispose()
            End With
            errLog("Sync faktur penjualan harian tidak berhasil. Response Accurate kosong.", "clsTransactions.SaveTransSumSalesDaily")
            Return resultTrans
        End If

        Dim allSuccess As Boolean = True
        Dim realErrors As New List(Of String)
        
        For Each item In b
            If Not item.s Then
                Dim msg As String = item.GetValue_d()
                If Not (msg.IndexOf("Sudah ada data lain", StringComparison.OrdinalIgnoreCase) >= 0) Then
                    allSuccess = False
                    realErrors.Add(msg)
                End If
            End If
        Next

        If allSuccess Then
            With cCMD
                Dim tgl1 As Date = Now.Date()
                .resetCmd("REPLACE INTO cld_syncsales VALUES ('" & dFrom.ToString("yyyy-MM-dd 00:00:00") & "' ," & solid & " ,'Acc' ,'Detail Success' ,'" & scustid & "' ,'" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "')")
                .executeNonQuery()
                .dispose()
            End With
            If b.Any(Function(x) x.s = False) Then
                errLog("Faktur penjualan harian tanggal " & dFrom.ToString("yyyy-MM-dd") & " membalas error yang diabaikan (duplikat/safe), ditandai sebagai sudah terkirim.", "clsTransactions.SaveTransSumSalesDaily")
            End If
        Else
            Dim combinedError As String = String.Join(" | ", realErrors)
            If combinedError.Length > 200 Then combinedError = combinedError.Substring(0, 200) & "..."
            With cCMD
                Dim tgl1 As Date = Now.Date()
                Dim errMsg As String = SafeSqlText(combinedError)
                Dim shortErrMsg As String = If(errMsg.Length > 20, errMsg.Substring(0, 20), errMsg)
                .resetCmd("REPLACE INTO cld_syncsales VALUES ('" & dFrom.ToString("yyyy-MM-dd 00:00:00") & "' ," & solid & " ,'Acc' ,'Detail Not_success: " & shortErrMsg & "' ,'" & scustid & "' ,'" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "')")
                .executeNonQuery()
                Try
                    .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & errMsg & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Detail' ,'" & solid & "')")
                    .executeNonQuery()
                Catch exLog As Exception
                    errLog("Gagal insert cld_log: " & exLog.Message, "clsTransactions.SaveTransSumSalesDaily")
                End Try
                errLog("Gagal simpan faktur penjualan ke Accurate: " & combinedError, "clsTransactions.SaveTransSumSalesDaily")
                .dispose()
            End With
        End If
        Return resultTrans

    End Function

    Private Function SafeSqlText(ByVal value As String) As String
        If value Is Nothing Then Return ""
        Dim result As String = value.Replace("'", "''")
        If result.Length > 255 Then result = result.Substring(0, 255)
        Return result
    End Function

    Public Function SaveTransSumSalesDailyJU(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal connUrl As String, ByVal dFrom As Date)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cTransSave As New RequestJU
        Dim listItemSave As New List(Of RequestJU.Datum)
        Dim responseContent As String = ""
        Dim resultTrans As New clsTransactions.ResponseSaveTransSumSalesDaily
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Try
            With cCMD
                .resetCmd("SELECT COUNT(*) FROM cld_syncsales " &
                          "WHERE busidate='" & dFrom.Date.ToString("yyyy-MM-dd 00:00:00") & "' " &
                          "AND olid=" & solid & " AND custid='" & scustid & "' AND Type='Detail Success'")
                If Val(.executeScaler(False)) > 0 Then
                    errLog("Jurnal tanggal " & dFrom.ToString("yyyy-MM-dd") & " sudah pernah terkirim. Request ke Accurate dilewati.", "clsTransactions.SaveTransSumSalesDailyJU")
                    Return resultTrans
                End If
            End With

            With cTransSave
                .data = getTransSumSalesDailyJU(connUrl, dFrom)
            End With

            If cTransSave.data Is Nothing OrElse cTransSave.data.Count = 0 Then
                errLog("Tidak ada detail jurnal untuk tanggal " & CDate(dFrom).ToString("yyyy-MM-dd") & ". Request ke Accurate dibatalkan.", "clsTransactions.SaveTransSumSalesDailyJU")
                Return resultTrans
            End If

            For Each journalData In cTransSave.data
                errLog("[REQUEST_JOURNAL] BrandID=" & sGlobalActiveBrandId & " | OutletId=" & solid &
                       " | Branch=" & If(journalData.branchName, "<kosong>") & " | Number=" & journalData.number & " | Date=" & journalData.transDate, "clsTransactions.SaveTransSumSalesDailyJU")
                Dim debitTotal As Decimal = 0
                Dim creditTotal As Decimal = 0
                If journalData.detailJournalVoucher IsNot Nothing Then
                    debitTotal = journalData.detailJournalVoucher.Where(Function(detail) detail.amountType = "DEBIT").Sum(Function(detail) detail.amount)
                    creditTotal = journalData.detailJournalVoucher.Where(Function(detail) detail.amountType = "CREDIT").Sum(Function(detail) detail.amount)
                End If

                errLog("Payload jurnal " & journalData.number & " tanggal " & journalData.transDate &
                       ". Detail: " & If(journalData.detailJournalVoucher Is Nothing, 0, journalData.detailJournalVoucher.Count) &
                       ", debit: " & debitTotal.ToString("0.##") &
                       ", credit: " & creditTotal.ToString("0.##"),
                       "clsTransactions.SaveTransSumSalesDailyJU")
            Next

            '    Dim settings As New JsonSerializerSettings With {
            '    .ContractResolver = New Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            '    .ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            '    .NullValueHandling = NullValueHandling.Ignore
            '}
            sJson = JsonConvert.SerializeObject(cTransSave)


            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            sURL = hostUrl & "/accurate/api/journal-voucher/bulk-save.do"
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            myWebReq = DirectCast(WebRequest.Create(sURL), HttpWebRequest)

            With myWebReq
                .ContentType = "application/json"
                .Headers.Add("Authorization: Bearer " & accesstoken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "POST"
                .Timeout = 80000
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
                responseContent = ReadWebExceptionResponse(ex)
                errLog("Sync jurnal umum gagal request ke Accurate. Detail: " & responseContent, "clsTransactions.SaveTransSumSalesDailyJU")
            End Try

        Catch ex As Exception
            errLog(Err.Description, "clsTransactions.SaveTransSumSalesDailyJU " & responseContent.ToString)
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                resultTrans = JsonConvert.DeserializeObject(Of clsTransactions.ResponseSaveTransSumSalesDaily)(responseContent)
                '  ErrLog(responseContent, " JsonResult", False)
                errLog(Err.Description, "clsTransactions.SaveTransSumSalesDailyJU " & responseContent.ToString)
                Err.Clear()
            End If
        End If

        Dim b = resultTrans.d
        If b Is Nothing OrElse b.Count = 0 Then
            With cCMD
                Dim tgl1 As Date = Now.Date()
                .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & SafeSqlText(responseContent) & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Detail' ,'" & solid & "')")
                .executeNonQuery()
                .dispose()
            End With
            Return resultTrans
        End If

        If b(0).s = True Then
            With cCMD
                Dim tgl1 As Date = Now.Date()
                .resetCmd("REPLACE INTO cld_syncsales VALUES ('" & dFrom.ToString("yyyy-MM-dd 00:00:00") & "' ," & solid & " ,'Acc' ,'Detail Success' ,'" & scustid & "' ,'" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "')")
                .executeNonQuery()
                .dispose()
            End With
        Else
            With cCMD
                Dim tgl1 As Date = Now.Date()
                Dim errMsg As String = SafeSqlText(b(0).GetValue_d())
                Dim shortErrMsg As String = If(errMsg.Length > 20, errMsg.Substring(0, 20), errMsg)
                .resetCmd("REPLACE INTO cld_syncsales VALUES ('" & dFrom.ToString("yyyy-MM-dd 00:00:00") & "' ," & solid & " ,'Acc' ,'Detail Not_success: " & shortErrMsg & "' ,'" & scustid & "' ,'" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "')")
                .executeNonQuery()
                Try
                    .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & errMsg & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Detail' ,'" & solid & "')")
                    .executeNonQuery()
                Catch exLog As Exception
                    errLog("Gagal insert cld_log: " & exLog.Message, "clsTransactions.SaveTransSumSalesDailyJU")
                End Try
            End With
        End If
        Return resultTrans
    End Function

    Public Function getTransSumSalesDaily(ByVal connUrl As String, ByVal dFrom As String) 'As List(Of Request.Datum)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of Request.Datum)
        Dim SalesID As String = ""
        Dim sBranchName As String = ""
        Dim sKode As String = ""
        Dim taxperitem As Boolean = False
        If My.Settings.taxperitem = 1 Then
            taxperitem = True
        End If
        Try
            ' Ambil Kode dan kode2 dari outlet
            Try
                cCMD.resetCmd("SELECT Kode, IFNULL(kode2,'') kode2 FROM cld_outlet WHERE ID = " & solid)
                Dim dtOutlet As DataTable = cCMD.selectDatatable()
                If dtOutlet IsNot Nothing AndAlso dtOutlet.Rows.Count > 0 Then
                    sKode = dtOutlet.Rows(0)("Kode").ToString()
                    sBranchName = dtOutlet.Rows(0)("kode2").ToString()
                End If
            Catch ex As Exception
                sBranchName = ""
                sKode = solid
            End Try

            errLog("[DIAG] getTransSumSalesDaily | solid='" & solid & "' | Kode='" & sKode & "' | kode2='" & sBranchName & "' | isMultiBranch=" & sGlobalActiveBrandIsMultiBranch.ToString() & " | dFrom='" & dFrom & "'", "clsTransactions.getTransSumSalesDaily")

            If sGlobalActiveBrandIsMultiBranch Then
                ' Mode multi-branch (brand 1): kode2 wajib ada
                If String.IsNullOrEmpty(sBranchName) Then
                    errLog("Cabang belum didaftar di cld_outlet.kode2 untuk olid " & solid & ". Proses di-skip.", "clsTransactions.getTransSumSalesDaily")
                    Return listData
                End If
            End If
            ' Mode non-multi-branch: lanjut meskipun kode2 kosong

            Dim sqlQuery As String = " SELECT olnumber,busidate as transdate,SUM(Subtotal) Subtotal, SUM(Tax) Tax, SUM(Service) Service, SUM(Total) ttl, SUM(Discount) Discount, " & _
                          "IFNULL(((SELECT SUM(Total) FROM cld_sumpaymentdaily WHERE olid = cld_sumsalesdaily.olId AND busidate = cld_sumsalesdaily.busidate AND olnumber = cld_sumsalesdaily.olnumber)),0)-(SUM(subtotal)+IFNULL((SELECT SUM(ROUND((price * qty) * 0.1)) FROM cld_sumsalesdetaildaily WHERE olid = cld_sumsalesdaily.olId AND busidate = cld_sumsalesdaily.busidate AND olnumber = cld_sumsalesdaily.olnumber AND itemno <> 'Disc Amt' AND price >= 0 AND qty > 0), 0)+SUM(service)-SUM(discount)+ROUND(SUM(service) * 0.1)) RoundOff,'' as slstypenm,sum(deliveryfee) deliveryfee " & _
                          " FROM cld_sumsalesdaily " & _
                          "    WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' " & _
                          "    AND olid = " & solid & " GROUP BY olnumber, busidate, olId"
            errLog("[DIAG] SQL getTransSumSalesDaily: " & sqlQuery, "clsTransactions.getTransSumSalesDaily")
            With cCMD
                .resetCmd(sqlQuery)
                ds = .selectDataset()
                .dispose()
                errLog("[DIAG] Jumlah row hasil query cld_sumsalesdaily untuk solid='" & solid & "' tanggal '" & dFrom & "': " & ds.Tables(0).Rows.Count, "clsTransactions.getTransSumSalesDaily")

                For Each dtrw As DataRow In ds.Tables(0).Rows
                    Dim currentDetailItem = getDetailItemSalesSumDaily(connUrl, dFrom, dtrw.Item("olnumber").ToString())
                    If currentDetailItem Is Nothing OrElse currentDetailItem.Count = 0 Then
                        errLog("Faktur penjualan harian dilewati karena detail barang kosong. (No: " & dtrw.Item("olnumber") & ")", "clsTransactions.getTransSumSalesDaily")
                        Continue For
                    End If

                    If CDbl(dtrw.Item("Service")) <> 0 Then
                        currentDetailItem.Add(New Request.Detailitem With
                                         {
                                             .itemNo = "100001",
                                             .itemUnitName = "PCS",
                                             .quantity = 1,
                                             .unitPrice = dtrw.Item("Service"),
                                             .warehouseName = "",
                                             .useTax1 = True
                                         })
                    End If

                    ' Tentukan branchName dan nomor faktur berdasarkan mode
                    Dim invoiceBranchName As String = Nothing   ' Nothing = tidak dikirim (NullValueHandling.Ignore)
                    Dim invoiceNumber As String
                    If sGlobalActiveBrandIsMultiBranch Then
                        ' Multi-branch: kirim branchName = kode2, nomor pakai kode2 (Brand 1 dan 2).
                        invoiceBranchName = sBranchName
                        invoiceNumber = sBranchName.Replace(" ", "") & "-" & CDate(dFrom).ToString("yyyyMMdd") & "-" & dtrw.Item("olnumber")
                    Else
                        ' Non-multi-branch: tidak kirim branchName, nomor pakai Kode outlet.
                        invoiceBranchName = Nothing
                        Dim kodePart As String = If(String.IsNullOrEmpty(sKode), solid.ToString(), sKode.Replace(" ", ""))
                        invoiceNumber = kodePart & "-" & CDate(dFrom).ToString("yyyyMMdd") & "-" & dtrw.Item("olnumber")
                    End If

                    With listData
                        .Add(New Request.Datum With
                      {
                        .customerNo = customer_no,
                        .detailItem = currentDetailItem,
                        .branchName = invoiceBranchName,
                        .inclusiveTax = False,
                        .number = invoiceNumber,
                        .taxable = taxperitem,
                        .transDate = CDate(dFrom).ToString("dd/MM/yyyy"),
                        .saveAsStatusType = "UNAPPROVED",
                        .detailExpense = getDetailexpense(dtrw)
                      })
                    End With
                Next

            End With
        Catch ex As Exception
            errLog(ex.Message, "clsTransactions.getTransSumSalesDaily")
        End Try
        Return listData
    End Function

    Public Function getTransSumSalesDailyJU(ByVal connUrl As String, ByVal dFrom As String) 'As List(Of Request.Datum)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of RequestJU.Datum)
        Dim listDetail As New List(Of RequestJU.detailJournalVoucher)
        Dim missingCoa As New List(Of String)
        Dim sBranchName As String = ""
        Dim taxperitem As Boolean = False
        If My.Settings.taxperitem = 1 Then
            taxperitem = True
        End If
        Try
            ' Ambil Kode dan kode2 dari outlet
            Dim sKodeJU As String = ""
            Try
                cCMD.resetCmd("SELECT Kode, IFNULL(kode2,'') kode2 FROM cld_outlet WHERE ID = " & solid)
                Dim dtOutletJU As DataTable = cCMD.selectDatatable()
                If dtOutletJU IsNot Nothing AndAlso dtOutletJU.Rows.Count > 0 Then
                    sKodeJU = dtOutletJU.Rows(0)("Kode").ToString()
                    sBranchName = dtOutletJU.Rows(0)("kode2").ToString()
                End If
            Catch ex As Exception
                sBranchName = ""
                sKodeJU = solid
            End Try
            If sGlobalActiveBrandIsMultiBranch Then
                If String.IsNullOrEmpty(sBranchName) Then
                    errLog("Cabang belum didaftar di cld_outlet.kode2 untuk olid " & solid & ". Proses di-skip.", "clsTransactions.getTransSumSalesDailyJU")
                    Return listData
                End If
            End If
            With cCMD

                .resetCmd("SELECT MIN(olnumber) AS olnumber,MIN(busidate) AS transdate,majorgroup Account,(SELECT majorcoa FROM `cld_majorgroup` WHERE NAME=majorgroup) coa,SUM(total) amount,'CREDIT' Amounttype " & _
                          "  FROM cld_sumsalesdetaildaily " & _
                          "  WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' AND total>=0 " & _
                          "      AND olid = " & solid & " GROUP BY  majorgroup " & _
                          "  UNION ALL " & _
                          "  SELECT MIN(olnumber) AS olnumber,MIN(busidate) AS transdate,'TAX',(SELECT coa FROM `cld_syncother` WHERE NAME='PB1'), SUM(Tax),'CREDIT' Amounttype FROM cld_sumsalesdaily " & _
                          "      WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' " & _
                          "      AND olid = " & solid & " " & _
                          "      UNION ALL " & _
                          "  SELECT MIN(olnumber) AS olnumber,MIN(busidate) AS transdate,'SERVICE',(SELECT coa FROM `cld_syncother` WHERE NAME='service'), SUM(Service) ,'CREDIT' Amounttype FROM cld_sumsalesdaily " & _
                          "      WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' " & _
                          "      AND olid = " & solid & " " & _
                          "      UNION ALL " & _
                          "  SELECT MIN(olnumber) AS olnumber,MIN(busidate) AS transdate,'DISCOUNT',(SELECT coa FROM `cld_syncother` WHERE NAME='Discount'), SUM(Discount) ,'DEBIT' Amounttype FROM cld_sumsalesdaily " & _
                          "      WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' " & _
                          "      AND olid = " & solid & " " & _
                          "       UNION ALL " & _
                          "  SELECT MIN(olnumber) AS olnumber,MIN(busidate) AS transdate,'ROUNDoff',(SELECT coa FROM `cld_syncother` WHERE NAME='rounding'), " & _
                          "  IFNULL(((SELECT SUM(Total) FROM cld_sumpaymentdaily WHERE olid = " & solid & " AND Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' AND Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59')),0)-(SUM(subtotal)+IFNULL((SELECT SUM(ROUND((price * qty) * 0.1)) FROM cld_sumsalesdetaildaily WHERE olid = cld_sumsalesdaily.olId AND busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' AND busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59'), 0)+SUM(service)-SUM(discount)) " & _
                          "   ,'CREDIT' Amounttype FROM cld_sumsalesdaily " & _
                          "      WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' " & _
                          "      AND olid = " & solid & " " & _
                          "   UNION ALL " & _
                          "   SELECT MIN(olnumber) AS olnumber,MIN(busidate) AS transdate,payname Account,(SELECT paycoa FROM `cld_syncpaymaster` WHERE payNAME=cld_sumpaymentdaily.payname LIMIT 1),SUM(total) amount,'DEBIT' Amounttype " & _
                          "  FROM cld_sumpaymentdaily " & _
                          "   WHERE Busidate >= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dFrom).ToString("yyyy-MM-dd") & " 23:59:59' " & _
                          "      AND olid = " & solid & " GROUP BY  payname")
                ds = .selectDataset()
                .dispose()

                For Each dtrw As DataRow In ds.Tables(0).Rows
                    If dtrw.Item("amount") Is DBNull.Value OrElse dtrw.Item("amount") = 0 Then
                        Continue For
                    End If

                    If dtrw.Item("coa") Is DBNull.Value OrElse dtrw.Item("coa").ToString().Trim() = "" Then
                        missingCoa.Add(dtrw.Item("Account").ToString())
                        Continue For
                    End If

                    With listDetail
                        .Add(New RequestJU.detailJournalVoucher With
                         {
                           .accountNo = dtrw.Item("coa").ToString(),
                           .amount = dtrw.Item("amount"),
                            .amountType = dtrw.Item("Amounttype")
                                })
                    End With
                Next

                If missingCoa.Count > 0 Then
                    errLog("COA belum diset untuk jurnal: " & String.Join(", ", missingCoa.Distinct().ToArray()), "clsTransactions.getTransSumSalesDailyJU")
                    listDetail.Clear()
                    Return listData
                End If

                If listDetail.Count > 0 Then
                    Dim debitTotal As Decimal = listDetail.Where(Function(detail) detail.amountType = "DEBIT").Sum(Function(detail) detail.amount)
                    Dim creditTotal As Decimal = listDetail.Where(Function(detail) detail.amountType = "CREDIT").Sum(Function(detail) detail.amount)
                    Dim balanceDiff As Decimal = Math.Round(debitTotal - creditTotal, 2)

                    If balanceDiff <> 0D Then
                        Dim roundingCoa As String = ""
                        .resetCmd("SELECT coa FROM cld_syncother WHERE NAME='rounding' LIMIT 1")
                        roundingCoa = .executeScaler()
                        .dispose()

                        If roundingCoa Is Nothing OrElse roundingCoa.Trim() = "" Then
                            errLog("COA rounding belum diset, jurnal tidak balance. Debit: " & debitTotal.ToString("0.##") & ", credit: " & creditTotal.ToString("0.##"), "clsTransactions.getTransSumSalesDailyJU")
                            listDetail.Clear()
                            Return listData
                        End If

                        listDetail.Add(New RequestJU.detailJournalVoucher With {
                            .accountNo = roundingCoa,
                            .amount = Math.Abs(balanceDiff),
                            .amountType = If(balanceDiff > 0D, "CREDIT", "DEBIT")
                        })

                        errLog("Auto balance jurnal ke akun rounding " & roundingCoa & ". Selisih: " & balanceDiff.ToString("0.##"), "clsTransactions.getTransSumSalesDailyJU")
                    End If
                End If

                If listDetail.Count > 0 Then
                    ' Tentukan branchName dan nomor berdasarkan mode
                    Dim juBranchName As String = Nothing
                    Dim juNumber As String
                    If sGlobalActiveBrandIsMultiBranch Then
                        juBranchName = sBranchName
                        juNumber = CDate(dFrom).ToString("yyyyMMdd") & "-" & solid
                    Else
                        juBranchName = Nothing
                        Dim kodePartJU As String = If(String.IsNullOrEmpty(sKodeJU), solid.ToString(), sKodeJU.Replace(" ", ""))
                        juNumber = CDate(dFrom).ToString("yyyyMMdd") & "-" & kodePartJU
                    End If
                    With listData
                        .Add(New RequestJU.Datum With
                              {
                                .detailJournalVoucher = listDetail,
                                 .transDate = CDate(dFrom).ToString("dd/MM/yyyy"),
                                .branchName = juBranchName,
                                 .number = juNumber,
                                .description = "Sales " & CDate(dFrom).ToString("dd MMMM yyyy")
                                                      })
                    End With
                End If




            End With
        Catch ex As Exception
            errLog(ex.Message, "clsTransactions.getTransSumSalesDailyJU")
            Err.Clear()
        End Try
        Return listData
    End Function
    Public Function SaveTransReceiptSumSalesDaily(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal connUrl As String, ByVal dfrom As Date)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cTransSave As New RequestSalesReceipt
        Dim listItemSave As New List(Of RequestSalesReceipt.Datum)
        Dim responseContent As String = ""
        Dim resultTrans As New clsTransactions.ResponseSaveTransReceiptSumSalesDaily
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Try
            With cTransSave
                .data = getTransReceiptSumSalesDaily(connUrl, dfrom)
            End With

            If cTransSave.data Is Nothing OrElse cTransSave.data.Count = 0 Then
                errLog("Tidak ada data payment/receipt harian untuk tanggal " & CDate(dfrom).ToString("yyyy-MM-dd") & ". Request ke Accurate dibatalkan.", "clsTransactions.SaveTransReceiptSumSalesDaily")
                Return resultTrans
            End If

            For Each receiptData In cTransSave.data
                errLog("[REQUEST_RECEIPT] BrandID=" & sGlobalActiveBrandId & " | OutletId=" & solid &
                       " | Branch=" & If(receiptData.branchName, "<kosong>") & " | Number=" & receiptData.number & " | Date=" & receiptData.transDate &
                       " | Amount=" & Convert.ToDecimal(receiptData.chequeAmount).ToString(Globalization.CultureInfo.InvariantCulture), "clsTransactions.SaveTransReceiptSumSalesDaily")
            Next



            sJson = JsonConvert.SerializeObject(cTransSave)
            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            sURL = hostUrl & "/accurate/api/sales-receipt/bulk-save.do"
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
                responseContent = ReadWebExceptionResponse(ex)
                errLog("Sync payment/sales receipt summary gagal request ke Accurate. Detail: " & responseContent, "clsTransactions.SaveTransReceiptSumSalesDaily")
            End Try

        Catch ex As Exception
            errLog(Err.Description, "clsTransactions.SaveTransReceiptSumSalesDaily " & responseContent.ToString)
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                Try
                    resultTrans = JsonConvert.DeserializeObject(Of clsTransactions.ResponseSaveTransReceiptSumSalesDaily)(responseContent)
                    errLog(Err.Description, "clsTransactions.SaveTransReceiptSumSalesDaily " & responseContent.ToString)
                Catch jsonEx As Exception
                    Dim msg As String = "Sync payment/sales receipt gagal membaca response Accurate." & vbCrLf &
                                        "Kemungkinan: jumlah data bulk melebihi batas Accurate." & vbCrLf &
                                        "Response: " & responseContent
                    errLog(jsonEx.Message & " | " & responseContent, "clsTransactions.SaveTransReceiptSumSalesDaily")
                    Return resultTrans
                End Try
            End If
        End If
        Dim b = resultTrans.d
        If b Is Nothing OrElse b.Count = 0 Then
            Dim msg As String = "Sync payment/sales receipt tidak berhasil. Response Accurate kosong atau tidak berisi detail." & vbCrLf &
                                "Response: " & responseContent
            errLog(msg, "clsTransactions.SaveTransReceiptSumSalesDaily")
            Return resultTrans
        End If
        Dim allSuccess As Boolean = True
        Dim realErrors As New List(Of String)
        
        For Each item In b
            If Not item.s Then
                Dim msg As String = item.GetValue_d()
                If Not (msg.IndexOf("Sudah ada data lain", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                        msg.IndexOf("melebihi nilai piutang", StringComparison.OrdinalIgnoreCase) >= 0) Then
                    allSuccess = False
                    realErrors.Add(msg)
                End If
            End If
        Next

        If allSuccess Then
            With cCMD
                Dim tgl1 As Date = Now.Date()
                .resetCmd("REPLACE INTO cld_syncsales VALUES ('" & dfrom.ToString("yyyy-MM-dd 00:00:00") & "' ," & solid & " ,'Acc' ,'Payment Success' ,'" & scustid & "' ,'" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "')")
                .executeNonQuery()
                .dispose()
            End With
            If b.Any(Function(x) x.s = False) Then
                errLog("Penerimaan penjualan harian tanggal " & dfrom.ToString("yyyy-MM-dd") & " membalas error yang diabaikan (duplikat/safe), ditandai sebagai sudah terkirim.", "clsTransactions.SaveTransReceiptSumSalesDaily")
            End If
        Else
            Dim combinedError As String = String.Join(" | ", realErrors)
            If combinedError.Length > 200 Then combinedError = combinedError.Substring(0, 200) & "..."
            With cCMD
                Dim tgl1 As Date = Now.Date()
                Dim errMsg As String = SafeSqlText(combinedError)
                Dim shortErrMsg As String = If(errMsg.Length > 20, errMsg.Substring(0, 20), errMsg)
                .resetCmd("REPLACE INTO cld_syncsales VALUES ('" & dfrom.ToString("yyyy-MM-dd 00:00:00") & "' ," & solid & " ,'Acc' ,'Payment Not_success: " & shortErrMsg & "' ,'" & scustid & "' ,'" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "')")
                .executeNonQuery()
                Try
                    .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc','" & errMsg & "','failed','" & tgl1.Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Payment' ,'" & solid & "')")
                    .executeNonQuery()
                Catch exLog As Exception
                    errLog("Gagal insert cld_log: " & exLog.Message, "clsTransactions.SaveTransReceiptSumSalesDaily")
                End Try
                errLog("Gagal simpan penerimaan penjualan ke Accurate: " & combinedError, "clsTransactions.SaveTransReceiptSumSalesDaily")
                .dispose()
            End With
        End If
        Return resultTrans

    End Function
    Public Function getTransReceiptSumSalesDaily(ByVal connUrl As String, ByVal dfrom As String) As List(Of RequestSalesReceipt.Datum)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of RequestSalesReceipt.Datum)
        Dim SalesID As String = ""
        Dim paymentNo As String = ""
        Dim sBranchName As String = ""
        Dim sKodeRcpt As String = ""
        Try
            ' Ambil Kode dan kode2 dari outlet
            Try
                cCMD.resetCmd("SELECT Kode, IFNULL(kode2,'') kode2 FROM cld_outlet WHERE ID = " & solid)
                Dim dtOutletRcpt As DataTable = cCMD.selectDatatable()
                If dtOutletRcpt IsNot Nothing AndAlso dtOutletRcpt.Rows.Count > 0 Then
                    sKodeRcpt = dtOutletRcpt.Rows(0)("Kode").ToString()
                    sBranchName = dtOutletRcpt.Rows(0)("kode2").ToString()
                End If
            Catch ex As Exception
                sBranchName = ""
                sKodeRcpt = solid
            End Try
            If sGlobalActiveBrandIsMultiBranch Then
                If String.IsNullOrEmpty(sBranchName) Then
                    errLog("Cabang belum didaftar di cld_outlet.kode2 untuk olid " & solid & ". Proses di-skip.", "clsTransactions.getTransReceiptSumSalesDaily")
                    Return listData
                End If
            End If

            ' Tentukan prefix nomor penerimaan berdasarkan mode
            Dim rcptPrefix As String
            Dim rcptBranchName As String
            If sGlobalActiveBrandIsMultiBranch Then
                rcptPrefix = sBranchName.Replace(" ", "")
                rcptBranchName = sBranchName
            Else
                rcptPrefix = If(String.IsNullOrEmpty(sKodeRcpt), solid.ToString(), sKodeRcpt.Replace(" ", ""))
                rcptBranchName = Nothing   ' Nothing = tidak dikirim ke Accurate
            End If

            With cCMD
                .resetCmd("SELECT olnumber, payname, SUM(total) as total FROM cld_sumpaymentdaily WHERE Busidate >= '" & CDate(dfrom).ToString("yyyy-MM-dd") & " 00:00:00' and Busidate <= '" & CDate(dfrom).ToString("yyyy-MM-dd") & " 23:59:59' AND olid = " & solid & " GROUP BY olnumber, payname")
                ds = .selectDataset()
                .dispose()

                For Each dtrw As DataRow In ds.Tables(0).Rows
                    .resetCmd("SELECT paycoa FROM cld_syncpaymaster WHERE payname = '" & dtrw.Item("payname") & "'")
                    paymentNo = .executeScaler
                    .dispose()

                    If dtrw.Item("payname") = "CASH" Then
                        With listData
                            .Add(New RequestSalesReceipt.Datum With
                          {
                            .customerNo = customer_no,
                          .branchName = rcptBranchName,
                          .chequeAmount = dtrw.Item("total"),
                          .currencyCode = "IDR",
                          .paymentMethod = "CASH_OTHER",
                          .transDate = CDate(dfrom).ToString("dd/MM/yyyy"),
                          .bankNo = paymentNo,
                          .number = rcptPrefix & "-" & paymentNo & "-" & CDate(dfrom).ToString("ddMMyy") & "-" & dtrw.Item("olnumber").ToString(),
                          .detailInvoice = getInvoiceDetail(rcptPrefix & "-" & CDate(dfrom).ToString("yyyyMMdd") & "-" & dtrw.Item("olnumber"), dtrw.Item("total"))
                          })
                        End With
                    Else
                        With listData
                            .Add(New RequestSalesReceipt.Datum With
                          {
                            .customerNo = customer_no,
                          .branchName = rcptBranchName,
                          .chequeAmount = dtrw.Item("total"),
                          .currencyCode = "IDR",
                          .paymentMethod = "OTHERS",
                          .transDate = CDate(dfrom).ToString("dd/MM/yyyy"),
                          .bankNo = paymentNo,
                          .number = rcptPrefix & "-" & paymentNo & "-" & CDate(dfrom).ToString("ddMMyy") & "-" & dtrw.Item("olnumber").ToString(),
                          .detailInvoice = getInvoiceDetail(rcptPrefix & "-" & CDate(dfrom).ToString("yyyyMMdd") & "-" & dtrw.Item("olnumber"), dtrw.Item("total"))
                          })
                        End With
                    End If
                Next

            End With
        Catch ex As Exception
            errLog(ex.Message, "clsTransactions.getTransReceiptSumSalesDaily")
        End Try
        Return listData
    End Function


    ' --- ADDED FOR ACCURATE STATUS CHECKING AND DELETION ---
    Public Class ResponseListInvoice
        Public Property s As Boolean
        Public Property d As List(Of InvoiceData)
    End Class
    Public Class InvoiceData
        Public Property id As Long
        Public Property number As String
        Public Property totalAmount As Decimal
        Public Property outstandingAmount As Decimal
    End Class

    Public Class ResponseListReceipt
        Public Property s As Boolean
        Public Property d As List(Of ReceiptData)
    End Class
    Public Class ReceiptData
        Public Property id As Long
        Public Property number As String
    End Class

    Public Function DeleteAccurateTransactions(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal moduleName As String, ByVal id As Long) As Boolean
        Dim sURL As String = hostUrl & "/accurate/api/" & moduleName & "/delete.do?id=" & id
        Dim myWebReq As HttpWebRequest = DirectCast(WebRequest.Create(sURL), HttpWebRequest)
        Dim myWebResp As HttpWebResponse = Nothing
        Try
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            With myWebReq
                .ContentType = "application/json"
                .Headers.Add("Authorization: Bearer " & accesstoken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "DELETE"
                .Timeout = 30000
            End With

            myWebResp = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
            Dim sr As New StreamReader(myWebResp.GetResponseStream())
            Dim responseContent As String = sr.ReadToEnd()
            sr.Dispose()
            myWebResp.Close()
            Return True
        Catch ex As Exception
            errLog("Gagal menghapus " & moduleName & " dengan ID " & id & " | Error: " & ex.Message, "clsTransactions.DeleteAccurateTransactions")
            Return False
        End Try
    End Function

    ' Read the saved invoice, never infer amounts from the upload response.
    Private Function ReconciliationAmount(ByVal data As JObject, ByVal field As String) As Decimal
        Dim token As JToken = data.GetValue(field, StringComparison.OrdinalIgnoreCase)
        If token Is Nothing OrElse token.Type = JTokenType.Null Then
            Throw New InvalidDataException("Field Accurate tidak tersedia: " & field)
        End If
        Return Decimal.Parse(token.ToString(), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture)
    End Function

    Private Shared ReadOnly ReconciliationLogLock As New Object

    Public Sub ReconcileSalesDaily(ByVal accessToken As String, ByVal sessionId As String, ByVal host As String, ByVal transDate As Date, ByVal outletCode As String)
        Dim localSubtotal As Decimal = 0D, localTax As Decimal = 0D
        Dim accurateSubtotal As Decimal = 0D, accurateTax As Decimal = 0D
        Dim issues As New List(Of String)
        Dim numbers As New List(Of String)
        Dim complete As Boolean = True
        Dim cmd As New clsCMD(My.Settings.sKoneksi)
        Try
            ' Match the source scope used by getTransSumSalesDaily (all rows for this outlet/day).
            cmd.resetCmd("SELECT olnumber, IFNULL(SUM(subtotal),0) subtotal, IFNULL(SUM(tax),0) tax FROM cld_sumsalesdaily WHERE olid=" & solid &
                         " AND busidate >= '" & transDate.ToString("yyyy-MM-dd") & " 00:00:00' AND busidate < '" & transDate.AddDays(1).ToString("yyyy-MM-dd") & " 00:00:00' GROUP BY olnumber")
            Dim rows As DataTable = cmd.selectDatatable()
            For Each row As DataRow In rows.Rows
                localSubtotal += Convert.ToDecimal(row("subtotal"))
                localTax += Convert.ToDecimal(row("tax"))
            Next
            For Each row As DataRow In rows.Rows
                Dim number As String = outletCode.Replace(" ", "") & "-" & transDate.ToString("yyyyMMdd") & "-" & row("olnumber").ToString()
                numbers.Add(number)
                Try
                    ' Stay below Accurate's request limit, including the preceding sync call.
                    Threading.Thread.Sleep(150)
                    Dim req As HttpWebRequest = DirectCast(WebRequest.Create(host & "/accurate/api/sales-invoice/detail.do?number=" & Uri.EscapeDataString(number)), HttpWebRequest)
                    req.Method = "GET"
                    req.Timeout = 30000
                    req.Headers.Add("Authorization", "Bearer " & accessToken)
                    req.Headers.Add("X-Session-ID", sessionId)
                    Dim result As JObject
                    Using response As WebResponse = req.GetResponse()
                        Using reader As New StreamReader(response.GetResponseStream())
                            result = JObject.Parse(reader.ReadToEnd())
                        End Using
                    End Using
                    If result("s") Is Nothing OrElse Not result("s").Value(Of Boolean)() OrElse Not TypeOf result("d") Is JObject Then
                        Throw New InvalidDataException("Faktur tidak ditemukan / respons detail gagal")
                    End If
                    Dim detail As JObject = DirectCast(result("d"), JObject)
                    If Not String.Equals(CStr(detail("number")), number, StringComparison.OrdinalIgnoreCase) Then
                        Throw New InvalidDataException("Nomor faktur respons tidak cocok")
                    End If
                    ' Compare the API subtotal directly; do not subtract discounts or service.
                    Dim subtotal As Decimal = ReconciliationAmount(detail, "subTotal")
                    Dim tax As Decimal = ReconciliationAmount(detail, "tax1Amount")
                    Dim tax2 As JToken = detail.GetValue("tax2Amount", StringComparison.OrdinalIgnoreCase)
                    If tax2 IsNot Nothing AndAlso tax2.Type <> JTokenType.Null Then tax += ReconciliationAmount(detail, "tax2Amount")
                    accurateSubtotal += subtotal
                    accurateTax += tax
                    Dim invFormat As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
                    Dim deltaSubtotal As Decimal = subtotal - Convert.ToDecimal(row("subtotal"))
                    Dim deltaTax As Decimal = tax - Convert.ToDecimal(row("tax"))
                    Dim matches As Boolean = Decimal.Round(deltaSubtotal, 2, MidpointRounding.AwayFromZero) = 0D AndAlso Decimal.Round(deltaTax, 2, MidpointRounding.AwayFromZero) = 0D
                    errLog("[REKON_FAKTUR] Tanggal=" & transDate.ToString("yyyy-MM-dd") & " | OutletId=" & solid.ToString() & " | Faktur=" & number &
                           " | SubtotalLokal=" & Convert.ToDecimal(row("subtotal")).ToString(invFormat) & " | TaxLokal=" & Convert.ToDecimal(row("tax")).ToString(invFormat) &
                           " | SubtotalAccurate=" & subtotal.ToString(invFormat) & " | TaxAccurate=" & tax.ToString(invFormat) &
                           " | SelisihSubtotal=" & deltaSubtotal.ToString(invFormat) & " | SelisihTax=" & deltaTax.ToString(invFormat) &
                           " | Status=" & If(matches, "COCOK", "SELISIH"), "clsTransactions.ReconcileSalesDaily")
                    If Decimal.Round(subtotal - Convert.ToDecimal(row("subtotal")), 2, MidpointRounding.AwayFromZero) <> 0D OrElse
                       Decimal.Round(tax - Convert.ToDecimal(row("tax")), 2, MidpointRounding.AwayFromZero) <> 0D Then
                        issues.Add("SELISIH: " & number)
                    End If
                Catch ex As Exception
                    complete = False
                    issues.Add("GAGAL_CEK: " & number & " - " & ex.Message)
                    errLog("[REKON_FAKTUR] BrandID=" & sGlobalActiveBrandId & " | OutletId=" & solid & " | Tanggal=" & transDate.ToString("yyyy-MM-dd") & " | Faktur=" & number &
                           " | SubtotalLokal=" & Convert.ToDecimal(row("subtotal")).ToString(Globalization.CultureInfo.InvariantCulture) &
                           " | TaxLokal=" & Convert.ToDecimal(row("tax")).ToString(Globalization.CultureInfo.InvariantCulture) &
                           " | Status=GAGAL_CEK | Accurate=tidak tersedia | " & ex.Message, "clsTransactions.ReconcileSalesDaily")
                End Try
            Next
        Catch ex As Exception
            complete = False
            issues.Add("GAGAL_CEK_LOKAL: " & ex.Message)
        Finally
            cmd.dispose()
        End Try

        Try
            Dim directory As String = Path.GetDirectoryName(My.Settings.ErrLog)
            If String.IsNullOrEmpty(directory) Then directory = AppDomain.CurrentDomain.BaseDirectory
            Dim logPath As String = Path.Combine(directory, "SalesReconciliation.csv")
            Dim key As String = ReconciliationCsv(transDate.ToString("yyyy-MM-dd")) & ";" & ReconciliationCsv(solid.ToString()) & ";"
            SyncLock ReconciliationLogLock
                Dim lines As New List(Of String)
                lines.Add("Tanggal;OutletId;KodeOutlet;SubtotalLokal;TaxLokal;SubtotalAccurate;TaxAccurate;SelisihSubtotal;SelisihTax;Status;Faktur;Keterangan;DiperiksaPada")
                If File.Exists(logPath) Then
                    For Each line As String In File.ReadAllLines(logPath)
                        If Not line.StartsWith("Tanggal;") AndAlso Not line.StartsWith(key, StringComparison.Ordinal) Then lines.Add(line)
                    Next
                End If
                ' Temporary server testing: retain every comparison, including matching totals.
                Dim inv As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
                Dim values As String() = {transDate.ToString("yyyy-MM-dd"), solid.ToString(), outletCode,
                    localSubtotal.ToString(inv), localTax.ToString(inv),
                    If(complete, accurateSubtotal.ToString(inv), ""), If(complete, accurateTax.ToString(inv), ""),
                    If(complete, (accurateSubtotal - localSubtotal).ToString(inv), ""), If(complete, (accurateTax - localTax).ToString(inv), ""),
                    If(Not complete, "GAGAL_CEK", If(numbers.Count = 0, "TIDAK_ADA_DATA", If(issues.Count > 0, "SELISIH", "COCOK"))), String.Join(" | ", numbers), If(issues.Count > 0, String.Join(" | ", issues), If(numbers.Count = 0, "Tidak ada data lokal untuk dibandingkan", "Subtotal dan tax seluruh faktur cocok")), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
                lines.Add(String.Join(";", Array.ConvertAll(values, Function(value) ReconciliationCsv(value))))
                Dim tempPath As String = logPath & ".tmp"
                File.WriteAllLines(tempPath, lines, System.Text.Encoding.UTF8)
                If File.Exists(logPath) Then
                    File.Replace(tempPath, logPath, Nothing)
                Else
                    File.Move(tempPath, logPath)
                End If
            End SyncLock
        Catch ex As Exception
            errLog("Gagal menulis log rekonsiliasi: " & ex.Message, "clsTransactions.ReconcileSalesDaily")
        End Try
    End Sub

    Private Function ReconciliationCsv(ByVal value As String) As String
        Return """" & value.Replace(vbCr, " ").Replace(vbLf, " ").Replace("""", """""") & """"
    End Function

    Public Function CheckAccurateSyncStatus(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal dfrom As Date, ByVal branchName As String, ByRef isComplete As Boolean, ByRef existingInvoiceIds As List(Of Long), ByRef existingReceiptIds As List(Of Long)) As Boolean
        Dim sURL As String = hostUrl & "/accurate/api/sales-invoice/list.do?fields=id,number,totalAmount,outstanding,outstandingAmount,status&filter.transDate.op=EQUAL&filter.transDate.val=" & dfrom.ToString("dd/MM/yyyy") & "&keywords=" & branchName.Replace(" ", "") & "-" & dfrom.ToString("yyyyMMdd")
        Dim myWebReq As HttpWebRequest = DirectCast(WebRequest.Create(sURL), HttpWebRequest)
        Dim myWebResp As HttpWebResponse = Nothing
        Dim responseContent As String = ""
        isComplete = False
        existingInvoiceIds = New List(Of Long)
        existingReceiptIds = New List(Of Long)
        
        Dim validOlnumbers As New List(Of String)
        Dim cCMDLocal As New clsCMD(My.Settings.sKoneksi)
        Try
            cCMDLocal.resetCmd("SELECT DISTINCT olnumber FROM cld_sumsalesdaily WHERE busidate='" & dfrom.ToString("yyyy-MM-dd 00:00:00") & "' AND olid=" & solid & " AND custid='" & scustid & "'")
            Dim dtOlnumber As DataTable = cCMDLocal.selectDatatable()
            For Each dr As DataRow In dtOlnumber.Rows
                validOlnumbers.Add(dr("olnumber").ToString())
            Next
        Catch ex As Exception
        Finally
            cCMDLocal.dispose()
        End Try

        Try
            ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
            With myWebReq
                .ContentType = "application/json"
                .Headers.Add("Authorization: Bearer " & accesstoken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "GET"
                .Timeout = 30000
            End With

            myWebResp = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
            Dim sr As New StreamReader(myWebResp.GetResponseStream())
            responseContent = sr.ReadToEnd()
            sr.Dispose()
            myWebResp.Close()
            
            errLog("RAW JSON (" & branchName & " - " & dfrom.ToString("yyyy-MM-dd") & "): " & responseContent, "DEBUG_JSON")

            Dim resultInvoice As ResponseListInvoice = JsonConvert.DeserializeObject(Of ResponseListInvoice)(responseContent)
            Dim totalAccurateInvoices As Integer = 0
            Dim totalAccurateAmount As Decimal = 0
            Dim allPaid As Boolean = True

            If resultInvoice IsNot Nothing AndAlso resultInvoice.s AndAlso resultInvoice.d IsNot Nothing Then
                For Each inv In resultInvoice.d
                    Dim isValidForThisSolid As Boolean = False
                    For Each oNum In validOlnumbers
                        If (inv.number & "").ToUpper().EndsWith("-" & oNum.ToUpper()) Then
                            isValidForThisSolid = True
                            Exit For
                        End If
                    Next
                    
                    If isValidForThisSolid Then
                        existingInvoiceIds.Add(inv.id)
                        totalAccurateAmount += inv.totalAmount
                        
                        Try
                            Dim detailUrl As String = hostUrl & "/accurate/api/sales-invoice/detail.do?id=" & inv.id
                            Dim reqDetail As HttpWebRequest = DirectCast(WebRequest.Create(detailUrl), HttpWebRequest)
                            reqDetail.Headers.Add("Authorization: Bearer " & accesstoken)
                            reqDetail.Headers.Add("X-Session-ID:" & sessionID)
                            reqDetail.Method = "GET"
                            reqDetail.Timeout = 15000
                            Dim respDetail As HttpWebResponse = DirectCast(reqDetail.GetResponse(), HttpWebResponse)
                            Dim srDetail As New StreamReader(respDetail.GetResponseStream())
                            Dim respStr As String = srDetail.ReadToEnd()
                            srDetail.Dispose()
                            respDetail.Close()
                            
                            ' errLog("RAW JSON DETAIL (" & inv.id & "): " & respStr, "DEBUG_JSON_DETAIL")
                            
                            Dim jObj As JObject = JObject.Parse(respStr)
                            If jObj("d") IsNot Nothing AndAlso jObj("d")("outstanding") IsNot Nothing Then
                                If CBool(jObj("d")("outstanding")) = True Then
                                    allPaid = False
                                End If
                            End If
                        Catch ex As Exception
                            ' allPaid = False (Removed)
                        End Try

                        totalAccurateInvoices += 1
                    End If
                Next
            End If

            ' Compare with local db count and amount
            Dim cCMD As New clsCMD(My.Settings.sKoneksi)
            Dim dtLocal As DataTable
            Dim localInvoices As Integer = 0
            Dim localAmount As Decimal = 0
            Dim localReceipts As Integer = 0
            Try
                cCMD.resetCmd("SELECT COUNT(DISTINCT olnumber) as cnt, SUM(subtotal - discount + tax) as total FROM cld_sumsalesdaily WHERE busidate='" & dfrom.ToString("yyyy-MM-dd 00:00:00") & "' AND olid=" & solid & " AND custid='" & scustid & "'")
                dtLocal = cCMD.selectDatatable()
                If dtLocal.Rows.Count > 0 Then
                    localInvoices = Val(dtLocal.Rows(0)("cnt").ToString())
                    localAmount = Val(dtLocal.Rows(0)("total").ToString())
                End If
                
                cCMD.resetCmd("SELECT COUNT(*) as cnt FROM (SELECT olnumber, payname FROM cld_sumpaymentdaily WHERE busidate='" & dfrom.ToString("yyyy-MM-dd 00:00:00") & "' AND olid=" & solid & " GROUP BY olnumber, payname) t")
                Dim dtPay As DataTable = cCMD.selectDatatable()
                If dtPay.Rows.Count > 0 Then
                    localReceipts = Val(dtPay.Rows(0)("cnt").ToString())
                End If
            Finally
                cCMD.dispose()
            End Try

            ' Fetch Receipt IDs just in case we need to delete them later
            sURL = hostUrl & "/accurate/api/sales-receipt/list.do?fields=id,number&filter.transDate.op=EQUAL&filter.transDate.val=" & dfrom.ToString("dd/MM/yyyy") & "&keywords=" & branchName.Replace(" ", "")
            myWebReq = DirectCast(WebRequest.Create(sURL), HttpWebRequest)
            With myWebReq
                .ContentType = "application/json"
                .Headers.Add("Authorization: Bearer " & accesstoken)
                .Headers.Add("X-Session-ID:" & sessionID)
                .Method = "GET"
                .Timeout = 30000
            End With

            myWebResp = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
            Dim sr2 As New StreamReader(myWebResp.GetResponseStream())
            responseContent = sr2.ReadToEnd()
            sr2.Dispose()
            myWebResp.Close()

            errLog("RAW JSON RECEIPT (" & branchName & " - " & dfrom.ToString("yyyy-MM-dd") & "): " & responseContent, "DEBUG_JSON_RECEIPT")

            Dim resultReceipt As ResponseListReceipt = JsonConvert.DeserializeObject(Of ResponseListReceipt)(responseContent)
            If resultReceipt IsNot Nothing AndAlso resultReceipt.s AndAlso resultReceipt.d IsNot Nothing Then
                For Each rcp In resultReceipt.d
                    Dim isValidForThisSolid As Boolean = False
                    For Each oNum In validOlnumbers
                        If (rcp.number & "").ToUpper().EndsWith("-" & oNum.ToUpper()) Then
                            isValidForThisSolid = True
                            Exit For
                        End If
                    Next

                    If isValidForThisSolid Then
                        existingReceiptIds.Add(rcp.id)
                    End If
                Next
            End If

            If totalAccurateInvoices = localInvoices AndAlso existingReceiptIds.Count = localReceipts AndAlso allPaid = True Then
                isComplete = True
            Else
                errLog("[DIAG] CheckAccurateSyncStatus FAILED. localInvoices=" & localInvoices & ", accurateInvoices=" & totalAccurateInvoices & ", localAmount=" & localAmount & ", accurateAmount=" & totalAccurateAmount & ", localReceipts=" & localReceipts & ", accurateReceipts=" & existingReceiptIds.Count, "clsTransactions.CheckAccurateSyncStatus")
            End If

            Return True
        Catch ex As Exception
            errLog("Gagal mengecek status ke Accurate: " & ex.Message & " | " & responseContent, "clsTransactions.CheckAccurateSyncStatus")
            Return False
        End Try
    End Function

End Class


