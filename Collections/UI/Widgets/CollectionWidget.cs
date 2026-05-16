namespace Collections;

public class CollectionWidget
{
    private const float IconSize = 68f;
    private float iconSizeScaled = 68f;
    private int pageSortWidgetWidth = "Sort By".Length * 13;
    private string searchFilter = ""; 
    public CollectibleSortOption PageSortOption { get; set; }
    private List<CollectibleSortOption> cachedOptions = [];
    private List<CollectibleFilterOption> cachedFilters = [];
    private bool isGlam { get; init; } = false;
    private EventService EventService { get; init; }
    private TooltipWidget CollectibleTooltipWidget { get; init; }
    public CollectionWidget(EventService eventService, bool isGlam, List<CollectibleSortOption>? collectibleSortOptions = null, List<CollectibleFilterOption>? filterOptions = null)
    {
        EventService = eventService;
        this.isGlam = isGlam;
        CollectibleTooltipWidget = new TooltipWidget(EventService);
        PageSortOption = new CollectibleSortOption("Patch", (c) => c.PatchAdded, true);
        if (collectibleSortOptions != null && collectibleSortOptions.Count > 1)
        {
            PageSortOption = collectibleSortOptions.First();
            cachedOptions = collectibleSortOptions;
        }
        if(filterOptions != null && filterOptions.Count > 1)
        {
            cachedFilters = filterOptions;
        }
        iconSizeScaled = UiHelper.ScaleForFontSize(IconSize);
    }

    private int obtainedState = 0;

    // Draws the collection. Utilizes ImGuiListClipper to optimize on large collections. If enableCollectionHeaders is set to true,
    // this will instead fall back to rendering based on 
    public unsafe void Draw(List<ICollectible> collectionList, bool enableFilters = true, bool enableCollectionHeaders = false)
    {
        // Draw filters
        if (enableFilters)
        {
            DrawFilters();
        }
        // separate to prevent constant reassignment
        if(iconSizeScaled != UiHelper.ScaleForFontSize(IconSize))
        {
            iconSizeScaled = UiHelper.ScaleForFontSize(IconSize);
        }

        drawItemCount = 0;
        var iconsPerRow = GetIconsPerRow();
        // sanity check
        if (iconsPerRow < 1) return;
        // used when adding header displays to align rows properly while using ListClipper
        int drawRowItemCount = 0;
        // only draws items currently within frame.
        ImGuiListClipper clipper = new ImGuiListClipper();

        // clipper based on the number of items per row, not items themselves
        clipper.Begin((int)Math.Ceiling(collectionList.Count / (double)iconsPerRow), iconSizeScaled);
        if (ImGui.BeginChild("scroll-area"))
        {
            // using full collection instead of clipped one, due to variable heights from the headers, and variable rows from ending early. We could in theory change
            // this to calculate everything based on pixel height and provide a way to seek to the next item manually instead of using ListClipper's automatic seeking,
            // but it seems like a pain to implement. I'll take a look at it though because headers are quite nice for organizing.
            if (enableCollectionHeaders)
            {
                ImGui.Selectable(collectionList.FirstOrDefault()?.GetCollectionName() ?? "");
                for (int i = 0; i < collectionList.Count; i++)
                {
                    var collectible = collectionList[i];
                    var icon = collectible.GetIcon();

                    if (icon is null)
                    {
                        continue;
                    }

                    DrawItem(collectible);
                    drawRowItemCount++;
                    drawItemCount++;

                    int nextIndex = i + 1;
                    if (nextIndex >= collectionList.Count) nextIndex = i;
                    var nextCollectible = collectionList[nextIndex];
                    if (collectible.GetCollectionName() != nextCollectible.GetCollectionName())
                    {
                        drawRowItemCount = iconsPerRow;
                        ImGui.Selectable(nextCollectible.GetCollectionName());
                    }

                    // Align item rows
                    if (drawRowItemCount < iconsPerRow)
                        ImGui.SameLine();
                    else
                        drawRowItemCount = 0;
                }
            }
            else
            {
                while (clipper.Step())
                {
                    for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
                    {
                        for (int col = 0; col < iconsPerRow; col++)
                        {
                            int i = (row * iconsPerRow) + col;

                            // sanity check
                            if (i >= collectionList.Count) break;

                            var collectible = collectionList[i];
                            var icon = collectible.GetIcon();

                            if (icon is null)
                            {
                                continue;
                            }

                            DrawItem(collectible);
                            drawRowItemCount++;
                            drawItemCount++;


                            // Align item rows
                            if (drawRowItemCount < iconsPerRow)
                                ImGui.SameLine();
                            else
                                drawRowItemCount = 0;
                        }
                    }
                }
            }
        }
        clipper.End();
        ImGui.EndChild();
    }

    private void DrawFilters()
    {
        ImGui.SetNextItemWidth(ImGui.GetColumnWidth() - pageSortWidgetWidth);
        string prev = searchFilter;
        if (ImGui.InputTextWithHint($"##Filter", "Filter...", ref searchFilter, 40))
        {
            EventService.Publish<FilterChangeEvent, FilterChangeEventArgs>(new FilterChangeEventArgs(searchFilter: searchFilter));
        };

        // default behavior cuts the dropdown a little bit off.
        ImGui.SameLine(ImGui.GetColumnWidth() - pageSortWidgetWidth + 4, 0);
        DrawSortOptions();

        

        ImGui.Text("Show:");
        ImGui.SameLine();

        if(ImGui.RadioButton("All", ref obtainedState, 0)) {
            EventService.Publish<FilterChangeEvent, FilterChangeEventArgs>(new FilterChangeEventArgs(itemStatusFilter: CollectibleStatusFilter.All));
        };
        ImGui.SameLine();

        if(ImGui.RadioButton("Obtained", ref obtainedState, 1)) {
            EventService.Publish<FilterChangeEvent, FilterChangeEventArgs>(new FilterChangeEventArgs(itemStatusFilter: CollectibleStatusFilter.Obtained));
        };
        ImGui.SameLine();

        if (ImGui.RadioButton("Unobtained", ref obtainedState, 2))
        {
            EventService.Publish<FilterChangeEvent, FilterChangeEventArgs>(new FilterChangeEventArgs(itemStatusFilter: CollectibleStatusFilter.Unobtained));
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Favorites", ref obtainedState, 3))
        {
            EventService.Publish<FilterChangeEvent, FilterChangeEventArgs>(new FilterChangeEventArgs(itemStatusFilter: CollectibleStatusFilter.Favorite));
        }
        // Advanced Filters
        DrawAdvancedFilters();

        if (isGlam)
        {
            // Preview Button
            if (ImGui.RadioButton("Preview", !Services.Configuration.ForceTryOn))
            {
                Services.Configuration.ForceTryOn = false;
            }
            ImGuiComponents.HelpMarker("Preview items on your character. Resets on window closing.\nDisabled for Mog Station items.");
            ImGui.SameLine();

            // Try On Button
            if (ImGui.RadioButton("Try On", Services.Configuration.ForceTryOn))
            {
                Services.Configuration.ForceTryOn = true;
            }
            ImGui.SameLine();

            // Reset Preview Button
            ImGui.PushStyleColor(ImGuiCol.Button, Services.WindowsInitializer.MainWindow.originalButtonColor);
            if (ImGui.Button("Reset Preview"))
            {
                Services.PreviewExecutor.ResetAllPreview();
            }
            ImGui.PopStyleColor();
            ImGui.SameLine();

            // Reapply Preview Button
            ImGui.PushStyleColor(ImGuiCol.Button, Services.WindowsInitializer.MainWindow.originalButtonColor);
            if (ImGui.Button("Reapply Preview"))
            {
                EventService.Publish<ReapplyPreviewEvent, ReapplyPreviewEventArgs>(new ReapplyPreviewEventArgs());
            }
            ImGui.PopStyleColor();
        }
    }

    private unsafe void DrawSortOptions()
    {
        if (cachedOptions.Count == 0) return;
        ImGui.SetNextItemWidth(pageSortWidgetWidth);

        if (ImGui.BeginCombo($"##sortCollectionDropdown", "Sort By", ImGuiComboFlags.HeightRegular))
        {
            foreach (var sortOpt in cachedOptions)
            {
                bool selected = PageSortOption.Equals(sortOpt);
                if (ImGui.RadioButton(sortOpt.Name, selected))
                {
                    // if user already has clicked on button, swap sort order
                    if (selected)
                    {
                        sortOpt.Reverse = !sortOpt.Reverse;
                    }
                    else
                    {
                        sortOpt.Reverse = sortOpt.ReverseDefault;
                    }
                    PageSortOption = sortOpt;
                    EventService.Publish<FilterChangeEvent, FilterChangeEventArgs>(new FilterChangeEventArgs(sortOptionSelected: PageSortOption));
                    selected = true;
                }
                if (selected)
                {
                    ImGui.SameLine();
                    UiHelper.DisabledIconButton(sortOpt.GetSortIcon(), "");
                }
            }
            ImGui.EndCombo();
        }
    }

    private void DrawAdvancedFilters()
    {
        ImGui.SameLine();
        if (ImGui.Button("More Filters"))
            ImGui.OpenPopup("##advancedFilters", ImGuiPopupFlags.None);
        if(ImGui.BeginPopup("##advancedFilters"))
        {
            foreach(var filter in cachedFilters)
            {
                filter.Draw(service: EventService);
            }
            ImGui.EndPopup(); 
        }
    }

    private int drawItemCount = 0;
    private Vector4 defaultTint = new(1f, 1f, 1f, 1f);
    private unsafe void DrawItem(ICollectible collectible)
    {
        // for debouncing, prevents interaction and favorite at the same time.
        var interact = false;
        var debounce = false;

        // to show red/green border instead of checkmark
        var showObtainedBorders = Services.Configuration.HighVisibilityObtained;

        // to properly draw everything
        var icon = collectible.GetIcon();

        var tint = collectible.GetIsObtained() ? defaultTint : ColorsPalette.GREY2;
        if (ImGui.ImageButton(icon.GetWrapOrEmpty().Handle, new Vector2(iconSizeScaled, iconSizeScaled), default, new Vector2(1f, 1f), -1, default, tint))
        {
        }

        if (ImGui.IsItemClicked())
        {
            interact = true;
        }


        if (showObtainedBorders)
        {
            var obtainedColor = collectible.GetIsObtained() ? ColorsPalette.LIME_GREEN : ColorsPalette.RED;

            // offset to compensate the frame padding 
            const float borderOffset = 2f;
            var min = ImGui.GetItemRectMin() + new Vector2(borderOffset, borderOffset);
            var max = ImGui.GetItemRectMax() - new Vector2(borderOffset, borderOffset);

            // Draw border
            ImGui.GetWindowDrawList().AddRect(min, max, ImGui.ColorConvertFloat4ToU32(obtainedColor), 8f, ImDrawFlags.None, 2f);
        }


        // for rendering additional content ontop of Icons 
        if (Services.Configuration.AdditionalTooltips.Contains(collectible.GetCollectionName()))
        {
            collectible.DrawAdditionalIconOverlay();
        }
        

        // Details on hover
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();

            ImGui.PushStyleColor(ImGuiCol.Text, ColorsPalette.GREY2);
            ImGui.Text("Right Click To Interact");
            ImGui.PopStyleColor();

            CollectibleTooltipWidget.DrawItemTooltip(collectible);
            ImGui.EndTooltip();
        }

        // Details on click
        if (ImGui.BeginPopupContextItem($"click-glam-item##{collectible.GetHashCode()}", ImGuiPopupFlags.MouseButtonRight))
        {
            if(collectible.GetType() == typeof(GlamourCollectible))
            {
                if(ImGui.Button("Apply to Glamour Slot"))
                {
                    Dev.Log("Publishing GlamourItemChangeEvent");
                    EventService.Publish<GlamourItemChangeEvent, GlamourItemChangeEventArgs>(new GlamourItemChangeEventArgs((GlamourCollectible)collectible, true));
                }
            }
            CollectibleTooltipWidget.DrawItemTooltip(collectible);
            ImGui.EndPopup();
        }

        // Favorite
        var isFavorite = collectible.IsFavorite();
        ImGui.SetItemAllowOverlap();
        UiHelper.IconButtonWithOffset(drawItemCount, FontAwesomeIcon.Star, ImGui.GetStyle().ItemSpacing.X * 2 + ImGui.GetFontSize(), 0, ref isFavorite, 1.0f);
        if(ImGui.IsItemClicked())
        {
            debounce = true;
        }
        if (isFavorite != collectible.IsFavorite())
        {
            collectible.SetFavorite(isFavorite);
            EventService.Publish<FilterChangeEvent, FilterChangeEventArgs>(new FilterChangeEventArgs());
        }

        if(interact && !debounce)
        {
            Dev.Log($"Interacting with {collectible.Name}");
            collectible.Interact();
            if (isGlam)
            {
                if (collectible.GetType() == typeof(GlamourCollectible))
                {
                    Dev.Log("Publishing GlamourItemChangeEvent");
                    EventService.Publish<GlamourItemChangeEvent, GlamourItemChangeEventArgs>(new GlamourItemChangeEventArgs((GlamourCollectible)collectible));
                }
                else if (collectible.GetType() == typeof(OutfitsCollectible))
                {
                    Dev.Log("Publishing OutfitsItemChangeEvent");
                    EventService.Publish<OutfitItemChangeEvent, OutfitItemChangeEventArgs>(new OutfitItemChangeEventArgs((OutfitsCollectible)collectible));
                }
            }
        }

        // Checkmark
        if (!showObtainedBorders)
        {
            // Mimicks the official FFXIV Yellow checkmark
            var obtained = collectible.GetIsObtained();
            // color
            // UiHelper.IconButtonWithOffset(drawItemCount, FontAwesomeIcon.Check, iconSize, 0, ref obtained, 1.0f, new Vector4(1f, .741f, .188f, 1), ColorsPalette.BLACK.WithAlpha(0));
            UiHelper.IconButtonWithOffset(drawItemCount, FontAwesomeIcon.Check, ImGui.GetStyle().ItemSpacing.X * 2 + ImGui.GetFontSize(), -iconSizeScaled + ImGui.GetFontSize(), ref obtained, 1.0f, new Vector4(1f, .741f, .188f, 1), ColorsPalette.BLACK.WithAlpha(0));
        }
    }

    private int GetIconsPerRow()
    {
        // Window Size / Icon Size + ImGui Item Padding x 2;
        return (int)Math.Floor((ImGui.GetWindowWidth() - ImGui.GetCursorPosX()) / (iconSizeScaled + (ImGui.GetStyle().ItemSpacing.X * 2)));
    }

    public bool IsFiltered(ICollectible collectible)
    {
        // Search filter
        if (searchFilter != "")
        {
            if (!collectible.Name.Contains(searchFilter, StringComparison.CurrentCultureIgnoreCase))
                return true;
        }

        // Obtain state filter
        var obtained = collectible.GetIsObtained();
        if ((obtained && obtainedState == 2) || (!obtained && obtainedState == 1))
            return true;
        if (obtainedState == 3 && !collectible.IsFavorite())
            return true;
        
        // supplied filters
        foreach(var filter in cachedFilters)
        {
            if(filter.IsFiltered(collectible))
            {
                return true;
            }
        }
        // Default
        return false;
    }
}
