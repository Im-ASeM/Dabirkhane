public class Recivers : Parent
{
    public int? ReciverId { get; set; }
    public Users? Reciver { get; set; }
    public int? MessageId { get; set; }
    public Messages? Message { get; set; }
    public string? Type { get; set; } // to ، cc
    public bool isRead { get; set; }
    public int Status { get; set; } // 1) normal | 2) trash | 3) delete

    public Recivers()
    {
        this.ReciverId = null;
        this.Reciver = null;
        this.Message = null;
        this.MessageId = null;
        this.Type = null;
        this.isRead = false;
    }
}