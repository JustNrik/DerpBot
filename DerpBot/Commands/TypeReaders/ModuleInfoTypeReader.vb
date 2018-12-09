Option Compare Text
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection

Public Class ModuleInfoTypeReader
    Inherits TypeReader

    Public Overrides Async Function ReadAsync(context As ICommandContext, input As String, services As IServiceProvider) As Task(Of TypeReaderResult)
        Dim commands = services.GetService(Of CommandService)
        Dim modules = commands.Modules
        Dim targetModule = modules.FirstOrDefault(Function(x) x.Name = input)
        Return If(targetModule Is Nothing,
        TypeReaderResult.FromError(New FailedResult("Module not found", False, CommandError.ObjectNotFound)),
        If(((Await targetModule.CheckPermissionsAsync(context, services)).IsSuccess),
        TypeReaderResult.FromSuccess(targetModule),
        TypeReaderResult.FromError(New FailedResult("You don't have permission to use this module", False, CommandError.UnmetPrecondition))))
    End Function
End Class
