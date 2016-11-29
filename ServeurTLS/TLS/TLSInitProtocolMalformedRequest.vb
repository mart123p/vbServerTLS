Public Class TLSInitProtocolMalformedRequest
    Inherits Exception
    Sub New(ByVal example As String)
        MyBase.New("The request is malformed it should be formed this way: " + example)
    End Sub
End Class
