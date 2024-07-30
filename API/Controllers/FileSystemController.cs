using API.Repositories;
using elFinder.AspNet.Drawing;
using elFinder.AspNet.Drivers.FileSystem;
using elFinder.AspNet.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.AspNet.Web.Controllers
{
    [Route("el-finder/file-system")]
    public class FileSystemController : Controller
    {
        private readonly ProjectRepository _projectRepository;

        public FileSystemController(ProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        [HttpGet("/api/storage")]
        public async Task<IActionResult> GetUserStorageLocation()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            var userStorage = await _projectRepository.GetUserStorageLocation(currentUserId);

            return Ok(userStorage);
        }

        [HttpGet("connector/{userStorage}")]
        public async Task<IActionResult?> Connector(string userStorage)
        {
            var connector = GetConnector(userStorage);

            var parameters = Request.Query.ToDictionary(k => k.Key, v => v.Value);

            var result = (await connector.ProcessAsync(parameters)).Value;

            if (result is FileContent)
            {
                var file = result as FileContent;

                return File(file.ContentStream, file.ContentType);
            }

            return Json(result);
        }

        [HttpPost("connector/{userStorage}")] // put/upload are HTTP POST
        public async Task<IActionResult?> ConnectorPost(string userStorage)
        {
            var connector = GetConnector(userStorage);

            var parameters = Request.Form.ToDictionary(k => k.Key, v => v.Value);

            if (Request.Form.Files.Count > 0)
            {
                var files = new List<FileContent>();
                foreach (var file in Request.Form.Files)
                {
                    files.Add(new FileContent
                    {
                        Length = file.Length,
                        ContentStream = file.OpenReadStream(),
                        ContentType = file.ContentType,
                        FileName = file.FileName
                    });
                }

                return Json((await connector.ProcessAsync(parameters, files)).Value);
            }

            return Json((await connector.ProcessAsync(parameters)).Value);
        }

        [HttpGet("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector(null);

            var result = (await connector.GetThumbnailAsync(hash)).Value;
            if (result is ImageWithMimeType)
            {
                var file = result as ImageWithMimeType;

                return File(file.ImageStream, file.MimeType);
            }

            return Json(result);
        }

        private Connector GetConnector(string? storage)
        {
            var driver = new FileSystemDriver();
            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "store\\" + storage + "\\bibliotheque");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var root = new RootVolume(
                path,
                $"{uri.Scheme}://{uri.Authority}/Files/",
                $"{uri.Scheme}://{uri.Authority}/el-finder/file-system/thumb/")
            {
                IsReadOnly = false,
                IsLocked = false,
                Alias = "Files"
            };

            driver.AddRoot(root);

            return new Connector(driver)
            {
                // This allows support for the "onlyMimes" option on the client.
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}
