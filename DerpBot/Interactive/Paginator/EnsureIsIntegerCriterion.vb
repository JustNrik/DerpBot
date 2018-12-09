Imports Discord.Commands
Imports Discord.WebSocket

Friend Class EnsureIsIntegerCriterion
    Implements ICriterion(Of SocketMessage)

    Public Function JudgeAsync(sourceContext As ICommandContext, parameter As SocketMessage) As Task(Of Boolean) Implements ICriterion(Of SocketMessage).JudgeAsync
        Return Task.FromResult(Integer.TryParse(parameter.Content, Nothing))
    End Function
End Class