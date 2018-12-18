Imports SQLExpress

Public Class CommandConfig
    Inherits SQLObject

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "CommandConfig"
        End Get
    End Property

    <Store>
    Public Property CaseSensitiveCommands As Boolean
End Class

