namespace CoreKit.SharedKernel.Common;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    DateTime? DeletedAtUtc { get; set; }

    Guid? DeletedBy { get; set; }
}