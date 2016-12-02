Imports System.Security.Cryptography

Public Class AuthManager
    Dim authTokens As New Dictionary(Of String, String())
    Dim rngCsp As RNGCryptoServiceProvider

    Sub New()
        rngCsp = New RNGCryptoServiceProvider()
    End Sub

    Public Function add(ByVal endpoint As String, ByVal id As String) As String
        If Not authTokens.ContainsKey(endpoint) Then
            Dim token(15) As Byte
            rngCsp.GetBytes(token)
            Dim auth As String
            auth = Convert.ToBase64String(token)
            Dim str(1) As String
            str(0) = auth
            str(1) = id
            authTokens.Add(endpoint, str)
            Return auth
        Else
            Return authTokens(endpoint)(0)
        End If
    End Function

    Public Function validate(ByVal endpoint As String, ByVal token As String)
        Try
            If authTokens(endpoint)(0) = token Then
                Return True
            Else
                Return False
            End If
        Catch ex As KeyNotFoundException
            Return False
        End Try
    End Function

    Public Function getId(ByVal endpoint As String)
        Try
            Return authTokens(endpoint)(1)
        Catch ex As KeyNotFoundException
            Return Nothing
        End Try
    End Function


    Public Sub remove(ByVal endpoint As String)
        authTokens.Remove(endpoint)
    End Sub

End Class
