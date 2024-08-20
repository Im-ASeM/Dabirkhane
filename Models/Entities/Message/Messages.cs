public class Messages : Parent
{
    public string? SerialNumber { get; set; }
    public int? SerialNumberCode { get; set; }
    public int? SenderUserId { get; set; }
    public Users? SenderUser { get; set; }
    public string? Subject { get; set; }
    public string? BodyText { get; set; }
    public int Status4Sender { get; set; } // 1) normall | 2) trash | 3) delete

    public List<Files> files { get; set; }
    public List<Recivers> recivers { get; set; }
    public List<Reply> replies { get; set; }
    public List<MsgLog> logs { get; set; }

    public Messages()
    {
        this.SerialNumber = null;
        this.SenderUser = null;
        this.SenderUserId = null;
        this.Subject = null;
        this.BodyText = null;
    }
}