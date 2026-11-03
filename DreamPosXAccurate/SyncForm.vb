Imports System.Threading
Imports Newtonsoft.Json

Public Class SyncForm
    Dim Token As New clsToken.ProcessToken
    Dim sessionStore As New clsStore.ResponseOpenDB
    Dim Store As New clsStore
    Dim listStore As New clsStore.Response
    Dim account As New clsPayment.Request
    Dim itemProcess As New clsItem
    Dim transProcess As New clsTransactions
    Dim payProcess As New clsPayment
    Dim second As Integer
    Dim authReady As Boolean = False
        Private Sub SyncForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Sync Data - DreamposXAccurate v" & Application.ProductVersion
        Try
            Dim cCMD As New clsCMD(sConnection)
            cCMD.resetCmd("SELECT ID, Name FROM cld_brand ORDER BY Name")
            Dim dtBrand As DataTable = cCMD.selectDatatable()
            If dtBrand IsNot Nothing AndAlso dtBrand.Rows.Count > 0 Then
                Dim rowAll As DataRow = dtBrand.NewRow()
                rowAll("ID") = 0
                rowAll("Name") = "ALL BRANDS"
                dtBrand.Rows.InsertAt(rowAll, 0)

                cmbBrand.DataSource = dtBrand
                cmbBrand.DisplayMember = "Name"
                cmbBrand.ValueMember = "ID"
                cmbBrand.SelectedIndex = 0
            End If
        Catch ex As Exception
            errLog("Gagal memuat daftar brand: " & ex.Message, "SyncForm_Load")
        End Try
    End Sub

    Private Sub cmbBrand_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbBrand.SelectedIndexChanged
        If cmbBrand.SelectedValue IsNot Nothing AndAlso TypeOf cmbBrand.SelectedValue Is Integer Then
            Dim selectedBrandId As Integer = CInt(cmbBrand.SelectedValue)
            sGlobalActiveBrandId = selectedBrandId
            ' Brand 1 and Brand 2 both use Accurate branches mapped by cld_outlet.kode2.
            sGlobalActiveBrandIsMultiBranch = True
            errLog("[SYNC_MODE] BrandID=" & selectedBrandId & " | MultiBranch=True | Version=" & Application.ProductVersion, "UI")
            If selectedBrandId > 0 Then
                LoadOAuthCredentialFromConfig(selectedBrandId)
                errLog("Brand diubah ke " & cmbBrand.Text & ". Memuat kredensial Accurate dari App.config untuk brand ID " & selectedBrandId & ".", "UI")
            Else
                LoadOAuthCredentialFromConfig(0)
                ' If ALL BRANDS, reset solid to default
                solid = My.Settings.solid
            End If
        End If
    End Sub
    'errLog(Err.Description, "SyncForm.Timer1_Tick")
    '       Err.Clear()

    Private Function GetToken() As Boolean
        Dim acctoken As String = ""
        Try
            authReady = False
            LoadOAuthCredentialFromConfig(sGlobalActiveBrandId)
            acctoken = Token.GetToken(sClientID, sUser, sPw) 'untuk login | client id,email/nohp, password
            TextBox1.Text = acctoken
            If acctoken = "" Then
                errLog("Access token kosong. Login OAuth belum berhasil atau callback belum diterima.", "SyncForm.GetToken")
                Return False
            End If

            listStore = Store.getStoreList(acctoken) 'untuk nampilin store nya, untuk diambil storeid nya
            If listStore.d Is Nothing OrElse listStore.d.Count = 0 Then
                errLog("Access token valid, tetapi Accurate mengembalikan daftar database kosong. Pastikan akun OAuth memiliki akses ke Data Usaha Accurate dan aplikasi sudah diotorisasi untuk database tersebut.", "SyncForm.GetToken")
                Return False
            End If

            Dim storeID As String = ""
            For Each b In listStore.d
                If b.alias.ToLower().Contains(sStoreName.ToLower()) Then
                    storeID = b.id
                    Exit For
                End If
            Next

            If storeID = "" Then
                errLog("Store " & sStoreName & " tidak ditemukan pada akun Accurate", "SyncForm.GetToken")
                Return False
            End If

            sessionStore = Store.openDBStore(acctoken, storeID) 'setelah dapat store id, untuk dapetin session nya, supaya bisa akses api accurate
              ' NOTE: sessionStore.host diperoleh dari openDBStore dan tidak boleh ditimpa dengan sAccountingUrl (OAuth URL)
            TextBox2.Text = sessionStore.session
            If sessionStore.session = "" OrElse sessionStore.host = "" Then
                errLog("Session atau host Accurate kosong setelah open-db", "SyncForm.GetToken")
                Return False
            End If

            authReady = True

            ' Fetch branch list and log it (commented out because it requires branch_view scope which isn't always available)
            ' Store.getBranchList(acctoken, sessionStore.session, sessionStore.host)

            Return True
        Catch ex As Exception
            errLog(ex.Message, "SyncForm.GetToken")
            Err.Clear()
            Return False
        End Try
    End Function


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        GetToken()
    End Sub

    Private Function IsReadyToSync(ByVal source As String) As Boolean
        If Not authReady OrElse TextBox1.Text = "" OrElse TextBox2.Text = "" OrElse sessionStore.host = "" Then
            errLog("Sync dibatalkan karena OAuth/session/host belum siap. Jalankan Get Access sampai berhasil terlebih dahulu.", source)
            Return False
        End If

        Return True
    End Function

    Private Function IsJournalAlreadySynced(ByVal transDate As Date, ByVal syncType As String) As Boolean
        Dim cCMD As New clsCMD(sConnection)
        Try
            With cCMD
                .resetCmd("SELECT COUNT(*) FROM cld_syncsales " &
                          "WHERE busidate='" & transDate.Date.ToString("yyyy-MM-dd 00:00:00") & "' " &
                          "AND olid=" & solid & " AND custid='" & scustid & "' AND Type='" & syncType & " Success'")
                Return Val(.executeScaler(False)) > 0
            End With
        Catch ex As Exception
            errLog(ex.Message, "SyncForm.IsJournalAlreadySynced")
            Err.Clear()
            Return False
        Finally
            cCMD.dispose()
        End Try
    End Function

    Private Function HasJournalValidationIssue(ByVal transDate As Date) As Boolean
        Dim dt As New DataTable
        Dim cCMD As New clsCMD(sConnection)
        Try
            With cCMD
                .resetCmd("SELECT busidate, olid, custid " &
                          ",IFNULL(((SUM(subtotal-discount)) - (SELECT SUM(subtotal-discount) FROM cld_sumsalesdaily AS smd WHERE smd.olid = sm.olid AND smd.custid = sm.custid AND smd.busidate = sm.BusiDate)),0) AS selisih1 " &
                          ",IFNULL((SELECT SUM(Total) FROM cld_sumsalesdetaildaily AS smd WHERE smd.olid = sm.olid AND smd.custid = sm.custid AND smd.busidate = sm.BusiDate),0) AS SumSlsDetail " &
                          ",IFNULL(((SUM(subtotal - discount)) - (SELECT SUM(Total) FROM cld_sumsalesdetaildaily AS smd WHERE smd.olid = sm.olid AND smd.custid = sm.custid AND smd.busidate = sm.BusiDate)),0) AS selisih2 " &
                          ",IFNULL((SUM(total)-(SELECT SUM(total) FROM cld_sumpaymentdaily p WHERE p.busidate =sm.`BusiDate` AND p.olid = sm.`olId`)),0) AS pdif " &
                          "FROM cld_salesmaster sm " &
                          "WHERE busidate ='" & transDate.Date.ToString("yyyy-MM-dd 00:00:00") & "' AND sm.`flag` = 1 " &
                          "GROUP BY busidate, olid, custid " &
                          "HAVING(selisih1 < -1 OR selisih1 > 1 Or pdif < -1000 OR pdif > 1000 or selisih2 < -1000 OR selisih2 > 1000) " &
                          "ORDER BY busidate DESC")
                dt = .selectDatatable
            End With

            If dt.Rows.Count > 0 Then
                errLog("Peringatan: Jurnal tanggal " & transDate.ToString("yyyy-MM-dd") & " terdapat validasi summary/detail/payment yang belum balance. Jumlah issue: " & dt.Rows.Count & " (Sync tetap dilanjutkan)", "SyncForm.HasJournalValidationIssue")
                Return False
            End If

            Return False
        Catch ex As Exception
            errLog(ex.Message, "SyncForm.HasJournalValidationIssue")
            Err.Clear()
            Return True
        Finally
            cCMD.dispose()
        End Try
    End Function

    Private Sub SyncSalesInvoiceDailyRange(ByVal dateFrom As Date, ByVal source As String)
        Dim currentDate As Date = dateFrom.Date
        Dim endDate As Date = Date.Today.AddDays(-1)

        If currentDate > endDate Then
            errLog("Sync harian dibatalkan karena Trans Date lebih besar dari hari ini: " & currentDate.ToString("yyyy-MM-dd"), source)
            Exit Sub
        End If

        errLog("Mulai sync harian dari " & currentDate.ToString("yyyy-MM-dd") & " sampai " & endDate.ToString("yyyy-MM-dd"), source)
        errLog("[DIAG] SyncSalesInvoiceDailyRange dipanggil | solid='" & solid & "' | scustid='" & scustid & "' | doFaktur=" & chkSyncFaktur.Checked.ToString() & " | doJurnal=" & chkSyncJurnal.Checked.ToString(), source)

        Dim doFaktur As Boolean = chkSyncFaktur.Checked
        Dim doJurnal As Boolean = chkSyncJurnal.Checked

        With sessionStore
            Do While currentDate <= endDate
                Dim alreadySyncedDetail As Boolean = IsJournalAlreadySynced(currentDate, "Detail")
                Dim alreadySyncedPayment As Boolean = IsJournalAlreadySynced(currentDate, "Payment")
                errLog("[DIAG] Tanggal " & currentDate.ToString("yyyy-MM-dd") & " | AlreadySynced-Detail=" & alreadySyncedDetail.ToString() & " | AlreadySynced-Payment=" & alreadySyncedPayment.ToString(), source)

                If Not HasJournalValidationIssue(currentDate) Then
                    If doFaktur Then
                        Dim isComplete As Boolean = False
                        Dim existingInvoiceIds As New List(Of Long)
                        Dim existingReceiptIds As New List(Of Long)
                        Dim branchPrefix As String = ""
                        Dim cCMD2 As New clsCMD(My.Settings.sKoneksi)
                        Try
                            If sGlobalActiveBrandIsMultiBranch Then
                                cCMD2.resetCmd("SELECT IFNULL(kode2, '') FROM cld_outlet WHERE ID=" & solid)
                            Else
                                cCMD2.resetCmd("SELECT IFNULL(Kode, '') FROM cld_outlet WHERE ID=" & solid)
                            End If
                            Dim obj = cCMD2.executeScaler(False)
                            If obj IsNot Nothing Then branchPrefix = obj.ToString()
                            If Not sGlobalActiveBrandIsMultiBranch AndAlso String.IsNullOrEmpty(branchPrefix) Then
                                branchPrefix = solid.ToString()
                            End If
                        Catch
                        Finally
                            cCMD2.dispose()
                        End Try

                        errLog("Cek status sinkronisasi di Accurate Online untuk " & branchPrefix & " tanggal " & currentDate.ToString("yyyy-MM-dd"), source)
                        Dim checkSuccess As Boolean = transProcess.CheckAccurateSyncStatus(TextBox1.Text, TextBox2.Text, .host, currentDate, branchPrefix, isComplete, existingInvoiceIds, existingReceiptIds)

                        If checkSuccess AndAlso isComplete Then
                            errLog("Pengiriman Faktur dan Penerimaan harian tanggal " & currentDate.ToString("yyyy-MM-dd") & " dilewati karena sudah SUCCESS, LENGKAP dan LUNAS di Accurate. Tetap GET ulang detail faktur dan bandingkan subtotal/tax dengan cld_sumsalesdaily.", source)
                            transProcess.ReconcileSalesDaily(TextBox1.Text, TextBox2.Text, .host, currentDate, branchPrefix)
                        Else
                            ' Delete if any exists (Receipt first, then Invoice)
                            If existingReceiptIds.Count > 0 OrElse existingInvoiceIds.Count > 0 Then
                                errLog("Ditemukan data yang belum lengkap/lunas di Accurate. Menghapus data lama untuk dikirim ulang...", source)
                                For Each rId In existingReceiptIds
                                    transProcess.DeleteAccurateTransactions(TextBox1.Text, TextBox2.Text, .host, "sales-receipt", rId)
                                Next
                                For Each iId In existingInvoiceIds
                                    transProcess.DeleteAccurateTransactions(TextBox1.Text, TextBox2.Text, .host, "sales-invoice", iId)
                                Next
                            End If

                            ' Sync Data Fresh
                            Dim invoiceResult = transProcess.SaveTransSumSalesDaily(TextBox1.Text, TextBox2.Text, .host, My.Settings.sKoneksi, currentDate)

                            If IsSalesInvoiceResultSuccessful(invoiceResult) Then
                                ' Tambahkan jeda waktu 5 detik agar Faktur Penjualan (bulk save) selesai diproses
                                ' di background worker Accurate Online sebelum mengirim Penerimaan.
                                ' Jika tidak, penerimaan akan menggantung (Unapplied) karena faktur belum tercipta.
                                Threading.Thread.Sleep(5000)

                                transProcess.SaveTransReceiptSumSalesDaily(TextBox1.Text, TextBox2.Text, .host, My.Settings.sKoneksi, currentDate)
                            Else
                                errLog("Penerimaan penjualan tanggal " & currentDate.ToString("yyyy-MM-dd") & " tidak dikirim karena faktur penjualan belum berhasil dibuat.", source)
                            End If

                            ' Final check after sync
                            Dim isCompleteAfterSync As Boolean = False
                            Dim dummyIds1 As New List(Of Long)
                            Dim dummyIds2 As New List(Of Long)
                            transProcess.CheckAccurateSyncStatus(TextBox1.Text, TextBox2.Text, .host, currentDate, branchPrefix, isCompleteAfterSync, dummyIds1, dummyIds2)
                            
                            If Not isCompleteAfterSync Then
                                errLog("Peringatan: Hasil akhir sync " & branchPrefix & " tanggal " & currentDate.ToString("yyyy-MM-dd") & " ternyata MASIH GAGAL/TIDAK LENGKAP di Accurate. Menghapus status sukses di lokal (cld_syncsales) agar diulang nanti...", source)
                                Dim cCMD3 As New clsCMD(My.Settings.sKoneksi)
                                Try
                                    cCMD3.resetCmd("DELETE FROM cld_syncsales WHERE busidate='" & currentDate.ToString("yyyy-MM-dd 00:00:00") & "' AND olid=" & solid & " AND custid='" & scustid & "' AND Type LIKE '% Success'")
                                    cCMD3.executeNonQuery()
                                Catch ex As Exception
                                Finally
                                    cCMD3.dispose()
                                End Try
                            End If
                            ' Audit after an upload attempt; existing invoices are audited in the branch above.
                            transProcess.ReconcileSalesDaily(TextBox1.Text, TextBox2.Text, .host, currentDate, branchPrefix)
                        End If
                    End If

                    If doJurnal Then
                        ' Sync Jurnal Umum
                        transProcess.SaveTransSumSalesDailyJU(TextBox1.Text, TextBox2.Text, .host, My.Settings.sKoneksi, currentDate)
                    End If
                End If

                currentDate = currentDate.AddDays(1)
            Loop
        End With
    End Sub

    Private Function IsSalesInvoiceResultSuccessful(ByVal result As clsTransactions.ResponseSaveTransSumSalesDaily) As Boolean
        If result Is Nothing OrElse result.d Is Nothing OrElse result.d.Count = 0 Then
            Return False
        End If

        For Each item In result.d
            If item IsNot Nothing AndAlso Not item.s Then
                Dim msg As String = item.GetValue_d()
                If msg Is Nothing OrElse msg.IndexOf("Sudah ada data lain", StringComparison.OrdinalIgnoreCase) < 0 Then
                    Return False
                End If
            End If
        Next

        Return True
    End Function

    Private Sub SyncAllBranchesDailyRange(ByVal dateFrom As Date, ByVal source As String)
        Dim cCMD As New clsCMD(sConnection)
        Dim dtOutlets As DataTable
        Try
            ' Filter outlet: custid cocok, aktif, belum expired, sesuai brand aktif
            ' Untuk multi-branch: wajib punya kode2 (nama cabang Accurate)
            ' Untuk non-multi-branch: semua outlet brand aktif
            Dim outletFilter As String = " WHERE custid = " & scustid & _
                                         " AND active = 1" & _
                                         " AND (eDate IS NULL OR eDate >= CURDATE())" & _
                                         " AND brandID = " & sGlobalActiveBrandId
            Dim outletSQL As String
            If sGlobalActiveBrandIsMultiBranch Then
                outletSQL = "SELECT ID, Kode, kode2 FROM cld_outlet" & outletFilter & " AND IFNULL(kode2,'') <> ''"
            Else
                outletSQL = "SELECT ID, Kode, IFNULL(kode2,'') kode2 FROM cld_outlet" & outletFilter
            End If
            errLog("[DIAG] SyncAllBranchesDailyRange | SQL outlet: " & outletSQL, source)
            cCMD.resetCmd(outletSQL)
            dtOutlets = cCMD.selectDatatable()
        Catch ex As Exception
            errLog("Gagal mengambil daftar cabang lokal: " & ex.Message, "SyncForm.SyncAllBranchesDailyRange")
            Exit Sub
        Finally
            cCMD.dispose()
        End Try

        If dtOutlets Is Nothing OrElse dtOutlets.Rows.Count = 0 Then
            errLog("Tidak ada outlet aktif untuk brandID=" & sGlobalActiveBrandId & ". Sync dibatalkan.", "SyncForm.SyncAllBranchesDailyRange")
            Exit Sub
        End If

        Dim localBranchLog As String = "[DIAG] Outlet brandID=" & sGlobalActiveBrandId & ": "
        For Each row As DataRow In dtOutlets.Rows
            localBranchLog &= "[ID=" & row("ID").ToString() & ", Kode='" & row("Kode").ToString() & "', kode2='" & row("kode2").ToString() & "'] "
        Next
        errLog(localBranchLog, source)

        Dim originalSolid As String = solid
        Dim currentCachedCredHash As String = ""
        Dim validBranches As New List(Of String)

        Try
            For Each row As DataRow In dtOutlets.Rows
                Dim outletID As String = row("ID").ToString()
                Dim outletKode As String = row("Kode").ToString()
                Dim branchName As String = row("kode2").ToString()

                solid = outletID
                LoadOAuthCredentialFromConfig(sGlobalActiveBrandId)

                Dim credHash As String = sClientID & "|" & sUser & "|" & sPw & "|" & sStoreName & "|" & sAccountingUrl

                If credHash <> currentCachedCredHash Then
                    errLog("[DIAG] Kredensial berubah untuk outlet ID=" & outletID & ", mengambil token baru...", source)
                    If Not GetToken() Then
                        errLog("Gagal mendapatkan token untuk outlet [" & outletKode & "] (ID=" & outletID & "). Di-skip.", source)
                        Continue For
                    End If
                    currentCachedCredHash = credHash

                    If sGlobalActiveBrandIsMultiBranch Then
                        validBranches.Clear()
                        Try
                            With sessionStore
                                Dim storeProcess As New clsStore()
                                Dim responseStr As String = storeProcess.getBranchList(TextBox1.Text, .session, .host)
                                If responseStr <> "" Then
                                    Dim respBranch As clsStore.ResponseBranchList = JsonConvert.DeserializeObject(Of clsStore.ResponseBranchList)(responseStr)
                                    If respBranch IsNot Nothing AndAlso respBranch.s AndAlso respBranch.d IsNot Nothing Then
                                        For Each b In respBranch.d
                                            validBranches.Add(b.name.ToUpper())
                                        Next
                                        ' Auto-sinkronisasi nama cabang Accurate -> cld_outlet.kode2
                                        SyncBranchNamesFromAccurate(respBranch.d, sGlobalActiveBrandId, source)
                                    End If
                                End If
                            End With
                        Catch ex As Exception
                            errLog("Gagal mengambil daftar cabang dari Accurate: " & ex.Message, source)
                        End Try
                        If validBranches.Count = 0 Then
                            errLog("Daftar cabang dari Accurate kosong atau gagal diambil. Pastikan scope 'branch_view' sudah aktif.", source)
                        End If
                    End If
                End If

                If sGlobalActiveBrandIsMultiBranch Then
                    ' Refresh branchName dari DB, mungkin sudah diupdate oleh SyncBranchNamesFromAccurate
                    Dim cCMDRefresh As New clsCMD(sConnection)
                    Try
                        cCMDRefresh.resetCmd("SELECT IFNULL(kode2,'') FROM cld_outlet WHERE ID=" & outletID)
                        Dim refreshedVal = cCMDRefresh.executeScaler(False)
                        If refreshedVal IsNot Nothing AndAlso refreshedVal.ToString() <> "" Then
                            branchName = refreshedVal.ToString()
                        End If
                    Catch
                    Finally
                        cCMDRefresh.dispose()
                    End Try

                    errLog("[BRANCH_CHECK] BrandID=" & sGlobalActiveBrandId & " | OutletId=" & outletID & " | Kode=" & outletKode & " | kode2='" & branchName & "' | CabangAccurate=" & String.Join(", ", validBranches), source)
                    If validBranches.Contains(branchName.ToUpper()) Then
                        sGlobalActiveBranchName = branchName
                    Else
                        errLog("Cabang lokal [" & branchName & "] (ID=" & outletID & ") di-skip: cabang belum terverifikasi di daftar cabang Accurate. Periksa cld_outlet.kode2 dan scope branch_view.", source)
                        Continue For
                    End If
                Else
                    sGlobalActiveBranchName = "NONE"
                    errLog("[DIAG] Non-multi-branch | Outlet Kode='" & outletKode & "' (ID=" & outletID & ") diproses tanpa branchName.", source)
                End If

                errLog(">> Memulai sync untuk Outlet: Kode=" & outletKode & " | kode2=" & branchName & " | ID=" & solid, source)
                SyncSalesInvoiceDailyRange(dateFrom, source & " [" & outletKode & "]")
            Next
        Finally
            solid = originalSolid
            LoadOAuthCredentialFromConfig(sGlobalActiveBrandId)
            errLog("[DIAG] solid dikembalikan ke nilai awal: '" & solid & "'", source)
        End Try
    End Sub

    ''' <summary>
    ''' Sinkronisasi nama cabang dari Accurate ke cld_outlet.kode2.
    ''' - Jika kode2 berbeda case dengan nama Accurate -> auto UPDATE + log.
    ''' - Jika kode2 tidak ditemukan di Accurate -> log WARNING saja (tidak auto-update,
    '''   karena mapping ambiguous dan berisiko salah cabang).
    ''' Dipanggil otomatis setiap kali branch list berhasil diambil dari Accurate.
    ''' </summary>
    Private Sub SyncBranchNamesFromAccurate(ByVal accurateBranches As List(Of clsStore.ResponseBranchList.BranchData),
                                             ByVal brandId As Integer,
                                             ByVal source As String)
        Dim branchNames As String = String.Join(", ", accurateBranches.Select(Function(b) b.name).ToList())
        errLog("[SyncBranch] Cek nama cabang brandID=" & brandId & ". Cabang Accurate: [" & branchNames & "]", source)

        ' Dictionary: key=UPPERCASE, value=nama asli dari Accurate
        Dim accBranchDict As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each b In accurateBranches
            If Not accBranchDict.ContainsKey(b.name.ToUpper()) Then
                accBranchDict(b.name.ToUpper()) = b.name
            End If
        Next

        Dim cCMD As New clsCMD(sConnection)
        Try
            cCMD.resetCmd("SELECT ID, Kode, kode2 FROM cld_outlet WHERE brandID=" & brandId & _
                          " AND active=1 AND (eDate IS NULL OR eDate >= CURDATE()) AND IFNULL(kode2,'') <> ''")
            Dim dt As DataTable = cCMD.selectDatatable()
            errLog("[SyncBranch] Jumlah outlet dicek: " & dt.Rows.Count, source)

            Dim countOK As Integer = 0, countUpdated As Integer = 0, countMismatch As Integer = 0

            For Each row As DataRow In dt.Rows
                Dim outletID As String = row("ID").ToString()
                Dim kode As String = row("Kode").ToString()
                Dim kode2 As String = row("kode2").ToString()

                If accBranchDict.ContainsKey(kode2) Then
                    Dim exactName As String = accBranchDict(kode2)
                    If kode2 <> exactName Then
                        ' Perbedaan hanya kapitalisasi -> auto update
                        Dim cCMDUpd As New clsCMD(sConnection)
                        Try
                            cCMDUpd.resetCmd("UPDATE cld_outlet SET kode2='" & exactName.Replace("'", "''") & "' WHERE ID=" & outletID)
                            cCMDUpd.executeNonQuery()
                            errLog("[SyncBranch] UPDATE kode2: Outlet ID=" & outletID & " Kode=" & kode & _
                                   " | '" & kode2 & "' -> '" & exactName & "' (perbedaan kapitalisasi, sudah diperbaiki).", source)
                            countUpdated += 1
                        Catch exUpd As Exception
                            errLog("[SyncBranch] GAGAL update kode2 outlet ID=" & outletID & ": " & exUpd.Message, source)
                        Finally
                            cCMDUpd.dispose()
                        End Try
                    Else
                        countOK += 1
                    End If
                Else
                    ' Nama tidak cocok sama sekali -> warning saja, tidak auto-update
                    errLog("[SyncBranch] *** PERINGATAN: Outlet ID=" & outletID & " Kode=" & kode & _
                           " | kode2='" & kode2 & "' TIDAK DITEMUKAN di Accurate. " & _
                           "Outlet ini akan di-skip saat sync sales! " & _
                           "Update kode2 secara manual sesuai nama cabang di Accurate.", source)
                    countMismatch += 1
                End If
            Next

            errLog("[SyncBranch] Selesai | OK=" & countOK & " | Diupdate=" & countUpdated & " | Tidak cocok=" & countMismatch, source)
        Catch ex As Exception
            errLog("[SyncBranch] Error: " & ex.Message, source)
        Finally
            cCMD.dispose()
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If Not IsReadyToSync("SyncForm.Button2_Click") Then Exit Sub
        With sessionStore
            itemProcess.SaveItem(TextBox1.Text, .session, .host)
        End With
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If Not IsReadyToSync("SyncForm.Button4_Click") Then Exit Sub
        With sessionStore
            payProcess.SaveAccount(TextBox1.Text, .session, .host)

            ' Run InsertMissingPaymentMaster at the end so it waits for user mapping
            payProcess.InsertMissingPaymentMaster()
        End With

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If Not IsReadyToSync("SyncForm.Button3_Click") Then Exit Sub
        If sGlobalActiveBrandId = 0 Then
            SyncAllBrandsSequentially(DateTimePicker1.Value.Date, "SyncForm.Button3_Click")
        Else
            SyncOneBrand(brandId:=sGlobalActiveBrandId, isMultiBranch:=True, dateFrom:=DateTimePicker1.Value.Date, source:="SyncForm.Button3_Click")
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If Not IsReadyToSync("SyncForm.Button5_Click") Then Exit Sub
        If sGlobalActiveBrandId = 0 Then
            SyncAllBrandsSequentially(DateTimePicker1.Value.Date, "SyncForm.Button5_Click")
        Else
            SyncOneBrand(brandId:=sGlobalActiveBrandId, isMultiBranch:=True, dateFrom:=DateTimePicker1.Value.Date, source:="SyncForm.Button5_Click")
        End If
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If Not IsReadyToSync("SyncForm.Button6_Click") Then Exit Sub
        'untuk save item maks hanya 100 record /nembak
        With sessionStore
            itemProcess.SaveItemCat(TextBox1.Text, .session, .host, My.Settings.sKoneksi)
        End With
    End Sub

    Private Sub cekCategory()

    End Sub
    Private Sub cekItem()

    End Sub
    Private Sub cekPayment()

    End Sub

    Private Sub StartProcess()

        second = 0
        Timer1.Start()
        Lblcount.Visible = True
    End Sub

    Private Sub SyncForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        GetToken()
        If authReady Then
            StartProcess()
        Else
            errLog("Auto sync tidak dijalankan karena OAuth/session belum siap", "SyncForm.Shown")
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            second = second + 1
            Lblcount.Text = second

            If second >= sTimer Then
                If Not authReady OrElse TextBox1.Text = "" OrElse TextBox2.Text = "" OrElse sessionStore.host = "" Then
                    Timer1.Stop()
                    errLog("Sync dibatalkan karena OAuth/session/host belum siap", "SyncForm.Timer1_Tick")
                    Exit Sub
                End If

                Timer1.Stop()
                SyncAllBrandsSequentially(DateTimePicker1.Value.Date.AddDays(-8), "SyncForm.Timer1_Tick")

                ' Run InsertMissingPaymentMaster at the end so it waits for user mapping
                payProcess.InsertMissingPaymentMaster()

                Threading.Thread.Sleep(1000)
                second = 0
                End
                'Timer1.Start()
            End If
        Catch ex As Exception
            errLog(Err.Description, "SyncForm.Timer1_Tick")
            Err.Clear()
        End Try

    End Sub

    ''' <summary>
    ''' Sinkronisasi nama MajorGroup di cld_sumsalesdetaildaily agar konsisten dengan cld_itemmaster & cld_majorgroup
    ''' Dibatasi dari H-1 (kemarin) sampai saat ini agar proses ringan dan cepat.
    ''' </summary>
    Private Sub UpdateSalesDetailMajorGroup(ByVal source As String)
        Dim cCMD As New clsCMD(sConnection)
        Try
            Dim sqlUpdate As String = "UPDATE cld_sumsalesdetaildaily s " & _
                                      "JOIN cld_itemmaster i ON i.ItemNo = s.itemno AND i.CustID = s.CustID " & _
                                      "JOIN cld_majorgroup m ON m.id = i.majorgroup AND m.custid = i.CustID " & _
                                      "SET s.MajorGroup = m.Name " & _
                                      "WHERE s.busidate >= CURDATE() - INTERVAL 1 DAY " & _
                                      "AND s.MajorGroup <> m.Name;"
            cCMD.resetCmd(sqlUpdate)
            cCMD.executeNonQuery()
            errLog("[UPDATE-MAJORGROUP] Berhasil menjalankan update MajorGroup cld_sumsalesdetaildaily (H-1 s/d sekarang)", source)
        Catch ex As Exception
            errLog("[UPDATE-MAJORGROUP Error] Gagal update MajorGroup: " & ex.Message, source)
        Finally
            cCMD.dispose()
        End Try
    End Sub

    ''' <summary>
    ''' Loop semua brand; brand terpilih didahulukan. ALL BRANDS memakai urutan 1 lalu 2.
    ''' </summary>
    Private Sub SyncAllBrandsSequentially(ByVal dateFrom As Date, ByVal source As String)
        errLog("=== SyncAllBrandsSequentially dimulai ===", source)

        ' Update nama MajorGroup di cld_sumsalesdetaildaily (H-1 s/d saat ini) sebelum sync ke Accurate
        UpdateSalesDetailMajorGroup(source)

        Dim brandOrder As Integer() = If(sGlobalActiveBrandId = 2, New Integer() {2, 1}, New Integer() {1, 2})
        errLog("[SYNC_ORDER] Semua brand dikirim | BrandTerpilih=" & sGlobalActiveBrandId &
               " | Urutan=" & String.Join(" -> ", brandOrder.Select(Function(id) id.ToString())), source)
        For Each brandId As Integer In brandOrder
            SyncOneBrand(brandId:=brandId, isMultiBranch:=True, dateFrom:=dateFrom, source:=source)
        Next
        errLog("=== SyncAllBrandsSequentially selesai ===", source)
    End Sub

    ''' <summary>
    ''' Sync satu brand: login, sync master data, lalu sync sales ke semua outlet brand tersebut
    ''' </summary>
    Private Sub SyncOneBrand(brandId As Integer, isMultiBranch As Boolean, dateFrom As Date, source As String)
        errLog(">> Mulai SyncOneBrand | brandID=" & brandId & " | isMultiBranch=" & isMultiBranch.ToString(), source)

        ' Simpan state global sebelum diubah
        Dim savedSolid As String = solid
        Dim savedBrandId As Integer = sGlobalActiveBrandId
        Dim savedMultiBranch As Boolean = sGlobalActiveBrandIsMultiBranch

        Try
            ' Set state global untuk brand ini
            sGlobalActiveBrandId = brandId
            sGlobalActiveBrandIsMultiBranch = isMultiBranch
            LoadOAuthCredentialFromConfig(brandId)

            ' Coba set solid dari outlet pertama brand jika tabel cld_outlet masih memiliki data cabang
            Try
                Dim cCMD As New clsCMD(sConnection)
                Try
                    Dim outletQuery As String
                    If isMultiBranch Then
                        outletQuery = "SELECT ID FROM cld_outlet WHERE brandID=" & brandId & " AND custid=" & scustid & " AND active=1 AND (eDate IS NULL OR eDate >= CURDATE()) AND IFNULL(kode2,'') <> '' LIMIT 1"
                    Else
                        outletQuery = "SELECT ID FROM cld_outlet WHERE brandID=" & brandId & " AND custid=" & scustid & " AND active=1 AND (eDate IS NULL OR eDate >= CURDATE()) LIMIT 1"
                    End If
                    cCMD.resetCmd(outletQuery)
                    Dim obj = cCMD.executeScaler(False)
                    If obj IsNot Nothing AndAlso obj.ToString() <> "" Then
                        solid = obj.ToString()
                    End If
                Finally
                    cCMD.dispose()
                End Try
            Catch exOut As Exception
                errLog("Info cek cabang operasional brandID=" & brandId & ": " & exOut.Message, source)
            End Try

            ' Login ke database Accurate brand ini menggunakan credential dari App.config
            If Not GetToken() Then
                errLog("Gagal GetToken untuk brandID=" & brandId & ". Brand di-skip.", source)
                Exit Sub
            End If

            With sessionStore
                ' Sync kategori item (ke semua brand, idempotent)
                errLog("Mulai sync kategori untuk brandID=" & brandId, source)
                itemProcess.SaveItemCat(TextBox1.Text, .session, .host, My.Settings.sKoneksi)

                ' Sync item sesuai brandid di cld_itemmaster
                If brandId = 2 Then
                    ' Otomatisasi: Update item yang dijual di outlet Brand 2 tapi terdaftar under brandid lain -> ubah ke brandid=2 & flag=0
                    Dim cCMDAutoItem As New clsCMD(sConnection)
                    Try
                        cCMDAutoItem.resetCmd("UPDATE cld_itemmaster SET brandid=2, flag=0 WHERE itemno IN (SELECT DISTINCT itemno FROM cld_sumsalesdetaildaily WHERE olid IN (SELECT ID FROM cld_outlet WHERE brandID=2) OR olnumber IN (SELECT Kode FROM cld_outlet WHERE brandID=2)) AND brandid <> 2")
                        cCMDAutoItem.executeNonQuery()
                        errLog("[AUTO-FIX] Memeriksa & mengupdate item sales Brand 2 dari brandid lain menjadi brandid=2 (flag=0)", source)
                    Catch exAuto As Exception
                        errLog("[AUTO-FIX Error] " & exAuto.Message, source)
                    Finally
                        cCMDAutoItem.dispose()
                    End Try
                End If

                errLog("Mulai sync item untuk brandID=" & brandId, source)
                itemProcess.failinsert = 0
                itemProcess.SaveItem(TextBox1.Text, .session, .host)
                If itemProcess.failinsert > 0 Then
                    errLog("Sync sales brandID=" & brandId & " dibatalkan: ada item gagal disimpan.", source)
                    Exit Sub
                End If

                ' Sync akun payment (flag 2-bit per brand)
                errLog("Mulai sync payment/account untuk brandID=" & brandId, source)
                payProcess.SaveAccount(TextBox1.Text, .session, .host)

                ' Sync akun jurnal (idempotent, buat jika belum ada di Accurate)
                errLog("Mulai sync akun jurnal untuk brandID=" & brandId, source)
                payProcess.SaveJournalAccounts(TextBox1.Text, .session, .host)
            End With

            ' Sync sales transaksi per outlet brand ini
            errLog("Mulai sync sales untuk brandID=" & brandId, source)
            SyncAllBranchesDailyRange(dateFrom, source & " [Brand" & brandId & "]")

        Finally
            ' Kembalikan semua state global ke nilai semula
            solid = savedSolid
            sGlobalActiveBrandId = savedBrandId
            sGlobalActiveBrandIsMultiBranch = savedMultiBranch
            LoadOAuthCredentialFromConfig(savedBrandId)
            errLog("[DIAG] State global dikembalikan setelah SyncOneBrand brandID=" & brandId, source)
        End Try
    End Sub

End Class
