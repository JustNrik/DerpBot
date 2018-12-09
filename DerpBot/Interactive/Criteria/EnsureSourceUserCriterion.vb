Imports Discord
Imports Qmmands

Public Class EnsureSourceUserCriteria
    Implements ICriteria(Of IMessage)

    Public Function JudgeAsync(sourceContext As IDerpContext, parameter As IMessage) As Task(Of Boolean) Implements ICriteria(Of IMessage).JudgeAsync
        Return Task.FromResult(sourceContext.User.Id = parameter.Author.Id)
    End Function
End Class