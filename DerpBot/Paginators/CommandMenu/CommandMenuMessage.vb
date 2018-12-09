Imports Discord
Imports Discord.Commands

Public Class CommandMenuMessage
    Inherits BasePaginator

    Public Emojis As Dictionary(Of String, Emoji) = New Dictionary(Of String, Emoji)
    Public CommandsDictionary As Dictionary(Of ModuleInfo, IEnumerable(Of CommandInfo))
    Public MoveUp As Emoji = New Emoji("⬆")
    Public MoveDown As Emoji = New Emoji("⬇")
    Public [Select] As Emoji = New Emoji("✅")
    Public Back As Emoji = New Emoji("🔙")
    Public Delete As Emoji = New Emoji("❌")
    Public Sub New(ByVal dict As Dictionary(Of ModuleInfo, IEnumerable(Of CommandInfo)))
        Emojis.Add("up", MoveUp)
        Emojis.Add("down", MoveDown)
        Emojis.Add("select", [Select])
        Emojis.Add("back", Back)
        Emojis.Add("delete", Delete)
        CommandsDictionary = dict
    End Sub
End Class