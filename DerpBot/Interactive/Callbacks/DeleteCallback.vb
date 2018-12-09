Imports Discord
Imports Discord.WebSocket
Imports Qmmands

Public Class DeleteCallback
    Implements IReactionCallback

    Public ReadOnly Property RunMode As RunMode Implements IReactionCallback.RunMode
        Get
            Return RunMode.Sequential
        End Get
    End Property

    Public ReadOnly Property Criteria As ICriteria(Of SocketReaction) Implements IReactionCallback.Criteria
        Get
            Return New EnsureReactionFromSourceUserCriteria()
        End Get
    End Property

    Public ReadOnly Property Timeout As TimeSpan? Implements IReactionCallback.Timeout
        Get
            Return TimeSpan.FromMinutes(2)
        End Get
    End Property

    Public ReadOnly Property Context As ICommandContext Implements IReactionCallback.Context
    Public ReadOnly Property Interative As InteractiveService
    Public ReadOnly Property Reaction As IEmote
    Public ReadOnly Property Message As IUserMessage

    Public Sub New(context As ICommandContext, interactive As InteractiveService, message As IUserMessage, reaction As IEmote)
        _Context = context
        _Interative = interactive
        _Message = message
        _Reaction = reaction
        _Interative.AddReactionCallback(message, Me)
    End Sub

    Public Sub StartDelayAsync()
        Task.Run(Sub() Task.Delay(Timeout.GetValueOrDefault()).ContinueWith(Sub()
                                                                                Task.Run(Sub() Message.DeleteAsync())
                                                                                Interative.RemoveReactionCallback(Message)
                                                                            End Sub))
    End Sub

    Public Async Function HandleCallbackAsync(reaction As SocketReaction) As Task(Of Boolean) Implements IReactionCallback.HandleCallbackAsync
        If Not reaction.Emote.Equals(reaction) Then Return False
        Await Message.DeleteAsync()
        Interative.RemoveReactionCallback(Message)
        Return True
    End Function
End Class
