Imports Discord.Commands

Public Class RequireGuildOwnerAttribute
    Inherits PreconditionAttribute

    Public Overrides Function CheckPermissionsAsync(context As ICommandContext, command As CommandInfo, services As IServiceProvider) As Task(Of PreconditionResult)
        Return If(context.User.Id = context.Guild.OwnerId,
            Task.FromResult(PreconditionResult.FromSuccess()),
            Task.FromResult(PreconditionResult.FromError("Only the guild owner can execute this commands")))
    End Function
End Class
