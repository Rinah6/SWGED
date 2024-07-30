using Microsoft.EntityFrameworkCore;
using API.Context;
using API.Data.Entities;
using System.Data.SqlClient;

namespace API.Repositories
{
    public class ProjectRepository
    {
        private readonly SoftGED_DBContext _db;
        private readonly string _connectionString;

        public ProjectRepository(SoftGED_DBContext db, IConfiguration configuration)
        {
            _db = db;
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<bool> IsExist(Guid id)
        {
            return await _db.Projects.AnyAsync(x => x.Id == id && x.DeletionDate != null);
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public async Task AddNewProject(ProjectToAdd projectToAdd, int SOAID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO Projects (Id, Name, Storage, ServerName, Login, Password, DataBaseName, SoaId, SoaName, Sites)
                VALUES (NEWID(), @name, @storage, @serverName, @login, @password, @dataBaseName, @SoaId, @SoaName, @Sites)
            ", conn);

            cmd.Parameters.AddWithValue("@name", projectToAdd.Name);
            cmd.Parameters.AddWithValue("@storage", projectToAdd.Storage);
            cmd.Parameters.AddWithValue("@serverName", projectToAdd.ServerName);
            cmd.Parameters.AddWithValue("@login", projectToAdd.Login != null ? projectToAdd.Login : "");
            cmd.Parameters.AddWithValue("@password", projectToAdd.Password != null ? projectToAdd.Password : "");
            cmd.Parameters.AddWithValue("@dataBaseName", projectToAdd.DataBaseName);
            cmd.Parameters.AddWithValue("@SoaId", SOAID);
            cmd.Parameters.AddWithValue("@SoaName", projectToAdd.SOA);
            cmd.Parameters.AddWithValue("@Sites", projectToAdd.Sites);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Model.Project?> Get(Guid id)
        {
            return await _db.Projects.FirstOrDefaultAsync(project => project.Id == id && project.DeletionDate == null);
        }

        public async Task<Model.Project?> GetProjectByUserId(Guid userId)
        {
            return await _db.Users.Where(x => x.Id == userId).Include(x => x.Project).Join(_db.Projects, x => x.ProjectId, y => y.Id, (x, y) => y).FirstOrDefaultAsync();
        }

        public async Task<List<Model.Project>> GetAll()
        {
            var query = _db.Projects.Where(x => x.DeletionDate == null);

            return await query.ToListAsync();
        }

        public async Task<List<Model.Project>> GetAllBySoaId(int id)
        {
            var query = _db.Projects.Where(x => x.DeletionDate == null && x.SoaId == id);

            return await query.ToListAsync();
        }

        public async Task Update(Guid id, ProjectToUpdate projectToUpdate)
        {
            Model.Project? project = await Get(id);

            if (project == null)
            {
                return;
            }

            project.Name = projectToUpdate.Name;
            project.HasAccessToInternalUsersHandling = projectToUpdate.HasAccessToInternalUsersHandling;
            project.HasAccessToSuppliersHandling = projectToUpdate.HasAccessToSuppliersHandling;
            project.HasAccessToProcessingCircuitsHandling = projectToUpdate.HasAccessToProcessingCircuitsHandling;
            project.HasAccessToSignMySelfFeature = projectToUpdate.HasAccessToSignMySelfFeature;
            project.HasAccessToArchiveImmediatelyFeature = projectToUpdate.HasAccessToArchiveImmediatelyFeature;
            project.HasAccessToGlobalDynamicFieldsHandling = projectToUpdate.HasAccessToGlobalDynamicFieldsHandling;
            project.HasAccessToPhysicalLocationHandling = projectToUpdate.HasAccessToPhysicalLocationHandling;
            project.HasAccessToNumericLibrary = projectToUpdate.HasAccessToNumericLibrary;
            project.HasAccessToTomProLinking = projectToUpdate.HasAccessToTomProLinking;
            project.HasAccessToUsersConnectionsInformation = projectToUpdate.HasAccessToUsersConnectionsInformation;
            project.HasAccessToDocumentTypesHandling = projectToUpdate.HasAccessToDocumentTypesHandling;
            project.HasAccessToDocumentsAccessesHandling = projectToUpdate.HasAccessToDocumentsAccessesHandling;
            project.HasAccessToRSF = projectToUpdate.HasAccessToRSF;
            project.Sites = projectToUpdate.Sites;
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Projects SET DeletionDate = GETDATE()
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<string> GetUserStorageLocation(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
               SELECT s.Storage FROM Projects AS s
               INNER JOIN Users AS u On s.Id = u.ProjectId
               WHERE u.Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader["Storage"].ToString()!;
            }

            return "";
        }

        private async Task DeleteProjectBinding(string projectAId, string projectBId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE ProjectsBindings SET DeletionDate = GETDATE()
                WHERE ProjectAId = @projectAId AND ProjectBId = @projectBId;
            ", conn);

            cmd.Parameters.AddWithValue("@projectAId", projectAId);
            cmd.Parameters.AddWithValue("@projectBId", projectBId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteProjectsBindings(string projectAId, List<string> projectsId)
        {
            for (int i = 0; i < projectsId.Count; i += 1)
            {
                await DeleteProjectBinding(projectAId, projectsId[i]);
            }
        }

        public async Task<List<User>> GetUsersByProjectId(Guid projectId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var res = new List<User>();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.Username FROM Users AS u
                WHERE u.ProjectId = @projectId
                AND u.DeletionDate IS NULL
                ORDER BY u.CreationDate DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new User
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    Username = reader["Username"].ToString()!
                });
            }

            return res;
        }

        public async Task<Guid?> GetProjectIdByUserId(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT p.Id FROM Projects AS p
                INNER JOIN Users AS u ON u.ProjectId = p.Id
                WHERE u.Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Guid.Parse(reader["Id"].ToString()!);
            }

            return null;
        }
    }
}
