Imports Discord

Public Class PaginatedAppearanceOptions
    Public Shared [Default] As New PaginatedAppearanceOptions
    Public ReadOnly Property First As IEmote = New Emoji("⏮")
    Public ReadOnly Property Back As IEmote = New Emoji("◀")
    Public ReadOnly Property [Next] As IEmote = New Emoji("▶")
    Public ReadOnly Property Last As IEmote = New Emoji("⏭")
    Public ReadOnly Property [Stop] As IEmote = New Emoji("⏹")
    Public ReadOnly Property Jump As IEmote = New Emoji("🔢")
    Public ReadOnly Property Info As IEmote = New Emoji("ℹ")
    Public Const FooterFormat = "Page {0}/{1}"
    Public Const InformationText = "This is a paginator. React with the respective icons to change page."
    Public JumpDisplayOptions As JumpDisplayOptions = JumpDisplayOptions.WithManageMessages
    Public DisplayInformationIcon As Boolean = True
    Public Timeout As TimeSpan?
    Public InfoTimeout As TimeSpan = TimeSpan.FromSeconds(30)
End Class

Public Enum JumpDisplayOptions
    Never
    WithManageMessages
    Always
End Enum
