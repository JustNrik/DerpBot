Imports Discord
Imports Discord.WebSocket
Imports Microsoft.Extensions.DependencyInjection
Imports Qmmands
Imports SQLExpress
Imports System.Console
Imports System.Reflection
Imports System.Xml

Public Module Program

    Public Const COMMAND_CONFIG = 1336UL
    Public Const BOT_CONFIG = 1337UL
    Public Const CLIENT_CONFIG = 1338UL
    Public Const BOT_OWNER_ID = 304088134352764930UL

    Public ReadOnly StartTime As Date = Date.UtcNow

    Private _client As DiscordShardedClient
    Private _commands As CommandService
    Private _services As IServiceProvider
    Private _db As SQLExpressClient

    Private ReadOnly _assemblyTypes As Type() = Assembly.GetEntryAssembly().GetTypes()

    Sub Main()
        MainAsync().GetAwaiter().GetResult()
    End Sub

    Async Function MainAsync() As Task
        ShowLogo()
        InitializeDatabase()

        Dim token = (Await _db.LoadObjectAsync(Of BotConfig)(BOT_CONFIG)).Token

        _client = New DiscordShardedClient(GetClientConfig())
        _commands = New CommandService(GetCommandConfig())
        _services = BuildServices()

        Await SetupCommandsAsync()
        Await _services.GetService(Of StartupService).InitializeAsync()
        Await _client.LoginAsync(TokenType.Bot, token)
        Await _client.StartAsync()

        Await Task.Delay(-1)
    End Function

    Async Function SetupCommandsAsync() As Task
        Dim typeParserInterface = _commands.GetType().Assembly.GetTypes().FirstOrDefault(Function(x) x.Name = "ITypeParser")?.GetTypeInfo()
        If typeParserInterface Is Nothing Then Throw New QuahuRenamedException("ITypeParser")

        Dim parsers = _assemblyTypes.Where(Function(x) typeParserInterface.IsAssignableFrom(x))
        Dim internalAddParser = _commands.GetType().GetMethod("AddParserInternal", BindingFlags.NonPublic Or BindingFlags.Instance)
        If internalAddParser Is Nothing Then Throw New QuahuRenamedException("InternalAddParser")

        For Each parser In parsers
            Dim targetType = parser.BaseType.GetGenericArguments().First()
            internalAddParser.Invoke(_commands, New Object() {targetType, Activator.CreateInstance(parser), False})
        Next

        Await _commands.AddModulesAsync(Assembly.GetEntryAssembly())
    End Function

    Function BuildServices() As IServiceProvider
        Dim serviceCollection = New ServiceCollection()
        Dim services = From type In _assemblyTypes
                       Where type.GetCustomAttribute(Of ServiceAttribute)(True) IsNot Nothing AndAlso
                            GetType(IService).IsAssignableFrom(type)

        For Each service In services
            Select Case service.GetCustomAttribute(Of ServiceAttribute)(True).Type
                Case ServiceType.Singleton
                    serviceCollection.AddSingleton(service)
                Case ServiceType.Scoped
                    serviceCollection.AddScoped(service)
                Case ServiceType.Trasient
                    serviceCollection.AddTransient(service)
            End Select
        Next

        With serviceCollection
            .AddSingleton(_db)
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(Of DerpRandom)
        End With

        Return serviceCollection.BuildServiceProvider()
    End Function

    Sub InitializeDatabase()
        Dim xDoc As New XmlDocument()

        xDoc.Load(".\config.xml")
        _db = New SQLExpressClient(xDoc)

        Dim objs = (From type In _assemblyTypes
                    Where GetType(IStoreableObject).IsAssignableFrom(type)
                    Select DirectCast(Activator.CreateInstance(type), IStoreableObject)).ToArray()

        _db.InitialiseObjects(objs)
        _db.LoadObjectsCache(objs)
    End Sub

    Function GetClientConfig() As DiscordSocketConfig
        Dim clientConfig = _db.LoadObject(Of ClientConfig)(CLIENT_CONFIG)
        Return New DiscordSocketConfig With
        {
            .AlwaysDownloadUsers = clientConfig.AlwaysDownloadUsers,
            .LogLevel = DirectCast(clientConfig.LogLevel, LogSeverity),
            .MessageCacheSize = clientConfig.MessageCacheSize,
            .TotalShards = clientConfig.TotalShards
        }
    End Function

    Function GetCommandConfig() As CommandServiceConfiguration
        Dim commandConfig = _db.LoadObject(Of CommandConfig)(COMMAND_CONFIG)
        Return New CommandServiceConfiguration With
        {
            .CaseSensitive = commandConfig.CaseSensitiveCommands,
            .IgnoreExtraArguments = True
        }
    End Function

    Sub ShowLogo()
        ForegroundColor = ConsoleColor.Cyan
        WriteLine(" ________      _______       ________      ________    ________      ________      _________   ")
        WriteLine("|\   ___ \    |\  ___ \     |\   __  \    |\   __  \  |\   __  \    |\   __  \    |\___   ___\ ")
        WriteLine("\ \  \_|\ \   \ \   __/|    \ \  \|\  \   \ \  \|\  \ \ \  \|\ /_   \ \  \|\  \   \|___ \  \_| ")
        WriteLine(" \ \  \ \\ \   \ \  \_|/__   \ \   _  _\   \ \   ____\ \ \   __  \   \ \  \\\  \       \ \  \  ")
        WriteLine("  \ \  \_\\ \   \ \  \_|\ \   \ \  \\  \|   \ \  \___|  \ \  \|\  \   \ \  \\\  \       \ \  \ ")
        WriteLine("   \ \_______\   \ \_______\   \ \__\\ _\    \ \__\      \ \_______\   \ \_______\       \ \__\")
        WriteLine("    \|_______|    \|_______|    \|__|\|__|    \|__|       \|_______|    \|_______|        \|__|")
        WriteLine("                                                                                               ")
        ForegroundColor = ConsoleColor.White
    End Sub

    Public Enum ServiceType
        Singleton
        Scoped
        Trasient
    End Enum

    Public Enum LogSource
        GuildAvailable
        GuildUnavailable
        JoinedGuild
        LeftGuild
        MessageReceived
        UserBanned
        UserJoined
        UserUnbanned
        GuildMembersDownloaded
        ' Guild ^
        ShardReady
        ShardConnected
        ShardDisconnected
        ' Shard ^
        Command
        CommandError
        ' Command ^
        Service
        ServiceError
        ' Service ^
        Economy
        ' Economy ^
    End Enum

    Public Enum SpecialRole
        Admin
        Moderator
    End Enum

End Module