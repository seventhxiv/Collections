using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace Collections;

public abstract class Collectible<T> : ICollectible where T : struct, IExcelRow<T>
{
    public string Name { get; init; }
    public uint Id { get; init; }
    protected virtual string GetCollectionName() => "";
    public abstract void UpdateObtainedState();
    public abstract void Interact();
    public HintModule PrimaryHint { get; init; }
    public HintModule SecondaryHint { get; init; }
    public string Description { get; init; }
    // keep as decimal; Version sucks for how FF14 handles versioning.
    public decimal PatchAdded {get; init; }

    protected abstract int GetIconId();
    protected abstract uint GetId();
    protected abstract string GetName();
    protected abstract string GetDescription();
    public virtual void DrawAdditionalTooltip()
    {
        return;
    }

    public virtual void DrawAdditionalIconOverlay()
    {
        return;
    }

    public ICollectibleKey CollectibleKey { get; init; }
    public T ExcelRow { get; set; }
    protected IconHandler IconHandler { get; init; }
    protected List<CollectibleSortOption> SortOptions = [
        new CollectibleSortOption(
            "Patch",
            (c) => c.PatchAdded,
            Reverse: true,
            Icons: (FontAwesomeIcon.SortNumericDownAlt, FontAwesomeIcon.SortNumericUpAlt)
        ),
        new CollectibleSortOption(
            "Name",
            (c) => c.Name,
            Icons: (FontAwesomeIcon.SortAlphaUp, FontAwesomeIcon.SortAlphaDown)
        ),
        new CollectibleSortOption(
            "Obtained",
            (c) => c.GetIsObtained(),
            Reverse: true
        ),
        // new CollectibleSortOption(
        //     "Marketboard Value",
        //     (c) => c.CollectibleKey.GetIsTradeable() == Tradeability.Tradeable ? c.CollectibleKey.GetMarketBoardPriceLazy() ?? 0 : 0
        // ),
    ];
    protected List<CollectibleFilterOption> FilterOptions = [
        new CollectibleListFilterOption<Tradeability>(
            "Tradability",
            (c) => c.Item1.CollectibleKey.GetIsTradeable() != c.Item2,
            Enum.GetValues<Tradeability>().ToList(),
            Description: "Whether this item can be bought off the marketboard"
        ),
        new CollectibleBooleanFilterOption(
            "Exclude Events",
            c => c.CollectibleKey.SourceCategories.Contains(SourceCategory.Event),
            Description: "Exclude items only obtainable from seasonal or limited events"
        ),
        new CollectibleBooleanFilterOption(
            "Exclude Mog Station",
            c => c.CollectibleKey.SourceCategories.Contains(SourceCategory.MogStation) && c.CollectibleKey.SourceCategories.Count == 1,
            Description: "Exclude items only ever obtainable via cash shop"
        ),
        new CollectibleBooleanFilterOption(
            "Exclude Unknown",
            c => c.CollectibleKey.CollectibleSources.Count == 0,
            Description: "Exclude items that this plugin cannot find a source for"
        ),
        // new CollectibleBooleanFilterOption(
        //     "Exclude Ranked PvP",
        //     c => c.CollectibleKey.CollectibleSources.Any(source => source.GetType() == typeof(PvPRankingSource)),
        //     Description: "Exclude items only obtainable through ranked PvP"
        // ),
    ];

    public Collectible(T excelRow)
    {
        ExcelRow = excelRow;
        Id = GetId();
        CollectibleKey = CollectibleKeyFactory.Get(this);
        Name = GetName();
        Description = GetDescription();
        if (CollectibleKey is null)
        {
            Dev.Log($"Missing collectible key: {Name} ({Id}) for row {ExcelRow.RowId} in collection {GetCollectionName()}");
        }
        PatchAdded = GetPatchAdded();
        PrimaryHint = GetPrimaryHint();
        SecondaryHint = GetSecondaryHint();
        IconHandler = new IconHandler(GetIconId());
    }

    public virtual void OpenGamerEscape()
    {
        WikiOpener.OpenGamerEscape(GetDisplayName());
    }

    protected virtual HintModule GetPrimaryHint()
    {
        return new HintModule($"Patch {GetDisplayPatch()}", FontAwesomeIcon.Hashtag);
    }
    protected virtual HintModule GetSecondaryHint()
    {
        return new HintModule("", null);
    }

    protected bool isObtained = false;
    public bool GetIsObtained()
    {
        return isObtained;
    }

    public bool IsFavorite()
    {
        var key = GetCollectionName();
        if (!Services.Configuration.Favorites.ContainsKey(key))
        {
            return false;
        }
        return Services.Configuration.Favorites[key].Contains(Id);
    }

    public void SetFavorite(bool favorite)
    {
        var key = GetCollectionName();
        if (!Services.Configuration.Favorites.ContainsKey(key))
        {
            Services.Configuration.Favorites[key] = new();
        }

        if (favorite)
        {
            Dev.Log($"Adding {Id} to Favorites");
            Services.Configuration.Favorites[key].Add(Id);
        }
        else
        {
            Dev.Log($"Removing {Id} from Favorites");
            Services.Configuration.Favorites[key].Remove(Id);
        }
        Services.Configuration.Save();
    }

    public bool IsWishlist()
    {
        var key = GetCollectionName();
        if (!Services.Configuration.Wishlist.ContainsKey(key))
        {
            return false;
        }
        return Services.Configuration.Wishlist[key].Contains(Id);
    }

    public void SetWishlist(bool wishlist)
    {
        var key = GetCollectionName();
        if (!Services.Configuration.Wishlist.ContainsKey(key))
        {
            Services.Configuration.Wishlist[key] = new();
        }

        if (wishlist)
        {
            Dev.Log($"Adding {Id} to Wishlist");
            Services.Configuration.Wishlist[key].Add(Id);
        }
        else
        {
            Dev.Log($"Removing {Id} from Wishlist");
            Services.Configuration.Wishlist[key].Remove(Id);
        }
        Services.Configuration.Save();
    }

    public ISharedImmediateTexture GetIcon()
    {
        return IconHandler.GetIcon();
    }

    public virtual string GetDisplayName()
    {
        return Name
                .UpperCaseAfterSpaces()
                .LowerCaseWords(new List<string>() { "Of", "Up", "The"});
    }

    public virtual List<CollectibleSortOption> GetSortOptions()
    {
        return SortOptions;
    }

    public virtual List<CollectibleFilterOption> GetFilterOptions()
    {
        return FilterOptions;
    }

    public virtual bool GetIsFiltered()
    {
        foreach(var filterOptions in FilterOptions)
        {
            if(filterOptions.IsFiltered(this)) return true;
        }
        return false;
    }

    protected virtual decimal GetPatchAdded()
    {
        // try manual override
        var patchOverrides = DataOverrides.collectibleIdToPatchAdded;
        foreach(var (type, dict) in patchOverrides)
        {
            if(typeof(T) == type)
            {
                foreach(var (id, patch) in dict)
                if(Id == id)
                {
                    return patch;
                }
            }
        }
        // LuminaSupplemental source
        if(CollectibleKey != null)
        {
            // find patch added to the game
            foreach(var patch in CsvLoader.LoadResource<ItemPatch>(CsvLoader.ItemPatchResourceName, true, out var failedLines, out var exceptions))
            {
                if( CollectibleKey.Id >= patch.StartItemId && CollectibleKey.Id <= patch.EndItemId )
                {
                    return patch.PatchNo;
                }
            }
        }
        // this way, unknown patch (new) items will appear at the top when sorted
        return new decimal(999.0);
    }

    public virtual string GetDisplayPatch()
    {
        return PatchAdded >= 999 ? "Unknown" : PatchAdded.ToString();
    }

    string ICollectible.GetCollectionName()
    {
        return GetCollectionName();
    }
}
