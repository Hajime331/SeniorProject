public class TrainingSetItemDetailVm
{
    public Guid SetItemId { get; set; }
    public Guid VideoId { get; set; }
    public string VideoTitle { get; set; } = "(未知影片)";
    public int? OrderNo { get; set; }
    public int? TargetReps { get; set; }
    public int? RestSec { get; set; }
    public int? Rounds { get; set; }
}
