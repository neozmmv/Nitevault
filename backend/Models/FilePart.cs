

public class FilePart
{
    public Guid Id {get; set;}
    public Guid FileId {get; set;}
    public int PartNumber {get; set;}
    public long PartSize {get; set;}
    public string TelegramFileId {get; set;} = string.Empty;
    public long ChatId {get; set;}
    public long MessageId {get;set;}
    public FileEntity File {get; set;} = null!;
}