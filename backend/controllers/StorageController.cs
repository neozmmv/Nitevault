using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;

[ApiController]
[Route("/api/storage")]
public class StorageController : ControllerBase
{
    private readonly StorageService _storage;

    public StorageController(StorageService storage)
    {
        _storage = storage;
    }

    [Authorize]
    [HttpPost("upload")]
    [RequestSizeLimit(2_100_000_000)]
    public async Task<ActionResult> Upload(IFormFile file)
    {
        var userId = User.GetAuthorizedTokenOwner();
        await using var stream = file.OpenReadStream();
        var fileId = await _storage.UploadAsync(stream, file.FileName, file.ContentType, file.Length, userId);
        return Ok(new {fileId});
    }

    [Authorize]
    [HttpGet("download/{fileId}")]
    public async Task<ActionResult> Download(Guid fileId)
    {
        var userId = User.GetAuthorizedTokenOwner();

        FileDownloadInfo? result = await _storage.GetFileForDownloadAsync(fileId, userId);
        if (result is null) return NotFound();

        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{result.fileName}\"");
        return File(result.stream, result.contentType);
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<ActionResult> ListFiles()
    {
        var userId = User.GetAuthorizedTokenOwner();
        var fileList = await _storage.GetUserFiles(userId);
        return Ok(fileList);
    }

    [Authorize]
    [HttpGet("file/{fileId}")]
    public async Task<ActionResult> FileInfo(Guid fileId)
    {
        var userId = User.GetAuthorizedTokenOwner();
        FileEntity? file = await _storage.GetFileById(fileId, userId);
        return Ok(file);
    }

    [Authorize]
    [HttpDelete("file/{fileId}")]
    public async Task<ActionResult> DeleteFile(Guid fileId)
    {
        var userId = User.GetAuthorizedTokenOwner();
        bool deleted = await _storage.DeleteFile(fileId, userId);
        if (!deleted) return NotFound();

        return NoContent();
    }
}