' Support for 2nd amendment criteria during the transitional period
Public Class TransitionalService
    Private Shared _use2ndAmendment As Boolean = False
    Public Shared Property Use2ndAmendment() As Boolean
        Get
            Return _use2ndAmendment
        End Get
        Set(value As Boolean)
            _use2ndAmendment = value
        End Set
    End Property
End Class
