Imports SQLExpress
Public NotInheritable Class Guild
    Inherits SQLObject

    <Store>
    Public Property Prefixes As New List(Of String) From {"d!"}
    <Store, NotNull>
    Public Property Owner As ULong
    <Store>
    Public Property WelcomeChannel As ULong?
    <Store>
    Public Property AnnouncementChannel As ULong?
    <Store>
    Public Property Starboard As ULong?
    <Store>
    Public Property StarCount As Integer
    <Store>
    Public Property StarredMessages As New List(Of Message)
    <Store>
    Public Property Admins As New List(Of ULong)
    <Store>
    Public Property Mods As New List(Of ULong)
    <Store, NotNull>
    Public Property UseWhiteList As Boolean
    <Store>
    Public Property WhiteList As New List(Of ULong) From {304088134352764930}
    <Store>
    Public Property BlackList As New List(Of ULong)
    <Store>
    Public Property RestrictedChannels As New List(Of ULong)
    <Store>
    Public Property Tags As New List(Of Tag)
    <Store>
    Public Property Reminders As New List(Of Reminder)

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "guilds"
        End Get
    End Property

End Class
