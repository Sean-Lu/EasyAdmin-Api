namespace EasyAdmin.Web.Models;

public class RefreshTokenModel
{
    public long UserId { get; set; }
    public long TenantId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}

public class TokenBlacklistItem
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; }
}