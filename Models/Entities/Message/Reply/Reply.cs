public class Reply : Parent
{
    public int? MessageId { get; set; }
    public Messages? Message { get; set; }
    public int? ReciverId { get; set; }
    public Recivers? Reciver { get; set; }
    public string? Subject { get; set; }
    public string? BodyText { get; set; }
    public bool isRead { get; set; }
    public List<Files> files { get; set; }

    public Reply()
    {
        this.MessageId = null;
        this.Message = null;
        this.ReciverId = null;
        this.Reciver = null;
        this.Subject = null;
        this.BodyText = null;
    }
}