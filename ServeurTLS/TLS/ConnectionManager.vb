Imports System.Net.Sockets
Public Class ConnectionManager
    Dim connections As New List(Of Connection)

    Public Sub Add(ByRef socket As Socket)
        connections.Add(New Connection(socket.RemoteEndPoint.ToString))
    End Sub

    Public Sub Remove(ByRef socket As Socket)
        connections.Remove(Find(socket))
    End Sub

    Public Function Find(ByRef socket As Socket) As Connection
        Dim socketip As String = socket.RemoteEndPoint.ToString
        For i = 0 To connections.Count - 1
            If connections(i).ToString = socketip Then
                Return connections(i)
            End If
        Next
        Return Nothing
    End Function

End Class
