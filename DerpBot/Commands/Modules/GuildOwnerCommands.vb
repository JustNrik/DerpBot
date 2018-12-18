Imports DerpBot.DerpCommandResult
Imports Discord
Imports SQLExpress
Imports Qmmands

<RequiredGuildOwner>
<RequiredContext(ContextType.Guild)>
Public Class GuildOwnerCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Database As SQLExpressClient

    <Command("addmod")>
    Async Function AddMod(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Mods.Contains(user.Id) Then
            Await ReplyAsync("The user is already a Mod")
        ElseIf guild.Admins.Contains(user.Id) Then
            Await ReplyAsync("The user is an Admin, can't add him as Mod")
        ElseIf guild.BlackList.Contains(user.Id) Then
            Await ReplyAsync("The user is blacklisted, remove him from the black list in order to make him a mod.")
        Else
            guild.Mods.Add(user.Id)
            Await ReplyAsync($"The user {user} is now a mod!")
            Await Database.UpdateObjectAsync(guild)
            Return Successful
        End If
        Return Unsuccessful
    End Function

    <Command("addadmin")>
    Async Function AddAdmin(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Admins.Contains(user.Id) Then
            Await ReplyAsync("The user is already an Admin")
        ElseIf guild.BlackList.Contains(user.id) Then
            Await ReplyAsync("The user is blacklisted, can't add him as Admin")
        ElseIf guild.Mods.Contains(user.id) Then
            guild.Admins.Add(user.Id)
            guild.Mods.Remove(user.Id)
            Await ReplyAsync("The user has been promoted from Mod to Admin")
            Await Database.UpdateObjectAsync(guild)
            Return Successful
        ElseIf guild.BlackList.Contains(user.Id) Then
            Await ReplyAsync("The user is blacklisted, remove him from the black list in order to make him a mod.")
        Else
            guild.Admins.Add(user.Id)
            Await ReplyAsync($"The user {user} is now a mod!")
            Await Database.UpdateObjectAsync(guild)
            Return Successful
        End If
        Return Unsuccessful
    End Function

    <Command("demotemod")>
    Async Function DemoteMod(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Mods.Contains(user.Id) Then
            Await ReplyAsync(user.ToString() & " has been demoted from the moderation team")
            Return Successful
        End If
        Await ReplyAsync("The user is not a mod, hence can't demote.")
        Return Unsuccessful
    End Function

    <Command("demoteadmin")>
    Async Function DemoteAdmin(<Remainder> user As IGuildUser) As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Admins.Contains(user.Id) Then
            Await ReplyAsync(user.ToString() & " has been demoted from the administration team")
            Return Successful
        End If
        Await ReplyAsync("The user is not an admin, hence can't demote.")
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
