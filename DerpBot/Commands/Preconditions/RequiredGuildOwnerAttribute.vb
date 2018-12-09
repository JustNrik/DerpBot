Imports Qmmands

Public Class RequiredGuildOwnerAttribute
    Inherits CheckBaseAttribute

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, IDerpContext)
        Return If(ctx.User.Id = ctx.Guild.OwnerId OrElse ctx.User.Id = BOT_OWNER_ID,
            Task.FromResult(CheckResult.Successful),
            Task.FromResult(CheckResult.Unsuccessful("Only the guild owner can execute this commands")))
    End Function
End Class
