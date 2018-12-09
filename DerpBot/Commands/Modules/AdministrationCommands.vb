Imports Discord
Imports SQLExpress
Imports Qmmands

<RequiredRole(SpecialRole.Admin)>
<RequiredContext(ContextType.Guild)>
Public Class AdministrationCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Database As SQLExpressClient

    <Command("ban", "b")>
    <Description("Bans the user from the guild. You can optionally provide the reason and the amount of days his messages will be deleted (max 7)")>
    <RequiredBotPermission(GuildPermission.BanMembers)>
    Async Function Ban(user As IGuildUser, Optional reason As String = Nothing, Optional pruneDays As Integer = 0) As Task
        If pruneDays < 0 OrElse pruneDays > 7 Then
            Await ReplyAsync("Prune days must be between 0 and 7")
            Return
        End If
        If Await Context.Guild.GetBanAsync(user) IsNot Nothing Then
            Await Context.Guild.AddBanAsync(user, pruneDays, reason)
            Await ReplyAsync($"{user} has been banned because: {If(reason, "*no reason provided*")}")
        Else
            Await ReplyAsync($"{user} is already banned")
        End If
    End Function

    <Command("getban", "getb")>
    <Description("Gets the ban reason of the user providen his id.")>
    <Remarks("getb 123456")>
    Async Function GetBan(id As ULong) As Task
        Dim ban = Await Context.Guild.GetBanAsync(id)
        Await ReplyAsync($"{If(ban Is Nothing, "That user is not banned", $"The user {ban.User.Username} got banned because: {If(ban.Reason, "*no reason provided*")}")}")
    End Function

    <Command("blacklist", "bl")>
    <Description("Blacklists the user so he won't be able to execute commands in this guild.")>
    <Remarks("bl `@user`")>
    Async Function BlackList(<Remainder> user As IGuildUser) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.BlackList.Contains(user.Id) Then
            Await ReplyAsync($"{user} is already blacklisted")
            Return
        Else
            guild.BlackList.Add(user.Id)
            Await ReplyAsync($"{user} has been blacklisted, he won't be able to execute commands in this guild")
        End If
    End Function

    <Command("addstarboard", "addstarb")>
    <Description("Creates and sets the starboard channel with the amount of stars needed")>
    <Remarks("addstarb")>
    <RequiredBotPermission(GuildPermission.ManageChannels)>
    Async Function AddStarboard(starCount As Byte) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Starboard.HasValue Then
            Await ReplyAsync("There's already an starboard in this server")
        Else
            Dim channel = Await Context.Guild.CreateTextChannelAsync("Starboard")
            Dim permissions = OverwritePermissions.DenyAll(channel).Modify(viewChannel:=PermValue.Allow)
            Await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, permissions)
            guild.Starboard = channel.Id
            guild.StarCount = starCount
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
        End If
    End Function

    <Command("setstarboard", "setstarb")>
    <Description("Sets the starboard channel for this server with the amount of stars needed. This does not creates the channel.")>
    <Remarks("setstarb #Starboard 3")>
    Async Function SetStarboard(channel As ITextChannel, starCount As Byte) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Starboard.HasValue Then
            Await ReplyAsync("There's already an starboard in this server")
        Else
            Dim permissions = OverwritePermissions.DenyAll(channel).Modify(viewChannel:=PermValue.Allow)
            Await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, permissions)
            guild.Starboard = channel.Id
            guild.StarCount = starCount
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
        End If
    End Function

    <Command("setstarboard", "setstarb")>
    <Description("Sets the starboard channel for this server with the amount of stars needed. This does not creates the channel.")>
    <Remarks("setstarb #Starboard")>
    Async Function SetStarboard(channelId As ULong, starCount As Byte) As Task
        If starCount = 0 Then
            Await ReplyAsync($"How am I supposed to set up a starboard with 0 stars {EmotesDict("roothink")}")
            Return
        End If
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Starboard.HasValue Then
            Await ReplyAsync("There's already an starboard in this server")
        Else
            Dim channel = Context.Guild.GetTextChannel(channelId)
            If channel Is Nothing Then
                Await ReplyAsync("Invalid channel")
            Else
                Dim permissions = OverwritePermissions.DenyAll(channel).Modify(viewChannel:=PermValue.Allow)
                Await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, permissions)
                guild.Starboard = channel.Id
                guild.StarCount = starCount
                Await Database.UpdateObjectAsync(guild)
                Await ReactOkAsync()
            End If
        End If
    End Function

    <Command("deletestarboard", "delstarb", "deletestarb", "removestarboard", "remstarb")>
    <Description("Removes the starboard channel id from the database. This does not delete the channel.")>
    <Remarks("delstarb #Starboard")>
    <RequiredBotPermission(GuildPermission.ManageChannels)>
    Async Function RemoveStarboard(<Remainder> channel As ITextChannel) As Task
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        If guild.Starboard.HasValue Then
            guild.Starboard = Nothing
            guild.StarCount = 0
            Await Database.UpdateObjectAsync(guild)
            Await ReactOkAsync()
        Else
            Await ReplyAsync("There's not a starboard assigned to this guild. Hence can't remove it.")
        End If
    End Function

    <Command("nickname", "nick")>
    <Description("Changes the nickname of the targeted user")>
    <Remarks("nick derp `@user`")>
    <RequiredBotPermission(GuildPermission.ManageNicknames)>
    Function NickName(user As IGuildUser, <Remainder> nick As String) As Task
        Return user.ModifyAsync(Sub(x) x.Nickname = nick)
    End Function

    <Command("botnick", "bnick")>
    <Description("Changes the nickname of the bot")>
    <Remarks("nick DerpBot")>
    Function NickName(<Remainder> nick As String) As Task
        Return Context.Guild.CurrentUser.ModifyAsync(Sub(x) x.Nickname = nick)
    End Function

End Class
