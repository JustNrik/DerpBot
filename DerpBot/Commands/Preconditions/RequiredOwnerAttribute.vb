Imports Qmmands

<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method)>
Public Class RequiredOwnerAttribute
    Inherits CheckBaseAttribute

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, DerpContext)
        Return Task.FromResult(If(ctx.User.Id = BOT_OWNER_ID,
                               CheckResult.Successful,
                               CheckResult.Unsuccessful("This command can only be used by bot's owner!")))
    End Function
End Class
