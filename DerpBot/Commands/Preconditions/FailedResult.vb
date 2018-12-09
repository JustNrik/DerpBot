Imports Qmmands

Public Class FailedResult
    Implements IResult
    Public ReadOnly Property IsSuccessful As Boolean Implements IResult.IsSuccessful

    Public Sub New(isSuccessful As Boolean)
        _IsSuccessful = isSuccessful
    End Sub
End Class