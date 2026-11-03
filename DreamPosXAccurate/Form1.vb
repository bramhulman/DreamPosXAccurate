Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Security.Policy
Imports System.Text
Imports System.Web
Imports Newtonsoft.Json
Imports OpenQA.Selenium
Imports OpenQA.Selenium.Chrome
Public Class Form1

    Dim Token As New clsToken.ProcessToken
    Dim sessionStore As New clsStore.ResponseOpenDB
    Dim Store As New clsStore
    Dim listStore As New clsStore.Response
    Dim account As New clsPayment.Request
    Dim itemProcess As New clsItem
    Dim transProcess As New clsTransactions

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        TextBox4.Text = "AB000001"
        With sessionStore
            'untuk save item maks hanya 100 record /nembak
            '   itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 0, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 100, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 200, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 300, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 400, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 500, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 600, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 700, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 800, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 900, 100)
            'itemProcess.SaveItem(acctoken, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 1000, 100)

            TextBox1.ReadOnly = True
            TextBox2.ReadOnly = True
            TextBox3.Text = "S0000096"
        End With






        Dim a As String = ""

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim acctoken As String = ""
        LoadOAuthCredentialFromConfig()
        acctoken = Token.GetToken(sClientID, sUser, sPw) 'untuk login | client id,email/nohp, password
        TextBox1.Text = acctoken

        listStore = Store.getStoreList(acctoken) 'untuk nampilin store nya, untuk diambil storeid nya
        If listStore.d Is Nothing OrElse listStore.d.Count = 0 Then
            errLog("Access token valid, tetapi Accurate mengembalikan daftar database kosong. Pastikan akun OAuth memiliki akses ke Data Usaha Accurate dan aplikasi sudah diotorisasi untuk database tersebut.", "Form1.Button1_Click")
            Exit Sub
        End If

        Dim storeID As String = ""
        For Each b In listStore.d
            If b.alias = sStoreName Then
                storeID = b.id
                Exit For
            End If
        Next

        If storeID = "" Then
            errLog("Store " & sStoreName & " tidak ditemukan pada akun Accurate", "Form1.Button1_Click")
            Exit Sub
        End If

        sessionStore = Store.openDBStore(acctoken, storeID) 'setelah dapat store id, untuk dapetin session nya, supaya bisa akses api accurate
        If sAccountingUrl <> "" Then sessionStore.host = sAccountingUrl
        TextBox2.Text = sessionStore.session
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles Button2.Click
        With sessionStore
            'untuk membuat faktur penjualan
            transProcess.SaveTrans(TextBox1.Text, TextBox2.Text, .host, My.Settings.sKoneksi, TextBox3.Text)
        End With
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        With sessionStore
            'untuk membuat faktur penjualan
            transProcess.SaveTransReceipt(TextBox1.Text, TextBox2.Text, .host, My.Settings.sKoneksi, TextBox3.Text)
        End With
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        'untuk save item maks hanya 100 record /nembak
        With sessionStore
            itemProcess.SaveItemCat(TextBox1.Text, .session, .host, My.Settings.sKoneksi)
        End With
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        'untuk save item maks hanya 100 record /nembak
        With sessionStore
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 0, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 100, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 200, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 300, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 400, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 500, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 600, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 700, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 800, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 900, 100)
            'itemProcess.SaveItem(TextBox1.Text, .session, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;", 1000, 100)
        End With

    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        With sessionStore
            'untuk membuat faktur penjualan
            transProcess.SaveTransSumSalesDaily(TextBox1.Text, TextBox2.Text, .host, My.Settings.sKoneksi, DateTimePicker1.Value)
        End With
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        With sessionStore
            'untuk membuat faktur penjualan
            transProcess.SaveTransReceiptSumSalesDaily(TextBox1.Text, TextBox2.Text, .host, My.Settings.sKoneksi, DateTimePicker1.Value)
        End With
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        With sessionStore
            'account.SaveAccount(TextBox1.Text, TextBox2.Text, .host, "Server=localhost;user id=root;password=1234;database=cldmembergtc; Connection Timeout=3000; default command timeout=2400;")
        End With
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click

        Dim Response As New clsItem.ResponseSearchItem
        '    Dim ListResponse As New List(Of clsItem.ResponseSearchItem.Dd)
        With sessionStore
            Response = itemProcess.SearchItemByID(TextBox1.Text, .session, .host, TextBox4.Text)

            For Each a In Response.d
                TextBox5.Text = a.id
            Next

        End With
    End Sub
End Class
