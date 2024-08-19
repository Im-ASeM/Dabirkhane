using System.ComponentModel.DataAnnotations;

public class Parent
{
    [Key]
    public int Id { get; set; }
    public DateTime? CreateDateTime { get; set; }

    
    public Parent()
    {
        this.CreateDateTime = null;
    }
}