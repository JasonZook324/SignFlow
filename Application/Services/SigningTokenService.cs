using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;

namespace SignFlow.Application.Services;

public class SigningTokenService
{
    private readonly AppDbContext _db;
    public SigningTokenService(AppDbContext db) { _db = db; }

    public async Task<SigningToken> CreateAsync(Guid proposalId, TimeSpan lifetime)
    {
        var token = GenerateToken();
        var entity = new SigningToken
        {
            Id = Guid.NewGuid(),
            ProposalId = proposalId,
            Token = token,
            ExpiresUtc = DateTime.UtcNow.Add(lifetime)
        };
        _db.SigningTokens.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<SigningToken?> GetValidAsync(string token)
    {
        var entity = await _db.SigningTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (entity == null) return null;
        if (entity.UsedUtc != null) return null;
        if (DateTime.UtcNow > entity.ExpiresUtc) return null;
        return entity;
    }

    public async Task MarkUsedAsync(SigningToken token)
    {
        token.UsedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static string GenerateToken()
    {
        // 128-bit token -> 22 char Base64Url
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Base64Url(bytes.ToArray());
    }

    private static string Base64Url(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
