Imports Discord
Imports Humanizer
Imports SQLExpress
Imports Qmmands

<RequiredRole(SpecialRole.Admin Or SpecialRole.Moderator)>
<RequiredContext(ContextType.Guild)>
Public Class ModerationCommands
    Inherits DerpBase(Of DerpContext)

    <Command("mute", "m")>
    <Description("Removes all permissions (except view channel) from the user. He won't be able to send messages, react or anything else.")>
    <Remarks("mute `@user`")>
    <RequiredBotPermission(ChannelPermission.MuteMembers)>
    Async Function Mute(<Remainder> user As IGuildUser) As Task
        Await MuteAsync(user, Nothing)
        Await ReplyAsync($"{user} has been muted")
    End Function

    <Command("unmute", "um")>
    <Description("Restores all permissions from the user")>
    <Remarks("mute `@user`")>
    <RequiredBotPermission(ChannelPermission.MuteMembers)>
    Async Function Unmute(<Remainder> user As IGuildUser) As Task
        Await UnmuteAsync(user)
        Await ReplyAsync($"{user} has been unmuted")
    End Function

    <Command("mute", "m")>
    <Description("Temporary removes all permissions (except view channel) from the user. He won't be able to send messages, react or anything else.")>
    <Remarks("mute `@user` 1d")>
    <RequiredBotPermission(ChannelPermission.MuteMembers)>
    Async Function Mute(user As IGuildUser, <Remainder> duration As TimeSpan) As Task
        Await MuteAsync(user, duration)
        Await ReplyAsync($"{user} has been muted for " + duration.Humanize(3))
    End Function

    <Command("mute", "m")>
    <Description("Temporary removes all permissions (except view channel) from the user. He won't be able to send messages, react or anything else.")>
    <Remarks("mute 123456 1d")>
    <RequiredBotPermission(ChannelPermission.MuteMembers)>
    Async Function Mute(userId As ULong, <Remainder> duration As TimeSpan) As Task
        Dim user = Context.Guild.GetUser(userId)
        If user Is Nothing Then
            Await ReplyAsync("User not found")
        Else
            Await MuteAsync(user, duration)
            Await ReplyAsync($"{user} has been muted for " + duration.Humanize(3))
        End If
    End Function

    <Command("mute", "m")>
    <Description("Removes all permissions (except view channel) from the user. He won't be able to send messages, react or anything else.")>
    <Remarks("mute 123456 1d")>
    <RequiredBotPermission(ChannelPermission.MuteMembers)>
    Async Function Mute(userId As ULong) As Task
        Dim user = Context.Guild.GetUser(userId)
        If user Is Nothing Then
            Await ReplyAsync("User not found")
        Else
            Await MuteAsync(user, Nothing)
            Await ReplyAsync($"{user} has been muted")
        End If
    End Function

    <Command("kick", "k")>
    <Description("kicks the user from the guild.")>
    <Remarks("kick `@user`")>
    <RequiredBotPermission(GuildPermission.KickMembers)>
    Async Function Kick(user As IGuildUser, <Remainder> Optional reason As String = Nothing) As Task
        Await user.KickAsync(reason, New RequestOptions With {.AuditLogReason = reason})
        Await ReplyAsync($"{user} has been kicked by {Context.User} because: {If(reason, "*no reason provided*")}")
    End Function

    <Command("kick", "k")>
    <Description("kicks the user from the guild.")>
    <Remarks("kick `@user`")>
    <RequiredBotPermission(GuildPermission.KickMembers)>
    Async Function Kick(userId As ULong, <Remainder> Optional reason As String = Nothing) As Task
        Dim user = Context.Guild.GetUser(userId)
        If user Is Nothing Then
            Await ReplyAsync("User not found")
        Else
            Await user.KickAsync(reason, New RequestOptions With {.AuditLogReason = reason})
            Await ReplyAsync($"{user} has been kicked by {Context.User} because: {If(reason, "*no reason provided*")}")
        End If
    End Function
End Class
