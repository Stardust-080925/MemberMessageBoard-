using System.ComponentModel.DataAnnotations;
namespace MemberMessageBoard.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "標題名稱此欄位必填")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "長度必須在5-15字之間")]
        [Display(Name = "標題")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "請輸入內容")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "長度必須在5-100字之間")]
        [Display(Name = "內容")]
        public string Content { get; set; }
        
        [Display(Name = "圖片")]
        public string? ImagePath { get; set; }
        
        // 在 Controller 裡沒手動指定 CreatedAt，也會自動填入目前時間。
        public DateTime CreatedAt { get; set; }

        // 🔗 外鍵欄位：對應 AspNetUsers 的主鍵 Id
        [StringLength(15, MinimumLength = 5, ErrorMessage = "長度必須在5-15字之間")]
        [Display(Name = "使用者編號")]
        public string UserId { get; set; }
        
        // 🔗 導覽屬性：對應 ApplicationUser 類別
        public ApplicationUser User { get; set; }
    }
}
