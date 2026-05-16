using System.Text;

namespace Collections.Collectibles.Collectible
{
    public class TripleTriadNpcCollectible : Collectible<Lumina.Excel.Sheets.ENpcResident>, ICreateable<TripleTriadNpcCollectible, Lumina.Excel.Sheets.ENpcResident>
    {
        public new static string CollectionName => "Triad NPC";

        public TripleTriadNpcCollectible(ENpcResident excelRow) : base(excelRow)
        {
        }

        public override void UpdateObtainedState()
        {

            isObtained = TripleTriadNpcStateProvider.Instance.IsNpcBeaten((int)Id);
        }

        protected override string GetCollectionName()
        {
            return CollectionName;
        }

        public override void Interact()
        {
            //Nothing
        }

        protected override int GetIconId()
        {
            return TripleTriadNpcStateProvider.Instance.IsNpcBeaten((int)Id) ? 71302 : 71301;
        }

        protected override uint GetId()
        {
            var tripleTriad = GetTripleTriad();
            return tripleTriad?.RowId ?? ExcelRow.RowId;
        }

        protected override string GetName()
        {
            return ExcelRow.Singular.ToString();
        }

        protected override string GetDescription()
        {
            return "";
        }

        protected override decimal GetPatchAdded()
        {
            return GetLocation()?.TerritoryType.ExVersion.RowId switch
            {
                0 => 2.0m,
                1 => 3.0m,
                2 => 4.0m,
                3 => 5.0m,
                4 => 6.0m,
                5 => 7.0m,
                _ => base.GetPatchAdded(),
            };
        }

        public static TripleTriadNpcCollectible Create(ENpcResident excelRow)
        {
            return new(excelRow);
        }

        private ENpcBase? GetNpcBase()
        {
            return ExcelCache<ENpcBase>.GetSheet().GetRow(ExcelRow.RowId);
        }

        private TripleTriad? GetTripleTriadByNpc(ENpcBase npc)
        {
            var tripleTriadSheet = ExcelCache<TripleTriad>.GetSheet();
            foreach (var npcData in npc.ENpcData)
            {
                var tripleTriad = tripleTriadSheet.GetRow(npcData.RowId);
                if (!tripleTriad.HasValue || tripleTriad.Value.Fee == 0)
                {
                    continue;
                }

                return tripleTriad.Value;
            }
            return null;
        }

        private TripleTriad? GetTripleTriad()
        {
            var npc = GetNpcBase();
            if (npc == null) return null;

            var tt = GetTripleTriadByNpc(npc.Value);
            if (tt == null) return null;

            return tt.Value;
        }

        private Location? GetLocation()
        {
            var hasValue = Services.DataGenerator.NpcLocationDataGenerator.npcToLocation.TryGetValue(ExcelRow.RowId, out var location);
            return hasValue ? location : null;
        }
    }
}
