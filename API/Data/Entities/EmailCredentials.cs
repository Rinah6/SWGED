namespace API.Data.Entities
{
    public class EmailCredentials
    {
        public required string Smtp { get; set; }
        public required int Port { get; set; }
        public required string Mail { get; set; }
        public required string Password { get; set; }
        public required string Alias { get; set; }
    }
}
