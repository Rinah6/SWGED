namespace API.Data.Entities
{
    public class GlobalDynamicField_
    {
        public required Guid Id { get; set; }
        public required string Label { get; set; }
        public required bool IsRequired { get; set; }
        public required Data.DynamicFieldType Type { get; set; }
        public List<DynamicFieldItem>? Values { get; set; }
    }
}
