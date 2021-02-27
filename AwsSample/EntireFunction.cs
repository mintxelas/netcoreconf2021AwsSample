using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AwsSample
{
    public class EntireFunction
    {
        public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest lambdaEvent, ILambdaContext context)
        {
            try
            {
                var traceParent = string.Empty;
                if (lambdaEvent.Headers.ContainsKey("traceparent"))
                    traceParent = lambdaEvent.Headers["traceparent"];
                
                var message = string.Empty;
                using (var client = new AmazonSimpleSystemsManagementClient())
                {
                    var response = await client.GetParameterAsync(new GetParameterRequest
                    {
                        Name = Environment.GetEnvironmentVariable("messagePath"),
                        WithDecryption = true
                    });
                    message = response.Parameter.Value;
                }

                context.Logger.LogLine($"Received {lambdaEvent.HttpMethod} request {context.AwsRequestId} with traceparent {traceParent}.");
                
                return new APIGatewayProxyResponse
                {
                    Body = message,
                    StatusCode = 200
                };
            }
            catch(BusinessException bex)
            {
                return new APIGatewayProxyResponse
                {
                    Body = bex.Message,
                    StatusCode = 400
                };
            }
            catch (Exception ex)
            {
                var message = $"{ex.GetType().Name}: {ex.Message}";
                context.Logger.LogLine(message);
                return new APIGatewayProxyResponse
                {
                    Body = message,
                    StatusCode = 500
                };
            }
        }
    }
}
