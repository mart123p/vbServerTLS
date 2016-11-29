Imports System.Security.Cryptography
Imports System.Text
Module Hash

    Public Function sha256(ByVal str As String) As String
        Dim uEncode As New UnicodeEncoding()
        Dim bytClearString() As Byte = uEncode.GetBytes(str)
        Dim sha As New _
        System.Security.Cryptography.SHA256Managed()
        Dim hash() As Byte = sha.ComputeHash(bytClearString)
        Return BitConverter.ToString(hash).Replace("-", "").ToLower
    End Function



End Module
