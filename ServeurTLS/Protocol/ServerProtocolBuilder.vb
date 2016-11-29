Public Class ServerProtocolBuilder

    Public Function setInscription(ByVal id As String, ByVal status As ProtocolStatus) As String
    End Function

    Public Function setConnection(ByVal auth As String, ByVal status As ProtocolStatus) As String

    End Function

    Public Function setStudyField(ByVal fields() As String, ByVal status As ProtocolStatus) As String

    End Function

    Public Function setDisconnect(ByVal status As ProtocolStatus) As String

    End Function

    Public Function setStudentDirectory(ByVal etudiant() As Etudiants, ByVal status As ProtocolStatus) As String

    End Function

    Public Function setProfile(ByVal status As ProtocolStatus) As String

    End Function
End Class
