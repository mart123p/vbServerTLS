Public Class ClientProtocolReader
    Public Function getInscription(ByVal request As String) As User
        Dim requestArray As String() = request.Split(vbCrLf)
        If requestArray.Length = 2 Then

        Else
            Throw New BadRequestException("POST /user" & vbCrLf & "jsonObject")
        End If

    End Function
End Class
