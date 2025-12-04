using Dalamud.Interface.Windowing;

namespace Collections;

public class MainWindow : Window, IDisposable
{
    public Vector4 originalButtonColor { get; set; }

    private List<(string name, IDrawable window)> tabs { get; set; }

    public MainWindow() : base("Collections", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse)
    {
        StoreOriginalColors();

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(520, 300),
            MaximumSize = new Vector2(2000, 1000)
        };

        RefreshTabs();
    }

    public string? forceOpenTab = null;
    private string previousTabName = String.Empty;

    public void RefreshTabs()
    {
        tabs = GetCollectionTabs();
        var additionalTabs = new List<(string name, IDrawable window)>()
        {
            ("All Items", new FullCollectionTab()),
            ("Instance",  new InstanceTab()),
            ("Wish List", new WishlistTab()),
            ("Settings", new SettingsTab()),
        };

        tabs.AddRange(additionalTabs);
    }
    
    public override void Draw()
    {
        if (ImGui.BeginTabBar("tab-bar", ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.AutoSelectNewTabs))
        {
            foreach (var (name, window) in tabs)
            {
                // Use AutoSelectNewTabs to focus the forceOpenTab
                if (forceOpenTab == name)
                {
                    forceOpenTab = null;
                    continue;
                }

                // Tab item
                if (ImGui.BeginTabItem($"{name}"))
                {
                    // Run OnOpen when a new tab is selected
                    if (name != previousTabName)
                    {
                        window.OnOpen();
                        previousTabName = name;
                    }

                    // Draw window
                    window.Draw();

                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    private List<(string name, IDrawable window)> GetCollectionTabs()
    {
        var collections = Services.DataProvider.collections.OrderBy(e => e.Value.orderKey);
        return collections.Select(kv => (kv.Value.name, GetCollectionTab(kv.Key))).ToList();

    }

    private IDrawable GetCollectionTab(Type T)
    {
        if (T == typeof(GlamourCollectible))
            return new GlamourTab();
        else if (T == typeof(OutfitsCollectible))
            return new OutfitsTab();
        else
            return new CollectionTab(Services.DataProvider.GetCollection(T));

    }

    public void OpenTab(string tabName)
    {
        forceOpenTab = tabName;
        IsOpen = true;
    }

    public unsafe void StoreOriginalColors()
    {
        originalButtonColor = *ImGui.GetStyleColorVec4(ImGuiCol.Button);
    }

    public override void OnOpen()
    {
    }

    public override void OnClose()
    {
        previousTabName = string.Empty;

        Services.PreviewExecutor.ResetAllPreview();

        foreach (var (_, window) in tabs)
        {
            window.OnClose();
        }
    }

    public void Dispose()
    {
        foreach (var (_, window) in tabs)
        {
            window.Dispose();
        }
    }

    // Resets preview under certain conditions (GPose open)
    public void OnFrameworkTick(IFramework framework)
    {
        if (IsOpen && PreviewExecutor.IsInGPose())
        {
            Services.PreviewExecutor.ResetAllPreview();
        }
    }

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1, 1, 1, 0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 3));
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }
}

public interface IDrawable : IDisposable
{
    public void Draw();
    public void OnOpen();
    public void OnClose();
}

