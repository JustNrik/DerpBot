Imports Discord.Commands

Public Class RequireGuildAttribute
    Inherits PreconditionAttribute

    Private ReadOnly _guildId As ULong

    Public Sub New(guildId As ULong)
        _guildId = guildId
    End Sub

    Public Overrides Function CheckPermissionsAsync(context As ICommandContext, command As CommandInfo, services As IServiceProvider) As Task(Of PreconditionResult)
        Return If(context.Guild.Id = _guildId,
            Task.FromResult(PreconditionResult.FromSuccess()),
            Task.FromResult(PreconditionResult.FromError(PreconditionResult.FromError(New FailedResult("Unknown Command", False, CommandError.UnknownCommand)))))
    End Function
End Class
