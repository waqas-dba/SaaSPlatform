namespace CoreKit.IAM.Interfaces;

public interface IRoleDocumentRequirementService
{
    Task AddRequirementAsync(Guid roleId, string documentType);
    Task RemoveRequirementAsync(Guid roleId, string documentType);
    Task<List<string>> GetRequiredDocumentsAsync(Guid roleId);
    Task<bool> IsRequiredAsync(Guid roleId, string documentType);
}