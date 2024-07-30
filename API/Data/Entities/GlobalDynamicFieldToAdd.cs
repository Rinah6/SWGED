namespace API.Data.Entities
{
    public class GlobalDynamicFieldToAdd
    {
        public required string Label { get; set; }
        public required bool IsForUsersProject { get; set; }
        public required bool IsForSuppliers { get; set; }
        public required bool IsRequired { get; set; }
        public required int Type { get; set; }
        public List<string>? Values { get; set; }
    }
}
