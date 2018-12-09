Imports Discord.WebSocket
Imports Qmmands

Friend Class EnsureReactionFromSourceUserCriterion
    Implements ICriteria(Of SocketReaction)

    Public Function JudgeAsync(sourceContext As IDerpContext, reaction As SocketReaction) As Task(Of Boolean) Implements ICriteria(Of SocketReaction).JudgeAsync
        Return Task.FromResult(reaction.UserId = sourceContext.User.Id)
    End Function
End Class
