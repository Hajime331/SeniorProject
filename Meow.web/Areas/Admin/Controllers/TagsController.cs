using Meow.Shared.Dtos.Tags;
using Meow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TagsController : Controller
    {
        private readonly IBackendApi _api;
        public TagsController(IBackendApi api) => _api = api;

        // GET: /Admin/Tags?keyword=
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, string? category)
        {
            var trimmedCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim();

            var tags = await _api.GetTagsAsync(keyword, trimmedCategory);
            var allTags = await _api.GetTagsAsync();
            var categories = allTags
                .Select(t => t.Category?.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.SelectedCategory = trimmedCategory;
            ViewBag.Categories = categories;
            return View(tags);
        }

        // GET: /Admin/Tags/Create
        [HttpGet]
        public IActionResult Create() => View(new TagCreateDto("", "一般"));

        // POST: /Admin/Tags/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                ModelState.AddModelError(nameof(dto.Name), "請輸入名稱");
            if (string.IsNullOrWhiteSpace(dto.Category))
                ModelState.AddModelError(nameof(dto.Category), "請選擇分類");

            if (!ModelState.IsValid) return View(dto);

            await _api.CreateTagAsync(dto);
            TempData["Ok"] = "已建立標籤。";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Tags/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var one = (await _api.GetTagsAsync(null)).FirstOrDefault(t => t.TagId == id);
            if (one == null) return NotFound();
            return View(new TagUpdateDto(one.Name, one.Category));
        }

        // POST: /Admin/Tags/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TagUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                ModelState.AddModelError(nameof(dto.Name), "請輸入名稱");
            if (string.IsNullOrWhiteSpace(dto.Category))
                ModelState.AddModelError(nameof(dto.Category), "請選擇分類");

            if (!ModelState.IsValid) return View(dto);

            await _api.UpdateTagAsync(id, dto);
            TempData["Ok"] = "已更新標籤。";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Tags/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, string? returnUrl = null)
        {
            await _api.DeleteTagAsync(id);
            TempData["Ok"] = "已刪除標籤。";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}
