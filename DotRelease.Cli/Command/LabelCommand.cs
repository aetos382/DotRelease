using System.CommandLine;

namespace DotRelease.Cli.Command;

internal sealed class LabelCommand :
    CliCommand
{
    public LabelCommand()
        : base(
            "label",
            Resources.LabelCommandDescription)
    {
    }
}
