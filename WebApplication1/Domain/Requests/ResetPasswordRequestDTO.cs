using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Domain.Requests
{
    public class ResetPasswordRequestDTO
    {
        required public string UserId { set; get; }
        [Required(ErrorMessage = "The New Password Field is required")]
        public string Password { set; get; }
        required public string Email { set; get; }
    }
}
