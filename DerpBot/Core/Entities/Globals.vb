Imports Discord
Imports Discord.WebSocket
Imports Microsoft.Extensions.DependencyInjection
Imports SQLExpress

Public Class Globals
    Public Property Context As DerpContext
    Public Property User As SocketUser
    Public Property Guild As SocketGuild
    Public Property Channel As ISocketMessageChannel
    Public Property Channels As IReadOnlyCollection(Of SocketGuildChannel)
    Public Property Roles As IReadOnlyCollection(Of SocketRole)
    Public Property Users As IReadOnlyCollection(Of SocketGuildUser)
    Public Property Provider As IServiceProvider
    Public Property Database As SQLExpressClient

    Public Function ReplyAsync(Optional text As String = Nothing, Optional isTTS As Boolean = False, Optional embed As Embed = Nothing, Optional options As RequestOptions = Nothing) As Task
        Return Task.FromResult(Channel?.SendMessageAsync(text, isTTS, embed, options))
    End Function

    Public Function SendEmbedAsync(embed As Embed) As Task
        Return Task.FromResult(ReplyAsync(,, embed))
    End Function

    Sub New(context As DerpContext, provider As IServiceProvider)
        _Context = context
        _User = context.User
        _Channel = context.Channel
        _Guild = context.Guild
        _Roles = Guild.Roles
        _Users = Guild.Users
        _Channels = Guild.Channels
        _Provider = provider
        _Database = provider.GetService(Of SQLExpressClient)
    End Sub
End Class
