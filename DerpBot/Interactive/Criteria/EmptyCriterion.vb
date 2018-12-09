Imports Qmmands

Public Class EmptyCriteria(Of T)
    Implements ICriteria(Of T)

    Public Function JudgeAsync(sourceContext As IDerpContext, parameter As T) As Task(Of Boolean) Implements ICriteria(Of T).JudgeAsync
        Return Task.FromResult(True)
    End Function
End Class
