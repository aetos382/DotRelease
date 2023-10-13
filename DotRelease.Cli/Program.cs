using System.CommandLine;
using System.CommandLine.Hosting;
using System.Diagnostics;
using System.Reflection;

using DotRelease.Cli;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Octokit.GraphQL;

using DotRelease.Cli.Command;

var commandLine = new CliConfiguration(new MainCommand())
    .UseHost(
        static args => Host.CreateDefaultBuilder(args),
        static hostBuilder =>
        {
            hostBuilder.ConfigureServices(
                static services =>
                {
                    services
                        .AddHttpClient<IConnection, Connection>(
                            static (client, serviceProvider) =>
                            {
                                var parseResult = serviceProvider.GetRequiredService<ParseResult>();
                                var connection = CreateConnection(client, parseResult);

                                return connection;
                            })
                        .AddHttpMessageHandler(
                            static services =>
                            {
                                var handler = new LoggingHandler(
                                    requestLogger: message => Debug.WriteLine(message),
                                    responseLogger: message => Debug.WriteLine(message));

                                return handler;
                            });
                });
        });

return commandLine.Invoke(args);

static Connection CreateConnection(
    HttpClient client,
    ParseResult parseResult)
{
    var version = Assembly
        .GetEntryAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion;

    var productInfo = new ProductHeaderValue("DotRelease", version);

    var url = parseResult.GetValue(MainCommand.ApiUrlOption)!;
    var token = parseResult.GetValue(MainCommand.ApiTokenOption)!;

    var tokenStore = new TokenStore(token);
    var connection = new Connection(productInfo, url, tokenStore, client);

    return connection;
}

file readonly struct TokenStore :
    ICredentialStore
{
    public TokenStore(
        string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        this._token = token;
    }

    public Task<string> GetCredentials(
        CancellationToken cancellationToken)
    {
        return Task.FromResult(this._token);
    }

    private readonly string _token;
}
