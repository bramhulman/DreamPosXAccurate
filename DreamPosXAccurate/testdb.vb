Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

Module Module1
    Sub Main()
        Dim connStr As String = "Server=localhost;user id=root;password=t4v14n4ul1;port=3307;database=cldmemberfongsheng;"
        Using conn As New MySqlConnection(connStr)
            conn.Open()
            Dim cmd As New MySqlCommand("SELECT payname, total FROM cld_sumpaymentdaily WHERE busidate >= '2026-07-30 00:00:00' AND busidate <= '2026-07-30 23:59:59' AND olid = 2", conn)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Console.WriteLine(reader("payname").ToString() & " - " & reader("total").ToString())
                End While
            End Using
            Console.WriteLine("---")
            Dim cmd2 As New MySqlCommand("SELECT SUM(total) FROM cld_sumpaymentdaily WHERE busidate >= '2026-07-30 00:00:00' AND busidate <= '2026-07-30 23:59:59' AND olid = 2", conn)
            Console.WriteLine("Sum: " & cmd2.ExecuteScalar().ToString())
        End Using
    End Sub
End Module
