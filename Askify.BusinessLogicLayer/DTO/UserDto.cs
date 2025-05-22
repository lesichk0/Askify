namespace Askify.BusinessLogicLayer.DTO
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsVerifiedExpert { get; set; }
        public bool IsBlocked { get; set; }
        public string Role { get; set; } = "User"; // Make sure Role property exists and has default
        public string? Email { get; set; } // Add Email property for better compatibility
    }

}
