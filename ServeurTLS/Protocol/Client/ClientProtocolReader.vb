Imports Newtonsoft.Json.Linq

Public Class ClientProtocolReader
    Public Function getInscription(ByVal request As String) As User
        Dim requestArray As String() = request.Split(vbCrLf)
        If requestArray.Length = 2 Then
            If requestArray(0) = "POST /user" Then
                Try
                    Dim o As JObject = JObject.Parse(requestArray(1))
                    If Not o.GetValue("firstName").ToString = "" And Not o.GetValue("lastName").ToString = "" And Not o.GetValue("email").ToString = "" And Not o.GetValue("studyField").ToString = "" And Not o.GetValue("birthday").ToString = "" And Not o.GetValue("password").ToString = "" Then
                        Dim user As New User(o.GetValue("firstName"), o.GetValue("lastName"), o.GetValue("email"), o.GetValue("studyField"), o.GetValue("birthday"), o.GetValue("password"))
                        Return user
                    Else
                        Throw New BadRequestException("POST /user" & vbCrLf & "jsonObject")
                        Return Nothing
                    End If
                Catch ex As Exception
                    Throw New BadRequestException("POST /user" & vbCrLf & "jsonObject")
                    Return Nothing
                End Try
            Else
                Throw New BadRequestException("POST /user" & vbCrLf & "jsonObject")
                Return Nothing
            End If
        Else
            Throw New BadRequestException("POST /user" & vbCrLf & "jsonObject")
            Return Nothing
        End If
        Return Nothing
    End Function

    Public Function getStudyField(ByVal request As String) As Boolean
        Dim requestArray As String() = request.Split(vbCrLf)
        If requestArray.Length = 1 Then
            If requestArray(0) = "GET /studyField" Then
                Return True
            Else
                Throw New BadRequestException("GET /studyField")
                Return False
            End If
        Else
            Throw New BadRequestException("GET /studyField")
            Return False
        End If
    End Function

    Public Function getConnection(ByVal request As String) As String()
        Dim requestArray As String() = request.Split(vbCrLf)
        If requestArray.Length = 2 Then
            If requestArray(0) = "CONNECT /" Then
                Try
                    Dim o As JObject = JObject.Parse(requestArray(1))
                    Dim output(1) As String
                    output(0) = o.GetValue("id")
                    output(1) = o.GetValue("password")
                    Return output
                Catch ex As Exception
                    Throw New BadRequestException("CONNECT /" & vbCrLf & "jsonObject")
                    Return Nothing
                End Try
            Else
                Throw New BadRequestException("CONNECT /" & vbCrLf & "jsonobject")
                Return Nothing
            End If
        Else
            Throw New BadRequestException("CONNECT /" & vbCrLf & "jsonobject")
            Return Nothing
        End If
    End Function

    Public Function getDisconnect(ByVal request As String) As UserRequest
        Dim requestArray As String() = request.Split(vbCrLf)
        If requestArray.Length = 2 Then
            If requestArray(0) = "DISCONNECT /" Then
                If requestArray(1).Length > 7 Then
                    If requestArray(1).Substring(1, 6) = "AUTH: " Then
                        Dim auth As String = requestArray(1).Substring(7)
                        Return New UserRequest(auth, Nothing)
                    Else
                        Throw New BadRequestException("DISCONNECT /" & vbCrLf & "AUTH: token")
                        Return Nothing
                    End If
                Else
                    Throw New BadRequestException("DISCONNECT /" & vbCrLf & "AUTH: token")
                    Return Nothing
                End If
            Else
                Throw New BadRequestException("DISCONNECT /" & vbCrLf & "AUTH: token")
                Return Nothing
            End If
        Else
            Throw New BadRequestException("DISCONNECT /" & vbCrLf & "AUTH: token")
            Return Nothing
        End If
    End Function

    Public Function getStudentDirectory(ByVal request As String) As UserRequest
        Dim requestArray() As String = request.Split(vbCrLf)
        If requestArray.Length = 3 Then
            If requestArray(0) = "GET /students" Then
                If requestArray(1).Length > 7 Then
                    If requestArray(1).Substring(1, 6) = "AUTH: " Then
                        Dim auth As String = requestArray(1).Substring(7)
                        Try
                            Dim o As JObject = JObject.Parse(requestArray(2))
                            Dim dic As New Dictionary(Of String, String)
                            dic.Add("studyField", o.GetValue("studyField"))

                            Return New UserRequest(auth, dic)

                        Catch ex As Exception
                            Throw New BadRequestException("GET /students" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
                            Return Nothing
                        End Try
                    Else
                        Throw New BadRequestException("GET /students" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
                        Return Nothing
                    End If
                Else
                    Throw New BadRequestException("GET /students" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
                    Return Nothing
                End If
            Else
                Throw New BadRequestException("GET /students" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
                Return Nothing
            End If
        Else
            Throw New BadRequestException("GET /students" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
            Return Nothing
        End If
    End Function

    Public Function getModifyProfile(ByVal request As String) As UserRequest
        Dim requestArray() As String = request.Split(vbCrLf)
        If requestArray.Length = 3 Then
            If requestArray(0) = "PUT /user" Then
                If requestArray(1).Substring(1, 6) = "AUTH: " Then
                    Dim auth As String = requestArray(1).Substring(7)

                    Try
                        'TODO  change here from object to jobject
                        Dim o As JObject = JObject.Parse(requestArray(2))

                        Dim dic As New Dictionary(Of String, String)

                        dic.Add("email", o.GetValue("email"))
                        dic.Add("password", o.GetValue("password"))
                        dic.Add("studyField", o.GetValue("studyField"))

                        Return New UserRequest(auth, dic)

                    Catch ex As Exception
                        Throw New BadRequestException("PUT /user" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
                        Return Nothing
                    End Try


                Else
                    Throw New BadRequestException("PUT /user" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
                    Return Nothing
                End If
            Else
                    Throw New BadRequestException("PUT /user" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
                Return Nothing
            End If
        Else
                Throw New BadRequestException("PUT /user" & vbCrLf & "AUTH: token" & vbCrLf & "jsonobject")
            Return Nothing
        End If
    End Function

    Private Function checkProperty(ByVal o As Object, ByVal propertyToCheck As String) As Boolean
        Dim type As Type = o.GetType
        Return type.GetProperty(propertyToCheck) IsNot Nothing
    End Function

    Public Function getCall(ByVal request As String) As ClientProtocolRequests
        Dim requestArray() As String = request.Split(vbCrLf)
        Try
            Select Case (requestArray(0))
                Case "POST /user"
                    Return ClientProtocolRequests.SignUp
                Case "GET /studyField"
                    Return ClientProtocolRequests.StudyField
                Case "CONNECT /"
                    Return ClientProtocolRequests.Connection
                Case "DISCONNECT /"
                    Return ClientProtocolRequests.Disconnect
                Case "GET /students"
                    Return ClientProtocolRequests.StudentDirectory
                Case "PUT /user"
                    Return ClientProtocolRequests.ModifyProfile
                Case Else
                    Throw New NotFoundException
                    Return Nothing
            End Select
        Catch ex As IndexOutOfRangeException
            Throw New BadRequestException("[POST/GET/CONNECT/DISCONNECT/PUT] /resource")
            Return Nothing
        End Try

    End Function
End Class
