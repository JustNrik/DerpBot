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
