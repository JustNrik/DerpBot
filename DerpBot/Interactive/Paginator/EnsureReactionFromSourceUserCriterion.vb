Imports Discord.Commands
Imports Discord.WebSocket

Friend Class EnsureReactionFromSourceUserCriterion
    Implements ICriterion(Of SocketReaction)

    Public Function JudgeAsync(sourceContext As ICommandContext, reaction As SocketReaction) As Task(Of Boolean) Implements ICriterion(Of SocketReaction).JudgeAsync
        Return Task.FromResult(reaction.UserId = sourceContext.User.Id)
    End Function
End Class
