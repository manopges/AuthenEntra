using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace AuthenEntra.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(GraphServiceClient graphClient) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentUser(
        CancellationToken cancellationToken = default)
    {
        var user = await graphClient.Me.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Select =
                ["id", "displayName", "userPrincipalName", "mail", "jobTitle", "department"];
        }, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserProfileResponse(
            user.Id,
            user.DisplayName,
            user.UserPrincipalName,
            user.Mail,
            user.JobTitle,
            user.Department));
    }
}

public sealed record UserProfileResponse(
    string? Id,
    string? DisplayName,
    string? UserPrincipalName,
    string? Mail,
    string? JobTitle,
    string? Department);