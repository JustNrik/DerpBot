Imports System.Math
Public Class DerpRandom
    Inherits Random

    Public Function NextByte() As Byte
        Return CByte(Round(NextDouble() * Byte.MaxValue))
    End Function

    Public Function NextByte(maxValue As Byte) As Byte
        Return CByte(Round(NextDouble() * maxValue))
    End Function

    Public Function NextByte(minValue As Byte, maxValue As Byte) As Byte
        Return CByte(Round(NextDouble() * (maxValue - minValue)) + minValue)
    End Function
End Class
