using System.Data.SqlClient;
using API.Context;
using API.Model;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class AttachementRepository
    {
        private readonly SoftGED_DBContext _db;
        private readonly string _connectionString;
        private readonly ProjectRepository _projectRepository;

        public AttachementRepository(SoftGED_DBContext db, IConfiguration configuration, ProjectRepository projectRepository)
        {
            _db = db;

            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;

            _projectRepository = projectRepository;
        }

        public async Task Delete(Guid id)
        {
            var attachement = await _db.Attachements.FirstOrDefaultAsync(u => u.Id == id);

            if (attachement == null)
            {
                return;
            }

            attachement.DeletionDate = DateTime.Now;

            await _db.SaveChangesAsync();
        }

        public async Task AddAttachement(string attachementFileName, string attachementPath, Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO Attachements (Id, FileName, Url, DocumentId)
                VALUES (NEWID(), @fileName, @url, @documentId);
            ", conn);

            cmd.Parameters.AddWithValue("@fileName", attachementFileName);
            cmd.Parameters.AddWithValue("@url", attachementPath);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddAttachements(List<IFormFile> attachements, Guid documentId, Guid userId)
        {
            var location = await _projectRepository.GetUserStorageLocation(userId);

            var locationPath = Path.Combine("wwwroot/store", location);
            var attachementsPath = Path.Combine(locationPath, "attachements");

            Utils.File.CreateDirectory(locationPath);
            Utils.File.CreateDirectory(attachementsPath);

            for (int i = 0; i < attachements.Count; i += 1)
            {
                string url = Utils.File.GetFileName(Path.Combine(attachementsPath, attachements[i].FileName));

                await Utils.File.CreateFile(attachements[i], url);

                await AddAttachement(attachements[i].FileName, url, documentId);
            }
        }

        public async Task Rename(string id, string name)
        {
            var attachement = await _db.Attachements.FirstOrDefaultAsync(u => u.Id.ToString() == id);

            if (attachement == null)
            {
                return;
            }

            attachement.Filename = name;

            await _db.SaveChangesAsync();
        }

        public async Task<Attachement?> Get(Guid id)
        {
            var attachement = await _db.Attachements.FirstOrDefaultAsync(u => u.Id == id);

            return attachement;
        }


        public async Task<List<Attachement>?> FindByDocumentId(Guid documentId)
        {
            var attachement = await _db.Attachements.Where(u => u.DocumentId == documentId && u.DeletionDate == null).OrderBy(u => u.Filename).ToListAsync();

            return attachement;
        }
    }
}
