using Microsoft.AspNetCore.DataProtection;

namespace BFF.Data;

public sealed class TokenDatabase(IDataProtectionProvider provider)
{
    private readonly IDataProtector _protector = provider.CreateProtector(nameof(TokenDatabase));
    private readonly Dictionary<(string UserId, string Provider), TokenRecord> _tokens = new();
    private readonly Dictionary<(string UserId, string Provider), string?> _connectedAccounts = new();

    public void StoreToken(string userId, string provider, TokenRecord record)
    {
        _tokens[(userId, provider)] = record with
        {
            AccessToken = _protector.Protect(record.AccessToken),
            RefreshToken = string.IsNullOrEmpty(record.RefreshToken)
                ? record.RefreshToken
                : _protector.Protect(record.RefreshToken)
        };
    }

    public TokenRecord? GetToken(string userId, string provider)
    {
        if (!_tokens.TryGetValue((userId, provider), out var record))
            return null;

        return record with
        {
            AccessToken = _protector.Unprotect(record.AccessToken),
            RefreshToken = string.IsNullOrEmpty(record.RefreshToken)
                ? record.RefreshToken
                : _protector.Unprotect(record.RefreshToken)
        };
    }

    public bool IsConnected(string userId, string provider)
    {
        return _tokens.ContainsKey((userId, provider));
    }

    public bool Unlink(string userId, string provider)
    {
        _connectedAccounts.Remove((userId, provider));
        return _tokens.Remove((userId, provider));
    }

    public void StoreConnectedAccount(string userId, string provider, string? accountName)
    {
        _connectedAccounts[(userId, provider)] = accountName;
    }

    public string? GetConnectedAccount(string userId, string provider)
    {
        return _connectedAccounts.GetValueOrDefault((userId, provider));
    }
}

public static class TokenDatabaseExtensions
{
    public static IServiceCollection AddTokenDatabase(this IServiceCollection services)
    {
        services.AddDataProtection();
        services.AddSingleton<TokenDatabase>();

        return services;
    }
}