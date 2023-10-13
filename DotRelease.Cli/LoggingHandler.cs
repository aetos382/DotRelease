using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace DotRelease.Cli;

internal class LoggingHandler :
    DelegatingHandler
{
    private readonly Action<string>? _requestLogger;
    private readonly Action<string>? _responseLogger;
    private readonly Action<Exception>? _exceptionHandler;

    public LoggingHandler(
        Action<string>? requestLogger = null,
        Action<string>? responseLogger = null,
        Action<Exception>? exceptionHandler = null)
    {
        this._requestLogger = requestLogger;
        this._responseLogger = responseLogger;
        this._exceptionHandler = exceptionHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (this._requestLogger is { } requestLogger)
            {
                var requestString = await PrintRequest(request, cancellationToken).ConfigureAwait(false);
                requestLogger(requestString);
            }

            var response = await base
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (this._responseLogger is { } responseLogger)
            {
                var responseString = await PrintResponse(response, cancellationToken).ConfigureAwait(false);
                responseLogger(responseString);
            }

            return response;
        }
        catch (Exception e)
        {
            this._exceptionHandler?.Invoke(e);
            throw;
        }
    }

    private static async Task<string> PrintRequest(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var logBuilder = new StringBuilder();

        logBuilder.AppendLine(
            CultureInfo.InvariantCulture,
            $"HTTP/{request.Version} {request.Method} {request.RequestUri}");

        PrintHeaders(logBuilder, request.Headers);

        var content = request.Content;

        if (content is not null)
        {
            PrintHeaders(logBuilder, content.Headers);

            logBuilder.AppendLine();

            var contentString = await content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            logBuilder.AppendLine(contentString);
        }

        var requestLog = logBuilder.ToString();

        return requestLog;
    }

    private static async Task<string> PrintResponse(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var logBuilder = new StringBuilder();

        logBuilder.AppendLine(
            CultureInfo.InvariantCulture,
            $"HTTP/{response.Version} {response.StatusCode} {response.ReasonPhrase}");

        PrintHeaders(logBuilder, response.Headers);

        var content = response.Content;

        PrintHeaders(logBuilder, content.Headers);

        logBuilder.AppendLine();

        var contentString = await content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        logBuilder.AppendLine(contentString);

        logBuilder.AppendLine();

        PrintHeaders(logBuilder, response.TrailingHeaders);

        var requestLog = logBuilder.ToString();

        return requestLog;
    }

    private static void PrintHeaders(
        StringBuilder stringBuilder,
        HttpHeaders headers)
    {
        foreach (var (key, values) in headers)
        {
            stringBuilder.AppendLine(
                CultureInfo.InvariantCulture,
                $"{key}: {string.Join(",", values)}");
        }
    }
}
