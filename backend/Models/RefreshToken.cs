namespace JobTrail.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }            // FK scalar
    public User User { get; set; } = null!;     // navigation property
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? RevokedOn { get; set; }
}