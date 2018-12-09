Option Compare Text

Imports Discord
Imports Qmmands

Public Class GuildUserTypeParser
    Inherits TypeParser(Of IGuildUser)

    Public Overrides Function ParseAsync(value As String, context As ICommandContext, provider As IServiceProvider) As Task(Of TypeParserResult(Of IGuildUser))
        Dim ctx = DirectCast(context, DerpContext)
        Dim user = ctx.Guild.Users.FirstOrDefault(Function(u) u.Username.Contains(value))
        Dim userId As ULong

        If user IsNot Nothing Then Return Task.FromResult(TypeParserResult(Of IGuildUser).Successful(user))
        If Not (value.Substring(0, 3) = "<@!" AndAlso value(value.Length - 1) = ">") Then Return Task.FromResult(TypeParserResult(Of IGuildUser).Unsuccessful("Invalid format or user not found"))
        If Not ULong.TryParse(value.Substring(3, value.Length - 4), userId) Then Return Task.FromResult(TypeParserResult(Of IGuildUser).Unsuccessful("Invalid user id"))

        Dim guildUser = ctx.Guild.Users.FirstOrDefault(Function(u) u.Id = userId)
        If guildUser Is Nothing Then Return Task.FromResult(TypeParserResult(Of IGuildUser).Unsuccessful("Couldn't find any user with that id"))

        Return Task.FromResult(TypeParserResult(Of IGuildUser).Successful(guildUser))
    End Function
End Class
