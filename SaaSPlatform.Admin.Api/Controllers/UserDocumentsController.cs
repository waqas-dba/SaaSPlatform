using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v1/admin/users/{userId:guid}/documents")]
[Authorize]
public class UserDocumentsController : ApiControllerBase
{
    private readonly IUserDocumentService _documentService;

    public UserDocumentsController(IUserDocumentService documentService)
        => _documentService = documentService;

    [HttpGet]
    [RequiresPermission(Permissions.Documents.View)]
    public async Task<IActionResult> GetAll(Guid userId, CancellationToken ct)
    {
        var docs = await _documentService.GetByUserAsync(userId);
        return OkResponse(docs.Select(d => new { d.Id, d.DocumentType, d.ImageUrl, d.CreatedAt }));
    }

    [HttpGet("{documentType}")]
    [RequiresPermission(Permissions.Documents.View)]
    public async Task<IActionResult> Get(Guid userId, string documentType, CancellationToken ct)
    {
        var doc = await _documentService.GetAsync(userId, documentType);
        return doc is null ? NotFound() : OkResponse(new { doc.Id, doc.DocumentType, doc.ImageUrl, doc.CreatedAt });
    }

    [HttpPost("{documentType}")]
    [RequiresPermission(Permissions.Documents.Upload)]
    public async Task<IActionResult> Upload(Guid userId, string documentType, [FromBody] UploadDocumentRequest request, CancellationToken ct)
    {
        await _documentService.AddOrUpdateAsync(userId, documentType, request.ImageUrl);
        return CreatedResponse("Document uploaded successfully.");
    }

    [HttpDelete("{documentType}")]
    [RequiresPermission(Permissions.Documents.Delete)]
    public async Task<IActionResult> Delete(Guid userId, string documentType, CancellationToken ct)
    {
        await _documentService.DeleteAsync(userId, documentType);
        return DeletedResponse();
    }
}

public class UploadDocumentRequest
{
    public string ImageUrl { get; set; } = default!;
}