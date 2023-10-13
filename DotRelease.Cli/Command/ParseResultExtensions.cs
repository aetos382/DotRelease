using System.CommandLine;

using Settings = DotRelease.Cli.Setting.Settings;

namespace DotRelease.Cli.Command;

internal static class ParseResultExtensions
{
    public static RepositoryName GetRepositoryName(
        this ParseResult parseResult)
    {
        return parseResult.GetValue(MainCommand.RepositoryNameOption)!;
    }

    public static string GetBranchName(
        this ParseResult parseResult)
    {
        return parseResult.GetValue(MainCommand.BranchNameOption)!;
    }
}
