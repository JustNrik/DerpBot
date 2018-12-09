Imports Qmmands

Public Class RequireGuildAttribute
    Inherits CheckBaseAttribute

    Private ReadOnly _guildId As ULong

    Public Sub New(guildId As ULong)
        _guildId = guildId
    End Sub

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Return If(DirectCast(context, DerpContext).Guild.Id = _guildId,
            Task.FromResult(CheckResult.Successful),
            Task.FromResult(CheckResult.Unsuccessful("This command cannot be executed in this guild")))
    End Function
End Class
