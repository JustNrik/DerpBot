Imports Discord
Imports SQLExpress
Imports Qmmands

<RequiredGuildOwner>
<RequiredContext(ContextType.Guild)>
Public Class GuildOwnerCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Database As SQLExpressClient

    <Command("addmodrole")>
    Async Function AddModRole(<Remainder> role As IRole) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            guild.ModRole = role.Id
            Await ReplyAsync("Mod role has been set")
            Await Database.UpdateObjectAsync(guild)
        Else
            Await ReplyAsync("There's already a Mod role assigned for this guild")
        End If
    End Function

    <Command("addadminrole")>
    Async Function AddAdminRole(<Remainder> role As IRole) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            guild.AdminRole = role.Id
            Await ReplyAsync("Mod role has been set")
            Await Database.UpdateObjectAsync(guild)
        Else
            Await ReplyAsync("There's already a Mod role assigned for this guild")
        End If
    End Function

    <Command("addmod")>
    Async Function AddMod(<Remainder> user As IGuildUser) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            Await ReplyAsync("There's no Mod role set for this guild")
        Else
            Dim role = Context.Guild.GetRole(guild.ModRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await ReplyAsync("The user already has a Mod role")
            Else
                Await user.AddRoleAsync(role)
                Await ReplyAsync($"{user} is not a Mod")
            End If
        End If
    End Function

    <Command("addadmin")>
    Async Function AddAdmin(<Remainder> user As IGuildUser) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            Await ReplyAsync("There's no Admin role set for this guild")
        Else
            Dim role = Context.Guild.GetRole(guild.AdminRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await ReplyAsync("The user already has an Admin role")
            Else
                Await user.AddRoleAsync(role)
                Await ReplyAsync($"{user} is not an Admin")
            End If
        End If
    End Function

    <Command("removemodrole")>
    Async Function RemoveModRole(<Remainder> role As IRole) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            Await ReplyAsync("There's no Mod role set, hence can't remove any")
        Else
            guild.ModRole = Nothing
            Await ReplyAsync($"The Mod role {role} has been successfully removed")
            Await Database.UpdateObjectAsync(guild)
        End If
    End Function

    <Command("removeadminrole")>
    Async Function RemoveAdminRole(<Remainder> role As IRole) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            Await ReplyAsync("There's no Admin role set, hence can't remove any")
        Else
            guild.AdminRole = Nothing
            Await ReplyAsync($"The Admin role {role} has been successfully removed")
            Await Database.UpdateObjectAsync(guild)
        End If
    End Function

    <Command("demotemod")>
    Async Function DemoteMod(<Remainder> user As IGuildUser) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.ModRole.HasValue Then
            Await ReplyAsync("There's no Mod role, hence can't demote")
        Else
            Dim role = Context.Guild.GetRole(guild.AdminRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await user.RemoveRoleAsync(role)
                Await ReplyAsync($"{user} has been demoted")
            Else
                Await ReplyAsync($"{user} does have any Admin role, hence can't demote")
            End If
        End If
    End Function

    <Command("demoteadmin")>
    Async Function DemoteAdmin(<Remainder> user As IGuildUser) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AdminRole.HasValue Then
            Await ReplyAsync("There's no Admin role, hence can't demote")
        Else
            Dim role = Context.Guild.GetRole(guild.AdminRole.Value)
            If user.RoleIds.Contains(role.Id) Then
                Await user.RemoveRoleAsync(role)
                Await ReplyAsync($"{user} has been demoted")
            Else
                Await ReplyAsync($"{user} does have any Admin role, hence can't demote")
            End If
        End If
    End Function

    <Command("addprefix")>
    Async Function AddPrefix(<Remainder> prefix As String) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Prefixes.Contains(prefix) Then
            Await ReplyAsync("That prefix is already in this guild")
            Return
        Else
            If prefix.Length > 5 Then
                Await ReplyAsync("Your prefix is too long, a maximun of 5 characters is allowed.")
            Else
                guild.Prefixes.Add(prefix)
                Await Database.UpdateObjectAsync(guild)
                Await ReactOkAsync()
            End If
        End If
    End Function

    <Command("deleteprefix", "delprefix", "removeprefix")>
    Async Function RemovePrefix(<Remainder> prefix As String) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.Prefixes.Contains(prefix) Then
            Await ReplyAsync("That prefix is not in this guild, hence can't remove")
        Else
            guild.Prefixes.Remove(prefix)
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
        End If
    End Function

    <Command("addannchannel", "addannoucementchannel", "aac")>
    Async Function AddAnnoucementChannel(channel As ITextChannel) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AnnouncementChannel.HasValue Then
            guild.AnnouncementChannel = channel.Id
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
        Else
            Await ReplyAsync("There's already an annoucement channel.")
        End If
    End Function

    <Command("addannchannel", "addannoucementchannel", "aac")>
    Async Function AddAnnoucementChannel(channelId As ULong) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AnnouncementChannel.HasValue Then
            guild.AnnouncementChannel = channelId
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
        Else
            Await ReplyAsync("There's already an annoucement channel.")
        End If
    End Function

    <Command("delannchannel", "deleteannoucementchannel", "removeannchannel", "removeannc", "dac")>
    Async Function RemoveAnnoucementChannel() As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.AnnouncementChannel.HasValue Then
            guild.AnnouncementChannel = Nothing
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
        Else
            Await ReplyAsync("There's not any annoucement channel, hence can't delete.")
        End If
    End Function

    <Command("announce", "ann")>
    Async Function Announce(<Remainder> text As String) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If Not guild.AnnouncementChannel.HasValue Then
            Await ReplyAsync("There's not annoucement channel set.")
        Else
            Dim channel = Context.Guild.TextChannels.FirstOrDefault(Function(ch) ch.Id = guild.AnnouncementChannel.Value)
            If channel Is Nothing Then
                Await ReplyAsync("You set an invalid channel as an Annoucement channel.")
            Else
                Await channel.SendMessageAsync(text)
            End If
        End If
    End Function
End Class
