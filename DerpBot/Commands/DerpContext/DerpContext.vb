Imports Discord
Imports Discord.WebSocket

Public Class DerpContext
    Implements IDerpContext

#Region "Explicit Interface Implementation"
    Public ReadOnly Property IClient As IDiscordClient Implements IDerpContext.Client
        Get
            Return Client
        End Get
    End Property

    Public ReadOnly Property IGuild As IGuild Implements IDerpContext.Guild
        Get
            Return Guild
        End Get
    End Property

    Public ReadOnly Property IChannel As IMessageChannel Implements IDerpContext.Channel
        Get
            Return Channel
        End Get
    End Property

    Public ReadOnly Property IUser As IUser Implements IDerpContext.User
        Get
            Return User
        End Get
    End Property
#End Region
    Public ReadOnly Property Client As DiscordShardedClient
    Public ReadOnly Property Shard As DiscordSocketClient
    Public ReadOnly Property Guild As SocketGuild
    Public ReadOnly Property User As SocketGuildUser
    Public ReadOnly Property Channel As SocketTextChannel
    Public ReadOnly Property Message As IUserMessage Implements IDerpContext.Message

    Sub New(client As DiscordShardedClient, message As IUserMessage)
        _Client = client
        _Message = message
        _Channel = TryCast(message.Channel, SocketTextChannel)
        _User = DirectCast(message.Author, SocketGuildUser)
        _Guild = Channel?.Guild
        _Shard = client.GetShardFor(Guild)
    End Sub

End Class
