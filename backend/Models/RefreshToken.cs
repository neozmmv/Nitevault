namespace nitevault.Models;

public class RefreshToken
{
    public int Id {get; set;}
    public Guid UserId {get; set;}
    public User User {get; set;} = null!;
    public string TokenHash {get; set;} = string.Empty;
    public DateTime CreatedAt = DateTime.UtcNow;
    public DateTime ExpiresAt {get; set;}
    public bool Revoked {get; set;} = false;
}