Option Compare Text

Imports Microsoft.Extensions.DependencyInjection
Imports Qmmands

Public Class ModuleTypeParser
    Inherits TypeParser(Of [Module])

    Public Overrides Async Function ParseAsync(value As String, context As ICommandContext, provider As IServiceProvider) As Task(Of TypeParserResult(Of [Module]))
        Dim commands = provider.GetService(Of CommandService)
        Dim modules = commands.GetAllModules()
        Dim targetModule = modules.FirstOrDefault(Function(x) x.Name = value)

        If targetModule Is Nothing Then Return TypeParserResult(Of [Module]).Unsuccessful("Module not found")

        If Not (Await targetModule.RunChecksAsync(context, provider)).IsSuccessful Then Return _
            TypeParserResult(Of [Module]).Unsuccessful("You can't execute any command in this module")

        Return If(targetModule.Name = "Help",
            TypeParserResult(Of [Module]).Unsuccessful("Module not found"),
            TypeParserResult(Of [Module]).Successful(targetModule))

    End Function
End Class
