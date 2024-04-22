namespace WebApplication1.Domain.Models
{
    public class ADB2CUser
    {
        required public string CompanyName { set; get; }
        required public string IssuerAssignedId { set; get; }

        required public string DisplayName { set; get; }
        required public string Password { set; get; }
        public string? MobilePhone { set; get; }

    }
}
