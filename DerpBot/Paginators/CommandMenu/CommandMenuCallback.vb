Option Compare Text
Imports Discord
Imports Discord.WebSocket
Imports System.Text
Imports Qmmands
Imports Qmmands.CommandUtilities

Public Class CommandMenuCallback
    Implements IReactionCallback, ICallback

    Private ReadOnly _commands As CommandService
    Private ReadOnly _services As IServiceProvider
    Private ReadOnly _interactive As InteractiveService
    Private ReadOnly _properties As CommandMenuMessage
    Private ReadOnly _messageService As MessageService
    Private _currentMenu As String
    Private _executing As Boolean
    Private _isMain As Boolean = True
    Private _selectedIndex As Integer
    Public Property Message As IUserMessage Implements ICallback.Message

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

    Public ReadOnly Property Context As ICommandContext Implements IReactionCallback.Context
    Private ReadOnly _derpContext As DerpContext

    Public Sub New(commands As CommandService, services As IServiceProvider, interactive As InteractiveService, properties As CommandMenuMessage, message As MessageService, context As ICommandContext)
        _commands = commands
        _services = services
        _interactive = interactive
        _properties = properties
        _messageService = message
        _Context = context
        _derpContext = DirectCast(_Context, DerpContext)
    End Sub

    Public Async Function DisplayAsync() As Task Implements ICallback.DisplayAsync
        Dim message = Await _derpContext.Channel.SendMessageAsync(String.Empty,, BuildEmbed)
        message = message
        _interactive.AddReactionCallback(message, Me)
        Await Task.Run(action:=Async Sub() Await message.AddReactionsAsync(_properties.Emojis.Values))
        If Timeout.HasValue AndAlso Timeout.Value <> Nothing Then Await Task.Run(action:=Async Sub() Await Task.Delay(Timeout.Value).
                                                                                             ContinueWith(continuationAction:=Async Sub()
                                                                                                                                  _interactive.RemoveReactionCallback(message)
                                                                                                                                  Await message.DeleteAsync()
                                                                                                                              End Sub)).ConfigureAwait(False)
    End Function

    Public Async Function HandleCallbackAsync(reaction As SocketReaction) As Task(Of Boolean) Implements IReactionCallback.HandleCallbackAsync
        Dim emote = reaction.Emote
        Dim emotes = _properties.Emojis
        Dim count = If(_isMain,
            _properties.CommandsDictionary.Keys.Count,
            _properties.CommandsDictionary.FirstOrDefault(Function(x) x.Key.Name = _currentMenu).Value.Count)

        If _executing Then Return False

        Select Case True
            Case emote.Equals(emotes("up")) : _selectedIndex = If(_selectedIndex = 0, count - 1, _selectedIndex - 1)
            Case emote.Equals(emotes("down")) : _selectedIndex = If(_selectedIndex = count - 1, 0, _selectedIndex + 1)
            Case emote.Equals(emotes("back"))
                _isMain = True
                _selectedIndex = 0
            Case emote.Equals(emotes("select"))
                If _isMain Then
                    _isMain = False
                    _currentMenu = _properties.CommandsDictionary.Keys.ElementAt(_selectedIndex).Name
                    _selectedIndex = 0
                Else
                    _executing = True
                    Dim selectedCommand = _properties.CommandsDictionary.FirstOrDefault(Function(x) x.Key.Name = _currentMenu).Value.ElementAt(_selectedIndex)
                    Await Task.Run(action:=Async Sub()
                                               Dim paramValues As New StringBuilder()
                                               Dim execute = True
                                               For Each param In selectedCommand.Parameters
                                                   Dim criteria = New Criteria(Of SocketMessage)().
                                                        AddCriteria(New EnsureSourceChannelCriteria).
                                                        AddCriteria(New EnsureFromUserCriteria(reaction.UserId))
                                                   Await _messageService.SendMessageAsync(_derpContext, $"What do you want the {param.Name} to be? Respond with `cancel` to cancel execution")
                                                   Dim response = Await _interactive.NextMessageAsync(_derpContext, criteria, TimeSpan.FromSeconds(15))
                                                   If response Is Nothing OrElse response.Content = "cancel" Then
                                                       execute = False
                                                       Exit For
                                                   End If
                                                   paramValues.Append(" " + response.Content)
                                               Next

                                               If execute Then Await _commands.ExecuteAsync(paramValues.ToString(), _derpContext, _services)

                                               _executing = False
                                           End Sub)
                End If
            Case emote.Equals(emotes("delete"))
                Await Message.DeleteAsync
                Return False
        End Select

        Await Message.RemoveReactionAsync(emote, reaction.User.Value)
        Await Message.ModifyAsync(Sub(x) x.Embed = BuildEmbed)

        Return False
    End Function

    Private Function BuildEmbed() As Embed
        Dim embed = New EmbedBuilder With {
            .Author = New EmbedAuthorBuilder With {
            .IconUrl = _derpContext.User.GetAvatarOrDefaultUrl(),
            .Name = _derpContext.User.GetDisplayName()},
        .Color = Color.DarkTeal,
        .Title = "Umbreon's Command Menu",
        .Description = "A menu to help you navigate and use commands",
        .Timestamp = DateTimeOffset.UtcNow,
        .ThumbnailUrl = _derpContext.Client.CurrentUser.GetAvatarOrDefaultUrl()}
        embed.AddEmptyField()
        Dim i = 0
        Dim builder = New StringBuilder()

        If _isMain Then

            For Each [module] In _properties.CommandsDictionary.Keys
                builder.AppendLine(If(Math.Min(Threading.Interlocked.Increment(i), i - 1) = _selectedIndex, $"**>{If(ULong.TryParse([module].Name, Nothing),
                                   _derpContext.Guild.Name, [module].Name)}**", If(ULong.TryParse([module].Name, Nothing), _derpContext.Guild.Name, [module].Name)))
            Next

            embed.AddField("Modules", builder.ToString())
        Else
            Dim commands = _properties.CommandsDictionary.FirstOrDefault(Function(x) x.Key.Name = _currentMenu).Value

            For Each cmd In commands
                builder.AppendLine(If(Math.Min(Threading.Interlocked.Increment(i), i - 1) = _selectedIndex, $"**>{cmd.Name}**", cmd.Name))
            Next

            embed.AddField($"{(If(ULong.TryParse(_currentMenu, Nothing), _derpContext.Guild.Name, _currentMenu))} Commands", builder.ToString())
        End If

        Return embed.Build()
    End Function

End Class
