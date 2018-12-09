Imports Discord
Imports System.ComponentModel
Imports Qmmands

Public Class EnsureFromUserCriteria
    Implements ICriteria(Of IMessage)

    Private ReadOnly _id As ULong

    Public Sub New(user As IUser)
        _id = user.Id
    End Sub

    <EditorBrowsable(EditorBrowsableState.Never)>
    Public Sub New(id As ULong)
        _id = id
    End Sub

    Public Function JudgeAsync(sourceContext As IDerpContext, parameter As IMessage) As Task(Of Boolean) Implements ICriteria(Of IMessage).JudgeAsync
        Return Task.FromResult(_id = parameter.Author.Id)
    End Function
End Class
