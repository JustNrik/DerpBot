Imports Discord

Public Module Helpers
    Public Const DEFAULT_LOGGING_LENGTH = 9
    Public Property EmotesDict As New Dictionary(Of String, IEmote) From
        {
            {"rooderp", Emote.Parse("<:rooderp:500815075611901952>")},
            {"roothink", Emote.Parse("<:roothink:500815014282788897>")},
            {"star", New Emoji("⭐")}, {"ok", New Emoji("👌🏻")}
        }

    Public Enum ContextType
        Guild
        DM
    End Enum

    Public Function HasMentionPrefix(input As String, ByRef output As String) As Boolean
        output = input

        If Not input.StartsWith("<@!447495504834723850>") Then Return False

        output = input.Substring(22)

        Return True
    End Function


    Public Function ParseSource(source As LogSource) As String
        Select Case source
            Case LogSource.GuildAvailable To LogSource.GuildMembersDownloaded
                Return "Guild"
            Case LogSource.ShardReady To LogSource.ShardDisconnected
                Return "Shard"
            Case LogSource.Command To LogSource.CommandError
                Return "Command"
            Case LogSource.Service To LogSource.ServiceError
                Return "Service"
            Case LogSource.Economy
                Return "Economy"
        End Select
        Return Nothing
    End Function

    Public Function Center(input As String, Optional totalLength As Integer = DEFAULT_LOGGING_LENGTH, Optional paddingChar As Char = " "c) As String
        If input Is Nothing Then Return Nothing
        If input.Length > totalLength Then Throw New ArgumentOutOfRangeException("The input is larger than the limit set")
        Return input.PadLeft((totalLength - input.Length) \ 2 + input.Length, paddingChar).PadRight(totalLength, paddingChar)
    End Function

    Public Function CalcLevenshteinDistance(a As String, b As String) As Integer
        Dim x = 0
        Dim LengthA = a.Length
        Dim LengthB = b.Length
        Dim m = New Integer(LengthA, LengthB) {}
        For i As Integer = 0 To LengthA
            m(i, 0) = i
        Next
        For i As Integer = 0 To LengthB
            m(0, i) = i
        Next
        For indexA As Integer = 1 To LengthA
            For indexB As Integer = 1 To LengthB
                x = If(a(indexA - 1) = b(indexB - 1), 0, 1)
                m(indexA, indexB) = Math.Min(Math.Min(m(indexA - 1, indexB) + 1, m(indexA, indexB - 1) + 1), m(indexA - 1, indexB - 1) + x)
            Next
        Next
        Return m(LengthA, LengthB)
    End Function
End Module