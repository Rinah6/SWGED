using System.Data.SqlClient;
using API.Data.Entities;

namespace API.Repositories
{
    public class ProjectDocumentsReceiverRepository
    {
        private readonly string _connectionString;

        public ProjectDocumentsReceiverRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<List<ProjectDocumentsReceiver>> GetProjectDocumentsReceivers(Guid projectId, bool isDocumentsReceiver = true)
        {
            var res = new List<ProjectDocumentsReceiver>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.Username, u.FirstName, u.LastName, u.Email
                FROM Users AS u
                WHERE u.ProjectId = @projectId
                AND u.IsADocumentsReceiver = @isDocumentsReceiver
                AND u.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);
            cmd.Parameters.AddWithValue("@isDocumentsReceiver", isDocumentsReceiver);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new ProjectDocumentsReceiver
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    Username = reader["Username"].ToString()!,
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Email = reader["Email"].ToString()!,
                });
            }

            return res;
        }

        public async Task SetIsDocumentsReceiver(Guid userId, bool isDocumentsReceiver)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Users SET IsADocumentsReceiver = @isDocumentsReceiver
                WHERE Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@isDocumentsReceiver", isDocumentsReceiver);
            cmd.Parameters.AddWithValue("@userId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task PostProjectDocumentsReceivers(List<Guid> usersId)
        {
            for (int i = 0; i < usersId.Count; i += 1)
            {
                await SetIsDocumentsReceiver(usersId[i], true);
            }
        }

        private async Task PostToDocumentsReceptions(Guid documentId, Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DocumentsReceptions (DocumentId, UserId)
                VALUES (@documentId, @userId)
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@userId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task PostToDocumentsReceptions(Guid documentId, List<Guid> usersId)
        {
            for (int i = 0; i < usersId.Count; i += 1)
            {
                await PostToDocumentsReceptions(documentId, usersId[i]);
            }
        }
    }
}
