using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RestApi.Dtos;
using RestApi.Mappers;
using RestApi.Services;
using RestApi.Exceptions;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace RestApi.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class GroupsController : ControllerBase {

    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }
    //localhosts:port/groups/192282892929
    [HttpGet("{id}")]
    public async Task <ActionResult<GroupResponse>> GetGroupById(string id, CancellationToken cancellationToken){
        var group = await _groupService.GetGroupByIdAsync(id, cancellationToken);

        if (group is null){
            return NotFound();
        }

        return Ok(group.ToDto());
    }

    [HttpGet("SearchByExactName")]
    public async Task<ActionResult> GetGroupByExactNameAsync(string name, CancellationToken cancellationToken)
    {
        var group = await _groupService.GetGroupByExactNameAsync(name, cancellationToken);

        if (group is null)
        {
            return NotFound();
        }  
        return Ok(group.ToDto());
        
    }

    

    [HttpGet]
    public async Task<ActionResult<IList<GroupResponse>>> GetGroupsByName([FromQuery] string name, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string orderBy, CancellationToken cancellationToken)
    {
        var groups = await _groupService.GetGroupsByNameAsync(name, pageNumber, pageSize, orderBy, cancellationToken);

        return Ok(groups.Select(group => group.ToDto()).ToList());
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteGroup(String id, CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.DeleteGroupByIdAsync(id, cancellationToken);
            return NoContent();
        }
        catch (GroupNotFoundException) {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<GroupResponse>> CreateGroup([FromBody] CreateGroupRequest groupRequest, CancellationToken cancellationToken)
    {
        try
        {
            var validUsers = await _groupService.ValidateUserAsync(groupRequest.Users, cancellationToken);
            if (!validUsers)
            {
                return BadRequest("One or more users are invalid.");
            }

            var group = await _groupService.CreateGroupAsync(groupRequest.Name, groupRequest.Users, cancellationToken);
            return CreatedAtAction(nameof(GetGroupById), new { id = group.Id }, group.ToDto());
        }
        catch (InvalidGroupRequestFormatException)
        {
            return BadRequest(NewValidationProblemDetails("One or more validation errors occurred.",
            HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", new[] { "Users array is empty" }}
            }));
        }
        catch (GroupAlreadyExistsException)
        {
            return Conflict(NewValidationProblemDetails("One or more validation errors occurred.",
            HttpStatusCode.Conflict, new Dictionary<string, string[]>{
                {"Groups", new[] { "Group with the same name already exists." }}
            }));
        }
        catch (UserValidationException)
        {
            return BadRequest(NewValidationProblemDetails("One or more validation errors occurred.",
            HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", new[] { "Users array is empty" }}
            }));
        }
       
    }


    //PU localhost:8080/groups/dnauifheqiu78
    [HttpPut("{id}")]
    public async Task <ActionResult> UpdateGroup(string id, [FromBody] UpdateGroupRequest groupRequest, CancellationToken cancellationToken){
        try {

            var validUsers = await _groupService.ValidateUserAsync(groupRequest.Users, cancellationToken);
            if (!validUsers)
            {
                return BadRequest("One or more users are invalid.");
            }

            await _groupService.UpdateGroupAsync(id, groupRequest.Name, groupRequest.Users, cancellationToken);
            return NoContent();

        }
        catch(GroupNotFoundException)
        {
            return NotFound();
        }

        catch (InvalidGroupRequestFormatException)
        {
            return BadRequest(NewValidationProblemDetails("One or more validation errors ocurred.",
            HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", ["Users array is empty"]}
            }));
        }
        catch(GroupAlreadyExistsException)
        {
            return Conflict(NewValidationProblemDetails("One or more validation errors ocurred.",
            HttpStatusCode.Conflict, new Dictionary<string, string[]>{
                {"Groups", ["Group with same name alreadye exists."]}
            }));
        }
        catch (UserValidationException)
        {
            return BadRequest(NewValidationProblemDetails("One or more validation errors occurred.",
            HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", new[] { "Users array is empty" }}
            }));
        }
    }


    private static ValidationProblemDetails NewValidationProblemDetails(string title, HttpStatusCode statusCode, Dictionary<string, string[]> errors)
    {
        return new ValidationProblemDetails {
            Title = title,
            Status = (int) statusCode,
            Errors = errors
        };

    }

    

}