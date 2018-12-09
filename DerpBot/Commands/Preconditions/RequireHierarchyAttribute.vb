Imports Discord.Commands
Imports Discord.WebSocket

Public Class RequireHierarchyAttribute
    Inherits ParameterPreconditionAttribute

    Public Overrides Function CheckPermissionsAsync(context As ICommandContext, parameter As ParameterInfo, value As Object, services As IServiceProvider) As Task(Of PreconditionResult)
        Dim currentUser = TryCast(context, SocketCommandContext)?.Guild.CurrentUser
        Dim guildUser = TryCast(value, SocketGuildUser)

        If guildUser IsNot Nothing Then Return Task.FromResult(If(currentUser?.Hierarchy > guildUser.Hierarchy,
                                                               PreconditionResult.FromSuccess(),
                                                               PreconditionResult.FromError("You don't have hierarchy over this user")))

        Return Task.FromResult(PreconditionResult.FromError("This error shouldn't exist"))
    End Function
End Class
