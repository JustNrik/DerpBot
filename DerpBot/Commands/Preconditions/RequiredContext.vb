Imports Qmmands

Public Class RequiredContextAttribute
    Inherits CheckBaseAttribute

    Private ReadOnly _contextType As ContextType

    Sub New(contextType As ContextType)
        _contextType = contextType
    End Sub

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = TryCast(context, IDerpContext)
        Dim ctxType = If(ctx.Guild IsNot Nothing, ContextType.Guild, ContextType.DM)
        Return If(_contextType = ctxType,
            Task.FromResult(CheckResult.Successful),
            Task.FromResult(CheckResult.Unsuccessful("Bot commands are enabled in guilds only")))
    End Function
End Class
