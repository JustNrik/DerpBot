Imports System.Text.RegularExpressions
Imports Qmmands

Public NotInheritable Class TimeSpanTypeParser
    Inherits TypeParser(Of TimeSpan)

    Private Shared ReadOnly TimeSpanRegex As New Regex("(\d+)(w(?:eeks?)?|d(?:ays?)?|h(?:ours?)?|m(?:inutes?)?|s(?:econds?)?)", RegexOptions.Compiled)

    Public Overrides Function ParseAsync(value As String, context As ICommandContext, provider As IServiceProvider) As Task(Of TypeParserResult(Of TimeSpan))
        Dim matches = TimeSpanRegex.Matches(value)
        If matches.Count <= 0 Then Return Task.FromResult(New TypeParserResult(Of TimeSpan)("Failed to parse time span"))
        Dim result As New TimeSpan()
        Dim weeks As Boolean, days As Boolean, hours As Boolean,
            minutes As Boolean, seconds As Boolean, amount As UInteger

        For m = 0 To matches.Count - 1
            Dim match = matches(m)
            If Not UInteger.TryParse(match.Groups(1).Value, amount) Then Continue For
            Dim character = match.Groups(2).Value(0)
            Select Case character
                Case "w"c
                    If Not weeks Then
                        result = result.Add(TimeSpan.FromDays(amount * 7))
                        weeks = True
                    End If
                    Continue For
                Case "d"c
                    If Not days Then
                        result = result.Add(TimeSpan.FromDays(amount))
                        days = True
                    End If
                    Continue For
                Case "h"c
                    If Not hours Then
                        result = result.Add(TimeSpan.FromHours(amount))
                        hours = True
                    End If
                    Continue For
                Case "m"c
                    If Not minutes Then
                        result = result.Add(TimeSpan.FromMinutes(amount))
                        minutes = True
                    End If
                    Continue For
                Case "s"c
                    If Not seconds Then
                        result = result.Add(TimeSpan.FromSeconds(amount))
                        seconds = True
                    End If
                    Continue For
            End Select
        Next

        Return Task.FromResult(If(result > TimeSpan.FromSeconds(10), New TypeParserResult(Of TimeSpan)(result), New TypeParserResult(Of TimeSpan)("Time span must be greater than 10 seconds")))
    End Function
End Class
