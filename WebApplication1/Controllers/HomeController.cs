using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using WebApplication1.Models;
using Microsoft.Graph.Users.Item.Authentication.Methods.Item.ResetPassword;
using WebApplication1.Utility;
using WebApplication1.Application.Interface;
using WebApplication1.Domain.Responses;


namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        
        public async Task<IActionResult> Index(string? error)
        {
            if(!string.IsNullOrEmpty(error))
            {
                TempData["error"] = "You are not Authorized to Signin here";
            }
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        

    }
   
}
