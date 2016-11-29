Imports System.Text
Public Class TLSInitProtocolReader
    Dim request As String
    Dim example As String
    Sub New(ByVal request As Byte())
        Me.request = Encoding.ASCII.GetString(request)
    End Sub

    Public Function getClientPublicKey() As String
        example = "INIT" & vbCrLf & "PubKey: public key goes here..."
        Dim params As String() = request.Split(vbCrLf)
        If (params.Count = 2) Then
            If params(0) = "INIT" Then
                If params(1).Contains("PubKey: ") Then
                    'We can get the client Public Key without any problem
                    Return params(1).Replace("PubKey: ", "")
                Else
                    errorThrow()
                End If
            Else
                errorThrow()
            End If
        Else
            errorThrow()
        End If
        Return ""

    End Function

    Public Function getServerPublicKey() As String
        example = "OK" & vbCrLf & "PubKey: public key goes here..."
        Dim params As String() = request.Split(vbCrLf)
        If (params.Count = 2) Then
            If params(0) = "OK" Then
                If params(1).Contains("PubKey: ") Then
                    Return params(1).Replace("PubKey: ", "")
                Else
                    errorThrow()
                End If
            Else
                errorThrow()
            End If
        Else
            errorThrow()
        End If
        Return ""
    End Function

    Public Function getClientAESKey() As String
        example = "AES EXCHANGE" & vbCrLf & "AES: aes key goes here..."
        Dim params As String() = request.Split(vbCrLf)
        If (params.Count = 2) Then
            If params(0) = "AES EXCHANGE" Then
                If params(1).Contains("AES: ") Then
                    Return params(1).Replace("AES: ", "")
                Else
                    errorThrow()
                End If
            Else
                errorThrow()
            End If
        Else
            errorThrow()
        End If
        Return Nothing
    End Function

    Public Function getServerAESKey() As String
        example = "OK" & vbCrLf & "AES: aes key goes here..."
        Dim params As String() = request.Split(vbCrLf)
        If (params.Count = 2) Then
            If params(0) = "OK" Then
                If params(1).Contains("AES: ") Then
                    Return params(1).Replace("AES: ", "")
                Else
                    errorThrow()
                End If
            Else
                errorThrow()
            End If
        Else
            errorThrow()
        End If
        Return Nothing
    End Function

    Private Function errorThrow()
        Throw New TLSInitProtocolMalformedRequest(example)
        Return Nothing
    End Function

End Class
