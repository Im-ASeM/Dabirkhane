public class MsgLog:Parent
{
    public int Status { get; set; }
    public bool isDone { get; set; }
    public int UserId { get; set; }
    public Users User { get; set; }
    public int? MsgId { get; set; }
    public Messages? Msg { get; set; }
    public string Description { get; set; }
}