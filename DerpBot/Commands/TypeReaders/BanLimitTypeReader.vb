Imports Discord.Commands

Public Class BanLimitTypeReader
    Inherits TypeReader

    Public Overrides Function ReadAsync(context As ICommandContext, input As String, services As IServiceProvider) As Task(Of TypeReaderResult)
        Dim num As UInteger
        If Not UInteger.TryParse(input, num) Then Return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Prune amount must be an integer input > 0"))
        If num > 7 Then Return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Prune amount cannot be > 7"))
        Return Task.FromResult(TypeReaderResult.FromSuccess(CInt(num)))
    End Function
End Class
