using API.Data.Entities;
using Microsoft.Data.SqlClient;

namespace API.Repositories
{
    public class UsersConnectionsHistoryRepository
    {
        private readonly string _connectionString;

        public UsersConnectionsHistoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<UserLastConnection?> GetUserLastConnection(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT TOP 1 uc.CreationDate, uc.EndDate
                FROM UsersConnections AS uc
                WHERE uc.UserId = @userId
                ORDER BY uc.CreationDate DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<UserConnection>();

            if (await reader.ReadAsync())
            {
                var endDate = reader["EndDate"];

                return new UserLastConnection
                {
                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                    EndDate = endDate == DBNull.Value ? null : Convert.ToDateTime(endDate)
                };
            }

            return null;
        }

        public async Task<List<UserConnection>> GetUsersConnections()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.LastName, u.FirstName, u.Username
                FROM Users AS u
                WHERE u.DeletionDate IS NULL;
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<UserConnection>();

            while (await reader.ReadAsync())
            {
                var userId = Guid.Parse(reader["Id"].ToString()!);

                var userLastConnection = await GetUserLastConnection(userId);

                if (userLastConnection != null)
                {
                    res.Add(new UserConnection
                    {
                        Id = userId,
                        LastName = reader["LastName"].ToString()!,
                        FirstName = reader["FirstName"].ToString()!,
                        Username = reader["Username"].ToString()!,
                        CreationDate = userLastConnection.CreationDate,
                        EndDate = userLastConnection.EndDate
                    });
                }
            }

            return res.OrderByDescending(userLastConnection => userLastConnection.CreationDate).ToList();
        }

        public async Task<List<UserConnection>> GetUsersConnections(Guid projectId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.LastName, u.FirstName, u.Username
                FROM Users AS u
                INNER JOIN Projects AS p ON u.ProjectId = p.Id
                WHERE p.Id = @projectId
                AND u.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<UserConnection>();

            while (await reader.ReadAsync())
            {
                var userId = Guid.Parse(reader["Id"].ToString()!);

                var userLastConnection = await GetUserLastConnection(userId);

                if (userLastConnection != null)
                {
                    res.Add(new UserConnection
                    {
                        Id = userId,
                        LastName = reader["LastName"].ToString()!,
                        FirstName = reader["FirstName"].ToString()!,
                        Username = reader["Username"].ToString()!,
                        CreationDate = userLastConnection.CreationDate,
                        EndDate = userLastConnection.EndDate
                    });
                }
            }

            return res.OrderByDescending(userLastConnection => userLastConnection.CreationDate).ToList();
        }

        public async Task<List<UserConnectionHistory>> GetUserConnectionsHistory(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT uc.Id, uc.CreationDate, uc.EndDate
                FROM UsersConnections AS uc
                WHERE uc.UserId = @userId
                ORDER BY uc.CreationDate;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<UserConnectionHistory>();

            while (await reader.ReadAsync())
            {
                var endDate = reader["EndDate"];

                res.Add(new UserConnectionHistory
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                    EndDate = endDate == DBNull.Value ? null : Convert.ToDateTime(endDate)
                });
            }

            return res.OrderByDescending(connection => connection.CreationDate).ToList();
        }
    }
}
