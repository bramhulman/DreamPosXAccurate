Imports System.Net
Imports System.Net.Mail

Public Class clsEmailNotification
    ''' <summary>
    ''' Mengirim notifikasi email saat terjadi kegagalan sync atau error kritis
    ''' Membaca konfigurasi langsung dari App.config (My.Settings)
    ''' </summary>
    ''' <param name="subject">Judul Email Notifikasi</param>
    ''' <param name="bodyMessage">Isi Pesan Error</param>
    Public Shared Sub SendErrorEmail(ByVal subject As String, ByVal bodyMessage As String)
        Try
            ' Membaca konfigurasi email dari App.config / My.Settings
            Dim smtpHost As String = My.Settings.emailHost
            Dim smtpPort As Integer = Val(My.Settings.emailPort)
            Dim senderEmail As String = My.Settings.emailName
            Dim senderPassword As String = My.Settings.emailPass
            Dim recipientEmail As String = My.Settings.emailTarget

            If String.IsNullOrEmpty(smtpHost) OrElse String.IsNullOrEmpty(senderEmail) OrElse String.IsNullOrEmpty(recipientEmail) Then
                errLog("[EMAIL NOTIF] Pengiriman email dibatalkan: Konfigurasi email di App.config belum lengkap.", "clsEmailNotification")
                Exit Sub
            End If

            Dim mail As New MailMessage()
            mail.From = New MailAddress(senderEmail, "DreamPOS Accurate Sync Alert")

            ' Mendukung multiple penerima yang dipisahkan koma (,) atau titik koma (;)
            Dim targets As String() = recipientEmail.Split(New Char() {","c, ";"c}, StringSplitOptions.RemoveEmptyEntries)
            For Each target In targets
                mail.To.Add(target.Trim())
            Next

            mail.Subject = subject
            mail.Body = "Terdeteksi Masalah Sinkronisasi Accurate Online:" & vbCrLf & vbCrLf & _
                        "Waktu: " & DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") & vbCrLf & _
                        "Pesan Error:" & vbCrLf & bodyMessage & vbCrLf & vbCrLf & _
                        "Silakan cek log aplikasi untuk detail lebih lanjut."
            mail.IsBodyHtml = False

            Dim smtp As New SmtpClient(smtpHost, If(smtpPort > 0, smtpPort, 587))
            smtp.Credentials = New NetworkCredential(senderEmail, senderPassword)
            smtp.EnableSsl = True
            smtp.Send(mail)

            errLog("[EMAIL NOTIF] Notifikasi email berhasil dikirim ke " & recipientEmail & ": " & subject, "clsEmailNotification")
        Catch ex As Exception
            errLog("[EMAIL NOTIF ERROR] Gagal mengirim email notifikasi: " & ex.Message, "clsEmailNotification")
        End Try
    End Sub
End Class
