namespace HomeService.Domain.Enums;

public enum ModerationAction
{
    Warning = 1,
    TemporarySuspension = 2,
    PermanentBan = 3,
    ContentRemoval = 4,
    AccountRestriction = 5,
    NoAction = 6
}
