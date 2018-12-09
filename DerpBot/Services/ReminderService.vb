Imports Discord.WebSocket
Imports SQLExpress

<Service(ServiceType.Singleton, 0)>
Public Class RemindersService
    Implements IService

    Private ReadOnly _database As SQLExpressClient
    Private ReadOnly _client As DiscordShardedClient
    Private ReadOnly _message As MessageService

    Public Sub New(database As SQLExpressClient, client As DiscordShardedClient, message As MessageService, random As DerpRandom)
        _database = database
        _client = client
        _message = message
    End Sub

    Public Async Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Await ClearDelayedRemindersAsync()
        Return True
    End Function

    Public Async Function ClearDelayedRemindersAsync() As Task
        Dim ids = _database.YieldData(Of ULong)("SELECT Id FROM reminders")
        For Each id In ids
            Dim reminder = Await _database.LoadObjectAsync(Of Reminder)(id)
            If reminder.RemindAt > Date.UtcNow Then Await RemindAsync(reminder)
        Next
    End Function

    Public Async Function RemindAsync(reminder As Reminder) As Task
        Dim user = _client.GetGuild(reminder.GuildId).GetUser(reminder.UserId)
        Await _message.NewMessageAsync(reminder.UserId, 0, reminder.ChannelId, $"{user.Mention} you wanted me to remind you:{vbLf}{reminder.Reminder}")
        Dim guild = Await _database.LoadObjectAsync(Of Guild)(reminder.GuildId)
        guild.Reminders.Remove(reminder)
        Await _database.UpdateObjectAsync(guild)
        Await _database.RemoveObjectAsync(reminder)
    End Function

    Public Async Function CreateReminder(content As String, guildId As ULong, channelId As ULong, userId As ULong, toExecute As TimeSpan) As Task
        Dim reminder = New Reminder With
        {
            .ChannelId = channelId,
            .GuildId = guildId,
            .Reminder = content,
            .UserId = userId,
            .RemindAt = Date.UtcNow + toExecute
        }
        Dim guild = Await _database.LoadObjectAsync(Of Guild)(guildId)
        guild.Reminders.Add(reminder)
        Await _database.UpdateObjectAsync(guild)
    End Function

End Class
