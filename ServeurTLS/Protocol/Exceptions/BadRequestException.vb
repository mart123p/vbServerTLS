Public Class BadRequestException
    Inherits Exception
    Sub New(ByVal example As String)
        MyBase.New("The request is malformed, it should be formed this way" & vbCrLf & example)
    End Sub
End Class
