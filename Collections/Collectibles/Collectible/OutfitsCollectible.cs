namespace Collections;

public class OutfitsCollectible : Collectible<Item>, ICreateable<OutfitsCollectible, Item>
{
    public new static string CollectionName => "Outfits";
    private bool outfitCompletionInitialized = false;

    //For showing collected number in text overlay
    private int outfitCount = 0;
    private int outfitMax = 0;

    public OutfitsCollectible(Item excelRow) : base(excelRow)
    {
    }

    public static OutfitsCollectible Create(Item excelRow)
    {
        return new(excelRow);
    }

    protected override string GetCollectionName()
    {
        return CollectionName;
    }

    protected override string GetName()
    {
        return ExcelRow.Name.ToString();
    }

    protected override uint GetId()
    {
        return ExcelRow.RowId;
    }

    protected override string GetDescription()
    {
        return "";
        // var items = Services.ItemFinder.ItemsInOutfit(ExcelRow.RowId);
        // return items.Select(i => i.Name).Aggregate((full, item) => full + "\n" + item).ToString();
    }

    public override void DrawAdditionalTooltip()
    {
        var items = Services.ItemFinder.ItemIdsInOutfit(ExcelRow.RowId);
        var collectibles = Services.DataProvider.GetCollection<GlamourCollectible>()?.Where(c => items.Contains(c.Id)).ToList();
        var iconSize = UiHelper.ScaleForFontSize(50);
        for(int i = 0; i < collectibles?.Count; i++)
        {
            var c = collectibles[i];
            var icon = c.GetIcon();
            if (icon != null)
            {
                ImGui.Image(icon.GetWrapOrEmpty().Handle, new Vector2(iconSize, iconSize));
                if (i < collectibles.Count - 1)
                {
                    ImGui.SameLine();
                }
                var finalPos = ImGui.GetCursorPos();
                c.UpdateObtainedState();
                var obtained = Services.ItemFinder.IsItemInDresser(c.Id);
                if (obtained)
                {
                    var _ = true;
                    UiHelper.IconButtonWithOffset(i, FontAwesomeIcon.Check, ImGui.GetFontSize() + (ImGui.ImGuiStyle().ItemSpacing.X * (17 / ImGui.GetFontSize())), -iconSize + ImGui.GetFontSize(), ref _, .735f, new Vector4(1f, .741f, .188f, 1), ColorsPalette.BLACK.WithAlpha(0));
                    ImGui.SetCursorPos(finalPos);
                }
            }
        }
    }

    public override void DrawAdditionalIconOverlay()
    {
        if (!outfitCompletionInitialized)
        {
            UpdateOutfitCompletionState();
        }

        var isCompleteSet = outfitMax == outfitCount;


        // Not an outfit
        if (outfitMax == 0)
        {
            return;
        }

        // Obtained and Complete Set
        if (isObtained && outfitCount == 0)
        {
            return;
        }

        var color = isCompleteSet ? ColorsPalette.YELLOW : ColorsPalette.WHITE;
        UiHelper.WriteTextOverlay($"{outfitCount}/{outfitMax}", color, ColorsPalette.BLACK);
    }

    protected override HintModule GetSecondaryHint()
    {
        if (this.CollectibleKey == null) return base.GetSecondaryHint();
        return new HintModule($"{(this.CollectibleKey as OutfitKey).FirstItem.ClassJobCategory.Value.Name}, Lv. {(this.CollectibleKey as OutfitKey).FirstItem.LevelEquip}", null);
    }

    public override void UpdateObtainedState()
    {
        isObtained = Services.ItemFinder.IsItemInDresser(ExcelRow.RowId);
        UpdateOutfitCompletionState();
    }

    private void UpdateOutfitCompletionState()
    {
        var outfitItemIds = Services.ItemFinder.ItemIdsInOutfit(ExcelRow.RowId);
        outfitMax = outfitItemIds.Count;
        outfitCount = outfitItemIds.Count(itemId => Services.ItemFinder.IsItemInInventory(itemId) || 
                                                    Services.ItemFinder.IsItemInArmoireCache(itemId) || 
                                                    Services.ItemFinder.IsItemInDresser(itemId));
        // reset count when item is already an Outfit
        if (isObtained)
        {
            outfitCount = outfitItemIds.Count(itemId => Services.ItemFinder.IsItemInDresser(itemId, false));
        }

        outfitCompletionInitialized = true;
    }

    protected override int GetIconId()
    {
        return ExcelRow.Icon;
    }

    public int GetNumberOfDyeSlots()
    {
        if (this.CollectibleKey == null) return 0;
        return (CollectibleKey as OutfitKey).FirstItem.DyeCount;
    }

    public override void Interact()
    {
        // Do nothing - taken care of by event service (should probably unify this in some way)
    }
}
