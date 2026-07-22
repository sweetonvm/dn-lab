using System.Net.Http.Headers;

namespace OAuthClean.Clients;

public class YoutubeServiceClient(HttpClient http)
{
    public async Task<string> GetMyChannelAsync(
        string accessToken,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "channels?part=snippet&mine=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}

public static class YoutubeClientExtensions
{
    public static IServiceCollection AddYoutubeClient(this IServiceCollection services)
    {
        services.AddHttpClient<YoutubeServiceClient>(client =>
            client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/"));

        return services;
    }
}