namespace HomeService.Application.DTOs.Admin;

public class AdminUserListDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
}

public class AdminUserDetailDto : AdminUserListDto
{
    public string? ProfileImageUrl { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public string PreferredLanguage { get; set; } = string.Empty;
    public List<AddressDto> Addresses { get; set; } = new();
    public List<BookingDto> RecentBookings { get; set; } = new();
    public List<AdminNote> AdminNotes { get; set; } = new();
    public UserActivityTimeline Activity { get; set; } = new();
}

public class AdminNote
{
    public Guid Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserActivityTimeline
{
    public DateTime? LastLogin { get; set; }
    public int LoginCount { get; set; }
    public DateTime RegistrationDate { get; set; }
    public List<ActivityEvent> RecentActivity { get; set; } = new();
}

public class ActivityEvent
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
