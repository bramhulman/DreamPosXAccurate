Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports System.IO
Imports System.Net
Imports System.Threading.Tasks

Public Class clsItem

    Public Property failinsert As Integer = 0
    Public Class Response


        Public Property s As Boolean
        'Public Property d As List(Of Dd)

        'Public Class Dd
        '    Public Property d As List(Of String)
        '    Public Property s As Boolean
        'End Class

    End Class


    Public Class Request

        Public Property data() As List(Of Datum)

        Public Class Datum
            Public Property itemType As String
            Public Property name As String
            Public Property calculateGroupPrice As Boolean
            Public Property cogsGlAccountNo As String
            Public Property controlQuantity As Boolean
            Public Property defaultDiscount As String
            Public Property detailGroup() As List(Of Detailgroup)
            Public Property detailOpenBalance() As List(Of Detailopenbalance)
            Public Property goodTransitGlAccountNo As String
            'Public Property id As Integer
            Public Property inventoryGlAccountNo As String
            Public Property itemCategoryName As String
            Public Property manageExpired As Boolean
            Public Property manageSN As Boolean
            Public Property minimumQuantity As Integer
            Public Property minimumQuantityReorder As Integer
            Public Property no As String
            Public Property notes As String
            Public Property percentTaxable As Integer
            Public Property preferedVendorName As String
            Public Property printDetailGroup As Boolean
            Public Property purchaseRetGlAccountNo As String
            Public Property ratio2 As Integer
            Public Property ratio3 As Integer
            Public Property ratio4 As Integer
            Public Property ratio5 As Integer
            Public Property salesDiscountGlAccountNo As String
            Public Property salesGlAccountNo As String
            Public Property salesRetGlAccountNo As String
            Public Property serialNumberType As String
            Public Property substituted As Boolean
            Public Property substitutedItemNo As String
            Public Property tax1Name As String
            Public Property tax2Name As String
            Public Property tax3Name As String
            Public Property tax4Name As String
            Public Property typeAutoNumber As Integer
            Public Property unBilledGlAccountNo As String
            Public Property unit1Name As String
            Public Property unit2Name As String
            Public Property unit2Price As Integer
            Public Property unit3Name As String
            Public Property unit3Price As Integer
            Public Property unit4Name As String
            Public Property unit4Price As Integer
            Public Property unit5Name As String
            Public Property unit5Price As Integer
            Public Property unitPrice As Integer
            Public Property upcNo As String
            Public Property usePPn As Boolean
            Public Property useWholesalePrice As Boolean
            Public Property vendorPrice As Integer
            Public Property vendorUnitName As String
        End Class

        Public Class Detailgroup
            ' Public Property [status] As String
            Public Property detailName As String
            '  Public Property id As Integer
            Public Property itemNo As String
            Public Property itemUnitName As String
            '  Public Property quantity As Integer
        End Class

        Public Class Detailopenbalance
            Public Property [status] As String
            Public Property asOf As String
            Public Property detailSerialNumber() As List(Of Detailserialnumber)
            Public Property id As Integer
            Public Property itemUnitName As String
            Public Property quantity As Integer
            Public Property unitCost As Integer
            Public Property warehouseName As String
        End Class

        Public Class Detailserialnumber
            Public Property [status] As String
            Public Property expiredDate As String
            Public Property id As Integer
            Public Property quantity As Integer
            Public Property serialNumberNo As String
        End Class

    End Class


    Public Class RequestUpdate

        Public Property data() As List(Of Datum)

        Public Class Datum
            Public Property itemType As String
            Public Property name As String
            Public Property calculateGroupPrice As Boolean
            'Public Property cogsGlAccountNo As String
            Public Property controlQuantity As Boolean
            Public Property defaultDiscount As String

            '  Public Property goodTransitGlAccountNo As String
            Public Property id As String
            '     Public Property inventoryGlAccountNo As String
            Public Property itemCategoryName As String
            Public Property manageExpired As Boolean
            Public Property manageSN As Boolean
            Public Property minimumQuantity As Integer
            Public Property minimumQuantityReorder As Integer
            Public Property no As String
            Public Property notes As String
            Public Property percentTaxable As Integer
            Public Property preferedVendorName As String
            Public Property printDetailGroup As Boolean
            ' Public Property purchaseRetGlAccountNo As String
            Public Property ratio2 As Integer
            Public Property ratio3 As Integer
            Public Property ratio4 As Integer
            Public Property ratio5 As Integer
            '   Public Property salesDiscountGlAccountNo As String
            Public Property salesGlAccountNo As String
            '     Public Property salesRetGlAccountNo As String
            Public Property serialNumberType As String
            Public Property substituted As Boolean
            Public Property substitutedItemNo As String
            Public Property tax1Name As String
            Public Property tax2Name As String
            Public Property tax3Name As String
            Public Property tax4Name As String
            Public Property typeAutoNumber As Integer
            '     Public Property unBilledGlAccountNo As String
            Public Property unit1Name As String
            Public Property unit2Name As String
            Public Property unit2Price As Integer
            Public Property unit3Name As String
            Public Property unit3Price As Integer
            Public Property unit4Name As String
            Public Property unit4Price As Integer
            Public Property unit5Name As String
            Public Property unit5Price As Integer
            Public Property unitPrice As Integer
            Public Property upcNo As String
            Public Property usePPn As Boolean
            Public Property useWholesalePrice As Boolean
            Public Property vendorPrice As Integer
            Public Property vendorUnitName As String
        End Class


    End Class

    Public Class RequestItemCat

        Public Property data() As List(Of Datum)

        Public Class Datum
            Public Property name As String
            Public Property defaultcategory As Boolean = False
        End Class


    End Class

    Public Class RequestSearchItem
        Public Property keywords As String
    End Class
    Public Class ResponseSearchItem


        Public Property s As Boolean
        Public Property d() As New List(Of Dd)
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
            Public Property no As String
            Public Property searchHitType As String
            Public Property searchHitSubText As Object
            Public Property name As String
            Public Property searchHitValue As String
            Public Property id As Integer
            Public Property upcNo As Object
            Public Property searchHitText As String
        End Class

    End Class
    Public Function SearchItemByID(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal itemID As String)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cItemSave As New clsItem.RequestSearchItem
        Dim listItemSave As New List(Of clsItem.RequestSearchItem)
        Dim responseContent As String = ""
        Dim resultTrans As New clsItem.ResponseSearchItem
        ' Dim cCMD As New clsCMD(sKoneksi)
        Dim ds As New DataSet
        Try
            With cItemSave
                .keywords = itemID
            End With

            sJson = JsonConvert.SerializeObject(cItemSave)
            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            sURL = hostUrl & "/accurate/api/item/search-by-item-or-sn.do"
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
                Dim myWebResp1 As HttpWebResponse
                myWebResp1 = DirectCast(ex.Response, HttpWebResponse)
                Dim sr1 As StreamReader
                sr1 = New StreamReader(myWebResp1.GetResponseStream())
                Dim err As String = sr1.ReadToEnd()
                responseContent = err
            Catch ex As Exception
                errLog(Err.ToString, ex.Message)
                Err.Clear()
            End Try

        Catch ex As Exception
            errLog(ex.Message.ToString(), "clsItem: SearchItemByID")
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                resultTrans = JsonConvert.DeserializeObject(Of clsItem.ResponseSearchItem)(responseContent)
            End If
        End If

        Return resultTrans

    End Function

    Public Function GetItemCat(ByVal connUrl As String)
        Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of RequestItemCat.Datum)
        Dim SalesID As String = ""
        Try
            With cCMD
                .resetCmd("SELECT * FROM cld_majorgroup")
                ds = .selectDataset()
                .dispose()
                If ds.Tables(0).Rows.Count > 0 Then
                    For Each dtrw As DataRow In ds.Tables(0).Rows
                        With listData
                            .Add(New RequestItemCat.Datum With
                                 {
                                     .name = dtrw.Item("Name"),
                                     .defaultcategory = False
                                 })
                        End With
                    Next
                End If


            End With
        Catch ex As Exception

        End Try
        Return listData
    End Function

    Public Function SaveItemCat(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal connUrl As String)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cItemSave As New clsItem.RequestItemCat
        Dim listItemSave As New List(Of clsItem.RequestItemCat.Datum)
        Dim responseContent As String = ""
        Dim resultTrans As New clsItem.Response
        ' Dim cCMD As New clsCMD(sKoneksi)
        Dim ds As New DataSet
        Try
            With cItemSave
                .data = GetItemCat(connUrl)
            End With

            sJson = JsonConvert.SerializeObject(cItemSave)
            Dim dataserialize As Byte() = encoding.GetBytes(sJson)
            sURL = hostUrl & "/accurate/api/item-category/bulk-save.do"
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
                Dim myWebResp1 As HttpWebResponse
                myWebResp1 = DirectCast(ex.Response, HttpWebResponse)
                Dim sr1 As StreamReader
                sr1 = New StreamReader(myWebResp1.GetResponseStream())
                Dim err As String = sr1.ReadToEnd()
                responseContent = err
            End Try

        Catch ex As Exception
            ' ErrLog(ex.Message.ToString(), "clsProc: syncTransactions", False)
            Err.Clear()
        End Try

        If responseContent IsNot Nothing Then
            If responseContent <> "" Then
                If responseContent.TrimStart().StartsWith("<") Then
                    errLog("SaveItemCat: Server mengembalikan HTML bukan JSON (kemungkinan hostUrl salah atau session expired). URL=" & sURL & " | Awal response=" & responseContent.Substring(0, Math.Min(300, responseContent.Length)), "clsItem.SaveItemCat")
                Else
                    Try
                        resultTrans = JsonConvert.DeserializeObject(Of clsItem.Response)(responseContent)
                        errLog(responseContent, "clsItem.SaveItemCat JsonResult")
                    Catch exJson As Exception
                        errLog("SaveItemCat: Gagal parse JSON: " & exJson.Message, "clsItem.SaveItemCat")
                    End Try
                End If
            End If
        End If

        Return resultTrans

    End Function

    Private Sub SendDataInChunks(dataSet As DataSet, chunkSize As Integer)
        ' Check if the DataSet is not null and contains at least one table
        If dataSet IsNot Nothing AndAlso dataSet.Tables.Count > 0 Then
            ' Assuming you want to process the first table in the DataSet
            Dim dataTable As DataTable = dataSet.Tables(0)

            ' Calculate the number of chunks based on the chunk size
            Dim totalChunks As Integer = Math.Ceiling(dataTable.Rows.Count / chunkSize)

            ' Loop through each chunk
            For chunkIndex As Integer = 0 To totalChunks - 1
                ' Get a subset of rows for the current chunk
                Dim startIndex As Integer = chunkIndex * chunkSize
                Dim endIndex As Integer = Math.Min((chunkIndex + 1) * chunkSize, dataTable.Rows.Count)
                Dim chunkRows As DataRow() = dataTable.AsEnumerable().Skip(startIndex).Take(chunkSize).ToArray()

                ' Process the current chunk (replace this with your actual processing logic)
                ProcessChunk(chunkRows)
            Next
        Else
            ' Console.WriteLine("Invalid DataSet or DataSet contains no tables.")
        End If
    End Sub

    Private Sub ProcessChunk(chunkRows As DataRow())
        ' Process the current chunk of rows (replace this with your actual processing logic)
        For Each row As DataRow In chunkRows
            ' Access data in the row (you should replace this with your actual data processing logic)
            'Console.WriteLine(row(0)) ' Assuming you want to print the values of the first column
        Next
    End Sub
    Public Function SaveItem(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String)
        Dim sJson As String = ""
        Dim sURL As String = ""
        Dim myWebReq As HttpWebRequest
        Dim myWebResp As HttpWebResponse
        Dim encoding As New System.Text.UTF8Encoding
        Dim cItemSave As New clsItem.Request
        Dim cItemUpdate As New clsItem.RequestUpdate
        Dim listItemSave As New List(Of clsItem.Request.Datum)
        Dim responseContent As String = ""
        Dim resultTrans As New clsItem.Response
        Dim ResultSearch As New ResponseSearchItem
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Dim itemCounts As Integer = 0
        'Dim failinsert As Integer = 0
        With cCMD
            Try

                If sGlobalActiveBrandId = 2 Then
                    ' ── Bandingkan item flag>=2 dengan Accurate Online ──────────────────────────
                    ' Jika item sudah ditandai berhasil sync tapi ternyata tidak ada lagi
                    ' di Accurate (misal dihapus manual), reset bit flag agar ikut dikirim ulang.
                    errLog("Mengecek item flag>=2 brandid=2 yang mungkin hilang dari Accurate...", "clsItem.SaveItem")
                    Dim cCMDCheck As New clsCMD(sConnection)
                    Dim dtFlag2 As DataTable = Nothing
                    Try
                        cCMDCheck.resetCmd("SELECT itemNo FROM cld_itemmaster WHERE brandid=2 AND flag>=2")
                        dtFlag2 = cCMDCheck.selectDatatable()
                    Catch exCheck As Exception
                        errLog("Gagal ambil item flag>=2: " & exCheck.Message, "clsItem.SaveItem")
                    Finally
                        cCMDCheck.dispose()
                    End Try

                    If dtFlag2 IsNot Nothing Then
                        For Each rowCheck As DataRow In dtFlag2.Rows
                            Dim checkItemNo As String = rowCheck("itemNo").ToString()
                            Try
                                Dim searchResult As ResponseSearchItem = SearchItemByID(accesstoken, sessionID, hostUrl, checkItemNo)
                                Dim foundInAccurate As Boolean = (searchResult IsNot Nothing AndAlso
                                                                  searchResult.d IsNot Nothing AndAlso
                                                                  searchResult.d.Count > 0 AndAlso
                                                                  searchResult.d(0).id <> 0)
                                If Not foundInAccurate Then
                                    ' Item tidak ada di Accurate → reset flag agar dikirim ulang
                                    Dim cCMDReset As New clsCMD(sConnection)
                                    Try
                                        cCMDReset.resetCmd("UPDATE cld_itemmaster SET flag=flag & ~2 WHERE itemNo='" & checkItemNo & "' AND brandid=2")
                                        cCMDReset.executeNonQuery()
                                        errLog("Item " & checkItemNo & " tidak ditemukan di Accurate (flag>=2 tapi tidak ada). Bit flag 2 direset.", "clsItem.SaveItem")
                                    Finally
                                        cCMDReset.dispose()
                                    End Try
                                End If
                            Catch exSearch As Exception
                                errLog("Gagal cek item " & checkItemNo & " ke Accurate: " & exSearch.Message, "clsItem.SaveItem")
                            End Try
                        Next
                    End If
                    ' ────────────────────────────────────────────────────────────────────────────

                    .resetCmd("SELECT COUNT(*) FROM cld_itemmaster WHERE brandid=2 AND flag<2 AND majorgroup=0")
                    Dim unmappedCount = CInt(.executeScaler())
                    .dispose()
                    If unmappedCount > 0 Then
                        errLog("ADA item yang majorgroupnya belum di set (jumlah: " & unmappedCount & ")", "clsItem.SaveItem")
                    End If
                    .resetCmd("SELECT * FROM cld_itemmaster WHERE brandid=2 AND flag<2 AND majorgroup<>0")
                ElseIf sGlobalActiveBrandId = 1 Then
                    ' Sync item brandid=1 yang belum tersync (flag bit-0 = 0)
                    .resetCmd("SELECT * FROM cld_itemmaster WHERE brandid=1 AND (flag & 1) = 0")
                Else
                    .resetCmd("select * from cld_itemmaster where flag=0")
                End If
                ds = .selectDataset
                .dispose()

                If ds.Tables(0).Rows.Count > 0 Then
                    itemCounts = ds.Tables(0).Rows.Count
                    ' Assuming you want to process the first table in the DataSet
                    Dim dataTable As DataTable = ds.Tables(0)

                    ' Calculate the number of chunks based on the chunk size
                    Dim totalChunks As Integer = Math.Ceiling(dataTable.Rows.Count / 100)

                    ' Loop through each chunk
                    For chunkIndex As Integer = 0 To totalChunks - 1
                        ' Get a subset of rows for the current chunk
                        Dim startIndex As Integer = chunkIndex * 100
                        Dim endIndex As Integer = Math.Min((chunkIndex + 1) * 100, dataTable.Rows.Count)
                        Dim chunkRows As DataRow() = dataTable.AsEnumerable().Skip(startIndex).Take(100).ToArray()

                        ' Process the current chunk (replace this with your actual processing logic)
                        ' ProcessChunk(chunkRows)

                        For Each data As DataRow In chunkRows
                            'errLog(accesstoken, " accesstoken")
                            'errLog(sessionID, " sessionID")
                            'errLog(hostUrl, " hostUrl")
                            'errLog(data.Item("ItemNo"), "item")
                            ResultSearch = SearchItemByID(accesstoken, sessionID, hostUrl, data.Item("ItemNo"))
                            'errLog(ResultSearch.ToString, "ResultSearchitem")
                            If ResultSearch.d.Count > 0 Then
                                errLog(ResultSearch.d.Count, "ResultSearch.d.Count > 0")
                                For Each a In ResultSearch.d

                                    If a.id <> 0 Then
                                        errLog(a.id, "a.id")
                                        With cItemUpdate
                                            .data = getItemsUpdate(data, a.id)
                                        End With
                                        sJson = JsonConvert.SerializeObject(cItemUpdate)

                                        Dim dataserialize As Byte() = encoding.GetBytes(sJson)
                                        sURL = hostUrl & "/accurate/api/item/bulk-save.do"
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
                                            Dim myWebResp1 As HttpWebResponse
                                            myWebResp1 = DirectCast(ex.Response, HttpWebResponse)
                                            Dim sr1 As StreamReader
                                            sr1 = New StreamReader(myWebResp1.GetResponseStream())
                                            Dim err As String = sr1.ReadToEnd()
                                            responseContent = err
                                        End Try

                                        If responseContent IsNot Nothing Then
                                            If responseContent <> "" Then
                                                resultTrans = JsonConvert.DeserializeObject(Of Response)(responseContent)
                                            End If
                                        End If

                                        If resultTrans.s Then
                                            Dim targetFlag1 As Integer = If(sGlobalActiveBrandId = 2, 2, 1)
                                            .resetCmd("UPDATE cld_itemmaster SET flag = flag | " & targetFlag1 & " WHERE itemno='" & data.Item("itemNo") & "' AND brandid=" & sGlobalActiveBrandId)
                                            .executeNonQuery()
                                            .dispose()
                                        Else
                                            Dim asd As String = ""
                                        End If
                                    End If
                                Next
                            Else
                                With cItemSave
                                    .data = getItems(data)
                                End With

                                sJson = JsonConvert.SerializeObject(cItemSave)
                                Dim dataserialize As Byte() = encoding.GetBytes(sJson)
                                sURL = hostUrl & "/accurate/api/item/bulk-save.do"
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
                                        Dim myWebResp1 As HttpWebResponse
                                        myWebResp1 = DirectCast(ex.Response, HttpWebResponse)
                                        Dim sr1 As StreamReader
                                        sr1 = New StreamReader(myWebResp1.GetResponseStream())
                                        Dim err As String = sr1.ReadToEnd()
                                        responseContent = err
                                    Else
                                        responseContent = "{""s"":false,""d"":[{""d"":[""" & ex.Message.Replace("""", "\""") & """],""s"":false}]}"
                                    End If
                                End Try

                                If responseContent IsNot Nothing Then
                                    If responseContent <> "" Then
                                        resultTrans = JsonConvert.DeserializeObject(Of Response)(responseContent)
                                    End If
                                End If

                                If resultTrans.s Then
                                    errLog(data.Item("ItemNo") & " berhasil disimpan", "clsItem.SaveItem")
                                    Dim targetFlag2 As Integer = If(sGlobalActiveBrandId = 2, 2, 1)
                                    .resetCmd("UPDATE cld_itemmaster SET flag = flag | " & targetFlag2 & " WHERE itemno='" & data.Item("itemNo") & "' AND brandid=" & sGlobalActiveBrandId)
                                    .executeNonQuery()
                                    .dispose()
                                Else
                                    errLog("Gagal simpan item " & data.Item("itemNo") & ". Response: " & responseContent, "clsItem.SaveItem")
                                    Try
                                        .resetCmd("INSERT INTO cld_log (type, name, status, datetime, custid, remarks, olid) VALUES ('Acc_sync','" & data.Item("itemNo") & "','failed','" & Now().Date.ToString("yyyy-MM-dd HH:mm:ss") & "'," & scustid & " ,'Item Master' ,'" & solid & "')")
                                        .executeNonQuery()
                                    Catch exLog As Exception
                                        errLog("Gagal insert ke cld_log: " & exLog.Message, "clsItem.SaveItem")
                                    End Try
                                    .dispose()
                                    failinsert = failinsert + 1
                                End If
                            End If


                        Next





                    Next


                End If


            Catch ex As Exception
                errLog(Err.Description, "clsItem.getItems " & responseContent)
                Err.Clear()
            End Try

            'If responseContent IsNot Nothing Then
            '    If responseContent <> "" Then
            '        resultTrans.d = JsonConvert.DeserializeObject(Of clsItem.Response.dd)(responseContent)
            '        '  ErrLog(responseContent, " JsonResult", False)
            '    End If
            'End If
        End With

        Return resultTrans

    End Function


    'Public Function UpdateItems(ByVal accesstoken As String, ByVal sessionID As String, ByVal hostUrl As String, ByVal connUrl As String, ByVal limitFrom As Integer, ByVal LimitTo As Integer)
    '    Dim sJson As String = ""
    '    Dim sURL As String = ""
    '    Dim myWebReq As HttpWebRequest
    '    Dim myWebResp As HttpWebResponse
    '    Dim encoding As New System.Text.UTF8Encoding
    '    Dim cItemSave As New clsItem.RequestUpdate
    '    Dim listItemSave As New List(Of clsItem.RequestUpdate.Datum)
    '    Dim responseContent As String = ""
    '    Dim resultTrans As New clsItem.Response
    '    ' Dim cCMD As New clsCMD(sKoneksi)
    '    Dim ds As New DataSet
    '    Try
    '        With cItemSave
    '            .data = getItemsUpdate(connUrl, limitFrom, LimitTo)
    '        End With

    '        sJson = JsonConvert.SerializeObject(cItemSave)
    '        Dim dataserialize As Byte() = encoding.GetBytes(sJson)
    '        sURL = hostUrl & "/accurate/api/item/bulk-save.do"
    '        ServicePointManager.SecurityProtocol = DirectCast(3072, SecurityProtocolType)
    '        myWebReq = DirectCast(WebRequest.Create(sURL), HttpWebRequest)

    '        With myWebReq
    '            .ContentType = "application/json"
    '            .Headers.Add("Authorization: Bearer " & accesstoken)
    '            .Headers.Add("X-Session-ID:" & sessionID)
    '            .Method = "POST"
    '            .Timeout = 30000
    '        End With

    '        Dim myStream As Stream = myWebReq.GetRequestStream()

    '        With myStream
    '            If dataserialize.Length > 0 Then
    '                .Write(dataserialize, 0, dataserialize.Length)
    '                .Close()
    '            End If
    '        End With

    '        Try
    '            Dim sr As StreamReader

    '            myWebResp = DirectCast(myWebReq.GetResponse(), HttpWebResponse)
    '            sr = New StreamReader(myWebResp.GetResponseStream())
    '            Dim StatusCode As String = ""
    '            StatusCode = myWebResp.StatusCode

    '            responseContent = sr.ReadToEnd()
    '            sr.Dispose()
    '            myWebResp.Close()



    '        Catch ex As WebException
    '            Dim myWebResp1 As HttpWebResponse
    '            myWebResp1 = DirectCast(ex.Response, HttpWebResponse)
    '            Dim sr1 As StreamReader
    '            sr1 = New StreamReader(myWebResp1.GetResponseStream())
    '            Dim err As String = sr1.ReadToEnd()
    '            responseContent = err
    '        End Try

    '    Catch ex As Exception
    '        ' ErrLog(ex.Message.ToString(), "clsProc: syncTransactions", False)
    '        Err.Clear()
    '    End Try

    '    If responseContent IsNot Nothing Then
    '        If responseContent <> "" Then
    '            resultTrans = JsonConvert.DeserializeObject(Of clsItem.Response)(responseContent)
    '            '  ErrLog(responseContent, " JsonResult", False)
    '        End If
    '    End If

    '    Return resultTrans

    'End Function




    Public Function getItems(ByVal dtrw As DataRow)
        Dim cCMD As New clsCMD(sConnection)
        ' Dim ds As New DataSet
        Dim listData As New List(Of Request.Datum)
        Dim SalesID As String = ""
        Try
            With cCMD


                '   For Each dtrw As DataRow In ds

                Dim MajorGroupName As String = ""
                .resetCmd("select name from cld_majorgroup where id ='" & dtrw.Item("majorgroup") & "'")
                MajorGroupName = .executeScaler
                .dispose()

                With listData
                    .Add(New Request.Datum With
                         {
                             .itemType = "NON_INVENTORY",
                             .name = dtrw.Item("itemname"),
                             .calculateGroupPrice = False,
                             .cogsGlAccountNo = "",
                             .controlQuantity = False,
                             .defaultDiscount = "",
                             .detailGroup = getDetailGroups(dtrw.Item("itemgroup")),
                             .detailOpenBalance = getOpenBalance(),
                             .goodTransitGlAccountNo = "",
                             .inventoryGlAccountNo = "",
                             .itemCategoryName = MajorGroupName,
                             .manageExpired = False,
                             .manageSN = False,
                             .minimumQuantity = 0,
                             .minimumQuantityReorder = 0,
                             .no = dtrw.Item("itemNo"),
                             .notes = "",
                             .percentTaxable = 100,
                             .preferedVendorName = "",
                             .printDetailGroup = False,
                             .purchaseRetGlAccountNo = "",
                             .ratio2 = 0,
                             .ratio3 = 0,
                             .ratio4 = 0,
                             .ratio5 = 0,
                             .salesDiscountGlAccountNo = "",
                             .salesGlAccountNo = "",
                             .salesRetGlAccountNo = "",
                             .serialNumberType = "",
                             .substituted = False,
                             .substitutedItemNo = "",
                             .tax1Name = itemtax,
                             .tax2Name = "",
                             .tax3Name = "",
                             .tax4Name = "",
                             .typeAutoNumber = 0,
                             .unBilledGlAccountNo = "",
                             .unit1Name = "PCS",
                             .unit2Name = "",
                             .unit2Price = 0,
                             .unit3Name = "",
                             .unit3Price = 0,
                             .unit4Name = "",
                             .unit4Price = 0,
                             .unit5Name = "",
                             .unit5Price = 0,
                             .unitPrice = dtrw.Item("Price"),
                             .upcNo = "",
                             .usePPn = False,
                             .useWholesalePrice = False,
                             .vendorPrice = 0,
                             .vendorUnitName = ""
                         })
                End With
                '  Next
            End With

        Catch ex As Exception
            errLog(Err.Description, "clsItem.getItems")
            Err.Clear()
        End Try
        Return listData
    End Function

    Public Function getItemsUpdate(ByVal dtrw As DataRow, ByVal ItemId As String) 'As List(Of RequestUpdate.Datum)
        Dim cCMD As New clsCMD(sConnection)
        ' Dim ds As New DataSet
        Dim listData As New List(Of RequestUpdate.Datum)
        Dim SalesID As String = ""

        Try
            With cCMD

                ' For Each dtrw As DataRow In ds
                Dim MajorGroupName As String = ""
                Dim majorCOA As String = ""
                Dim dt As New DataTable
                .resetCmd("select * from cld_majorgroup where id ='" & dtrw.Item("majorgroup") & "'")
                dt = .selectDatatable
                MajorGroupName = dt.Rows(0).Item("name")
                majorCOA = dt.Rows(0).Item("Majorcoa")
                .dispose()


                With listData
                    .Add(New RequestUpdate.Datum With
                         {
                             .itemType = "NON_INVENTORY",
                             .name = dtrw.Item("itemname"),
                             .calculateGroupPrice = False,
                             .controlQuantity = False,
                             .defaultDiscount = "",
                            .itemCategoryName = MajorGroupName,
                            .salesGlAccountNo = majorCOA,
                             .manageExpired = False,
                             .manageSN = False,
                             .minimumQuantity = 0,
                             .minimumQuantityReorder = 0,
                             .no = dtrw.Item("itemNo"),
                             .notes = "",
                             .percentTaxable = 100,
                             .preferedVendorName = "",
                             .printDetailGroup = False,
                             .ratio2 = 0,
                             .ratio3 = 0,
                             .ratio4 = 0,
                             .ratio5 = 0,
                             .serialNumberType = "",
                             .substituted = False,
                             .substitutedItemNo = "",
                             .tax1Name = itemtax,
                             .tax2Name = "",
                             .tax3Name = "",
                             .tax4Name = "",
                             .typeAutoNumber = 0,
                              .unit1Name = "PCS",
                             .unit2Name = "",
                             .unit2Price = 0,
                             .unit3Name = "",
                             .unit3Price = 0,
                             .unit4Name = "",
                             .unit4Price = 0,
                             .unit5Name = "",
                             .unit5Price = 0,
                             .unitPrice = dtrw.Item("Price"),
                             .upcNo = "",
                             .usePPn = False,
                             .useWholesalePrice = False,
                             .vendorPrice = 0,
                             .vendorUnitName = "",
                             .id = ItemId
                         })
                End With
                '  Next
            End With
        Catch ex As Exception
            errLog(Err.Description, "clsItem.getItemsUpdate")
            Err.Clear()
        End Try
        Return listData
    End Function


    Public Function getDetailGroups(ByVal itemGroupID As String) As List(Of Request.Detailgroup)
        Dim cCMD As New clsCMD(sConnection)
        Dim ds As New DataSet
        Dim listData As New List(Of Request.Detailgroup)
        Dim SalesID As String = ""
        Try
            With cCMD
                .resetCmd("select * from cld_itemgroup where id ='" & itemGroupID & "'")
                ds = .selectDataset
                .dispose()

                If ds.Tables(0).Rows.Count > 0 Then
                    For Each a As DataRow In ds.Tables(0).Rows
                        With listData
                            .Add(New Request.Detailgroup With
                                         {
                                             .detailName = a.Item("Name"),
                                             .itemNo = a.Item("id"),
                                             .itemUnitName = "PCS"
                                         })


                        End With
                    Next
                Else
                    With listData
                        .Add(New Request.Detailgroup With
                                     {
                                         .detailName = "",
                                         .itemNo = "",
                                         .itemUnitName = ""
                                     })


                    End With
                End If

            End With

        Catch ex As Exception
            errLog(Err.Description, "clsItem.getDetailGroups")
            Err.Clear()
        End Try
        Return listData
    End Function

    Public Function getOpenBalance() As List(Of Request.Detailopenbalance)
        '  Dim cCMD As New clsCMD(connUrl)
        Dim ds As New DataSet
        Dim listData As New List(Of Request.Detailopenbalance)
        Dim SalesID As String = ""
        Try

            With listData

            End With
        Catch ex As Exception
            errLog(Err.Description, "clsItem.getOpenBalance")
            Err.Clear()
        End Try
        Return listData
    End Function


End Class
