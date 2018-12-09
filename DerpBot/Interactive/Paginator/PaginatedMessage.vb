Imports Discord

Public Class PaginatedMessage
    Inherits BasePaginator

    Public Property Pages As IEnumerable(Of Object)
    Public Property Content As String = String.Empty
    Public Property Author As EmbedAuthorBuilder = Nothing
    Public Property Color As Color = Color.Default
    Public Property Title As String = String.Empty
    Public Property Options As PaginatedAppearanceOptions = PaginatedAppearanceOptions.Default
End Class