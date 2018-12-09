Imports Discord
Imports Qmmands

Public Class RoleTypeParser
    Inherits TypeParser(Of IRole)

    Public Overrides Function ParseAsync(value As String, context As ICommandContext, provider As IServiceProvider) As Task(Of TypeParserResult(Of IRole))
        Dim roleId As ULong
        Dim ctx = DirectCast(context, DerpContext)

        If Not (value.Substring(0, 3) = "<@&" AndAlso value(value.Length - 1) = ">") Then Return Task.FromResult(TypeParserResult(Of IRole).Unsuccessful("Invalid role format"))
        If Not ULong.TryParse(value.Substring(3, value.Length - 4), roleId) Then Return Task.FromResult(TypeParserResult(Of IRole).Unsuccessful("Invalid role id"))

        Dim role = ctx.Guild.GetRole(roleId)
        If role Is Nothing Then Return Task.FromResult(TypeParserResult(Of IRole).Unsuccessful("Couldn't find any role with that id"))

        Return Task.FromResult(TypeParserResult(Of IRole).Successful(role))
    End Function
End Class
