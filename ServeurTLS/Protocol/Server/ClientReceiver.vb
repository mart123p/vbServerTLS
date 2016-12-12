Imports System.Security.Cryptography
Imports Newtonsoft.Json.Linq

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
                        request.Send(protocolBuilder.setDisconnect(ProtocolStatus.OK))
                        gui.Invoke(New dLogger(AddressOf logger), "Disconnection made successfully", request.getIp, request.getPort, authManager.getId(request.getEndPoint))
                        authManager.remove(request.getEndPoint)

                    Else
                        request.Send(ProtocolStatus.UNAUTHORIZED & " DISCONNECT /")
                        gui.Invoke(New dLogger(AddressOf logger), "Disconnection bad auth token", request.getIp, request.getPort, "")
                    End If

                Case ClientProtocolRequests.ModifyProfile
                    Dim userRequest As UserRequest = protocolReader.getModifyProfile(request.getRequest)
                    Dim requestIsValid As Boolean = False
                    If authManager.validate(request.getEndPoint, userRequest.getAuth) Then
                        If userRequest.getParameters("email") <> "" Then
                            Try
                                Dim email As New Net.Mail.MailAddress(userRequest.getParameters("email"))
                                If Not db.userExists(userRequest.getParameters("email")) Then
                                    requestIsValid = True
                                Else
                                    requestIsValid = False
                                    gui.Invoke(New dLogger(AddressOf logger), "Cannot change email address, email already in db", request.getIp, request.getPort, authManager.getId(request.getEndPoint))
                                End If
                            Catch ex As Exception
                                Throw New BadRequestException("")
                            End Try

                        End If

                        If userRequest.getParameters("password") <> "" Then
                            requestIsValid = True
                        End If

                        If userRequest.getParameters("studyField") <> "" Then
                            Dim studyFields As String() = db.getStudyFields()
                            If studyFields.Contains(userRequest.getParameters("studyField")) Then
                                requestIsValid = True
                            Else
                                requestIsValid = False
                                Throw New BadRequestException("")
                            End If
                        End If
                    Else
                        requestIsValid = False
                    End If

                    If requestIsValid Then
                        If userRequest.getParameters("studyField") <> "" Then
                            db.setProfileStudyField(userRequest.getParameters("studyField"), authManager.getId(request.getEndPoint))
                            gui.Invoke(New dLogger(AddressOf logger), "StudyField changed successfully", request.getIp, request.getPort, authManager.getId(request.getEndPoint))

                        End If

                        If userRequest.getParameters("password") <> "" Then
                            Dim rngCsp As New RNGCryptoServiceProvider()
                            Dim iv(15) As Byte
                            rngCsp.GetBytes(iv)
                            db.setProfilePassword(Hash.sha256(Config.hashSalt & Convert.ToBase64String(iv) & userRequest.getParameters("password")), Convert.ToBase64String(iv), authManager.getId(request.getEndPoint))
                            gui.Invoke(New dLogger(AddressOf logger), "Password changed successfully", request.getIp, request.getPort, authManager.getId(request.getEndPoint))

                        End If

                        If userRequest.getParameters("email") <> "" Then
                            db.setProfileEmail(userRequest.getParameters("email"), authManager.getId(request.getEndPoint))
                            gui.Invoke(New dLogger(AddressOf logger), "Email changed successfully", request.getIp, request.getPort, authManager.getId(request.getEndPoint))
                        End If

                        request.Send(ProtocolStatus.OK & " PUT /user")
                    End If


                Case ClientProtocolRequests.SignUp

                    Dim newUser As User = protocolReader.getInscription(request.getRequest)

                    If Not db.userExists(newUser.getEmail) Then
                        If Not newUser.getFirstName.Trim = "" And Not newUser.getLastName.Trim = "" And Not newUser.getEmail.Trim = "" And Not newUser.getStudyField.Trim = "" And Not newUser.getPassword = "" And Not newUser.getBirthday = "" Then
                            Dim studyFields As String() = db.getStudyFields
                            If studyFields.Contains(newUser.getStudyField) Then
                                Try
                                    Dim email As New System.Net.Mail.MailAddress(newUser.getEmail)
                                Catch ex As Exception
                                    Throw New BadRequestException("")
                                End Try

                                Try
                                    Dim birthday As Date = Date.Parse(newUser.getBirthday)
                                    Dim id As String = newUser.getLastName.Substring(0, 3) & newUser.getFirstName.Substring(0, 3) & birthday.Year
                                    id = id.ToLower
                                    If Not db.userExistsMat(id) Then
                                        Dim rngCsp As New RNGCryptoServiceProvider()
                                        Dim iv(15) As Byte
                                        rngCsp.GetBytes(iv)

                                        If db.setUser(newUser, id, Convert.ToBase64String(iv)) Then
                                            Dim o As New JObject()
                                            o.Add("id", id)
                                            request.Send(ProtocolStatus.OK & " POST /user" & vbCrLf & o.ToString.Replace(vbCrLf, "").Replace(vbTab, ""))
                                            gui.Invoke(New dLogger(AddressOf logger), "User creation successfully completed", request.getIp, request.getPort, "")

                                        Else
                                            request.Send(ProtocolStatus.CONFLICT & " POST /user")
                                            gui.Invoke(New dLogger(AddressOf logger), "User creation a user already exists with the same id", request.getIp, request.getPort, "")
                                        End If
                                    Else
                                        request.Send(ProtocolStatus.CONFLICT & " POST /user")
                                        gui.Invoke(New dLogger(AddressOf logger), "User creation a user already exists with the same id", request.getIp, request.getPort, "")
                                    End If
                                Catch ex As FormatException
                                    Throw New BadRequestException("")
                                End Try
                            Else
                                Throw New BadRequestException("")
                            End If
                        Else
                            Throw New BadRequestException("")
                        End If
                    Else
                        request.Send(ProtocolStatus.CONFLICT & " POST /user")
                        gui.Invoke(New dLogger(AddressOf logger), "User creation a user already exists with the same email", request.getIp, request.getPort, "")
                    End If


                Case ClientProtocolRequests.StudentDirectory

                    Dim userRequest As UserRequest = protocolReader.getStudentDirectory(request.getRequest)

                    If authManager.validate(request.getEndPoint, userRequest.getAuth) Then
                        Dim logParameter As String = userRequest.getParameters("studyField")

                        request.Send(protocolBuilder.setStudentDirectory(db.getEtudiants(logParameter), ProtocolStatus.OK))
                        If userRequest.getParameters("studyField") = "" Then
                            logParameter = "all"
                        End If

                        gui.Invoke(New dLogger(AddressOf logger), "Students Directory show " & logParameter, request.getIp, request.getPort, authManager.getId(request.getEndPoint))
                    Else
                        request.Send(ProtocolStatus.UNAUTHORIZED & " GET /students")
                        gui.Invoke(New dLogger(AddressOf logger), "Students Directory bad auth token", request.getIp, request.getPort, "")
                    End If

                Case ClientProtocolRequests.StudyField
                    request.Send(protocolBuilder.setStudyField(db.getStudyFields, ProtocolStatus.OK))
                    gui.Invoke(New dLogger(AddressOf logger), "Studyfield show all", request.getIp, request.getPort, "")

                Case ClientProtocolRequests.GetUserDetails
                    If authManager.validate(request.getEndPoint, protocolReader.getUserDetails(request.getRequest)) Then
                        request.Send(protocolBuilder.setUserDetails(db.getUserDetail(authManager.getId(request.getEndPoint)), ProtocolStatus.OK))
                        gui.Invoke(New dLogger(AddressOf logger), "Get user details", request.getIp, request.getPort, authManager.getId(request.getEndPoint))
                    Else
                        request.Send(ProtocolStatus.UNAUTHORIZED & " GET /user")
                        gui.Invoke(New dLogger(AddressOf logger), "Get user details bad auth token", request.getIp, request.getPort, "")
                    End If


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
