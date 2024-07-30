using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using API.Data.Entities;
using API.Services;
using CliWrap;
using Newtonsoft.Json;

namespace API.Repositories
{
    public class SignatureRepository
    {
        private readonly string _connectionString;
        private readonly DocumentRepository _documentRepository;
        private readonly MailService _mailService;

        public SignatureRepository(IConfiguration configuration, DocumentRepository documentRepository, MailService mailService)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;

            _documentRepository = documentRepository;
            _mailService = mailService;
        }

        private async Task InsertUserSignature(Guid signatureId, Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO UserSignatures (Id, UserId)
                VALUES (@signatureId, @userId);
            ", conn);

            cmd.Parameters.AddWithValue("@signatureId", signatureId);
            cmd.Parameters.AddWithValue("@userId", currentUserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Guid?> ExtractSignatureFromImage(IFormFile image, Guid currentUserId)
        {
            var id = Guid.NewGuid();

            var absolutePath = Directory.GetCurrentDirectory();

            var pythonFilePath = Path.Combine(absolutePath, "wwwroot/scripts/extract_signature.py");

            var inputImagePath = Path.Combine(absolutePath, $"wwwroot/tmp_signatures", image.FileName);
            await Utils.File.CreateFile(image, inputImagePath);

            var targetPath = Path.Combine(absolutePath, $"wwwroot/signatures/{id}.png");

            await Cli.Wrap("\"C:\\Program Files\\Python312\\python.exe\"")
                .WithArguments($"\"{pythonFilePath}\" -i \"{inputImagePath}\" -o \"{targetPath}\"")
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteAsync();

            await InsertUserSignature(id, currentUserId);

            return id;
        }

        public async Task<MemoryStream?> GetSignature(Guid signatureId)
        {
            var targetPath = Path.Combine($"wwwroot/signatures/{signatureId}.png");

            if (System.IO.File.Exists(targetPath))
            {
                var fileStream = new FileStream(targetPath, FileMode.Open, FileAccess.Read);

                var memoryStream = new MemoryStream();

                await fileStream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;

                await fileStream.DisposeAsync();

                return memoryStream;
            }

            return null;
        }

        private async Task InsertVerificationTokens(Guid id, string content)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO VerificationTokens (Id, Content)
                VALUES (@id, @content);
            ", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@content", content);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task InsertIntoVerificationTokensHistory(string content, Guid signatureId, Guid verificationTokenId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO VerificationTokensHistory (Content, SignatureId, VerificationTokenId)
                VALUES (@content, @signatureId, @verificationTokenId);
            ", conn);

            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@signatureId", signatureId);
            cmd.Parameters.AddWithValue("@verificationTokenId", verificationTokenId);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<Guid?> GetVerificationTokenId(Guid currentUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT TOP 1 vt.Id
                FROM VerificationTokens AS vt
                INNER JOIN VerificationTokensHistory AS vth ON vt.Id = vth.VerificationTokenId
                INNER JOIN UserSignatures AS us ON vth.SignatureId = us.Id
                WHERE us.UserId = @currentUserId
                ORDER BY vt.CreationDate DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@currentUserId", currentUserId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Guid.Parse(reader["Id"].ToString()!);
            }

            return null;
        }

        public async Task UpdateVerificationTokens(Guid signatureId, Guid currentUserId, string currentUserEmail)
        {
            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(71)).ToLower();

            var body = @$"
                {token}
            ";

            var verificationTokenId = await GetVerificationTokenId(currentUserId);

            if (verificationTokenId == null)
            {
                var id = Guid.NewGuid();

                await InsertVerificationTokens(id, token);

                await InsertIntoVerificationTokensHistory(token, signatureId, id);

                await _mailService.SendEmailWithAttachements("Token d'authenticité", body, new List<string> { currentUserEmail }, new List<string> { Path.Combine($"wwwroot/signatures/{signatureId}.png") });

                return;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE VerificationTokens SET Content = @content
                WHERE Id = @verificationTokenId;
            ", conn);

            cmd.Parameters.AddWithValue("@content", token);
            cmd.Parameters.AddWithValue("@verificationTokenId", verificationTokenId);

            await cmd.ExecuteNonQueryAsync();

            await InsertIntoVerificationTokensHistory(token, signatureId, (Guid)verificationTokenId);

            await _mailService.SendEmailWithAttachements("Token d'authenticité", body, new List<string> { currentUserEmail }, new List<string> { Path.Combine($"wwwroot/signatures/{signatureId}.png") });
        }

        public async Task<bool> VerifyToken(Guid currentUserId, string content)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT TOP 1 1
                FROM VerificationTokens AS vt
                INNER JOIN VerificationTokensHistory AS vth ON vt.Id = vth.VerificationTokenId
                INNER JOIN UserSignatures AS us ON vth.SignatureId = us.Id
                WHERE us.UserId = @currentUserId
                AND vt.Content = @content
                ORDER BY vt.CreationDate DESC;
            ", conn);

            cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("@content", content);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return true;
            }

            return false;
        }

        // public static void GenerateAndSaveKeys(string publicKeyPath, string privateKeyPath)
        // {
        //     using (var rsa = new RSACryptoServiceProvider(4096))
        //     {
        //         File.WriteAllText(publicKeyPath, rsa.ToXmlString(false));
        //         File.WriteAllText(privateKeyPath, rsa.ToXmlString(true));
        //     }
        // }

        private static RSAParameters LoadKey(string filePath, bool isPrivateKey = false)
        {
            using var rsa = new RSACryptoServiceProvider();

            rsa.FromXmlString(System.IO.File.ReadAllText(filePath));

            return rsa.ExportParameters(isPrivateKey);
        }

        private static string EncryptString(string message)
        {
            RSAParameters publicKey = LoadKey(Path.Combine("wwwroot", "keys", "public_key.xml"));

            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(message);
            byte[] encryptedData;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);

                encryptedData = rsa.Encrypt(dataToEncrypt, true);
            }

            return Convert.ToBase64String(encryptedData);
        }

        private static string ShortenString(string encodedString)
        {
            byte[] hash = SHA512.HashData(Encoding.UTF8.GetBytes(encodedString));

            return Convert.ToBase64String(hash)[..41];
        }

        private async Task InsertDigitalSignature(string id, string encryptedSignature)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO DigitalSignatures (Id, EncryptedSignature)
                VALUES (@id, @encryptedSignature);
            ", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@encryptedSignature", encryptedSignature);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SignDocument(string documentPath, DigitalSignaturePayload digitalSignaturePayload)
        {
            var absolutePath = Directory.GetCurrentDirectory();

            var pythonFilePath = Path.Combine(absolutePath, "wwwroot/scripts/sign_document.py");

            var inputDocumentPath = Path.Combine(absolutePath, "wwwroot/store", documentPath);

            var inputImagePath = Path.Combine(absolutePath, "wwwroot/signatures", $"{digitalSignaturePayload.SignatureId}.png");

            var xPosition = digitalSignaturePayload.X;
            var yPosition = digitalSignaturePayload.Y;

            var res = JsonConvert.SerializeObject(digitalSignaturePayload);

            var encryptedData = EncryptString(res);

            var id = ShortenString(encryptedData);

            await InsertDigitalSignature(id, encryptedData);

            await Cli.Wrap("\"C:\\Program Files\\Python312\\python.exe\"")
                .WithArguments($"\"{pythonFilePath}\" --id {id} --input-pdf \"{inputDocumentPath}\" --input-img \"{inputImagePath}\" --page-index {digitalSignaturePayload.PageIndex} --x {xPosition} --y {yPosition} -o \"{inputDocumentPath}\"")
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteAsync();
        }

        private async Task<string> GetEncryptedSignature(string digitalSignatureId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT EncryptedSignature
                FROM DigitalSignatures
                WHERE Id = @digitalSignatureId;
            ", conn);

            cmd.Parameters.AddWithValue("@digitalSignatureId", digitalSignatureId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader["EncryptedSignature"].ToString()!;
            }

            return "";
        }

        private static string DecryptString(string encodedEncrypted, RSAParameters privateKey)
        {
            byte[] dataToDecrypt = Convert.FromBase64String(encodedEncrypted);
            byte[] decryptedData;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                decryptedData = rsa.Decrypt(dataToDecrypt, true);
            }

            return Encoding.UTF8.GetString(decryptedData);
        }

        public async Task<bool> VerifySignedDocument(Guid documentId, string digitalSignatureId)
        {
            if ((await _documentRepository.Get(documentId)) == null)
            {
                return false;
            }

            var encryptedSignature = await GetEncryptedSignature(digitalSignatureId);

            if (encryptedSignature == "")
            {
                return false;
            }

            RSAParameters privateKey = LoadKey(Path.Combine("wwwroot", "keys", "private_key.xml"), isPrivateKey: true);

            try
            {
                var decryptedSignature = DecryptString(encryptedSignature, privateKey);

                var digitalSignaturePayload = JsonConvert.DeserializeObject<DigitalSignaturePayload>(decryptedSignature)!;

                if (digitalSignaturePayload.DocumentId != documentId)
                {
                    return false;
                }
            }
            catch (CryptographicException)
            {
                return false;
            }

            return true;
        }
    }
}
