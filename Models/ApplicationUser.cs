using Microsoft.AspNetCore.Identity;
namespace MemberMessageBoard.Models
{
    public class ApplicationUser : IdentityUser
    {
        // 可以加上自訂欄位，例如暱稱等
        public string Nickname { get; set; }
    }
}
