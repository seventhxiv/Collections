namespace Collections;

public class SettingsTab : IDrawable
{
    private List<string> collectionNames = new();
    public SettingsTab()
    {
        separatePreviewAndApply = Services.Configuration.SeparatePreviewAndApply;
        showAdditionalTooltips = Services.Configuration.AdditionalTooltips;
        autoOpenInstanceTab = Services.Configuration.AutoOpenInstanceTab;
        onlyOpenIfUncollected = Services.Configuration.OnlyOpenIfUncollected;
        autoHideObtainedFromInstanceTab = Services.Configuration.AutoHideObtainedFromInstanceTab;
        excludedCollectionsFromInstanceTab = Services.Configuration.ExcludedCollectionsFromInstanceTab;
        highVisibilityObtained = Services.Configuration.HighVisibilityObtained;
        collectionNames = Services.DataProvider.GetCollections().AsParallel().Select(col => col.Key).OrderBy((key) => key != GlamourCollectible.CollectionName).ThenBy(k => k).ToList();
    }

    private bool autoOpenInstanceTab;
    private bool separatePreviewAndApply;
    private bool onlyOpenIfUncollected;
    private bool autoHideObtainedFromInstanceTab;
    private bool highVisibilityObtained;
    private List<string> showAdditionalTooltips;
    private List<string> excludedCollectionsFromInstanceTab;
    public void Draw()
    {
        if(ImGui.Checkbox("Separate Preview and Add to Equip Slot", ref separatePreviewAndApply))
        {
            Services.Configuration.SeparatePreviewAndApply = separatePreviewAndApply;
            Services.Configuration.Save();
        }
        
        if (ImGui.Checkbox("Auto open Instance tab when entering an instance", ref autoOpenInstanceTab))
        {
            Services.Configuration.AutoOpenInstanceTab = autoOpenInstanceTab;
            Services.Configuration.Save();
        }

        // padding to signify this is a sub-option for auto-open
        ImGui.InvisibleButton("padding", new Vector2(15, 1));
        ImGui.SameLine();
        if (ImGui.Checkbox("Only open Instance tab if there are uncollected items ", ref onlyOpenIfUncollected))
        {
            Services.Configuration.OnlyOpenIfUncollected = onlyOpenIfUncollected;
            Services.Configuration.Save();
        }

        if (ImGui.Checkbox("Auto hide obtained items from Instance tab", ref autoHideObtainedFromInstanceTab))
        {
            Services.Configuration.AutoHideObtainedFromInstanceTab = autoHideObtainedFromInstanceTab;
            Services.Configuration.Save();
        }

        if (ImGui.Checkbox("Use green borders to indicate obtained items instead of checkmarks", ref highVisibilityObtained))
        {
            Services.Configuration.HighVisibilityObtained = highVisibilityObtained;
            Services.Configuration.Save();
        }

        ImGui.Text("Show additional item information for these collections");
        ImGui.BeginListBox("##collection-box-add-tooltips", new Vector2(300, 200));
        foreach (var collection in collectionNames)
        {
            bool isShown = showAdditionalTooltips.Contains(collection);
            if (ImGui.Checkbox($"{collection}", ref isShown))
            {
                if (isShown)
                    showAdditionalTooltips.Add(collection);
                else
                    showAdditionalTooltips.Remove(collection);
                Services.Configuration.Save();
            }
        }
        ImGui.EndListBox();

        ImGui.Text("Exclude these collections from the Instance tab");
        ImGui.BeginListBox("##collection-box", new Vector2(300, 200));
        foreach (var collection in collectionNames)
        {
            bool isExcluded = excludedCollectionsFromInstanceTab.Contains(collection);
            if (ImGui.Checkbox($"{collection}", ref isExcluded))
            {
                if (isExcluded)
                    excludedCollectionsFromInstanceTab.Add(collection);
                else
                    excludedCollectionsFromInstanceTab.Remove(collection);
                Services.Configuration.Save();
            }
        }
        ImGui.EndListBox();

    }

    public void OnOpen()
    {
        Dev.Log();
    }

    public void OnClose()
    {
        
    }

    public void Dispose()
    {
    }
}
