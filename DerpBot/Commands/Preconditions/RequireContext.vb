Imports Discord.Commands

Public Class RequireContextAttribute
    Inherits PreconditionAttribute

    Private ReadOnly _contextType As ContextType

    Sub New(contextType As ContextType)
        _contextType = contextType
    End Sub

    Public Overrides Function CheckPermissionsAsync(context As ICommandContext, command As CommandInfo, services As IServiceProvider) As Task(Of PreconditionResult)
        Return If(_contextType = ContextType.Guild,
            Task.FromResult(PreconditionResult.FromSuccess()),
            Task.FromResult(PreconditionResult.FromError("Bot commands are enabled in guilds only")))
    End Function
End Class
