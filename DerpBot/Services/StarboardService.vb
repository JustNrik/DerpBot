Imports System.Threading
Imports Discord
Imports Discord.WebSocket
Imports SqlExpress

<Service(ServiceType.Singleton, 0)>
Public Class StarboardService
    Implements IService

    Private ReadOnly _database As SqlExpressClient
    Private ReadOnly _message As MessageService
    Private ReadOnly _semaphore As New SemaphoreSlim(1, 1)
    Private WithEvents _client As DiscordShardedClient

    Sub New(database As SqlExpressClient, message As MessageService, client As DiscordShardedClient)
        _database = database
        _message = message
        _client = client
    End Sub

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(False)
    End Function

    Async Function OnReactionAddedRemoved(msg As Cacheable(Of IUserMessage, ULong), channel As ISocketMessageChannel, reaction As SocketReaction) As Task _
        Handles _client.ReactionAdded, _client.ReactionRemoved

        If TypeOf reaction.Emote IsNot Emoji Then Return
        If Not reaction.Emote.Equals(EmotesDict("star")) Then Return

        Dim chn = TryCast(channel, ITextChannel)
        If chn Is Nothing Then Return

        Dim guild = Await _database.LoadObjectAsync(Of Guild)(chn.GuildId)
        If Not guild.Starboard.HasValue Then Return

        Await _semaphore.WaitAsync()

        Dim message = Await msg.GetOrDownloadAsync()
        Dim users = Await message.GetReactionUsersAsync(reaction.Emote, 1000).FlattenAsync()
        Dim validUsers = users.Count(Function(user) Not (user.IsBot OrElse user.IsWebhook OrElse user.Id = message.Author.Id))

        If validUsers >= guild.StarCount Then

            If guild.StarredMessages.Any(Function(m) m.Id = message.Id) Then

                Dim builder = GetBuilder(message, validUsers)
                Dim starMsgId = (Await _database.LoadObjectAsync(Of Message)(message.Id)).ExecutingId
                Dim starMsg = Await chn.GetMessageAsync(starMsgId)
                Await DirectCast(starMsg, IUserMessage).ModifyAsync(Sub(x) x.Embed = builder.Build())

            Else

                Dim builder = GetBuilder(message, validUsers)
                Dim starboard = Await chn.Guild.GetTextChannelAsync(guild.Starboard.Value)
                Dim newMsg = Await starboard.SendMessageAsync(,, builder.Build())
                Dim msgObj = New Message() With
                {
                    .Id = message.Id,
                    .ChannelId = message.Channel.Id,
                    .CreatedAt = message.CreatedAt,
                    .AttachedFile = message.Attachments.Any(),
                    .ExecutingId = newMsg.Id,
                    .UserId = message.Author.Id
                }
                guild.StarredMessages.Add(msgObj)
                Await _database.CreateObjectAsync(msgObj)

            End If

        ElseIf validUsers < guild.StarCount AndAlso guild.StarredMessages.Any(Function(m) m.Id = message.Id) Then

            Dim starboard = Await chn.Guild.GetTextChannelAsync(guild.Starboard.Value)
            Dim starMsgObj = Await _database.LoadObjectAsync(Of Message)(message.Id)
            Dim starMsg = Await starboard.GetMessageAsync(starMsgObj.ExecutingId)
            Await starMsg.DeleteAsync()
            guild.StarredMessages.Remove(starMsgObj)
            Await _database.UpdateObjectAsync(guild)
            Await _database.RemoveObjectAsync(starMsgObj)

        End If

        _semaphore.Release()
    End Function

    Function GetBuilder(msg As IUserMessage, starCount As Integer) As EmbedBuilder
        Dim GB = If(229 + starCount <= 255, 229 + starCount, 255)
        Return New EmbedBuilder().
            AddField("Content", Format.Sanitize(msg.Content)).
            WithUrl(msg.GetJumpUrl()).
            WithColor(New Color(0, GB, GB)).
            WithTitle($"{EmotesDict("star")} : {starCount} - ID: {msg.Id}")
    End Function
End Class
