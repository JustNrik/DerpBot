Imports Discord
Imports Discord.WebSocket
Imports SQLExpress
Imports System.Console
Imports System.Threading
Imports Qmmands

<Service(ServiceType.Singleton, 254)>
Public Class LogService
    Implements IService

    Private WithEvents _client As DiscordShardedClient
    Private WithEvents _db As SQLExpressClient
    Private ReadOnly _semaphore As New SemaphoreSlim(1, 1)

    Sub New(client As DiscordShardedClient, db As SQLExpressClient)
        _client = client
        _db = db
    End Sub

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(True)
    End Function

    Public Async Function LogAsync(message As String, source As LogSource) As Task
        Await _semaphore.WaitAsync()
        Select Case source
            Case LogSource.ShardDisconnected, LogSource.GuildUnavailable, LogSource.LeftGuild, LogSource.CommandError, LogSource.ServiceError
                ForegroundColor = ConsoleColor.Red
            Case Else
                ForegroundColor = ConsoleColor.Green
        End Select
        Await Out.WriteAsync($"[{Date.Now,-19}] [{Center(ParseSource(source))}] ")
        ForegroundColor = ConsoleColor.White
        Await Out.WriteLineAsync(message)
        _semaphore.Release()
    End Function

    Async Function OnLog(message As LogMessage) As Task Handles _client.Log
        Await _semaphore.WaitAsync()
        Select Case message.Severity
            Case LogSeverity.Critical, LogSeverity.Error
                ForegroundColor = ConsoleColor.Red
            Case LogSeverity.Warning
                ForegroundColor = ConsoleColor.Yellow
            Case LogSeverity.Info
                ForegroundColor = ConsoleColor.Magenta
            Case LogSeverity.Verbose, LogSeverity.Debug
                ForegroundColor = ConsoleColor.Cyan
        End Select
        Write($"[{Date.UtcNow,-19}] [{Center($"{message.Severity}")}] ")
        ForegroundColor = ConsoleColor.White
        WriteLine($"{message.Source}: {message.Message} {message.Exception}")
        _semaphore.Release()
    End Function

    Async Function OnDbLog(result As SqlResult) As Task Handles _db.Log
        Await _semaphore.WaitAsync()
        Select Case result.LogSource
            Case DbLogSource.Remove
                ForegroundColor = ConsoleColor.Red
            Case Else
                ForegroundColor = ConsoleColor.Green
        End Select
        Await Out.WriteAsync($"[{Date.UtcNow,-19}] [{Center("Database")}] ")
        ForegroundColor = ConsoleColor.White
        If result.Successful Then
            Await Out.WriteLineAsync($"An object of {result.Obj.TableName} ({result.Obj.Id}) has been {If(result.LogSource = DbLogSource.Load, Center("loaded"), Center(result.LogSource.ToString() & "d"))}")
        Else
            Await Out.WriteLineAsync($"Failed to {result.LogSource.ToString()} an object from {result.Obj.TableName} ({result.Obj.Id}), probably because of: {result.Summary} {result.Exception?.Message}")
        End If
        _semaphore.Release()
    End Function
    Function OnLeftGuild(guild As SocketGuild) As Task Handles _client.LeftGuild
        Return LogAsync($"We have been kicked from {guild.Name} :(", LogSource.LeftGuild)
    End Function

    Async Function OnJoinedGuild(guild As SocketGuild) As Task Handles _client.JoinedGuild
        Dim guildObj As New Guild With {.Owner = guild.OwnerId, .Id = guild.Id}
        Await LogAsync($"{guild.Owner.Username}#{guild.Owner.Discriminator} has joined us to his guild {guild.Name}", LogSource.JoinedGuild)
        Await _db.CreateObjectAsync(guildObj)
        Dim count = _client.GetShardFor(guild).Guilds.Count
        Dim currentConfig = Await _db.LoadObjectAsync(Of ClientConfig)(CLIENT_CONFIG)
        If count >= 2000 AndAlso currentConfig.TotalShards < count \ 2000 Then
            currentConfig.TotalShards = count \ 2000
            Await _db.UpdateObjectAsync(currentConfig)
            Await LogAsync($"A new shard has been added, restart the bot to initialize it! Total Shards: {currentConfig.TotalShards}", LogSource.JoinedGuild)
        End If
    End Function

    Async Function OnGuildAvailable(guild As SocketGuild) As Task Handles _client.GuildAvailable
        Await LogAsync($"{guild.Name} is now available", LogSource.GuildAvailable)
        Dim guildObj As New Guild With {.Owner = guild.OwnerId, .Id = guild.Id}
        If Not _db.CheckExistence(guildObj) Then Await _db.CreateObjectAsync(guildObj)
    End Function

    Function OnGuildMembersDownloaded(guild As SocketGuild) As Task Handles _client.GuildMembersDownloaded
        Return LogAsync($"{guild.Name} has downloaded all offline members", LogSource.GuildMembersDownloaded)
    End Function

    Function OnGuildUnavailable(guild As SocketGuild) As Task Handles _client.GuildUnavailable
        Return LogAsync($"{guild.Name} is unavailable", LogSource.GuildUnavailable)
    End Function

    Function OnUserBanned(user As SocketUser, guild As SocketGuild) As Task Handles _client.UserBanned
        Return LogAsync($"{user.Username}#{user.Discriminator} has been banned from {guild.Name}", LogSource.UserBanned)
    End Function

    Async Function OnUserJoined(user As SocketGuildUser) As Task Handles _client.UserJoined
        Await LogAsync($"{user.Username}#{user.Discriminator} has joined to {user.Guild.Name}", LogSource.UserJoined)
        Dim userObj As New User With {.Id = user.Id}
        If Not Await _db.CheckExistenceAsync(userObj) Then Await _db.CreateObjectAsync(userObj)
    End Function

    Function OnUserUnbanned(user As SocketUser, guild As SocketGuild) As Task Handles _client.UserUnbanned
        Return LogAsync($"{user.Username}#{user.Discriminator} has been unbanned from {guild.Name}", LogSource.UserUnbanned)
    End Function

    Function OnShardReady(client As DiscordSocketClient) As Task Handles _client.ShardReady
        Return LogAsync($"Shard#{client?.ShardId.ToString("D4")} is ready", LogSource.ShardReady)
    End Function

    Function OnShardConnected(client As DiscordSocketClient) As Task Handles _client.ShardConnected
        Return LogAsync($"Shard#{client?.ShardId.ToString("D4")} is connected", LogSource.ShardConnected)
    End Function

    Function OnShardDisconnected(exception As Exception, client As DiscordSocketClient) As Task Handles _client.ShardDisconnected
        Return LogAsync($"Shard#{client?.ShardId.ToString("D4")} disconnected! Exception? = {If(exception?.Message, "nothing...")}", LogSource.ShardDisconnected)
    End Function
End Class
