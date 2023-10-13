using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Runtime.CompilerServices;

using DotRelease.Cli;

using Octokit.GraphQL;
using Octokit.GraphQL.Model;

using static Octokit.GraphQL.Variable;

namespace DotRelease.Cli.Command;

internal sealed class VersionCommand :
    CliCommand
{
    public VersionCommand()
        : base(
            "version",
            Resources.VersionCommandDescription)
    {
        this.Action = CommandHandler.Create(this.Handler);
    }

    private async Task<int> Handler(
        IConnection connection,
        RepositoryName repository,
        string branch,
        string settings,
        CancellationToken cancellationToken)
    {
        var settingsObject = await connection
            .GetSettings(repository, branch, settings, cancellationToken)
            .ConfigureAwait(false);

        var branchSetting = settingsObject.Branches
            .SingleOrDefault(b => b.Name == branch);

        if (branchSetting is null)
        {
            throw new Exception();
        }

        var releases = GetReleases(connection, repository, cancellationToken).ConfigureAwait(false);

        await foreach (var release in releases)
        {

        }

        return 0;
    }

    private static async IAsyncEnumerable<ReleaseCommitInfo> GetReleases(
        IConnection connection,
        RepositoryName repositoryName,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var variables = new Dictionary<string, object?>
        {
            ["cursor_after"] = null
        };

        var releasesQuery = new Query()
            .Repository(
                repositoryName.Name,
                repositoryName.Owner)
            .Releases(
                10,
                Var("cursor_after"),
                orderBy: new ReleaseOrder
                {
                    Field = ReleaseOrderField.CreatedAt,
                    Direction = OrderDirection.Desc
                })
            .Select(static x => new
            {
                Commits = x
                    .Nodes
                    .Select(static x => new
                    {
                        x.TagName,
                        x.TagCommit.Oid,
                        x.IsPrerelease
                    })
                    .ToList(),
                PageInfo = new
                {
                    x.PageInfo.HasNextPage,
                    x.PageInfo.EndCursor
                }
            })
            .Compile();

        bool hasNextPage = false;

        do
        {
            var queryResult = await connection
                .Run(releasesQuery, variables, cancellationToken)
                .ConfigureAwait(false);

            foreach (var commit in queryResult.Commits)
            {
                var result = new ReleaseCommitInfo(
                    commit.TagName,
                    commit.Oid,
                    commit.IsPrerelease);

                yield return result;
            }

            hasNextPage = queryResult.PageInfo.HasNextPage;
            variables["cursor_after"] = queryResult.PageInfo.EndCursor;
        }
        while (hasNextPage);
    }

    private readonly record struct ReleaseCommitInfo(
        string TagName,
        string TagCommitOid,
        bool IsPrerelease);

    private static async IAsyncEnumerable<string> GetCommits(
        Connection connection,
        RepositoryName repositoryName,
        string startCommitOid,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var repositoryQuery = new Query()
            .Repository(
                repositoryName.Name,
                repositoryName.Owner);

        var cursorQuery = repositoryQuery
            .Object(oid: startCommitOid)
            .Cast<Commit>()
            .History(first: 1)
            .Select(static x =>
                new
                {
                    Commits = x
                        .Nodes
                        .Select(static x => new
                        {
                            x.Oid,
                            x.Message
                        })
                        .ToList(),
                    PageInfo = new
                    {
                        x.PageInfo.HasPreviousPage,
                        x.PageInfo.HasNextPage,
                        x.PageInfo.StartCursor,
                        x.PageInfo.EndCursor
                    }
                })
            .Compile();

        var cursorQueryResult = await connection
            .Run(cursorQuery, null, cancellationToken)
            .ConfigureAwait(false);

        var variables = new Dictionary<string, object?>
        {
            ["cursor_before"] = cursorQueryResult.PageInfo.StartCursor
        };

        var commitsQuery = repositoryQuery
            .Object(oid: startCommitOid)
            .Cast<Commit>()
            .History(last: 100, before: Var("cursor_before"))
            .Select(static x => x.Nodes.Select(static x => x.Oid).ToList())
            .Compile();

        var hasPreviousPage = false;

        do
        {
            var commitsQueryResult = await connection
                .Run(commitsQuery, variables, cancellationToken)
                .ConfigureAwait(false);

            foreach (var commit in commitsQueryResult)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return commit;
            }

            // hasPreviousPage = result.PageInfo.HasPreviousPage;
        }
        while (hasPreviousPage);
    }
}
