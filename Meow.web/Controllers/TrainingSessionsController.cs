using Meow.Shared.Dtos.TrainingSessions;
using Meow.Shared.Dtos.TrainingSets;
using Meow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class TrainingSessionsController : Controller
{
    private readonly IBackendApi _api;

    public TrainingSessionsController(IBackendApi api)
    {
        _api = api;
    }



    // GET /TrainingSessions?from=...&to=...&tagId=...
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? tagId, int page = 1, int pageSize = 20)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var memberId)) return Forbid();

        // 單選：把字串安全轉 Guid?
        Guid? tagGuid = null;
        if (!string.IsNullOrWhiteSpace(tagId) && Guid.TryParse(tagId, out var g))
            tagGuid = g;

        // 組合 API 需要的 tagIds（字串清單）。若未選擇就傳空清單。
        var selectedTagIds = new List<string>();
        if (tagGuid.HasValue) selectedTagIds.Add(tagGuid.Value.ToString());

        var tags = await _api.GetTagsAsync();

        var vm = new TrainingSessionIndexVm
        {
            From = from,
            To = to,
            TagId = tagGuid, // 供下拉預選用
            AllTags = tags.Select(t => (t.TagId, t.Name, t.Category)).ToList()
        };

        vm.Result = await _api.GetTrainingSessionsAsync(memberId, from, to, page, pageSize, selectedTagIds);

        return View(vm);
    }


    // POST /TrainingSessions/Start?setId=...
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(string? setId, string? note)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var memberId)) return Forbid();

        if (!Guid.TryParse(setId, out var setGuid) || setGuid == Guid.Empty)
            return BadRequest("setId is required");

        var dto = new TrainingSessionCreateDto
        {
            SetID = setGuid,
            Notes = note
        };

        var created = await _api.StartTrainingSessionAsync(memberId, dto);
        TempData["Success"] = "已開始一場新的訓練。";
        return RedirectToAction(nameof(InProgress), new { id = created.SessionID });
    }


    // GET /TrainingSessions/InProgress/{id}
    [HttpGet]
    public async Task<IActionResult> InProgress(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["Error"] = "缺少必要的參數：id。";
            return RedirectToAction(nameof(Index));
        }

        // 取 Session 明細
        var session = await _api.GetTrainingSessionAsync(id);
        if (session == null)
        {
            TempData["Error"] = "找不到這筆訓練紀錄。";
            return RedirectToAction(nameof(Index));
        }

        // 取封面（可選：為了讓版面像 TrainingSets/Details）
        TrainingSetDetailDto? set = null;
        try { set = await _api.GetTrainingSetAsync(session.SetID); } catch { /* ignore */ }

        ViewBag.SetCoverUrl = set?.CoverUrl;
        return View(session); // View 使用 TrainingSessionDetailDto
    }



    // GET /TrainingSessions/Detail/{id}   
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        if (id == Guid.Empty)
        {
            TempData["Error"] = "缺少必要的參數：id。";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var dto = await _api.GetTrainingSessionAsync(id);
            if (dto == null)
            {
                TempData["Error"] = "找不到這筆訓練紀錄。";
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"讀取訓練明細失敗：{ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }



    // POST /TrainingSessions/Complete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, int? calories, int? points, string? note)
    {
        if (id == Guid.Empty) return BadRequest();

        var dto = new TrainingSessionCompleteDto
        {
            EndedAt = DateTime.UtcNow,   // 直接以現在時間結束
            CompletedFlag = true,
            CaloriesBurned = calories,
            PointsAwarded = points,
            Notes = note
        };

        try
        {
            var updated = await _api.CompleteTrainingSessionAsync(id, dto);
            TempData["Success"] = "本次訓練已結束。";
            return RedirectToAction(nameof(Detail), new { id = updated.SessionID });
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"結束訓練失敗：{ex.Message}";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAjax(Guid id, int? calories, int? points, string? note)
    {
        if (id == Guid.Empty) return BadRequest(new { ok = false, message = "id required" });

        var dto = new TrainingSessionCompleteDto
        {
            EndedAt = DateTime.UtcNow,
            CompletedFlag = true,
            CaloriesBurned = calories,
            PointsAwarded = points,
            Notes = note
        };

        try
        {
            var updated = await _api.CompleteTrainingSessionAsync(id, dto);
            // 回傳前端更新所需的最小欄位
            var minutes = updated.EndedAt.HasValue
                ? (int)Math.Max(0, (updated.EndedAt.Value - updated.StartedAt).TotalMinutes)
                : 0;

            return Json(new
            {
                ok = true,
                sessionId = updated.SessionID,
                completedFlag = updated.CompletedFlag,
                endedAt = updated.EndedAt,                     // ISO 格式，前端自行轉字串
                minutes
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, message = ex.Message });
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItem(TrainingSessionItemUpdateDto dto)
    {
        if (dto.SessionItemID == Guid.Empty)
        {
            TempData["Error"] = "缺少 SessionItemID。";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var updated = await _api.UpdateTrainingSessionItemAsync(dto);
            TempData["Success"] = $"步驟 #{updated.OrderNo} 更新成功。";
            return RedirectToAction(nameof(Detail), new { id = dto.SessionID }); // SessionID 需由 View 帶回來
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"更新失敗：{ex.Message}";
            return RedirectToAction(nameof(Detail), new { id = dto.SessionID });
        }
    }

 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItemAjax([FromForm] TrainingSessionItemUpdateDto dto)
    {
        if (dto.SessionItemID == Guid.Empty || dto.SessionID == Guid.Empty)
            return BadRequest(new { ok = false, error = "缺少必要參數。" });

        try
        {
            var updated = await _api.UpdateTrainingSessionItemAsync(dto);
            return Json(new
            {
                ok = true,
                item = new
                {
                    updated.SessionItemID,
                    updated.OrderNo,
                    updated.Status,
                    updated.ActualReps,
                    updated.ActualWeight,
                    updated.ActualDurationSec,
                    updated.ActualRestSec,
                    updated.RoundsDone,
                    updated.Note,
                    updated.VideoTitle
                }
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { ok = false, error = $"更新失敗：{ex.Message}" });
        }
    }

}
