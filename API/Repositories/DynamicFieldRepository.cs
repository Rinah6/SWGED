using API.Data.Entities;
using API.Services;
using Microsoft.Data.SqlClient;

namespace API.Repositories
{
    public class DynamicFieldRepository
    {
        private readonly string _connectionString;
        private readonly DocumentService _documentService;

        public DynamicFieldRepository(IConfiguration configuration, DocumentService documentService)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
            _documentService = documentService;
        }

        public async Task<List<GlobalDynamicField>> GetGlobalDynamicFieldsListByProjectId(Guid projectId)
        {
            var res = new List<GlobalDynamicField>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id, Label
                FROM DynamicFields
                WHERE ProjectId = @projectId 
                AND DeletionDate IS NULL AND IsGlobal = 1;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new GlobalDynamicField
                {
                    Id = reader["Id"].ToString()!,
                    Label = reader["Label"].ToString()!
                });
            }

            return res;
        }

        public async Task<List<GlobalDynamicField_>> GetAllGlobalDynamicFieldsByProjectId(Guid projectId)
        {
            var res = new List<GlobalDynamicField_>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT df.Id, df.Label, df.IsRequired, df.Type
                FROM DynamicFields AS df
                WHERE df.ProjectId = @projectId 
                AND df.DeletionDate IS NULL 
                AND df.IsGlobal = 1
                AND df.IsForUsersProject = 1;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var Id = Guid.Parse(reader["Id"].ToString()!);
                var dynamicFieldType = (Data.DynamicFieldType)reader["Type"];

                res.Add(new GlobalDynamicField_
                {
                    Id = Id,
                    Label = reader["Label"].ToString()!,
                    IsRequired = (bool)reader["IsRequired"],
                    Type = dynamicFieldType,
                    Values = dynamicFieldType == Data.DynamicFieldType.Texte || dynamicFieldType == Data.DynamicFieldType.Date || dynamicFieldType == Data.DynamicFieldType.Checkbox ? null : await GetDynamicFieldItems(Id)
                });
            }

            return res;
        }

        public async Task<List<GlobalDynamicField_>> GetAllGlobalSuppliersDynamicFieldsByProjectId(Guid projectId)
        {
            var res = new List<GlobalDynamicField_>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id, Label, IsRequired, Type
                FROM DynamicFields
                WHERE ProjectId = @projectId 
                AND DeletionDate IS NULL 
                AND IsGlobal = 1
                AND IsForSuppliers = 1;
            ", conn);

            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var Id = Guid.Parse(reader["Id"].ToString()!);
                var dynamicFieldType = (Data.DynamicFieldType)reader["Type"];

                res.Add(new GlobalDynamicField_
                {
                    Id = Id,
                    Label = reader["Label"].ToString()!,
                    IsRequired = (bool)reader["IsRequired"],
                    Type = dynamicFieldType,
                    Values = dynamicFieldType == Data.DynamicFieldType.Texte || dynamicFieldType == Data.DynamicFieldType.Date || dynamicFieldType == Data.DynamicFieldType.Checkbox ? null : await GetDynamicFieldItems(Id)
                });
            }

            return res;
        }

        public async Task<GlobalDynamicFieldDetails?> GetGlobalDynamicDetailByIdAndProjectId(Guid Id, Guid projectId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT d.Label, d.IsForUsersProject, d.IsForSuppliers, d.IsRequired, dt.Id AS TypeId, dt.Title AS TypeTitle
                FROM DynamicFields AS d
                INNER JOIN DynamicFieldTypes as dt ON d.Type = dt.Id
                WHERE d.Id = @Id 
                AND d.ProjectId = @projectId 
                AND d.DeletionDate IS NULL 
                AND d.IsGlobal = 1;
            ", conn);

            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@projectId", projectId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var dynamicFieldType = (Data.DynamicFieldType)reader["TypeId"];

                return new GlobalDynamicFieldDetails
                {
                    Label = reader["Label"].ToString()!,
                    IsForUsersProject = (bool)reader["IsForUsersProject"],
                    IsForSuppliers = (bool)reader["IsForSuppliers"],
                    IsRequired = (bool)reader["IsRequired"],
                    Type = reader["TypeTitle"].ToString()!,
                    Values = dynamicFieldType == Data.DynamicFieldType.Texte || dynamicFieldType == Data.DynamicFieldType.Date || dynamicFieldType == Data.DynamicFieldType.Checkbox || dynamicFieldType == Data.DynamicFieldType.File ? null : await GetDynamicFieldItems(Id)
                };
            }

            return null;
        }

        public async Task<List<DynamicFieldType>> GetDynamicFieldTypes()
        {
            var res = new List<DynamicFieldType>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id, Title
                FROM DynamicFieldTypes;
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new DynamicFieldType
                {
                    Id = Enum.Parse<Data.DynamicFieldType>(reader["Id"].ToString()!),
                    Title = reader["Title"].ToString()!
                });
            }

            return res;
        }

        public async Task AddDynamicFieldItem(string value, Guid dynamicFieldId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DynamicFieldItems (Id, Value, DynamicFieldId, CreatedBy)
                VALUES (@Id, @value, @dynamicFieldId, @createdBy);
            ", conn);

            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@value", value);
            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@createdBy", currentUserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveDynamicFieldItem(Guid dynamicFieldItemId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var Id = Guid.NewGuid();

            using var cmd = new SqlCommand(@"
                UPDATE DynamicFieldItems SET DeletionDate = GETUTCDATE(), DeletedBy = @deletedBy
                WHERE Id = @dynamicFieldItemId;
            ", conn);

            cmd.Parameters.AddWithValue("@deletedBy", currentUserId);
            cmd.Parameters.AddWithValue("@dynamicFieldItemId", dynamicFieldItemId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddDocumentDynamicAttachement(Guid userId, IFormFile dynamicAttachement, Guid documentId, Guid dynamicFieldId)
        {
            var dynamicAttachementPath = await _documentService.CreateDynamicAttachement(userId, dynamicAttachement);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DocumentsDynamicAttachements (DocumentId, DynamicFieldId, Filename, FilePath) 
                VALUES (@documentId, @dynamicFieldId, @filename, @filePath);
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@filename", dynamicAttachement.FileName);
            cmd.Parameters.AddWithValue("@filePath", dynamicAttachementPath);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddGlobalDynamicField(GlobalDynamicFieldToAdd dynamicFieldToAdd, Guid projectId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var Id = Guid.NewGuid();

            using var cmd = new SqlCommand(@"
                INSERT INTO DynamicFields (Id, Label, IsForUsersProject, IsForSuppliers, IsRequired, Type, projectId, IsGlobal, CreatedBy)
                VALUES (@Id, @Label, @IsForUsersProject, @IsForSuppliers, @IsRequired, @Type, @projectId, 1, @createdBy);
            ", conn);

            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@Label", dynamicFieldToAdd.Label);
            cmd.Parameters.AddWithValue("@IsForUsersProject", dynamicFieldToAdd.IsForUsersProject);
            cmd.Parameters.AddWithValue("@IsForSuppliers", dynamicFieldToAdd.IsForSuppliers);
            cmd.Parameters.AddWithValue("@IsRequired", dynamicFieldToAdd.IsRequired);
            cmd.Parameters.AddWithValue("@Type", dynamicFieldToAdd.Type);
            cmd.Parameters.AddWithValue("@projectId", projectId);
            cmd.Parameters.AddWithValue("@createdBy", currentUserId);

            await cmd.ExecuteNonQueryAsync();

            if (dynamicFieldToAdd.Values != null)
            {
                List<Task> concurrentTasks = new();

                for (var i = 0; i < dynamicFieldToAdd.Values.Count; i += 1)
                {
                    concurrentTasks.Add(AddDynamicFieldItem(dynamicFieldToAdd.Values[i], Id, currentUserId));
                }

                await Task.WhenAll(concurrentTasks);
            }
        }

        public async Task RemoveGlobalDynamicField(Guid dynamicFieldId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var Id = Guid.NewGuid();

            using var cmd = new SqlCommand(@"
                UPDATE DynamicFields SET DeletionDate = GETUTCDATE(), DeletedBy = @deletedBy 
                WHERE Id = @dynamicFieldId;
            ", conn);

            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@deletedBy", currentUserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddDocumentDynamicField(Guid documentId, Guid dynamicFieldId, string value)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DocumentDynamicFields (DocumentId, DynamicFieldId, Value)
                VALUES (@documentId, @dynamicFieldId, @value);
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@value", value);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<DynamicFieldItem>> GetDynamicFieldItems(Guid dynamicFieldId)
        {
            var res = new List<DynamicFieldItem>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT Id, Value
                FROM DynamicFieldItems
                WHERE DynamicFieldId = @dynamicFieldId 
                AND DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new DynamicFieldItem
                {
                    Id = reader["Id"].ToString()!,
                    Value = reader["Value"].ToString()!
                });
            }

            return res;
        }

        public async Task<List<DocumentDynamicField>> GetAllDynamicFieldsByDocumentId(Guid documentId)
        {
            var res = new List<DocumentDynamicField>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT df.Id, df.Label, ddf.Value
                FROM DocumentDynamicFields AS ddf
                LEFT JOIN DynamicFields AS df ON df.Id = ddf.DynamicFieldId
                WHERE ddf.DocumentID = @documentId 
                AND df.DeletionDate IS NULL;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new DocumentDynamicField
                {
                    Id = reader["Id"].ToString()!,
                    Label = reader["Label"].ToString()!,
                    Value = reader["Value"].ToString()!
                });
            }

            return res;
        }

        public async Task UpdateDynamicFieldLabel(Guid dynamicFieldId, string label)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE DynamicFields SET Label = @label
                WHERE Id = @dynamicFieldId;
            ", conn);

            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@label", label);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateDynamicFieldRequirement(Guid dynamicFieldId, bool isRequired)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE DynamicFields SET IsRequired = @isRequired
                WHERE Id = @dynamicFieldId;
            ", conn);

            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@isRequired", isRequired);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateDynamicFieldUsersProjectVisibility(Guid dynamicFieldId, bool isForUsersProject)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE DynamicFields SET IsForUsersProject = @isForUsersProject
                WHERE Id = @dynamicFieldId;
            ", conn);

            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@isForUsersProject", isForUsersProject);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateDynamicFieldSuppliersVisibility(Guid dynamicFieldId, bool isForSuppliers)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE DynamicFields SET IsForSuppliers = @isForSuppliers
                WHERE Id = @dynamicFieldId;
            ", conn);

            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@isForSuppliers", isForSuppliers);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<DocumentDynamicAttachementDetails>> GetDocumentDynamicAttachements(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT df.Id, df.Label, dda.Filename 
                FROM DynamicFields AS df 
                INNER JOIN DocumentsDynamicAttachements AS dda ON df.Id = dda.DynamicFieldId
                WHERE dda.DocumentId = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            var res = new List<DocumentDynamicAttachementDetails>();

            while (await reader.ReadAsync())
            {
                res.Add(new DocumentDynamicAttachementDetails
                {
                    Id = reader["Id"].ToString()!,
                    Label = reader["Label"].ToString()!,
                    Filename = reader["Filename"].ToString()!
                });
            }

            return res;
        }

        public async Task<string?> GetDynamicAttachementPath(Guid dynamicFieldId, Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT dda.FilePath
                FROM DocumentsDynamicAttachements AS dda
                WHERE dda.DynamicFieldId = @dynamicFieldId
                AND dda.DocumentId = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@dynamicFieldId", dynamicFieldId);
            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader["FilePath"].ToString()!;
            }

            return null;
        }
    }
}
