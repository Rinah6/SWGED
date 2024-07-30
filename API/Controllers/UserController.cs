using Microsoft.AspNetCore.Mvc;
using API.Data;
using Microsoft.AspNetCore.Authorization;
using API.Repositories;
using API.Data.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly RoleRepository _roleRepository;

        public UserController(UserRepository userRepository, ProjectRepository projectRepository, RoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
        }

        [HttpGet("/api/users")]
        public async Task<ActionResult> GetUsers()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            if (Enum.Parse<UserRole>(currentUserRole) == UserRole.SuperAdmin)
            {
                var admins = await _userRepository.GetAdmins();

                return Ok(admins);
            }

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            var users = await _userRepository.GetUsersByProjectId(project.Id);

            return Ok(users);
        }

        [HttpGet("/api/users/{userId}")]
        public async Task<ActionResult> GetUserInfo(Guid userId)
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            if (Enum.Parse<UserRole>(currentUserRole) != UserRole.Admin && Enum.Parse<UserRole>(currentUserRole) != UserRole.SuperAdmin)
            {
                return StatusCode(403);
            }

            var userInfo = await _userRepository.GetUserDetails(userId);

            if (userInfo == null)
            {
                return NotFound();
            }

            return Ok(new UserDetails
            {
                Username = userInfo.Username,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                Email = userInfo.Email,
                Role = userInfo.Role,
                ProjectId = userInfo.ProjectId,
                Sites = userInfo.Sites
            });
        }

        [HttpGet("/api/users/roles")]
        public async Task<ActionResult> GetRoles()
        {
            var roles = await _roleRepository.GetRoles();

            return Ok(roles);
        }

        [HttpGet("statistique")]
        public async Task<ActionResult> Getstatistique()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            if (Enum.Parse<UserRole>(currentUserRole) == UserRole.SuperAdmin)
            {
                return Ok(new
                {
                    curr_user = (await _userRepository.GetAdmins()).Count
                });
            }

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return StatusCode(403);
            }

            return Ok(new
            {
                curr_user = (await _userRepository.GetUsersByProjectId(project.Id)).Count
            });
        }

        [HttpPatch("/api/users/{Id}")]
        public async Task<ActionResult> UpdateUser(Guid Id, UserToUpdate userToUpdate)
        {
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            await _userRepository.Update(Id, userToUpdate, Enum.Parse<UserRole>(currentUserRole));

            return Ok();
        }

        [HttpDelete("/api/users/{Id}")]
        public async Task<ActionResult> Delete(Guid Id)
        {
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            if (Enum.Parse<UserRole>(currentUserRole) != UserRole.Admin && Enum.Parse<UserRole>(currentUserRole) != UserRole.SuperAdmin)
            {
                return StatusCode(403);
            }

            await _userRepository.Delete(Id, currentUserId);

            return Ok();
        }

        [HttpGet("/api/users/project")]
        public async Task<ActionResult> GetProjectIdByUserId()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var projectId = await _projectRepository.GetProjectIdByUserId(currentUserId);

            return Ok(projectId);
        }
    }
}
