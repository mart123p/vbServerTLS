Public Class TLSInitProtocolBuilder
    Public Function setClientPublicKey(ByVal clientPublicKey As String) As String
        Return "INIT" & vbCrLf & "PubKey: " & clientPublicKey
    End Function

    Public Function setServerPublicKey(ByVal serverPublicKey As String) As String
        Return "OK" & vbCrLf & "PubKey: " & serverPublicKey
    End Function

    Public Function setClientAESKey(ByVal key As String) As String
        Return "AES EXCHANGE" & vbCrLf & "AES: " & key
    End Function

    Public Function setServerAESKey(ByVal key As String) As String
        Return "OK" & vbCrLf & "AES: " & key
    End Function
End Class
