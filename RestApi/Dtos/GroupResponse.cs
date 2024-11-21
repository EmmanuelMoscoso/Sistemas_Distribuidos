namespace RestApi.Dtos;

public class GroupResponse{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<UserResponse> Users {get; set; } = null!;
    public DateTime CreationDate { get; set; }

}