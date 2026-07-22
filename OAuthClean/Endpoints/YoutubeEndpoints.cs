using System.Net;
using System.Security.Claims;
using OAuthClean.Clients;

namespace OAuthClean.Endpoints;

public static class YoutubeEndpoints
{
    public static IEndpointRouteBuilder MapYoutubeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/yt/info", GetChannelInfo)
            .RequireAuthorization("youtube-enabled");

        return app;
    }

    private static async Task<IResult> GetChannelInfo(HttpContext ctx, YoutubeServiceClient youtube)
    {
        var token = ctx.User.FindFirstValue("yt-access-token");
        if (string.IsNullOrEmpty(token))
            return Results.Unauthorized();

        try
        {
            var json = await youtube.GetMyChannelAsync(token);
            return Results.Content(json, "application/json");
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError));
        }
    }
}