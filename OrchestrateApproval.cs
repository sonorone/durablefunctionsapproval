using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TO_Approval.Models;

namespace To.Orchestration
{
    public static class OrchestrateApproval
    {
        /// <summary>
        /// Durable Orchestration
        /// Orchestrates a Request Approval in Human Interaction pattern using WaitForExternalEvent method.
        /// The Approval Request can be sent via Email using SendGrid. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [FunctionName("OrchestrateApproval")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var isApproved = false;
            ApprovalRequestMetadata approvalRequestMetadata = context.GetInput<ApprovalRequestMetadata>();
            approvalRequestMetadata.InstanceId = context.InstanceId;

            await context.CallActivityAsync("SendApprovalRequestViaEmail", approvalRequestMetadata);

            // Wait for Response as an external event or a time out. 
            // The approver has a limit to approve otherwise the request will be rejected.
            using (var timeoutCts = new CancellationTokenSource())
            {
                int timeout;
                if (!int.TryParse(Environment.GetEnvironmentVariable("Workflow:Timeout"), out timeout))
                    timeout = 1;

                // create timeout task using Durable Function's orechestration context (writes message/event to Storage)
                // "Durable timers cannot last longer than 7 days due to limitations in Azure Storage. 
                // We are working on a feature request to extend timers beyond 7 days."

                DateTime expiration = context.CurrentUtcDateTime.AddMinutes(timeout);
                Task timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

                // This event can come from a click on the Email sent via SendGrid. 
                Task<bool> approvalResponse = context.WaitForExternalEvent<bool>("ReceiveApprovalResponse");

                // and off to the races!
                Task winner = await Task.WhenAny(approvalResponse, timeoutTask);

                ApprovalResponseMessage responseMessage = new ApprovalResponseMessage()
                {
                    Id = context.InstanceId,
                    ReferenceUrl = approvalRequestMetadata.ReferenceUrl
                };

                if (winner == approvalResponse)
                {
                    if (approvalResponse.Result)
                        responseMessage.Result = "approved";
                    else
                        responseMessage.Result = "rejected";
                }
                else
                {
                    responseMessage.Result = "rejected";
                }

                if (!timeoutTask.IsCompleted)
                {
                    // All pending timers must be completed or cancelled before the function exits.
                    timeoutCts.Cancel();
                }

                // Once the approval process has been finished, the result is published to Storage Queue.
                await context.CallActivityAsync<string>("PublishApprovalResult", responseMessage);
                return isApproved;
            }
        }
    }
}
