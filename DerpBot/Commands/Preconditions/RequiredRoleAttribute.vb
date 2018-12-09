Imports Discord.WebSocket
Imports Microsoft.Extensions.DependencyInjection
Imports SQLExpress
Imports Qmmands

Public Class RequiredRoleAttribute
    Inherits CheckBaseAttribute

    Private ReadOnly role As SpecialRole

    Public Sub New(role As SpecialRole)
        Me.role = role
    End Sub

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, IDerpContext)
        Dim dbo = provider.GetService(Of SQLExpressClient)
        Dim guild = dbo.LoadObject(Of Guild)(ctx.Guild.Id)
        Dim roleId As ULong

        If ctx.User.Id = BOT_OWNER_ID Then Return Task.FromResult(CheckResult.Successful)

        Select Case role
            Case SpecialRole.Admin
                roleId = If(guild.AdminRole, 0UL)
            Case SpecialRole.Moderator
                roleId = If(guild.ModRole, 0UL)
            Case Else
                Throw New ArgumentOutOfRangeException()
        End Select

        If roleId = 0 OrElse Not ctx.Guild.Roles.Any(Function(x) x.Id = roleId) Then _
            Return Task.FromResult(CheckResult.Unsuccessful($"{role} role not found. Please do `{guild.Prefixes.First()}set {role} Role` to setup this role"))
        Dim user = DirectCast(ctx.User, SocketGuildUser)
        Return If(user.HasRole(roleId),
            Task.FromResult(CheckResult.Successful),
            Task.FromResult(CheckResult.Unsuccessful("You do not have the required role")))
    End Function
End Class
