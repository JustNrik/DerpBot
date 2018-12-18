Imports SQLExpress

Public Class BotConfig
    Inherits SqlObject

    <Store, NotNull, Varchar(60)>
    Public Property Token As String = ""

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "BotConfig"
        End Get
    End Property
End Class
