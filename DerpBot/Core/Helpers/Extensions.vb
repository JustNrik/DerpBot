Option Compare Text
Imports System.Collections.Immutable
Imports System.Net
Imports System.Runtime.CompilerServices
Imports Discord
Imports Discord.Net
Imports Discord.WebSocket
Imports Qmmands

Module Extensions
    <Extension>
    Public Function FirstLetterToUpper(str As String) As String
        Return Char.ToUpper(str(0)) & str.Substring(1)
    End Function
    <Extension>
    Public Function GetJumpLink(message As IUserMessage) As String
        Return $"https://discordapp.com/channels/{TryCast(message.Channel, IGuildChannel)?.Guild.Id}/{message.Channel.Id}/{message.Id}"
    End Function
    <Extension>
    Public Function AddEmptyField(embedBuilder As EmbedBuilder) As EmbedBuilder
        Return embedBuilder.AddField(ChrW(&H200B), ChrW(&H200B))
    End Function
    <Extension>
    Public Function AddFieldIf(embedBuilder As EmbedBuilder, condition As Boolean, field As EmbedFieldBuilder) As EmbedBuilder
        Return If(condition, embedBuilder.AddField(field), embedBuilder)
    End Function
    <Extension>
    Public Function AddFieldIf(embedBuilder As EmbedBuilder, condition As Boolean, name As String, value As String, Optional inline As Boolean = False) As EmbedBuilder
        Return If(condition, embedBuilder.AddField(name, value, inline), embedBuilder)
    End Function
    <Extension>
    Public Function AddFieldIf(embedBuilder As EmbedBuilder, condition As Boolean, action As Action(Of EmbedFieldBuilder)) As EmbedBuilder
        Return If(condition, embedBuilder.AddField(action), embedBuilder)
    End Function
    <Extension>
    Public Function AddFieldsIf(embedBuilder As EmbedBuilder, fields As IEnumerable(Of EmbedFieldBuilder), condition As Boolean) As EmbedBuilder
        Return If(condition, embedBuilder.AddFields(fields), embedBuilder)
    End Function
    <Extension>
    Public Function AddReactionsAsync(msg As IUserMessage, emotes As IEnumerable(Of IEmote)) As Task
        Return Task.WhenAll(emotes.Select(Function(emote) msg.AddReactionAsync(emote)))
    End Function
    <Extension>
    Public Function AddFields(builder As EmbedBuilder, fields As IEnumerable(Of EmbedFieldBuilder)) As EmbedBuilder
        For Each field In fields
            builder.AddField(field)
        Next
        Return builder
    End Function
    <Extension>
    Public Function GetDisplayName(guildUser As IGuildUser) As String
        Return If(guildUser.Nickname, guildUser.Username)
    End Function
    <Extension>
    Public Function HasRole(guildUser As IGuildUser, roleId As ULong) As Boolean
        Return HasRole(guildUser, guildUser.Guild.GetRole(roleId))
    End Function
    <Extension>
    Public Function HasRole(guildUser As IGuildUser, role As IRole) As Boolean
        Return HasRole(guildUser, role.Name)
    End Function
    <Extension>
    Public Function HasRole(guildUser As IGuildUser, roleName As String) As Boolean
        Return guildUser.RoleIds.Select(Function(x) guildUser.Guild.GetRole(x).Name).Contains(roleName)
    End Function
    <Extension>
    Public Function GetAvatarOrDefaultUrl(guildUser As IUser, Optional format As ImageFormat = ImageFormat.Auto) As String
        Return If(guildUser.GetAvatarUrl(format), guildUser.GetDefaultAvatarUrl())
    End Function
    <Extension>
    Public Function TrySendDMAsync(user As IUser, content As String, Optional isTTS As Boolean = False, Optional embed As Embed = Nothing, Optional options As RequestOptions = Nothing) As Task(Of IUserMessage)
        Try
            Return user?.SendMessageAsync(content, isTTS, embed, options)
        Catch e As HttpException When e.HttpCode = HttpStatusCode.Forbidden
            Return Nothing
        End Try
    End Function
    <Extension>
    Function GetDefaultChannel(guild As SocketGuild) As SocketTextChannel
        Return (From textChannel In guild.TextChannels
                Where guild.CurrentUser.GetPermissions(textChannel).SendMessages AndAlso
                      guild.CurrentUser.GetPermissions(textChannel).ViewChannel
                Order By textChannel.Position).FirstOrDefault()
    End Function
    <Extension>
    Public Async Function CheckPermissionsAsync([module] As [Module], context As IDerpContext, services As IServiceProvider) As Task(Of CheckResult)
        Dim checkGroup = Async Function(preconditions As IReadOnlyList(Of CheckBaseAttribute), type As String) As Task(Of CheckResult)
                             For Each preconditionGroup In preconditions.GroupBy(Function(x) x.Group)
                                 If preconditionGroup.Key Is Nothing Then
                                     For Each precondition In preconditionGroup
                                         Dim result = Await precondition.CheckAsync(context, services)
                                         If Not result.IsSuccessful Then Return result
                                     Next
                                 Else
                                     Dim results As New List(Of CheckResult)
                                     For Each precondition In preconditionGroup
                                         results.Add(Await precondition.CheckAsync(context, services))
                                     Next

                                     If Not results.Any(Function(x) x.IsSuccessful) Then Return CheckResult.Unsuccessful($"{type} precondition group {preconditionGroup.Key} failed")
                                     ' waiting for answer for PreconditionGroupResult
                                 End If
                             Next

                             Return CheckResult.Successful
                         End Function

        Dim moduleResult = Await checkGroup([module].Checks, "Module")
        Return If(Not moduleResult.IsSuccessful, moduleResult, CheckResult.Successful)
    End Function
    <Extension>
    Public Function AddSignature(embedBuilder As EmbedBuilder, user As IUser, Optional guild As IGuild = Nothing) As EmbedBuilder
        embedBuilder.
            WithCurrentTimestamp().
            WithFooter($"Requested by: {user.Username}#{user.DiscriminatorValue}{If(guild Is Nothing, "", $" - {guild.Name}")}", user.GetAvatarOrDefaultUrl())
        Return embedBuilder
    End Function
    <Extension>
    Public Async Function AddDeleteCallbackAsync(msg As IUserMessage, context As ICommandContext, interactive As InteractiveService) As Task(Of IUserMessage)
        Dim emoji As New Emoji("🚮")
        Await msg.AddReactionAsync(emoji)
        Dim callback As New DeleteCallback(context, interactive, msg, emoji)
        callback.StartDelayAsync()
        Return msg
    End Function
End Module
