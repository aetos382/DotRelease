using System.CommandLine;

namespace DotRelease.Cli.Command;

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
