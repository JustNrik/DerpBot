Public Interface ICriteria(Of In T)
    Function JudgeAsync(sourceContext As IDerpContext, parameter As T) As Task(Of Boolean)
End Interface
