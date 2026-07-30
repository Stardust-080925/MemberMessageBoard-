using System.ComponentModel.DataAnnotations;
using MemberMessageBoard.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemberMessageBoard.ViewModels
{
    public class UploadFileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請輸入標題")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "長度必須在5-50字之間")]
        [Display(Name ="標題")]
        public string Title { get; set; }

        [Required(ErrorMessage = "請輸入內容")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "長度必須在5-1000字之間")]
        [Display(Name ="留言內容")]
        public string Content { get; set; }

        [Display(Name = "選擇圖片")]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".gif" }, ErrorMessage = "僅允許 jpg, jpeg, png, gif 格式")]    // 自訂屬性(Validation/AllowedExtensionsAttribute.cs)
        public IFormFile? FormFile { get; set; }    // 這裡設為nullable，圖片設為選填，只有留言沒有圖片也一樣可以新增留言

        [Display(Name = "圖片")]
        public string? ImagePath { get; set; }

        public bool DeleteImage { get; set; } // ✅ 使用者是否勾選刪除圖片
        
        public string? ExistingImagePath { get; set; } // 顯示原圖用

        
    }
}