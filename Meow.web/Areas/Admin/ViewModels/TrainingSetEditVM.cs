// 用於 TrainingSet 編輯表單
using System.ComponentModel.DataAnnotations;

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
}