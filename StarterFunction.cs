using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace TO.Starter
{
    public static class StarterFunction
    {
        [FunctionName("StarterFunction")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient orchestrationClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Function input comes from the request content.
            string instanceId = await orchestrationClient.StartNewAsync("OrchestrateApproval", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
