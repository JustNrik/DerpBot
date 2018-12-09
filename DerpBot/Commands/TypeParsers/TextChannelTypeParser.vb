Option Compare Text

Imports Discord
Imports Qmmands

Public Class TextChannelTypeParser
    Inherits TypeParser(Of ITextChannel)

    Public Overrides Function ParseAsync(value As String, context As ICommandContext, provider As IServiceProvider) As Task(Of TypeParserResult(Of ITextChannel))
        Dim ctx = DirectCast(context, DerpContext)
        Dim channel = ctx.Guild.TextChannels.FirstOrDefault(Function(u) u.Name.Contains(value))
        Dim channelId As ULong

        If channel IsNot Nothing Then Return Task.FromResult(TypeParserResult(Of ITextChannel).Successful(channel))
        If Not (value.Substring(0, 2) = "<#" AndAlso value(value.Length - 1) = ">") Then Return Task.FromResult(TypeParserResult(Of ITextChannel).Unsuccessful("Invalid format or channel not found"))
        If Not ULong.TryParse(value.Substring(3, value.Length - 4), channelId) Then Return Task.FromResult(TypeParserResult(Of ITextChannel).Unsuccessful("Invalid channel id"))

        Dim guildUser = ctx.Guild.Users.FirstOrDefault(Function(u) u.Id = channelId)
        If guildUser Is Nothing Then Return Task.FromResult(TypeParserResult(Of ITextChannel).Unsuccessful("Couldn't find any channel with that id"))

        Return Task.FromResult(TypeParserResult(Of ITextChannel).Successful(channel))
    End Function
End Class
