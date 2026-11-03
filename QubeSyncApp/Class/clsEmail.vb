Imports System.Net
Imports System.Net.Mail

Public Class clsEmail
    Public Shared Sub SendNotification(subject As String, body As String, smtpHost As String, smtpPort As Integer, smtpUser As String, smtpPass As String, fromEmail As String, toEmail As String)
        Try
            If String.IsNullOrEmpty(toEmail) Then Return

            Dim mail As New MailMessage()
            mail.From = New MailAddress(fromEmail)
            mail.To.Add(toEmail)
            mail.Subject = subject
            mail.Body = body
            mail.IsBodyHtml = False

            Dim smtp As New SmtpClient(smtpHost)
            smtp.Port = smtpPort
            smtp.Credentials = New NetworkCredential(smtpUser, smtpPass)
            smtp.EnableSsl = True

            smtp.Send(mail)
        Catch ex As Exception
            ' If email fails, log to file
            Console.WriteLine($"Email failed: {ex.Message}")
        End Try
    End Sub
End Class
