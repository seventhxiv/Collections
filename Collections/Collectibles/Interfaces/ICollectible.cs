namespace Collections;

public interface ICollectible
{
    public string Name { get; init; }
    public uint Id { get; init; }
    public decimal PatchAdded { get; init; }
    public HintModule PrimaryHint { get; init; }
    public HintModule SecondaryHint { get; init; }
    public string Description { get; init; }
    public ICollectibleKey CollectibleKey { get; init; }
    public bool IsFavorite();
    public void SetFavorite(bool favorite);
    public bool IsWishlist();
    public void SetWishlist(bool wishlist);
    public void OpenGamerEscape();
    public bool GetIsObtained();
    public void UpdateObtainedState();
    public ISharedImmediateTexture GetIcon();
    public void DrawAdditionalTooltip();
    public void DrawAdditionalIconOverlay();
    public void Interact();
    public string GetDisplayName();
    public string GetCollectionName();
    public List<CollectibleSortOption> GetSortOptions();
    public List<CollectibleFilterOption> GetFilterOptions();
    public bool GetIsFiltered();
    public string GetDisplayPatch();
}
