Imports Discord
Imports Discord.WebSocket
Imports SQLExpress

<Service(ServiceType.Singleton, 0)>
Public Class StarboardService
    Implements IService

    Private ReadOnly _database As SQLExpressClient
    Private ReadOnly _message As MessageService
    Private WithEvents _client As DiscordShardedClient

    Sub New(database As SQLExpressClient, message As MessageService, client As DiscordShardedClient)
        _database = database
        _message = message
        _client = client
    End Sub

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(False)
    End Function

    Async Function OnReactionAdded(__ As Cacheable(Of IUserMessage, ULong), channel As ISocketMessageChannel, reaction As SocketReaction) As Task _
        'Handles _client.ReactionAdded, _client.ReactionRemoved

        If Not reaction.Emote.Equals(EmotesDict("star")) Then Return

        Dim guildChannel = TryCast(channel, IGuildChannel)
        If guildChannel Is Nothing Then Return

        Dim author = reaction.Message.Value.Author
        If author.IsBot OrElse author.IsWebhook Then Return

        Dim guild = Await _database.LoadObjectAsync(Of Guild)(guildChannel.GuildId)
        If Not (guild.Starboard.HasValue OrElse reaction.Message.IsSpecified) Then Return

        Dim starCount = reaction.Message.Value.Reactions.Where(Function(x) x.Key.Equals(EmotesDict("star"))).Count
        If starCount < guild.StarCount Then Return

        Dim starChannel = Await guildChannel.Guild.GetTextChannelAsync(guild.Starboard.Value)
        Dim embedBuilder = GetBuilder(reaction.Message.Value, starCount)
        Dim message As Message
        If Await _database.CheckExistenceAsync(Of Message)(reaction.MessageId) Then
            message = Await _database.LoadObjectAsync(Of Message)(reaction.MessageId)
            Dim msg = Await channel.GetMessageAsync(message.Id)
        Else
            message = New Message With
            {
                .ChannelId = channel.Id,
                .CreatedAt = reaction.Message.Value.CreatedAt,
                .Id = reaction.MessageId,
                .UserId = reaction.Message.Value.Author.Id
            }
            Await _database.CreateNewObjectAsync(message)
        End If
        ' *brain fart* I will finish this later, damn
    End Function

    Function GetBuilder(msg As SocketUserMessage, starCount As Integer) As EmbedBuilder
        Dim GB = If(229 + starCount <= 255, 229 + starCount, 255)
        Return New EmbedBuilder().
            AddSignature(msg.Author).
            AddField("Content", Format.Sanitize(msg.Content)).
            WithUrl(msg.GetJumpUrl).
            WithColor(New Color(0, 227 + starCount, 1)).
            WithTitle($"{EmotesDict("star")} : {starCount} - ID: {msg.Id}")
    End Function
End Class
