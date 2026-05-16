namespace Collections;

public class TripleTriadNpcKey : CollectibleKey<(ENpcResident, int)>, ICreateable<TripleTriadNpcKey, (ENpcResident, int)>
{
    public TripleTriadNpcKey((ENpcResident, int) input) : base(input)
    {
    }

    public static TripleTriadNpcKey Create((ENpcResident, int) input)
    {
        return new(input);
    }

    protected override string GetName((ENpcResident, int) input)
    {
        return input.Item1.Singular.ToString();
    }

    protected override uint GetId((ENpcResident, int) input)
    {
        return input.Item2 == 0 ? input.Item1.RowId : (uint)input.Item2;
    }

    protected override List<ICollectibleSource> GetCollectibleSources((ENpcResident, int) input)
    {
        return new List<ICollectibleSource>() { new NpcSource(input.Item1) };
    }

    protected override HashSet<SourceCategory> GetBaseSourceCategories()
    {
        return new HashSet<SourceCategory>();
    }

    public override Tradeability GetIsTradeable()
    {
        return Tradeability.Untradeable;
    }
}
