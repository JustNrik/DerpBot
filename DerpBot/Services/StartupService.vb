Imports Discord.WebSocket
Imports SQLExpress
Imports System.Reflection
Imports Qmmands

<Service(ServiceType.Singleton, 255)>
Public Class StartupService
    Implements IService

    Private ReadOnly _log As LogService
    Private ReadOnly _services As IServiceProvider

    Sub New(services As IServiceProvider, log As LogService)
        _services = services
        _log = log
    End Sub

    Public Async Function InitializeAsync() As Task(Of Boolean) Implements IService.InitializeAsync
        Dim services = From type In Assembly.GetEntryAssembly().GetTypes()
                       Where GetType(IService).IsAssignableFrom(type) AndAlso
                            type.GetCustomAttribute(Of ServiceAttribute)(True) IsNot Nothing AndAlso
                            type IsNot GetType(StartupService)
                       Order By type.GetCustomAttribute(Of ServiceAttribute).Priority Descending
                       Select type

        For Each service In services
            Await _log.LogAsync($"Initializing {service.Name}...", LogSource.Service)
            If Await DirectCast(_services.GetService(service), IService).InitializeAsync() Then
                Await _log.LogAsync($"{service.Name} has been initialized successfully", LogSource.Service)
            Else
                Await _log.LogAsync($"there was an error initializing {service.Name}", LogSource.ServiceError)
            End If
        Next
        Return True
    End Function

End Class
