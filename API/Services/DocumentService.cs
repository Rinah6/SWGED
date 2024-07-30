using API.Repositories;
using CliWrap;
using API.Data;
using API.Data.Entities;

namespace API.Services
{
    public class DocumentService
    {
        private readonly DocumentsProcessesRepository _documentsProcessesRepository;
        private readonly ProjectRepository _projectRepository;
        private readonly AttachementRepository _attachementRepository;

        public DocumentService(DocumentsProcessesRepository documentsProcessesRepository, ProjectRepository projectRepository, AttachementRepository attachementRepository)
        {
            _documentsProcessesRepository = documentsProcessesRepository;
            _projectRepository = projectRepository;
            _attachementRepository = attachementRepository;
        }

        public async Task<bool> UpdateAttachement(Model.Document document, IFormFile? File, Guid userId)
        {
            var location = await _projectRepository.GetUserStorageLocation(userId);

            // await DeleteFile(Path.Combine("wwwroot/store", document.Url));
            document.Url = Path.Combine(location, File.FileName);
            document.OriginalFilename = File.FileName;
            document.Filename = File.FileName;

            await Utils.File.CreateFile(File, Path.Combine("wwwroot/store", document.Url));

            return true;
        }

        public Guid Generate()
        {
            var buffer = Guid.NewGuid().ToByteArray();

            var time = new DateTime(0x76c, 1, 1);
            var now = DateTime.Now;
            var span = new TimeSpan(now.Ticks - time.Ticks);
            var timeOfDay = now.TimeOfDay;

            var bytes = BitConverter.GetBytes(span.Days);
            var array = BitConverter.GetBytes(
                (long)(timeOfDay.TotalMilliseconds / 3.333333));

            Array.Reverse(bytes);
            Array.Reverse(array);
            Array.Copy(bytes, bytes.Length - 2, buffer, buffer.Length - 6, 2);
            Array.Copy(array, array.Length - 4, buffer, buffer.Length - 4, 4);

            return new Guid(buffer);
        }

        private async Task SanitizeDocument(string workingDirectory, string input, string output)
        {
            await Cli.Wrap("\"C:\\Program Files\\qpdf 11.9.0\\bin\\qpdf.exe\"")
                .WithArguments($"--warning-exit-0 --linearize --stream-data=uncompress \"{input}\" \"{output}\"")
                .WithWorkingDirectory(Path.Combine(Directory.GetCurrentDirectory(), workingDirectory))
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteAsync();
        }

        public async Task<Model.Document?> CreateDocument(IFormFile pdfDocument, List<IFormFile>? attachements, string Title, string Object, string Message, bool RSF, Guid userId, DocumentStatus documentStatus, string Site)
        {
            var uploadFileName = pdfDocument.FileName.Replace(" ", "_");
            string filename = Path.GetFileNameWithoutExtension(uploadFileName);

            Model.Document? document = new()
            {
                OriginalFilename = uploadFileName,
                Title = Title,
                Site = Site,
                Object = Object,
                Message = Message,
                Status = documentStatus,
                SenderId = userId,
                RSF = RSF,
            };

            var now = DateTime.Now;
            document.CreationDate = now;
            document.Id = Generate();

            var location = await _projectRepository.GetUserStorageLocation(userId);

            document.Filename = $"{now:yyyyMMddhhmmss-}{filename}{Path.GetExtension(uploadFileName)}";
            document.OriginalFilename = $"{filename}{Path.GetExtension(uploadFileName)}";

            var locationPath = Path.Combine("wwwroot/store", location);
            var attachementsPath = Path.Combine(locationPath, "attachements");

            Utils.File.CreateDirectory(locationPath);
            Utils.File.CreateDirectory(attachementsPath);

            document.Url = Path.Combine(location, document.Filename);

            await Utils.File.CreateFile(pdfDocument, Path.Combine(locationPath, "(_original_)" + document.Filename));

            await SanitizeDocument(locationPath, "(_original_)" + document.Filename, document.Filename);

            await _documentsProcessesRepository.AddDocument(new DocumentToAdd
            {
                Id = document.Id,
                Site = document.Site,
                Filename = document.Filename,
                OriginalFilename = document.OriginalFilename,
                Url = document.Url,
                Title = document.Title,
                Object = document.Object,
                Message = document.Message,
                Status = document.Status,
                SenderId = document.SenderId,
                RSF = document.RSF,
            });

            if (attachements != null)
            {
                await _attachementRepository.AddAttachements(attachements, document.Id, userId);
            }

            return document;
        }

        public async Task<Model.Document> CreateDocument(NewDocumentDetails newDocumentDetails, Guid userId, DocumentStatus documentStatus, string Site)
        {
            //try
            //{
                var uploadFileName = newDocumentDetails.DocumentFile.FileName.Replace(" ", "_");
                string filename = Path.GetFileNameWithoutExtension(uploadFileName);

                Model.Document document = new()
                {
                    OriginalFilename = uploadFileName,
                    Title = newDocumentDetails.Title,
                    Object = newDocumentDetails.Object,
                    Message = newDocumentDetails.Message,
                    Status = documentStatus,
                    SenderId = userId,
                    Site = Site,
                    RSF = newDocumentDetails.RSF,
                };

                var now = DateTime.Now;
                document.CreationDate = now;
                document.Id = Generate();

                var location = await _projectRepository.GetUserStorageLocation(userId);

                document.Filename = $"{now:yyyyMMddhhmmss-}{filename}{Path.GetExtension(uploadFileName)}";
                document.OriginalFilename = $"{filename}{Path.GetExtension(uploadFileName)}";

                var locationPath = Path.Combine("wwwroot/store", location);
                var attachementsPath = Path.Combine(locationPath, "attachements");

                Utils.File.CreateDirectory(locationPath);
                Utils.File.CreateDirectory(attachementsPath);

                document.Url = Path.Combine(location, document.Filename);

                await Utils.File.CreateFile(newDocumentDetails.DocumentFile, Path.Combine(locationPath, "(_original_)" + document.Filename));

                await SanitizeDocument(locationPath, "(_original_)" + document.Filename, document.Filename);

                document = await _documentsProcessesRepository.Create(document);

                if (newDocumentDetails.Attachements != null)
                {
                    await _attachementRepository.AddAttachements(newDocumentDetails.Attachements, document.Id, userId);
                }

                return document!;
            //}
            //catch (Exception ex) {
            //}
        }

        public async Task<string> CreateDynamicAttachement(Guid userId, IFormFile dynamicAttachement)
        {
            var location = await _projectRepository.GetUserStorageLocation(userId);

            var uploadFileName = dynamicAttachement.FileName.Replace(" ", "_");
            string filename = Path.GetFileNameWithoutExtension(uploadFileName);

            var newFileName = $"{DateTime.Now:yyyyMMddhhmmss-}{filename}{Path.GetExtension(uploadFileName)}";

            Utils.File.CreateDirectory(Path.Combine("wwwroot/store", location, "dynamic attachements"));

            var locationPath = Path.Combine("wwwroot/store", location, "dynamic attachements", newFileName);

            await Utils.File.CreateFile(dynamicAttachement, locationPath);

            return Path.Combine(location, "dynamic attachements", newFileName);
        }
    }
}
