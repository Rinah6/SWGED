namespace API.Data.Entities
{
    public class DBConnexionDetails
    {
        public required string ServerName { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
    }
}
