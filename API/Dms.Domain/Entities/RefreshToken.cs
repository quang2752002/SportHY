namespace Dms.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime? Revoked { get; set; }
        public bool IsRevoked => Revoked != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Liên kết đến ApplicationUser
        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
