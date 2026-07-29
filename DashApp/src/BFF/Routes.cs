namespace BFF;

public static class BffRoutes
{
    public const string Login = "/login";
    public const string Logout = "/logout";
    public const string Dashboard = "/dashboard";
    public const string ApiPrefix = "/api";
    public const string ApiDashboard = "/api/dashboard";
    public const string ApiSession = "/api/session";
    public const string Connect = "/connect/{provider}";
    public const string Unlink = "/unlink/{provider}";
    public const string GithubCallback = "/oauth/github-cb";

    public static string ConnectUrl(string provider)
    {
        return $"/connect/{provider}";
    }
}