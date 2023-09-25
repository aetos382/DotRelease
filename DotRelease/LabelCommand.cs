using System.CommandLine;

namespace DotRelease;

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
