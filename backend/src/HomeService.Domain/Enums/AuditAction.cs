namespace HomeService.Domain.Enums;

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    Logout = 5,
    PasswordChange = 6,
    PasswordReset = 7,
    EmailVerified = 8,
    PhoneVerified = 9,
    StatusChange = 10,
    PaymentProcessed = 11,
    RefundIssued = 12,
    Other = 99
}
