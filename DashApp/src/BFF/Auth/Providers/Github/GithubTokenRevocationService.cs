using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BFF.Auth.Providers.Github;

public class GithubTokenRevocationService(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<bool> RevokeAsync(string accessToken, CancellationToken ct = default)
    {
        var clientId = configuration["Github:ClientId"]
                       ?? throw new InvalidOperationException("Github:ClientId missing");
        var clientSecret = configuration["Github:ClientSecret"]
                           ?? throw new InvalidOperationException("Github:ClientSecret missing");

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"https://api.github.com/applications/{clientId}/token");

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}")));
        request.Headers.UserAgent.ParseAdd("BFF");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Content = JsonContent.Create(new { access_token = accessToken });

        using var response = await httpClient.SendAsync(request, ct);

        // 204 = revoked. 404 = token already invalid/unknown to GitHub treat as success
        return response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound;
    }
}