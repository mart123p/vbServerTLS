Public Class Etudiants
    Dim _firstName As String
    Dim _lastName As String
    Dim _email As String
    Dim _studyField As String

    Public Sub New(ByVal firstName As String, ByVal lastName As String, ByVal email As String, ByVal studyField As String)
        _firstName = firstName
        _lastName = lastName
        _email = email
        _studyField = studyField
    End Sub
    Public Function getFirstName() As String

        Return _firstName
    End Function
    Public Function getLastName() As String

        Return _lastName
    End Function
    Public Function getEmail() As String

        Return _email
    End Function
    Public Function getStudyField() As String

        Return _studyField
    End Function
    Public Sub setFirstName(ByVal firstName As String)
        _firstName = firstName
    End Sub
    Public Sub setLastName(ByVal lastName As String)
        _lastName = lastName
    End Sub
    Public Sub setEmail(ByVal email As String)
        _email = email
    End Sub
    Public Sub setStudyField(ByVal studyfield As String)
        _studyField = studyfield
    End Sub
End Class
