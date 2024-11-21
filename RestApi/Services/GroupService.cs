using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration.UserSecrets;
using RespApi.Exeptions;
using RestApi.Exceptions;
using RestApi.Models;
using RestApi.Repositories;

namespace RestApi.Services;

public class GroupService : IGroupService {

    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    public GroupService(IGroupRepository groupRepository, IUserRepository userRepository)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
    }

    public async Task DeleteGroupByIdAsync(string id, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group is null) {
            throw new GroupNotFoundException();
        }

        await _groupRepository.DeleteByIdAsync(id, cancellationToken);
    }

    public async Task<GroupUserModel> CreateGroupAsync (string name, Guid[] users, CancellationToken cancellationToken)
    {
        if(users.Length == 0) {
            throw new InvalidGroupRequestFormatException();
        }

        var groups = await _groupRepository.GetGroupsByNameAsync(name, 1, 1, "Name", cancellationToken);
        if (groups.Any()) {
            throw new GroupAlreadyExistsException();
        }
        var group = await _groupRepository.CreateAsync(name, users, cancellationToken);

        return new GroupUserModel{
            Id = group.Id,
            Name = group.Name,
            CreationDate = group.CreationDate,
            Users = (await Task.WhenAll(
                group.Users.Select(userId => _userRepository.GetByIdAsync(
                    userId, cancellationToken)))).Where(user => user != null)
                    .ToList()
        };
    }

    public async Task<GroupUserModel> GetGroupByIdAsync (string id, CancellationToken cancellationToken){

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group is null){
            throw new GroupNotFoundException();
        }

        return new GroupUserModel{
            Id = group.Id,
            Name = group.Name,
            CreationDate = group.CreationDate,
            Users = (await Task.WhenAll(
                group.Users.Select(userId => _userRepository.GetByIdAsync(
                    userId, cancellationToken)))).Where(user => user != null)
                    .ToList()
        };
    }

    public async Task<GroupUserModel> GetGroupByExactNameAsync(string name, CancellationToken cancellationToken)
    {

        var group = await _groupRepository.GetByExactNameAsync(name, cancellationToken);

        if (group is null)
        {
            throw new GroupNotFoundException();
        }

        return new GroupUserModel{
            Id = group.Id,
            Name = group.Name,
            CreationDate = group.CreationDate,
            Users = (await Task.WhenAll(
                group.Users.Select(userId => _userRepository.GetByIdAsync(
                    userId, cancellationToken)))).Where(user => user != null)
                    .ToList()
        };
    }

    public async Task<IList<GroupUserModel>> GetGroupsByNameAsync(string name, int pageNumber, int pageSize, string orderBy, CancellationToken cancellationToken){
        
        var groups = await _groupRepository.GetGroupsByNameAsync(name, pageNumber, pageSize, orderBy, cancellationToken);
        
        var groupUserModels = await Task.WhenAll(groups.Select(async group => new GroupUserModel{
            Id = group.Id,
            Name = group.Name,
            CreationDate = group.CreationDate,
            Users = (await Task.WhenAll(group.Users.Select(async user => await _userRepository.GetByIdAsync(user, cancellationToken)))).ToList()
        }));

        return groupUserModels.ToList();
    }

    public async Task UpdateGroupAsync(string id, string name, Guid[] users, CancellationToken cancellationToken)
    {
        if(users.Length == 0) {
            throw new InvalidGroupRequestFormatException();
        }

        var group = await _groupRepository.GetByIdAsync(id, cancellationToken);
        if (group is null){
            throw new GroupNotFoundException();
        }

        var groups = await _groupRepository.GetByExactNameAsync(name, cancellationToken);
        if (groups is not null && groups.Id != id){
            throw new GroupAlreadyExistsException();
        }

        await _groupRepository.UpdateGroupAsync(id, name, users, cancellationToken);
    }

    public async Task<bool> ValidateUserAsync(Guid[] users, CancellationToken cancellationToken){
        var validUsers = await Task.WhenAll(users.Select(async x => await _userRepository.GetByIdAsync(x,cancellationToken) != null));
        return validUsers.All(s => s == true);

    }

    


    
}