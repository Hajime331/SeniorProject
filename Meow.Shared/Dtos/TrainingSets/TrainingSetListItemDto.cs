namespace Meow.Shared.Dtos.TrainingSets;

public record TrainingSetListItemDto(
    Guid SetId,
    string Name,
    string BodyPart,
    string Equipment,
    string? Difficulty,
    int? EstimatedDurationSec,
    List<Guid> TagIds,
    int ItemCount,
    string? CoverUrl // ← 封面網址
);
