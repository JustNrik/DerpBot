Imports SQLExpress
Public Class Message
    Inherits SQLObject

    <Store>
    Public Property UserId As ULong
    <Store>
    Public Property ExecutingId As ULong
    <Store>
    Public Property ResponseId As ULong
    <Store>
    Public Property ChannelId As ULong
    <Store>
    Public Property CreatedAt As DateTimeOffset
    <Store>
    Public Property AttachedFile As Boolean


    Public Overrides ReadOnly Property TableName As String
        Get
            Return "Messages"
        End Get
    End Property
End Class
