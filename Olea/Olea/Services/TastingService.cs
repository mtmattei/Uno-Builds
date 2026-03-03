namespace Olea.Services;

public class TastingService
{
    private readonly List<TastingEntry> _journal;

    public TastingService()
    {
        _journal = new List<TastingEntry>(SeedData.Entries);
    }

    public IReadOnlyList<TastingEntry> GetJournal() => _journal.AsReadOnly();

    public int Count => _journal.Count;

    public void AddEntry(TastingEntry entry)
    {
        _journal.Insert(0, entry);
    }
}
