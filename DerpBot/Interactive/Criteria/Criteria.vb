Public Class Criteria(Of T)
    Implements ICriteria(Of T)

    Private ReadOnly _critiera As New List(Of ICriteria(Of T))

    Public Function AddCriteria(criteria As ICriteria(Of T)) As Criteria(Of T)
        _critiera.Add(criteria)
        Return Me
    End Function

    Public Async Function JudgeAsync(sourceContext As IDerpContext, parameter As T) As Task(Of Boolean) Implements ICriteria(Of T).JudgeAsync
        For Each criteria In _critiera
            If Not Await criteria.JudgeAsync(sourceContext, parameter) Then Return False
        Next
        Return True
    End Function
End Class
