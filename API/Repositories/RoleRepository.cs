using System.Data.SqlClient;
using API.Data;
using API.Data.Entities;

namespace API.Repositories
{
    public class RoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<List<Role>> GetRoles()
        {
            var res = new List<Role>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id, Title 
                FROM UsersRoles
                WHERE Id != @superAdminId;
            ", conn);

            cmd.Parameters.AddWithValue("@superAdminId", UserRole.SuperAdmin);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new Role
                {
                    Id = Enum.Parse<UserRole>(reader["Id"].ToString()!),
                    Title = reader["Title"].ToString()!
                });
            }

            return res;
        }
    }
}
