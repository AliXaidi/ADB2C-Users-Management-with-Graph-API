using Azure.Identity;
using Microsoft.Graph;
using WebApplication1.Application.Interface;

namespace WebApplication1.Application.Implementation
{
    public class GraphService : IGraphService
    {
        private readonly IConfiguration _configuration;

        public GraphService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GraphServiceClient CreateGraphClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var tenantId = _configuration["AzureAdB2C:TenantId"];
            var clientId = _configuration["AzureAdB2C:ClientId"];
            var clientSecret = _configuration["AzureAdB2C:ClientSecret"];

            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret);

            return new GraphServiceClient(clientSecretCredential, scopes);
        }
    }
}
