using Microsoft.EntityFrameworkCore;
using API.Context;
using API.Data.Entities;
using System.Data.SqlClient;
using API.Data;

namespace API.Repositories
{
    public class SiteRepository
    {
        private readonly SoftGED_DBContext _db;
        private readonly string _connectionString;

        public SiteRepository(SoftGED_DBContext db, IConfiguration configuration)
        {
            _db = db;
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<bool> IsExist(Guid id)
        {
            return await _db.Sites.AnyAsync(x => x.Id == id && x.DeletionDate != null);
        }

        public bool Save()
        {
            return _db.SaveChanges() > 0;
        }

        public async Task<SiteToUpdate?> GetSiteDetails(Guid siteId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT s.SiteId, s.Name
                FROM Sites AS s
                WHERE s.DeletionDate IS NULL
                AND s.Id = @siteId;
            ", conn);

            cmd.Parameters.AddWithValue("@siteId", siteId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new SiteToUpdate
                {
                    SiteId = reader["SiteId"].ToString()!,
                    Name = reader["Name"].ToString()!,
                };
            }

            return null;
        }

        public async Task AddNewSite(SiteToAdd SiteToAdd, Guid currentUserId)
        {
            try
            {
                //var newSiteId = Guid.NewGuid();

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand(@"
                    INSERT INTO Sites (Id, SiteId, Name, CreationDate, CreatedBy)
                    VALUES (NEWID(), @siteid, @name, @creationdate, @createdby)
                ", conn);

                cmd.Parameters.AddWithValue("@siteid", SiteToAdd.SiteId);
                cmd.Parameters.AddWithValue("@name", SiteToAdd.Name);
                cmd.Parameters.AddWithValue("@createdby", currentUserId);
                cmd.Parameters.AddWithValue("@creationdate", DateTime.Now);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<Model.Site?> Get(Guid id)
        {
            return await _db.Sites.FirstOrDefaultAsync(Site => Site.Id == id && Site.DeletionDate == null);
        }

        public async Task<List<Model.Site>> GetAll()
        {
            var query = _db.Sites.Where(x => x.DeletionDate == null);

            return await query.ToListAsync();
        }

        public async Task Update(Guid id, SiteToUpdate SiteToUpdate)
        {
            Model.Site? Site = await Get(id);

            if (Site == null)
            {
                return;
            }

            Site.SiteId = SiteToUpdate.SiteId;
            Site.Name = SiteToUpdate.Name;

            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Sites SET DeletionDate = GETDATE(), DeletedBy = @deletedBy
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@deletedBy", currentUserId);


            await cmd.ExecuteNonQueryAsync();
        }
    }
}
