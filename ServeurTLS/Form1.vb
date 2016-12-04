Imports System.Threading
Public Class Form1
    Dim clientReceiver As ClientReceiver
    Dim crypto As CryptoTLS
    Delegate Sub dLogger(ByVal messsage As String, ByVal ip As String, ByVal port As Integer, ByVal user As String)
    Delegate Sub dLogger2(ByVal messsage As String, ByVal endpoint As String, ByVal user As String)

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        clientReceiver = New ClientReceiver(Me, TextBox1)


    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Button1.Enabled = False
        'We start the server
        Dim serverThread As New Thread(AddressOf Server)
        serverThread.IsBackground = True
        serverThread.Start()
    End Sub

    Private Sub Form_Closing(sender As Object, e As EventArgs) Handles Me.Closing
        clientReceiver.dispose()
    End Sub

    Private Sub Server()
        crypto = New CryptoTLS(True)
        Invoke(New dLogger(AddressOf logger), "Server started", "127.0.0.1", 5000, "Me")
        Dim socket As New SocketTLS(SocketTLSType.Server, crypto, "127.0.0.1", 5000, AddressOf receiver, AddressOf onConnect, AddressOf onDisconnect)
    End Sub

    Private Sub onDisconnect(ByVal endPoint As String)
        Invoke(New dLogger2(AddressOf logger), "Client socket disconnection", endPoint, "")
    End Sub

    Private Sub onConnect(ByVal etudiantsRequest As EtudiantsRequest)
        Invoke(New dLogger(AddressOf logger), etudiantsRequest.getRequest, etudiantsRequest.getIp, etudiantsRequest.getPort, "")
    End Sub


    Private Sub logger(ByVal messsage As String, ByVal ip As String, ByVal port As Integer, ByVal user As String)
        If user Is "" Then
            user = "anonymous"
        End If
        TextBox1.AppendText(ip & ":" & port & " - " & user & " [" & DateTime.Now.ToString("dd/MM/yyyy:HH:mm:ss") & "] " & messsage & vbCrLf)
    End Sub

    Private Sub logger(ByVal messsage As String, ByVal endpoint As String, ByVal user As String)
        If user Is "" Then
            user = "anonymous"
        End If
        TextBox1.AppendText(endpoint & " - " & user & " [" & DateTime.Now.ToString("dd/MM/yyyy:HH:mm:ss") & "] " & messsage & vbCrLf)
    End Sub

    Private Sub receiver(ByVal etudiantsRequest As EtudiantsRequest)
        clientReceiver.read(etudiantsRequest)
    End Sub
End Class
