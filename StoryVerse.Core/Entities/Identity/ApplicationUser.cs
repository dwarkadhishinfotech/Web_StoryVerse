using Microsoft.AspNetCore.Identity;

namespace StoryVerse.Core.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfileImage { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
    public string? Theme { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
}
