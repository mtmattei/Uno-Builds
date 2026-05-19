using Composer.Models;

namespace Composer.Services;

/// <summary>
/// Pure function over <see cref="IntentValues"/>. Extracts the domain noun,
/// user noun, and a few orthogonal flags so downstream layer models can
/// project stack- and domain-correct content without depending on AI
/// augmentation.
///
/// Per <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §10.2.
/// </summary>
public interface IContextDeriver
{
    DerivedContext Derive(IntentValues intent);
}
