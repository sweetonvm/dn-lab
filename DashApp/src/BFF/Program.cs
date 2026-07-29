using BFF;
using BFF.Auth.Providers.Github;
using BFF.Auth.Providers.Microsoft;
using BFF.Data;
using BFF.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTokenDatabase()
    .AddCookieAuth(builder.Configuration)
    .AddGithubOAuth(builder.Configuration);
    
if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
}

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapLoginEndpoints();
app.MapConnectEndpoints();
app.MapDashboardEndpoints();

if (app.Environment.IsDevelopment())
{
    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Path == BffRoutes.Dashboard &&
            ctx.User.Identity?.IsAuthenticated != true)
        {
            ctx.Response.Redirect("/");
            return;
        }

        await next();
    });

    app.MapReverseProxy();
}

app.Run();
