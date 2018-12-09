Imports Discord.Commands
Imports Discord.WebSocket

Friend Class EnsureReactionUser
    Implements ICriterion(Of SocketReaction)

    Private ReadOnly _id As ULong

    Public Sub New(id As ULong)
        _id = id
    End Sub

    Public Function JudgeAsync(sourceContext As ICommandContext, reaction As SocketReaction) As Task(Of Boolean) Implements ICriterion(Of SocketReaction).JudgeAsync
        Return Task.FromResult(_id = reaction.UserId)
    End Function
End Class
