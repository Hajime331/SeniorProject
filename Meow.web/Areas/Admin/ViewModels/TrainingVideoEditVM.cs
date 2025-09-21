// 用於 TrainingVideo 編輯表單
using System.ComponentModel.DataAnnotations;

public class TrainingVideoEditViewModel
{
    [Required]
    public Guid VideoId { get; set; }
    [Required] public string Title { get; set; } = "";
    public string BodyPart { get; set; } = "";
    [Required] public string Url { get; set; } = "";
    [Required] public int DurationSec { get; set; }
    public string Status { get; set; } = "Draft";
    public List<Guid> TagIds { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
}