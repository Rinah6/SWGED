using System.Data.SqlClient;
using API.Data.Entities;

namespace API.Repositories
{
    public class SupplierRepository
    {
        private readonly string _connectionString;

        public SupplierRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<Guid?> GetSupplierId(Supplier supplier)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id 
                FROM Suppliers
                WHERE NIF = @nif
                AND STAT = @stat
                AND Name = @name
                AND ProjectId = @projectId
                AND DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@nif", supplier.NIF);
            cmd.Parameters.AddWithValue("@stat", supplier.STAT);
            cmd.Parameters.AddWithValue("@name", supplier.Name);
            cmd.Parameters.AddWithValue("@projectId", supplier.ProjectId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Guid(reader["Id"].ToString()!);
            }

            return null;
        }

        public async Task<Guid?> GetSupplieIdWithoutNIFAndSTAT(string name, Guid projectId, string? cin)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id 
                FROM Suppliers
                WHERE (Name = @name OR CIN = @cin)
                AND ProjectId = @projectId
                AND DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@cin", cin);
            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Guid(reader["Id"].ToString()!);
            }

            return null;
        }

        public async Task<Guid?> CheckSupplier(Supplier supplier)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id 
                FROM Suppliers
                WHERE (NIF = @nif OR STAT = @stat OR Name = @name)
                AND ProjectId = @projectId
                AND DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@nif", supplier.NIF);
            cmd.Parameters.AddWithValue("@stat", supplier.STAT);
            cmd.Parameters.AddWithValue("@name", supplier.Name);
            cmd.Parameters.AddWithValue("@projectId", supplier.ProjectId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Guid(reader["Id"].ToString()!);
            }

            return null;
        }

        public async Task RegisterSupplier(Supplier supplier)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DocumentsSenders (Id, Type)
		        VALUES (@Id, 1);

                INSERT INTO Suppliers (Id, NIF, STAT, MAIL, CONTACT, CIN, Name, ProjectId)
                VALUES (@Id, @nif, @stat, @mail, @contact, @cin, @name, @projectId);
            ", conn);

            var Id = Guid.NewGuid();

            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@nif", supplier.NIF == null ? DBNull.Value : supplier.NIF);
            cmd.Parameters.AddWithValue("@stat", supplier.STAT == null ? DBNull.Value : supplier.STAT);
            cmd.Parameters.AddWithValue("@mail", supplier.MAIL == null ? DBNull.Value : supplier.MAIL);
            cmd.Parameters.AddWithValue("@contact", supplier.CONTACT == null ? DBNull.Value : supplier.CONTACT);
            cmd.Parameters.AddWithValue("@cin", supplier.CIN == null ? DBNull.Value : supplier.CIN);
            cmd.Parameters.AddWithValue("@name", supplier.Name);
            cmd.Parameters.AddWithValue("@projectId", supplier.ProjectId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RegisterSupplierEmail(string supplierId, string email)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO SuppliersEmails (SupplierId, Email)
		        VALUES (@supplierId, @email);
            ", conn);

            cmd.Parameters.AddWithValue("@supplierId", supplierId);
            cmd.Parameters.AddWithValue("@email", email);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<SupplierInfo?> GetSupplierByDocumentId(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT sm.Email, s.NIF, s.STAT, s.Name 
                FROM SuppliersEmails AS sm
                INNER JOIN Documents AS d ON d.SenderId = sm.SupplierId
                INNER JOIN Suppliers AS s ON s.Id = sm.SupplierId
                WHERE d.Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var nif = reader["NIF"];
                var stat = reader["STAT"];

                return new SupplierInfo
                {
                    NIF = nif is DBNull ? null : nif.ToString(),
                    STAT = stat is DBNull ? null : stat.ToString(),
                    Name = reader["Email"].ToString()!,
                    Email = reader["Email"].ToString()!
                };
            }

            return null;
        }

        public async Task SetWasAcknowledged(Guid documentId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO SuppliersDocumentsAcknowledgements (Id, InitiatorId)
                VALUES (@documentId, @currentUserId);
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@currentUserId", currentUserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<SupplierDetails>> GetSuppliersByProjectId(Guid projectId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var res = new List<SupplierDetails>();

            using var cmd = new SqlCommand(@"
                SELECT Id, NIF, STAT, CIN, MAIL, CONTACT, Name, CreationDate
                FROM Suppliers
                WHERE ProjectId = @projectId
                AND DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var nif = reader["NIF"];
                var stat = reader["STAT"];
                var mail = reader["MAIL"];
                var contact = reader["CONTACT"];
                var cin = reader["CIN"];

                res.Add(new SupplierDetails
                {
                    Id = reader["Id"].ToString()!,
                    NIF = nif is DBNull ? null : nif.ToString(),
                    STAT = stat is DBNull ? null : stat.ToString(),
                    MAIL = mail is DBNull ? null : mail.ToString(),
                    CONTACT = contact is DBNull ? null : contact.ToString(),
                    CIN = cin is DBNull ? null : cin.ToString(),
                    Name = reader["Name"].ToString()!,
                    CreationDate = Convert.ToDateTime(reader["CreationDate"])
                });
            }

            return res;
        }

        public async Task DeleteSupplier(string supplierId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Suppliers SET DeletionDate = GETDATE()
                WHERE Id = @supplierId;
            ", conn);

            cmd.Parameters.AddWithValue("@supplierId", supplierId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
