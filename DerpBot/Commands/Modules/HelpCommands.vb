Option Compare Text

Imports DerpBot.DerpCommandResult
Imports Discord
Imports Discord.Format
Imports SQLExpress
Imports Qmmands

<Group("help")>
Public Class HelpCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Commands As CommandService
    Public Property Database As SQLExpressClient
    Public Property Services As IServiceProvider
    Public Property Interactive As InteractiveService

    <Command>
    Async Function Help() As Task(Of CommandResult)
        Dim modules = _Commands.GetAllModules().Where(Function(x) x.Name <> "Help")
        Dim canExecute As New List(Of [Module])

        For Each [module] In modules
            If (Await [module].RunChecksAsync(Context, Services)).IsSuccessful Then canExecute.Add([module])
        Next

        Dim builder = Await GetBuilderAsync()
        With builder
            .WithFooter($"You can view help on a specific module by doing {Await GetPrefixAsync()}help module")
            .AddField("Modules", String.Join(", ", canExecute.Select(Function(x) $"`{Sanitize(x.Name)}`")))
            Await (Await SendEmbedAsync(.Build())).AddDeleteCallbackAsync(Context, Interactive)
        End With
        Return Successful
    End Function

    <Command>
    <Priority(1)>
    Async Function Help(<Remainder> [module] As [Module]) As Task(Of CommandResult)
        Dim commands = [module].Commands
        Dim canExecute As New List(Of Command)
        For Each cmd In commands
            If (Await cmd.RunChecksAsync(Context, Services)).IsSuccessful Then canExecute.Add(cmd)
        Next

        If canExecute.Count = 0 Then
            Await ReplyAsync("You can't execute any commands in this module")
            Return Unsuccessful
        End If

        Dim remarks = [module].Attributes.OfType(Of RemarksAttribute).FirstOrDefault()
        Dim builder = Await GetBuilderAsync()
        With builder
            .WithFooter($"You can view help on a specific command by doing {Await GetPrefixAsync()}help command")
            .AddField($"{[module].Name} Information", $"**Summary**: {[module].Description}" &
                      $"{If(remarks Is Nothing, "", $"{vbCrLf}** remarks **: {String.Join(", ", remarks.Remarks)}")}")
            .AddField("Commands", String.Join(", ", canExecute.Select(Function(x) $"`{Sanitize(x.Aliases.FirstOrDefault())}`")))
            Await (Await SendEmbedAsync(.Build())).AddDeleteCallbackAsync(Context, Interactive)
        End With
        Return Successful
    End Function

    <Command>
    <Priority(2)>
    Async Function Help(<Remainder> commands As IEnumerable(Of Command)) As Task(Of CommandResult)
        Dim builder = Await GetBuilderAsync()
        With builder
            For Each cmd In commands
                Dim usage = cmd.Attributes.OfType(Of RemarksAttribute).FirstOrDefault
                .AddField(cmd.Name, $"**Usage**: {Await GetPrefixAsync()}{usage?.Remarks}" & vbCrLf &
                                        $"**Summary**: {cmd.Description}" &
                                        $"{If(cmd.Aliases.Count > 1, $"{vbCrLf}** Aliases **:  {String.Join(", ", cmd.Aliases.Select(Function(x) $"`{Sanitize(x)}`"))}", "")}")
            Next
            Await (Await SendEmbedAsync(.Build())).AddDeleteCallbackAsync(Context, Interactive)
        End With
        Return Successful
    End Function

    Async Function GetPrefixAsync() As Task(Of String)
        Return (Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)).Prefixes.First
    End Function

    Async Function GetBuilderAsync() As Task(Of EmbedBuilder)
        Return New EmbedBuilder With
            {
                .Color = New Color(255, 255, 39),
                .Title = "Derp Bot's help",
                .Author = New EmbedAuthorBuilder With
                {
                    .IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    .Name = Context.User.ToString()
                },
                .ThumbnailUrl = Context.Guild.CurrentUser.GetAvatarOrDefaultUrl(),
                .Timestamp = DateTimeOffset.UtcNow,
                .Description = $"Hello, my name is Derp Bot{EmotesDict("rooderp")}! You can invoke my commands either by mentioning me or using the `{Sanitize(Await GetPrefixAsync())}` prefix!"
            }
    End Function
End Class
