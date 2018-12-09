Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection

Public Class TagTypeReader
    Inherits TypeReader

    Public Overrides Function ReadAsync(context As ICommandContext, input As String, services As IServiceProvider) As Task(Of TypeReaderResult)
        Dim tagService = services.GetService(Of TagService)
        Dim currentTags = tagService.GetTags(context)
        Dim levenTags = currentTags.Where(Function(x) CalcLevenshteinDistance(x.TagName, input) < 5)
        Dim containsTags = currentTags.Where(Function(x) x.TagName.Contains(input))
        Dim totalTags = levenTags.Concat(containsTags)
        Dim foundTag As Tag = Nothing
        Return Task.FromResult(If(TagService.TryParse(currentTags, input, foundTag), TypeReaderResult.FromSuccess(foundTag), TypeReaderResult.FromError(CommandError.ParseFailed, ("Tag not found did you mean?" & vbLf & $"{String.Join(vbLf, totalTags.[Select](Function(x) x.TagName))}"))))
    End Function
End Class
