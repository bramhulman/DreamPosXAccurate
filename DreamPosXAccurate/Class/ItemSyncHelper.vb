Imports System
Imports System.Collections.Generic
Imports System.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class ItemSyncHelper
    ''' <summary>
    ''' Checks all items in cld_itemmaster with flag=2 (already marked as synced) for brand 2.
    ''' If an item is not found in Accurate Online, its flag is reset to 0 so that SaveItem will resend it.
    ''' This routine can be called manually (e.g., from a button) before running the regular item sync.
    ''' </summary>
    Public Shared Sub ResetMissingItemsForBrand2()
        Try
            Dim sql As String = "SELECT itemNo FROM cld_itemmaster WHERE brandid = 2 AND flag = 2"
            Dim dt As DataTable = clsCMD.selectDatatable(sql)
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                modul.errLog("No items with flag=2 for brand 2", "ResetMissingItemsForBrand2")
                Return
            End If

            ' Ensure we have a valid token/ session for Accurate API
            If String.IsNullOrEmpty(modul.sUser) OrElse String.IsNullOrEmpty(modul.sPw) Then
                modul.errLog("OAuth credentials not loaded", "ResetMissingItemsForBrand2")
                Return
            End If

            Dim token As String = clsOAuth.GetAccessToken() ' assuming existing helper, otherwise use LoadOAuthCredentialFromOutlet
            Dim sessionID As String = ""
            Dim hostUrl As String = modul.sStoreName

            For Each row As DataRow In dt.Rows
                Dim itemNo As String = row("itemNo").ToString()
                Dim exists As Boolean = False
                Try
                    Dim result As clsItem.ResponseSearchItem = clsItem.SearchItemByID(token, sessionID, hostUrl, itemNo)
                    If result IsNot Nothing AndAlso result.result IsNot Nothing AndAlso result.result.Count > 0 Then
                        exists = True
                    End If
                Catch ex As Exception
                    ' API call failed – treat as not existing
                    exists = False
                End Try

                If Not exists Then
                    ' Reset flag so SaveItem will attempt to sync again
                    Dim updSql As String = $"UPDATE cld_itemmaster SET flag = 0 WHERE itemNo = '{itemNo}' AND brandid = 2"
                    clsCMD.executeNonQuery(updSql)
                    modul.errLog($"Reset flag for missing item {itemNo}", "ResetMissingItemsForBrand2")
                End If
            Next
        Catch ex As Exception
            modul.errLog(ex.Message, "ResetMissingItemsForBrand2")
        End Try
    End Sub
End Class
