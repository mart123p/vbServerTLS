Imports Newtonsoft.Json.Linq

Public Class ServerProtocolBuilder

    Public Function setInscription(ByVal id As String, ByVal status As ProtocolStatus) As String
        Dim o As New JObject()
        o.Add("id", id)
        Return status & " POST /user" & vbCrLf & o.ToString
    End Function

    Public Function setConnection(ByVal auth As String, ByVal status As ProtocolStatus) As String
        Return status & " CONNECT /" & vbCrLf & "AUTH: " & auth
    End Function

    Public Function setStudyField(ByVal fields() As String, ByVal status As ProtocolStatus) As String
        Dim a As New JArray()
        For i = 0 To fields.Length - 1
            a.Add(fields(i))
        Next
        Return status & " GET /studyField" & vbCrLf & a.ToString.Replace(vbCrLf, "").Replace(vbTab, "")
    End Function

    Public Function setUserDetails(ByVal etudiant As Etudiants, ByVal status As ProtocolStatus) As String
        Dim o As New JObject()
        o.Add("firstName", etudiant.getFirstName)
        o.Add("lastName", etudiant.getLastName)
        o.Add("email", etudiant.getEmail)
        o.Add("studyField", etudiant.getStudyField)
        Return status & " GET /user" & vbCrLf & o.ToString.Replace(vbCrLf, "").Replace(vbTab, "")
    End Function

    Public Function setDisconnect(ByVal status As ProtocolStatus) As String
        Return status & " DISCONNECT /"
    End Function

    Public Function setStudentDirectory(ByVal etudiant As Etudiants(), ByVal status As ProtocolStatus) As String
        Dim a As New JArray()
        For i = 0 To etudiant.Length - 1
            Dim o As New JObject()
            o.Add("firstName", etudiant(i).getFirstName)
            o.Add("lastName", etudiant(i).getLastName)
            o.Add("email", etudiant(i).getEmail)
            o.Add("studyField", etudiant(i).getStudyField)
            a.Add(o)
        Next

        Return status & " GET /students" & vbCrLf & a.ToString.Replace(vbCrLf, "").Replace(vbTab, "")
    End Function

    Public Function setProfile(ByVal status As ProtocolStatus) As String
        Return status & " PUT /user"
    End Function
End Class
