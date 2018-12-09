Imports Discord.WebSocket
Imports Qmmands

Friend Class EnsureReactionUserCriteria
    Implements ICriteria(Of SocketReaction)

    Private ReadOnly _id As ULong

    Public Sub New(id As ULong)
        _id = id
    End Sub

    Public Function JudgeAsync(sourceContext As IDerpContext, reaction As SocketReaction) As Task(Of Boolean) Implements ICriteria(Of SocketReaction).JudgeAsync
        Return Task.FromResult(_id = reaction.UserId)
    End Function
End Class
