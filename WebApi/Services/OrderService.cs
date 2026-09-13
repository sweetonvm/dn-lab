using Microsoft.Extensions.Options;
using WebApi.Constants;
using WebApi.Exceptions;
using WebApi.Models.Request;
using WebApi.Options;

namespace WebApi.Services;

/// <summary>
/// Calls the downstream orders API and maps failures to application exceptions.
/// </summary>
public sealed class OrderService(
    HttpClient httpClient,
    StandardRequestHeaders headers,
    IOptions<OrdersApiOptions> options
)
{
    private readonly OrdersApiOptions _options = options.Value;

    public async Task<string> PlaceOrderAsync(OrdersQuery query, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _options.BaseUrl);

        if (!string.IsNullOrWhiteSpace(headers.CorrelationId))
        {
            request.Headers.TryAddWithoutValidation(
                ApiConstants.CorrelationIdHeaderName,
                headers.CorrelationId
            );
        }

        if (!string.IsNullOrWhiteSpace(headers.SessionId))
        {
            request.Headers.TryAddWithoutValidation(
                ApiConstants.SessionIdHeaderName,
                headers.SessionId
            );
        }

        HttpResponseMessage response;

        try
        {
            response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
        }
        catch (HttpRequestException ex)
        {
            throw new DownstreamServiceException(
                ErrorMessages.Downstream,
                Ulid.NewUlid().ToString(),
                ex
            );
        }

        using (response)
        {
            if (query.DownstreamErrorScenario)
            {
                throw new DownstreamServiceException(
                    "Downstream error scenario requested.",
                    Ulid.NewUlid().ToString()
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new DownstreamServiceException(
                    $"Downstream service returned HTTP {(int)response.StatusCode}.",
                    Ulid.NewUlid().ToString()
                );
            }

            return $"Order processed for {headers.SessionId} and type: {query.OrderType}";
        }
    }
}
