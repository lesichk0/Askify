namespace Askify.BusinessLogicLayer.Options
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenExpiryDays { get; set; } = 365; // Increased to 365 days for long-lasting sessions
    }
}
