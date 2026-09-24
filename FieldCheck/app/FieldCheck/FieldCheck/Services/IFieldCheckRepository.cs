using FieldCheck.Models;

namespace FieldCheck.Services;

public interface IFieldCheckRepository
{
    /// <summary>Raised after a successful write so views can refresh.</summary>
    event EventHandler? DataChanged;

    Task<IReadOnlyList<Asset>> GetAssetsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Inspection>> GetInspectionsAsync(CancellationToken ct = default);

    /// <summary>Persists a new inspection and updates the asset's status and last inspection date.</summary>
    /// <exception cref="RepositoryException">The data could not be written.</exception>
    Task<Inspection> AddInspectionAsync(InspectionDraft draft, PickedFile? attachment, CancellationToken ct = default);
}

public sealed class RepositoryException(string message, Exception? inner = null) : Exception(message, inner);
