namespace API.Data.Entities
{
    public class UserCredentials
    {
        public required string Username { get; set; }
        public required UserRole Role { get; set; }
        public required bool IsADocumentsReceiver { get; set; }
        public required bool HasAccessToInternalUsersHandling { get; set; }
        public required bool HasAccessToSuppliersHandling { get; set; }
        public required bool HasAccessToProcessingCircuitsHandling { get; set; }
        public required bool HasAccessToSignMySelfFeature { get; set; }
        public required bool HasAccessToArchiveImmediatelyFeature { get; set; }
        public required bool HasAccessToGlobalDynamicFieldsHandling { get; set; }
        public required bool HasAccessToPhysicalLocationHandling { get; set; }
        public required bool HasAccessToNumericLibrary { get; set; }
        public required bool HasAccessToTomProLinking { get; set; }
        public required bool HasAccessToUsersConnectionsInformation { get; set; }
        public required bool HasAccessToDocumentTypesHandling { get; set; }
        public required bool HasAccessToDocumentsAccessesHandling { get; set; }
        public required bool HasAccessToRSF { get; set; }
    }
}
