Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.Extensions.DependencyInjection
Imports SQLExpress

Public Class RequireRoleAttribute
    Inherits PreconditionAttribute

    Private ReadOnly role As SpecialRole

    Public Sub New(role As SpecialRole)
        Me.role = role
    End Sub

    Public Overrides Function CheckPermissionsAsync(context As ICommandContext, command As CommandInfo, services As IServiceProvider) As Task(Of PreconditionResult)
        If context.User.Id = context.Guild.OwnerId Then Return Task.FromResult(PreconditionResult.FromSuccess())

        Dim database = services.GetService(Of SQLExpressClient)
        Dim guild = database.LoadObject(Of Guild)(context.Guild.Id)
        Dim roleId As ULong

        Select Case role
            Case SpecialRole.Admin
                roleId = If(guild.AdminRole, 0UL)
            Case SpecialRole.Moderator
                roleId = If(guild.ModRole, 0UL)
            Case Else
                Throw New ArgumentOutOfRangeException()
        End Select

        If roleId = 0 OrElse Not context.Guild.Roles.Select(Function(x) x.Id).Contains(roleId) Then _
            Return Task.FromResult(PreconditionResult.FromError($"{role} role not found. Please do `{guild.Prefixes.First()}set {role}Role` to setup this role"))
        Dim user = TryCast(context.User, SocketGuildUser)
        Return If(user.HasRole(roleId),
            Task.FromResult(PreconditionResult.FromSuccess()),
            Task.FromResult(PreconditionResult.FromError("You do not have the required role")))
    End Function
End Class
