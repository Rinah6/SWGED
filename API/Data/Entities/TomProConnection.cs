namespace API.Data.Entities
{
    public class TomProConnection
    {
        public required string ServerName { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public required string DatabaseName { get; set; }
    }
}
