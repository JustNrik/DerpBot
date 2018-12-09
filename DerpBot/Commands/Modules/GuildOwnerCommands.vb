Imports DerpBot.DerpCommandResult
Imports Discord
Imports SQLExpress
Imports Qmmands

<RequiredGuildOwner>
<RequiredContext(ContextType.Guild)>
Public Class GuildOwnerCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Database As SQLExpressClient

    <Command("addmodrole")>
    Async Function AddModRole(<Remainder> role As IRole) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            guild.ModRole = role.Id
            Await ReplyAsync("Mod role has been set")
            Await Database.UpdateObjectAsync(guild)
            Return Successful
        Else
            Await ReplyAsync("There's already a Mod role assigned for this guild")
            Return Unsuccessful
        End If
    End Function

    <Command("addadminrole")>
    Async Function AddAdminRole(<Remainder> role As IRole) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            guild.AdminRole = role.Id
            Await ReplyAsync("Mod role has been set")
            Await Database.UpdateObjectAsync(guild)
            Return Successful
        Else
            Await ReplyAsync("There's already a Mod role assigned for this guild")
            Return Unsuccessful
        End If
    End Function

    <Command("addmod")>
    Async Function AddMod(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            Await ReplyAsync("There's no Mod role set for this guild")
            Return Unsuccessful
        Else
            Dim role = Context.Guild.GetRole(guild.ModRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await ReplyAsync("The user already has a Mod role")
                Return Unsuccessful
            Else
                Await user.AddRoleAsync(role)
                Await ReplyAsync($"{user} is now a Mod")
                Return Successful
            End If
        End If
    End Function

    <Command("addadmin")>
    Async Function AddAdmin(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            Await ReplyAsync("There's no Admin role set for this guild")
            Return Unsuccessful
        Else
            Dim role = Context.Guild.GetRole(guild.AdminRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await ReplyAsync("The user already has an Admin role")
                Return Unsuccessful
            Else
                Await user.AddRoleAsync(role)
                Await ReplyAsync($"{user} is now an Admin")
                Return Successful
            End If
        End If
    End Function

    <Command("removemodrole")>
    Async Function RemoveModRole(<Remainder> role As IRole) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            Await ReplyAsync("There's no Mod role set, hence can't remove any")
            Return Unsuccessful
        Else
            guild.ModRole = Nothing
            Await ReplyAsync($"The Mod role {role} has been successfully removed")
            Await Database.UpdateObjectAsync(guild)
            Return Successful
        End If
    End Function

    <Command("removeadminrole")>
    Async Function RemoveAdminRole(<Remainder> role As IRole) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            Await ReplyAsync("There's no Admin role set, hence can't remove any")
            Return Unsuccessful
        Else
            guild.AdminRole = Nothing
            Await ReplyAsync($"The Admin role {role} has been successfully removed")
            Await Database.UpdateObjectAsync(guild)
            Return Successful
        End If
    End Function

    <Command("demotemod")>
    Async Function DemoteMod(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            Await ReplyAsync("There's no Mod role, hence can't demote")
            Return Unsuccessful
        Else
            Dim role = Context.Guild.GetRole(guild.AdminRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await user.RemoveRoleAsync(role)
                Await ReplyAsync($"{user} has been demoted")
                Return Successful
            Else
                Await ReplyAsync($"{user} does have any Admin role, hence can't demote")
                Return Unsuccessful
            End If
        End If
    End Function

    <Command("demoteadmin")>
    Async Function DemoteAdmin(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            Await ReplyAsync("There's no Admin role, hence can't demote")
        Else
            Dim role = Context.Guild.GetRole(guild.AdminRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await user.RemoveRoleAsync(role)
                Await ReplyAsync($"{user} has been demoted")
                Return Successful
            Else
                Await ReplyAsync($"{user} does have any Admin role, hence can't demote")
            End If
        End If
        Return Unsuccessful
    End Function

    <Command("addprefix")>
    Async Function AddPrefix(<Remainder> prefix As String) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Prefixes.Contains(prefix) Then
            Await ReplyAsync("That prefix is already in this guild")
        Else
            If prefix.Length > 5 Then
                Await ReplyAsync("Your prefix is too long, a maximun of 5 characters is allowed.")
            Else
                guild.Prefixes.Add(prefix)
                Await Database.UpdateObjectAsync(guild)
                Await ReactOkAsync()
                Return Successful
            End If
        End If
        Return Unsuccessful
    End Function

    <Command("deleteprefix", "delprefix", "removeprefix")>
    Async Function RemovePrefix(<Remainder> prefix As String) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.Prefixes.Contains(prefix) Then
            Await ReplyAsync("That prefix is not in this guild, hence can't remove")
            Return Unsuccessful
        Else
            guild.Prefixes.Remove(prefix)
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
            Return Successful
        End If
    End Function

    <Command("addannchannel", "addannoucementchannel", "aac")>
    Async Function AddAnnoucementChannel(channel As ITextChannel) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AnnouncementChannel.HasValue Then
            guild.AnnouncementChannel = channel.Id
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
            Return Successful
        Else
            Await ReplyAsync("There's already an annoucement channel.")
            Return Unsuccessful
        End If
    End Function

    <Command("addannchannel", "addannoucementchannel", "aac")>
    Async Function AddAnnoucementChannel(channelId As ULong) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AnnouncementChannel.HasValue Then
            guild.AnnouncementChannel = channelId
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
            Return Successful
        Else
            Await ReplyAsync("There's already an annoucement channel.")
            Return Unsuccessful
        End If
    End Function

    <Command("delannchannel", "deleteannoucementchannel", "removeannchannel", "removeannc", "dac")>
    Async Function RemoveAnnoucementChannel() As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.AnnouncementChannel.HasValue Then
            guild.AnnouncementChannel = Nothing
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
            Return Successful
        Else
            Await ReplyAsync("There's not any annoucement channel, hence can't delete.")
            Return Unsuccessful
        End If
    End Function

    <Command("announce", "ann")>
    Async Function Announce(<Remainder> text As String) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AnnouncementChannel.HasValue Then
            Await ReplyAsync("There's not annoucement channel set.")
        Else
            Dim channel = Context.Guild.TextChannels.FirstOrDefault(Function(ch) ch.Id = guild.AnnouncementChannel.Value)
            If channel Is Nothing Then
                Await ReplyAsync("You set an invalid channel as an Annoucement channel.")
            Else
                Await channel.SendMessageAsync(text)
                Return Successful
            End If
        End If
        Return Unsuccessful
    End Function
End Class
