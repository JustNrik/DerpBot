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
    Private WithEvents _command As CommandService
    Private WithEvents _db As SQLExpressClient
    Private ReadOnly _semaphore As New SemaphoreSlim(1, 1)

    Sub New(client As DiscordShardedClient, command As CommandService, db As SQLExpressClient)
        _client = client
        _command = command
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
        Write($"[{Date.Now,-19}] [{Center(ParseSource(source))}] ")
        ForegroundColor = ConsoleColor.White
        WriteLine(message)
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

    Async Sub OnDbLog(obj As IStoreableObject, logType As LogType) Handles _db.Log
        Await _semaphore.WaitAsync()
        Select Case logType
            Case LogType.Delete
                ForegroundColor = ConsoleColor.Red
            Case Else
                ForegroundColor = ConsoleColor.Green
        End Select
        Write($"[{Date.UtcNow,-19}] [{Center("Database")}] ")
        ForegroundColor = ConsoleColor.White
        WriteLine($"An object of {obj.TableName} ({obj.Id}) has been {If(logType = LogType.Load, Center("loaded"), Center(logType.ToString() & "d"))}")
        _semaphore.Release()
    End Sub
    Function OnLeftGuild(guild As SocketGuild) As Task Handles _client.LeftGuild
        Return LogAsync($"We have been kicked from {guild.Name} :(", LogSource.LeftGuild)
    End Function

    Async Function OnJoinedGuild(guild As SocketGuild) As Task Handles _client.JoinedGuild
        Dim guildObj As New Guild With {.Owner = guild.OwnerId, .Id = guild.Id}
        Await LogAsync($"{guild.Owner.Username}#{guild.Owner.Discriminator} has joined us to his guild {guild.Name}", LogSource.JoinedGuild)
        _db.CreateNewObject(guildObj)
        Dim count = _client.GetShardFor(guild).Guilds.Count
        Dim currentConfig = _db.LoadObject(Of ClientConfig)(CLIENT_CONFIG)
        If count >= 2000 AndAlso currentConfig.TotalShards < count \ 2000 Then
            currentConfig.TotalShards = count \ 2000
            _db.UpdateObject(currentConfig)
            Await LogAsync($"A new shard has been added, restart the bot to initialize it! Total Shards: {currentConfig.TotalShards}", LogSource.JoinedGuild)
        End If
    End Function

    Async Function OnGuildAvailable(guild As SocketGuild) As Task Handles _client.GuildAvailable
        Await LogAsync($"{guild.Name} is now available", LogSource.GuildAvailable)
        Dim guildObj As New Guild With {.Owner = guild.OwnerId, .Id = guild.Id}
        If Not _db.CheckExistence(guildObj) Then _db.CreateNewObject(guildObj)
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
        If Not Await _db.CheckExistenceAsync(userObj) Then Await _db.CreateNewObjectAsync(userObj)
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

    Function OnCommandErrored(result As ExecutionFailedResult, context As ICommandContext, provider As IServiceProvider) As Task Handles _command.CommandErrored
        Dim ctx = DirectCast(context, DerpContext)
        If result.Reason.Contains("exception") Then
            Return LogAsync($"The command ""{result.Command}"" failed to be executed by the user {ctx.User.ToString()} in the guild/channel {ctx.Guild}/{ctx.Channel}. Reason: {result.Reason}, Exception trace: {result.Exception}", LogSource.CommandError)
        End If
        Return LogAsync($"The command ""{result.Command}"" failed to be executed by the user {ctx.User.ToString()} in the guild/channel {ctx.Guild}/{ctx.Channel}. Reason: {result.Reason}", LogSource.CommandError)
    End Function

    Function OnCommandExecuted(command As Command, result As CommandResult, context As ICommandContext, provider As IServiceProvider) As Task Handles _command.CommandExecuted
        Dim ctx = DirectCast(context, DerpContext)
        Return LogAsync($"The command {command.Name} was successfully executed by the user {ctx.User.ToString()} in the guild/channel {ctx.Guild}/{ctx.Channel}", LogSource.Command)
    End Function
End Class
