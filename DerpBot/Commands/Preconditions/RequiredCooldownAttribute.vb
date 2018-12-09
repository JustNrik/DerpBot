Imports Discord
Imports System.Collections.Concurrent
Imports Qmmands

Public NotInheritable Class RequiredCooldownAttribute
    Inherits CheckBaseAttribute

    Private ReadOnly _applyPerGuild As Boolean
    Private ReadOnly _invokeLimit As UInteger
    Private ReadOnly _invokeLimitPeriod As TimeSpan
    Private ReadOnly _invokeTracker As New ConcurrentDictionary(Of (ULong, ULong?), CommandTimeout)
    Private ReadOnly _noLimitForAdmins As Boolean
    Private ReadOnly _noLimitInDMs As Boolean

    Public Sub New(times As UInteger, period As Double, measure As Measure, Optional flags As RateLimitFlags = RateLimitFlags.None)
        _invokeLimit = times
        _noLimitInDMs = (flags And RateLimitFlags.NoLimitInDMs) = RateLimitFlags.NoLimitInDMs
        _noLimitForAdmins = (flags And RateLimitFlags.NoLimitForAdmins) = RateLimitFlags.NoLimitForAdmins
        _applyPerGuild = (flags And RateLimitFlags.ApplyPerGuild) = RateLimitFlags.ApplyPerGuild

        Select Case measure
            Case Measure.Days
                _invokeLimitPeriod = TimeSpan.FromDays(period)
            Case Measure.Hours
                _invokeLimitPeriod = TimeSpan.FromHours(period)
            Case Measure.Minutes
                _invokeLimitPeriod = TimeSpan.FromMinutes(period)
            Case Measure.Seconds
                _invokeLimitPeriod = TimeSpan.FromSeconds(period)
        End Select
    End Sub

    Public Overrides Function CheckAsync(context As ICommandContext, provider As IServiceProvider) As Task(Of CheckResult)
        Dim ctx = DirectCast(context, IDerpContext)
        If _noLimitInDMs AndAlso TypeOf ctx.Channel Is IPrivateChannel Then Return Task.FromResult(CheckResult.Successful)

        Dim user = TryCast(ctx.User, IGuildUser)
        If _noLimitForAdmins AndAlso user IsNot Nothing AndAlso user.GuildPermissions.Administrator Then Task.FromResult(CheckResult.Successful)

        Dim now = Date.UtcNow
        Dim key As (ULong, ULong?) = If(_applyPerGuild, (ctx.User.Id, ctx.Guild?.Id), (ctx.User.Id, New ULong?))

        Dim t As CommandTimeout = Nothing
        Dim timeout = If(_invokeTracker.TryGetValue(key, t) AndAlso now - t.FirstInvoke < _invokeLimitPeriod, t, New CommandTimeout(now))

        timeout.TimesInvoked += 1UI

        If timeout.TimesInvoked > _invokeLimit Then Return _
            Task.FromResult(CheckResult.Unsuccessful($"This command is on cooldown please wait {(timeout.FirstInvoke.Add(_invokeLimitPeriod) - Date.UtcNow).Seconds}s"))

        _invokeTracker(key) = timeout
        Return Task.FromResult(CheckResult.Successful)
    End Function

    Private NotInheritable Class CommandTimeout
        Public Property TimesInvoked As UInteger
        Public ReadOnly Property FirstInvoke As Date

        Public Sub New(timeStarted As Date)
            FirstInvoke = timeStarted
        End Sub
    End Class

    Public Enum Measure
        Days
        Hours
        Minutes
        Seconds
    End Enum

    <Flags>
    Public Enum RateLimitFlags
        None
        NoLimitInDMs = 1 << 0
        NoLimitForAdmins = 1 << 1
        ApplyPerGuild = 1 << 2
    End Enum

End Class