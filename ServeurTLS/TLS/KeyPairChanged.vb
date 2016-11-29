Public Class KeyPairChanged
    Inherits Exception
    Sub New()
        MyBase.New("The key pair has changed")
    End Sub
End Class
