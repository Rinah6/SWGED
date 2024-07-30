namespace API.Data.Entities
{
    public class DynamicFieldItemToAdd
    {
        public required string Value { get; set; }
        public required Guid DynamicFieldId { get; set; }
    }
}
