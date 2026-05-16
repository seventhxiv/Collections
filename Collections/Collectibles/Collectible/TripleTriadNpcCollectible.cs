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
            // This is guessing Patch based on map expansion.
            // todo better approach for retrieving Triad NPC quest
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
                if (!tripleTriad.HasValue)
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

        private IEnumerable<Quest> GetRequiredQuests()
        {
            var tripleTriad = GetTripleTriad();
            if (tripleTriad is null)
            {
                yield break;
            }

            foreach (var quest in tripleTriad.Value.PreviousQuest)
            {
                if (quest.RowId == 0)
                {
                    continue;
                }

                yield return quest.Value;
            }
        }

        private Location? GetLocation()
        {
            var hasValue = Services.DataGenerator.NpcLocationDataGenerator.npcToLocation.TryGetValue(ExcelRow.RowId, out var location);
            return hasValue ? location : null;
        }

        public override void DrawAdditionalTooltip()
        {
            var location = GetLocation();
            var quests = GetRequiredQuests().ToList();

            if (ImGui.BeginTable($"##tt-npc-{ExcelRow.RowId}-additional-tooltip", 2, ImGuiTableFlags.NoHostExtendX))
            {
                ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, UiHelper.UnitWidth() * 14);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

                if (location is not null)
                {
                    DrawTooltipRow("Map", location.TerritoryType.PlaceName.Value.Name.ToString());
                    DrawTooltipRow("Location", $"X:{location.Xmap:F1} Y:{location.Ymap:F1}");
                }

                if (quests.Count > 0)
                {
                    DrawTooltipRow(quests.Count == 1 ? "Quest Req" : "Quests Req", string.Join("\n", quests.Select(quest => quest.Name.ToString())));
                }

                ImGui.EndTable();
            }
        }

        private static void DrawTooltipRow(string label, string value)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(ColorsPalette.GREY2, label);
            ImGui.TableNextColumn();
            ImGui.PushTextWrapPos(UiHelper.UnitWidth() * 50);
            ImGui.TextUnformatted(value);
            ImGui.PopTextWrapPos();
        }
    }
}
