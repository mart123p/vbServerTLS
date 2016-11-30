Imports Newtonsoft.Json.Linq

Public Class ClientProtocolReader
    Public Function getInscription(ByVal request As String) As User
        Dim requestArray As String() = request.Split(vbCrLf)
        If requestArray.Length = 2 Then
            If requestArray(0) = "POST /user" Then
                Try
                    Dim o As Object = JObject.Parse(requestArray(1))
                    Dim user As New User(o.firstName, o.lastName, o.email, o.studyField, o.birthday, o.password)
                    Return user
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
                    Dim o As Object = JObject.Parse(requestArray(1))
                    Dim output(1) As String
                    output(0) = o.id
                    output(1) = o.password
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
                If requestArray(1).Substring(0, 6) = "AUTH: " Then
                    Dim auth As String = requestArray(1).Replace("AUTH: ", "")
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
    End Function

    Public Function getStudentDirectory(ByVal request As String) As UserRequest
        Dim requestArray() As String = request.Split(vbCrLf)
        If requestArray.Length = 3 Then
            If requestArray(0) = "GET /students" Then
                If requestArray(1).Substring(0, 6) = "AUTH: " Then
                    Dim auth As String = requestArray(1).Replace("AUTH: ", "")
                    Try
                        Dim o As Object = JObject.Parse(requestArray(2))
                        Dim dic As New Dictionary(Of String, String)
                        dic.Add("studyField", o.studyField)

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
    End Function

    Public Function getModifyProfile(ByVal request As String) As UserRequest
        Dim requestArray() As String = request.Split(vbCrLf)
        If requestArray.Length = 3 Then
            If requestArray(0) = "PUT /user" Then
                If requestArray(1).Substring(0, 6) = "AUTH: " Then
                    Dim auth As String = requestArray(1).Replace("AUTH: ", "")

                    Try
                        Dim o As Object = JObject.Parse(requestArray(2))
                        Dim dic As New Dictionary(Of String, String)

                        If checkProperty(o, "email") Then
                            dic.Add("email", o.email)
                        End If

                        If checkProperty(o, "password") Then
                            dic.Add("password", o.password)
                        End If

                        If checkProperty(o, "studyField") Then
                            dic.Add("studyField", o.studyField)
                        End If

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
