using System.Net.WebSockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Server.Controllers
{
    [Route("/ws")]
    [Authorize]
    public class WebSocketController : ControllerBase
    {
        private readonly string _connectionString;
        private static DateTime? s_endDate;
        private static bool s_isLoggedIn = false;
        private static bool s_isConnected = false;
        private static bool s_isAlreadyLoggedOut = false;

        public WebSocketController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SoftGED_DBContext")!;
        }

        private async Task Login(Guid userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO UsersConnections (UserId, creationDate)
                VALUES (@userId, GETDATE());
            ", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task Logout(Guid userId, DateTime? endDate)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
                UPDATE UsersConnections SET EndDate = @endDate
                WHERE Id = (
                    SELECT TOP 1 Id
                    FROM UsersConnections
                    WHERE UserId = @userId
                    ORDER BY CreationDate DESC
                );
            ", conn);

            cmd.Parameters.AddWithValue("@endDate", endDate == null ? DBNull.Value : endDate);
            cmd.Parameters.AddWithValue("@userId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        [HttpGet("")]
        public async Task ConnectWS()
        {
            var currentUserId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")!.Value);

            try
            {
                using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

                if (!s_isLoggedIn)
                {
                    await Login(currentUserId);

                    s_isLoggedIn = true;
                }

                s_isConnected = true;
                s_isAlreadyLoggedOut = false;

                var buffer = new byte[1024 * 4];

                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType != WebSocketMessageType.Close)
                {
                    s_isLoggedIn = false;
                    s_isConnected = false;

                    s_endDate = DateTime.Now;

                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

                    await Logout(currentUserId, s_endDate);

                    s_isAlreadyLoggedOut = true;
                }

                s_endDate = DateTime.Now;

                s_isConnected = false;

                await Task.Run(() =>
                {
                    Thread.Sleep(60 * 1000);
                });

                if (!s_isConnected && !s_isAlreadyLoggedOut)
                {
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

                    await Logout(currentUserId, s_endDate);

                    s_isLoggedIn = false;
                    s_isConnected = false;
                    s_isAlreadyLoggedOut = true;
                }
            }
            catch (Exception)
            {
                await Logout(currentUserId, DateTime.Now);

                s_isLoggedIn = false;
                s_isConnected = false;
                s_isAlreadyLoggedOut = true;
            }
        }
    }
}
