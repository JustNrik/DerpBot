Imports Discord
Imports Qmmands

Public Class EnsureSourceChannelCriteria
    Implements ICriteria(Of IMessage)

    Public Function JudgeAsync(sourceContext As IDerpContext, parameter As IMessage) As Task(Of Boolean) Implements ICriteria(Of IMessage).JudgeAsync
        Return Task.FromResult(sourceContext.Channel.Id = parameter.Channel.Id)
    End Function
End Class
