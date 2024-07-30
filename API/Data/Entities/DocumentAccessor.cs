namespace API.Data.Entities
{
    public class DocumentAccessor
    {
        public required Guid Id { get; set; }
        public required string Username { get; set; }
        public required bool CanAccess { get; set; }
    }
}
