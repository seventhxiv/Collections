namespace Collections;

public class CollectibleKeyFactory
{
    public static ICollectibleKey Get<T>(Collectible<T> collectible) where T : struct, IExcelRow<T>
    {
        var type = typeof(T);
        var keysDataGenerator = Services.DataGenerator.KeysDataGenerator;
        var id = collectible.Id;

        if (type == typeof(Item))
        {
            var item = (Item)ExcelCache<Item>.GetSheet().GetRow(id)!;
            if(item.ItemUICategory.RowId == 112)
            {
                return CollectibleKeyCache<OutfitKey, Item>.Instance.GetObject((item, 0));
            }
            return CollectibleKeyCache<ItemKey, Item>.Instance.GetObject((item, 0));
        }

        if (keysDataGenerator.collectibleIdToItem.TryGetValue(type, out var itemDict))
        {
            if (itemDict.TryGetValue(id, out var item))
            {
                return CollectibleKeyCache<ItemKey, Item>.Instance.GetObject((item, 0));
            }
        }
        if (keysDataGenerator.collectibleIdToQuest.TryGetValue(type, out var questDict))
        {
            if (questDict.TryGetValue(id, out var quest))
            {
                return CollectibleKeyCache<QuestKey, Quest>.Instance.GetObject((quest, 0));
            }
        }
        if (keysDataGenerator.collectibleIdToInstance.TryGetValue(type, out var instanceDict))
        {
            if (instanceDict.TryGetValue(id, out var instance))
            {
                return CollectibleKeyCache<InstanceKey, ContentFinderCondition>.Instance.GetObject((instance, 0));
            }
        }
        if (keysDataGenerator.collectibleIdToAchievement.TryGetValue(type, out var achievementDict))
        {
            if (achievementDict.TryGetValue(id, out var achievement))
            {
                return CollectibleKeyCache<AchievementKey, Achievement>.Instance.GetObject((achievement, 0));
            }
        }
        if (keysDataGenerator.collectibleIdToMisc.TryGetValue(type, out var miscDict))
        {
            if (miscDict.TryGetValue(id, out var misc))
            {
                return new MiscKey((misc, false));
            }
        }
        if (type == typeof(Lumina.Excel.Sheets.Action))
        {
            if (Services.DataGenerator.KeysDataGenerator.ActionIdToBlueSpell.TryGetValue(id, out var monster))
            {
                return new MonsterKey((monster, false));
            }
        }

        if (type == typeof(ENpcResident))
        {
            var npc = (ENpcResident)(object)collectible.ExcelRow;
            return CollectibleKeyCache<TripleTriadNpcKey, ENpcResident>.Instance.GetObject((npc, (int)id));
        }

        return null;
    }
}

