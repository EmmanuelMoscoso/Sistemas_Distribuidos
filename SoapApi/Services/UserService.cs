using System.ServiceModel;
using Microsoft.AspNetCore.WebUtilities;
using SoapApi.Contracts;
using SoapApi.Dtos;
using SoapApi.Mappers;
using SoapApi.Repositories;

namespace SoapApi.Services;

public class UserService : IUserContract
{

    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IList<UserResponseDto>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        if(users is not null && users.Any()){
            return users.Select(user => user.ToDto()).ToList();
        }
        throw new FaultException(reason: "Users not found.");
    }

    public async Task<IList<UserResponseDto>> GetAllByEmail(string email, CancellationToken cancellationToken)
    {

        var users = await _userRepository.GetAllByEmailAsync(email, cancellationToken);

        var users2 = users.Where(s => s.Email.Contains(email)).ToList();

        if(users2.Any()){
            return users2.Select(user => user.ToDto()).ToList();
        }
        throw new FaultException(reason: "There is no user with " + email + " email");

    }

    public async Task<UserResponseDto> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if(user is not null){
            return user.ToDto();
        }

        throw new FaultException(reason: "User not found");
    }

    public async Task<bool> DeleteUserById (Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId,  cancellationToken);

        if (user is null){
            throw new FaultException("User not found");
        }

        await _userRepository.DeleteByIdAsync(user, cancellationToken);
        return true;
    }

    public async Task<UserResponseDto> CreateUser(UserCreateRequestDto userRequest, CancellationToken cancellationToken)
    {
        var user = userRequest.ToModel();
        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
        return createdUser.ToDto();
    }

    public async Task<UserResponseDto> UpdateUser(Guid userId, UserUpdateRequestDto userRequest, CancellationToken cancellationToken)
    {

    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

    if (user is null)
    {
        throw new FaultException($"User with ID {userId} not found.");
    }

    // Update user fields
    user.FirstName = userRequest.FirstName;
    user.LastName = userRequest.LastName;
    user.BirthDate = userRequest.Birthday; 

    // Save updated user
    var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);


    return updatedUser.ToDto();
}

}