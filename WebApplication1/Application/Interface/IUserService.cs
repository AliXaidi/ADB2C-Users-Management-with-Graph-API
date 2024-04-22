using WebApplication1.Domain.Models;
using WebApplication1.Domain.Requests;
using WebApplication1.Domain.Responses;

namespace WebApplication1.Application.Interface
{
    public interface IUserService
    {
        Task<Microsoft.Graph.Models.User> CreateAsync(ADB2CUserRequestDTO user);
        Task<bool> DeleteAsync(string userId);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO model);
        Task<List<ADB2CUserResponseDTO>> GetUsersAsync(string company);
    }
}
