using Dalamud.IoC;
using Dalamud.Plugin;

namespace Collections;

public class Services
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static IClientState ClientState { get; private set; }
    [PluginService] public static IObjectTable ObjectTable { get; private set; }
    [PluginService] public static IUnlockState UnlockState { get; private set; }
    [PluginService] public static IPlayerState PlayerState { get; private set; }
    [PluginService] public static ICommandManager CommandManager { get; private set; }
    [PluginService] public static IDataManager DataManager { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static ITargetManager TargetManager { get; private set; }
    [PluginService] public static IPluginLog PluginLog { get; private set; }
    [PluginService] public static ITextureProvider TextureProvider { get; private set; }
    [PluginService] public static IDutyState DutyState { get; private set; }
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; }
    [PluginService] public static ISigScanner SigScanner { get; private set; }

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

        Dev.Stop();

        // DataDebugExporter.ExportCollectionsData();
        // DataDebugExporter.LogDataReport();
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
    }
}
