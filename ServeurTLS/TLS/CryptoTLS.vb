Imports System.Security.Cryptography
Imports System.Text

Public Class CryptoTLS
    Inherits AES256
    Dim HostPrivateKey As String
    Dim HostPublicKey As String
    Dim HostAESKey As Aes
    Dim rsaProvider As RSACryptoServiceProvider
    Sub New(Optional ByVal isPersitent As Boolean = False)
        rsaProvider = New RSACryptoServiceProvider(2048)
        If (isPersitent) Then
            'We need to use the same public and private key, this is usefull to generate a public key signature for the server
            If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantServeur\TLS", "PrivateKey", Nothing) IsNot Nothing And My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantServeur\TLS", "PublicKey", Nothing) IsNot Nothing Then
                'We generate the PPK
                HostPrivateKey = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantServeur\TLS", "PrivateKey", Nothing)
                HostPublicKey = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantServeur\TLS", "PublicKey", Nothing)
                rsaProvider = New RSACryptoServiceProvider()
                Try
                    rsaProvider.FromXmlString(HostPrivateKey)
                    rsaProvider.FromXmlString(HostPublicKey)
                Catch ex As Exception
                    Throw New KeyPairChanged
                End Try

            Else
                My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantServeur\TLS", "PrivateKey", rsaProvider.ToXmlString(True))
                My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\CegepTR\EtudiantServeur\TLS", "PublicKey", rsaProvider.ToXmlString(False))
                HostPrivateKey = rsaProvider.ToXmlString(True)
                HostPublicKey = rsaProvider.ToXmlString(False)
            End If

        Else
            HostPrivateKey = rsaProvider.ToXmlString(True)
            HostPublicKey = rsaProvider.ToXmlString(False)
        End If
        'We generate a new AES key

    End Sub

    'Encrypt using the others publicKey
    Public Function RSAEncrypt(ByVal str As String, ByVal publicKeyRemote As String) As Byte()
        Dim rsaRemote As New RSACryptoServiceProvider(2048)
        rsaRemote.FromXmlString(publicKeyRemote)
        Return rsaRemote.Encrypt(Encoding.ASCII.GetBytes(str), True)
    End Function

    'Decrypt using the host private key. The rsa is used to send an aes key
    Public Function RSADecrypt(ByVal data As Byte()) As Byte()
        rsaProvider.FromXmlString(HostPrivateKey)
        Return rsaProvider.Decrypt(data, True)
    End Function

    Public Function AESEncrypt(ByVal str As String, ByVal key As String) As Byte()
        Return Encrypt(str, key)
    End Function

    Public Function AESDecrypt(ByVal data As Byte(), ByVal key As String) As String
        Return Decrypt(data, key)
    End Function

    Public Function getCurrentPublicKey() As String
        Return HostPublicKey
    End Function

    Public Function getCurrentPrivateKey() As String
        Return HostPrivateKey
    End Function
End Class
