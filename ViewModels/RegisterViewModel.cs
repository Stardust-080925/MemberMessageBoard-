using System.ComponentModel.DataAnnotations;
namespace MemberMessageBoard.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage ="電子郵件格式不正確")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "密碼至少需要6個字元")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "請再次輸入密碼")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "兩次輸入的密碼不一致")]
        public string ConFirmPassword { get; set; }

        [Required(ErrorMessage = "請輸入暱稱")]
        public string Nickname{ get; set; }
    }
}
