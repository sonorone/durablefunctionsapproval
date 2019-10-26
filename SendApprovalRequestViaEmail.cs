using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.SendGrid;
using SendGrid.Helpers.Mail;
using TO_Approval.Models;
using Microsoft.Extensions.Logging;

namespace TO_Approval.Approval
{
    public static class SendApprovalRequestViaEmail
    {
        /// <summary>
        /// Sends an email with an Approval Request via SendGrid
        /// </summary>
        /// <param name="requestMetadata"></param>
        /// <param name="message"></param>
        /// <param name="log"></param>
        [FunctionName("SendApprovalRequestViaEmail")]
        public static void Run([ActivityTrigger] ApprovalRequestMetadata requestMetadata, [SendGrid] out SendGridMessage message, ILogger log)
        {
            message = new SendGridMessage();
            message.AddTo(requestMetadata.Approver);

            message.AddContent("text/html", string.Format(
                Environment.GetEnvironmentVariable("SendGrid:ApprovalEmailTemplate"), 
                requestMetadata.Subject, 
                requestMetadata.Requestor, 
                requestMetadata.ReferenceUrl, 
                requestMetadata.InstanceId, 
                Environment.GetEnvironmentVariable("Function:BasePath")));
            message.SetFrom(requestMetadata.Requestor);
            message.SetSubject(String.Format(Environment.GetEnvironmentVariable("SendGrid:SubjectTemplate"), requestMetadata.Subject, requestMetadata.Requestor));
            log.LogInformation($"Message '{message.Subject}' sent!");
            log.LogInformation(message.Contents[0].Value);
        }
    }
}