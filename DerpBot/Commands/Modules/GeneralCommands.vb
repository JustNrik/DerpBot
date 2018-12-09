Imports System.IO
Imports Discord
Imports Humanizer
Imports SixLabors.ImageSharp
Imports SixLabors.ImageSharp.Formats.Png
Imports SixLabors.ImageSharp.PixelFormats
Imports SixLabors.ImageSharp.Processing
Imports Qmmands

<RequiredContext(ContextType.Guild)>
Public Class GeneralCommands
    Inherits DerpBase(Of DerpContext)

    Public Property Random As DerpRandom

    <Command("ping", "p")>
    <RunMode(RunMode.Parallel)>
    Async Function Ping() As Task
        Dim sw = Stopwatch.StartNew()
        Dim message = Await ReplyAsync($"Current Shard Latency: {Context.Shard.Latency}ms, Current Ping: ")
        sw.Stop()
        Await message.ModifyAsync(Sub(msg) msg.Content = message.Content & " " & sw.ElapsedMilliseconds & " ms")
    End Function

    <Command("today")>
    Function Today() As Task
        Return ReplyAsync($"Today is {Date.UtcNow}")
    End Function

    <Command("botinfo", "binfo", "info")>
    Async Function BotInfo() As Task
        Dim embedBuilder As New EmbedBuilder
        With embedBuilder
            .WithColor(New Color(Random.NextByte(), Random.NextByte(), Random.NextByte()))
            .AddField("Bot Owner", "JustNrik")
            .AddField("Uptime", (Date.UtcNow - StartTime).Humanize(4,,, Localisation.TimeUnit.Second))
            .AddSignature(Context.User, Context.Guild)
        End With
        Await SendEmbedAsync(embedBuilder.Build())
    End Function

    <Command("me")>
    Function [Me]() As Task
        Return ReplyAsync(Context.User.ToString())
    End Function

    <Command("color")>
    <RunMode(RunMode.Parallel)>
    Function Color() As Task
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
                Return Context.Channel.SendFileAsync(stream, "color.png", "",, embedBuilder.Build())
            End Using
        End Using
    End Function

    <Command("color")>
    <RunMode(RunMode.Parallel)>
    Function SendColorAsync(r As Byte, g As Byte, b As Byte) As Task
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
                Return Context.Channel.SendFileAsync(stream, "color.png", "",, embedBuilder.Build())
            End Using
        End Using
    End Function

    <Command("color")>
    <RunMode(RunMode.Parallel)>
    Function SendColorAsync(value As String) As Task
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
                    Return Context.Channel.SendFileAsync(stream, "color.png", "",, embedBuilder.Build())
                End Using
            End Using
        End If
        Return ReplyAsync("Invalid hex")
    End Function

End Class
