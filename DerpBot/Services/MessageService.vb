﻿Imports Discord
Imports Discord.WebSocket
Imports SqlExpress
Imports System.Collections.Concurrent
Imports System.IO
Imports Qmmands
Imports Qmmands.CommandUtilities
Imports System.Threading

<Service(ServiceType.Singleton, 253)>
Public Class MessageService
    Implements IService

    Private WithEvents _client As DiscordShardedClient
    Private WithEvents _commands As CommandService
    Private ReadOnly _database As SqlExpressClient
    Private ReadOnly _interactive As InteractiveService
    Private ReadOnly _random As DerpRandom
    Private ReadOnly _economy As EcomonyService
    Private ReadOnly _log As LogService
    Private ReadOnly _semaphore As New SemaphoreSlim(1, 1)
    Private ReadOnly _services As IServiceProvider
    Private Const CACHE_SIZE = 10

    Private ReadOnly _messageCache As New ConcurrentDictionary(Of ULong, ConcurrentQueue(Of Message))

    Sub New(client As DiscordShardedClient, database As SqlExpressClient,
            commands As CommandService, interactive As InteractiveService,
            services As IServiceProvider, random As DerpRandom,
            economy As EcomonyService, log As LogService)

        _client = client
        _database = database
        _commands = commands
        _interactive = interactive
        _services = services
        _random = random
        _economy = economy
        _log = log
    End Sub

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(True)
    End Function

    Public Function OnMessageReceived(msg As SocketMessage) As Task Handles _client.MessageReceived
        Return Task.Run(Function() HandleMessageAsync(msg, False))
    End Function

    Public Function OnMessageUpdated(cache As Cacheable(Of IMessage, ULong), message As SocketMessage, channel As ISocketMessageChannel) As Task Handles _client.MessageUpdated
        Return Task.Run(Function() HandleMessageAsync(message, True))
    End Function

    Async Function HandleMessageAsync(msg As SocketMessage, isUpdated As Boolean) As Task
        Dim channel = TryCast(msg.Channel, SocketGuildChannel)
        Dim message = TryCast(msg, SocketUserMessage)
        If msg.Author.IsBot OrElse String.IsNullOrEmpty(msg.Content) OrElse channel Is Nothing OrElse message Is Nothing Then Return

        Await _semaphore.WaitAsync()

        If Not isUpdated AndAlso _random.Next(100) < 10 Then
            Dim user = Await _database.LoadObjectAsync(Of User)(message.Author.Id)
            Dim amount = _random.Next(1, 4)
            Dim __ = Task.Run(Function() _economy.AddMoneyAsync(user, amount))
            Await _log.LogAsync($"The user {DirectCast(message.Author, IGuildUser).GetDisplayName()} ({message.Author.Id}) won {amount} money in the guild {channel.Guild.Name}!", LogSource.Economy)
        End If

        Dim guild = Await _database.LoadObjectAsync(Of Guild)(channel.Guild.Id)

        If guild.BlackList.Contains(message.Author.Id) OrElse
           guild.RestrictedChannels.Contains(channel.Id) OrElse
           guild.UseWhiteList AndAlso Not guild.WhiteList.Contains(message.Author.Id) Then Return

        Dim output As String = ""
        If HasAnyPrefix(message.Content, guild.Prefixes, Nothing, output) OrElse
           HasMentionPrefix(message.Content, output) Then

            Dim context As New DerpContext(_client, message)
            If Not context.Guild.CurrentUser.GetPermissions(context.Channel).SendMessages Then Return
            Await _commands.ExecuteAsync(output, context, _services)
        End If
        _semaphore.Release()
    End Function

    Function OnCommandErrored(result As ExecutionFailedResult, context As ICommandContext, provider As IServiceProvider) As Task Handles _commands.CommandErrored
        Dim ctx = DirectCast(context, DerpContext)
        Return If(result.Reason.Contains("exception"),
            _log.LogAsync($"The command ""{result.Command}"" failed to be executed by the user {ctx.User.ToString()} in the guild/channel {ctx.Guild}/{ctx.Channel}. Reason: {result.Reason}, Exception trace: {result.Exception}", LogSource.CommandError),
            _log.LogAsync($"The command ""{result.Command}"" failed to be executed by the user {ctx.User.ToString()} in the guild/channel {ctx.Guild}/{ctx.Channel}. Reason: {result.Reason}", LogSource.CommandError))
    End Function

    Function OnCommandExecuted(command As Command, result As CommandResult, context As ICommandContext, provider As IServiceProvider) As Task Handles _commands.CommandExecuted
        Dim ctx = DirectCast(context, DerpContext)
        Return _log.LogAsync($"The command {command.Name} was successfully executed by the user {ctx.User.ToString()} in the guild/channel {ctx.Guild}/{ctx.Channel}", LogSource.Command)
    End Function

    Public Async Function SendMessageAsync(context As ICommandContext, Optional content As String = Nothing, Optional isTTS As Boolean = False, Optional embed As Embed = Nothing) As Task(Of IUserMessage)
        Dim ctx = DirectCast(context, DerpContext)
        Dim message = Await GetExistingMessageAsync(ctx)
        If message Is Nothing Then Return Await NewMessageAsync(context, content, isTTS, embed)

        Dim currentUser = ctx.Guild.CurrentUser
        Dim perms = currentUser.GetPermissions(ctx.Channel)

        If perms.ManageMessages Then Await message.RemoveAllReactionsAsync

        Await message.ModifyAsync(Sub(x)
                                      x.Content = content
                                      x.Embed = embed
                                  End Sub)

        Return message
    End Function

    Public Function SendEmbedAsync(context As ICommandContext, embed As Embed) As Task(Of IUserMessage)
        Return SendMessageAsync(context,,, embed)
    End Function

    Public Function NewMessageAsync(context As ICommandContext, content As String, Optional isTTS As Boolean = False, Optional embed As Embed = Nothing) As Task(Of IUserMessage)
        Dim ctx = DirectCast(context, DerpContext)
        Return NewMessageAsync(ctx.User.Id, ctx.Message.Id, ctx.Channel.Id, content, isTTS, embed)
    End Function

    Public Async Function NewMessageAsync(userId As ULong,
                                          messageId As ULong,
                                          channelId As ULong,
                                          content As String,
                                          Optional isTTS As Boolean = False,
                                          Optional embed As Embed = Nothing) As Task(Of IUserMessage)

        Dim channel = TryCast(_client.GetChannel(channelId), SocketTextChannel)
        If channel Is Nothing Then Return Nothing

        Dim response = Await channel.SendMessageAsync(content, isTTS, embed)
        Await NewItemAsync(userId, channelId, response.CreatedAt, messageId, response.Id, False)

        Return response
    End Function

    Public Async Function DeleteMessageAsync(context As ICommandContext, message As IUserMessage) As Task
        Dim ctx = DirectCast(context, DerpContext)
        Dim found As ConcurrentQueue(Of Message) = Nothing
        If _messageCache.TryGetValue(ctx.User.Id, found) AndAlso found.Any(Function(x) x.ResponseId = message.Id) Then

            Await message.DeleteAsync
            _messageCache(ctx.User.Id) = New ConcurrentQueue(Of Message)(found.Where(Function(x) x.ResponseId <> message.Id))

            Dim newFound As ConcurrentQueue(Of Message) = Nothing
            If _messageCache.TryGetValue(ctx.User.Id, newFound) AndAlso newFound.IsEmpty Then _messageCache.TryRemove(ctx.User.Id, Nothing)
        End If
    End Function

    Public Async Function SendPaginatedMessageAsync(context As ICommandContext, paginator As BasePaginator) As Task(Of IUserMessage)
        Dim ctx = DirectCast(context, DerpContext)
        Dim message = Await GetExistingMessageAsync(ctx)
        If message IsNot Nothing Then Await message.DeleteAsync

        Dim callback As ICallback = Nothing
        Select Case True
            Case TypeOf paginator Is HelpPaginatedMessage
                callback = New HelpPaginatedCallback(_interactive, ctx, DirectCast(paginator, HelpPaginatedMessage))
            Case TypeOf paginator Is CommandMenuMessage
                callback = New CommandMenuCallback(_commands, _services, _interactive, DirectCast(paginator, CommandMenuMessage), Me, ctx)
            Case TypeOf paginator Is PaginatedMessage
                callback = New PaginatedMessageCallback(_interactive, ctx, DirectCast(paginator, PaginatedMessage))
        End Select

        If callback Is Nothing Then Return Nothing

        Await callback.DisplayAsync.ConfigureAwait(False)
        Await NewItemAsync(ctx.User.Id, ctx.Channel.Id, callback.Message.CreatedAt, ctx.Message.Id, callback.Message.Id, False)
        Return callback.Message
    End Function

    Public Async Function ClearMessagesAsync(context As ICommandContext, amount As Integer) As Task(Of Integer)
        Dim found As ConcurrentQueue(Of Message) = Nothing
        Dim ctx = DirectCast(context, DerpContext)
        If Not _messageCache.TryGetValue(ctx.User.Id, found) Then Return 0

        Dim matching = found.Where(Function(x) x.ChannelId = ctx.Channel.Id).TakeWhile(Function(item) Math.Max(Threading.Interlocked.Decrement(amount), amount + 1) <> 0)
        Dim retrieved As New List(Of IMessage)

        For Each item In matching
            Dim msg = Await GetOrDownloadMessageAsync(context, item.ResponseId)
            If msg Is Nothing Then Continue For
            retrieved.Add(msg)
        Next

        Dim currentUser = ctx.Guild.CurrentUser
        Dim perms = currentUser.GetPermissions(ctx.Channel)

        If perms.ManageMessages Then
            Await ctx.Channel.DeleteMessagesAsync(retrieved)
        Else
            For Each message In retrieved
                Await ctx.Channel.DeleteMessageAsync(message)
            Next
        End If

        Dim newQueue As New ConcurrentQueue(Of Message)
        Dim DelIds = retrieved.Select(Function(x) x.Id)

        For Each item In found
            If DelIds.Contains(item.ResponseId) Then Continue For
            newQueue.Enqueue(item)
        Next

        If newQueue.IsEmpty Then _messageCache.TryRemove(ctx.User.Id, Nothing) Else _messageCache(ctx.User.Id) = newQueue

        Return retrieved.AsEnumerable.Count(Function(x) x IsNot Nothing)
    End Function

    Private Function NewItemAsync(userId As ULong, channelId As ULong, createAt As DateTimeOffset, messageId As ULong, responseId As ULong, attachedFile As Boolean) As Task
        _messageCache.TryAdd(userId, New ConcurrentQueue(Of Message))
        Dim found As ConcurrentQueue(Of Message) = Nothing
        If Not _messageCache.TryGetValue(userId, found) Then Return Nothing
        If found.Count > CACHE_SIZE Then found.TryDequeue(Nothing)

        Dim newMessage = New Message With
        {
            .UserId = userId,
            .ChannelId = channelId,
            .CreatedAt = createAt,
            .ExecutingId = messageId,
            .ResponseId = responseId
        }

        found.Enqueue(newMessage)
        _messageCache(userId) = found

        Return Task.CompletedTask
    End Function

    Private Async Function GetExistingMessageAsync(context As ICommandContext) As Task(Of IUserMessage)
        Dim queue As ConcurrentQueue(Of Message) = Nothing
        Dim ctx = DirectCast(context, DerpContext)

        If Not _messageCache.TryGetValue(ctx.User.Id, queue) Then Return Nothing

        Dim found = queue.FirstOrDefault(Function(x) x.ExecutingId = ctx.Message.Id)
        If found Is Nothing Then Return Nothing

        Dim retrievedMessage = Await GetOrDownloadMessageAsync(ctx, found.ResponseId)
        Return TryCast(retrievedMessage, IUserMessage)
    End Function

    Private Shared Function GetOrDownloadMessageAsync(context As ICommandContext, messageId As ULong) As Task(Of IMessage)
        Dim ctx = DirectCast(context, DerpContext)
        Return ctx.Channel.GetMessageAsync(messageId)
    End Function
    Public Function SendFileAsync(context As ICommandContext,
                                  stream As Stream,
                                  Optional content As String = Nothing,
                                  Optional isTTS As Boolean = False,
                                  Optional embed As Embed = Nothing) As Task(Of IUserMessage)

        Dim ctx = DirectCast(context, DerpContext)
        Return SendFileAsync(ctx.User.Id, ctx.Message.Id, ctx.Channel.Id, stream, content, isTTS, embed)
    End Function

    Public Async Function SendFileAsync(userId As ULong,
                                        executingId As ULong,
                                        channelId As ULong,
                                        stream As Stream,
                                        Optional content As String = Nothing,
                                        Optional isTTS As Boolean = False,
                                        Optional embed As Embed = Nothing) As Task(Of IUserMessage)

        Dim channel = TryCast(_client.GetChannel(channelId), SocketTextChannel)
        If channel Is Nothing Then Return Nothing
        Dim response = Await channel.SendFileAsync(stream, "image.png", content, isTTS, embed)
        Await NewItemAsync(userId, channelId, response.CreatedAt, executingId, response.Id, True)
        Return response
    End Function

End Class