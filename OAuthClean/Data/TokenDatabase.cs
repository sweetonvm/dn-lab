namespace OAuthClean.Data;

// Placeholder for an interface (ITokenRepository) backed by Redis or SQL
public class TokenDatabase : Dictionary<string, string>;

public static class TokenDatabaseExtensions
{
    public static IServiceCollection AddTokenDatabase(this IServiceCollection services)
        => services.AddSingleton<TokenDatabase>();
}