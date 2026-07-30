using MemberMessageBoard.Models;

namespace MemberMessageBoard.ViewModels
{
    public class MessageSearchViewModel
    {
        //查詢關鍵字
        public string? Keyword { get; set; }
        //當前頁數
        public int PageNumber { get; set; }
        //每頁幾筆資料
        public int PageSize { get; set; }
        //總頁數(由後端計算)
        public int TotalPages { get; set; }
        //結果資料
        public List<Message> Messages { get; set; } = new List<Message>();

        // 對應 AspNetUsers 的主鍵 Id
        public string UserId { get; set; }
    }
}