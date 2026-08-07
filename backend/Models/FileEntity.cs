

public class FileEntity
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public Guid? FolderId {get; set;}
    public string OriginalFileName {get; set;} = string.Empty;
    public string ContentType {get; set;} = string.Empty;
    public long TotalSize {get; set;}
    public DateTime CreatedAt {get; set;}
    public List<FilePart> Parts {get; set;} = new();
}