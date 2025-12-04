using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Collections;

public class Services
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IChatGui ChatGui { get; private set; }
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static ICommandManager CommandManager { get; private set; }
    [PluginService] public static ICondition Condition { get; private set; }
    [PluginService] public static IDataManager DataManager { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IGameGui GameGui { get; private set; }
    [PluginService] public static IKeyState KeyState { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }
    [PluginService] public static ITargetManager TargetManager { get; private set; }
    [PluginService] public static IToastGui ToastGui { get; private set; }
    [PluginService] public static IPluginLog PluginLog { get; private set; }
    [PluginService] public static ITextureProvider TextureProvider { get; private set; }
    [PluginService] public static IDutyState DutyState { get; private set; }
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; }

    public static Plugin Plugin { get; private set; }
    public static Configuration Configuration { get; private set; }
    public static CommandsInitializer CommandsInitializer { get; private set; }
    public static WindowsInitializer WindowsInitializer { get; private set; }
    public static DataGenerator DataGenerator { get; private set; }
    public static UniversalisClient UniversalisClient { get; private set; }
    public static DataProvider DataProvider { get; private set; }
    public static LodestoneClient LodestoneClient { get; private set; }
    public static DresserObserver DresserObserver { get; private set; }
    public static ItemFinder ItemFinder { get; private set; }
    public static PreviewExecutor PreviewExecutor { get; private set; }

    public Services(Plugin plugin)
    {
        Plugin = plugin;
        Dev.Start();

        // Config
        Configuration = Configuration.GetConfig();

        // General
        PreviewExecutor = new PreviewExecutor();
        UniversalisClient = new UniversalisClient();
        ItemFinder = new ItemFinder();
        DresserObserver = new DresserObserver();

        // Data
        DataGenerator = new DataGenerator();
        DataProvider = new DataProvider();

        // Windows
        CommandsInitializer = new CommandsInitializer();
        WindowsInitializer = new WindowsInitializer();
        LodestoneClient = new LodestoneClient();

        // Framework ticks
        Framework.Update += WindowsInitializer.MainWindow.OnFrameworkTick;

        ClientState.Login += RefreshCollectionsForPlayer;

        if (ClientState.IsLoggedIn)
        {
            RefreshCollectionsForPlayer();
        }

        Dev.Stop();

        // DataDebugExporter.ExportCollectionsData();
        // DataDebugExporter.LogDataReport();
    }

    private static void RefreshCollectionsForPlayer()
    {
        // Needs to be in main thread to access local player from ObjectTable.
        Framework.RunOnFrameworkThread(() =>
        {
            if (ObjectTable.LocalPlayer is { } localPlayer)
            {
                DataProvider.RepopulateDataForLoggedInPlayer(localPlayer);
                WindowsInitializer.MainWindow.RefreshTabs();
            }
        });
    }

    public static void Dispose()
    {
        // Cmds
        CommandsInitializer.Dispose(CommandManager);

        // Windows
        WindowsInitializer.Dispose();

        // Framework ticks
        Framework.Update -= WindowsInitializer.MainWindow.OnFrameworkTick;

        // Dresser
        DresserObserver.Dispose();

        ClientState.Login -= RefreshCollectionsForPlayer;
    }
}
