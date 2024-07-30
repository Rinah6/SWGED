using API.Data;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    public class UsersConnectionsHistoryController : ControllerBase
    {
        private readonly UsersConnectionsHistoryRepository _usersConnectionsHistoryRepository;
        private readonly ProjectRepository _projectRepository;

        public UsersConnectionsHistoryController(UsersConnectionsHistoryRepository usersConnectionsHistoryRepository, ProjectRepository projectRepository)
        {
            _usersConnectionsHistoryRepository = usersConnectionsHistoryRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet("/api/users_connections")]
        public async Task<IActionResult> GetConnectedUsers()
        {
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            if (Enum.Parse<UserRole>(currentUserRole) == UserRole.SuperAdmin)
            {
                return Ok(await _usersConnectionsHistoryRepository.GetUsersConnections());
            }

            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            if (projectId == null)
            {
                return StatusCode(403);
            }

            return Ok(await _usersConnectionsHistoryRepository.GetUsersConnections((Guid)projectId));
        }

        [HttpGet("/api/connections_history/users/{userId}")]
        public async Task<IActionResult> GetUserConnectionsHistory(Guid userId)
        {
            return Ok(await _usersConnectionsHistoryRepository.GetUserConnectionsHistory(userId));
        }
    }
}
