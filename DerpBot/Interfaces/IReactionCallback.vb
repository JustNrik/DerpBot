Imports Discord.WebSocket
Imports Qmmands

Public Interface IReactionCallback
    ReadOnly Property RunMode As RunMode
    ReadOnly Property Criteria As ICriteria(Of SocketReaction)
    ReadOnly Property Timeout As TimeSpan?
    ReadOnly Property Context As ICommandContext
    Function HandleCallbackAsync(reaction As SocketReaction) As Task(Of Boolean)
End Interface