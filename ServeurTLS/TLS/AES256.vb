Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public MustInherit Class AES256
    Protected Function Encrypt(ByVal str As String, ByVal key As String) As Byte()
        Dim datatmp As Byte()

        'We need to generate 
        Dim rngCsp As New RNGCryptoServiceProvider()
        Dim iv(15) As Byte
        rngCsp.GetBytes(iv)
        Console.WriteLine(Convert.ToBase64String(iv))


        Dim aes As New AesCryptoServiceProvider()
        aes.BlockSize = 128
        aes.KeySize = 256
        aes.IV = iv
        aes.Key = Convert.FromBase64String(key)
        aes.Mode = CipherMode.CBC
        aes.Padding = PaddingMode.PKCS7
        Dim strByte As Byte()
        strByte = Encoding.UTF8.GetBytes(str)


        Using encrypter As ICryptoTransform = aes.CreateEncryptor()
            datatmp = encrypter.TransformFinalBlock(strByte, 0, strByte.Length)
        End Using
        Dim contentLength As Integer = datatmp.Length + 20
        Dim contentLengthByte As Byte() = BitConverter.GetBytes(contentLength)

        Dim data(datatmp.Length + 19) As Byte
        'We store the first 16 iv bytes in the data array after the encrypted message
        For i = 0 To 3
            data(i) = contentLengthByte(i)
        Next

        For i = 0 To iv.Length - 1
            data(i + 4) = iv(i)
        Next
        For i = 0 To datatmp.Length - 1
            data(i + 20) = datatmp(i)
        Next
        Return data
    End Function

    Protected Function Decrypt(ByVal data As Byte(), ByVal key As String) As String
        Dim str As String = Nothing
        Dim contentLength(3) As Byte
        Dim iv(15) As Byte

        For i = 0 To 3
            contentLength(i) = data(i)
        Next

        For i = 4 To 19
            iv(i - 4) = data(i)
        Next
        Dim dataToDecrypt(BitConverter.ToInt32(contentLength, 0) - 21) As Byte
        For i = 20 To BitConverter.ToInt32(contentLength, 0) - 1
            dataToDecrypt(i - 20) = data(i)
        Next
        Console.WriteLine(key)
        Console.WriteLine(Convert.ToBase64String(iv))
        Console.WriteLine(dataToDecrypt.Length)

        Dim aes As New AesCryptoServiceProvider()
        aes.BlockSize = 128
        aes.KeySize = 256
        aes.IV = iv
        aes.Key = Convert.FromBase64String(key)
        aes.Mode = CipherMode.CBC
        aes.Padding = PaddingMode.PKCS7

        Dim decryptedBytes As Byte()

        Using decrypter As ICryptoTransform = aes.CreateDecryptor()
            decryptedBytes = decrypter.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length)
        End Using
        str = Encoding.UTF8.GetString(decryptedBytes)
        Return str
    End Function

    Public Shared Function Generate() As String
        Dim rngCsp As New RNGCryptoServiceProvider()
        Dim key(31) As Byte
        rngCsp.GetBytes(key)
        Return Convert.ToBase64String(key)
    End Function
End Class