Option Compare Text

Imports SQLExpress
Imports Qmmands

<Service(ServiceType.Singleton, 0)>
Public Class TagService
    Implements IService

    Private ReadOnly _database As SQLExpressClient

    Public Sub New(Database As SQLExpressClient)
        _database = Database
    End Sub

    Public Async Function UseTagAsync(context As IDerpContext, tagName As String) As Task
        Dim guild = Await _database.LoadObjectAsync(Of Guild)(context.Guild.Id)
        Dim tag = guild.Tags.Find(Function(x) x.TagName = tagName)
        tag.Uses += 1
        Await _database.UpdateObjectAsync(guild)
    End Function

    Public Async Function CreateTagAsync(context As IDerpContext, tagName As String, tagValue As String) As Task
        Dim newTag = New Tag With
        {
            .TagName = tagName,
            .TagValue = tagValue,
            .TagOwner = context.User.Id,
            .CreatedAt = Date.UtcNow,
            .Uses = 0
        }
        Dim guild = Await _database.LoadObjectAsync(Of Guild)(context.Guild.Id)
        guild.Tags.Add(newTag)
        Await _database.UpdateObjectAsync(guild)
    End Function

    Public Async Function UpdateTagAsync(context As IDerpContext, tagName As String, tagValue As String) As Task
        Dim guild = Await _database.LoadObjectAsync(Of Guild)(context.Guild.Id)
        guild.Tags.Find(Function(x) x.TagName = tagName).TagValue = tagValue
        Await _database.UpdateObjectAsync(guild)
    End Function

    Public Async Function DeleteTag(context As IDerpContext, tagName As String) As Task
        Dim targetTag = (Await GetTagsAsync(context)).FirstOrDefault(Function(x) x.TagName = tagName)
        Dim guild = Await _database.LoadObjectAsync(Of Guild)(context.Guild.Id)
        guild.Tags.Remove(targetTag)
        Await _database.UpdateObjectAsync(guild)
    End Function

    Public Async Function GetTagsAsync(context As ICommandContext) As Task(Of List(Of Tag))
        Dim ctx = DirectCast(context, IDerpContext)
        Return (Await _database.LoadObjectAsync(Of Guild)(ctx.Guild.Id)).Tags
    End Function

    Public Shared Function TryParse(tags As List(Of Tag), tagName As String, ByRef tag As Tag) As Boolean
        tag = tags.FirstOrDefault(Function(x) x.TagName = tagName)
        Return tag IsNot Nothing
    End Function

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(True)
    End Function
End Class