Imports System.IO
Imports System.Configuration

Module modul
    Public sConnection As String = My.Settings.sKoneksi
    Public sTimeProduct As String = My.Settings.sTimeSyncProduct
    Public sTimeTransaction As String = My.Settings.sTimeSyncTrans
    Public sClientID As String = My.Settings.sClientID
    Public sClientSecret As String = ""
    Public sCompCode As String = ""
    Public sAccountingApp As String = "https://account.accurate.id"
    Public sAccountingUrl As String = "https://account.accurate.id"
    Public sOAuthRedirectUri As String = "https://oauth.pstmn.io/v1/callback"
    Public sOAuthResponseType As String = "token"
    Public sOAuthScope As String = "company_data branch_view glaccount_view glaccount_save item_view item_save item_category_view item_category_save tax_view tax_save sales_invoice_view sales_invoice_save sales_receipt_view sales_receipt_save journal_voucher_view journal_voucher_save customer_view customer_save"
    Public sUser As String = My.Settings.sUser
    Public sGlobalActiveBranchName As String = ""
    Public sGlobalActiveBrandId As Integer = 0
    Public sGlobalActiveBrandIsMultiBranch As Boolean = True
    Public sPw As String = My.Settings.sPw
    Public sTimer As String = My.Settings.sTime
    Public sStoreName As String = My.Settings.sStoreName
    Public solid As String = My.Settings.solid
    Public scustid As String = My.Settings.scustid
    Public service_coa As String = My.Settings.service_coa
    Public PB1_coa As String = My.Settings.PB1_coa
    Public Discount_coa As String = My.Settings.Discount_coa
    Public rounding_coa As String = My.Settings.rounding_coa
    Public customer_no As String = My.Settings.customer_no
    Public itemtax As String = My.Settings.itemtax
    Public taxperitem As String = My.Settings.taxperitem
    Public sApiKey As String = ""
    Public sToken As String = ""
    Public sDatabaseId As String = ""

    ''' <summary>
    ''' Helper untuk membaca nilai konfigurasi dari App.config menggunakan ConfigurationManager.
    ''' Mendukung pengecekan key spesifik brand (misal: Accurate_Brand1_*) lalu fallback ke default (Accurate_*).
    ''' </summary>
    Public Function GetConfigValue(ByVal brandId As Integer, ByVal keyName As String, Optional ByVal fallbackValue As String = "") As String
        Dim val As String = ""
        Try
            ' 1. Cek konfigurasi spesifik brand jika brandId > 0 (contoh: Accurate_Brand2_ClientID)
            If brandId > 0 Then
                Dim brandKey As String = "Accurate_Brand" & brandId & "_" & keyName
                val = ConfigurationManager.AppSettings(brandKey)
                If Not String.IsNullOrWhiteSpace(val) Then Return val.Trim()
            End If

            ' 2. Cek konfigurasi default Accurate (contoh: Accurate_ClientID)
            Dim defaultKey As String = "Accurate_" & keyName
            val = ConfigurationManager.AppSettings(defaultKey)
            If Not String.IsNullOrWhiteSpace(val) Then Return val.Trim()

            ' 3. Gunakan fallbackValue jika disediakan
            If Not String.IsNullOrWhiteSpace(fallbackValue) Then Return fallbackValue.Trim()
        Catch ex As Exception
            errLog("Gagal membaca App.config key '" & keyName & "': " & ex.Message, "modul.GetConfigValue")
        End Try

        Return ""
    End Function

    ''' <summary>
    ''' Memuat kredensial Accurate Online dari ConfigurationManager (App.config).
    ''' Menggantikan query ke tabel cld_outlet sehingga database cld_outlet tidak lagi dipakai untuk credential.
    ''' </summary>
    Public Sub LoadOAuthCredentialFromConfig(Optional ByVal brandId As Integer = 0)
        Dim targetBrandId As Integer = brandId
        If targetBrandId <= 0 AndAlso sGlobalActiveBrandId > 0 Then
            targetBrandId = sGlobalActiveBrandId
        End If

        Try
            ' 1. Client ID / Client Key
            Dim clientKey As String = GetConfigValue(targetBrandId, "ClientID")
            If String.IsNullOrWhiteSpace(clientKey) Then clientKey = GetConfigValue(targetBrandId, "ClientKey")
            If String.IsNullOrWhiteSpace(clientKey) Then clientKey = My.Settings.sClientID
            If Not String.IsNullOrWhiteSpace(clientKey) Then
                sClientID = clientKey
            Else
                errLog("[PERINGATAN CONFIG] Client ID kosong di App.config (Key: Accurate_ClientID / Accurate_Brand" & targetBrandId & "_ClientID)!", "modul.LoadOAuthCredentialFromConfig")
            End If

            ' 2. Client Secret
            Dim clientSecret As String = GetConfigValue(targetBrandId, "ClientSecret")
            If String.IsNullOrWhiteSpace(clientSecret) Then clientSecret = GetConfigValue(targetBrandId, "Secret")
            sClientSecret = clientSecret

            ' 3. Password / Auth Secret (sPw)
            Dim passwordVal As String = GetConfigValue(targetBrandId, "Password")
            If Not String.IsNullOrWhiteSpace(passwordVal) Then
                sPw = passwordVal
            ElseIf Not String.IsNullOrWhiteSpace(clientSecret) Then
                sPw = clientSecret
            Else
                sPw = My.Settings.sPw
            End If
            If String.IsNullOrWhiteSpace(sPw) AndAlso String.IsNullOrWhiteSpace(sClientSecret) Then
                errLog("[PERINGATAN CONFIG] ClientSecret/Password kosong di App.config untuk BrandID=" & targetBrandId & "!", "modul.LoadOAuthCredentialFromConfig")
            End If

            ' 4. User / AuthUser / Token
            Dim userVal As String = GetConfigValue(targetBrandId, "AuthUser")
            If String.IsNullOrWhiteSpace(userVal) Then userVal = GetConfigValue(targetBrandId, "Token")
            If String.IsNullOrWhiteSpace(userVal) Then userVal = GetConfigValue(targetBrandId, "User")
            If String.IsNullOrWhiteSpace(userVal) Then userVal = My.Settings.sUser
            If Not String.IsNullOrWhiteSpace(userVal) Then
                sUser = userVal
            Else
                errLog("[PERINGATAN CONFIG] AuthUser/Token kosong di App.config untuk BrandID=" & targetBrandId & "!", "modul.LoadOAuthCredentialFromConfig")
            End If

            ' 5. Store Name / CompCode / Database ID
            Dim storeVal As String = GetConfigValue(targetBrandId, "StoreName")
            If String.IsNullOrWhiteSpace(storeVal) Then storeVal = GetConfigValue(targetBrandId, "CompCode")
            If String.IsNullOrWhiteSpace(storeVal) Then storeVal = GetConfigValue(targetBrandId, "DatabaseId")
            If String.IsNullOrWhiteSpace(storeVal) Then storeVal = My.Settings.sStoreName
            If Not String.IsNullOrWhiteSpace(storeVal) Then
                sStoreName = storeVal
                sCompCode = storeVal
            Else
                errLog("[PERINGATAN CONFIG] StoreName/CompCode kosong di App.config untuk BrandID=" & targetBrandId & "!", "modul.LoadOAuthCredentialFromConfig")
            End If

            ' 6. Accounting URL / Base URL
            Dim appUrl As String = GetConfigValue(targetBrandId, "AccountingApp")
            If String.IsNullOrWhiteSpace(appUrl) Then appUrl = GetConfigValue(targetBrandId, "AccountingUrl", "https://odin.accurate.id")
            If Not String.IsNullOrWhiteSpace(appUrl) Then
                sAccountingApp = appUrl
                sAccountingUrl = NormalizeUrl(appUrl)
            End If

            ' 7. Parameter OAuth Tambahan (Redirect URI, Response Type, Scope)
            Dim redirectUri As String = GetConfigValue(targetBrandId, "OAuthRedirectUri")
            If Not String.IsNullOrWhiteSpace(redirectUri) Then sOAuthRedirectUri = redirectUri

            Dim responseType As String = GetConfigValue(targetBrandId, "OAuthResponseType")
            If Not String.IsNullOrWhiteSpace(responseType) Then sOAuthResponseType = responseType

            Dim scopeVal As String = GetConfigValue(targetBrandId, "OAuthScope")
            If Not String.IsNullOrWhiteSpace(scopeVal) Then sOAuthScope = scopeVal

            ' 8. API Key / Token / DatabaseId tambahan jika ada
            sApiKey = GetConfigValue(targetBrandId, "ApiKey")
            sToken = GetConfigValue(targetBrandId, "Token")
            sDatabaseId = GetConfigValue(targetBrandId, "DatabaseId")

            errLog("Kredensial Accurate Online berhasil dimuat dari App.config (ConfigurationManager) untuk BrandID=" & targetBrandId & " | Store: " & sStoreName & " | URL: " & sAccountingUrl, "modul.LoadOAuthCredentialFromConfig")

        Catch ex As Exception
            errLog("Gagal memuat kredensial Accurate dari App.config: " & ex.Message, "modul.LoadOAuthCredentialFromConfig")
            Err.Clear()
        End Try
    End Sub

    ''' <summary>
    ''' Wrapper untuk kompatibilitas kode lama yang sebelumnya memanggil LoadOAuthCredentialFromOutlet.
    ''' Memanggil LoadOAuthCredentialFromConfig tanpa mengakses tabel cld_outlet.
    ''' </summary>
    Public Sub LoadOAuthCredentialFromOutlet(Optional ByVal brandId As Integer = 0)
        LoadOAuthCredentialFromConfig(brandId)
    End Sub

    Private Function GetDbString(ByVal row As DataRow, ByVal columnName As String) As String
        If Not row.Table.Columns.Contains(columnName) Then Return ""
        If row(columnName) Is DBNull.Value Then Return ""
        Return row(columnName).ToString().Trim()
    End Function

    Public Function NormalizeUrl(ByVal url As String) As String
        Dim cleanUrl As String = url.Trim()

        If cleanUrl = "" Then Return ""
        If cleanUrl.EndsWith("/") Then cleanUrl = cleanUrl.TrimEnd("/"c)

        Return cleanUrl
    End Function

    Public Sub errLog(ByVal strErrDesc As String, ByVal strAccurIn As String)
        Dim reader As New System.Configuration.AppSettingsReader
        Dim strErrPath As String


        Try

            strErrPath = My.Settings.ErrLog

            If strErrPath <> "" Then
                Dim strDire As String
                strDire = Path.GetDirectoryName(strErrPath)

                Dim objWriter As New StreamWriter(strErrPath, True)

                strErrDesc &= " (Occurred IN :" & strAccurIn & ")"
                strErrDesc = "ERROR:" & strErrDesc
                objWriter.WriteLine(Format(Now, "dd/MM/yy hh:mm:ss tt") & " ==>" & strErrDesc)
                objWriter.Close()
                objWriter.Dispose()
            End If


        Catch ex As Exception
            Err.Clear()
        End Try
    End Sub
End Module



