namespace TO_Approval.Models
{
    public class ApprovalResponseMessage
    {
        public string Id { get; set; }
        public string ReferenceUrl { get; set; }
        public string Destination { get; set; }
        public string Requestor { get; set; }
        public string Subject { get; set; }
        public string Approver { get; set; }
        public string Result { get; set; }
    }
}