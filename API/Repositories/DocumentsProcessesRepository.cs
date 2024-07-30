using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using API.Context;
using API.Data;
using API.Data.Entities;
using Newtonsoft.Json;

namespace API.Repositories
{
    public partial class DocumentsProcessesRepository
    {
        private readonly SoftGED_DBContext _db;
        private readonly string _connectionString;
        private readonly ValidationsHistoryRepository _validationsHistoryRepository;

        public DocumentsProcessesRepository(IConfiguration configuration, SoftGED_DBContext db, ValidationsHistoryRepository validationsHistoryRepository)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
            _db = db;
            _validationsHistoryRepository = validationsHistoryRepository;
        }

        public async Task<List<UserDocumentRole>> GetUserDocumentRoles()
        {
            var res = new List<UserDocumentRole>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id, Title
                FROM UserDocumentRoles
                WHERE Id != @reader;
            ", conn);

            cmd.Parameters.AddWithValue("@reader", DocumentRole.Reader);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new UserDocumentRole
                {
                    Id = (DocumentRole)reader["Id"],
                    Title = reader["Title"].ToString()!
                });
            }

            return res;
        }

        public async Task<Model.Document> Create(Model.Document document)
        {
            document = _db.Documents.Add(document).Entity;

            await _db.SaveChangesAsync();

            return document;
        }

        public async Task AddDocument(DocumentToAdd documentToAdd)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO Documents (Id, Filename, OriginalFilename, Url, Title, Object, Message, RSF, Status, SenderId, Site)
                VALUES (@id, @filename, @originalFilename, @url, @title, @object, @message, @RSF, @documentStatus, @senderId, @site);
            ", conn);

            cmd.Parameters.AddWithValue("@id", documentToAdd.Id);
            cmd.Parameters.AddWithValue("@filename", documentToAdd.Filename);
            cmd.Parameters.AddWithValue("@originalFilename", documentToAdd.OriginalFilename);
            cmd.Parameters.AddWithValue("@url", documentToAdd.Url);
            cmd.Parameters.AddWithValue("@title", documentToAdd.Title);
            cmd.Parameters.AddWithValue("@object", documentToAdd.Object);
            cmd.Parameters.AddWithValue("@message", documentToAdd.Message);
            cmd.Parameters.AddWithValue("@RSF", documentToAdd.RSF);
            cmd.Parameters.AddWithValue("@documentStatus", documentToAdd.Status);
            cmd.Parameters.AddWithValue("@senderId", documentToAdd.SenderId);
            cmd.Parameters.AddWithValue("@site", documentToAdd.Site);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateDocumentStatus(Guid documentId, DocumentStatus documentStatus)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Documents SET Status = @documentStatus 
                WHERE Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentStatus", documentStatus);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddToSuppliersDocumentsSendings(Guid documentId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO SuppliersDocumentsSendings (Id, InitiatorId)
                VALUES (@documentId, @currentUserId);
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@currentUserId", currentUserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddUsersSteps(Guid userId, Guid documentStepId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO UsersSteps (UserId, DocumentStepId, ProcessingDate)
                VALUES (@userId, @documentStepId, @processingDate);
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@documentStepId", documentStepId);
            cmd.Parameters.AddWithValue("@processingDate", DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddDocumentValidatorStep(UserStepToAdd userStepToAdd, Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DocumentSteps (Id, StepNumber, ProcessingDescription, ProcessingDuration, Role, Color, DocumentId)
                VALUES (@id, @stepNumber, @processingDuration, @role, @color, @documentId);
            ", conn);

            var id = Guid.NewGuid();

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@stepNumber", userStepToAdd.StepNumber);
            cmd.Parameters.AddWithValue("@processingDescription", userStepToAdd.ProcessingDescription);
            cmd.Parameters.AddWithValue("@processingDuration", userStepToAdd.ProcessingDuration);
            cmd.Parameters.AddWithValue("@role", DocumentRole.Validator);
            cmd.Parameters.AddWithValue("@color", userStepToAdd.Color);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            await cmd.ExecuteNonQueryAsync();

            for (int i = 0; i < userStepToAdd.UsersId.Count; i += 1)
            {
                await AddUsersSteps(userStepToAdd.UsersId[i], id);
            }
        }

        public async Task AddDocumentStep(RecipientsStep recipientsStep, Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DocumentSteps (Id, StepNumber, ProcessingDescription, ProcessingDuration, Role, Color, Message, DocumentId)
                VALUES (@id, @stepNumber, @processingDuration, @role, @color, @message, @documentId);
            ", conn);

            var id = Guid.NewGuid();

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@stepNumber", recipientsStep.StepNumber);
            cmd.Parameters.AddWithValue("@processingDescription", recipientsStep.ProcessingDescription);
            cmd.Parameters.AddWithValue("@processingDuration", recipientsStep.ProcessingDuration);
            cmd.Parameters.AddWithValue("@role", recipientsStep.Role);
            cmd.Parameters.AddWithValue("@color", recipientsStep.Color);
            cmd.Parameters.AddWithValue("@message", recipientsStep.Message);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            await cmd.ExecuteNonQueryAsync();

            for (int i = 0; i < recipientsStep.UsersId.Count; i += 1)
            {
                await AddUsersSteps(recipientsStep.UsersId[i], id);
            }
        }

        public async Task UpdateUserStep(Guid userId, Guid documentId, DateTime? processingDate)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE UsersSteps SET ProcessingDate = @processingDate
                WHERE UserId = @userId 
                AND DocumentStepId = (
                    SELECT TOP 1 dst.Id
                    FROM DocumentSteps AS dst
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    WHERE dst.DocumentId = @documentId
                    AND us.UserId = @userId
                    AND us.ProcessingDate IS NULL
                    ORDER BY dst.StepNumber ASC
                );
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@processingDate", processingDate == null ? DBNull.Value : processingDate);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<NextUserStepDetails>> GetNextUserStepsDetails(Guid documentStepId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.Email
                FROM UsersSteps AS us
                INNER JOIN Users AS u ON us.UserId = u.Id
                WHERE us.DocumentStepId = @documentStepId
                AND u.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@documentStepId", documentStepId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<NextUserStepDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new NextUserStepDetails
                {
                    Id = reader["Id"].ToString()!,
                    Email = reader["Email"].ToString()!
                });
            }

            return res;
        }

        public async Task<NextUsersStep?> GetNextUsersStep(Guid userId, Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT dst.Id
                FROM DocumentSteps AS dst
                WHERE dst.DocumentId = @documentId
                AND dst.StepNumber = (
                    SELECT TOP 1 (dst.StepNumber + 1) AS StepNumber 
                    FROM DocumentSteps AS dst
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    WHERE us.UserId = @userId 
                    AND dst.DocumentId = @documentId
                    AND us.ProcessingDate IS NOT NULL
                    ORDER BY dst.StepNumber ASC
                );
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var documentStepId = Guid.Parse(reader["Id"].ToString()!);

                return new NextUsersStep
                {
                    Id = documentStepId,
                    Users = await GetNextUserStepsDetails(documentStepId)
                };
            }

            return null;
        }

        public async Task ArchiveDocument(Guid documentId, Guid userId, string comment)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Documents SET Status = @documentStatus , Message = @comment
                WHERE Id = @Id;
            ", conn);

            cmd.Parameters.AddWithValue("@documentStatus", DocumentStatus.Archived);
            cmd.Parameters.AddWithValue("@comment", comment);
            cmd.Parameters.AddWithValue("@Id", documentId);

            await cmd.ExecuteNonQueryAsync();

            await _validationsHistoryRepository.AddToValidationsHistory(new ValidationHistory
            {
                FromUserId = userId,
                ToDocumentStepId = null,
                DocumentId = documentId,
                Comment = comment,
                ActionType = DocumentActionType.Archive,
                CreationDate = DateTime.Now,
            });
        }

        public async Task ValidateDocument(Guid documentId, Guid currentUserId, string comment)
        {
            try
            {


                await UpdateUserStep(currentUserId, documentId, DateTime.Now);

                var nextUsersStep = await GetNextUsersStep(currentUserId, documentId);

                if (nextUsersStep == null)
                {
                    await ArchiveDocument(documentId, currentUserId, comment);

                    return;
                }

                await _validationsHistoryRepository.AddToValidationsHistory(new ValidationHistory
                {
                    FromUserId = currentUserId,
                    ToDocumentStepId = nextUsersStep.Id,
                    DocumentId = documentId,
                    Comment = comment,
                    ActionType = DocumentActionType.Validate,
                    CreationDate = DateTime.Now,
                });
            }
            catch (Exception e)
            {
                return;
            }
        }

        public async Task CancelDocument(Guid documentId, Guid currentUserId, DocumentToDeny documentToDeny)
        {
            await UpdateUserStep(currentUserId, documentId, DateTime.Now);

            await UpdateDocumentStatus(documentId, DocumentStatus.Canceled);

            await _validationsHistoryRepository.AddToValidationsHistory(new ValidationHistory
            {
                FromUserId = currentUserId,
                ToDocumentStepId = null,
                DocumentId = documentId,
                Comment = documentToDeny.Message,
                ActionType = DocumentActionType.Cancel,
                CreationDate = DateTime.Now,
            });
        }

        public async Task<List<FormerDocumentStep>> GetFormerDocumentSteps(Guid documentId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT dst.Id, dst.StepNumber
                FROM DocumentSteps AS dst
                WHERE dst.DocumentId = @documentId
                AND dst.StepNumber < (
                    SELECT TOP 1 dst.StepNumber
                    FROM DocumentSteps AS dst
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    WHERE us.UserId = @currentUserId 
                    AND dst.DocumentId = @documentId
                    AND us.ProcessingDate IS NULL
                    ORDER BY dst.StepNumber ASC
                )
                ORDER BY dst.StepNumber ASC;
            ", conn);

            cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<FormerDocumentStep>();

            while (await reader.ReadAsync())
            {
                res.Add(new FormerDocumentStep
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    StepNumber = (int)reader["StepNumber"],
                });
            }

            return res;
        }

        public async Task<List<User>> GetUsersDocumentStep(Guid documentStepId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT u.Id, u.Username
                FROM UsersSteps AS us
                INNER JOIN Users AS u ON us.UserId = u.Id
                WHERE us.DocumentStepId = @documentStepId
                AND us.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@documentStepId", documentStepId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<User>();


            while (await reader.ReadAsync())
            {
                res.Add(new User
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    Username = reader["Username"].ToString()!,
                });
            }

            return res;
        }

        private async Task<List<Guid>> GetInBetweenDocumentSteps(Guid documentId, Guid currentUserId, Guid targetDocumentStepId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                DECLARE @currentUserStepNumber AS INT = -1;
                DECLARE @targetDocumentStepNumber AS INT = -1;

                SELECT @currentUserStepNumber = dst.StepNumber
                FROM DocumentSteps AS dst
                INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                WHERE us.UserId = @currentUserId 
                AND dst.DocumentId = @documentId
                AND us.ProcessingDate IS NULL;

                SELECT @targetDocumentStepNumber = dst.StepNumber
                FROM DocumentSteps AS dst
                WHERE dst.Id = @targetDocumentStepId;

                SELECT dst.Id
                FROM DocumentSteps AS dst
                INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                WHERE dst.StepNumber <= @currentUserStepNumber 
                AND dst.StepNumber >= @targetDocumentStepNumber
                AND dst.DocumentId = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("@targetDocumentStepId", targetDocumentStepId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<Guid>();

            while (await reader.ReadAsync())
            {
                res.Add(Guid.Parse(reader["Id"].ToString()!));
            }

            return res;
        }

        public async Task UpdateProcessingDate(Guid documentStepId, DateTime? processingDate)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE UsersSteps SET ProcessingDate = @processingDate
                WHERE DocumentStepId = @documentStepId;
            ", conn);

            cmd.Parameters.AddWithValue("@processingDate", processingDate == null ? DBNull.Value : processingDate);
            cmd.Parameters.AddWithValue("@documentStepId", documentStepId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateSignAndParaphe(Guid documentId, Guid userId, string? signImage, string? parapheImage)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE UsersDocuments SET Signature = @signature, Paraphe = @paraph
                WHERE UserId = @userId AND DocumentId = @documentId;
            ", conn);

            cmd.Parameters.Add("@signature", SqlDbType.VarBinary, -1).Value = signImage == null ? DBNull.Value : DataToImage(signImage);
            cmd.Parameters.Add("@paraph", SqlDbType.VarBinary, -1).Value = parapheImage == null ? DBNull.Value : DataToImage(parapheImage);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Redirect(Guid documentId, Guid currentUserId, NewDocumentRedirection newDocumentRedirection)
        {
            var inBetweenDocumentsId = await GetInBetweenDocumentSteps(documentId, currentUserId, newDocumentRedirection.TargetDocumentStepId);

            for (int i = 0; i < inBetweenDocumentsId.Count; i += 1)
            {
                await UpdateProcessingDate(inBetweenDocumentsId[i], null);

                // await UpdateSignAndParaphe(documentId, Guid.Parse(inBetweenDocumentsId[i]), null, null);
            }

            await _validationsHistoryRepository.AddToValidationsHistory(new ValidationHistory
            {
                FromUserId = currentUserId,
                ToDocumentStepId = newDocumentRedirection.TargetDocumentStepId,
                DocumentId = documentId,
                Comment = newDocumentRedirection.Message,
                ActionType = DocumentActionType.Redirect,
                CreationDate = DateTime.Now,
            });
        }

        // public async Task<Model.UserDocument?> Create(Model.UserDocument newUserDocument)
        // {
        // if (await _db.UsersDocuments.AnyAsync(x => x.Id == newUserDocument.Id))
        //     return null;

        // newUserDocument = _db.UsersDocuments.Add(newUserDocument).Entity;
        // await _db.SaveChangesAsync();

        // return newUserDocument;
        // }

        public async Task<List<Field>> GetFields(Guid documentId, Guid userId)
        {
            var res = new List<Field>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT f.Variable, f.FirstPage, f.LastPage, f.X, f.Y, f.Width, f.Height, f.PDF_Width, f.PDF_Height, f.FieldType
                FROM Fields AS f
                WHERE f.DocumentId = @documentId
                AND f.UserId = @userId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new Field
                {
                    Variable = reader["Variable"].ToString()!,
                    FirstPage = (int)reader["FirstPage"],
                    LastPage = (int)reader["LastPage"],
                    X = Convert.ToDouble(reader["X"]),
                    Y = Convert.ToDouble(reader["Y"]),
                    Width = Convert.ToDouble(reader["Width"]),
                    Height = Convert.ToDouble(reader["Height"]),
                    PDF_Width = Convert.ToDouble(reader["PDF_Width"]),
                    PDF_Height = Convert.ToDouble(reader["PDF_Height"]),
                    FieldType = (FieldType)reader["FieldType"]
                });
            }

            return res;
        }

        private async Task<bool> HasDocumentSignature(Guid documentId, Guid userId, FieldType fieldType)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) AS total
                FROM Fields AS f
                WHERE f.DocumentId = @documentId
                AND f.UserId = @userId
                AND f.FieldType = @fieldType;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@fieldType", fieldType);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return (int)reader["total"] > 0;
            }

            return false;
        }

        public async Task AddField(Guid documentId, Guid userId, Field field)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO Fields (Variable, FirstPage, LastPage, X, Y, Width, Height, PDF_Width, PDF_Height, FieldType, DocumentId, UserId)
                VALUES (@variable, @firstPage, @lastPage, @x, @y, @width, @height, @pdfWidth, @pdfHeight, @fieldType, @documentId, @userId);
            ", conn);

            cmd.Parameters.AddWithValue("@variable", field.Variable);
            cmd.Parameters.AddWithValue("@firstPage", field.FirstPage);
            cmd.Parameters.AddWithValue("@lastPage", field.LastPage);
            cmd.Parameters.AddWithValue("@x", field.X);
            cmd.Parameters.AddWithValue("@y", field.Y);
            cmd.Parameters.AddWithValue("@width", field.Width);
            cmd.Parameters.AddWithValue("@height", field.Height);
            cmd.Parameters.AddWithValue("@pdfWidth", field.PDF_Width);
            cmd.Parameters.AddWithValue("@pdfHeight", field.PDF_Height);
            cmd.Parameters.AddWithValue("@fieldType", field.FieldType);
            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@userId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        [GeneratedRegex(@"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$")]
        private static partial Regex MyRegex();

        private static byte[]? DataToImage(string data)
        {
            if (data == null)
                return null;

            var matchGroups = MyRegex().Match(data).Groups;
            var base64Data = matchGroups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);

            return binData;
        }
    }
}
