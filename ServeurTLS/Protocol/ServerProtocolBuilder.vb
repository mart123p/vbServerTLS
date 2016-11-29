Imports Newtonsoft.Json.Linq

Public Class ServerProtocolBuilder

    Public Function setInscription(ByVal id As String, ByVal status As ProtocolStatus) As String
        Dim o As New JObject()
        o.Add("id", id)
        Return status & vbCrLf & o.ToString
    End Function

    Public Function setConnection(ByVal auth As String, ByVal status As ProtocolStatus) As String
        Return status & vbCrLf & "AUTH: " & auth & vbCrLf
    End Function

    Public Function setStudyField(ByVal fields() As String, ByVal status As ProtocolStatus) As String
        Dim a As New JArray()
        For i = 0 To fields.Length - 1
            a.Add(fields(i))
        Next
        Return status & vbCrLf & a.ToString & vbCrLf
    End Function

    Public Function setDisconnect(ByVal status As ProtocolStatus) As String
        Return status & vbCrLf
    End Function

    Public Function setStudentDirectory(ByVal etudiant() As Etudiants, ByVal status As ProtocolStatus) As String
        Dim a As New JArray()
        For i = 0 To etudiant.Length - 1
            Dim o As New JObject()
            o.Add("firstName", etudiant(i).getFirstName)
            o.Add("lastName", etudiant(i).getLastName)
            o.Add("email", etudiant(i).getEmail)
            o.Add("studyField", etudiant(i).getStudyField)
            a.Add(o)
        Next

        Return status & vbCrLf & a.ToString & vbCrLf
    End Function

    Public Function setProfile(ByVal status As ProtocolStatus) As String
        Return status & vbCrLf
    End Function
End Class
