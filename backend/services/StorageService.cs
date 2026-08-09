using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using nitevault.Dto;

public class StorageService
{
    private readonly HttpClient _botClient;
    private readonly AppDbContext _db;
    private readonly long _storageChatId;

    public StorageService(HttpClient botClient, AppDbContext db)
    {
        _botClient = botClient;
        _db = db;

        var chatIdValue = Environment.GetEnvironmentVariable("CHAT_ID");
        if (string.IsNullOrEmpty(chatIdValue)) throw new InvalidOperationException("Could not read CHAT_ID from env file!");
        _storageChatId = long.Parse(chatIdValue);
    }

    public async Task<Guid> UploadAsync(Stream fileStream, string fileName, string contentType, long fileSize, Guid userId)
    {
        FileDto telegramResult = await SendToTelegramAsync(fileStream, fileName);

        FileEntity file = new FileEntity
        {
            UserId = userId,
            OriginalFileName = fileName,
            ContentType = contentType,
            TotalSize = fileSize
        };

        file.Parts.Add(new FilePart
        {
            PartNumber = 1, // 1 for now since there is no chunking yet
            PartSize = fileSize,
            TelegramFileId = telegramResult.fileId,
            ChatId = telegramResult.chatId,
            MessageId = telegramResult.messageId 
        });

        _db.Files.Add(file);
        await _db.SaveChangesAsync();

        return file.Id;
    }

    private async Task<FileDto> SendToTelegramAsync(Stream fileStream, string fileName)
    {
       using var content = new MultipartFormDataContent();
       content.Add(new StringContent(_storageChatId.ToString()), "chat_id");
       content.Add(new StringContent("true"), "disable_content_type_detection"); // forces document

       var fileContent = new StreamContent(fileStream);
       content.Add(fileContent, "document", fileName);

       var response = await _botClient.PostAsync("sendDocument", content);
       response.EnsureSuccessStatusCode();
       
       /* if(!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Telegram API ERROR: {errorBody}");
        } */
       
       var json = await response.Content.ReadFromJsonAsync<JsonElement>();
       var result = json.GetProperty("result");
       var document = result.GetProperty("document");

       return new FileDto(
        document.GetProperty("file_id").GetString()!,
        result.GetProperty("chat").GetProperty("id").GetInt64(),
        result.GetProperty("message_id").GetInt64()
       );
    }

    public async Task<List<FileEntity>> GetUserFiles(Guid userId)
    {
        var userFiles = await _db.Files.Where(f => f.UserId == userId).ToListAsync();
        return userFiles;
    }

    public async Task<FileEntity?> GetFileById(Guid fileId, Guid userId)
    {
        FileEntity? file = await _db.Files.Include(f => f.Parts).FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
        return file;
    }

    public async Task<FileDownloadInfo?> GetFileForDownloadAsync(Guid fileId, Guid userId)
    {
        var file = await _db.Files
            .Include(f => f.Parts.OrderBy(p => p.PartNumber))
            .FirstOrDefaultAsync(f => f.Id == fileId);
        
        if (file is null || file.UserId != userId) return null;

        var part = file.Parts.First();
        var filePath = await GetFile(part.TelegramFileId);
        var stream = File.OpenRead(filePath);

        return new FileDownloadInfo(stream, file.OriginalFileName, file.ContentType);
    }

    private async Task<string> GetFile(string fileId)
    {
        var response = await _botClient.GetAsync($"getFile?file_id={Uri.EscapeDataString(fileId)}");

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Telegram API ERROR: {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<TelegramGetFileResponse>();

        if(result is null || !result.Ok) throw new Exception("Failed to get the file info from Telegram!");

        return result.Result.FilePath;
    }

    public async Task<bool> DeleteFile(Guid fileId, Guid userId)
    {
        FileEntity? file = await GetFileById(fileId, userId);
        if (file is null) return false;

        foreach (var part in file.Parts)
        {
            var response = await _botClient.PostAsJsonAsync("deleteMessage", new
            {
                chat_id = part.ChatId,
                message_id = part.MessageId
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Telegram API Error: {errorBody}");
            }
        }

        _db.Files.Remove(file);
        await _db.SaveChangesAsync();
        return true;
    }
}