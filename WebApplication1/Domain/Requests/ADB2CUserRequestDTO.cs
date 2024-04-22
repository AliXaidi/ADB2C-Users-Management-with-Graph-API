using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Domain.Requests
{
    public class ADB2CUserRequestDTO
    {
        [Required(ErrorMessage ="The Company Name Field is Required")]
        required public string CompanyName { set; get; }
        [Required(ErrorMessage = "The Email Field is Required")]
        required public string IssuerAssignedId { set; get; }
        [Required(ErrorMessage = "The  Name Field is Required")]
        required public string DisplayName { set; get; }
        [Required(ErrorMessage = "The Password Field is Required")]
        required public string Password { set; get; }
        public string? MobilePhone { set; get; }

    }
}
