using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Collections;

public unsafe class DresserObserver
{
    public List<uint> DresserItemIds;
    public List<ushort> DresserItemSetUnlockBits;
    public List<uint> ArmoireItemIds;

    private static MirageManager* MirageManager => FFXIVClientStructs.FFXIV.Client.Game.MirageManager.Instance();
    private static FFXIVClientStructs.FFXIV.Client.Game.UI.Cabinet Cabinet => UIState.Instance()->Cabinet;

    private const int DRESSER_ITEM_LIMIT = 800;

    public DresserObserver()
    {

        Dev.Log($"DresserObserver: AddonEventHandler called with ");
        InitializeContentsFromConfiguration();
        Services.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "MiragePrismPrismBox", (_, args) =>
        {
            LoadDresserContentsIfLoaded();
        });
    }

    public void Dispose()
    {
        Services.AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "MiragePrismPrismBox");
    }

    private unsafe void LoadDresserContentsIfLoaded()
    {
        Dev.Log("DresserObserver: LoadDresserContentsDeduped called");

        if (IsDresserLoaded())
        {
            LoadDresserContents();

            // Only load Armoire when loading Dresser
            // Dresser loaded is good indication that we are at an Inn, and interacted with either Dresser or Armoire
            if (IsArmoireLoaded())
            {
                LoadArmoireContents();
            }
        }
        else
        {
            Dev.Log("DresserObserver: MirageManager is null, skipping Dresser load");
        }
    }

    private unsafe bool IsDresserLoaded()
    {
        if (MirageManager == null)
            return false;
        return MirageManager->PrismBoxLoaded;
    }

    public bool IsArmoireLoaded()
    {
        return Cabinet.IsCabinetLoaded();
    }

    private void LoadDresserContents()
    {
        var initialItemCount = DresserItemIds.Count;
        DresserItemIds.Clear();
        DresserItemSetUnlockBits.Clear();

        var itemFinderModule = UIModule.Instance()->GetItemFinderModule();

        for (var i = 0; i < DRESSER_ITEM_LIMIT; i++)
        {
            var itemId = itemFinderModule != null ? itemFinderModule->GlamourDresserItemIds[i] : MirageManager->PrismBoxItemIds[i];
            if (itemId == 0)
                continue;

            var pureItemId = Services.ItemFinder.GetPureItemId(itemId);
            DresserItemIds.Add(pureItemId);
            DresserItemSetUnlockBits.Add(itemFinderModule != null ? itemFinderModule->GlamourDresserItemSetUnlockBits[i] : (ushort)0);
        }

        Dev.Log($"Refreshing Dresser contents count: {initialItemCount} -> {DresserItemIds.Count}");

        SaveDresserContentsInConfiguration();
    }

    private void LoadArmoireContents()
    {
        var initialItemCount = ArmoireItemIds.Count;
        ArmoireItemIds.Clear();

        var cabinetSheet = ExcelCache<Lumina.Excel.Sheets.Cabinet>.GetSheet();
        foreach (var cabinet in cabinetSheet)
        {
            if (Cabinet.IsItemInCabinet(cabinet.RowId))
            {
                var itemId = (uint)Services.ItemFinder.ItemIdFromCabinetId(cabinet.RowId);
                ArmoireItemIds.Add(itemId);
            }
        }

        Dev.Log($"Refreshing Armoire contents count: {initialItemCount} -> {ArmoireItemIds.Count}");

        SaveArmoireContentsInConfiguration();
    }

    private void InitializeContentsFromConfiguration()
    {
        DresserItemIds = Services.Configuration.DresserItemIds;
        DresserItemSetUnlockBits = Services.Configuration.DresserItemSetUnlockBits;
        while (DresserItemSetUnlockBits.Count < DresserItemIds.Count)
        {
            DresserItemSetUnlockBits.Add(0);
        }
        ArmoireItemIds = Services.Configuration.ArmoireItemIds;
    }

    private void SaveDresserContentsInConfiguration()
    {
        Services.Configuration.DresserItemIds = DresserItemIds;
        Services.Configuration.DresserItemSetUnlockBits = DresserItemSetUnlockBits;
        Services.Configuration.Save();
    }

    private void SaveArmoireContentsInConfiguration()
    {
        Services.Configuration.ArmoireItemIds = ArmoireItemIds;
        Services.Configuration.Save();
    }
}
