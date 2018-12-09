Imports Discord
Imports Qmmands

Public Interface IDerpContext
    Inherits ICommandContext

    ReadOnly Property Client As IDiscordClient
    ReadOnly Property Message As IUserMessage
    ReadOnly Property Guild As IGuild
    ReadOnly Property Channel As IMessageChannel
    ReadOnly Property User As IUser

End Interface
