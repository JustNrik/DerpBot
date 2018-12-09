Imports Discord
Imports Discord.WebSocket
Imports Qmmands

Public Class PaginatedMessageCallback
    Implements IReactionCallback, ICallback

    Private ReadOnly _pager As PaginatedMessage
    Private ReadOnly _pages As Integer
    Private ReadOnly _interactive As InteractiveService
    Private _page As Integer = 1

    Private ReadOnly Property Options As PaginatedAppearanceOptions
        Get
            Return _pager.Options
        End Get
    End Property

    Public Property Message As IUserMessage Implements ICallback.Message
    Public ReadOnly Property Context As ICommandContext Implements IReactionCallback.Context

    Public ReadOnly Property RunMode As RunMode Implements IReactionCallback.RunMode
        Get
            Return RunMode.Sequential
        End Get
    End Property

    Public ReadOnly Property Criteria As ICriteria(Of SocketReaction) Implements IReactionCallback.Criteria

    Public ReadOnly Property Timeout As TimeSpan? Implements IReactionCallback.Timeout
        Get
            Return TimeSpan.FromMinutes(2)
        End Get
    End Property

    Private ReadOnly _derpContext As DerpContext

    Public Sub New(interactive As InteractiveService, sourceContext As ICommandContext, pager As PaginatedMessage, Optional Criteria As ICriteria(Of SocketReaction) = Nothing)
        _interactive = interactive
        _Context = sourceContext
        Me.Criteria = If(Criteria, New EmptyCriteria(Of SocketReaction)())
        _pager = pager
        _pages = pager.Pages.Count()
        _derpContext = DirectCast(sourceContext, DerpContext)
    End Sub

    Public Async Function DisplayAsync() As Task Implements ICallback.DisplayAsync
        Dim message = Await _derpContext.Channel.SendMessageAsync(_pager.Content,, BuildEmbed()).ConfigureAwait(False)
        message = message
        _interactive.AddReactionCallback(message, Me)
        Dim derp = Task.Run(Async Function()
                                Await message.AddReactionAsync(Options.First)
                                Await message.AddReactionAsync(Options.Back)
                                Await message.AddReactionAsync(Options.Next)
                                Await message.AddReactionAsync(Options.Last)
                                Dim manageMessages = _derpContext.User.GetPermissions(_derpContext.Channel).ManageMessages
                                If Options.JumpDisplayOptions = JumpDisplayOptions.Always OrElse
                                   Options.JumpDisplayOptions = JumpDisplayOptions.WithManageMessages AndAlso manageMessages Then Await message.AddReactionAsync(Options.Jump)
                                Await message.AddReactionAsync(Options.Stop)
                                If Options.DisplayInformationIcon Then Await message.AddReactionAsync(Options.Info)
                            End Function)
        If Timeout.HasValue AndAlso Timeout.Value <> Nothing Then _
            Dim __ = Task.Run(Sub() Task.Delay(Timeout.Value).
            ContinueWith(continuationAction:=Async Sub()
                                                 _interactive.RemoveReactionCallback(message)
                                                 Await message.DeleteAsync()
                                             End Sub))
    End Function

    Public Async Function HandleCallbackAsync(reaction As SocketReaction) As Task(Of Boolean) Implements IReactionCallback.HandleCallbackAsync
        Dim emote = reaction.Emote

        If emote.Equals(Options.First) Then
            _page = 1
        ElseIf emote.Equals(Options.Next) Then

            If _page >= _pages Then
                _page = 1
                Return False
            End If

            _page += 1
        ElseIf emote.Equals(Options.Back) Then

            If _page <= 1 Then
                _page = _pages
                Return False
            End If

            _page -= 1
        ElseIf emote.Equals(Options.Last) Then
            _page = _pages
        ElseIf emote.Equals(Options.Stop) Then
            Await Message.DeleteAsync().ConfigureAwait(False)
            Return True
        ElseIf emote.Equals(Options.Jump) Then
            Dim derpx = Task.Run(Async Function()
                                     Dim criteria = New Criteria(Of SocketMessage)().
                                         AddCriteria(New EnsureSourceChannelCriteria()).
                                         AddCriteria(New EnsureFromUserCriteria(reaction.UserId)).
                                         AddCriteria(New EnsureIsIntegerCriteria())

                                     Dim response = Await _interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15))
                                     Dim request = Integer.Parse(response.Content)

                                     If request < 1 OrElse request > _pages Then
                                         Await response.DeleteAsync().ConfigureAwait(False)
                                         Await _interactive.ReplyAndDeleteAsync(Context, Options.[Stop].Name)
                                         Return
                                     End If

                                     _page = request
                                     Await response.DeleteAsync().ConfigureAwait(False)
                                     Await RenderAsync().ConfigureAwait(False)
                                 End Function)
        ElseIf emote.Equals(Options.Info) Then
            Await _interactive.ReplyAndDeleteAsync(Context, PaginatedAppearanceOptions.InformationText, timeout:=Options.InfoTimeout)
            Return False
        End If

        Await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value)
        Await RenderAsync().ConfigureAwait(False)
        Return False
    End Function

    Protected Function BuildEmbed() As Embed
        Return New EmbedBuilder().
            WithAuthor(_pager.Author).
            WithColor(_pager.Color).
            WithDescription(_pager.Pages.ElementAt(_page - 1).ToString).
            WithFooter(Sub(f) f.Text = String.Format(PaginatedAppearanceOptions.FooterFormat, _page, _pages)).
            WithTitle(_pager.Title).
            Build()
    End Function

    Private Async Function RenderAsync() As Task
        Await Message.ModifyAsync(Sub(m) m.Embed = BuildEmbed()).ConfigureAwait(False)
    End Function
End Class
