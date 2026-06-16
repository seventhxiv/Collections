using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

namespace Collections;

// Maintain order to set glamour plate pointer based on this index
public enum EquipSlot : uint
{
    MainHand,
    OffHand,
    Head,
    Body,
    Gloves,
    Legs,
    Feet,
    Ears,
    Neck,
    Wrists,
    FingerR,
    FingerL,
    None,
}

class EquipSlotConverter
{
    public static WeaponSlot EquipSlotToWeaponSlot(EquipSlot equipSlot)
    {
        switch (equipSlot)
        {
            case EquipSlot.MainHand: return WeaponSlot.MainHand;
            case EquipSlot.OffHand: return WeaponSlot.OffHand;
            default: return WeaponSlot.System;
        }
    }

    public static EquipmentSlot EquipSlotToEquipmentSlot(EquipSlot equipSlot)
    {
        switch (equipSlot)
        {
            case EquipSlot.Head: return EquipmentSlot.Head;
            case EquipSlot.Body: return EquipmentSlot.Body;
            case EquipSlot.Gloves: return EquipmentSlot.Hands;
            case EquipSlot.Legs: return EquipmentSlot.Legs;
            case EquipSlot.Feet: return EquipmentSlot.Feet;
            case EquipSlot.Ears: return EquipmentSlot.Ears;
            case EquipSlot.Neck: return EquipmentSlot.Neck;
            case EquipSlot.Wrists: return EquipmentSlot.Wrists;
            case EquipSlot.FingerR: return EquipmentSlot.RFinger;
            case EquipSlot.FingerL: return EquipmentSlot.LFinger;
            default: throw new ArgumentException($"EquipSlot ${equipSlot} has no match to an EquipmentSlot");
        }
    }

    public static int EquipSlotToInventorySlot(EquipSlot equipSlot)
    {
        if (equipSlot <= EquipSlot.Gloves)
        {
            return (int)equipSlot;
        }
        // accounts for belts being removed
        return (int)equipSlot + 1;
    }

    public static uint EquipSlotToIcon(EquipSlot equipSlot)
    {

        switch (equipSlot)
        {
            
            case EquipSlot.OffHand: return 060110;
            case EquipSlot.Head: return 060124;
            case EquipSlot.Body: return 060126;
            case EquipSlot.Gloves: return 060129;
            case EquipSlot.Legs: return 060128;
            case EquipSlot.Feet: return 060130;
            case EquipSlot.Ears: return 060133;
            case EquipSlot.Neck: return 060132;
            case EquipSlot.Wrists: return 060134;
            case EquipSlot.FingerR: return 060135;
            case EquipSlot.FingerL: return 060135;
            default: return 061762;
        }
    }
}
