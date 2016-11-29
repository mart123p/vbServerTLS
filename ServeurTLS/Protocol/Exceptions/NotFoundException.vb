Public Class NotFoundException
    Inherits Exception
    Sub New()
        MyBase.New("The resource was not found")
    End Sub
End Class
