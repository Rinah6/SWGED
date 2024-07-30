namespace API.Data.Entities
{
    public class GlobalDynamicFieldDetails
    {
        public required string Label { get; set; }
        public required bool IsForUsersProject { get; set; }
        public required bool IsForSuppliers { get; set; }
        public required bool IsRequired { get; set; }
        public required string Type { get; set; }
        public List<DynamicFieldItem>? Values { get; set; }
    }
}
