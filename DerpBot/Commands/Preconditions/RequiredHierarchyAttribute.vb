Imports Discord.WebSocket
Imports Qmmands

Public Class RequiredHierarchyAttribute
    Inherits CheckBaseAttribute

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, DerpContext)
        Dim guildUser = TryCast(ctx.User, SocketGuildUser)

        If guildUser IsNot Nothing Then Return Task.FromResult(If(ctx.Guild.CurrentUser.Hierarchy > guildUser.Hierarchy,
                                                               CheckResult.Successful,
                                                               CheckResult.Unsuccessful("You don't have hierarchy over this user")))

        Return Task.FromResult(CheckResult.Unsuccessful("This error shouldn't exist"))
    End Function
End Class
