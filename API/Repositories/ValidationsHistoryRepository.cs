using System.Data.SqlClient;
using API.Data.Entities;

namespace API.Repositories
{
    public class ValidationsHistoryRepository
    {
        private readonly string _connectionString;
        private readonly DocumentRepository _documentRepository;

        public ValidationsHistoryRepository(IConfiguration configuration, DocumentRepository documentRepository)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
            _documentRepository = documentRepository;
        }

        public async Task AddToValidationsHistory(ValidationHistory validationHistory)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO ValidationsHistory (FromUserId, ToDocumentStepId, DocumentId, Comment, ActionType, CreationDate)  
                VALUES (@fromUserId, @toDocumentStepId, @documentId, @comment, @actionType, @creationDate);
            ", conn);

            cmd.Parameters.AddWithValue("@fromUserId", validationHistory.FromUserId);
            cmd.Parameters.AddWithValue("@toDocumentStepId", validationHistory.ToDocumentStepId == null ? DBNull.Value : validationHistory.ToDocumentStepId);
            cmd.Parameters.AddWithValue("@documentId", validationHistory.DocumentId);
            cmd.Parameters.AddWithValue("@comment", validationHistory.Comment == null ? DBNull.Value : validationHistory.Comment);
            cmd.Parameters.AddWithValue("@actionType", validationHistory.ActionType);
            cmd.Parameters.AddWithValue("@creationDate", validationHistory.CreationDate);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<ValidationHistoryDetails>> GetValidationHistoryDetails(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Username, vh.CreationDate, vh.Comment
                FROM ValidationsHistory AS vh
                INNER JOIN Users AS u ON vh.FromUserId = u.Id
                WHERE vh.DocumentId = @documentId
                ORDER BY vh.CreationDate ASC;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<ValidationHistoryDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new ValidationHistoryDetails
                {
                    Username = reader["Username"].ToString()!,
                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                    Comment = reader["Comment"] == DBNull.Value ? null : reader["Comment"].ToString(),
                });
            }

            return res;
        }

        private async Task<DateTime> GetDocumentCreationDate(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT CreationDate
                FROM Documents
                WHERE Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Convert.ToDateTime(reader["CreationDate"]);
            }

            return DateTime.Now;
        }

        private async Task<double> GetProcessingDuration(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT TOP 1 CreationDate
                FROM ValidationsHistory
                WHERE DocumentId = @documentId
                ORDER BY CreationDate DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var documentCreationDate = await GetDocumentCreationDate(documentId);

                return Convert.ToDateTime(reader["CreationDate"]).Subtract(documentCreationDate).TotalHours;
            }

            return 0;
        }

        private async Task<double> GetDocumentStepsTotalProcessingDuration(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT SUM(ProcessingDuration) AS total
                FROM DocumentSteps
                WHERE DocumentId = @documentId
                GROUP BY DocumentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Convert.ToDouble(reader["total"]);
            }

            return 0;
        }

        public async Task<List<Document>> GetLateDocuments(Guid currentUserId)
        {
            var documents = await _documentRepository.GetDocuments(currentUserId, Data.DocumentStatus.Archived);

            var res = new List<Document>();

            for (int i = 0; i < documents.Count; i += 1)
            {
                var documentStepsTotalProcessingDuration = await GetDocumentStepsTotalProcessingDuration(documents[i].Id);
                var processingDuration = await GetProcessingDuration(documents[i].Id);

                if (processingDuration != 0 && documentStepsTotalProcessingDuration < processingDuration)
                {
                    res.Add(documents[i]);
                }
            }

            return res;
        }

        public async Task<List<Document>> GetNonLateDocuments(Guid currentUserId)
        {
            var documents = await _documentRepository.GetDocuments(currentUserId, Data.DocumentStatus.Archived);

            var res = new List<Document>();

            for (int i = 0; i < documents.Count; i += 1)
            {
                var documentStepsTotalProcessingDuration = await GetDocumentStepsTotalProcessingDuration(documents[i].Id);
                var processingDuration = await GetProcessingDuration(documents[i].Id);

                if (processingDuration != 0 && documentStepsTotalProcessingDuration >= processingDuration)
                {
                    res.Add(documents[i]);
                }
            }

            return res;
        }
    }
}
