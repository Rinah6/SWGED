namespace API.Data.Entities
{
    public class ProjectToUpdate
    {
        public required string Name { get; set; }
        public string? ServerName { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? DataBaseName { get; set; }
        public bool HasAccessToInternalUsersHandling { get; set; }
        public bool HasAccessToSuppliersHandling { get; set; }
        public bool HasAccessToProcessingCircuitsHandling { get; set; }
        public bool HasAccessToSignMySelfFeature { get; set; }
        public bool HasAccessToArchiveImmediatelyFeature { get; set; }
        public bool HasAccessToGlobalDynamicFieldsHandling { get; set; }
        public bool HasAccessToPhysicalLocationHandling { get; set; }
        public bool HasAccessToNumericLibrary { get; set; }
        public bool HasAccessToTomProLinking { get; set; }
        public bool HasAccessToUsersConnectionsInformation { get; set; }
        public bool HasAccessToDocumentTypesHandling { get; set; }
        public bool HasAccessToDocumentsAccessesHandling { get; set; }
        public bool HasAccessToRSF { get; set; }

        public string? Sites { get; set; }
    }
}
