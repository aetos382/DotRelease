using System.CommandLine;

namespace DotRelease;

internal sealed class MainCommand :
    CliRootCommand
{
    public MainCommand()
        : base(
            Resources.MainCommandDescription)
    {
        this.Add(new LabelCommand());
        this.Add(new VersionCommand());
        this.Add(new ReleaseCommand());
    }
}
