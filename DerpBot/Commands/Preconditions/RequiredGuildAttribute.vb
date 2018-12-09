Imports Qmmands

Public Class RequiredGuildAttribute
    Inherits CheckBaseAttribute

    Private ReadOnly _guildId As ULong

    Public Sub New(guildId As ULong)
        _guildId = guildId
    End Sub

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, IDerpContext)
        Return If(ctx.Guild.Id = _guildId,
            Task.FromResult(CheckResult.Successful),
            Task.FromResult(CheckResult.Unsuccessful("Unknown Command")))
    End Function
End Class
