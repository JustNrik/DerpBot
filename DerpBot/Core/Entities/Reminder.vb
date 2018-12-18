Imports SQLExpress
Public Class Reminder
    Inherits SqlObject

    <Store, Varchar(200)>
    Public Property Reminder As String
    <Store, Varchar(100)>
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
            Return "Reminders"
        End Get
    End Property
End Class
