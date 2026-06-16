using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Extensions;

namespace Collections;

public unsafe class ItemFinder
{
    private static FFXIVClientStructs.FFXIV.Client.Game.UI.Cabinet Cabinet = UIState.Instance()->Cabinet;

    public bool IsItemInArmoireCache(uint itemId)
    {
        return Services.DresserObserver.ArmoireItemIds.Contains(itemId);
    }

    public uint? CabinetIdFromItemId(uint itemId)
    {
        var cabinetItem = ExcelCache<Lumina.Excel.Sheets.Cabinet>.GetSheet().Where(entry => entry.Item.RowId == itemId).FirstOrNull();
        return cabinetItem is not null ? cabinetItem.Value.RowId : null;
    }

    public uint? ItemIdFromCabinetId(uint cabinetId)
    {
        var cabinetItem = ExcelCache<Lumina.Excel.Sheets.Cabinet>.GetSheet().GetRow(cabinetId);
        return cabinetItem is not null ? cabinetItem.Value.Item.RowId : null;
    }

    public bool IsItemInDresser(uint itemId, bool checkOutfits = false)
    {
        var pureItemId = GetPureItemId(itemId);
        return Services.DresserObserver.DresserItemIds.Contains(pureItemId) || (checkOutfits && OutfitsContainingItem(pureItemId).Any(outfitId => IsItemInDresserOutfit(outfitId, pureItemId)));
    }

    public List<uint> OutfitsContainingItem(uint itemId)
    {
        return ExcelCache<MirageStoreSetItem>.GetSheet().Where(outfit =>
            ((List<uint>)[
                outfit.MainHand.RowId,
                outfit.OffHand.RowId,
                outfit.Head.RowId,
                outfit.Body.RowId,
                outfit.Hands.RowId,
                outfit.Legs.RowId,
                outfit.Feet.RowId,
                outfit.Earrings.RowId,
                outfit.Necklace.RowId,
                outfit.Bracelets.RowId,
                outfit.Ring.RowId,
            ]).Contains(itemId)).Select((outfit) => outfit.RowId).ToList();
    }

    // Internally, outfits and their associated items are stored as 'MirageStoreSetItem'
    // We can use this to get the items required to create the outfit in the first place.
    // reason the collection isn't a MirageStoreSetItem is because that class is only a LookupTable,
    // and it's more convenient to store it internally like a GlamourCollectible.
    public List<uint> ItemIdsInOutfit(uint itemId)
    {
        return ItemIdSlotsInOutfit(itemId).Select(item => item.Id).ToList();
    }

    public List<uint> ItemIdsObtainedInOutfit(uint outfitId)
    {
        var unlockBits = GetDresserOutfitSetUnlockBits(outfitId);
        if (unlockBits is null)
        {
            return [];
        }

        // outfits in the dresser can now be partial. so check the slot bits
        return ItemIdSlotsInOutfit(outfitId)
            .Where(item => IsOutfitSlotUnlocked(unlockBits.Value, item.Slot))
            .Select(item => item.Id)
            .ToList();
    }

    public bool IsItemInDresserOutfit(uint outfitId, uint itemId)
    {
        var pureItemId = GetPureItemId(itemId);
        var unlockBits = GetDresserOutfitSetUnlockBits(outfitId);
        if (unlockBits is null)
        {
            return false;
        }

        return ItemIdSlotsInOutfit(outfitId).Any(item => item.Id == pureItemId && IsOutfitSlotUnlocked(unlockBits.Value, item.Slot));
    }

    private List<(uint Id, int Slot)> ItemIdSlotsInOutfit(uint itemId)
    {
        var outfitSet = ExcelCache<MirageStoreSetItem>.GetSheet().GetRow(itemId);
        if (outfitSet is null)
        {
            return [];
        }

        var related = outfitSet.Value;
        // slot order matches the dresser outfit bitmask
        List<(uint Id, int Slot)> items = [
            (related.MainHand.RowId, 0),
            (related.OffHand.RowId, 1),
            (related.Head.RowId, 2),
            (related.Body.RowId, 3),
            (related.Hands.RowId, 4),
            (related.Legs.RowId, 5),
            (related.Feet.RowId, 6),
            (related.Earrings.RowId, 7),
            (related.Necklace.RowId, 8),
            (related.Bracelets.RowId, 9),
            (related.Ring.RowId, 10),
        ];

        return items.Where(item => item.Id != 0).ToList();
    }

    private ushort? GetDresserOutfitSetUnlockBits(uint outfitId)
    {
        var pureOutfitId = GetPureItemId(outfitId);
        var index = Services.DresserObserver.DresserItemIds.IndexOf(pureOutfitId);
        if (index < 0 || index >= Services.DresserObserver.DresserItemSetUnlockBits.Count)
        {
            return null;
        }

        return Services.DresserObserver.DresserItemSetUnlockBits[index];
    }

    private bool IsOutfitSlotUnlocked(ushort unlockBits, int slot)
    {
        // bit set means missing, bit clear means collected
        return (unlockBits & (1 << slot)) == 0;
    }

    // Helper function to find the actual item refs stored within an outfit
    public List<Item> ItemsInOutfit(uint itemId)
    {
        return ItemIdsInOutfit(itemId)
            .Where(id => ExcelCache<Item>.GetSheet().GetRow(id).HasValue)
            .Select(id => ExcelCache<Item>.GetSheet().GetRow(id).Value)
            .ToList();
    }

    public uint GetPureItemId(uint itemId)
    {
        return itemId > 1000000 ? itemId - 1000000 : itemId;
    }

    public bool IsItemInInventory(uint itemId)
    {
        var inventoryTypes = new List<InventoryType>()
        {
            InventoryType.Inventory1,
            InventoryType.Inventory2,
            InventoryType.Inventory3,
            InventoryType.Inventory4,
            InventoryType.EquippedItems,
            InventoryType.ArmoryOffHand,
            InventoryType.ArmoryHead,
            InventoryType.ArmoryBody,
            InventoryType.ArmoryHands,
            InventoryType.ArmoryWaist,
            InventoryType.ArmoryLegs,
            InventoryType.ArmoryFeets,
            InventoryType.ArmoryEar,
            InventoryType.ArmoryNeck,
            InventoryType.ArmoryWrist,
            InventoryType.ArmoryRings,
            InventoryType.ArmoryMainHand,
            InventoryType.SaddleBag1,
            InventoryType.SaddleBag2,
            InventoryType.PremiumSaddleBag1,
            InventoryType.PremiumSaddleBag2,
            InventoryType.RetainerPage1,
            InventoryType.RetainerPage2,
            InventoryType.RetainerPage3,
            InventoryType.RetainerPage4,
            InventoryType.RetainerPage5,
            InventoryType.RetainerPage6,
            InventoryType.RetainerPage7,
        };
        foreach (var inventoryType in inventoryTypes)
        {
            if (InventoryManager.Instance()->GetItemCountInContainer(itemId, inventoryType, true) > 0)
                return true;
            else if (InventoryManager.Instance()->GetItemCountInContainer(itemId, inventoryType, false) > 0)
                return true;
        }
        return false;
    }
}
