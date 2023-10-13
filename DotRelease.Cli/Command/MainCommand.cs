using System.CommandLine;
using System.CommandLine.Parsing;

using Octokit.GraphQL;

namespace DotRelease.Cli.Command;

internal sealed class MainCommand :
    CliRootCommand
{
    public MainCommand()
        : base(
            Resources.MainCommandDescription)
    {
        this.Add(ApiUrlOption);
        this.Add(ApiTokenOption);
        this.Add(RepositoryNameOption);
        this.Add(BranchNameOption);
        this.Add(SettingsFileOption);

        this.Add(new LabelCommand());
        this.Add(new VersionCommand());
        this.Add(new ReleaseCommand());
    }

    static MainCommand()
    {
        ApiUrlOption = new CliOption<Uri>("--api-url")
        {
            Recursive = true,
            CustomParser = ParseApiUrl,
            DefaultValueFactory = _ => Connection.GithubApiUri
        };

        ApiTokenOption = new CliOption<string>("--api-token")
        {
            Required = true,
            Recursive = true
        };

        RepositoryNameOption = new CliOption<RepositoryName>("--repository", "-r")
        {
            Required = true,
            Recursive = true,
            CustomParser = ParseRepositoryName
        };

        BranchNameOption = new CliOption<string>("--branch", "-b")
        {
            Required = true,
            Recursive = true
        };

        var settingsFileOption = new CliOption<string>("--settings")
        {
            Required = true,
            Recursive = true
        };

        settingsFileOption.AcceptLegalFilePathsOnly();

        SettingsFileOption = settingsFileOption;
    }

    internal static CliOption<Uri> ApiUrlOption { get; }

    internal static CliOption<string> ApiTokenOption { get; }

    internal static CliOption<RepositoryName> RepositoryNameOption { get; }

    internal static CliOption<string> BranchNameOption { get; }

    internal static CliOption<string> SettingsFileOption { get; }

    private static Uri? ParseApiUrl(
        ArgumentResult argumentResult)
    {
        if (!Uri.TryCreate(argumentResult.Tokens[0].Value, UriKind.Absolute, out var result))
        {
            argumentResult.AddError("Invalid format.");
            return null;
        }

        return result;
    }

    private static RepositoryName? ParseRepositoryName(
        ArgumentResult argumentResult)
    {
        if (!RepositoryName.TryParse(argumentResult.Tokens[0].Value, out var result))
        {
            argumentResult.AddError("Invalid format.");
            return null;
        }

        return result;
    }
}
