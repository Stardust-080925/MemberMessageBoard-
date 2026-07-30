using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MemberMessageBoard.Data;
using Microsoft.AspNetCore.Authorization;
using MemberMessageBoard.Models;
using Microsoft.EntityFrameworkCore;
using MemberMessageBoard.ViewModels;

public class MessageController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessageController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _env = env;
        _userManager = userManager;
    }

    // 搜尋留言
    public async Task<IActionResult> Index(string? keyword, int page = 1)
    {
        int pageSize = 5;
        var query = _context.Messages
            .Include(m => m.User) // ✅ 加在這裡
            .AsQueryable();

        var loweredKeyword = string.IsNullOrWhiteSpace(keyword) ? "" : keyword.ToLower();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c =>
            (c.Title ?? "").ToLower().Contains(loweredKeyword) ||
            (c.Content ?? "").ToLower().Contains(loweredKeyword) ||
            (c.User.Nickname ?? "").ToLower().Contains(loweredKeyword));
        }

        int totalCount = await query.CountAsync();

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vm = new MessageSearchViewModel
        {
            Keyword = keyword,
            PageNumber = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Messages = messages
        };

        return View(vm);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UploadFileViewModel uploadfilevm)
    {
        string? imagePath = null;

        // ✅ 圖片驗證與儲存（如果有上傳)，只有在使用者有上傳圖片時，才會進行格式與大小檢查，否則就跳過圖片處理
        if (uploadfilevm.FormFile != null && uploadfilevm.FormFile.Length > 0)
        {
            var ext = Path.GetExtension(uploadfilevm.FormFile.FileName)?.ToLower().Trim();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (!allowedExts.Contains(ext))
            {
                ModelState.AddModelError("FormFile", "不支援的檔案格式，僅允許 jpg, jpeg, png, gif");
                return View(uploadfilevm);
            }

            if (uploadfilevm.FormFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("FormFile", "檔案太大，請勿超過 2MB");
                return View(uploadfilevm);
            }

            string filename = $"{Guid.NewGuid()}{ext}";
            string uploadPath = Path.Combine(_env.WebRootPath, "uploads", filename);

            try
            {
                using var stream = new FileStream(uploadPath, FileMode.Create);
                await uploadfilevm.FormFile.CopyToAsync(stream);
                imagePath = "/uploads/" + filename;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"圖片儲存失敗：{ex.Message}");
                return View(uploadfilevm);
            }
        }

        // ✅ 前端欄位驗證
        if (!ModelState.IsValid)
        {
            return View(uploadfilevm);
        }

        // ✅ 建立留言資料
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge(); // 或 RedirectToAction("Login")
        }
        
        // 時區更改成台灣時區
        var taipeiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
        var taipeiNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taipeiTimeZone);

        var message = new Message
        {
            Title = uploadfilevm.Title,
            Content = uploadfilevm.Content,
            CreatedAt = taipeiNow,
            UserId = user.Id,
            ImagePath = imagePath // ✅ 有上傳才會有值，否則為 null
        };

        try
        {
            _context.Messages.Add(message);
            var result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                ModelState.AddModelError("", "儲存失敗：資料庫未新增任何紀錄");
                return View(uploadfilevm);
            }

            TempData["Title"] = "新增成功";
            TempData["SuccessMessage"] = "留言新增成功!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"留言儲存失敗：{ex.Message}");
            return View(uploadfilevm);
        }
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var message = _context.Messages.FirstOrDefault(m => m.Id == id);
        if (message == null) return NotFound();

        var vm = new UploadFileViewModel
        {
            Id = message.Id,
            Title = message.Title,
            Content = message.Content,
            ImagePath = message.ImagePath,
            ExistingImagePath = message.ImagePath
        };
        
        return View(vm);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(UploadFileViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        } 

        var message = _context.Messages.FirstOrDefault(m => m.Id == vm.Id);
        if (message == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);
        if (message.UserId != currentUserId)
        {
            return Forbid();    // ✅ 身份檢查應放在這裡
        }
        
        message.Title = vm.Title;
        message.Content = vm.Content;

        // 刪除原圖並清空 ImagePath
        if (vm.DeleteImage && !string.IsNullOrEmpty(message.ImagePath))
        {
            var oldPath = Path.Combine(_env.WebRootPath, message.ImagePath.TrimStart('/'));
            try
            {
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"刪除圖片失敗：{ex.Message}");
                return View(vm);
            }

            message.ImagePath = null;
            vm.ExistingImagePath = null; // ✅ Razor 頁面不再顯示圖片
        }

        // 圖片處理完後，如果沒有新圖，就保留舊圖
        if (vm.FormFile != null && vm.FormFile.Length > 0)
        {
            // 🔥 這裡是更新圖片的邏輯
            // 1. 刪除舊圖片（如果有）
            // 2. 儲存新圖片
            // 3. 更新 message.ImagePath 為新路徑
            // 使用者只要上傳新圖片，就會自動「覆蓋」原圖，不需要勾選刪除。
            // 儲存新圖 → message.ImagePath 被更新
            var ext = Path.GetExtension(vm.FormFile.FileName).ToLower();
            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (!allowedExts.Contains(ext) || vm.FormFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "不支援的檔案格式或檔案太大");
                return View(vm);
            }

            // 刪除舊圖片
            if (!string.IsNullOrEmpty(message.ImagePath))
            {
                var oldPath = Path.Combine(_env.WebRootPath, message.ImagePath.TrimStart('/'));

                try
                {
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"刪除舊圖片失敗：{ex.Message}");
                    return View(vm);
                }
            }

            // 儲存照片
            var filename = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(_env.WebRootPath, "uploads", filename);
            using var stream = new FileStream(path, FileMode.Create);
            await vm.FormFile.CopyToAsync(stream);
            message.ImagePath = "/uploads/" + filename;
        }
        else
        {
            message.ImagePath = vm.ImagePath ?? vm.ExistingImagePath;   // ✅ 沒有新圖 → 保留舊圖
        }

        await _context.SaveChangesAsync();
        TempData["Title"] = "留言編輯";
        TempData["SuccessMessage"] = "留言已更新";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var message = _context.Messages.FirstOrDefault(m => m.Id == id);
        if (message == null)
        {
            return NotFound();
        }
        return View(message);
    }

    [Authorize]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var message = _context.Messages.FirstOrDefault(m => m.Id == id);
        if (message == null)
        {
            return NotFound();
        } 

        // 限制只能編輯/刪除自己的留言
        var currentUserId = _userManager.GetUserId(User);
        if (message.UserId != currentUserId)
        {
            return Forbid();    // 或 RedirectToAction("Index") 並顯示錯誤訊息
        } 

        // 刪除圖片檔案
        if (!string.IsNullOrEmpty(message.ImagePath))
        {
            var filePath = Path.Combine(_env.WebRootPath, message.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            } 
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        TempData["Title"] = "留言刪除";
        TempData["SuccessMessage"] = "留言已刪除";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Detail(int id)
    {
        var message = _context.Messages.Find(id);
        if (message == null)
        {
            return NotFound();
        }
        
        return View(message);
    }
}
