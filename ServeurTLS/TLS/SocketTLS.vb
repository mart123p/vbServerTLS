
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Public Enum SocketTLSType
    Client
    Server
End Enum

Public Enum SocketTLSServerState
    Initialize
    ClientPublicKeyExchanged
    ServerPublicKeyExchanged
    ServerAESKeyExchanged
    ClientAESKeyExchanged
    Ready
End Enum

Public Class SocketTLS
    Inherits Socket
    Dim connections As New ConnectionManager
    Dim communicationState As SocketTLSServerState = SocketTLSServerState.Initialize
    Dim type As SocketTLSType
    Dim remotePublicKey As String
    Dim remoteAesKey As String
    Dim hostAesKey As String
    Dim crypto As CryptoTLS
    Dim receiver As Action(Of EtudiantsRequest)
    Dim onConnected As Action(Of EtudiantsRequest)

    Sub New(ByVal type As SocketTLSType, ByRef crypto As CryptoTLS, ByVal ip As String, ByVal port As Integer, ByRef receiver As Action(Of EtudiantsRequest), ByRef onConnected As Action(Of EtudiantsRequest))
        MyBase.New(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Me.type = type
        Me.crypto = crypto
        Me.receiver = receiver
        Me.onConnected = onConnected
        Dim ep As IPEndPoint = New IPEndPoint(IPAddress.Parse(ip), port)

        If (type = SocketTLSType.Server) Then
            Bind(ep)
            While True
                Listen(1)
                'We should start a new thread here!
                Dim socketClient As Socket = Accept()
                Dim clientThread As New Thread(AddressOf serverReceive)
                clientThread.IsBackground = True
                clientThread.Start(socketClient)
                'serverReceive(socketClient)

            End While
        Else
            Connect(ep)
            'We must send the first request
            Dim protocolBuilder As New TLSInitProtocolBuilder
            MyBase.Send(Encoding.ASCII.GetBytes(protocolBuilder.setClientPublicKey(crypto.getCurrentPublicKey)))
            communicationState = SocketTLSServerState.ClientPublicKeyExchanged
            While communicationState <> SocketTLSServerState.Ready
                clientReceive()
            End While
            'We have done the socket tls protocol, we will wait for the client to call the send command to encrypt data and send it to the server
        End If

    End Sub

    Public Overloads Sub Send(ByVal str As String)
        If type = SocketTLSType.Client Then
            MyBase.Send(crypto.AESEncrypt(str, hostAesKey))
            'We place the client in waiting mode for the server to respond
            clientReceive()
        End If
    End Sub


    Private Sub serverReceive(ByVal socketClient As Socket)
        connections.Add(socketClient)
        Try
            While True
                Select Case (connections.Find(socketClient).getConnexionState)
                    Case SocketTLSServerState.Initialize
                        'We probably received a public key from a client we must process his request
                        Dim ClientPubkey(1023) As Byte
                        socketClient.Receive(ClientPubkey)
                        Dim reader As New TLSInitProtocolReader(ClientPubkey)
                        connections.Find(socketClient).setRemotePublicKey(reader.getClientPublicKey())

                        'We must send our very own public key
                        Dim protocolBuilder As New TLSInitProtocolBuilder
                        socketClient.Send(Encoding.ASCII.GetBytes(protocolBuilder.setServerPublicKey(crypto.getCurrentPublicKey)))
                        connections.Find(socketClient).setConnectionState(SocketTLSServerState.ServerPublicKeyExchanged)

                    Case SocketTLSServerState.ServerPublicKeyExchanged
                        'We will receive an encrypted communication with our public key by the client
                        Dim ClientAesKey(255) As Byte 'We are using AES since there is no maximum length to the socket.
                        socketClient.Receive(ClientAesKey)
                        'It is encrypted we must decrypt it
                        Dim reader As New TLSInitProtocolReader(crypto.RSADecrypt(ClientAesKey))
                        connections.Find(socketClient).setRemoteAesKey(reader.getClientAESKey())

                        'We must send our aes key which is different for every socket
                        'We generate it
                        Dim tmphostAesKey = AES256.Generate()
                        connections.Find(socketClient).setHostAesKey(tmphostAesKey)

                        Dim protocolBuilder As New TLSInitProtocolBuilder
                        socketClient.Send(crypto.RSAEncrypt(protocolBuilder.setServerAESKey(tmphostAesKey), connections.Find(socketClient).getRemotePublicKey))
                        connections.Find(socketClient).setConnectionState(SocketTLSServerState.Ready)
                        onConnected(New EtudiantsRequest(socketClient, "Secure handshake completed with new Client!", tmphostAesKey, crypto))

                    Case SocketTLSServerState.Ready
                        'We must call the receive function of the user with the decrypted request
                        Dim clientResponse(2047) As Byte 'We are using AES since there is no maximum length to the socket.

                        socketClient.Receive(clientResponse)
                        Dim request As New EtudiantsRequest(socketClient, crypto.AESDecrypt(clientResponse, connections.Find(socketClient).getRemoteAesKey), connections.Find(socketClient).getHostAesKey, crypto)
                        receiver(request) 'We callback where the user will be able to create to send on the same socket
                End Select
            End While
        Catch ex As SocketException
            'The client disconnected it self

        End Try



    End Sub

    Private Sub clientReceive()

        Select Case (communicationState)
            Case SocketTLSServerState.ClientPublicKeyExchanged
                Dim serverResponse(1023) As Byte
                Receive(serverResponse)
                Dim reader As New TLSInitProtocolReader(serverResponse)
                remotePublicKey = reader.getServerPublicKey
                'TODO check if the key was changed if it already connected to this server


                'We generate an aes key
                hostAesKey = AES256.Generate()
                Dim protocolBuilder As New TLSInitProtocolBuilder
                MyBase.Send(crypto.RSAEncrypt(protocolBuilder.setClientAESKey(hostAesKey), remotePublicKey))
                communicationState = SocketTLSServerState.ClientAESKeyExchanged
                clientReceive()

            Case SocketTLSServerState.ClientAESKeyExchanged
                Dim serverResponse(255) As Byte
                Receive(serverResponse)
                Dim reader As New TLSInitProtocolReader(crypto.RSADecrypt(serverResponse))
                remoteAesKey = reader.getServerAESKey
                communicationState = SocketTLSServerState.Ready

            Case SocketTLSServerState.Ready
                Dim serverResponse(2048) As Byte
                Receive(serverResponse)
                Dim request As New EtudiantsRequest(Me, crypto.AESDecrypt(serverResponse, remoteAesKey), hostAesKey, crypto)
                receiver(request) 'We callback where the user will be able to create to send on the same socket

        End Select

    End Sub

End Class
