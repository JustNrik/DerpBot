Imports SQLExpress

Public Class BotConfig
    Inherits SQLObject

    <Store, NotNull, StringLength(60)>
    Public Property Token As String = ""

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "botconfig"
        End Get
    End Property
End Class
