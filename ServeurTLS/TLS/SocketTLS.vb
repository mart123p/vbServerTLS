
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
    Dim skipKeyVerification As Boolean
    Dim receiver As Action(Of EtudiantsRequest)
    Dim onConnect As Action(Of EtudiantsRequest)
    Dim onDisconnect As Action(Of String)

    Sub New(ByVal type As SocketTLSType, ByRef crypto As CryptoTLS, ByVal ip As String, ByVal port As Integer, ByRef receiver As Action(Of EtudiantsRequest), ByRef onConnect As Action(Of EtudiantsRequest), ByRef onDisconnect As Action(Of String), Optional skipKeyVerification As Boolean = False)
        MyBase.New(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Me.type = type
        Me.crypto = crypto
        Me.receiver = receiver
        Me.onConnect = onConnect
        Me.onDisconnect = onDisconnect
        Me.skipKeyVerification = skipKeyVerification
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
                        Try
                            'We probably received a public key from a client we must process his request
                            Dim ClientPubkey(1023) As Byte
                            socketClient.Receive(ClientPubkey)
                            Dim reader As New TLSInitProtocolReader(ClientPubkey)
                            connections.Find(socketClient).setRemotePublicKey(reader.getClientPublicKey())

                            'We must send our very own public key
                            Dim protocolBuilder As New TLSInitProtocolBuilder
                            socketClient.Send(Encoding.ASCII.GetBytes(protocolBuilder.setServerPublicKey(crypto.getCurrentPublicKey)))
                            connections.Find(socketClient).setConnectionState(SocketTLSServerState.ServerPublicKeyExchanged)
                        Catch ex As Exception
                            onDisconnect(socketClient.RemoteEndPoint.ToString)
                            connections.Remove(socketClient)
                            socketClient.Close()
                            Exit Sub
                        End Try


                    Case SocketTLSServerState.ServerPublicKeyExchanged
                        Try
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
                            onConnect(New EtudiantsRequest(socketClient, "Secure handshake completed with new Client!", tmphostAesKey, crypto))
                        Catch ex As Exception
                            onDisconnect(socketClient.RemoteEndPoint.ToString)
                            connections.Remove(socketClient)
                            socketClient.Close()
                            Exit Sub
                        End Try


                    Case SocketTLSServerState.Ready
                        'We must call the receive function of the user with the decrypted request
                        Dim clientResponse(2047) As Byte 'We are using AES since there is no maximum length to the socket.

                        socketClient.Receive(clientResponse)

                        Dim response As String = ""
                        Try
                            response = crypto.AESDecrypt(clientResponse, connections.Find(socketClient).getRemoteAesKey)
                        Catch ex As Exception
                            'There is a problem with the encryption we must close the socket
                            onDisconnect(socketClient.RemoteEndPoint.ToString)
                            connections.Remove(socketClient)
                            socketClient.Close()
                            Exit Sub
                        End Try

                        Dim request As New EtudiantsRequest(socketClient, response, connections.Find(socketClient).getHostAesKey, crypto)
                        receiver(request) 'We callback where the user will be able to create to send on the same socket
                End Select
            End While
        Catch ex As SocketException
            onDisconnect(socketClient.RemoteEndPoint.ToString)
            connections.Remove(socketClient)
            socketClient.Close()
            Exit Sub
            'The client disconnected it self


        End Try



    End Sub

    Private Sub clientReceive()

        Select Case (communicationState)
            Case SocketTLSServerState.ClientPublicKeyExchanged
                Try
                    Dim serverResponse(1023) As Byte
                    Receive(serverResponse)
                    Dim reader As New TLSInitProtocolReader(serverResponse)
                    remotePublicKey = reader.getServerPublicKey
                Catch ex As Exception
                    Close()
                    onDisconnect("")
                    communicationState = SocketTLSServerState.Ready
                    Exit Sub
                End Try
                'TODO check if the key was changed if it already connected to this server

                If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantClient\TLS", "ServerSignature", Nothing) IsNot Nothing Then
                    'We compare the signature of the public key to make sure that the key is not changed
                    Dim signature As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantClient\TLS", "ServerSignature", Nothing)
                    If signature <> Hash.sha256(remotePublicKey) Then
                        If Not skipKeyVerification Then
                            Throw New KeyPairChanged
                        Else
                            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantClient\TLS", "ServerSignature", Hash.sha256(remotePublicKey))
                        End If
                    End If
                Else
                    'We save the public key to the registery
                    My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantClient\TLS", "ServerSignature", Hash.sha256(remotePublicKey))
                End If


                Try
                    'We generate an aes key
                    hostAesKey = AES256.Generate()
                    Dim protocolBuilder As New TLSInitProtocolBuilder
                    MyBase.Send(crypto.RSAEncrypt(protocolBuilder.setClientAESKey(hostAesKey), remotePublicKey))
                    communicationState = SocketTLSServerState.ClientAESKeyExchanged
                    clientReceive()
                Catch ex As Exception
                    Close()
                    onDisconnect("")
                    communicationState = SocketTLSServerState.Ready
                    Exit Sub
                End Try


            Case SocketTLSServerState.ClientAESKeyExchanged
                Try
                    Dim serverResponse(255) As Byte
                    Receive(serverResponse)
                    Dim reader As New TLSInitProtocolReader(crypto.RSADecrypt(serverResponse))
                    remoteAesKey = reader.getServerAESKey
                    communicationState = SocketTLSServerState.Ready
                Catch ex As Exception
                    Close()
                    onDisconnect("")
                    communicationState = SocketTLSServerState.Ready
                    Exit Sub
                End Try


            Case SocketTLSServerState.Ready
                Dim request As EtudiantsRequest
                Try
                    Dim serverResponse(10240) As Byte
                    Receive(serverResponse)
                    request = New EtudiantsRequest(Me, crypto.AESDecrypt(serverResponse, remoteAesKey), hostAesKey, crypto)
                Catch ex As Exception
                    Close()
                    onDisconnect("")
                    communicationState = SocketTLSServerState.Ready
                    Exit Sub
                End Try
                receiver(request) 'We callback where the user will be able to create to send on the same socket

        End Select

    End Sub

End Class
