namespace RespApi.Models;

public class GroupModel{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid[] Users { get; set; } = null!;
    public DateTime CreationDate { get; set;}
}