Public Class Connection
    Dim endpoint As String
    Dim remotePublicKey As String
    Dim remoteAesKey As String
    Dim hostAesKey As String
    Dim connectionState As SocketTLSServerState

    Sub New(ByVal endpoint As String)
        Me.endpoint = endpoint
        connectionState = SocketTLSServerState.Initialize
    End Sub

    Public Function getRemotePublicKey() As String
        Return remotePublicKey
    End Function

    Public Function getRemoteAesKey() As String
        Return remoteAesKey
    End Function

    Public Function getHostAesKey() As String
        Return hostAesKey
    End Function

    Public Function getConnexionState() As String
        Return connectionState
    End Function

    Public Sub setRemotePublicKey(ByVal remotePublicKey As String)
        Me.remotePublicKey = remotePublicKey
    End Sub

    Public Sub setRemoteAesKey(ByVal remoteAesKey As String)
        Me.remoteAesKey = remoteAesKey
    End Sub

    Public Sub setHostAesKey(ByVal hostAesKey As String)
        Me.hostAesKey = hostAesKey
    End Sub

    Public Sub setConnectionState(ByVal state As SocketTLSServerState)
        connectionState = state
    End Sub


    Public Overloads Function ToString() As String
        Return endpoint
    End Function
End Class
