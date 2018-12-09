Imports SQLExpress
Imports Qmmands

<RequiredOwner>
<RunMode(RunMode.Parallel)>
Public Class OwnerCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Eval As EvalService
    Public Property Database As SQLExpressClient

    <Command("csrun")>
    Function CSRun(<Remainder> code As String) As Task
        Return Eval.RunCSharpScriptAsync(code, Context)
    End Function

    <Command("cseval")>
    Function CSEval(<Remainder> code As String) As Task
        Return Eval.EvaluateCSharpScriptAsync(code, Context)
    End Function
#Const VBScripting = True
#If VBScripting Then
    <Command("vbrun")>
    Function VBRun(<Remainder> code As String) As Task
        Return Eval.RunVBasicScriptAsync(code, Context)
    End Function

    <Command("vbeval")>
    Function VBEval(<Remainder> code As String) As Task
        Return Eval.EvaluateVBasicScriptAsync(code, Context)
    End Function
#End If

    <Command("globalannoucement", "ga")>
    Async Function GlobalAnnoucement(<Remainder> text As String) As Task
        Dim guildIds = Database.YieldData(Of ULong)("SELECT Id FROM guilds WHERE AnnouncementChannel > 0;")
        For Each id In guildIds
            Dim guildObj = Await Database.LoadObjectAsync(Of Guild)(id)
            Dim guild = Context.Client.GetGuild(id)
            Dim channel = guild.TextChannels.FirstOrDefault(Function(x) x.Id = guildObj.AnnouncementChannel.Value)
            If channel Is Nothing Then Continue For
            Await channel.SendMessageAsync(text)
        Next
    End Function

    <Command("memoryusage", "memusage", "getmem")>
    Function GetMemoryUsage(Optional bool As Boolean = False) As Task
        Return ReplyAsync($"{GC.GetTotalMemory(bool) \ (2 << 19)} MBs")
    End Function
End Class
