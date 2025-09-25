// 用於 TrainingSet 編輯表單
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class TrainingSetEditViewModel
{
    public Guid? SetId { get; set; }
    [Required] public string Name { get; set; } = "";
    public string BodyPart { get; set; } = "";
    public string Equipment { get; set; } = "";
    public string? Difficulty { get; set; }
    public int? EstimatedDurationSec { get; set; }
    public List<Guid> TagIds { get; set; } = new();
    public List<TrainingSetItemEditViewModel> Items { get; set; } = new();
    public IFormFile? CoverFile { get; set; }  // 表單 file input 會綁到這
    public string? CoverUrl { get; set; }      // 現有封面網址（Edit 顯示用
}