namespace nitevault.Dto;
public record FileDownloadInfo(Stream stream, string fileName, string contentType);