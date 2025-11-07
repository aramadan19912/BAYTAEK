namespace HomeService.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
