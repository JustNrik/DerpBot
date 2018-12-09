Imports Microsoft.Extensions.DependencyInjection
Imports Qmmands

Public Class TagTypeParser
    Inherits TypeParser(Of Tag)

    Public Overrides Function ParseAsync(value As String, context As ICommandContext, provider As IServiceProvider) As Task(Of TypeParserResult(Of Tag))
        Dim tarService = provider.GetService(Of TagService)
        Dim currentTags = tarService.GetTags(context)
        Dim levenTags = currentTags.Where(Function(x) CalcLevenshteinDistance(x.TableName, value) < 5)
        Dim containsTags = currentTags.Where(Function(x) x.TableName.Contains(value))
        Dim totalTags = levenTags.Concat(containsTags)
        Dim foundTag As Tag = Nothing
        Return Task.FromResult(If(TagService.TryParse(currentTags, value, foundTag),
                               TypeParserResult(Of Tag).Successful(foundTag),
                               TypeParserResult(Of Tag).Unsuccessful("Tag not found, did you mean?" & vbLf & $"{String.Join(vbLf, totalTags.Select(Function(x) x.TagName))}")))
    End Function

End Class
