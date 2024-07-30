using System.Data.SqlClient;
using API.Data.Entities;

namespace API.Repositories
{
    public class TomProConnectionRepository
    {
        private readonly string _connectionString;

        public TomProConnectionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        public async Task<List<Database>> GetDatabases(DBConnexionDetails connexionDetails)
        {
            var connectionString = connexionDetails.Login == null || connexionDetails.Password == null ?
                    $"Server={connexionDetails.ServerName}; Persist Security Info=False;Trusted_Connection=True; "
                    : $"Server={connexionDetails.ServerName}; User Id={connexionDetails.Login}; Password={connexionDetails.Password}; TrustServerCertificate=true; ";


            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            var res = new List<Database>();

            using var cmd = new SqlCommand(@"
                SELECT database_id AS Id, name AS Name
                FROM sys.databases;
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new Database
                {
                    Id = reader["Id"].ToString()!,
                    Name = reader["Name"].ToString()!
                });
            }

            return res;
        }

        private async Task PostTomProDatabase(Guid tomProConnectionId, TomProDatabaseToAdd tomProDatabaseToAdd, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO TomProDatabases (DatabaseName, DatabaseId, TomProConnectionId, CreatedBy)
                VALUES (@databaseName, @databaseId, @tomProConnectionId, @createdBy);
            ", conn);

            cmd.Parameters.AddWithValue("@databaseName", tomProDatabaseToAdd.DatabaseName);
            cmd.Parameters.AddWithValue("@databaseId", tomProDatabaseToAdd.DatabaseId);
            cmd.Parameters.AddWithValue("@tomProConnectionId", tomProConnectionId);
            cmd.Parameters.AddWithValue("@createdBy", currentUserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task PostTomProConnection(TomProConnectionToAdd tomProConnectionToAdd, Guid projectId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO TomProConnections (Id, ServerName, Login, Password, ProjectId, CreatedBy)
                VALUES (@id, @serverName, @login, @password, @projectId, @createdBy);
            ", conn);

            var id = Guid.NewGuid();

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@serverName", tomProConnectionToAdd.ServerName);
            cmd.Parameters.AddWithValue("@login", tomProConnectionToAdd.Login == null ? DBNull.Value : tomProConnectionToAdd.Login);
            cmd.Parameters.AddWithValue("@password", tomProConnectionToAdd.Password == null ? DBNull.Value : tomProConnectionToAdd.Password);
            cmd.Parameters.AddWithValue("@projectId", projectId);
            cmd.Parameters.AddWithValue("@createdBy", currentUserId);

            await cmd.ExecuteNonQueryAsync();

            for (int i = 0; i < tomProConnectionToAdd.Databases.Count; i += 1)
            {
                await PostTomProDatabase(id, tomProConnectionToAdd.Databases[i], currentUserId);
            }
        }

        public async Task<List<TomProConnectionServerName>> GetTomProConnections(Guid projectId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT tpc.Id, tpc.ServerName
                FROM TomProConnections AS tpc
                WHERE ProjectId = @projectId
                AND tpc.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<TomProConnectionServerName>();

            while (await reader.ReadAsync())
            {
                res.Add(new TomProConnectionServerName
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    ServerName = reader["ServerName"].ToString()!,
                });
            }

            return res;
        }

        public async Task<List<TomProConnectionDatabase>> GetTomProDatabases(Guid tomProConnectionId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT tpd.Id, tpd.DatabaseName
                FROM TomProDatabases AS tpd
                WHERE tpd.TomProConnectionId = @tomProConnectionId
                AND tpd.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@tomProConnectionId", tomProConnectionId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<TomProConnectionDatabase>();

            while (await reader.ReadAsync())
            {
                res.Add(new TomProConnectionDatabase
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    DatabaseName = reader["DatabaseName"].ToString()!,
                });
            }

            return res;
        }

        public async Task UpdateTomProDBConnection(Guid projectId, Tomate_DB_ConnectionToUpdate tomate_DB_ConnectionToUpdate)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Projects SET ServerName = @serverName, Login = @login, Password = @password, DatabaseId = @databaseID
                WHERE Id = @projectId;
            ", conn);

            cmd.Parameters.AddWithValue("@serverName", tomate_DB_ConnectionToUpdate.ServerName);
            cmd.Parameters.AddWithValue("@login", tomate_DB_ConnectionToUpdate.Login == null ? DBNull.Value : tomate_DB_ConnectionToUpdate.Login);
            cmd.Parameters.AddWithValue("@password", tomate_DB_ConnectionToUpdate.Password == null ? DBNull.Value : tomate_DB_ConnectionToUpdate.Password);
            cmd.Parameters.AddWithValue("@databaseId", tomate_DB_ConnectionToUpdate.DatabaseId);
            cmd.Parameters.AddWithValue("@projectId", projectId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<TomProConnection?> GetTomProDBConnection(Guid tomProConnectionId, Guid tomProDatabaseId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT tpc.ServerName, tpc.Login, tpc.Password, tpd.DatabaseName
                FROM TomProConnections AS tpc
                INNER JOIN TomProDatabases AS tpd ON tpc.Id = tpd.TomProConnectionId
                WHERE tpc.Id = @tomProConnectionId
                AND tpd.Id = @tomProDatabaseId;
            ", conn);

            cmd.Parameters.AddWithValue("@tomProConnectionId", tomProConnectionId);
            cmd.Parameters.AddWithValue("@tomProDatabaseId", tomProDatabaseId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var login = reader["Login"];
                var password = reader["Password"];

                return new TomProConnection
                {
                    ServerName = reader["ServerName"].ToString()!,
                    Login = login == DBNull.Value ? null : login.ToString(),
                    Password = password == DBNull.Value ? null : password.ToString(),
                    DatabaseName = reader["DatabaseName"].ToString()!,
                };
            }

            return null;
        }

        public async Task<List<LiquidationDetails>> GetLiquidations(TomProConnection tomProConnection, string code)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT tpmj.ID, cafl.NUMEROCA, tpmj.DESIGNATION, tpmj.MONTANT, tpmj.TYPEPIECE, tpmj.LIEN
                FROM TP_MPIECES_JUSTIFICATIVES AS tpmj 
                INNER JOIN CPTADMIN_FLIQUIDATION AS cafl ON tpmj.NUMERO_FICHE = cafl.Id
                WHERE cafl.NUMEROCA LIKE CONCAT('%', @code, '%');
            ", conn);

            cmd.Parameters.AddWithValue("@code", code);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<LiquidationDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new LiquidationDetails
                {
                    Id = reader["ID"].ToString()!,
                    Code = reader["NUMEROCA"].ToString()!,
                    Designation = reader["DESIGNATION"].ToString()!,
                    Montant = (decimal)reader["MONTANT"],
                    TypePiece = reader["TYPEPIECE"].ToString()!,
                    Lien = reader["LIEN"] == DBNull.Value ? null : reader["LIEN"].ToString(),
                });
            }

            return res;
        }

        public async Task<List<AvanceDetails>> GetAvances(TomProConnection tomProConnection, string code)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT tpmj.ID, cafa.NUMEROAVANCE, tpmj.DESIGNATION, tpmj.MONTANT, tpmj.TYPEPIECE, tpmj.LIEN
                FROM TP_MPIECES_JUSTIFICATIVES AS tpmj 
                INNER JOIN CPTADMIN_FAVANCE AS cafa ON tpmj.NUMERO_FICHE = cafa.Id
                WHERE cafa.NUMEROAVANCE LIKE CONCAT('%', @code, '%');
            ", conn);

            cmd.Parameters.AddWithValue("@code", code);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<AvanceDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new AvanceDetails
                {
                    Id = reader["ID"].ToString()!,
                    Code = reader["NUMEROAVANCE"].ToString()!,
                    Designation = reader["DESIGNATION"].ToString()!,
                    Montant = (decimal)reader["MONTANT"],
                    TypePiece = reader["TYPEPIECE"].ToString()!,
                    Lien = reader["LIEN"] == DBNull.Value ? null : reader["LIEN"].ToString(),
                });
            }

            return res;
        }

        public async Task<List<JustificatifDetails>> GetJustificatifs(TomProConnection tomProConnection, string code)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT gaj.ID, gaj.NUMERO_AVANCE, gaj.MONTANT, gaj.COMMENTAIRE 
                FROM GA_AVANCE_JUSTIFICATIF AS gaj
                WHERE gaj.NUMERO_AVANCE LIKE CONCAT('%', @code, '%');
            ", conn);

            cmd.Parameters.AddWithValue("@code", code);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<JustificatifDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new JustificatifDetails
                {
                    Id = reader["ID"].ToString()!,
                    Code = reader["NUMERO_AVANCE"].ToString()!,
                    Montant = (decimal)reader["MONTANT"],
                    Commentaire = reader["COMMENTAIRE"] == DBNull.Value ? null : reader["COMMENTAIRE"].ToString(),
                });
            }

            return res;
        }

        public async Task<List<ReversementDetails>> GetReversements(TomProConnection tomProConnection, string code)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT gar.ID, gar.NUMERO_AVANCE, gar.MONTANT, gar.COMMENTAIRE 
                FROM GA_AVANCE_REVERSEMENT AS gar
                WHERE gar.NUMERO_AVANCE LIKE CONCAT('%', @code, '%');
            ", conn);

            cmd.Parameters.AddWithValue("@code", code);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<ReversementDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new ReversementDetails
                {
                    Id = reader["ID"].ToString()!,
                    Code = reader["NUMERO_AVANCE"].ToString()!,
                    Montant = (decimal)reader["MONTANT"],
                    Commentaire = reader["COMMENTAIRE"] == DBNull.Value ? null : reader["COMMENTAIRE"].ToString(),
                });
            }

            return res;
        }

        public async Task UpdateLiquidationLink(TomProConnection tomProConnection, LiquidationToUpdate liquidationToUpdate)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE TP_MPIECES_JUSTIFICATIVES SET LIEN = @newLink
                WHERE ID = @liquidationId;
            ", conn);

            cmd.Parameters.AddWithValue("@newLink", liquidationToUpdate.NewLInk);
            cmd.Parameters.AddWithValue("@liquidationId", liquidationToUpdate.LiquidationId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAvanceLink(TomProConnection tomProConnection, AvanceToUpdate avanceToUpdate)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE TP_MPIECES_JUSTIFICATIVES SET LIEN = @newLink
                WHERE ID = @avanceId;
            ", conn);

            cmd.Parameters.AddWithValue("@newLink", avanceToUpdate.NewLInk);
            cmd.Parameters.AddWithValue("@avanceId", avanceToUpdate.AvanceId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateJustificatifLink(TomProConnection tomProConnection, JustificatifToUpdate justificatifToUpdate)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE GA_AVANCE_JUSTIFICATIF SET COMMENTAIRE = @newLink
                WHERE ID = @justificatifId;
            ", conn);

            cmd.Parameters.AddWithValue("@newLink", justificatifToUpdate.NewLInk);
            cmd.Parameters.AddWithValue("@justificatifId", justificatifToUpdate.JustificatifId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateReversementLink(TomProConnection tomProConnection, ReversementToUpdate reversementToUpdate)
        {
            var connectionString = tomProConnection.Login == null || tomProConnection.Password == null ?
               $"Server={tomProConnection.ServerName}; Database={tomProConnection.DatabaseName}; Persist Security Info=False;Trusted_Connection=True; "
               : $"Server={tomProConnection.ServerName}; User Id={tomProConnection.Login}; Password={tomProConnection.Password}; Database={tomProConnection.DatabaseName}; TrustServerCertificate=true; ";

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE GA_AVANCE_REVERSEMENT SET COMMENTAIRE = @newLink
                WHERE ID = @reversementId;
            ", conn);

            cmd.Parameters.AddWithValue("@newLink", reversementToUpdate.NewLInk);
            cmd.Parameters.AddWithValue("@reversementId", reversementToUpdate.ReversementId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
