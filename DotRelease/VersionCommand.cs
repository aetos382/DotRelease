using System.CommandLine;

namespace DotRelease;

internal sealed class VersionCommand :
    CliCommand
{
    public VersionCommand()
        : base(
            "version",
            Resources.VersionCommandDescription)
    {
    }
}
