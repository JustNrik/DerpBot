Imports Discord.WebSocket
Imports Microsoft.Extensions.DependencyInjection
Imports SQLExpress
Imports Qmmands

Public Class RequiredSpecialRoleAttribute
    Inherits CheckBaseAttribute

    Private ReadOnly role As SpecialRole

    Public Sub New(role As SpecialRole)
        Me.role = role
    End Sub

    Public Overrides Async Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, IDerpContext)
        Dim dbo = provider.GetService(Of SqlExpressClient)
        Dim guild = Await dbo.LoadObjectAsync(Of Guild)(ctx.Guild.Id)
        Dim hasRole As Boolean

        If ctx.User.Id = BOT_OWNER_ID Then Return CheckResult.Successful

        If guild.Admins.Count = 0 AndAlso guild.Mods.Count = 0 Then Return CheckResult.Unsuccessful("There aren't Admins or Mods added in this server")

        Select Case role
            Case SpecialRole.Admin
                hasRole = guild.Admins.Contains(ctx.User.Id)
            Case SpecialRole.Moderator
                hasRole = guild.Mods.Contains(ctx.User.Id)
            Case Else
                Throw New ArgumentOutOfRangeException()
        End Select

        Return If(hasRole,
            CheckResult.Successful,
            CheckResult.Unsuccessful("You do not have the required permission"))
    End Function
End Class
