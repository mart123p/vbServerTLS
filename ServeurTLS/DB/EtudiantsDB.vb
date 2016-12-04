Imports System.Data.OleDb
Public Class EtudiantsDB

    Dim connection As OleDbConnection
    Sub New()
        Try
            connection = New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & Config.dbFile)
            connection.Open()
        Catch ex As Exception
            Throw New DBNotFoundException
        End Try
    End Sub

    Public Function setUser(ByVal user As User, ByVal id As String, ByVal iv As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "INSERT INTO users (mat, firstName, lastName, email, studyField, [password], iv, birthday) VALUES(?,?,?,?,?,?,?,?)"
            command.Parameters.Add("mat", OleDbType.VarWChar, 255)
            command.Parameters.Add("firstName", OleDbType.VarWChar, 255)
            command.Parameters.Add("lastName", OleDbType.VarWChar, 255)
            command.Parameters.Add("email", OleDbType.VarWChar, 255)
            command.Parameters.Add("studyField", OleDbType.VarWChar, 255)
            command.Parameters.Add("password", OleDbType.VarWChar, 255)
            command.Parameters.Add("iv", OleDbType.VarWChar, 255)
            command.Parameters.Add("birthday", OleDbType.Date)

            Dim birthday As Date = Date.Parse(user.getBirthday)
            command.Parameters(0).Value = id
            command.Parameters(1).Value = Char.ToUpperInvariant(user.getFirstName(0)) & user.getFirstName.Substring(1)
            command.Parameters(2).Value = Char.ToUpperInvariant(user.getLastName(0)) & user.getLastName.Substring(1)
            command.Parameters(3).Value = user.getEmail
            command.Parameters(4).Value = user.getStudyField
            command.Parameters(5).Value = Hash.sha256(Config.hashSalt & iv & user.getPassword)
            command.Parameters(6).Value = iv
            command.Parameters(7).Value = birthday

            command.Prepare()
            command.ExecuteNonQuery()
            command.Dispose()
            Return True
        Catch ex As Exception
            MsgBox(ex.ToString)
            Return False
        End Try

    End Function

    Public Function getUserDetail(ByVal id As String) As Etudiants
        Try
            Dim command As New OleDbCommand
            command.Connection = connection
            command.CommandText = "SELECT firstName,lastName,email,studyField FROM users WHERE mat = ?"
            command.Parameters.Add("id", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = id

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            r.Read()
            command.Dispose()
            Dim etudiant As New Etudiants(r(0), r(1), r(2), r(3))
            r.Close()
            Return etudiant
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function setProfileEmail(ByVal email As String, ByVal id As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "UPDATE users SET email= ? WHERE mat = ?"
            command.Parameters.Add("email", OleDbType.VarWChar, 255)
            command.Parameters.Add("mat", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = email
            command.Parameters(1).Value = id

            command.Prepare()
            command.ExecuteNonQuery()
            command.Dispose()
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setProfileStudyField(ByVal studyField As String, ByVal id As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "UPDATE users SET studyField = ? WHERE mat = ?"
            command.Parameters.Add("studyField", OleDbType.VarWChar, 255)
            command.Parameters.Add("mat", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = studyField
            command.Parameters(1).Value = id

            command.Prepare()
            command.ExecuteNonQuery()
            command.Dispose()
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setProfilePassword(ByVal password As String, ByVal iv As String, ByVal id As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "UPDATE users SET [password] = ?,iv = ? WHERE mat = ?"
            command.Parameters.Add("password", OleDbType.VarWChar, 255)
            command.Parameters.Add("iv", OleDbType.VarWChar, 255)
            command.Parameters.Add("mat", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = password
            command.Parameters(1).Value = iv
            command.Parameters(2).Value = id

            command.Prepare()
            command.ExecuteNonQuery()
            command.Dispose()
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function getStudyFields() As String()
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "SELECT * FROM studyFields ORDER BY studyFieldName"

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            Dim str As New List(Of String)

            While r.Read()
                str.Add(r(1))
            End While

            r.Close()
            command.Dispose()
            Return str.ToArray
        Catch ex As Exception
            MsgBox(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Function getConnect(ByVal id As String, ByVal password As String) As Boolean
        Try
            'We need to hash the password using the id
            Dim command As New OleDbCommand
            command.Connection = connection
            command.CommandText = "SELECT COUNT(mat) as nbuser FROM users WHERE mat = ? AND password = ?"
            command.Parameters.Add("mat", OleDbType.VarWChar, 255)
            command.Parameters.Add("password", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = id
            command.Parameters(1).Value = password
            command.Prepare()

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            command.Dispose()
            r.Read()
            If r(0) >= 1 Then
                r.Close()
                Return True
            Else
                r.Close()
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
            Return False
        End Try

    End Function

    Public Function getIV(ByVal id As String) As String
        Try
            Dim command As New OleDbCommand
            command.Connection = connection
            command.CommandText = "SELECT iv FROM users WHERE mat = ?"
            command.Parameters.Add("id", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = id

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            r.Read()
            command.Dispose()
            Dim iv As String = r(0).ToString
            r.Close()
            Return iv
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function userExists(ByVal email As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "SELECT COUNT(email) as nbEmail FROM users WHERE email = ?"
            command.Parameters.Add("email", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = email
            command.Prepare()

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            command.Dispose()
            r.Read()
            If r(0) >= 1 Then
                r.Close()
                Return True
            Else
                r.Close()
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function userExistsMat(ByVal mat As String) As Boolean
        Try
            Dim command As New OleDbCommand()
            command.Connection = connection
            command.CommandText = "SELECT COUNT(email) as nbEmail FROM users WHERE mat = ?"
            command.Parameters.Add("mat", OleDbType.VarWChar, 255)
            command.Parameters(0).Value = mat
            command.Prepare()

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            command.Dispose()
            r.Read()
            If r(0) >= 1 Then
                r.Close()
                Return True
            Else
                r.Close()
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function getEtudiants(ByVal studyFields As String) As Etudiants()
        Dim etudiants As New List(Of Etudiants)
        Try

            Dim command As New OleDbCommand()
            command.Connection = connection
            If studyFields = "" Then
                command.CommandText = "SELECT * FROM users ORDER BY lastName"
            Else
                command.CommandText = "SELECT * FROM users WHERE studyField = ? ORDER BY lastName"
                command.Parameters.Add("studyField", OleDbType.VarWChar, 255)
                command.Parameters(0).Value = studyFields
                command.Prepare()
            End If

            Dim r As OleDbDataReader
            r = command.ExecuteReader
            command.Dispose()
            While r.Read()
                Console.WriteLine("It works! " & r(2))
                etudiants.Add(New Etudiants(r(2), r(3), r(4), r(5)))
            End While
            r.Close()
            Return etudiants.ToArray
        Catch ex As Exception
            MsgBox(ex.ToString)
            etudiants.Add(New Etudiants("", "", "", ""))
            Return etudiants.ToArray
        End Try
    End Function

    Public Sub close()
        connection.Close()
    End Sub
End Class
