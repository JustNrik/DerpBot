Imports Discord
Imports Qmmands

Public Class EnsureFromChannelCriteria
    Implements ICriteria(Of IMessage)

    Private ReadOnly _channelId As ULong

    Public Sub New(channel As IMessageChannel)
        _channelId = channel.Id
    End Sub

    Public Function JudgeAsync(sourceContext As IDerpContext, parameter As IMessage) As Task(Of Boolean) Implements ICriteria(Of IMessage).JudgeAsync
        Return Task.FromResult(_channelId = parameter.Channel.Id)
    End Function
End Class
