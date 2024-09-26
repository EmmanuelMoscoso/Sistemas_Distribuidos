
using RestApi.Repositories;

namespace RestApi.Services;

public class GroupService : IGroupService
{

    private readonly IGroupRepository _groupRepository;

    public GroupService(IGroupRepository groupRepository){
        _groupRepository = groupRepository;
    }

    public async Task<GroupUserModel> GetGroupByIdAsync(string id, CancellationToken cancellationToken)
    {

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group is null){
            return null;
        }

        return new GroupUserModel {
            Id = Guid.NewGuid().ToString(),
            Name = "Group Test",
            CreationDate = DateTime.UtcNow
        };


       
    }
}