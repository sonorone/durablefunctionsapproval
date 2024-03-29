namespace TO_Approval.Models
{
    public class ApprovalRequestMetadata
    {
        public string ApprovalType { get; set; }
        public string InstanceId { get; set; }
        public string ReferenceUrl { get; set; }
        public string Requestor { get; set; }
        public string Subject { get; set; }
        public string Approver { get; set; }
        public string Result { get; internal set; }
    }
}