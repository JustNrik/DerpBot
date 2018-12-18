Imports Discord
Imports Discord.WebSocket
Imports System.Collections.Concurrent
Imports Qmmands

<Service(ServiceType.Singleton, 0)>
Public Class InteractiveService
    Implements IService

    Private WithEvents _clients As DiscordShardedClient
    Private ReadOnly _callbacks As New ConcurrentDictionary(Of ULong, IReactionCallback)
    Private ReadOnly _defaultTimeout As TimeSpan

    Public Sub New(clients As DiscordShardedClient, Optional DefaultTimeout As TimeSpan? = Nothing)
        _clients = clients
        _defaultTimeout = If(DefaultTimeout, TimeSpan.FromSeconds(15))
    End Sub

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(True)
    End Function

    Public Function NextMessageAsync(context As ICommandContext, Optional fromSourceUser As Boolean = False, Optional isSourceChannel As Boolean = False, Optional timeout As TimeSpan? = Nothing) As Task(Of SocketMessage)
        Dim criteria As New Criteria(Of SocketMessage)
        Dim ctx = DirectCast(context, DerpContext)
        If fromSourceUser Then criteria.Addcriteria(New EnsureSourceUsercriteria)
        If isSourceChannel Then criteria.Addcriteria(New EnsureSourceChannelcriteria)
        Return NextMessageAsync(ctx, criteria, timeout)
    End Function

    Public Async Function NextMessageAsync(context As ICommandContext, criteria As ICriteria(Of SocketMessage), Optional timeout As TimeSpan? = Nothing) As Task(Of SocketMessage)
        timeout = If(timeout, _defaultTimeout)

        Dim eventTrigger As New TaskCompletionSource(Of SocketMessage)
        Dim ctx = DirectCast(context, DerpContext)

        Dim handler = Async Function(message As SocketMessage) As Task
                          Dim result = Await criteria.JudgeAsync(ctx, message).ConfigureAwait(False)
                          If result Then eventTrigger.SetResult(message)
                      End Function

        AddHandler ctx.Shard.MessageReceived, handler

        Dim trigger = eventTrigger.Task
        Dim delay = Task.Delay(timeout.Value)
        Dim localTask = Await Task.WhenAny(trigger, delay).ConfigureAwait(False)

        RemoveHandler ctx.Shard.MessageReceived, handler
        Return If(localTask Is trigger, Await trigger.ConfigureAwait(False), Nothing)
    End Function

    Public Async Function ReplyAndDeleteAsync(context As ICommandContext, content As String, Optional isTTS As Boolean = False, Optional embed As Embed = Nothing, Optional timeout As TimeSpan? = Nothing, Optional options As RequestOptions = Nothing) As Task(Of IUserMessage)
        timeout = If(timeout, _defaultTimeout)
        Dim ctx = DirectCast(context, DerpContext)
        Dim message = Await ctx.Channel.SendMessageAsync(content, isTTS, embed, options)
        Dim __ = Task.Run(Sub() Task.Delay(timeout.Value).ContinueWith(Sub() message.DeleteAsync.ConfigureAwait(False)).ConfigureAwait(False))
        Return message
    End Function

    Sub AddReactionCallback(message As IMessage, callback As IReactionCallback)
        _callbacks.TryAdd(message.Id, callback)
    End Sub

    Public Sub RemoveReactionCallback(message As IMessage)
        RemoveReactionCallback(message.Id)
    End Sub

    Public Sub RemoveReactionCallback(id As ULong)
        _callbacks.TryRemove(id, Nothing)
    End Sub

    Public Sub ClearReactionCallbacks()
        _callbacks.Clear()
    End Sub

    Private Async Function HandleReactionAsync(message As Cacheable(Of IUserMessage, ULong), channel As ISocketMessageChannel, reaction As SocketReaction) As Task Handles _clients.ReactionAdded
        If reaction.UserId = _clients.CurrentUser.Id Then Return

        Dim callback As IReactionCallback = Nothing
        If Not _callbacks.TryGetValue(message.Id, callback) Then Return
        If Not Await callback.Criteria.JudgeAsync(DirectCast(callback.Context, DerpContext), reaction).ConfigureAwait(False) Then Return

        Select Case callback.RunMode
            Case RunMode.Parallel
                Dim __ = Task.Run(action:=Async Sub() If Await callback.HandleCallbackAsync(reaction).ConfigureAwait(False) Then RemoveReactionCallback(message.Id))
            Case Else
                If Await callback.HandleCallbackAsync(reaction).ConfigureAwait(False) Then RemoveReactionCallback(message.Id)
        End Select
    End Function
End Class