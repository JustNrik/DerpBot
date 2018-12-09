Imports Discord.Commands
Imports Discord.WebSocket

Public Interface IReactionCallback
    ReadOnly Property RunMode As RunMode
    ReadOnly Property Criterion As ICriterion(Of SocketReaction)
    ReadOnly Property Timeout As TimeSpan?
    ReadOnly Property Context As ICommandContext
    Function HandleCallbackAsync(reaction As SocketReaction) As Task(Of Boolean)
End Interface