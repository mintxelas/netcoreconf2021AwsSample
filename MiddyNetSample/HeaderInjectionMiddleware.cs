using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voxel.MiddyNet;

namespace MiddyNetSample
{
    public class HeaderInjectionMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        private readonly string headerName;
        private readonly string headerValue;
        public HeaderInjectionMiddleware(string headerName, string headerValue)
        {
            this.headerName = headerName;
            this.headerValue = headerValue;
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            lambdaResponse.Headers ??= new Dictionary<string, string>();
            lambdaResponse.Headers[headerName] = headerValue;

            context.Logger.Log(LogLevel.Info, $"Header[{headerName}] = {headerValue}");

            return Task.FromResult(lambdaResponse);
        }

        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            return Task.CompletedTask;
        }
    }
}
