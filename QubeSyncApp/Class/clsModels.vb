Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class clsModels
    
    Public Class TokenRequest
        Public Property client_id As String
        Public Property client_secret As String
        Public Property company_code As String
    End Class

    Public Class TokenResponse
        Public Property status As String
        Public Property result As TokenResult
    End Class

    Public Class TokenResult
        Public Property access_token As String
        Public Property access_token_lifetime As Integer
        Public Property refresh_token As String
        Public Property refresh_token_lifetime As Integer
    End Class

    Public Class ImportPOSDataRequest
        Public Property code As String
        Public Property slposa As List(Of SLPOSA)
        Public Property slposb As List(Of SLPOSB)
    End Class

    Public Class SLPOSA
        Public Property store_code As String
        Public Property trans_date As String ' YYYYMMDD
        Public Property prod_code As String
        Public Property cat_name As String
        Public Property trans_qty As Decimal
        Public Property trans_bruto As Decimal
        Public Property disc_price As Decimal
        Public Property fg_sls As String ' Y or N
        Public Property sales_no As String
        Public Property covers As Integer
        Public Property foc_type As Integer
        Public Property receipt_no As String
        Public Property pos_id As String
        Public Property table_no As String
        Public Property group_code As String
        Public Property departement_code As String
    End Class

    Public Class SLPOSB
        Public Property store_code As String
        Public Property trans_date As String ' YYYYMMDD
        Public Property trans_bruto As Decimal
        Public Property service_charge As Decimal
        Public Property gov_tax As Decimal
        Public Property trans_netto As Decimal
        Public Property payment_type As String
        Public Property disc_price As Decimal
        Public Property fg_sls As String ' Y or N
        Public Property sales_no As String
        Public Property covers As Integer
        Public Property foc_type As String
        Public Property receipt_no As String
        Public Property pos_id As String
        Public Property table_no As String
        Public Property gov_tax2 As Decimal
        Public Property server As String
        Public Property rnd_adjustment As Decimal
    End Class

    Public Class GenericResponse
        Public Property status As String
        Public Property result As JObject
        Public Property message As List(Of ErrorMessage)
    End Class

    Public Class ErrorMessage
        Public Property code As String
        Public Property validations As List(Of String)
    End Class

    Public Class StatusResponse
        Public Property status As String
        Public Property result As StatusResult
    End Class

    Public Class StatusResult
        Public Property process_id As String
        Public Property company_code As String
        Public Property status As String ' I=In Progress, S=Success, E=Error
        Public Property error_messages As JToken
    End Class

End Class
