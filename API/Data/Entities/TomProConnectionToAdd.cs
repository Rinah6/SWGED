namespace API.Data.Entities
{
    public class TomProConnectionToAdd
    {
        public required string ServerName { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public required List<TomProDatabaseToAdd> Databases { get; set; }
    }
}
