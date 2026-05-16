namespace Collections;

// TripleTriadDataGenerator = Used for Triple Triad Collectible Source Lookup 
// TripleTriadNpcBattleDataGenerator = Used for tracking TT Npc for achievement tracking in `Triad Npc`
public class TripleTriadNpcBattleDataGenerator : BaseDataGenerator<ENpcResident>
{
    protected override void InitializeData()
    {
        var npcBases = ExcelCache<ENpcBase>.GetSheet();
        var npcResidents = ExcelCache<ENpcResident>.GetSheet();
        var tripleTriads = ExcelCache<TripleTriad>.GetSheet();

        // Fee != 0 removes duplicate/invalid Triple Triad NPC rows.
        var triadIds = tripleTriads
                       .Where(x => x.Fee != 0)
                       .Select(x => x.RowId)
                       .ToHashSet();

        foreach (var npc in npcBases)
        {
            var triadId = npc.ENpcData
                             .Select(x => x.RowId)
                             .FirstOrDefault(triadIds.Contains);

            if (triadId == 0)
                continue;

            var resident = npcResidents.GetRow(npc.RowId);
            var triad = tripleTriads.GetRow(triadId);

            if (!resident.HasValue || !triad.HasValue)
                continue;

            AddEntry(triadId, resident.Value);
            triadIds.Remove(triadId);
        }
    }
}

