Imports SQLExpress

Public Class CommandConfig
    Inherits SQLObject

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "commandconfig"
        End Get
    End Property

    <Store>
    Public Property CaseSensitiveCommands As Boolean
    <Store>
    Public Property LogLevel As Integer
End Class

