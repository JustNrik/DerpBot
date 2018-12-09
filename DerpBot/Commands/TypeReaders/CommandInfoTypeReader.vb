Option Compare Text
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection

Public Class CommandInfoTypeReader
    Inherits TypeReader

    Public Overrides Function ReadAsync(context As ICommandContext, input As String, services As IServiceProvider) As Task(Of TypeReaderResult)
        Dim commands = services.GetService(Of CommandService)
        Dim cmds = commands.Commands
        Dim targetCmds = cmds.Where(Function(x) x.Name = input)
        targetCmds = If(targetCmds.Count() > 0, targetCmds, cmds.Where(Function(x) x.Name.Contains(input)))
        Return If(targetCmds.Count() = 0,
            Task.FromResult(TypeReaderResult.FromError(New FailedResult("No commands found", False, CommandError.UnknownCommand))),
            Task.FromResult(TypeReaderResult.FromSuccess(targetCmds.Where(Function(x) x.CheckPreconditionsAsync(context, services).Result.IsSuccess))))
    End Function
End Class
