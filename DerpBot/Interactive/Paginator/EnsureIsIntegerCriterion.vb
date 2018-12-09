Imports Discord.WebSocket
Imports Qmmands

Friend Class EnsureIsIntegerCriterion
    Implements ICriteria(Of SocketMessage)

    Public Function JudgeAsync(sourceContext As IDerpContext, parameter As SocketMessage) As Task(Of Boolean) Implements ICriteria(Of SocketMessage).JudgeAsync
        Return Task.FromResult(Integer.TryParse(parameter.Content, Nothing))
    End Function
End Class