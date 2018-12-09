Imports SQLExpress
Public Class Reminder
    Inherits SQLObject

    <Store, StringLength(200)>
    Public Property Reminder As String
    <Store, StringLength(100)>
    Public Property JumpLink As String
    <Store, NotNull>
    Public Property GuildId As ULong
    <Store, NotNull>
    Public Property ChannelId As ULong
    <Store, NotNull>
    Public Property UserId As ULong
    <Store, NotNull>
    Public Property RemindAt As Date

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "reminders"
        End Get
    End Property
End Class
