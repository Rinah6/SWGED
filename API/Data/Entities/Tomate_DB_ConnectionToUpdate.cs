namespace API.Data.Entities
{
    public class Tomate_DB_ConnectionToUpdate
    {
        public required string ServerName { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public required int DatabaseId { get; set; }
    }
}
