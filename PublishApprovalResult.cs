using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Queue;
using TO_Approval.Models;

namespace TO_Approval.Approval
{
    [StorageAccount("AzureWebJobsStorage")]
    public static class PublishApprovalResult
    {
        [FunctionName("PublishApprovalResult")]
        public static async Task Run(
            [ActivityTrigger] ApprovalRequestMetadata request,
            [OrchestrationClient] DurableOrchestrationClient orchestrationClient,
            [Queue("published-results", Connection = "AzureWebJobsStorage")] CloudQueue queueMessage)
        {
            // add item to Storage queue for further processing
            await queueMessage.AddMessageAsync(
                new CloudQueueMessage(string.Format(
                    "{0} - {1} - {2} - {3} - {4}", 
                    request.InstanceId, 
                    request.Requestor, 
                    request.Subject, 
                    request.Approver, 
                    request.Result)));
            await orchestrationClient.RaiseEventAsync(request.InstanceId, "PublishApprovalResult", "success");
        }
    }
}