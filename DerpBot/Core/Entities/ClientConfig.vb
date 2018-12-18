Imports SQLExpress

Public Class ClientConfig
    Inherits SQLObject

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "ClientConfig"
        End Get
    End Property

    <Store, NotNull>
    Public Property AlwaysDownloadUsers As Boolean
    <Store, NotNull>
    Public Property LogLevel As Integer
    <Store, NotNull>
    Public Property MessageCacheSize As Integer
    <Store>
    Public Property TotalShards As Integer?
End Class