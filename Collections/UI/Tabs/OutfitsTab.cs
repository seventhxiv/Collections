
namespace Collections;

public class OutfitsTab : IDrawable
{
    private int collectionSize;
    private int obtainedCount;
    private List<ICollectible> filteredCollection { get; set; }
    private JobSelectorWidget JobSelectorWidget { get; init; }
    private ContentFiltersWidget ContentFiltersWidget { get; init; }
    private EquipSlotsWidget EquipSlotsWidget { get; init; }
    private CollectionWidget CollectionWidget { get; init; }
    private EventService EventService { get; init; }
    public OutfitsTab()
    {
        EventService = new EventService();
        JobSelectorWidget = new JobSelectorWidget(EventService);
        ContentFiltersWidget = new ContentFiltersWidget(EventService, 2);
        EquipSlotsWidget = new EquipSlotsWidget(EventService);
        EquipSlotsWidget.currentGlamourSet = new GlamourSet("outfits preview");
        filteredCollection = GetInitialCollection();
        collectionSize = filteredCollection.Count();
        CollectionWidget = new CollectionWidget(EventService, true, filteredCollection.FirstOrDefault()?.GetSortOptions(), filteredCollection.FirstOrDefault()?.GetFilterOptions());

        ApplyFilters();

        EventService.Subscribe<FilterChangeEvent, FilterChangeEventArgs>(OnPublish);
        EventService.Subscribe<OutfitItemChangeEvent, OutfitItemChangeEventArgs>(OnPublish);
        EventService.Subscribe<ReapplyPreviewEvent, ReapplyPreviewEventArgs>(OnPublish);
    }

    private const int SpaceBetweenFilterWidgets = 3;

    public void Draw()
    {
        // Dev.Start();

        // if (ImGui.BeginTable("glam-tree", 1, ImGuiTableFlags.Borders | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.SizingFixedFit))
        // {
        //     ImGui.TableSetupColumn("Glamour Sets", ImGuiTableColumnFlags.None, UiHelper.UnitWidth() * GlamourSetsWidgetWidth);
        //     ImGui.TableHeadersRow();

        //     ImGui.TableNextRow(ImGuiTableRowFlags.None, UiHelper.GetLengthToBottomOfWindow());
        //     ImGui.TableNextColumn();

        //     ImGui.EndTable();
        // }
        // ImGui.SameLine();

        ImGui.ProgressBar((float)obtainedCount / collectionSize, new(UiHelper.GetLengthToRightOfWindow() - UiHelper.UnitWidth() * 2, UiHelper.UnitHeight() * 1f), $"{obtainedCount}/{collectionSize}");
        // Equip slot buttons
        ImGui.BeginGroup();
        if (ImGui.BeginTable("equip-slots", 1, ImGuiTableFlags.Borders | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Equip Slots", ImGuiTableColumnFlags.None); // Not setting width here, allowing equip slot icon size to dictate width
            ImGui.TableHeadersRow();

            ImGui.TableNextRow(ImGuiTableRowFlags.None, UiHelper.GetLengthToBottomOfWindow());
            ImGui.TableNextColumn();
            EquipSlotsWidget.Draw();

            ImGui.EndTable();
        }
        ImGui.EndGroup();
        ImGui.SameLine();

        // Filters
        ImGui.BeginGroup();
        if (ImGui.BeginTable("filters", 1, ImGuiTableFlags.Borders | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.SizingFixedSame))
        {
            ImGui.TableNextColumn();
            ImGui.TableHeader("Equipped By");

            ImGui.TableNextRow(ImGuiTableRowFlags.None, (UiHelper.GetLengthToBottomOfWindow() / 2) - (UiHelper.UnitHeight() * SpaceBetweenFilterWidgets));
            ImGui.TableNextColumn();
            JobSelectorWidget.Draw();

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableHeader("Content Filters");

            ImGui.TableNextRow(ImGuiTableRowFlags.None, UiHelper.GetLengthToBottomOfWindow());
            ImGui.TableNextColumn();
            ContentFiltersWidget.Draw();

            ImGui.EndTable();
        }

        ImGui.EndGroup();

        // Glam collection
        ImGui.SameLine();
        ImGui.BeginGroup();

        CollectionWidget.Draw(filteredCollection); 
        //ImGui.Text("");

        ImGui.EndGroup();

        //var drawTime = Dev.Stop(false);
        //DrawTimer(drawTime);
    }

    private double drawCount = 1;
    private double drawAvg = 0;
    private void DrawTimer(double drawTime)
    {
        drawAvg = drawAvg + (drawTime - drawAvg) / drawCount;
        drawCount++;
        ImGui.Text(drawAvg.ToString());

        ImGui.SameLine();
        if (ImGui.Button("Reset"))
        {
            drawCount = 1;
            drawAvg = 0;
        }

        if (drawCount > 1000)
        {
            drawCount = 1;
            drawAvg = 0;
        }
        ImGui.Text(" ");
    }

    public void OnOpen()
    {
        Dev.Log();

        Task.Run(() =>
        {
            foreach (var collectible in Services.DataProvider.GetCollection<OutfitsCollectible>())
            {
                collectible.UpdateObtainedState();
            }

            obtainedCount = filteredCollection.Count(e => e.GetIsObtained());
        });
    }

    List<ICollectible> GetInitialCollection() => Services.DataProvider.GetCollection<OutfitsCollectible>();

    private void ApplyFilters()
    {
        // Refresh all filters (1) Equip slot (2) content type (3) job
        var contentFilters = ContentFiltersWidget.Filters.Where(d => d.Value).Select(d => d.Key);
        var jobFilters = JobSelectorWidget.Filters.Where(d => d.Value).Select(d => d.Key).ToList();

        // (1) Equip Slot filter
        filteredCollection = CollectionWidget.PageSortOption.SortCollection(GetInitialCollection())
        // (2) Content type filters
        .Where(c => c.CollectibleKey is not null)
        .Where(c => !contentFilters.Any() || contentFilters.Intersect(c.CollectibleKey.SourceCategories).Any())
        // (3) job filters
        .Where(c =>
            {
                // show all items if all filters disabled
                if (JobSelectorWidget.IsAllActive() || JobSelectorWidget.IsAllInactive())
                    return true;
                var itemJobCat = ((OutfitKey)c.CollectibleKey).FirstItem.ClassJobCategory.Value;
                // only show "All Classes" items if toggled
                if (itemJobCat.RowId < 2) return JobSelectorWidget.AllClasses();
                var itemJobs = itemJobCat.GetJobs();
                if(JobSelectorWidget.JobSpecific()) {
                    // Shows all job-specific gear if there aren't any specific jobs being looked for
                    if(!jobFilters.Any() && itemJobs.Count <= 2)
                        return true;
                    else if(itemJobs.Count > 2)
                        return false;
                }
                foreach (var jobFilter in jobFilters)
                {
                    if (itemJobs.Contains(jobFilter))
                    {
                        return true;
                    }
                }
                return false;
            })
        // Order
        .Where(c => !CollectionWidget.IsFiltered(c))
        .ToList();
    }

    public void OnPublish(OutfitItemChangeEventArgs args)
    {
        // revert to original player armor if this is going to the preview window
        if (Services.Configuration.ForceTryOn || args.Collectible.CollectibleKey.SourceCategories.Contains(SourceCategory.MogStation))
        {
            Services.PreviewExecutor.ResetAllPreview();
        }
        else
        {
            // Fully unequip models to preview outfit's items without other interference
            foreach (var equipSlot in Services.DataProvider.SupportedEquipSlots)
            {
                Services.PreviewExecutor.ResetSlotPreview(equipSlot);
            }
        }

        // Preview the selected set
        foreach (var (equipSlot, glamourItem) in EquipSlotsWidget.currentGlamourSet.Items)
        {
            var stain0Id = EquipSlotsWidget.paletteWidgets[equipSlot].ActiveStainPrimary.RowId;
            var stain1Id = EquipSlotsWidget.paletteWidgets[equipSlot].ActiveStainSecondary.RowId;
            Services.PreviewExecutor.PreviewWithTryOnRestrictions(glamourItem.GetCollectible(), stain0Id, stain1Id, Services.Configuration.ForceTryOn);
        }
    }

    public void OnPublish(FilterChangeEventArgs args)
    {
        ApplyFilters();
    }

    public void OnPublish(ReapplyPreviewEventArgs args)
    {
        Services.PreviewExecutor.ResetAllPreview();
        foreach (var (equipSlot, glamourItem) in EquipSlotsWidget.currentGlamourSet.Items)
        {
            var collectible = CollectibleCache<GlamourCollectible, Item>.Instance.GetObject(glamourItem.ItemId);
            Services.PreviewExecutor.PreviewWithTryOnRestrictions(collectible, glamourItem.Stain0Id, glamourItem.Stain1Id, Services.Configuration.ForceTryOn);
        }
    }

    public void Dispose()
    {
    }

    public void OnClose()
    {
    }
}

