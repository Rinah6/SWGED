using Microsoft.AspNetCore.Mvc;
using API.Data.Entities;
using API.Data;
using API.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ProjectRepository _projectRepository;

        public AuthenticationController(UserRepository userRepository, ProjectRepository projectRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
        }

        [HttpPost("/api/login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginCredentials loginCredentials)
        {
            var user = await _userRepository.GetByUsername(loginCredentials.Username);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!Utils.Password.IsValidPassword(loginCredentials.Password, user.Password))
            {
                return Unauthorized();
            }

            var claimsIdentity = new ClaimsIdentity(new[] {
                new Claim("Id", user.Id.ToString()),
                new Claim("role", user.RoleId.ToString())
            }, "10mOyIm3S1WMbwaCE7");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await Request.HttpContext.SignInAsync("10mOyIm3S1WMbwaCE7", claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddSeconds(12 * 60 * 60)
            });

            return Ok();
        }

        [HttpPost("/api/users/register")]
        public async Task<ActionResult> Register(UserToRegister userToRegister)
        {
            if (await _userRepository.DoesUsernameExist(userToRegister.Username))
            {
                return StatusCode(403);
            }

            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            var role = Enum.Parse<UserRole>(currentUserRole);

            if (role == UserRole.SuperAdmin || role == UserRole.Admin)
            {
                if (role == UserRole.Admin)
                {
                    var currentUserProject = await _projectRepository.GetProjectByUserId(currentUserId);

                    if (currentUserProject == null)
                    {
                        return StatusCode(403);
                    }

                    await _userRepository.Insert(userToRegister, currentUserProject.Id, currentUserId);
                }

                if (role == UserRole.SuperAdmin)
                {
                    await _userRepository.Insert(userToRegister, (Guid)userToRegister.ProjectId!, currentUserId);
                }

                return Ok();
            }

            return StatusCode(403);
        }

        [HttpGet("/api/users/credentials")]
        public async Task<ActionResult> GetUserCredentials()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);
            var currentUserRole = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")!.Value;

            if (Enum.Parse<UserRole>(currentUserRole) == UserRole.SuperAdmin)
            {
                return Ok(new UserCredentials
                {
                    Username = "Super Admin",
                    Role = Enum.Parse<UserRole>(currentUserRole),
                    IsADocumentsReceiver = false,
                    HasAccessToInternalUsersHandling = true,
                    HasAccessToSuppliersHandling = false,
                    HasAccessToProcessingCircuitsHandling = false,
                    HasAccessToSignMySelfFeature = true,
                    HasAccessToArchiveImmediatelyFeature = true,
                    HasAccessToGlobalDynamicFieldsHandling = false,
                    HasAccessToPhysicalLocationHandling = false,
                    HasAccessToNumericLibrary = true,
                    HasAccessToTomProLinking = false,
                    HasAccessToUsersConnectionsInformation = true,
                    HasAccessToDocumentTypesHandling = false,
                    HasAccessToDocumentsAccessesHandling = false,
                    HasAccessToRSF = false,
                });
            }

            var project = await _projectRepository.GetProjectByUserId(currentUserId);

            if (project == null)
            {
                return NotFound();
            }

            var isADocumentsReceiver = await _userRepository.IsUserADocumentsReceiver(currentUserId);

            var user = await _userRepository.GetUserDetails(currentUserId);

            if (user == null)
            {
                return StatusCode(403);
            }

            if (Enum.Parse<UserRole>(currentUserRole) == UserRole.Admin)
            {
                return Ok(new UserCredentials
                {
                    Username = user.Username,
                    Role = Enum.Parse<UserRole>(currentUserRole),
                    IsADocumentsReceiver = isADocumentsReceiver,
                    HasAccessToInternalUsersHandling = project.HasAccessToInternalUsersHandling,
                    HasAccessToSuppliersHandling = project.HasAccessToSuppliersHandling,
                    HasAccessToProcessingCircuitsHandling = project.HasAccessToProcessingCircuitsHandling,
                    HasAccessToSignMySelfFeature = project.HasAccessToSignMySelfFeature,
                    HasAccessToArchiveImmediatelyFeature = project.HasAccessToArchiveImmediatelyFeature,
                    HasAccessToGlobalDynamicFieldsHandling = project.HasAccessToGlobalDynamicFieldsHandling,
                    HasAccessToPhysicalLocationHandling = project.HasAccessToPhysicalLocationHandling,
                    HasAccessToNumericLibrary = project.HasAccessToNumericLibrary,
                    HasAccessToTomProLinking = project.HasAccessToTomProLinking,
                    HasAccessToUsersConnectionsInformation = project.HasAccessToUsersConnectionsInformation,
                    HasAccessToDocumentTypesHandling = project.HasAccessToDocumentTypesHandling,
                    HasAccessToDocumentsAccessesHandling = project.HasAccessToDocumentsAccessesHandling,
                    HasAccessToRSF = project.HasAccessToRSF,
                });
            }

            return Ok(new UserCredentials
            {
                Username = user.Username,
                Role = Enum.Parse<UserRole>(currentUserRole),
                IsADocumentsReceiver = isADocumentsReceiver,
                HasAccessToInternalUsersHandling = project.HasAccessToInternalUsersHandling,
                HasAccessToSuppliersHandling = false,
                HasAccessToProcessingCircuitsHandling = project.HasAccessToProcessingCircuitsHandling,
                HasAccessToSignMySelfFeature = project.HasAccessToSignMySelfFeature,
                HasAccessToArchiveImmediatelyFeature = project.HasAccessToArchiveImmediatelyFeature,
                HasAccessToGlobalDynamicFieldsHandling = project.HasAccessToGlobalDynamicFieldsHandling,
                HasAccessToPhysicalLocationHandling = project.HasAccessToPhysicalLocationHandling,
                HasAccessToNumericLibrary = project.HasAccessToNumericLibrary,
                HasAccessToTomProLinking = false,
                HasAccessToUsersConnectionsInformation = false,
                HasAccessToDocumentTypesHandling = project.HasAccessToDocumentTypesHandling,
                HasAccessToDocumentsAccessesHandling = project.HasAccessToDocumentsAccessesHandling,
                HasAccessToRSF = project.HasAccessToRSF,
            });
        }

        [HttpDelete("/api/logout")]
        public async Task<IActionResult> Logout()
        {
            await Request.HttpContext.SignOutAsync("10mOyIm3S1WMbwaCE7");

            return Ok();
        }
    }
}
