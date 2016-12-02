Public Class ClientReceiver
    Dim protocolReader As ClientProtocolReader
    Dim protocolBuilder As ServerProtocolBuilder
    Dim authManager As AuthManager
    Dim db As EtudiantsDB
    Delegate Sub dLogger(ByVal messsage As String, ByVal ip As String, ByVal port As Integer, ByVal user As String)
    Dim gui As Form
    Dim logBox As TextBox


    Sub New(ByRef gui As Form, ByRef logBox As TextBox)
        protocolReader = New ClientProtocolReader()
        protocolBuilder = New ServerProtocolBuilder()
        authManager = New AuthManager
        db = New EtudiantsDB
        Me.gui = gui
        Me.logBox = logBox
    End Sub

    Public Sub dispose()
        db.close()
    End Sub

    Public Sub read(ByRef request As EtudiantsRequest)
        Try
            Select Case (protocolReader.getCall(request.getRequest))
                Case ClientProtocolRequests.Connection
                    Dim authInfos() As String = protocolReader.getConnection(request.getRequest)

                    If db.userExistsMat(authInfos(0)) Then

                        Dim hashedPassword As String = Hash.sha256(Config.hashSalt & db.getIV(authInfos(0)) & authInfos(1))
                        Console.WriteLine(hashedPassword)

                        If db.getConnect(authInfos(0), hashedPassword) Then
                            'We generate an auth token and we store it into the authManager
                            request.Send(protocolBuilder.setConnection(authManager.add(request.getEndPoint, authInfos(0)), ProtocolStatus.OK))
                            gui.Invoke(New dLogger(AddressOf logger), "Connection made successfully", request.getIp, request.getPort, authInfos(0))
                        Else
                            request.Send(ProtocolStatus.UNAUTHORIZED & " CONNECT /")
                            gui.Invoke(New dLogger(AddressOf logger), "Connection failed wrong password", request.getIp, request.getPort, authInfos(0))
                        End If

                    Else
                        request.Send(ProtocolStatus.UNAUTHORIZED & " CONNECT /")
                        gui.Invoke(New dLogger(AddressOf logger), "Connection user not found", request.getIp, request.getPort, "")
                    End If

                Case ClientProtocolRequests.Disconnect

                    Dim userRequest As UserRequest = protocolReader.getDisconnect(request.getRequest)

                    If authManager.validate(request.getEndPoint, userRequest.getAuth) Then
                        authManager.remove(request.getEndPoint)
                        request.Send(protocolBuilder.setDisconnect(ProtocolStatus.OK))
                        gui.Invoke(New dLogger(AddressOf logger), "Disconnection made successfully", request.getIp, request.getPort, authManager.getId(request.getEndPoint))

                    Else
                        request.Send(ProtocolStatus.UNAUTHORIZED & " DISCONNECT /")
                        gui.Invoke(New dLogger(AddressOf logger), "Disconnection bad auth token", request.getIp, request.getPort, "")
                    End If

                Case ClientProtocolRequests.ModifyProfile

                Case ClientProtocolRequests.SignUp

                Case ClientProtocolRequests.StudentDirectory

                Case ClientProtocolRequests.StudyField

            End Select
        Catch ex As BadRequestException
            request.Send(ProtocolStatus.BAD_REQUEST & " " & request.getRequest().Split(vbCrLf)(0))
            gui.Invoke(New dLogger(AddressOf logger), "Malformed request", request.getIp, request.getPort, "")

        Catch ex As NotFoundException
            request.Send(ProtocolStatus.NOT_FOUND & " " & request.getRequest().Split(vbCrLf)(0))
            gui.Invoke(New dLogger(AddressOf logger), "Invalid request", request.getIp, request.getPort, "")
        End Try

    End Sub

    Private Sub logger(ByVal messsage As String, ByVal ip As String, ByVal port As Integer, ByVal user As String)
        If user Is "" Then
            user = "anonymous"
        End If
        logBox.AppendText(ip & ":" & port & " - " & user & " [" & DateTime.Now.ToString("dd/MM/yyyy:HH:mm:ss") & "] " & messsage & vbCrLf)
    End Sub
End Class
