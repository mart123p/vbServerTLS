Imports System.Net.Sockets

Public Class EtudiantsRequest
    Dim ip As String
    Dim port As String
    Dim request As String
    Dim socket As Socket
    Dim hostAesKey As String
    Dim crypto As CryptoTLS
    Sub New(ByRef socket As Socket, ByVal request As String, ByVal hostAesKey As String, ByRef crypto As CryptoTLS)
        Me.socket = socket

        Dim splited As String()
        splited = socket.RemoteEndPoint.ToString.Split(":")

        ip = splited(0)
        port = splited(1)
        Me.request = request
        Me.hostAesKey = hostAesKey
        Me.crypto = crypto
    End Sub

    Public Function getIp() As String
        Return ip
    End Function

    Public Function getPort() As Integer
        Return port
    End Function

    Public Sub Send(ByVal str As String)
        Dim encrypted As Byte() = crypto.AESEncrypt(str, hostAesKey)

        socket.Send(encrypted)
    End Sub

    Public Function getRequest() As String
        Return request
    End Function
End Class
