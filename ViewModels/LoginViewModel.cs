using System.ComponentModel.DataAnnotations;
using MemberMessageBoard.Models;
namespace MemberMessageBoard.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage ="電子郵件格式不正確")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "密碼至少需要6個字元")]
        public string Password { get; set; }

        public bool Remember { get; set; }

        // public string Nickname{ get; set;}

        // public ApplicationUser User{ get; set; }
    }
}
