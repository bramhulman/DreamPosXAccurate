Imports System.Data
Imports MySql.Data.MySqlClient

Public Class clsDatabase
    Private _connStr As String

    Public Sub New(connStr As String)
        _connStr = connStr
    End Sub

    Public Function GetSalesData(syncDate As Date, companyCode As String) As clsModels.ImportPOSDataRequest
        Dim request As New clsModels.ImportPOSDataRequest With {
            .code = companyCode,
            .slposa = New List(Of clsModels.SLPOSA)(),
            .slposb = New List(Of clsModels.SLPOSB)()
        }

        Try
            Using conn As New MySqlConnection(_connStr)
                conn.Open()

                ' 1. Fetch SLPOSA (Detail Items)
                Dim sqlA As String = "SELECT m.store_code, m.transdate, d.itemno, d.catname, d.qty, d.price, d.disc_amt, d.fg_sls, m.number as sales_no, m.receipt_no, m.pos_id, m.table_no " &
                                    "FROM cld_salesmaster m JOIN cld_salesdetail d ON m.number = d.smno " &
                                    "WHERE m.transdate >= @dateStart AND m.transdate <= @dateEnd"
                
                Using cmd As New MySqlCommand(sqlA, conn)
                    cmd.Parameters.AddWithValue("@dateStart", syncDate.Date)
                    cmd.Parameters.AddWithValue("@dateEnd", syncDate.Date.AddDays(1).AddSeconds(-1))
                    
                    Using rdr As MySqlDataReader = cmd.ExecuteReader()
                        While rdr.Read()
                            request.slposa.Add(New clsModels.SLPOSA With {
                                .store_code = rdr("store_code").ToString(),
                                .trans_date = CDate(rdr("transdate")).ToString("yyyyMMdd"),
                                .prod_code = rdr("itemno").ToString(),
                                .cat_name = rdr("catname").ToString(),
                                .trans_qty = Convert.ToDecimal(rdr("qty")),
                                .trans_bruto = Convert.ToDecimal(rdr("price")),
                                .disc_price = Convert.ToDecimal(rdr("disc_amt")),
                                .fg_sls = rdr("fg_sls").ToString(),
                                .sales_no = rdr("sales_no").ToString(),
                                .receipt_no = rdr("receipt_no").ToString(),
                                .pos_id = rdr("pos_id").ToString(),
                                .table_no = rdr("table_no").ToString()
                            })
                        End While
                    End Using
                End Using

                ' 2. Fetch SLPOSB (Payment/Summary)
                Dim sqlB As String = "SELECT m.store_code, m.transdate, m.bruto, m.service, m.tax, m.netto, p.paymentname, p.total as pay_total, m.number as sales_no, m.receipt_no " &
                                    "FROM cld_salesmaster m JOIN cld_payment p ON m.number = p.smno " &
                                    "WHERE m.transdate >= @dateStart AND m.transdate <= @dateEnd"

                Using cmd As New MySqlCommand(sqlB, conn)
                    cmd.Parameters.AddWithValue("@dateStart", syncDate.Date)
                    cmd.Parameters.AddWithValue("@dateEnd", syncDate.Date.AddDays(1).AddSeconds(-1))

                    Using rdr As MySqlDataReader = cmd.ExecuteReader()
                        While rdr.Read()
                            request.slposb.Add(New clsModels.SLPOSB With {
                                .store_code = rdr("store_code").ToString(),
                                .trans_date = CDate(rdr("transdate")).ToString("yyyyMMdd"),
                                .trans_bruto = Convert.ToDecimal(rdr("bruto")),
                                .service_charge = Convert.ToDecimal(rdr("service")),
                                .gov_tax = Convert.ToDecimal(rdr("tax")),
                                .trans_netto = Convert.ToDecimal(rdr("netto")),
                                .payment_type = rdr("paymentname").ToString(),
                                .sales_no = rdr("sales_no").ToString(),
                                .receipt_no = rdr("receipt_no").ToString()
                            })
                        End While
                    End Using
                End Using

            End Using
        Catch ex As Exception
            ' Handle/Log Exception
            Throw New Exception("Database Error: " & ex.Message)
        End Try

        Return request
    End Function
End Class
