using Microsoft.AspNetCore.Mvc;
using RestApi.Dtos;
using RestApi.Mappers;
using RestApi.Services;
using RestApi.Exceptions;
using RespApi.Dtos;
using System.Net;
using RespApi.Exeptions;

namespace RestApi.Controller;

[ApiController]
[Route("[controller]")]
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
            var group = await _groupService.CreateGroupAsync(groupRequest.Name, groupRequest.Users, cancellationToken);
            return CreatedAtAction(nameof(GetGroupById), new { id = group.Id}, group.ToDto());
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