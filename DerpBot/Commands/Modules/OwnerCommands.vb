﻿Imports DerpBot.DerpCommandResult
Imports SqlExpress
Imports Qmmands
Imports Discord

<RequiredOwner>
<RunMode(RunMode.Parallel)>
Public Class OwnerCommands
    Inherits DerpBase

    Public Property Eval As EvalService
    Public Property Database As SqlExpressClient

    <Command("csrun", "cseval")>
    Async Function CSRun(<Remainder> code As String) As Task(Of CommandResult)
        Await Eval.RunCSharpScriptAsync(code, Context)
        Return Successful
    End Function
#Const VBScripting = True
#If VBScripting Then
    <Command("vbrun", "vbeval")>
    Async Function VBRun(<Remainder> code As String) As Task(Of CommandResult)
        Await Eval.RunVBasicScriptAsync(code, Context)
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
    Async Function GetMemoryUsage() As Task(Of CommandResult)
        Using proc = Process.GetCurrentProcess()
            Dim builder As New EmbedBuilder

            With builder
                .AddField("Private Memory Size", $"{proc.PrivateMemorySize64 \ (2 << 19)} MBs")
                .AddField("Working Set", $"{proc.WorkingSet64 \ (2 << 19)} MBs")
                .AddField("Current Thread Memory", $"{GC.GetAllocatedBytesForCurrentThread() \ (2 << 19)} MBs")
                .AddField("Heap Allocation", $"{GC.GetTotalMemory(True) \ (2 << 19)} MBs")
            End With

            Await SendEmbedAsync(builder.Build())
        End Using
        Return Successful
    End Function

    <Command("gccollect")>
    Async Function GCCollect() As Task(Of CommandResult)
        GC.Collect()
        Await ReactOkAsync()
        Return Successful
    End Function
End Class
