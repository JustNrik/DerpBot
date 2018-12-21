Imports Discord
Imports Discord.WebSocket
Imports Microsoft.CodeAnalysis.CSharp.Scripting
Imports Microsoft.CodeAnalysis.Scripting
#Const VBScripting = True
#If VBScripting Then
Imports Microsoft.CodeAnalysis.VisualBasic.Scripting
Imports SqlExpress
#End If
Imports System.Reflection
<Service(ServiceType.Singleton, 0)>
Public Class EvalService
    Implements IService

    Private ReadOnly _message As MessageService
    Private ReadOnly _provider As IServiceProvider
    Private ReadOnly _namespaces As String() =
        {
            "Discord", "Qmmands", "Discord.WebSocket",
            "System", "System.Diagnostics", "System.Linq", "DerpBot",
            "System.Collections.Generic", "System.Net", "SqlExpress",
            "System.Threading.Tasks", "Microsoft.Extensions.DependencyInjection"
        }
    Private ReadOnly _references As Assembly() =
        {
            GetType(IDiscordClient).Assembly, GetType(DiscordSocketClient).Assembly, GetType(DerpBot).Assembly, GetType(SqlExpressClient).Assembly
        }
    Private Const CSHARP_LOGO = "https://camo.githubusercontent.com/0617f4657fef12e8d16db45b8d73def73144b09f/68747470733a2f2f646576656c6f7065722e6665646f726170726f6a6563742e6f72672f7374617469632f6c6f676f2f6373686172702e706e67"
#If VBScripting Then
    Private Const VBASIC_LOGO = "https://png2.kisspng.com/sh/419e5f9e62b041dd96cdfd62dc8ea23e/L0KzQYm3WMA0N6Z6hJH0aYP2gLBuTgZqe6ZmhJ9rYYPsc371hgQua15oh995dYTogn73kv9oepJyhdt3Zz3xdcW0hwJiNZ5ueAR4c3BphH7omwVzbV54fd5uY4TzdLe0gB9uNWZnTqYBMUi1dYTpWMI6Nmk6TKUDOEG6QYa6U8MxPWM2UKs8MkixgLBu/kisspng-visual-basic-net-c-computer-programming-net-fra-microsoft-azure-selectpdf-com-5b646182e3b829.8543881715333052189328.png"
#End If

    Sub New(message As MessageService, provider As IServiceProvider)
        _message = message
        _provider = provider
    End Sub

    Public Async Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Try
            Dim CSharpReady = (Await CSharpScript.RunAsync(Of Boolean)("return true;", ScriptOptions.Default.WithReferences(Assembly.GetEntryAssembly()))).ReturnValue
#If VBScripting Then
            Dim VBasicReady = (Await VisualBasicScript.RunAsync(Of Boolean)("Return True", ScriptOptions.Default.WithReferences(Assembly.GetEntryAssembly()))).ReturnValue
            Return CSharpReady AndAlso VBasicReady
#Else
            Return CSharpReady
#End If
        Catch
            Return False
        End Try
    End Function

    Public Async Function RunCSharpScriptAsync(code As String, context As DerpContext) As Task
        Dim globals As New Globals(context, _provider)
        Dim options = ScriptOptions.Default.WithImports(_namespaces).WithReferences(_references)
        Dim script = CSharpScript.Create(code, options, GetType(Globals))
        Try
            Dim sw1 = Stopwatch.StartNew()
            Dim compiled = script.Compile()
            sw1.Stop()
            Dim sw2 = Stopwatch.StartNew()
            Dim state = Await script.RunAsync(globals)
            sw1.Stop()
            Dim eb = New EmbedBuilder().
                    WithTitle("Eval executed successfully").
                    WithColor(Color.Green).
                    WithCurrentTimestamp().
                    WithFooter($"Requested by: {context.User.GetDisplayName()}, Compiled in {sw1.ElapsedMilliseconds}ms, Ran in {sw2.ElapsedMilliseconds}ms", context.User.GetAvatarOrDefaultUrl()).
                    WithThumbnailUrl(CSHARP_LOGO).
                    AddFieldIf(state.ReturnValue IsNot Nothing, "Output", $"```cs{vbLf}{state.ReturnValue}```").
                    AddFieldIf(state.ReturnValue IsNot Nothing, "Return Type", $"```cs{vbLf}[{state.ReturnValue.GetType()}]```")

            Await _message.SendEmbedAsync(context, eb.Build)
        Catch ex As CompilationErrorException
            _message.NewMessageAsync(context, String.Join(vbCrLf, ex.Diagnostics)).ConfigureAwait(False).GetAwaiter().GetResult()
        End Try
    End Function

#If VBScripting Then
    Public Async Function RunVBasicScriptAsync(code As String, context As DerpContext) As Task
        Dim globals As New Globals(context, _provider)
        Try
            Dim sw = Stopwatch.StartNew()
            Dim state = Await VisualBasicScript.RunAsync(code, ScriptOptions.Default.WithImports(_namespaces).WithReferences(Assembly.GetEntryAssembly()), globals)
            sw.Stop()
            Dim eb = New EmbedBuilder().
                    WithTitle("Eval executed successfully").
                    AddField("Input", $"```vb{vbLf}{code}```").
                    WithColor(Color.Green).
                    WithCurrentTimestamp().
                    WithFooter($"Requested by: {context.User.Username}#{context.User.Discriminator}, Compiled in {sw.ElapsedMilliseconds}ms", context.User.GetAvatarOrDefaultUrl()).
                    WithThumbnailUrl(VBASIC_LOGO).
                    AddFieldIf(state.ReturnValue IsNot Nothing, "Output", $"```vb{vbLf}{state.ReturnValue}```")
            Await _message.SendEmbedAsync(context, eb.Build)
        Catch ex As CompilationErrorException
            _message.NewMessageAsync(context, String.Join(vbCrLf, ex.Diagnostics)).ConfigureAwait(False).GetAwaiter().GetResult()
        End Try
    End Function
#End If

End Class
