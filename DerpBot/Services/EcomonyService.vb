Imports SQLExpress

<Service(ServiceType.Singleton, 0)>
Public Class EcomonyService
    Implements IService

    Private ReadOnly _db As SQLExpressClient
    Private ReadOnly _random As DerpRandom
    Private ReadOnly _log As LogService

    Sub New(db As SQLExpressClient, random As DerpRandom, log As LogService)
        _db = db
        _random = random
        _log = log
    End Sub

    Public Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Return Task.FromResult(True)
    End Function

    Public Async Function AddMoneyAsync(user As User, amount As Integer) As Task
        If amount = 0 Then Return
        If user.Money + amount > Integer.MaxValue Then user.Money = Integer.MaxValue
        If user.Money + amount < 0 Then user.Money = 0
        Await _db.UpdateObjectAsync(user)
    End Function

    Public Async Function ClaimDailyAsync(user As User) As Task
        If user.LastClaimed.AddDays(1) < Date.UtcNow Then Return
        user.LastClaimed = Date.UtcNow
        Dim amount = _random.Next(1, 10)
        Await AddMoneyAsync(user, amount)
        Await _log.LogAsync($"The user {user.Id} has claimed {amount} money!", LogSource.Economy)
    End Function

    Public Async Function GetMoneyAsync(id As ULong) As Task(Of Integer)
        Return (Await _db.LoadObjectAsync(Of User)(id)).Money
    End Function

End Class
