Imports Discord.Commands

Public Class TimespanTypeReader
    Inherits TypeReader

    Public Overrides Function ReadAsync(context As ICommandContext, input As String, services As IServiceProvider) As Task(Of TypeReaderResult)

        Dim d, h, m, s As Integer

        If input.Contains("d"c) Then
            d = Integer.Parse(input.Substring(0, input.IndexOf("d"c) - 1))
            input = input.Replace("days", "d").Replace("day", "d").Substring(input.IndexOf("d"c))
        End If
        If input.Contains("h"c) Then
            h = Integer.Parse(input.Substring(0, input.IndexOf("h"c) - 1))
            input = input.Replace("hours", "h").Replace("hour", "h").Substring(input.IndexOf("h"c))
        End If
        If input.Contains("m"c) Then
            m = Integer.Parse(input.Substring(0, input.IndexOf("m"c) - 1))
            input = input.Replace("minutes", "m").Replace("minute", "m").Substring(input.IndexOf("m"c))
        End If
        If input.Contains("s"c) Then
            s = Integer.Parse(input.Substring(0, input.IndexOf("s"c) - 1))
            input = input.Replace("seconds", "s").Replace("second", "s").Substring(input.IndexOf("s"c))
        End If

        If d = 0 AndAlso h = 0 AndAlso m = 0 AndAlso s = 0 Then Return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid timespan string"))

        Dim result = New TimeSpan(d, h, m, s)
        Return Task.FromResult(TypeReaderResult.FromSuccess(result))

    End Function

End Class
