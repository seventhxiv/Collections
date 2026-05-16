namespace Collections;

public class SourcesDataGenerator
{
    public ShopsDataGenerator ShopsDataGenerator { get; private set; }
    public InstancesDataGenerator InstancesDataGenerator { get; private set; }
    public EventDataGenerator EventDataGenerator { get; private set; }
    public MogStationDataGenerator MogStationDataGenerator { get; private set; }
    public ContainersDataGenerator ContainersDataGenerator { get; private set; }
    public AchievementsDataGenerator AchievementsDataGenerator { get; private set; }
    public PvPSeriesDataGenerator PvPDataGenerator { get; private set; }
    public QuestsDataGenerator QuestsDataGenerator { get; private set; }
    public CraftingDataGenerator CraftingDataGenerator { get; private set; }
    public TripleTriadNpcDataGenerator TripleTriadNpcDataGenerator { get; private set; }
    public TripleTriadNpcBattleDataGenerator TripleTriadNpcBattleDataGenerator { get; private set; }
    public SubmarineDataGenerator SubmarineDataGenerator {get; private set; }

    public SourcesDataGenerator()
    {
        Task.Run(AsyncInitializeDataGenerators).Wait();
    }

    private async Task AsyncInitializeDataGenerators()
    {
        var ShopsDataGeneratorTask = Task.Run(() => new ShopsDataGenerator());
        var InstancesDataGeneratorTask = Task.Run(() => new InstancesDataGenerator());
        var EventDataGeneratorTask = Task.Run(() => new EventDataGenerator());
        var MogStationDataGeneratorTask = Task.Run(() => new MogStationDataGenerator());
        var QuestsDataGeneratorTask = Task.Run(() => new QuestsDataGenerator());
        var ContainersDataGeneratorTask = Task.Run(() => new ContainersDataGenerator());
        var AchievementsDataGeneratorTask = Task.Run(() => new AchievementsDataGenerator());
        var PvPDataGeneratorTask = Task.Run(() => new PvPSeriesDataGenerator());
        var CraftingDataGeneratorTask = Task.Run(() => new CraftingDataGenerator());
        var TripleTriadNpcDataGeneratorTask = Task.Run(() => new TripleTriadNpcDataGenerator());
        var TripleTriadNpcBattleDataGeneratorTask = Task.Run(() => new TripleTriadNpcBattleDataGenerator());
        var SubmarineDataGeneratorTask = Task.Run(() => new SubmarineDataGenerator());

        ShopsDataGenerator = await ShopsDataGeneratorTask;
        InstancesDataGenerator = await InstancesDataGeneratorTask;
        EventDataGenerator = await EventDataGeneratorTask;
        MogStationDataGenerator = await MogStationDataGeneratorTask;
        QuestsDataGenerator = await QuestsDataGeneratorTask;
        ContainersDataGenerator = await ContainersDataGeneratorTask;
        AchievementsDataGenerator = await AchievementsDataGeneratorTask;
        PvPDataGenerator = await PvPDataGeneratorTask;
        CraftingDataGenerator = await CraftingDataGeneratorTask;
        TripleTriadNpcDataGenerator = await TripleTriadNpcDataGeneratorTask;
        TripleTriadNpcBattleDataGenerator = await TripleTriadNpcBattleDataGeneratorTask;
        SubmarineDataGenerator = await SubmarineDataGeneratorTask;
    }
}
