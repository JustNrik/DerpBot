Imports SQLExpress

Public Class User
    Inherits SQLObject

    <Store>
    Public Property Money As Integer = 10
    <Store>
    Public Property LastClaimed As Date = Date.Now.AddDays(-1)

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "Users"
        End Get
    End Property
End Class