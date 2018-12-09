Imports DerpBot.DerpCommandResult
Imports SQLExpress
Imports Qmmands

<RequiredOwner>
<RunMode(RunMode.Parallel)>
Public Class OwnerCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Eval As EvalService
    Public Property Database As SQLExpressClient

    <Command("csrun")>
    Async Function CSRun(<Remainder> code As String) As Task(Of CommandResult)
        Await Eval.RunCSharpScriptAsync(code, Context)
        Return Successful
    End Function

    <Command("cseval")>
    Async Function CSEval(<Remainder> code As String) As Task(Of CommandResult)
        Await Eval.EvaluateCSharpScriptAsync(code, Context)
        Return Successful
    End Function
#Const VBScripting = True
#If VBScripting Then
    <Command("vbrun")>
    Async Function VBRun(<Remainder> code As String) As Task(Of CommandResult)
        Await Eval.RunVBasicScriptAsync(code, Context)
        Return Successful
    End Function

    <Command("vbeval")>
    Async Function VBEval(<Remainder> code As String) As Task(Of CommandResult)
        Await Eval.EvaluateVBasicScriptAsync(code, Context)
        Return Successful
    End Function
#End If

    <Command("globalannoucement", "ga")>
    Async Function GlobalAnnoucement(<Remainder> text As String) As Task(Of CommandResult)
        Dim guildIds = Database.YieldData(Of ULong)("SELECT Id FROM guilds WHERE AnnouncementChannel > 0;")
        For Each id In guildIds
            Dim guildObj = Await Database.LoadObjectAsync(Of Guild)(id)
            Dim guild = Context.Client.GetGuild(id)
            Dim channel = guild.TextChannels.FirstOrDefault(Function(x) x.Id = guildObj.AnnouncementChannel.Value)
            If channel Is Nothing Then Continue For
            Await channel.SendMessageAsync(text)
        Next
        Return Successful
    End Function

    <Command("memoryusage", "memusage", "getmem")>
    Async Function GetMemoryUsage(Optional bool As Boolean = False) As Task(Of CommandResult)
        Await ReplyAsync($"{GC.GetTotalMemory(bool) \ (2 << 19)} MBs")
        Return Successful
    End Function
End Class
