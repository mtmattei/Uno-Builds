using Windows.Storage;

namespace FreewriteUno.Services;

public sealed partial class EntryStore
{
    public EntryStore() : this(DefaultRoot()) { }

    private static string DefaultRoot()
    {
        var local = ApplicationData.Current.LocalFolder.Path;
        return Path.Combine(local, FolderName);
    }
}
