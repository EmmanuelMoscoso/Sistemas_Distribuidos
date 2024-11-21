using RestApi.Models;

namespace RestApi.Services;

public interface IGroupService{
    Task <GroupUserModel> GetGroupByIdAsync(string id, CancellationToken cancellationToken);
    Task<IList<GroupUserModel>> GetGroupsByNameAsync(string name, int pageNumber, int pageSize, string orderBy, CancellationToken cancellationToken);

    Task DeleteGroupByIdAsync(string id, CancellationToken cancellationToken);

    Task <GroupUserModel> CreateGroupAsync (string name, Guid [] Users, CancellationToken cancellationToken);

    Task <GroupUserModel> GetGroupByExactNameAsync(string name, CancellationToken cancellationTgitoken);

    Task UpdateGroupAsync(string id, string name, Guid[] users,CancellationToken cancellationToken);

    Task<bool> ValidateUserAsync(Guid[] users, CancellationToken cancellationToken);



}

