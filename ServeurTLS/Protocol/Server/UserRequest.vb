Public Class UserRequest
    Dim auth As String
    Dim dic As Dictionary(Of String, String)
    Sub New(ByVal auth As String, ByVal dic As Dictionary(Of String, String))
        Me.auth = auth
        Me.dic = dic
    End Sub

    Public Function getAuth() As String
        Return auth
    End Function

    Public Function getParameters() As Dictionary(Of String, String)
        Return dic
    End Function

End Class
