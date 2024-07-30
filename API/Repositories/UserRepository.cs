using Microsoft.EntityFrameworkCore;
using API.Context;
using API.Data.Entities;
using API.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Primitives;

namespace API.Repositories
{
    public class UserRepository
    {
        private readonly SoftGED_DBContext _db;
        private readonly string _connectionString;

        public UserRepository(SoftGED_DBContext db, IConfiguration configuration)
        {
            _db = db;
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<bool> IsExist(Guid id)
        {
            return await _db.Users.AnyAsync(x => x.Id == id && x.DeletionDate != null);
        }

        public async Task<bool> DoesUsernameExist(string username)
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.Username == username && user.DeletionDate == null);

            return user != null;
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public async Task<Model.User?> Get(Guid id)
        {
            //using var conn = new SqlConnection(_connectionString);
            //await conn.OpenAsync();

            //using var cmd = new SqlCommand(@"
            //    SELECT u.Id, u.RoleId, u.Password
            //    FROM Users AS u
            //    WHERE u.Username = @username
            //    AND u.DeletionDate IS NULL;
            //", conn);

            //cmd.Parameters.AddWithValue("@username", username);

            //using var reader = await cmd.ExecuteReaderAsync();

            return await _db.Users.FirstOrDefaultAsync(user => user.Id == id && user.DeletionDate == null);
        }

        public async Task<int> GetCountAll(int count)
        {
            return await _db.Users.Where(user => user.DeletionDate == null).CountAsync() / count;
        }

        public async Task<Model.User?> GetByMail(string mail)
        {
            return await _db.Users.FirstOrDefaultAsync(user => user.Email == mail && user.DeletionDate == null);
        }

        public async Task<LoginResult?> GetByUsername(string username)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.RoleId, u.Password
                FROM Users AS u
                WHERE u.Username = @username
                AND u.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@username", username);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new LoginResult
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    RoleId = (UserRole)reader["RoleId"],
                    Password = reader["Password"].ToString()!
                };
            }

            return null;
        }

        public async Task<List<UserBasicDetails>> GetUsersByProjectId(Guid projectId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.Username, u.FirstName, u.LastName, u.Email, u.RoleId
                FROM Users AS u
                WHERE u.DeletionDate IS NULL
                AND u.ProjectId = @projectId;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<UserBasicDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new UserBasicDetails
                {
                    Id = reader["Id"].ToString()!,
                    Username = reader["Username"].ToString()!,
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    Role = (UserRole)reader["RoleId"],
                });
            }

            return res;
        }

        public async Task<List<UserBasicDetails>> GetAdmins()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.Username, u.FirstName, u.LastName, u.Email, u.RoleId
                FROM Users AS u
                WHERE u.DeletionDate IS NULL
                AND u.RoleId = @adminRoleId;
            ", conn);

            cmd.Parameters.AddWithValue("@adminRoleId", UserRole.Admin);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<UserBasicDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new UserBasicDetails
                {
                    Id = reader["Id"].ToString()!,
                    Username = reader["Username"].ToString()!,
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    Role = (UserRole)reader["RoleId"]
                });
            }

            return res;
        }

        public async Task<UserDetails?> GetUserDetails(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Username, u.FirstName, u.LastName, u.Email, u.RoleId, u.ProjectId, u.Sites
                FROM Users AS u
                WHERE u.DeletionDate IS NULL
                AND u.Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserDetails
                {
                    Username = reader["Username"].ToString()!,
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    Role = (UserRole)reader["RoleId"],
                    ProjectId = reader["ProjectId"].ToString()!,
                    Sites = reader["Sites"].ToString()!,
                };
            }

            return null;
        }

        public async Task<List<string>> GetSitesByUserId(Guid userId)
        {

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.Sites
                FROM Users AS u
                WHERE u.DeletionDate IS NULL
                AND u.Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {

                return JsonConvert.DeserializeObject<List<string>>(reader["Sites"].ToString()!);

            }

            return null;
        }



        public async Task<Model.User> Create(Model.User user)
        {
            var newUser = new Model.User
            {
                Email = user.Email.ToLower()
            };

            _db.Users.Add(newUser);

            await _db.SaveChangesAsync();

            return newUser;
        }

        public async Task Insert(UserToRegister userToRegister, Guid projectId, Guid currentUserId)
        {
            var newUserId = Guid.NewGuid();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DocumentsSenders (Id, Type)
                VALUES (@userId, @senderType);
            ", conn);

            cmd.Parameters.AddWithValue("@userId", newUserId);
            cmd.Parameters.AddWithValue("@senderType", SenderType.User);

            await cmd.ExecuteNonQueryAsync();

            _db.Users.Add(new Model.User
            {
                Id = newUserId,
                Username = userToRegister.Username,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
                Email = userToRegister.Email,
                Password = Utils.Password.HashPassword(userToRegister.Password),
                RoleId = (UserRole)userToRegister.Role,
                ProjectId = projectId,
                CreatedBy = currentUserId,
                Sites = userToRegister.Sites,
            });

            await _db.SaveChangesAsync();
        }

        public async Task<Model.User> Insert(string mail, string password)
        {
            mail = mail.ToLower();

            var newuser = _db.Users.Add(new Model.User()
            {
                Email = mail,
                Password = Utils.Password.HashPassword(password),
                RoleId = UserRole.User
            });

            await _db.SaveChangesAsync();

            return newuser.Entity;
        }

        public async Task Update(Guid userId, UserToUpdate userToUpdate, UserRole currentUserRole)
        {
            Model.User? user = await Get(userId);

            if (user == null)
            {
                return;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Users SET Username = @username, LastName = @lastName, FirstName = @firstName, Email = @email, Password = @password, RoleId = @role, ProjectId = @projectId, Sites = @sites
                WHERE Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@username", userToUpdate.Username);
            cmd.Parameters.AddWithValue("@firstName", userToUpdate.FirstName);
            cmd.Parameters.AddWithValue("@lastName", userToUpdate.LastName);
            cmd.Parameters.AddWithValue("@email", userToUpdate.Email == "" ? user.Email : userToUpdate.Email);
            cmd.Parameters.AddWithValue("@password", userToUpdate.Password == "" ? user.Password : Utils.Password.HashPassword(userToUpdate.Password));
            cmd.Parameters.AddWithValue("@role", currentUserRole == UserRole.SuperAdmin ? user.RoleId : userToUpdate.Role);
            cmd.Parameters.AddWithValue("@projectId", currentUserRole == UserRole.SuperAdmin ? userToUpdate.ProjectId : user.ProjectId);
            cmd.Parameters.AddWithValue("@sites", userToUpdate.Sites);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Delete(Guid userId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Users SET DeletionDate = GETDATE(), DeletedBy = @ideleted
                WHERE Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@ideleted", currentUserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsUserADocumentsReceiver(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT IsADocumentsReceiver FROM Users
                WHERE Id = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Convert.ToBoolean(reader["IsADocumentsReceiver"]);
            }

            return false;
        }
    }
}
