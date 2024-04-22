using Microsoft.Graph.Me.Authentication.Methods.Item.ResetPassword;
using Microsoft.Graph.Models;
using WebApplication1.Application.Interface;
using WebApplication1.Domain.Models;
using WebApplication1.Domain.Requests;
using WebApplication1.Domain.Responses;
using WebApplication1.Utility;

namespace WebApplication1.Application.Implementation
{
    public class UserService : IUserService
    {
        private readonly IGraphService _graphService;
        private readonly IConfiguration _configuration;

        public UserService(IGraphService graphService, IConfiguration configuration)
        {
            _graphService = graphService;
            _configuration = configuration;
        }

        public async Task<User> CreateAsync(ADB2CUserRequestDTO user)
        {
            try
            {

                var graphClient = _graphService.CreateGraphClient();
                var requestBody = new Microsoft.Graph.Models.User
                {

                    AccountEnabled = true,
                    DisplayName = user.DisplayName,
                    MobilePhone = user.MobilePhone,
                    CompanyName = user.CompanyName,
                    Identities = new List<ObjectIdentity>
                    {
                        new ObjectIdentity()
                        {
                            SignInType = "emailAddress",
                            Issuer = _configuration["AzureAdB2C:Domain"],
                            IssuerAssignedId =user.IssuerAssignedId
                        }
                    },
                    OnPremisesExtensionAttributes = new OnPremisesExtensionAttributes
                    {
                        ExtensionAttribute1 = AppConstants.Roles.User,
                    },
                    PasswordProfile = new PasswordProfile()
                    {
                        Password = user.Password,
                        ForceChangePasswordNextSignIn = false,
                    },
                    PasswordPolicies= "DisablePasswordExpiration"

                };

                var result = await graphClient.Users.PostAsync(requestBody);
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            try
            {
                var graphClient = _graphService.CreateGraphClient();
                await graphClient.Users[userId].DeleteAsync();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<List<ADB2CUserResponseDTO>> GetUsersAsync(string company)
        {
            var graphClient = _graphService.CreateGraphClient();

            var graphResponse = await graphClient.Users.GetAsync();

            var result = (await graphClient.Users.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "onPremisesExtensionAttributes", "identities", "companyName","mobilePhone" };
            }))?.Value as List<User>;
            List<ADB2CUserResponseDTO> users = new List<ADB2CUserResponseDTO>();
            if (company != null)
            {
                foreach (var item in result)
                {
                    if (item.CompanyName == company)
                    {
                        var user = new ADB2CUserResponseDTO
                        {
                            Id = item.Id,
                            DisplayName = item.DisplayName,
                            Email = item.Identities.FirstOrDefault().IssuerAssignedId,
                            CompanyName = item.CompanyName,
                            onPremisesExtensionAttributes = item.OnPremisesExtensionAttributes,
                            MobilePhone = item.MobilePhone


                        };
                        users.Add(user);
                    }
                }
            }
            return users;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO model)
        {
            try
            {


                var graphClient = _graphService.CreateGraphClient();
             
                var requestBody = new Microsoft.Graph.Models.User
                {
                    PasswordProfile = new PasswordProfile()
                    {
                        Password = model.Password,
                        ForceChangePasswordNextSignIn = false,
                    },
                    PasswordPolicies = "DisablePasswordExpiration"
                };
                await graphClient.Users[model.UserId].PatchAsync(requestBody);
             
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
