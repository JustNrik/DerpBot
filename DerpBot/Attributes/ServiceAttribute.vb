<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
Public Class ServiceAttribute
    Inherits Attribute

    Public Property Type As ServiceType
    Public Property Priority As Byte

    Sub New(type As ServiceType, priority As Byte)
        _Type = type
        _Priority = priority
    End Sub
End Class
