using Microsoft.Graph;

namespace WebApplication1.Application.Interface
{
    public interface IGraphService
    {
        GraphServiceClient CreateGraphClient();
    }
}
