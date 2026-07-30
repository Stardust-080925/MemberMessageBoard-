using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MemberMessageBoard.Data;
using MemberMessageBoard.Models;

var builder = WebApplication.CreateBuilder(args);
//1.註冊DbContext(這是新增的)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//2.註冊Identity(這是新增的) 
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
//3.保留原有的MVC 
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 讓 [Authorize] 屬性生效
app.UseAuthorization();  // 控管使用者是否有權限

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
