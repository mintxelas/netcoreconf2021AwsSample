using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Threading.Tasks;
using Voxel.MiddyNet;
using Voxel.MiddyNet.ProblemDetailsMiddleware;
using Voxel.MiddyNet.SSMMiddleware;
using Voxel.MiddyNet.Tracing.ApiGatewayMiddleware;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MiddyNetSample
{
    public class Nicer : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public Nicer()
        {
            Use(new ApiGatewayTracingMiddleware());
            Use(new ProblemDetailsMiddleware(
                    new ProblemDetailsMiddlewareOptions()
                        .Map<BusinessException>(400)));
            Use(new SSMMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>(new SSMOptions
            {
                ParametersToGet = new System.Collections.Generic.List<SSMParameterToGet>
                {
                    new SSMParameterToGet("message", Environment.GetEnvironmentVariable("messagePath"))
                }
            }));
            Use(new HeaderInjectionMiddleware("X-Message", "hidden message in header"));
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            context.Logger.EnrichWith(new LogProperty("AwsRequestId", context.LambdaContext.AwsRequestId));

            context.Logger.Log(LogLevel.Info, "Hello world");

            return Task.FromResult(new APIGatewayProxyResponse
            {
                Body = Convert.ToString(context.AdditionalContext["message"]),
                StatusCode = 200
            });
        }
    }
}
