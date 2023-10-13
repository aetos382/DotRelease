using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DotRelease;

[DebuggerDisplay("{Owner,nq}/{Name,nq}")]
public sealed record RepositoryName(
    string Owner,
    string Name)
{
    public static bool TryParse(
        string value,
        [MaybeNullWhen(false)] out RepositoryName result)
    {
        ArgumentNullException.ThrowIfNull(value);

        var parts = value.Split('/');

        if (parts is not [{ Length: > 0 } owner, { Length: > 0 } name])
        {
            result = null;
            return false;
        }

        result = new RepositoryName(owner, name);
        return true;
    }
}
