using System.ComponentModel.DataAnnotations;

public class TrainingSetItemEditViewModel
{
    public Guid? SetItemId { get; set; }
    [Required] public Guid VideoId { get; set; }
    public int OrderNo { get; set; }
    public int? TargetReps { get; set; }
    public int? RestSec { get; set; }
    public int? Rounds { get; set; }
    public string? VideoTitle { get; set; }
}
