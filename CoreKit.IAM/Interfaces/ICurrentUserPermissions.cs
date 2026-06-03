namespace CoreKit.IAM.Interfaces;

public interface ICurrentUserPermissions
{
    bool HasPermission(string permission);
    IReadOnlyList<string> GetPermissions();
}