Imports SQLExpress

Public Class Tag
    Inherits SQLObject

    <Store, StringLength(50)>
    Public Property TagName As String
    <Store, StringLength(1000)>
    Public Property TagValue As String
    <Store, NotNull>
    Public Property TagOwner As ULong
    <Store, NotNull>
    Public Property CreatedAt As Date
    <Store, NotNull>
    Public Property Uses As Integer

    Public Overrides ReadOnly Property TableName As String
        Get
            Return "tags"
        End Get
    End Property
End Class
