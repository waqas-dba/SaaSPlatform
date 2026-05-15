namespace CoreKit.SharedKernel.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
}