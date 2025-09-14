using System;

namespace Meow.Shared.Dtos.Analytics
{
    /// <summary>
    /// 聚合會員訓練統計（總場次、總分鐘、平均每次時長、連續週）
    /// </summary>
    public class MemberStatsDto
    {
        public Guid MemberID { get; set; }
        public int TotalSessions { get; set; }
        public int TotalMinutes { get; set; }
        public double AvgMinutesPerSession { get; set; }
        public int CurrentWeeklyStreak { get; set; }
        public int BestWeeklyStreak { get; set; }
        public DateTime? FirstSessionAt { get; set; }
        public DateTime? LastSessionAt { get; set; }
    }
}
