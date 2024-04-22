using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using WebApplication1.Domain.Requests;
using WebApplication1.Application.Interface;
using WebApplication1.Domain.Responses;
using WebApplication1.Helpers;
using Microsoft.Graph.Models.ExternalConnectors;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Controller used in web apps to manage accounts.
    /// </summary>


    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IOptionsMonitor<MicrosoftIdentityOptions> _optionsMonitor;
        private readonly IUserService _userService;

        public AccountController(IOptionsMonitor<MicrosoftIdentityOptions> optionsMonitor, IUserService userService)
        {
            _optionsMonitor = optionsMonitor;
            _userService = userService;
        }

        /// <summary>
        /// Constructor of <see cref="AccountController"/> from <see cref="MicrosoftIdentityOptions"/>
        /// This constructor is used by dependency injection.
        /// </summary>
        /// <param name="microsoftIdentityOptionsMonitor">Configuration options.</param>


        /// <summary>
        /// Handles user sign in.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <param name="redirectUri">Redirect URI.</param>
        /// <returns>Challenge generating a redirect to Azure AD to sign in the user.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignIn(
            [FromRoute] string scheme,
            [FromQuery] string redirectUri)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            string redirect;
            if (!string.IsNullOrEmpty(redirectUri) && Url.IsLocalUrl(redirectUri))
            {
                redirect = redirectUri;
            }
            else
            {
                redirect = Url.Content("~/")!;
            }

            return Challenge(
                new AuthenticationProperties { RedirectUri = redirect },
                scheme);
        }

        /// <summary>
        /// Challenges the user.
        /// </summary>
        /// <param name="redirectUri">Redirect URI.</param>
        /// <param name="scope">Scopes to request.</param>
        /// <param name="loginHint">Login hint.</param>
        /// <param name="domainHint">Domain hint.</param>
        /// <param name="claims">Claims.</param>
        /// <param name="policy">AAD B2C policy.</param>
        /// <param name="scheme">Authentication scheme.</param>
        /// <returns>Challenge generating a redirect to Azure AD to sign in the user.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult Challenge(
            string redirectUri,
            string scope,
            string loginHint,
            string domainHint,
            string claims,
            string policy,
            [FromRoute] string scheme)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
            Dictionary<string, string?> items = new Dictionary<string, string?>
            {
                { Constants.Claims, claims },
                { Constants.Policy, policy },
            };
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { Constants.LoginHint, loginHint },
                { Constants.DomainHint, domainHint },
            };

            OAuthChallengeProperties oAuthChallengeProperties = new OAuthChallengeProperties(items, parameters);
            if (scope != null)
            {
                oAuthChallengeProperties.Scope = scope.Split(" ");
            }
            oAuthChallengeProperties.RedirectUri = redirectUri;

            return Challenge(
                oAuthChallengeProperties,
                scheme);
        }

        /// <summary>
        /// Handles the user sign-out.
        /// </summary>
        /// <param name="scheme">Authentication scheme.</param>
        /// <returns>Sign out result.</returns>
        [HttpGet("{scheme?}")]
        public IActionResult SignOut(
            [FromRoute] string scheme)
        {
            if (AppServicesAuthenticationInformation.IsAppServicesAadAuthenticationEnabled)
            {
                if (AppServicesAuthenticationInformation.LogoutUrl != null)
                {
                    return LocalRedirect(AppServicesAuthenticationInformation.LogoutUrl);
                }
                return Ok();
            }
            else
            {
                scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
                var callbackUrl = Url.Action("Index", "Home", null, Request.Scheme); // Generate the callback URL for Home/Index
                if (Request.Query.Count > 0)
                {
                    var error = Request.Query.Where(a => a.Key == "error").First();
                    if (error.Key != null && error.Key == "error" && error.Value.First() == "401")
                    {
                        callbackUrl = Url.Action("Index", "Home", new { error = 401 }, Request.Scheme);
                    }
                }

                return SignOut(
                    new AuthenticationProperties
                    {
                        RedirectUri = callbackUrl,
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme, // Sign out from cookie authentication
                    scheme // Sign out from OpenID Connect
                );

            }
        }
        /// <summary>
        /// Action method for displaying the form to create a new user.
        /// </summary>
        /// <returns>The view to create a new user.</returns>
        [Authorize] // Apply authorization to ensure only authenticated users can access this action
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Uncomment the following block if you want to retrieve the company name from user claims
            //var companyName = User.Claims.FirstOrDefault(a => a.Type == "extension_Company")?.Value;

            // Create a new instance of ADB2CUserRequestDTO to use as the model for the view
            var model = new ADB2CUserRequestDTO()
            {
                // Retrieve the company name from user claims and set it in the model
                CompanyName = User.Claims.FirstOrDefault(a => a.Type == "extension_Company")?.Value,
                IssuerAssignedId = "", // Initialize IssuerAssignedId property with empty string
                DisplayName = "",      // Initialize DisplayName property with empty string
                Password = PasswordHelper.GenerateNewPassword(3, 3, 3), // Generate a new password using PasswordHelper class
                MobilePhone = ""       // Initialize MobilePhone property with empty string
            };

            // Return the view with the populated model
            return View(model);
        }
        /// <summary>
        /// Action method for handling the POST request to create a new user.
        /// </summary>
        /// <param name="model">The ADB2CUserRequestDTO model containing user details.</param>
        /// <returns>
        /// If the user creation is successful, redirects to the index page with a success message.
        /// If model state is not valid or user creation fails, shows an error message and returns to the view.
        /// If an exception occurs, shows the error message and returns to the view.
        /// </returns>
        [Authorize] // Apply authorization to ensure only authenticated users can access this action
        [HttpPost]
        public async Task<IActionResult> Create(ADB2CUserRequestDTO model)
        {
            try
            {
                // Check if the model state is valid
                if (ModelState.IsValid)
                {
                    // Call the UserService to create the user asynchronously
                    var result = await _userService.CreateAsync(model);

                    // If the user creation is successful, redirect to the index page with a success message
                    if (result != null)
                    {
                        TempData["success"] = "Operation Completed Successfully";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // If model state is not valid or user creation fails, show error message and return to the view
                TempData["error"] = "Please Check Values";
                return View(model);
            }
            catch (Exception ex)
            {
                // If an exception occurs, show the error message and return to the view
                TempData["error"] = ex.Message;
                return View(model);
            }
        }
        /// <summary>
        /// Action method for handling the GET request to display the edit view.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="email">The email of the user.</param>
        /// <returns>The edit view with the initialized model.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(string id, string email)
        {
            // Create a new ResetPasswordRequestDTO object and initialize its properties
            var model = new ResetPasswordRequestDTO()
            {
                UserId = id,
                Email = email,
                Password = "",
            };

            // Return the view with the initialized model
            return View(model);
        }
        /// <summary>
        /// Action method for handling the submission of the form to reset a user's password.
        /// </summary>
        /// <param name="model">The model containing the information needed to reset the password.</param>
        /// <returns>If the operation is successful, redirects to the index page with a success message; otherwise, returns to the view with an error message.</returns>
        [Authorize] // Apply authorization to ensure only authenticated users can access this action
        [HttpPost]
        public async Task<IActionResult> Edit(ResetPasswordRequestDTO model)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Call the UserService to reset the user's password asynchronously
                var result = await _userService.ResetPasswordAsync(model);

                // If the password reset is successful, redirect to the index page with a success message
                if (result)
                {
                    TempData["success"] = "Operation Completed, Successfully";
                    return RedirectToAction(nameof(Index));
                }
            }

            // If model state is not valid or password reset fails, show error message and return to the view
            TempData["error"] = "Please Check Values";
            return View(model);
        }
        /// <summary>
        /// Action method for handling the deletion of a user.
        /// </summary>
        /// <param name="id">The ID of the user to be deleted.</param>
        /// <returns>If the deletion is successful, redirects to the index page with a success message; otherwise, redirects to the index page with an error message.</returns>
        [Authorize] // Apply authorization to ensure only authenticated users can access this action
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            // Check if the user ID is provided
            if (!string.IsNullOrEmpty(id))
            {
                // Call the UserService to delete the user asynchronously
                var result = await _userService.DeleteAsync(id);

                // If the deletion is successful, redirect to the index page with a success message
                if (result)
                {
                    TempData["success"] = "Operation Completed, Successfully";
                    return RedirectToAction(nameof(Index));
                }
            }

            // If user ID is not provided or deletion fails, redirect to the index page with an error message
            TempData["error"] = "Please Check Values";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User != null)
            {

                var companyName = User.Claims.FirstOrDefault(a => a.Type == "extension_Company")?.Value;
                if (companyName == null)
                {
                    TempData["error"] = "No Any Company Found";
                }
                var result = await _userService.GetUsersAsync(companyName);
                return View(result);
            }
            else
            {
                TempData["error"] = "No Any Company Found";
                return View(new List<ADB2CUserResponseDTO>());
            }

        }

    }

}
