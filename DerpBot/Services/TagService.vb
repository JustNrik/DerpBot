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

    Public Sub UseTag(context As IDerpContext, tagName As String)
        Dim guild = _database.LoadObject(Of Guild)(context.Guild.Id)
        Dim tag = guild.Tags.Find(Function(x) x.TagName = tagName)
        tag.Uses += 1
        _database.UpdateObject(guild)
    End Sub

    Public Sub CreateTag(context As IDerpContext, tagName As String, tagValue As String)
        Dim newTag = New Tag With
        {
            .TagName = tagName,
            .TagValue = tagValue,
            .TagOwner = context.User.Id,
            .CreatedAt = Date.UtcNow,
            .Uses = 0
        }
        Dim guild = _database.LoadObject(Of Guild)(context.Guild.Id)
        guild.Tags.Add(newTag)
        _database.UpdateObject(guild)
    End Sub

    Public Sub UpdateTag(context As IDerpContext, tagName As String, tagValue As String)
        Dim guild = _database.LoadObject(Of Guild)(context.Guild.Id)
        guild.Tags.Find(Function(x) x.TagName = tagName).TagValue = tagValue
        _database.UpdateObject(guild)
    End Sub

    Public Sub DeleteTag(context As IDerpContext, tagName As String)
        Dim targetTag = GetTags(context).FirstOrDefault(Function(x) x.TagName = tagName)
        Dim guild = _database.LoadObject(Of Guild)(context.Guild.Id)
        guild.Tags.Remove(targetTag)
        _database.UpdateObject(guild)
    End Sub

    Public Function GetTags(context As ICommandContext) As List(Of Tag)
        Dim ctx = DirectCast(context, IDerpContext)
        Return _database.LoadObject(Of Guild)(ctx.Guild.Id).Tags
    End Function

    Public Shared Function TryParse(tags As List(Of Tag), tagName As String, ByRef tag As Tag) As Boolean
        tag = tags.FirstOrDefault(Function(x) x.TagName = tagName)
        Return tag IsNot Nothing
    End Function

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(True)
    End Function
End Class