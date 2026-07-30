using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MemberMessageBoard.Models;
using System.Security.Cryptography.X509Certificates;
namespace MemberMessageBoard.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // 這段會告訴 EF Core：「請在 AspNetUsers 資料表的 Nickname 欄位上加一個唯一索引」，這樣資料庫就會強制暱稱不能重複。
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Nickname)
                .IsUnique(); // ✅ 暱稱唯一
        }

        // 你可以加入其他 DbSet，例如留言板資料表
        public DbSet<Message> Messages { get; set; }
    }
}
