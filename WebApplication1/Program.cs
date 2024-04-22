using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using WebApplication1.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApplication1.Application.Implementation;
using WebApplication1.Application.Interface;
using WebApplication1.Middlewares;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, Microsoft.Identity.Web.Constants.AzureAdB2C);

        builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.Scope.Add(options.ClientId);
            options.SaveTokens = true;
            
        });
        builder.Services.AddSingleton<IGraphService, GraphService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        builder.Services.AddRazorPages();


        builder.Services.AddOptions();
        builder.Services.Configure<OpenIdConnectOptions>(builder.Configuration.GetSection("AzureAdB2C"));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAfterAuthenticationMiddleware();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}