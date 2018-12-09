Imports Discord
Imports Qmmands

<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method)>
Public Class RequiredBotPermissionAttribute
    Inherits CheckBaseAttribute

    Private ReadOnly ChannelPermissions As ChannelPermission()
    Private ReadOnly GuildPermissions As GuildPermission()

    Public Sub New(ParamArray permissions As ChannelPermission())
        ChannelPermissions = permissions
    End Sub

    Public Sub New(ParamArray permissions As GuildPermission())
        GuildPermissions = permissions
    End Sub

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, DerpContext)
        If ChannelPermissions.Length > 0 AndAlso ctx.User.GetPermissions(ctx.Channel).ToList().Any(Function(x) ChannelPermissions.Contains(x)) OrElse
            GuildPermissions.Length > 0 AndAlso ctx.User.GuildPermissions.ToList().Any(Function(x) GuildPermissions.Contains(x)) Then

            Return Task.FromResult(CheckResult.Successful)
        Else
            Return Task.FromResult(CheckResult.Unsuccessful("I don't have enough permissions to execute that command."))
        End If
    End Function
End Class
