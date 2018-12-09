Imports Qmmands

Public NotInheritable Class DerpCommandResult
    Inherits CommandResult

    Public Shared ReadOnly Successful As New DerpCommandResult(True)
    Public Shared ReadOnly Unsuccessful As New DerpCommandResult(False)
    Public Overrides ReadOnly Property IsSuccessful As Boolean

    Sub New(isSuccessful As Boolean)
        _IsSuccessful = isSuccessful
    End Sub

End Class
