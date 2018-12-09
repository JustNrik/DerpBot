Option Compare Text
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection
Public Class MoneyTypeReader
    Inherits TypeReader

    Public Overrides Function ReadAsync(context As ICommandContext, input As String, services As IServiceProvider) As Task(Of TypeReaderResult)
        Dim economy = services.GetService(Of EcomonyService)
        Dim user = economy.GetMoneyAsync(context.User.Id)
        If input = "all" Then Return Task.FromResult(TypeReaderResult.FromSuccess(user))

        Dim amount As Integer

        If Not Integer.TryParse(input, amount) Then Return Task.FromResult(TypeReaderResult.FromError(New FailedResult("Failed to parse amount", False, CommandError.ParseFailed)))
        If amount < 0 Then Return Task.FromResult(TypeReaderResult.FromError(New FailedResult("Requires a positive integer", False, CommandError.Unsuccessful)))
        If amount > user Then Return Task.FromResult(TypeReaderResult.FromError(New FailedResult("You don't have enough candies", False, CommandError.Unsuccessful)))
        Return Task.FromResult(TypeReaderResult.FromSuccess(amount))
    End Function
End Class