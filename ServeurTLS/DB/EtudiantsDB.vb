Imports System.Data.OleDb
Public Class EtudiantsDB

    Dim connection As OleDbConnection
    Sub New()
        Try
            connection = New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=db.accdb")
        Catch ex As Exception
            Throw New DBNotFoundException
        End Try
    End Sub

    Public Function setUser(ByVal user As User, ByVal id As Integer, ByVal iv As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "INSERT INTO users (mat,firstName,lastName,email,studyField,password,iv,birthday) VALUES(?,?,?,?,?,?,?,?)"
            command.Parameters.Add("mat", OleDbType.Integer, 4)
            command.Parameters.Add("firstName", OleDbType.VarWChar, 255)
            command.Parameters.Add("lastName", OleDbType.VarWChar, 255)
            command.Parameters.Add("email", OleDbType.VarWChar, 255)
            command.Parameters.Add("studyField", OleDbType.VarWChar, 255)
            command.Parameters.Add("password", OleDbType.VarWChar, 255)
            command.Parameters.Add("iv", OleDbType.VarWChar, 255)
            command.Parameters.Add("birthday", OleDbType.Date, 16)

            command.Parameters(0).Value = id
            command.Parameters(1).Value = user.getFirstName
            command.Parameters(2).Value = user.getLastName
            command.Parameters(3).Value = user.getEmail
            command.Parameters(4).Value = user.getStudyField
            command.Parameters(5).Value = user.getPassword
            command.Parameters(6).Value = iv
            command.Parameters(7).Value = user.getBirthday

            command.Prepare()
            command.ExecuteNonQuery()
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function setProfileEmail(ByVal email As String, ByVal id As Integer) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "UPDATE users SET email= ? WHERE mat = ?"
            command.Parameters.Add("email", OleDbType.VarWChar, 255)
            command.Parameters.Add("mat", OleDbType.Integer, 4)
            command.Parameters(0).Value = email
            command.Parameters(1).Value = id

            command.Prepare()
            command.ExecuteNonQuery()
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setProfileStudyField(ByVal studyField As String, ByVal id As Integer) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "UPDATE users SET studyField = ? WHERE mat = ?"
            command.Parameters.Add("studyField", OleDbType.VarWChar, 255)
            command.Parameters.Add("mat", OleDbType.Integer, 4)
            command.Parameters(0).Value = studyField
            command.Parameters(1).Value = id

            command.Prepare()
            command.ExecuteNonQuery()
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setProfilePassword(ByVal password As String, ByVal iv As String, ByVal id As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "UPDATE users SET studyField = ?,iv = ? WHERE mat = ?"
            command.Parameters.Add("studyField", OleDbType.VarWChar, 255)
            command.Parameters.Add("iv", OleDbType.VarWChar, 255)
            command.Parameters.Add("mat", OleDbType.Integer, 4)
            command.Parameters(0).Value = password
            command.Parameters(1).Value = iv
            command.Parameters(2).Value = id

            command.Prepare()
            command.ExecuteNonQuery()
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function getStudyFields() As String()
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "SELECT * FROM studyFields"

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            Dim str As New List(Of String)
            For i = 0 To r.FieldCount - 1
                r.Read()
                str.Add(r(1))
            Next

            Return str.ToArray
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function getConnect(ByVal id As Integer, ByVal password As String) As Boolean
        Try
            'We need to hash the password using the id
            Dim command As New OleDbCommand
            command.Connection = connection
            command.CommandText = "SELECT COUNT(mat) as nbuser WHERE mat = ? AND password = ?"
            command.Parameters.Add("mat", OleDbType.VarWChar, 255)
            command.Parameters.Add("password", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = id
            command.Parameters(1).Value = password
            command.Prepare()

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            r.Read()
            If r(0) >= 1 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function getIV(ByVal id As Integer) As String
        Try
            Dim command As New OleDbCommand
            command.Connection = connection
            command.CommandText = "SELECT iv WHERE mat = ?"
            command.Parameters.Add("iv", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = id

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            r.Read()
            Return r(0)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function userExists(ByVal email As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "SELECT COUNT(email) as nbEmail WHERE email = ?"
            command.Parameters.Add("email", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = email
            command.Prepare()

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            r.Read()
            If r(0) >= 1 Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function getEtudiants(ByVal studyFields As String) As Etudiants()
        Try
            Dim etudiants As New List(Of Etudiants)
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "SELECT * FROM users"
            Dim r As OleDbDataReader
            r = command.ExecuteReader
            For i = 0 To r.FieldCount - 1
                r.Read()
                etudiants.Add(New Etudiants(r("firstName"), r("lastName"), r("email"), r("studyField")))
            Next
            Return etudiants.ToArray
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
End Class
