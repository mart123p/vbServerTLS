Imports System.Threading
Public Class Form1

    Dim crypto As CryptoTLS
    Delegate Sub dLogger(ByVal messsage As String, ByVal ip As String, ByVal port As Integer, ByVal user As String)
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Button1.Enabled = False
        'We start the server
        Dim serverThread As New Thread(AddressOf Server)
        serverThread.IsBackground = True
        serverThread.Start()


    End Sub

    Private Sub Server()
        crypto = New CryptoTLS(True)
        Invoke(New dLogger(AddressOf logger), "Server started", "127.0.0.1", 5000, "Me")
        Dim socket As New SocketTLS(SocketTLSType.Server, crypto, "127.0.0.1", 5000, AddressOf receiver, AddressOf onConnected)
    End Sub


    Private Sub onConnected(ByVal etudiantsRequest As EtudiantsRequest)
        Invoke(New dLogger(AddressOf logger), etudiantsRequest.getRequest, etudiantsRequest.getIp, etudiantsRequest.getPort, "")
    End Sub


    Private Sub logger(ByVal messsage As String, ByVal ip As String, ByVal port As Integer, ByVal user As String)
        If user Is "" Then
            user = "anonymous"
        End If
        TextBox1.AppendText(ip & ":" & port & " - " & user & " [" & DateTime.Now.ToString("dd/MM/yyyy:HH:mm:ss") & "] " & messsage & vbCrLf)
    End Sub

    Private Sub receiver(ByVal etudiantsRequest As EtudiantsRequest)
        'We Will send an ok to say that everything is fine and to for the client to receive some data
        etudiantsRequest.Send("OK")
        Invoke(New dLogger(AddressOf logger), etudiantsRequest.getRequest, etudiantsRequest.getIp, etudiantsRequest.getPort, "")
    End Sub
End Class
