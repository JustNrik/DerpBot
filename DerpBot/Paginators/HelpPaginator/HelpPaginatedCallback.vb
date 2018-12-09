Imports Discord
Imports Discord.WebSocket
Imports Qmmands

Public Class HelpPaginatedCallback
    Implements IReactionCallback, ICallback

    Private ReadOnly Pager As HelpPaginatedMessage
    Private ReadOnly Interactive As InteractiveService
    Private ReadOnly Pages As Integer
    Private Page As Integer = 1

    Private ReadOnly Property Options As PaginatedAppearanceOptions
        Get
            Return Pager.Options
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
        Get
            Return New EnsureReactionFromSourceUserCriteria
        End Get
    End Property

    Public ReadOnly Property Timeout As TimeSpan? Implements IReactionCallback.Timeout
        Get
            Return TimeSpan.FromMinutes(2)
        End Get
    End Property

    Private ReadOnly _derpContext As DerpContext

    Public Sub New(interactive As InteractiveService, sourceContext As ICommandContext, pager As HelpPaginatedMessage)
        Me.Interactive = interactive
        Me.Pager = pager
        Context = sourceContext
        Pages = pager.Pages.Count
        _derpContext = DirectCast(sourceContext, DerpContext)
    End Sub

    Public Async Function DisplayAsync() As Task Implements ICallback.DisplayAsync
        Dim embed = BuildEmbed()
        Dim message = Await _derpContext.Channel.SendMessageAsync(,, embed).ConfigureAwait(False)
        Me.Message = message
        Interactive.AddReactionCallback(message, Me)
        Dim __ = Task.Run(action:=Async Sub()
                                      Await message.AddReactionAsync(Options.First)
                                      Await message.AddReactionAsync(Options.Back)
                                      Await message.AddReactionAsync(Options.Next)
                                      Await message.AddReactionAsync(Options.Last)
                                      Dim guildChannel = TryCast(_derpContext.Channel, IGuildChannel)
                                      Dim manageMessages = If(guildChannel IsNot Nothing, (TryCast(_derpContext.User, IGuildUser)).GetPermissions(guildChannel).ManageMessages, False)
                                      If Options.JumpDisplayOptions = JumpDisplayOptions.Always OrElse
                                           Options.JumpDisplayOptions = JumpDisplayOptions.WithManageMessages AndAlso manageMessages Then Await message.AddReactionAsync(Options.Jump)
                                      Await message.AddReactionAsync(Options.Stop)
                                      If Options.DisplayInformationIcon Then Await message.AddReactionAsync(Options.Info)
                                  End Sub)
        If Timeout.HasValue AndAlso Timeout.Value <> Nothing Then __ = Task.Run(Sub() Task.Delay(Timeout.Value).ContinueWith(Async Function(x)
                                                                                                                                 Interactive.RemoveReactionCallback(message)
                                                                                                                                 Await Me.Message.DeleteAsync()
                                                                                                                             End Function))
    End Function

    Public Async Function HandleCallbackAsync(reaction As SocketReaction) As Task(Of Boolean) Implements IReactionCallback.HandleCallbackAsync
        Dim emote = reaction.Emote

        If emote.Equals(Options.First) Then
            Page = 1
        ElseIf emote.Equals(Options.[Next]) Then
            If Page >= Pages Then Return False
            Page += 1
        ElseIf emote.Equals(Options.Back) Then
            If Page <= 1 Then Return False
            Page -= 1
        ElseIf emote.Equals(Options.Last) Then
            Page = Pages
        ElseIf emote.Equals(Options.Stop) Then
            Await Message.DeleteAsync().ConfigureAwait(False)
            Return True
        ElseIf emote.Equals(Options.Jump) Then
            Dim derp = Task.Run(action:=Async Sub()
                                            Dim criteria = New Criteria(Of SocketMessage)().
                                    AddCriteria(New EnsureSourceChannelCriteria()).
                                    AddCriteria(New EnsureFromUserCriteria(reaction.UserId)).
                                    AddCriteria(New EnsureIsIntegerCriteria())
                                            Dim response = Await Interactive.NextMessageAsync(_derpContext, criteria, TimeSpan.FromSeconds(15))
                                            Dim request = Integer.Parse(response.Content)

                                            If request < 1 OrElse request > Pages Then
                                                Await response.DeleteAsync().ConfigureAwait(False)
                                                Await Interactive.ReplyAndDeleteAsync(_derpContext, Options.Stop.Name)
                                                Return
                                            End If

                                            Page = request
                                            Await response.DeleteAsync().ConfigureAwait(False)
                                            Await RenderAsync().ConfigureAwait(False)
                                        End Sub)
        ElseIf emote.Equals(Options.Info) Then
            Await Interactive.ReplyAndDeleteAsync(_derpContext, PaginatedAppearanceOptions.InformationText, timeout:=Options.InfoTimeout)
            Return False
        End If

        Await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value)
        Await RenderAsync().ConfigureAwait(False)
        Return False
    End Function

    Protected Function BuildEmbed() As Embed
        Return New EmbedBuilder().
            WithAuthor(Pager.Author).
            WithColor(Color.Gold).
            WithDescription($"Type {Pager.Prefix}help CommandName to view more help for that command!{vbLf}" & $"e.g. {Pager.Prefix}help create tag").
            AddField(GetPage(Pager.Pages, Page - 1).Title).
            AddEmptyField.AddFields(GetPage(Pager.Pages, Page - 1).Fields).
            WithFooter($"Page: {Page}/{Pages}").Build()
    End Function

    Private Function GetPage(pages As IEnumerable(Of Page), index As Integer) As Page
        Return pages.ElementAt(index)
    End Function

    Private Async Function RenderAsync() As Task
        Dim e = BuildEmbed()
        Await Message.ModifyAsync(Sub(m) m.Embed = e).ConfigureAwait(False)
    End Function
End Class