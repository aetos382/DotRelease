using System.CommandLine;

namespace DotRelease;

internal sealed class ReleaseCommand :
    CliCommand
{
    public ReleaseCommand()
        : base(
            "release",
            Resources.ReleaseCommandDescription)
    {
    }
}
