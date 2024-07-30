namespace API.Data.Entities {
    public class SubscriptionToUpdate {
        public required string Code { get; set; }
        public required string Location { get; set; }
        public required DateTime BeginDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required int Capacity { get; set; }
        public required int MaxUser { get; set; }
        public required bool HasClientSpace { get; set; }
        public required bool HasFlowManager { get; set; }
        public required bool HasFlow { get; set; }
        public required bool HasDynamicFieldManager { get; set; }
        public required bool HasLibrary { get; set; }
        public required bool HasPhysicalLibrary { get; set; }
    }
}
