Imports Discord

Public Class HelpPaginatedMessage
    Inherits BasePaginator

    Public Property Pages As List(Of Page) = New List(Of Page)()
    Public Property Author As EmbedAuthorBuilder
    Public Property Prefix As String
    Public Property Options As PaginatedAppearanceOptions
End Class
