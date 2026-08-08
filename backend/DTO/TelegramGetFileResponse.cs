namespace nitevault.Dto;
using System.Text.Json.Serialization;

public record TelegramGetFileResponse(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("result")] TelegramFileResult Result
);

public record TelegramFileResult(
    [property: JsonPropertyName("file_id")] string FileId,
    [property: JsonPropertyName("file_unique_id")] string FileUniqueId,
    [property: JsonPropertyName("file_size")] long FileSize,
    [property: JsonPropertyName("file_path")] string FilePath
);