Public Class DBNotFoundException
    Inherits Exception
    Sub New()
        MyBase.New("The database was not found")
    End Sub
End Class
