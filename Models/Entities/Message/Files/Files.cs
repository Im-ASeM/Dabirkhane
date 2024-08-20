using System.ComponentModel.DataAnnotations.Schema;

public class Files : Parent
{
    public string? FileName { get; set; }
    [ForeignKey(nameof(CreatorUser))]
    public int CreatorUserId { get; set; }
    public Users CreatorUser { get; set; }
    public int MessageId { get; set; }
    public Messages Message { get; set; }
    public int? ReplyId { get; set; }
    public Reply? Reply { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }

    public Files()
    {
        this.FileName = null;
        this.FilePath = null;
        this.FileType = null;
    }
}