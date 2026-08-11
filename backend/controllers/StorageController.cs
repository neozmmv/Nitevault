using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nitevault.Dto;

[ApiController]
[Route("/api/storage")]
public class StorageController : ControllerBase
{
    private readonly StorageService _storage;
    private readonly DownloadTokenService _dts;

    public StorageController(StorageService storage, DownloadTokenService dts)
    {
        _storage = storage;
        _dts = dts;
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

    // [Authorize]
    [HttpGet("download/{fileId}")]
    public async Task<ActionResult> Download(Guid fileId, [FromQuery] string token)
    {
        var result = await _dts.ValidateToken(token);
        if (result is null) return Unauthorized();

        var (tokenFileId, tokenUserId) = result.Value;

        if (tokenFileId != fileId) return Unauthorized();

        FileDownloadInfo? fileInfo = await _storage.GetFileForDownloadAsync(fileId, tokenUserId);
        if (fileInfo is null) return NotFound();

        Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileInfo.fileName}\"");
        return File(fileInfo.stream, fileInfo.contentType);
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

    [Authorize]
    [HttpGet("generateToken/{fileId}")]
    public async Task<ActionResult> GetDownloadToken(Guid fileId)
    {
        Guid userId = User.GetAuthorizedTokenOwner();

        var file = await _storage.GetFileById(fileId, userId);
        if (file is null)
            return NotFound();

        string token = await _dts.GenerateToken(fileId, userId);
        return Ok(new {token});
    }
}