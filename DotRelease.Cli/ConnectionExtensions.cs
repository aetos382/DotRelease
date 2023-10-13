using Octokit.GraphQL;
using Octokit.GraphQL.Model;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Settings = DotRelease.Cli.Setting.Settings;

using static Octokit.GraphQL.Variable;

namespace DotRelease.Cli;

internal static class ConnectionExtensions
{
    public static async Task<Settings> GetSettings(
        this IConnection connection,
        RepositoryName repositoryName,
        string branchName,
        string settingsFilePath,
        CancellationToken cancellationToken)
    {
        var variables = new Dictionary<string, object?>
        {
            ["repository_owner"] = repositoryName.Owner,
            ["repository_name"] = repositoryName.Name,
            ["ref_qualified_name"] = $"refs/heads/{branchName}",
            ["file_path"] = settingsFilePath
        };

        var settingsFileContent = await connection
            .Run(_settingsFileQuery.Value, variables, cancellationToken)
            .ConfigureAwait(false);

        var settings = _deserializer.Value.Deserialize<Settings>(settingsFileContent);
        return settings;
    }

    private static readonly Lazy<ICompiledQuery<string>> _settingsFileQuery =
        new(() => new Query()
            .Repository(Var("repository_name"), Var("repository_owner"))
            .Ref(Var("ref_qualified_name"))
            .Target
            .Cast<Commit>()
            .File(Var("file_path"))
            .Object
            .Cast<Blob>()
            .Select(static x => x.Text)
            .Compile());

    private static readonly Lazy<IDeserializer> _deserializer =
        new(() => new DeserializerBuilder()
            .WithDuplicateKeyChecking()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build());

}
