using System.Data.SqlClient;
using API.Data.Entities;

namespace API.Repositories
{
    public class DocumentAccessesRepository
    {
        private readonly string _connectionString;
        private readonly ProjectRepository _projectRepository;

        public DocumentAccessesRepository(IConfiguration configuration, ProjectRepository projectRepository)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
            _projectRepository = projectRepository;
        }

        private async Task<bool> CanAccessTheDocument(Guid userId, Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT TOP 1 uda.CreationDate
                FROM UsersDocumentsAccesses AS uda
                WHERE uda.UserId = @userId
                AND uda.DocumentId = @documentId
                AND uda.DeletionDate IS NULL
                ORDER BY CreationDate DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return true;
            }

            return false;
        }

        public async Task<List<DocumentAccessor>> GetDocumentAccessors(Guid documentId, Guid currentUserProjectId, Guid currentUserId)
        {
            var users = await _projectRepository.GetUsersByProjectId(currentUserProjectId);

            var res = new List<DocumentAccessor>();

            for (int i = 0; i < users.Count; i += 1)
            {
                if (users[i].Id == currentUserId)
                {
                    continue;
                }

                res.Add(new DocumentAccessor
                {
                    Id = users[i].Id,
                    Username = users[i].Username,
                    CanAccess = await CanAccessTheDocument(users[i].Id, documentId),
                });
            }

            return res;
        }

        public async Task AddDocumentAccessor(Guid userId, Guid documentId, Guid? createdBy)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO UsersDocumentsAccesses (UserId, DocumentId, CreatedBy)
                VALUES (@userId, @documentId, @createdBy);
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@createdBy", createdBy == null ? DBNull.Value : createdBy);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveDocumentAccessor(Guid userId, Guid documentId, Guid deletedBy)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE UsersDocumentsAccesses SET DeletionDate = GETDATE(), DeletedBy = @deletedBy
                WHERE UserId = @userId
                AND DocumentId = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@deletedBy", deletedBy);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> CanTheDocumentBeAccessedByAnyone(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT d.CanBeAccessedByAnyone
                FROM Documents AS d
                WHERE d.Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            var res = new List<DocumentAccessor>();

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return (bool)reader["CanBeAccessedByAnyone"];
            }

            return false;
        }

        public async Task SetCanBeAccessedByAnyone(Guid documentId, bool status)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Documents SET CanBeAccessedByAnyone = @canBeAccessedByAnyone
                WHERE Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@canBeAccessedByAnyone", status);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
