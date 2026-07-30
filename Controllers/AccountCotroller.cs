using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MemberMessageBoard.Models;
using MemberMessageBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    //註冊GET
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }


    //註冊POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl)
    {

        // 帳號重複判斷
        var emailExists = await _userManager.Users.AnyAsync(u => u.Email == model.Email);
        if (emailExists)
        {
            ModelState.AddModelError("Email", "此帳號已被註冊");
        }

        // 密碼強度判斷
        if (!Regex.IsMatch(model.Password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,}$"))
        {
            ModelState.AddModelError("Password", "密碼必須包含大小寫字母與數字");
        }

        if (ModelState.IsValid)
        {
            return View(model); 
        }

        // 暱稱重複判斷
        // 希望暱稱是唯一，所以需要自己查詢
        var nicknameExists = await _userManager.Users.AnyAsync(u => u.Nickname == model.Nickname);
        if (nicknameExists)
        {
            ModelState.AddModelError("Nickname", "暱稱已被使用，請選擇其他暱稱");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Nickname = model.Nickname
        };

        // 會自動檢查Email 是否重複（如果你有設定 UserName 為 Email）
        // 會自動檢查密碼是否符合 Identity 的密碼規則（長度、特殊字元等）
        // 但它不會主動檢查暱稱是否重複，除非你自己加上邏輯。
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // ✅ 註冊成功後直接登入
            // 是在執行 「登入使用者」的動作，也就是讓剛註冊成功的使用者立即登入，不需要再手動輸入帳號密碼。
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Title"] = "註冊成功";
            TempData["SuccessMessage"] = $"歡迎 {user.Nickname} 加入留言板！";

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl); // ✅ 回到原頁面
            }

            return RedirectToAction("Index", "Message");     // ✅ 預設跳轉
        }

        return View(model);
    }

     //登入
    [HttpGet]
    [Route("Account/Login")]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;  // 這樣使用者如果是從受保護頁面被導向登入，登入後就會自動跳回原頁面；否則就跳到 Message/Index。
        return View();
    }

    //登入POST
    [HttpPost]
    [Route("Account/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        // 根據使用者輸入的 Email，從資料庫中查詢對應的使用者資料（ApplicationUser）
        // 這取決於你登入時是用 Email 還是 UserName。
        // 回傳使用者物件或是null
        var user = await _userManager.FindByEmailAsync(model.Email); // 或 GetUserAsync(User)
        if(user == null)
        {
            ModelState.AddModelError("Email", "此帳號不存在");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.Remember, true);

        if (result.Succeeded)
        {
            // ✅ 登入成功後更新 ClaimsPrincipal            
            // 是在執行 「登入使用者」的動作，也就是讓剛註冊成功的使用者立即登入，不需要再手動輸入帳號密碼。
            await _signInManager.SignInAsync(user, isPersistent: model.Remember);
            TempData["Title"] = "登入成功";
            TempData["SuccessMessage"] = $"登入成功，歡迎 {user.Nickname} 回來！";

            if (Url.IsLocalUrl(returnUrl))
            {
                // 是用來支援「使用者原本想去的頁面」，例如：
                // 使用者點了「留言區」但未登入 → 被導向到登入頁 → 登入成功後回到留言區
                // 如果你不需要這種「回到原頁」的功能，就可以刪掉這段
                return Redirect(returnUrl); // ✅ 回到原本頁面
            }
            return RedirectToAction("Index", "Message");    // ✅ 預設跳轉到留言列表
        }
        else
        {
            ModelState.AddModelError("", "登入失敗，帳號或密碼錯誤");
            // TempData["LoginError"] = "登入失敗，帳號或密碼錯誤";
            return View(model);
        }
        
    }

    //登出
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // ✅ 清除 Cookie，重設身份
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    //權限不足
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

}