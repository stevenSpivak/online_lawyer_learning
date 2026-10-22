namespace Model
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public User() { }

        public User(string username, string email, string passwordHash, bool isActive = true)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            IsActive = isActive;
        }

        public User(int id, string username, string email, string passwordHash,
                    int roleId, string roleName, bool isActive, DateTime createdAt) : base(id)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            RoleId = roleId;
            RoleName = roleName;
            IsActive = isActive;
            CreatedAt = createdAt;
        }

        public override string ToString() => $"{Username} ({RoleName})";
    }
}