Imports DerpBot.DerpCommandResult
Imports Discord
Imports Humanizer
Imports Qmmands
Imports System.IO
Imports SixLabors.ImageSharp
Imports SixLabors.ImageSharp.Formats.Png
Imports SixLabors.ImageSharp.PixelFormats
Imports SixLabors.ImageSharp.Processing
Imports SQLExpress

<RequiredContext(ContextType.Guild)>
Public Class GeneralCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Random As DerpRandom
    Public Property Economy As EcomonyService
    Public Property Database As SQLExpressClient

    <Command("ping", "p")>
    <RunMode(RunMode.Parallel)>
    Async Function Ping() As Task(Of CommandResult)
        Dim sw = Stopwatch.StartNew()
        Dim message = Await ReplyAsync($"Current Shard Latency: {Context.Shard.Latency}ms, Current Ping: ")
        sw.Stop()
        Await message.ModifyAsync(Sub(msg) msg.Content = message.Content & " " & sw.ElapsedMilliseconds & " ms")
        Return Successful
    End Function

    <Command("today")>
    Async Function Today() As Task(Of CommandResult)
        Await ReplyAsync($"Today is {Date.UtcNow}")
        Return Successful
    End Function

    <Command("botinfo", "binfo", "info")>
    Async Function BotInfo() As Task(Of CommandResult)
        Dim embedBuilder As New EmbedBuilder
        With embedBuilder
            .WithColor(New Color(Random.NextByte(), Random.NextByte(), Random.NextByte()))
            .AddField("Bot Owner", "JustNrik")
            .AddField("Uptime", (Date.UtcNow - StartTime).Humanize(4,,, Localisation.TimeUnit.Second))
            .AddSignature(Context.User, Context.Guild)
        End With
        Await SendEmbedAsync(embedBuilder.Build())
        Return Successful
    End Function

    <Command("me")>
    Async Function [Me]() As Task(Of CommandResult)
        Dim guild = Await Database.LoadObjectAsync(Of Guild)(Context.Guild.Id)
        Dim builder As New EmbedBuilder()
        With builder
            .WithAuthor(Context.User.ToString())
            .AddField("Money", Await Economy.GetMoneyAsync(Context.User.Id))
            .AddFieldIf(guild.Admins.Any(Function(id) id = Context.User.Id), "Admin", Context.User.GuildPermissions.ToString())
            .AddFieldIf(guild.Mods.Any(Function(id) id = Context.User.Id), "Mod", Context.User.GuildPermissions.ToString())
            .WithCurrentTimestamp()
        End With
        Await SendEmbedAsync(builder.Build())
        Return Successful
    End Function

    <Command("color")>
    <RunMode(RunMode.Parallel)>
    Async Function Color() As Task(Of CommandResult)
        Dim r = Random.NextByte()
        Dim g = Random.NextByte()
        Dim b = Random.NextByte()
        Dim arg As New Color(r, g, b)
        Dim embedBuilder As New EmbedBuilder()
        With embedBuilder
            .WithColor(arg)
            .AddField("Your color!", $"RGB = {arg.R} {arg.G} {arg.B}{vbCrLf}HEX = #{Convert.ToString(arg.RawValue, 16).ToUpper()}")
            .WithImageUrl("attachment://color.png")
            .AddSignature(Context.User)
        End With
        Using stream As New MemoryStream()
            Using image = SixLabors.ImageSharp.Image.Load("./transparent_200x200.png")
                image.Mutate(Sub(x) x.BackgroundColor(New Rgba32(arg.R, arg.G, arg.B)))
                image.Save(stream, New PngEncoder())
                stream.Position = 0
                Await Context.Channel.SendFileAsync(stream, "color.png", "",, embedBuilder.Build())
                Return Successful
            End Using
        End Using
    End Function

    <Command("color")>
    <RunMode(RunMode.Parallel)>
    Async Function SendColorAsync(r As Byte, g As Byte, b As Byte) As Task(Of CommandResult)
        Dim arg As New Color(r, g, b)
        Dim embedBuilder As New EmbedBuilder()
        With embedBuilder
            .WithColor(arg)
            .AddField("Your color!", $"RGB = {arg.R} {arg.G} {arg.B}{vbCrLf}HEX = #{Convert.ToString(arg.RawValue, 16).ToUpper()}")
            .WithImageUrl("attachment://color.png")
            .AddSignature(Context.User)
        End With
        Using stream As New MemoryStream()
            Using image = SixLabors.ImageSharp.Image.Load("./transparent_200x200.png")
                image.Mutate(Sub(x) x.BackgroundColor(New Rgba32(arg.R, arg.G, arg.B)))
                image.Save(stream, New PngEncoder())
                stream.Position = 0
                Await Context.Channel.SendFileAsync(stream, "color.png", "",, embedBuilder.Build())
                Return Successful
            End Using
        End Using
    End Function

    <Command("color")>
    <RunMode(RunMode.Parallel)>
    Async Function SendColorAsync(value As String) As Task(Of CommandResult)
        Dim result As UInteger
        If UInteger.TryParse(value, Globalization.NumberStyles.AllowHexSpecifier, Nothing, result) Then
            Dim randomColor As New Color(result)
            Dim embedBuilder As New EmbedBuilder()
            With embedBuilder
                .WithColor(randomColor)
                .AddField("Your color!", $"RGB = {randomColor.R} {randomColor.G} {randomColor.B}{vbCrLf}HEX = #{Convert.ToString(randomColor.RawValue, 16).ToUpper()}")
                .WithImageUrl("attachment://color.png")
                .AddSignature(Context.User)
            End With
            Using stream As New MemoryStream()
                Using image = SixLabors.ImageSharp.Image.Load("./transparent_200x200.png")
                    image.Mutate(Sub(x) x.BackgroundColor(New Rgba32(randomColor.R, randomColor.G, randomColor.B)))
                    image.Save(stream, New PngEncoder())
                    stream.Position = 0
                    Await Context.Channel.SendFileAsync(stream, "color.png", "",, embedBuilder.Build())
                    Return Successful
                End Using
            End Using
        End If
        Await ReplyAsync("Invalid hex")
        Return Unsuccessful
    End Function

End Class
