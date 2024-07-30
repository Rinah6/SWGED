namespace API.Data.Entities
{
    public class ProjectToAdd
    {
        public required string Name { get; set; }
        public required string Storage { get; set; }
        public string? ServerName { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? DataBaseName { get; set; }
        public string? SOA { get; set; }

        public string? Sites { get; set; }
    }
}
