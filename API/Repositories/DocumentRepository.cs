using Microsoft.EntityFrameworkCore;
using API.Context;
using API.Data;
using Microsoft.Data.SqlClient;
using API.Data.Entities;
using API.Model;
using Newtonsoft.Json;
using System.Text;

namespace API.Repositories
{
    public partial class DocumentRepository
    {
        private readonly string _connectionString;
        private readonly SoftGED_DBContext _db;
        private readonly ProjectRepository _projectRepository;
        private readonly UserRepository _userRepository;

        public DocumentRepository(IConfiguration configuration, SoftGED_DBContext db, ProjectRepository projectRepository, UserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
            _db = db;
        }

        public async Task<Model.Document?> GetWithAttachement(Guid documentId)
        {
            return await _db.Documents.Include(u => u.Attachements).FirstOrDefaultAsync(x => x.Id == documentId);
        }

        public async Task<int> GetCurrentDocumentStepNumber(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT TOP 1 dst.StepNumber
                FROM DocumentSteps AS dst
                INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId 
                WHERE us.ProcessingDate IS NOT NULL
                AND dst.DocumentId = @documentId
                ORDER BY dst.StepNumber DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return (int)reader["StepNumber"];
            }

            return 0;
        }

        public async Task<bool> CheckIsTheCurrentStepTurn(Guid userId, Guid documentId, DocumentStatus documentStatus)
        {
            if (documentStatus == DocumentStatus.Ongoing)
            {
                var currentDocumentStepNumber = await GetCurrentDocumentStepNumber(documentId);

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand(@"
                    SELECT dst.Role
                    FROM DocumentSteps AS dst
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId 
                    WHERE dst.DocumentId = @documentId
                    AND us.UserId = @userId
                    AND dst.StepNumber = @stepNumber + 1
                ", conn);

                cmd.Parameters.AddWithValue("@documentId", documentId);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@stepNumber", currentDocumentStepNumber);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public async Task<List<API.Data.Entities.Document>> GetDocuments(Guid userId, DocumentStatus documentStatus)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var res = new List<API.Data.Entities.Document>();

                var projectId = await _projectRepository.GetProjectByUserId(userId);

                if (projectId != null)
                {
                    var sqlCommand = @"
                    SELECT * FROM (
                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                        WHERE d.SenderId = @userId
                        AND d.Status = @documentStatus
                        AND d.DeletionDate IS NULL

                        UNION

                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                        INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                        INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                        INNER JOIN USERS AS u ON us.UserId = u.Id
                        WHERE us.UserId = @userId
                        AND u.ProjectId = @projectId
                        AND d.Status = @documentStatus
                        AND us.ProcessingDate IS NOT NULL
                        AND d.DeletionDate IS NULL
                        AND us.DeletionDate IS NULL
                    ) AS query
                    ORDER BY query.CreationDate DESC;
                ";

                    if (documentStatus == DocumentStatus.Archived)
                    {
                        sqlCommand = @"
                    SELECT * FROM (
                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
	                    INNER JOIN USERS AS u ON ds.Id = u.Id
                        WHERE d.SenderId = @userId
	                    AND u.ProjectId = @projectId
                        AND d.Status = 3
                        AND d.DeletionDate IS NULL

                        UNION

                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                        INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                        INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                        INNER JOIN USERS AS u ON us.UserId = u.Id
                        WHERE us.UserId = @userId
                        AND d.Status = 3
                        AND us.ProcessingDate IS NOT NULL
                        AND d.DeletionDate IS NULL
                        AND us.DeletionDate IS NULL
                        AND (
                            d.CanBeAccessedByAnyone = 1
                            OR (
                                SELECT TOP 1 uda.CreationDate
                                FROM UsersDocumentsAccesses AS uda
                                WHERE uda.DocumentId = d.Id
                                AND uda.UserId = @userId
                                AND uda.DeletionDate IS NULL
                                ORDER BY uda.CreationDate DESC
                            ) IS NOT NULL
                        )

                        UNION

                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
	                    INNER JOIN USERS AS u ON ds.Id = u.Id
                        WHERE (
                            d.Status = 3
		                    AND u.ProjectId = @projectId
                            AND (
                                d.CanBeAccessedByAnyone = 1 
                                OR (
                                    SELECT TOP 1 uda.CreationDate
                                    FROM UsersDocumentsAccesses AS uda
                                    WHERE uda.DocumentId = d.Id
                                    AND uda.UserId = @userId
                                    AND uda.DeletionDate IS NULL
                                    ORDER BY uda.CreationDate DESC
                                ) IS NOT NULL
                            )
                        )
                    ) AS query 
                        ORDER BY query.CreationDate DESC;
                    ";
                    }

                    using var cmd = new SqlCommand(sqlCommand, conn);

                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@documentStatus", documentStatus);
                    cmd.Parameters.AddWithValue("@projectId", projectId?.Id);

                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var documentId = Guid.Parse(reader["Id"].ToString()!);
                        //var site = Guid.Parse(reader["Site"].ToString()!);

                        res.Add(new API.Data.Entities.Document
                        {
                            Id = documentId,
                            Site = reader["Site"].ToString()!,
                            Title = reader["Title"].ToString()!,
                            CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                            Sender = await GetDocumentSenderName(documentId, (SenderType)reader["Type"]),
                            Role = DocumentRole.Reader,
                            IsTheCurrentStepTurn = false,
                        });
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<SupplierDocument>> GetDocumentsSendedBySuppliers(Guid userId)
        {

            try
            {
                var res = new List<SupplierDocument>();

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                List<string> userSites = await _userRepository.GetSitesByUserId(userId);
                if (userSites != null)
                {
                    int i = 1;
                    int j = userSites.Count;
                    StringBuilder sbin = new StringBuilder();
                    sbin.Append("AND d.Site IN (");
                    foreach (string siteid in userSites)
                    {
                        sbin.Append("@userSites" + i.ToString() + ",");
                        if (i == j)
                        {
                            sbin = sbin.Remove(sbin.Length - 1, 1);
                        }

                        i++;
                    }
                    sbin.Append(") ");

                    string sql = @"
                    SELECT d.Id, d.CreationDate, d.Message, d.Object, d.Title, d.Status, d.Site, s.NIF, s.STAT, s.Name
                    FROM Documents AS d
                    INNER JOIN Suppliers AS s ON d.SenderId = s.Id
                    INNER JOIN DocumentsReceptions AS dr ON d.Id = dr.DocumentId
                    INNER JOIN Users AS u ON dr.UserId = u.Id
                    INNER JOIN DocumentsSenders AS ds ON s.Id = ds.Id
                    WHERE u.Id = @userId 
                    {0}
                    AND ds.Type = 1
                    AND d.Status = 0
                    ORDER BY d.CreationDate DESC;
                    ";
                    sql = string.Format(sql, sbin);

                    using var cmd = new SqlCommand(sql, conn);

                    StringBuilder sb = new StringBuilder();
                    i = 1;
                    foreach (string siteid in userSites)
                    {
                        sb.Append("@userSites" + i.ToString() + ",");
                        if (i == j)
                        {
                            sb = sb.Remove(sb.Length - 1, 1);
                        }
                        cmd.Parameters.AddWithValue("@userSites" + i.ToString(), siteid);

                        i++;
                    }
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var documentId = Guid.Parse(reader["Id"].ToString()!);
                        var nif = reader["NIF"];
                        var stat = reader["STAT"];

                        res.Add(new SupplierDocument
                        {
                            Id = documentId,
                            Title = reader["Title"].ToString()!,
                            Site = reader["Site"].ToString()!,
                            Message = reader["Message"].ToString()!,
                            Object = reader["Object"].ToString()!,
                            NIF = nif == DBNull.Value ? null : nif.ToString(),
                            STAT = stat == DBNull.Value ? null : stat.ToString(),
                            Name = reader["Name"].ToString()!,
                            De = "",
                            Pour = "",
                            CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                            Status = (DocumentStatus)reader["Status"],
                            WasSendedByASupplier = true,
                        });
                    }

                    return res;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<SupplierDocument>> GetDocumentsBySupplierId(Guid supplierId)
        {
            var res = new List<SupplierDocument>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT d.Id, d.CreationDate, d.Message, d.Object, d.Title, d.Status, s.NIF, s.STAT, s.Name, d.Site
                FROM Documents AS d
                INNER JOIN Suppliers AS s ON d.SenderId = s.Id
                INNER JOIN DocumentsSenders AS ds ON s.Id = ds.Id
                WHERE s.Id = @supplierId 
                AND ds.Type = 1
                ORDER BY d.CreationDate DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@supplierId", supplierId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var documentId = Guid.Parse(reader["Id"].ToString()!);
                var nif = reader["NIF"];
                var stat = reader["STAT"];
                //var site = Guid.Parse(reader["Site"].ToString()!);

                res.Add(new SupplierDocument
                {
                    Id = documentId,
                    Site = reader["Site"].ToString()!,
                    Title = reader["Title"].ToString()!,
                    Message = reader["Message"].ToString()!,
                    Object = reader["Object"].ToString()!,
                    NIF = nif is DBNull ? null : nif.ToString(),
                    STAT = stat is DBNull ? null : stat.ToString(),
                    Name = reader["Name"].ToString()!,
                    De = "",
                    Pour = "",
                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                    Status = (DocumentStatus)reader["Status"],
                    WasSendedByASupplier = true,
                });
            }

            return res;
        }

        public async Task<string> GetDocumentSenderName(Guid documentId, SenderType senderType)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sqlCommand = @"
                SELECT u.Username AS Name
                FROM Documents AS d
                INNER JOIN Users AS u ON d.SenderId = u.Id
                WHERE d.Id = @documentId;
            ";

            if (senderType == SenderType.Supplier)
            {
                sqlCommand = @"
                    SELECT s.Name
                    FROM Documents AS d
                    INNER JOIN Suppliers AS s ON d.SenderId = s.Id
                    WHERE d.Id = @documentId;
                ";
            }

            using var cmd = new SqlCommand(sqlCommand, conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader["Name"].ToString()!;
            }

            return "";
        }

        public async Task<List<API.Data.Entities.Document>> GetReceivedDocuments(Guid currentUserId)
        {

            try
            {
                var res = new List<API.Data.Entities.Document>();

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                List<string> userSites = await _userRepository.GetSitesByUserId(currentUserId);
                if (userSites != null)
                {
                    int i = 1;
                    int j = userSites.Count;
                    StringBuilder sbin = new StringBuilder();
                    sbin.Append("AND d.Site IN (");
                    foreach (string siteid in userSites)
                    {
                        sbin.Append("@userSites" + i.ToString() + ",");
                        if (i == j)
                        {
                            sbin = sbin.Remove(sbin.Length - 1, 1);
                        }

                        i++;
                    }
                    sbin.Append(") ");

                    string sql = @"
                    SELECT d.Id, d.Title, d.CreationDate, d.Site, ds.Type, dst.Role
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                    INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    WHERE us.UserId = @userId 
                    AND us.ProcessingDate IS NULL
                    {0}
                    AND d.Status = @ongoingDocumentsStatus
                    AND d.DeletionDate IS NULL
                    AND dst.StepNumber = (
                        SELECT * FROM (
                            SELECT TOP 1 (dst.StepNumber + 1) AS stepNumber
                            FROM DocumentSteps AS dst
                            INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId 
                            WHERE us.ProcessingDate IS NOT NULL
                            AND dst.DocumentId = d.Id
                            ORDER BY dst.StepNumber DESC

                            UNION 

                            SELECT TOP 1 dst.stepNumber
                            FROM DocumentSteps AS dst
                            INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId 
                            WHERE us.ProcessingDate IS NULL
                            AND dst.DocumentId = d.Id
                            AND dst.StepNumber = 1
                            AND us.UserId = @userId
                            ORDER BY dst.StepNumber DESC
                        ) AS query
                    )
                    ORDER BY d.CreationDate DESC;
                    ";
                    sql = string.Format(sql, sbin);

                    using var cmd = new SqlCommand(sql, conn);

                    StringBuilder sb = new StringBuilder();
                    i = 1;
                    foreach (string siteid in userSites)
                    {
                        sb.Append("@userSites" + i.ToString() + ",");
                        if (i == j)
                        {
                            sb = sb.Remove(sb.Length - 1, 1);
                        }
                        cmd.Parameters.AddWithValue("@userSites" + i.ToString(), siteid);

                        i++;
                    }
                    cmd.Parameters.AddWithValue("@ongoingDocumentsStatus", DocumentStatus.Ongoing);
                    cmd.Parameters.AddWithValue("@userId", currentUserId);

                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var documentId = Guid.Parse(reader["Id"].ToString()!);

                        res.Add(new API.Data.Entities.Document
                        {
                            Id = documentId,
                            Site = reader["Site"].ToString()!,
                            Title = reader["Title"].ToString()!,
                            CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                            Sender = await GetDocumentSenderName(documentId, (SenderType)reader["Type"]),
                            Role = (DocumentRole)reader["Role"],
                            IsTheCurrentStepTurn = true,
                        });
                    }

                    return res;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<API.Data.Entities.Document>> GetSendedDocuments(Guid userId, string currentUserUsername)
        {
            var res = new List<API.Data.Entities.Document>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT d.Id, d.Title, d.CreationDate, d.Site
                FROM Documents AS d
                WHERE d.SenderId = @userId
                AND d.DeletionDate IS NULL
                AND d.Id = (
                    SELECT dst.DocumentId
                    FROM DocumentSteps AS dst
                    WHERE dst.StepNumber = 1
                    AND dst.DocumentId = d.Id
                );
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var documentId = Guid.Parse(reader["Id"].ToString()!);
                //var site = Guid.Parse(reader["Site"].ToString()!);

                res.Add(new API.Data.Entities.Document
                {
                    Id = documentId,
                    Site = reader["Site"].ToString()!,
                    Title = reader["Title"].ToString()!,
                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                    Sender = currentUserUsername,
                    Role = DocumentRole.Reader,
                    IsTheCurrentStepTurn = false,
                });
            }

            return res;
        }

        public async Task<List<API.Data.Entities.Document>> GetCommonDocuments(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var res = new List<API.Data.Entities.Document>();

            var projectId = await _projectRepository.GetProjectByUserId(userId);

            if (projectId != null)
            {
                using var cmd = new SqlCommand(@"
                SELECT * FROM (
                    SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                    INNER JOIN USERS AS u ON d.SenderId = u.Id
                    WHERE d.SenderId = @userId
	                AND u.ProjectId = @projectId
                    AND d.Status = 3
    
                    AND u.ProjectId = (
                        SELECT Projects.Id FROM Projects 
                        INNER JOIN Users ON Projects.Id = Users.ProjectId
                        WHERE Users.Id = @userId
                    )

                    UNION ALL

                    SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                    INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    INNER JOIN USERS AS u ON us.UserId = u.Id
                    WHERE us.UserId = @userId
                    AND d.Status = 3
                    AND u.ProjectId = (
                        SELECT Projects.Id FROM Projects 
                        INNER JOIN Users ON Projects.Id = Users.ProjectId
                        WHERE Users.Id = @userId
                    )
                    AND (
                        d.CanBeAccessedByAnyone = 1 
                        OR (
                            SELECT TOP 1 uda.CreationDate
                            FROM UsersDocumentsAccesses AS uda
                            WHERE uda.DocumentId = d.Id
                            AND uda.UserId = @userId
                            AND uda.DeletionDate IS NULL
                            ORDER BY uda.CreationDate DESC
                        ) IS NOT NULL
                    )

                    UNION

                    SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
	                INNER JOIN USERS AS u ON ds.Id = u.Id
                    INNER JOIN Suppliers AS s ON ds.Id = s.Id
                    WHERE d.Status = 3
	                AND u.ProjectId = @projectId
                    AND d.DeletionDate IS NULL
                    AND s.ProjectId = (
                        SELECT Projects.Id FROM Projects 
                        INNER JOIN Users ON Projects.Id = Users.ProjectId
                        WHERE Users.Id = @userId
                    )
                    AND (
                        d.CanBeAccessedByAnyone = 1 
                        OR (
                            SELECT TOP 1 uda.CreationDate
                            FROM UsersDocumentsAccesses AS uda
                            WHERE uda.DocumentId = d.Id
                            AND uda.UserId = @userId
                            AND uda.DeletionDate IS NULL
                            ORDER BY uda.CreationDate DESC
                        ) IS NOT NULL
                    )

                    UNION

                    SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
	                INNER JOIN USERS AS u ON ds.Id = u.Id
                    WHERE (
                        d.Status = 3
		                AND u.ProjectId = @projectId
                        AND (
                            d.CanBeAccessedByAnyone = 1 
                            OR (
                                SELECT TOP 1 uda.CreationDate
                                FROM UsersDocumentsAccesses AS uda
                                WHERE uda.DocumentId = d.Id
                                AND uda.UserId = @userId
                                AND uda.DeletionDate IS NULL
                                ORDER BY uda.CreationDate DESC
                            ) IS NOT NULL
                        )
                    )
                ) AS query 
                ORDER BY query.CreationDate DESC;
            ", conn);

                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@archivedDocumentStatus", DocumentStatus.Archived);
                cmd.Parameters.AddWithValue("@projectId", projectId?.Id);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var documentId = Guid.Parse(reader["Id"].ToString()!);
                    //var site = Guid.Parse(reader["Site"].ToString()!);

                    res.Add(new API.Data.Entities.Document
                    {
                        Id = documentId,
                        Site = reader["Site"].ToString()!,
                        Title = reader["Title"].ToString()!,
                        CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                        Sender = await GetDocumentSenderName(documentId, (SenderType)reader["Type"]),
                        Role = DocumentRole.Reader,
                        IsTheCurrentStepTurn = false
                    });
                }
            }

            return res;
        }

        public async Task<int> GetTotalNumberOfDocumentsFromSuppliers(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            List<string> userSites = await _userRepository.GetSitesByUserId(userId);
            if (userSites != null)
            {
                int i = 1;
                int j = userSites.Count;
                StringBuilder sbin = new StringBuilder();
                sbin.Append("AND d.Site IN (");
                foreach (string siteid in userSites)
                {
                    sbin.Append("@userSites" + i.ToString() + ",");
                    if (i == j)
                    {
                        sbin = sbin.Remove(sbin.Length - 1, 1);
                    }

                    i++;
                }
                sbin.Append(") ");

                string sql = @"
                    SELECT COUNT(*) AS Total
                    FROM Documents AS d
                    INNER JOIN Suppliers AS s ON d.SenderId = s.Id
                    INNER JOIN DocumentsReceptions AS dr ON d.Id = dr.DocumentId
                    INNER JOIN Users AS u ON dr.UserId = u.Id
                    INNER JOIN DocumentsSenders AS ds ON s.Id = ds.Id
                    WHERE u.Id = @userId 
                    {0}
                    AND ds.Type = 1
                    AND d.Status = 0;
                ";
                sql = string.Format(sql, sbin);

                using var cmd = new SqlCommand(sql, conn);

                StringBuilder sb = new StringBuilder();
                i = 1;
                foreach (string siteid in userSites)
                {
                    sb.Append("@userSites" + i.ToString() + ",");
                    if (i == j)
                    {
                        sb = sb.Remove(sb.Length - 1, 1);
                    }
                    cmd.Parameters.AddWithValue("@userSites" + i.ToString(), siteid);

                    i++;
                }
                cmd.Parameters.AddWithValue("@userId", userId);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return (int)reader["Total"];
                }

                return 0;
            }
            else
            {
                return 0;
            }
        }

        public async Task<int> GetTotalNumberOfReceivedDocuments(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            List<string> userSites = await _userRepository.GetSitesByUserId(userId);
            if (userSites != null)
            {
                int i = 1;
                int j = userSites.Count;
                StringBuilder sbin = new StringBuilder();
                sbin.Append("AND d.Site IN (");
                foreach (string siteid in userSites)
                {
                    sbin.Append("@userSites" + i.ToString() + ",");
                    if (i == j)
                    {
                        sbin = sbin.Remove(sbin.Length - 1, 1);
                    }

                    i++;
                }
                sbin.Append(") ");

                string sql = @"
                    SELECT COUNT(*) AS Total
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                    INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    WHERE us.UserId = @userId 
                    AND us.ProcessingDate IS NULL
                    {0}
                    AND d.Status = @ongoingDocumentsStatus
                    AND d.DeletionDate IS NULL
                    AND dst.StepNumber = (
                        SELECT * FROM (
                            SELECT TOP 1 (dst.StepNumber + 1) AS stepNumber
                            FROM DocumentSteps AS dst
                            INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId 
                            WHERE us.ProcessingDate IS NOT NULL
                            AND dst.DocumentId = d.Id
                            ORDER BY dst.StepNumber DESC

                            UNION 

                            SELECT TOP 1 dst.stepNumber
                            FROM DocumentSteps AS dst
                            INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId 
                            WHERE us.ProcessingDate IS NULL
                            AND dst.DocumentId = d.Id
                            AND dst.StepNumber = 1
                            AND us.UserId = @userId
                            ORDER BY dst.StepNumber DESC
                        ) AS query
                    );
                ";
                sql = string.Format(sql, sbin);

                using var cmd = new SqlCommand(sql, conn);

                StringBuilder sb = new StringBuilder();
                i = 1;
                foreach (string siteid in userSites)
                {
                    sb.Append("@userSites" + i.ToString() + ",");
                    if (i == j)
                    {
                        sb = sb.Remove(sb.Length - 1, 1);
                    }
                    cmd.Parameters.AddWithValue("@userSites" + i.ToString(), siteid);

                    i++;
                }
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@ongoingDocumentsStatus", DocumentStatus.Ongoing);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return (int)reader["Total"];
                }

                return 0;
            }
            else
            { return 0; }
        }

        public async Task<int> GetTotalNumberOfSendedDocuments(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) AS Total
                FROM Documents AS d
                WHERE d.SenderId = @userId
                AND d.DeletionDate IS NULL
                AND d.Id = (
                    SELECT dst.DocumentId
                    FROM DocumentSteps AS dst
                    WHERE dst.StepNumber = 1
                    AND dst.DocumentId = d.Id
                );
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return (int)reader["Total"];
            }

            return 0;
        }

        public async Task<int> GetTotalNumberOfOngoingDocuments(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) AS Total FROM (
                    SELECT d.Id, d.Title, d.CreationDate, ds.Type
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                    WHERE d.SenderId = @userId
                    AND d.Status = @ongoingDocumentsStatus
                    AND d.DeletionDate IS NULL

                    UNION

                    SELECT d.Id, d.Title, d.CreationDate, ds.Type
                    FROM Documents AS d
                    INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                    INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    INNER JOIN USERS AS u ON us.UserId = u.Id
                    WHERE us.UserId = @userId
                    AND d.Status = @ongoingDocumentsStatus
                    AND us.ProcessingDate IS NOT NULL
                    AND d.DeletionDate IS NULL
                    AND us.DeletionDate IS NULL
                ) AS query;
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@ongoingDocumentsStatus", DocumentStatus.Ongoing);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return (int)reader["Total"];
            }

            return 0;
        }

        public async Task<int> GetTotalNumberOfCanceledDocuments(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            List<string> userSites = await _userRepository.GetSitesByUserId(userId);
            if (userSites != null)
            {
                int i = 1;
                int j = userSites.Count;
                StringBuilder sbin = new StringBuilder();
                sbin.Append("AND d.Site IN (");
                foreach (string siteid in userSites)
                {
                    sbin.Append("@userSites" + i.ToString() + ",");
                    if (i == j)
                    {
                        sbin = sbin.Remove(sbin.Length - 1, 1);
                    }

                    i++;
                }
                sbin.Append(") ");

                string sql = @"
                    SELECT COUNT(*) AS Total FROM (
                        SELECT d.Id, d.Title, d.CreationDate, ds.Type
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                        WHERE d.SenderId = @userId
                        {0}
                        AND d.Status = @canceledDocumentsStatus
                        AND d.DeletionDate IS NULL

                        UNION

                        SELECT d.Id, d.Title, d.CreationDate, ds.Type
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                        INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                        INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                        INNER JOIN USERS AS u ON us.UserId = u.Id
                        WHERE us.UserId = @userId
                        {1}
                        AND d.Status = @canceledDocumentsStatus
                        AND us.ProcessingDate IS NOT NULL
                        AND d.DeletionDate IS NULL
                        AND us.DeletionDate IS NULL
                    ) AS query;
                ";
                sql = string.Format(sql, sbin, sbin);

                using var cmd = new SqlCommand(sql, conn);

                StringBuilder sb = new StringBuilder();
                i = 1;
                foreach (string siteid in userSites)
                {
                    sb.Append("@userSites" + i.ToString() + ",");
                    if (i == j)
                    {
                        sb = sb.Remove(sb.Length - 1, 1);
                    }
                    cmd.Parameters.AddWithValue("@userSites" + i.ToString(), siteid);

                    i++;
                }
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@canceledDocumentsStatus", DocumentStatus.Canceled);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return (int)reader["Total"];
                }

                return 0;
            }
            else
            {
                return 0;
            }
        }

        public async Task<int> GetTotalNumberOfArchivedDocuments(Guid userId)
        {

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                List<string> userSites = await _userRepository.GetSitesByUserId(userId);
                if (userSites != null)
                {
                    int i = 1;
                    int j = userSites.Count;
                    StringBuilder sbin = new StringBuilder();
                    sbin.Append("AND d.Site IN (");
                    foreach (string siteid in userSites)
                    {
                        sbin.Append("@userSites" + i.ToString() + ",");
                        if (i == j)
                        {
                            sbin = sbin.Remove(sbin.Length - 1, 1);
                        }

                        i++;
                    }
                    sbin.Append(") ");

                    var projectId = await _projectRepository.GetProjectByUserId(userId);

                    if (projectId != null)
                    {

                        string sql = @"
                        SELECT COUNT(*) AS Total FROM (
                            SELECT d.Id, d.Title, d.CreationDate, ds.Type
                            FROM Documents AS d
                            INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                            INNER JOIN USERS AS u ON ds.Id = u.Id
                            WHERE d.SenderId = @userId
                            AND u.ProjectId = @projectId
                            AND d.Status = @archivedDocumentStatus
                            AND d.DeletionDate IS NULL

                            UNION

                            SELECT d.Id, d.Title, d.CreationDate, ds.Type
                            FROM Documents AS d
                            INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                            INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                            INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                            INNER JOIN USERS AS u ON us.UserId = u.Id
                            WHERE us.UserId = @userId
                            {0}
                            AND d.Status = @archivedDocumentStatus
                            AND us.ProcessingDate IS NOT NULL
                            AND d.DeletionDate IS NULL
                            AND us.DeletionDate IS NULL
                            AND (
                                d.CanBeAccessedByAnyone = 1 
                                OR (
                                    SELECT TOP 1 uda.CreationDate
                                    FROM UsersDocumentsAccesses AS uda
                                    WHERE uda.DocumentId = d.Id
                                    AND uda.UserId = @userId
                                    AND uda.DeletionDate IS NULL
                                    ORDER BY uda.CreationDate DESC
                                ) IS NOT NULL
                            )

                            UNION

                            SELECT d.Id, d.Title, d.CreationDate, ds.Type
                            FROM Documents AS d
                            INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                            INNER JOIN USERS AS u ON ds.Id = u.Id
                            WHERE (
                                d.Status = @archivedDocumentStatus
                                AND u.ProjectId = @projectId
                                AND ( 
                                    d.CanBeAccessedByAnyone = 1 
                                    OR (
                                        SELECT TOP 1 uda.CreationDate
                                        FROM UsersDocumentsAccesses AS uda
                                        WHERE uda.DocumentId = d.Id
                                        AND uda.UserId = @userId
                                        AND uda.DeletionDate IS NULL
                                        ORDER BY uda.CreationDate DESC
                                    ) IS NOT NULL
                                )
                            )
                        ) AS query;
                        ";
                        sql = string.Format(sql, sbin);

                        using var cmd = new SqlCommand(sql, conn);

                        StringBuilder sb = new StringBuilder();
                        i = 1;
                        foreach (string siteid in userSites)
                        {
                            sb.Append("@userSites" + i.ToString() + ",");
                            if (i == j)
                            {
                                sb = sb.Remove(sb.Length - 1, 1);
                            }
                            cmd.Parameters.AddWithValue("@userSites" + i.ToString(), siteid);

                            i++;
                        }
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@archivedDocumentStatus", DocumentStatus.Archived);
                        cmd.Parameters.AddWithValue("@projectId", projectId?.Id);

                        using var reader = await cmd.ExecuteReaderAsync();

                        if (await reader.ReadAsync())
                        {
                            return (int)reader["Total"];
                        }

                        return 0;
                    }

                    return 0;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public async Task<int> GetTotalNumberOfCommonDocuments(Guid userId)
        {

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var projectId = await _projectRepository.GetProjectByUserId(userId);

                if ( projectId != null )
                {
                    using var cmd = new SqlCommand(@"
                    SELECT COUNT(*) AS Total FROM (
                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                        INNER JOIN USERS AS u ON d.SenderId = u.Id
                        WHERE d.SenderId = @userId
	                    AND u.ProjectId = @projectId
                        AND d.Status = 3
    
                        AND u.ProjectId = (
                            SELECT Projects.Id FROM Projects 
                            INNER JOIN Users ON Projects.Id = Users.ProjectId
                            WHERE Users.Id = @userId
                        )

                        UNION ALL

                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                        INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                        INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                        INNER JOIN USERS AS u ON us.UserId = u.Id
                        WHERE us.UserId = @userId
                        AND d.Status = 3
                        AND u.ProjectId = (
                            SELECT Projects.Id FROM Projects 
                            INNER JOIN Users ON Projects.Id = Users.ProjectId
                            WHERE Users.Id = @userId
                        )
                        AND (
                            d.CanBeAccessedByAnyone = 1 
                            OR (
                                SELECT TOP 1 uda.CreationDate
                                FROM UsersDocumentsAccesses AS uda
                                WHERE uda.DocumentId = d.Id
                                AND uda.UserId = @userId
                                AND uda.DeletionDate IS NULL
                                ORDER BY uda.CreationDate DESC
                            ) IS NOT NULL
                        )

                        UNION

                        SELECT d.Id, d.Title, d.CreationDate, ds.Type, d.Site
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
	                    INNER JOIN USERS AS u ON ds.Id = u.Id
                        INNER JOIN Suppliers AS s ON ds.Id = s.Id
                        WHERE d.Status = 3
	                    AND u.ProjectId = @projectId
                        AND d.DeletionDate IS NULL
                        AND s.ProjectId = (
                            SELECT Projects.Id FROM Projects 
                            INNER JOIN Users ON Projects.Id = Users.ProjectId
                            WHERE Users.Id = @userId
                        )
                        AND (
                            d.CanBeAccessedByAnyone = 1 
                            OR (
                                SELECT TOP 1 uda.CreationDate
                                FROM UsersDocumentsAccesses AS uda
                                WHERE uda.DocumentId = d.Id
                                AND uda.UserId = @userId
                                AND uda.DeletionDate IS NULL
                                ORDER BY uda.CreationDate DESC
                            ) IS NOT NULL
                        )

                        UNION

                        SELECT d.Id, d.Title, d.CreationDate, ds.Type
                        FROM Documents AS d
                        INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
	                    INNER JOIN USERS AS u ON ds.Id = u.Id
                        WHERE (
                            d.Status = 3
		                    AND u.ProjectId = @projectId
                            AND (
                                d.CanBeAccessedByAnyone = 1 
                                OR (
                                    SELECT TOP 1 uda.CreationDate
                                    FROM UsersDocumentsAccesses AS uda
                                    WHERE uda.DocumentId = d.Id
                                    AND uda.UserId = @userId
                                    AND uda.DeletionDate IS NULL
                                    ORDER BY uda.CreationDate DESC
                                ) IS NOT NULL
                            )
                        )
                    ) AS query;
                ", conn);

                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@archivedDocumentStatus", DocumentStatus.Archived);
                    cmd.Parameters.AddWithValue("@projectId", projectId?.Id);

                    using var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        return (int)reader["Total"];
                    }

                    return 0;
                }

                return 0;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public async Task<Dictionary<string, int>> GetNumberOfDocumentsByStatus(Guid userId, UserRole userRole)
        {
            if (userRole == UserRole.SuperAdmin)
            {
                return new Dictionary<string, int>
                {
                    { "received_from_suppliers", 0 },
                    { "received", 0 },
                    { "sended", 0 },
                    { "ongoing", 0 },
                    { "canceled", 0 },
                    { "archived", await GetTotalNumberOfArchivedDocuments(userId) },
                    { "common_documents", 0 },
                };
            }

            var res = new Dictionary<string, int>
            {
                { "received_from_suppliers", await GetTotalNumberOfDocumentsFromSuppliers(userId) },
                { "received", await GetTotalNumberOfReceivedDocuments(userId) },
                { "sended", await GetTotalNumberOfSendedDocuments(userId) },
                { "ongoing", await GetTotalNumberOfOngoingDocuments(userId) },
                { "canceled", await GetTotalNumberOfCanceledDocuments(userId) },
                { "archived", await GetTotalNumberOfArchivedDocuments(userId) },
                { "common_documents", await GetTotalNumberOfCommonDocuments(userId) },
            };

            return res;
        }

        public async Task<bool> WasSendedByASupplier(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT 1
                FROM Documents AS d
                INNER JOIN DocumentsSenders AS ds ON d.SenderId = ds.Id
                WHERE d.Id = @documentId
                AND ds.Type = @documentSenderType;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@documentSenderType", SenderType.Supplier);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return true;
            }

            return false;
        }

        public async Task<Model.Document?> Get(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT d.Title, d.Filename, d.Message, d.Object, d.OriginalFilename, d.Status, d.Url, d.Site 
                FROM Documents AS d
                WHERE d.Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                //var site = Guid.Parse(reader["Site"].ToString()!);
                return new Model.Document
                {
                    Title = reader["Title"].ToString()!,
                    Site = reader["Site"].ToString()!,
                    Object = reader["Object"].ToString()!,
                    Message = reader["Message"].ToString()!,
                    Filename = reader["Filename"].ToString()!,
                    OriginalFilename = reader["OriginalFilename"].ToString()!,
                    Status = Enum.Parse<DocumentStatus>(reader["Status"].ToString()!),
                    Url = reader["Url"].ToString()!
                };
            }

            return null;
        }

        public async Task<DocumentDetails?> GetWithUser(Guid documentId, Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT * FROM (
                    SELECT d.OriginalFilename, d.Title, d.Message, d.Object, d.CreationDate, d.Status, d.SenderId, d.PhysicalLocation, d.Site
                    FROM Documents AS d
                    INNER JOIN USERS AS us ON d.SenderId = us.Id
                    WHERE d.Id = @documentId 
                    AND d.SenderId = @userId

                    UNION ALL

                    SELECT d.OriginalFilename, d.Title, d.Message, d.Object, d.CreationDate, d.Status, d.SenderId, d.PhysicalLocation, d.Site
                    FROM Documents AS d
                    INNER JOIN DocumentSteps AS dst ON d.Id = dst.DocumentId
                    INNER JOIN UsersSteps AS us ON dst.Id = us.DocumentStepId
                    WHERE d.Id = @documentId 
                    AND us.UserId = @userId
                ) AS query;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var senderId = reader["SenderId"].ToString()!;
                var status = Enum.Parse<DocumentStatus>(reader["Status"].ToString()!);
                //var site = Guid.Parse(reader["Site"].ToString()!);
                return new DocumentDetails
                {
                    Filename = reader["OriginalFilename"].ToString()!,
                    Title = reader["Title"].ToString()!,
                    Site = reader["Site"].ToString()!,
                    Object = reader["Object"].ToString()!,
                    Message = reader["Message"].ToString()!,
                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                    Status = status,
                    IsTheCurrentStepTurn = await CheckIsTheCurrentStepTurn(userId, documentId, status),
                    HasSign = false,
                    HasParaphe = false,
                    PhysicalLocation = reader["PhysicalLocation"].ToString(),
                    IsTheCurrentUserTheSender = senderId == userId.ToString(),
                };
            }

            return null;
        }

        private async Task<bool> WasDocumentAcknowledged(Guid documentId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT sda.InitiatorId
                FROM SuppliersDocumentsAcknowledgements AS sda
                WHERE sda.Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader["InitiatorId"] != DBNull.Value;
            }

            return false;
        }

        public async Task<SupplierDocumentDetails?> GetSuppliersDocumentDetails(Guid documentId, Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT d.Id, d.OriginalFilename, d.CreationDate, d.Message, d.Object, d.Title, d.Status, s.NIF, s.STAT, s.Name, d.Status, d.Site
                FROM Documents AS d
                INNER JOIN Suppliers AS s ON d.SenderId = s.Id
                WHERE d.Id = @documentId;
            ", conn);

            cmd.Parameters.AddWithValue("@documentId", documentId);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var status = (DocumentStatus)reader["Status"];
                var nif = reader["NIF"];
                var stat = reader["STAT"];

                return new SupplierDocumentDetails
                {
                    Filename = reader["OriginalFilename"].ToString()!,
                    Title = reader["Title"].ToString()!,
                    Object = reader["Object"].ToString()!,
                    Message = reader["Message"].ToString()!,
                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                    WasAcknowledged = await WasDocumentAcknowledged(documentId),
                    Status = status,
                    NIF = nif is DBNull ? null : nif.ToString(),
                    STAT = stat is DBNull ? null : stat.ToString(),
                    Name = reader["Name"].ToString()!,
                    IsTheCurrentStepTurn = await CheckIsTheCurrentStepTurn(userId, documentId, status),
                    WasSendedByASupplier = true
                };
            }

            return null;
        }

        public async Task<SharedDocumentDetails?> GetDocumentDetails(Guid documentId)
        {
            var document = await Get(documentId);

            if (document == null)
            {
                return null;
            }

            return new SharedDocumentDetails
            {
                CreationDate = document.CreationDate,
                Filename = document.OriginalFilename,
                Message = document.Message,
                Object = document.Object,
                Title = document.Title,
                Status = document.Status
            };
        }

        public async Task ChangeFileName(Guid Id, string name)
        {
            var document = await Get(Id);

            if (document == null)
                return;

            document.OriginalFilename = name;

            await _db.SaveChangesAsync();
        }

        public async Task UpdateDocumentPhysicalLocation(string Id, string physicalLocation)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE Documents SET PhysicalLocation = @physicalLocation 
                WHERE Id = @Id;
            ", conn);

            cmd.Parameters.AddWithValue("@physicalLocation", physicalLocation);
            cmd.Parameters.AddWithValue("@Id", Id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
