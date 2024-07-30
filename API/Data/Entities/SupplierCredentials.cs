namespace API.Data.Entities
{
    public class SupplierCredentials
    {
        public required string ProjectId { get; set; }
        public required string Project { get; set; }
        public required bool HasAccessToGlobalDynamicFieldsHandling { get; set; }
    }
}
