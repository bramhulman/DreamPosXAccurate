
Imports System.Data.SqlClient
Imports MySql.Data.MySqlClient
Public Class clsCMD
    Private myCmd As New MySqlCommand

    Private myConn As MySqlConnection
    Public myRdr As MySqlDataReader

    Private myisConnect As Boolean = False
    Private myConnStr As String


    'dari ClsDbConn
    Public Sub New(ByVal connStr As String)
        Try
            myConnStr = connStr

            myConn = New MySqlConnection(connStr)

            myCmd.Connection = myConn

            resetCmd("Select 1 as cekConn")
            myisConnect = CBool(Val(executeScaler))

        Catch ex As Exception
            'errLog(Err.Description, "clsCMD:New: " & connStr, "")
            Err.Clear()
        End Try
    End Sub

    Public Function executeReader(ByVal StrQuery As String) As MySqlDataReader

        resetCmd(StrQuery)
        Try


            'open koneksi
            If myCmd.Connection.State = ConnectionState.Closed Then
                myCmd.Connection.Open()
            End If

            myRdr = myCmd.ExecuteReader
            Return myRdr
        Catch ex As Exception
            Err.Clear()
        End Try

        Return Nothing
    End Function

    Public Sub closeReader()
        If Not myRdr.IsClosed Then
            myRdr.Close()
        End If
    End Sub

    Public ReadOnly Property connStr() As String
        Get
            Return myConnStr
        End Get
    End Property


    Public Sub resetCmd(ByVal strQuery As String)
        Try
            myCmd.Parameters.Clear()
            myCmd.CommandText = strQuery
            myCmd.CommandTimeout = 120

        Catch ex As Exception
            errLog(Err.Description, "clsCMD.resetCmd " & myCmd.CommandText.ToString)
            Err.Clear()
        End Try
    End Sub

    Public Sub addParam(ByVal paramValue As Object, ByVal paramName As String, ByVal paramType As MySqlDbType, Optional ByVal bOutput As Boolean = False)
        Try


            If paramValue Is Nothing Then
                Select Case paramType
                    Case MySqlDbType.Decimal, MySqlDbType.Int32, MySqlDbType.Int16, MySqlDbType.Int64, MySqlDbType.Decimal, MySqlDbType.Float, MySqlDbType.Bit
                        myCmd.Parameters.AddWithValue(paramName, 0)
                    Case MySqlDbType.VarChar
                        myCmd.Parameters.AddWithValue(paramName, "")
                    Case Else
                        myCmd.Parameters.AddWithValue(paramName, DBNull.Value)
                End Select

            Else
                myCmd.Parameters.AddWithValue(paramName, paramValue)

                If bOutput Then
                    myCmd.Parameters(paramName).Direction = ParameterDirection.Output
                End If

            End If


        Catch ex As Exception
            errLog(Err.Description, "clsCMD.addParam " & myCmd.CommandText.ToString)
            Err.Clear()
        End Try
    End Sub

    Public Sub executeNonQuery(Optional ByVal cmdType As CommandType = CommandType.Text)
        Try
            myCmd.CommandType = cmdType

            'open koneksi
            If myCmd.Connection.State = ConnectionState.Closed Then
                myCmd.Connection.Open()
            End If

            myCmd.ExecuteNonQuery()
        Catch ex As Exception
            errLog(Err.Description, "clsCMD.executeNonQuery " & myCmd.CommandText.ToString)
            Err.Clear()
        End Try
    End Sub

    Public Function executeProcedure(ByVal strQuery As String, Optional bReturn As Boolean = False) As String
        Dim sHasil As String = 0

        Try

            myCmd.CommandText = strQuery
            myCmd.CommandType = CommandType.StoredProcedure
            myCmd.CommandTimeout = 120

            'open koneksi
            If myCmd.Connection.State = ConnectionState.Closed Then
                myCmd.Connection.Open()
            End If

            If bReturn Then
                sHasil = myCmd.ExecuteScalar()
            Else
                myCmd.ExecuteNonQuery()
            End If

        Catch ex As Exception
            'errLog(Err.Description, "clsDbo:executeNonQuery: " & myCmd.CommandText, "")
            Err.Clear()
        End Try

        Return sHasil
    End Function

    Public Function OutputReturn(ByVal sName As String) As String
        Dim sHasil As String = ""

        Try
            sHasil = myCmd.Parameters(sName).Value

        Catch ex As Exception
            'errLog(Err.Description, "OutputReturn: " & myCmd.CommandText, "")
            Err.Clear()
        End Try

        Return sHasil
    End Function

    Public Function executeScaler(Optional ByVal bstr As Boolean = True) As String
        Dim strRslt As String = ""

        'Debug.Print(myCmd.CommandText)
        Try
            'open koneksi
            If myCmd.Connection.State = ConnectionState.Closed Then
                myCmd.Connection.Open()
            End If
            Dim objRslt As Object

            objRslt = myCmd.ExecuteScalar


            If bstr Then

                If Not objRslt Is Nothing Then
                    strRslt = objRslt.ToString
                End If
            Else

                If Not objRslt Is Nothing Then
                    strRslt = objRslt.ToString
                Else
                    strRslt = 0
                End If

            End If



        Catch ex As Exception
            errLog(Err.Description, "clsCMD.executeScaler " & myCmd.CommandText.ToString)
            Err.Clear()
        End Try

        Return strRslt

    End Function

    Public Function executeScalerDec(Optional ByVal bZero As Boolean = True) As Integer
        Dim iRslt As Integer = 0

        Try

            If myCmd.Connection.State = ConnectionState.Closed Then
                myCmd.Connection.Open()
            End If
            Dim objRslt As Object

            objRslt = myCmd.ExecuteScalar

            If Not objRslt Is Nothing Then
                iRslt = objRslt
            Else

                If bZero Then
                    iRslt = 0
                Else
                    iRslt = -1
                End If

            End If

        Catch ex As Exception
            errLog(Err.Description, "clsCMD.executeScalerDec " & myCmd.CommandText.ToString)
            Err.Clear()
        End Try

        Return iRslt

    End Function

    Public Function selectDataset(Optional ByVal strQuery As String = "", Optional ByVal bStoreProcedure As Boolean = False) As DataSet
        Dim ds As New DataSet

        Try
            If strQuery <> "" Then
                resetCmd(strQuery)
            End If



            Using da As New MySqlDataAdapter(myCmd.CommandText, myCmd.Connection)

                If bStoreProcedure Then
                    da.SelectCommand.CommandType = CommandType.StoredProcedure
                Else
                    da.SelectCommand.CommandType = CommandType.Text
                End If


                For Each param As MySqlParameter In myCmd.Parameters
                    da.SelectCommand.Parameters.AddWithValue(param.ParameterName, param.Value)
                Next

                da.Fill(ds)
                da.Dispose()
            End Using
        Catch ex As Exception
            errLog(Err.Description, "clsCMD.selectDataset " & myCmd.CommandText.ToString)
            Err.Clear()
        End Try

        Return ds

    End Function


    Public Function selectDatatable(Optional ByVal strQuery As String = "", Optional ByVal bStoreProcedure As Boolean = False) As DataTable
        Dim dt As New DataTable

        Try
            If strQuery <> "" Then
                resetCmd(strQuery)
            End If

            Using da As New MySqlDataAdapter(myCmd.CommandText, myCmd.Connection)

                If bStoreProcedure Then
                    da.SelectCommand.CommandType = CommandType.StoredProcedure
                Else
                    da.SelectCommand.CommandType = CommandType.Text
                End If


                For Each param As MySqlParameter In myCmd.Parameters
                    da.SelectCommand.Parameters.AddWithValue(param.ParameterName, param.Value)
                Next

                da.Fill(dt)
                da.Dispose()
            End Using
        Catch ex As Exception
            errLog(Err.Description, "clsCMD.selectDatatable " & myCmd.CommandText.ToString)
            Err.Clear()
        End Try

        Return dt

    End Function

    Public Sub dispose()
        Try
            myCmd.Connection.Close()
            myCmd.Dispose()

        Catch ex As Exception
            'errLog(Err.Description, "clsCMD:dispose" & myCmd.CommandText, "")
            Err.Clear()
        End Try
    End Sub
End Class
