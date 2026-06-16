using Dalamud.Configuration;

namespace Collections;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public Dictionary<string, HashSet<uint>> Favorites = new();
    public Dictionary<string, HashSet<uint>> Wishlist = new();
    public HashSet<uint> WishListed = new();
    public List<uint> DresserItemIds = new();
    public List<ushort> DresserItemSetUnlockBits = new();
    public List<uint> ArmoireItemIds = new();
    public List<string> AdditionalTooltips = new();
    public GlamourTree GlamourTree = new();
    public bool AutoOpenInstanceTab = true;
    public bool OnlyOpenIfUncollected = false;
    public bool AutoHideObtainedFromInstanceTab = false;
    public List<string> ExcludedCollectionsFromInstanceTab = new();
    public bool ForceTryOn = false;
    // Changes left-click behaviour for glamour collections
    public bool SeparatePreviewAndApply = false;
    // Use a green border to indicate obtained items instead of displaying a checkmark
    public bool HighVisibilityObtained = false;

    public void Save()
    {
        Services.PluginInterface.SavePluginConfig(this);
    }

    public static Configuration GetConfig()
    {
        var config = Services.PluginInterface.GetPluginConfig() as Configuration;
        if (config is not null)
        {
            Dev.Log($"Loaded Configuration: V{config.Version}");
            return config;
        }

        Dev.Log("Initializing a new Configuration");
        return new();
    }
}
