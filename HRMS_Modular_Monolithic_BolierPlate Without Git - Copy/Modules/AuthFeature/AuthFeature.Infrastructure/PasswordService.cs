 // ================================================================
    // FILE: Modules/AuthFeature/AuthFeature.Infrastructure/PasswordService.cs
    // WHY: BCrypt password hashing — never store plain text passwords
    // INTERVIEW: Why BCrypt? Adaptive cost factor, built-in salt, replay protection
    // ================================================================
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool   VerifyPassword(string password, string hash);
    }
 
    public class PasswordService : IPasswordService
    {
        private const int WorkFactor = 12;  // 2^12 rounds — ~300ms per hash
 
        public string HashPassword(string password)  => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        public bool   VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
    