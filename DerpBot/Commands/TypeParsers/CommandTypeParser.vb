Option Compare Text

Imports Microsoft.Extensions.DependencyInjection
Imports Qmmands

Public Class CommandTypeParser
    Inherits TypeParser(Of IEnumerable(Of Command))

    Public Overrides Async Function ParseAsync(value As String, context As ICommandContext, provider As IServiceProvider) As Task(Of TypeParserResult(Of IEnumerable(Of Command)))
        Dim service = provider.GetService(Of CommandService)
        Dim commands = service.GetAllCommands()
        Dim matching = From cmd In commands
                       Where cmd.Name = value OrElse
                             cmd.Name.IndexOf(value) >= 0 OrElse
                             cmd.Aliases.Any(Function(x) x.IndexOf(value) >= 0)

        Dim canExecute As New List(Of Command)

        For Each cmd In matching
            If (Await cmd.RunChecksAsync(context, provider)).IsSuccessful AndAlso cmd.Name <> "help" Then canExecute.Add(cmd)
        Next

        Return If(canExecute.Count = 0,
            TypeParserResult(Of IEnumerable(Of Command)).Unsuccessful("Failed to find any command"),
            TypeParserResult(Of IEnumerable(Of Command)).Successful(canExecute))
    End Function
End Class
