Imports Discord
Imports Qmmands

Public MustInherit Class DerpBase(Of T As {IDerpContext, Class})
    Inherits ModuleBase(Of T)

    Protected Async Function SendEmbedAsync(embed As Embed, Optional options As RequestOptions = Nothing) As Task(Of IUserMessage)
        Return Await ReplyAsync(,, embed, options)
    End Function

    Protected Async Function ReplyAsync(Optional text As String = Nothing,
                                        Optional isTTS As Boolean = False,
                                        Optional embed As Embed = Nothing,
                                        Optional options As RequestOptions = Nothing) As Task(Of IUserMessage)

        Return Await Context.Channel.SendMessageAsync(text, isTTS, embed, options)
    End Function

    Protected Function ReactOkAsync() As Task
        Return Task.FromResult(Context.Message.AddReactionAsync(EmotesDict("ok")))
    End Function

    Protected Function MuteAsync(user As IGuildUser, duration As TimeSpan?) As Task
        Dim channel = TryCast(Context.Channel, ITextChannel)
        If channel Is Nothing Then Return Task.CompletedTask
        If duration IsNot Nothing Then Task.Run(Sub() Task.Delay(duration.Value.Milliseconds).ContinueWith(Sub() UnmuteAsync(user)))
        Return Task.FromResult(channel.AddPermissionOverwriteAsync(user, OverwritePermissions.DenyAll(Context.Channel).Modify(viewChannel:=PermValue.Allow)))
    End Function

    Protected Function UnmuteAsync(user As IGuildUser) As Task
        Dim channel = TryCast(Context.Channel, ITextChannel)
        If channel Is Nothing Then Return Task.CompletedTask
        Return Task.FromResult(channel.AddPermissionOverwriteAsync(user, OverwritePermissions.InheritAll))
    End Function

End Class

Public MustInherit Class DerpBase
    Inherits DerpBase(Of DerpContext)
End Class