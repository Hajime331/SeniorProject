namespace Meow.Shared.Dtos.TrainingSets
{
    public record TrainingSetCreateDto(
    string Name,
    string BodyPart,                  // 必填；後端再做白名單驗證
    string Equipment,                 // 必填；後端再做白名單驗證
    string? Difficulty,
    int? EstimatedDurationSec,
    List<Guid> TagIds,                // 非 null，空集合可
    List<TrainingSetItemCreateDto> Items // 非 null，且至少 1 筆由後端驗證
    );
}