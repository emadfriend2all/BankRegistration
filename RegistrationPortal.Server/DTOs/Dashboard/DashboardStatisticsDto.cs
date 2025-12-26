namespace RegistrationPortal.Server.DTOs.Dashboard
{
    public class DashboardStatisticsDto
    {
        public int NewCustomersCount { get; set; }
        public double NewCustomersPercentage { get; set; }
        public int UpdateCustomersCount { get; set; }
        public double UpdateCustomersPercentage { get; set; }
        public int TotalCustomersCount { get; set; }
        public int PendingReviewsCount { get; set; }
        public double PendingReviewsPercentage { get; set; }
        public int ApprovedReviewsCount { get; set; }
        public double ApprovedReviewsPercentage { get; set; }
        public int RejectedReviewsCount { get; set; }
        public double RejectedReviewsPercentage { get; set; }
    }

    public class DailyRequestDataDto
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardGraphDataDto
    {
        public List<DailyRequestDataDto> DailyRequests { get; set; } = new();
    }
}
