using Microsoft.Azure.WebJobs;
using TO_Approval.Models;

namespace TO_Approval.Approval
{
    public static class PublishApprovalResult
    {
        [FunctionName("PublishApprovalResult")]
        public static void Run([ActivityTrigger] ApprovalRequestMetadata requestMetadata)
        {
            
        }
    }
}